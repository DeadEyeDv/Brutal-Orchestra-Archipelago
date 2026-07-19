using System;
using System.Collections.Generic;
using System.Text;
using System.Net.WebSockets;
using System.Threading;
using UnityEngine;

namespace BrutalOrchestraAr
{
    public class APClient
    {
        private ClientWebSocket socket;
        private string host;          // host:port, без схемы
        private bool sentConnect = false;
        private string playerName;
        private string password;
        public static string CurrentSeed = "";
        public bool IsOpen => socket != null && socket.State == WebSocketState.Open;

        public APClient(string hostPort, string name, string pass = null)
        {
            host = hostPort;
            playerName = name;
            password = pass;
        }

        public async void Connect()
        {
            sentConnect = false;
            // wss:// для archipelago.gg, ws:// для локального сервера — пробуем оба
            foreach (var scheme in new[] { "wss://", "ws://" })
            {
                socket = new ClientWebSocket();
                try
                {
                    await socket.ConnectAsync(new Uri(scheme + host), CancellationToken.None);
                    Debug.Log($"AP: Socket connected via {scheme}");
                    ReceiveLoop();
                    return;
                }
                catch (Exception e)
                {
                    Debug.Log($"AP: {scheme} failed — {e.Message}");
                }
            }
            BrutalAPMod.OnConnectFailed("Cannot reach " + host);
        }
        
        public async void SendConnectUpdate(bool deathLinkOn)
        {
            string tags = deathLinkOn ? "[\"AP\",\"DeathLink\"]" : "[\"AP\"]";
            string json = "[{\"cmd\":\"ConnectUpdate\",\"tags\":" + tags + "}]";
            var bytes = Encoding.UTF8.GetBytes(json);
            await socket.SendAsync(new ArraySegment<byte>(bytes), WebSocketMessageType.Text, true, CancellationToken.None);
            Debug.Log("AP: DeathLink " + (deathLinkOn ? "enabled" : "disabled"));
        }

        private async void ReceiveLoop()
        {
            var buffer = new byte[4096];
            var messageBuffer = new StringBuilder();
            bool sentConnect = false;

            try
            {
                while (socket.State == WebSocketState.Open)
                {
                    var result = await socket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
                    if (result.MessageType == WebSocketMessageType.Close) break;

                    messageBuffer.Append(Encoding.UTF8.GetString(buffer, 0, result.Count));
                    string msg = messageBuffer.ToString();

                    while (true)
                    {
                        int start = msg.IndexOf('[');
                        if (start == -1) break;
                        int depth = 0, end = -1;
                        for (int i = start; i < msg.Length; i++)
                        {
                            if (msg[i] == '[') depth++;
                            else if (msg[i] == ']') depth--;
                            if (depth == 0) { end = i; break; }
                        }
                        if (end == -1) break;

                        string json = msg.Substring(start, end - start + 1);
                        messageBuffer.Remove(0, end + 1);
                        msg = messageBuffer.ToString();

                        foreach (string obj in SplitTopLevelObjects(json))
                            DispatchCommand(obj);
                        
                        
                    }
                }
                BrutalAPMod.AddChatLine("*** Disconnected from server ***");
            }
            catch (Exception e)
            {
                Debug.LogError("AP: ReceiveLoop error — " + e);
                BrutalAPMod.AddChatLine("*** Connection lost: " + e.Message + " ***");
                BrutalAPMod.OnConnectFailed(e.Message);
            }
        }

        private void DispatchCommand(string json)
        {
            if (!sentConnect && json.Contains("\"cmd\":\"RoomInfo\""))
            {
                int seedIdx = json.IndexOf("\"seed_name\":\"");
                if (seedIdx != -1)
                {
                    int seedStart = seedIdx + 13;
                    int seedEnd = json.IndexOf("\"", seedStart);
                    if (seedEnd != -1) CurrentSeed = json.Substring(seedStart, seedEnd - seedStart);
                }
                Debug.Log("AP: Got RoomInfo, sending Connect");
                sentConnect = true;
                SendConnectPacket();
            }
            else if (json.Contains("\"cmd\":\"Connected\""))
            {
                Debug.Log("AP: Successfully connected to room!");
                ParsePlayers(json);
                SendGetDataPackage();
                
                int slotDataIndex = json.IndexOf("\"slot_data\":{");
                if (slotDataIndex != -1)
                {
                    int bracketOpen = json.IndexOf("{", slotDataIndex);
                    int sdDepth = 0, bracketClose = -1;
                    for (int i = bracketOpen; i < json.Length; i++)
                    {
                        if (json[i] == '{') sdDepth++;
                        else if (json[i] == '}') { sdDepth--; if (sdDepth == 0) { bracketClose = i; break; } }
                    }
                    if (bracketClose != -1)
                        BrutalAPMod.InitSlotData(json.Substring(bracketOpen, bracketClose - bracketOpen + 1));
                }
                BrutalAPMod.OnConnectSucceeded();
            }
            else if (json.Contains("\"cmd\":\"ConnectionRefused\""))
            {
                string reason = ExtractErrors(json);
                Debug.LogWarning("AP: ConnectionRefused — " + reason);
                BrutalAPMod.OnConnectFailed(reason);
                sentConnect = false;
            }
            else if (json.Contains("\"cmd\":\"ReceivedItems\""))
            {
                ParseReceivedItems(json);
            }
            else if (json.Contains("\"cmd\":\"PrintJSON\""))
            {
                ParsePrintJson(json);
            }
            else if (json.Contains("\"cmd\":\"DataPackage\""))
            {
                ParseDataPackage(json);
            }
            else if (json.Contains("\"cmd\":\"Bounced\""))
            {
                Debug.Log("AP: Bounced received: " + json);
                if (json.Contains("\"DeathLink\"") && BrutalAPMod.DeathLinkEnabled)
                {
                    string src = ExtractString(json, "\"source\":\"") ?? "someone";
                    string cause = ExtractString(json, "\"cause\":\"");
                    string me = BrutalAPMod.playerIdToName.TryGetValue(BrutalAPMod.mySlot, out var n) ? n : "";
                    Debug.Log($"AP: DeathLink parse — src='{src}', me='{me}'");
                    if (src != me)
                    {
                        BrutalAPMod.AddChatLine("*** DeathLink: " + (string.IsNullOrEmpty(cause) ? src + " died" : cause) + " ***");
                        BrutalAPMod.QueueIncomingDeath();
                    }
                    else Debug.Log("AP: DeathLink — own death echo, ignored");
                }
            }
            else
            {
                Debug.Log("AP RECV: " + json);
            }
        }

        private static List<string> SplitTopLevelObjects(string json)
        {
            var result = new List<string>();
            int depth = 0, start = -1;
            bool inString = false;
            for (int i = 0; i < json.Length; i++)
            {
                char c = json[i];
                if (inString)
                {
                    if (c == '\\') i++;
                    else if (c == '"') inString = false;
                    continue;
                }
                if (c == '"') { inString = true; continue; }
                if (c == '{') { if (depth == 0) start = i; depth++; }
                else if (c == '}')
                {
                    depth--;
                    if (depth == 0 && start != -1)
                    {
                        result.Add(json.Substring(start, i - start + 1));
                        start = -1;
                    }
                }
            }
            return result;
        }
        
        // "errors":["InvalidSlot"] -> "InvalidSlot"
        private static string ExtractErrors(string json)
        {
            int idx = json.IndexOf("\"errors\":[");
            if (idx == -1) return "Connection refused";
            int start = json.IndexOf('[', idx), end = json.IndexOf(']', start);
            if (start == -1 || end == -1) return "Connection refused";
            return json.Substring(start + 1, end - start - 1).Replace("\"", "").Trim();
        }

        // "slot":1 и "players":[{"team":0,"slot":1,"alias":"Name","name":"Name"}, ...]
        private void ParsePlayers(string json)
        {
            int slotIdx = json.IndexOf("\"slot\":");
            if (slotIdx != -1)
            {
                int ns = slotIdx + 7;
                while (ns < json.Length && !char.IsDigit(json[ns])) ns++;
                int ne = ns;
                while (ne < json.Length && char.IsDigit(json[ne])) ne++;
                if (ne > ns && int.TryParse(json.Substring(ns, ne - ns), out int s))
                    BrutalAPMod.mySlot = s;
            }

            int pIdx = json.IndexOf("\"players\":[");
            if (pIdx == -1) return;
            int start = json.IndexOf('[', pIdx);
            int depth = 0, end = -1;
            for (int i = start; i < json.Length; i++)
            {
                if (json[i] == '[') depth++;
                else if (json[i] == ']') { depth--; if (depth == 0) { end = i; break; } }
            }
            if (end == -1) return;

            string arr = json.Substring(start, end - start + 1);
            BrutalAPMod.playerIdToName.Clear();

            int pos = 0;
            BrutalAPMod.playerIdToName.Clear();
            BrutalAPMod.playerIdToGame.Clear();
            while ((pos = arr.IndexOf("\"slot\":", pos)) != -1)
            {
                pos += 7;
                int ns = pos;
                while (ns < arr.Length && !char.IsDigit(arr[ns])) ns++;
                int ne = ns;
                while (ne < arr.Length && char.IsDigit(arr[ne])) ne++;
                if (ne == ns) break;
                int.TryParse(arr.Substring(ns, ne - ns), out int slot);

                // alias идёт после slot в том же объекте; если его нет — берём name
                int aliasIdx = arr.IndexOf("\"alias\":\"", ne);
                int nameIdx = arr.IndexOf("\"name\":\"", ne);
                int nextSlot = arr.IndexOf("\"slot\":", ne);

                string playerName = null;
                if (aliasIdx != -1 && (nextSlot == -1 || aliasIdx < nextSlot))
                {
                    int vs = aliasIdx + 9, ve = arr.IndexOf('"', vs);
                    if (ve != -1) playerName = arr.Substring(vs, ve - vs);
                }
                if (string.IsNullOrEmpty(playerName) && nameIdx != -1 && (nextSlot == -1 || nameIdx < nextSlot))
                {
                    int vs = nameIdx + 8, ve = arr.IndexOf('"', vs);
                    if (ve != -1) playerName = arr.Substring(vs, ve - vs);
                }

                if (!string.IsNullOrEmpty(playerName))
                    BrutalAPMod.playerIdToName[slot] = playerName;
                pos = ne;
            }
            Debug.Log($"AP: Parsed {BrutalAPMod.playerIdToName.Count} players, my slot = {BrutalAPMod.mySlot}");
            
            ParseSlotInfo(json);
        }

        // "slot_info":{"1":{"name":"X","game":"Brutal Orchestra","type":1,...}, ...}
        private void ParseSlotInfo(string json)
        {
            int idx = json.IndexOf("\"slot_info\":{");
            if (idx == -1) return;
            int start = json.IndexOf('{', idx);
            int end = MatchBracket(json, start, '{', '}');
            if (end == -1) return;

            string body = json.Substring(start + 1, end - start - 1);
            int pos = 0;
            while (pos < body.Length)
            {
                int keyStart = body.IndexOf('"', pos);
                if (keyStart == -1) break;
                int keyEnd = body.IndexOf('"', keyStart + 1);
                if (keyEnd == -1) break;
                if (!int.TryParse(body.Substring(keyStart + 1, keyEnd - keyStart - 1), out int slot)) { pos = keyEnd + 1; continue; }

                int objStart = body.IndexOf('{', keyEnd);
                if (objStart == -1) break;
                int objEnd = MatchBracket(body, objStart, '{', '}');
                if (objEnd == -1) break;

                string obj = body.Substring(objStart, objEnd - objStart + 1);
                string game = ExtractString(obj, "\"game\":\"");
                if (!string.IsNullOrEmpty(game)) BrutalAPMod.playerIdToGame[slot] = game;

                pos = objEnd + 1;
            }
            Debug.Log($"AP: Parsed slot_info for {BrutalAPMod.playerIdToGame.Count} slots");
        }
        
        private void ParsePrintJson(string json)
        {
            int dataIdx = json.IndexOf("\"data\":[");
            if (dataIdx == -1) return;
            int start = json.IndexOf('[', dataIdx);
            int depth = 0, end = -1;
            for (int i = start; i < json.Length; i++)
            {
                if (json[i] == '[') depth++;
                else if (json[i] == ']') { depth--; if (depth == 0) { end = i; break; } }
            }
            if (end == -1) return;

            string data = json.Substring(start, end - start + 1);
            var sb = new StringBuilder();

            // Идём по объектам {...} внутри data[]
            int pos = 0;
            while (true)
            {
                int objStart = data.IndexOf('{', pos);
                if (objStart == -1) break;
                int d = 0, objEnd = -1;
                for (int i = objStart; i < data.Length; i++)
                {
                    if (data[i] == '{') d++;
                    else if (data[i] == '}') { d--; if (d == 0) { objEnd = i; break; } }
                }
                if (objEnd == -1) break;

                string obj = data.Substring(objStart, objEnd - objStart + 1);
                pos = objEnd + 1;

                string text = ExtractString(obj, "\"text\":\"");
                if (text == null) continue;
                string type = ExtractString(obj, "\"type\":\"");

                if (type == "player_id" && int.TryParse(text, out int pid))
                {
                    string pname = BrutalAPMod.playerIdToName.TryGetValue(pid, out var n) ? n : text;
                    sb.Append(pid == BrutalAPMod.mySlot ? "<color=#C77DFF>" + pname + "</color>" : pname);
                }
                else if (type == "item_id" && long.TryParse(text, out long iid))
                {
                    int owner = ExtractInt(obj, "\"player\":", BrutalAPMod.mySlot);
                    sb.Append(BrutalAPMod.LookupItemName(iid, owner));
                }
                else if (type == "location_id" && long.TryParse(text, out long lid))
                {
                    int owner = ExtractInt(obj, "\"player\":", BrutalAPMod.mySlot);
                    sb.Append(BrutalAPMod.LookupLocationName(lid, owner));
                }
                else
                {
                    sb.Append(text);
                }
            }

            if (sb.Length > 0) BrutalAPMod.AddChatLine(sb.ToString());
        }
        
        private static int ExtractInt(string obj, string key, int fallback)
        {
            int idx = obj.IndexOf(key);
            if (idx == -1) return fallback;
            int ns = idx + key.Length;
            while (ns < obj.Length && !char.IsDigit(obj[ns])) ns++;
            int ne = ns;
            while (ne < obj.Length && char.IsDigit(obj[ne])) ne++;
            return ne > ns && int.TryParse(obj.Substring(ns, ne - ns), out int v) ? v : fallback;
        }
        
        private async void SendGetDataPackage()
        {
            // games не указываем — сервер отдаст пакет по всем играм комнаты
            string json = "[{\"cmd\":\"GetDataPackage\"}]";
            var bytes = Encoding.UTF8.GetBytes(json);
            await socket.SendAsync(new ArraySegment<byte>(bytes), WebSocketMessageType.Text, true, CancellationToken.None);
            Debug.Log("AP: Sent GetDataPackage");
        }

        // Возвращает индекс закрывающей скобки для блока, открывающегося в openIdx
        private static int MatchBracket(string s, int openIdx, char open, char close)
        {
            int depth = 0;
            for (int i = openIdx; i < s.Length; i++)
            {
                if (s[i] == open) depth++;
                else if (s[i] == close) { depth--; if (depth == 0) return i; }
            }
            return -1;
        }

        private void ParseDataPackage(string json)
        {
            int gamesIdx = json.IndexOf("\"games\":{");
            if (gamesIdx == -1) return;
            int gamesStart = json.IndexOf('{', gamesIdx);
            int gamesEnd = MatchBracket(json, gamesStart, '{', '}');
            if (gamesEnd == -1) return;

            string games = json.Substring(gamesStart + 1, gamesEnd - gamesStart - 1);
            int pos = 0, count = 0;

            while (pos < games.Length)
            {
                // Имя игры — ключ верхнего уровня
                int nameStart = games.IndexOf('"', pos);
                if (nameStart == -1) break;
                int nameEnd = nameStart + 1;
                var nameSb = new StringBuilder();
                for (int i = nameStart + 1; i < games.Length; i++)
                {
                    if (games[i] == '\\' && i + 1 < games.Length) { nameSb.Append(games[i + 1]); i++; continue; }
                    if (games[i] == '"') { nameEnd = i; break; }
                    nameSb.Append(games[i]);
                }
                string gameName = nameSb.ToString();

                int objStart = games.IndexOf('{', nameEnd);
                if (objStart == -1) break;
                int objEnd = MatchBracket(games, objStart, '{', '}');
                if (objEnd == -1) break;

                string body = games.Substring(objStart, objEnd - objStart + 1);
                BrutalAPMod.dpItems[gameName] = ParseNameToIdMap(body, "\"item_name_to_id\":{");
                BrutalAPMod.dpLocations[gameName] = ParseNameToIdMap(body, "\"location_name_to_id\":{");
                count++;

                pos = objEnd + 1;
            }
            Debug.Log($"AP: DataPackage loaded for {count} games");
        }

        // "item_name_to_id":{"Name":123,...} -> {123: "Name"}
        private static Dictionary<long, string> ParseNameToIdMap(string body, string key)
        {
            var result = new Dictionary<long, string>();
            int idx = body.IndexOf(key);
            if (idx == -1) return result;
            int start = body.IndexOf('{', idx);
            int end = MatchBracket(body, start, '{', '}');
            if (end == -1) return result;

            string map = body.Substring(start + 1, end - start - 1);
            int pos = 0;
            while (pos < map.Length)
            {
                int nameStart = map.IndexOf('"', pos);
                if (nameStart == -1) break;
                var sb = new StringBuilder();
                int nameEnd = -1;
                for (int i = nameStart + 1; i < map.Length; i++)
                {
                    if (map[i] == '\\' && i + 1 < map.Length) { sb.Append(map[i + 1]); i++; continue; }
                    if (map[i] == '"') { nameEnd = i; break; }
                    sb.Append(map[i]);
                }
                if (nameEnd == -1) break;

                int colon = map.IndexOf(':', nameEnd);
                if (colon == -1) break;
                int ns = colon + 1;
                while (ns < map.Length && (map[ns] == ' ' || map[ns] == '-')) ns++;
                int ne = ns;
                while (ne < map.Length && char.IsDigit(map[ne])) ne++;
                if (ne > ns && long.TryParse(map.Substring(ns, ne - ns), out long id))
                    result[id] = sb.ToString();

                pos = ne;
            }
            return result;
        }

        private static string ExtractString(string obj, string key)
        {
            int idx = obj.IndexOf(key);
            if (idx == -1) return null;
            int vs = idx + key.Length;
            var sb = new StringBuilder();
            for (int i = vs; i < obj.Length; i++)
            {
                if (obj[i] == '\\' && i + 1 < obj.Length) { sb.Append(obj[i + 1]); i++; continue; }
                if (obj[i] == '"') break;
                sb.Append(obj[i]);
            }
            return sb.ToString();
        }

        private async void SendConnectPacket()
        {
            string myUuid = Guid.NewGuid().ToString("N");
            string pwdField = password == null ? "null" : "\"" + password.Replace("\"", "\\\"") + "\"";
            var json = "[{\"cmd\":\"Connect\",\"password\":" + pwdField + ",\"game\":\"Brutal Orchestra\",\"name\":\"" + playerName + "\",\"uuid\":\"" + myUuid + "\",\"version\":{\"major\":0,\"minor\":6,\"build\":7,\"class\":\"Version\"},\"items_handling\":7,\"tags\":[\"AP\"]}]";
            var bytes = Encoding.UTF8.GetBytes(json);
            await socket.SendAsync(new ArraySegment<byte>(bytes), WebSocketMessageType.Text, true, CancellationToken.None);
            Debug.Log("AP: Sent Connect");
        }

        public async void SendChat(string text)
        {
            if (!IsOpen) return;
            string escaped = text.Replace("\\", "\\\\").Replace("\"", "\\\"");
            string json = "[{\"cmd\":\"Say\",\"text\":\"" + escaped + "\"}]";
            var bytes = Encoding.UTF8.GetBytes(json);
            await socket.SendAsync(new ArraySegment<byte>(bytes), WebSocketMessageType.Text, true, CancellationToken.None);
        }

        private void ParseReceivedItems(string json)
        {
            // "index" — позиция первого предмета пакета в общей истории (0 = полный реплей)
            int packetIndex = 0;
            int idxPos = json.IndexOf("\"index\":");
            if (idxPos != -1)
            {
                int ns = idxPos + 8;
                while (ns < json.Length && !char.IsDigit(json[ns])) ns++;
                int ne = ns;
                while (ne < json.Length && char.IsDigit(json[ne])) ne++;
                if (ne > ns) int.TryParse(json.Substring(ns, ne - ns), out packetIndex);
            }
            if (packetIndex == 0) BrutalAPMod.startMoneyLevel = 0;

            int itemsIdx = json.IndexOf("\"items\":[");
            if (itemsIdx == -1) return;
            int start = json.IndexOf('[', itemsIdx);
            int end = json.IndexOf(']', start);
            if (start == -1 || end == -1) return;
            string itemsArray = json.Substring(start, end - start + 1);

            Debug.Log($"AP: ReceivedItems — packetIndex={packetIndex}, processedItemIndex={BrutalAPMod.processedItemIndex}");

            int itemPos = packetIndex;
            int pos = 0;
            while ((pos = itemsArray.IndexOf("\"item\":", pos)) != -1)
            {
                pos += 7;
                int numStart = pos;
                while (numStart < itemsArray.Length && !char.IsDigit(itemsArray[numStart])) numStart++;
                int numEnd = numStart;
                while (numEnd < itemsArray.Length && char.IsDigit(itemsArray[numEnd])) numEnd++;
                if (numEnd > numStart && long.TryParse(itemsArray.Substring(numStart, numEnd - numStart), out long itemId))
                {
                    bool applyTransient = itemPos >= BrutalAPMod.processedItemIndex;
                    if (BrutalAPMod.itemIdToName.TryGetValue(itemId, out string itemName))
                        BrutalAPMod.OnItemReceived(itemName, applyTransient);
                    else
                        Debug.LogWarning($"Unknown item ID: {itemId}");
                    itemPos++;
                }
                pos = numEnd;
            }

            if (itemPos > BrutalAPMod.processedItemIndex)
            {
                BrutalAPMod.processedItemIndex = itemPos;
                BrutalAPMod.SaveProcessedItemIndex();
            }
        }

        public async void SendCheck(string locationName)
        {
            if (!IsOpen) return;
            if (BrutalAPMod.locationIDs.TryGetValue(locationName, out long id))
            {
                string json = $"[{{\"cmd\":\"LocationChecks\",\"locations\":[{id}]}}]";
                var bytes = Encoding.UTF8.GetBytes(json);
                await socket.SendAsync(new ArraySegment<byte>(bytes), WebSocketMessageType.Text, true, CancellationToken.None);
                Debug.Log($"AP: Sent check {locationName} ({id})");
            }
            else Debug.LogError("Unknown location: " + locationName);
        }
        
        public async void SendDeathLink(string cause)
        {
            if (!IsOpen || !BrutalAPMod.DeathLinkEnabled) return;
            string src = BrutalAPMod.playerIdToName.TryGetValue(BrutalAPMod.mySlot, out var n) ? n : "Player";
            double ts = (DateTime.UtcNow - new DateTime(1970, 1, 1)).TotalSeconds;
            string json = "[{\"cmd\":\"Bounce\",\"tags\":[\"DeathLink\"],\"data\":{"
                          + "\"time\":" + ts.ToString(System.Globalization.CultureInfo.InvariantCulture) + ","
                          + "\"source\":\"" + src.Replace("\"", "\\\"") + "\","
                          + "\"cause\":\"" + cause.Replace("\"", "\\\"") + "\"}}]";
            var bytes = Encoding.UTF8.GetBytes(json);
            await socket.SendAsync(new ArraySegment<byte>(bytes), WebSocketMessageType.Text, true, CancellationToken.None);
            Debug.Log("AP: Sent DeathLink — " + cause);
        }
        
        public async void SendGoal()
        {
            if (!IsOpen) return;
            string json = "[{\"cmd\":\"StatusUpdate\",\"status\":30}]"; // CLIENT_GOAL
            var bytes = Encoding.UTF8.GetBytes(json);
            await socket.SendAsync(new ArraySegment<byte>(bytes), WebSocketMessageType.Text, true, CancellationToken.None);
            Debug.Log("AP: Sent goal complete!");
        }
    }
}