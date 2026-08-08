using System;
using System.Collections.Generic;
using System.Text;
using System.Net.WebSockets;
using System.Threading;
using UnityEngine;
using System.Threading.Tasks;  

namespace BrutalOrchestraAr
{
    public partial class APClient
    {
        private ClientWebSocket socket;
        private string host;          // host:port, без схемы
        private bool sentConnect = false;
        private string playerName;
        private string password;
        public bool needReconnect = false;
        public bool isAutoReconnect = false;
        public static bool userDisconnected = false;
        public static string CurrentSeed = "";
        public static string CurrentSlot = null;
        public static string StateKey => (CurrentSeed ?? "default") + "_" + (CurrentSlot ?? "noslot");
        public bool IsOpen => socket != null && socket.State == WebSocketState.Open;

        public APClient(string hostPort, string name, string pass = null)
        {
            host = hostPort;
            playerName = name;
            password = pass;
        }

        public async void Connect()
        {
            userDisconnected = false;
            sentConnect = false;
            foreach (var scheme in new[] { "wss://", "ws://" })
            {
                socket = new ClientWebSocket();
                try
                {
                    await socket.ConnectAsync(new Uri(scheme + host), CancellationToken.None);
                    Debug.Log($"AP: Socket connected via {scheme}");
                    needReconnect = false;   // успех — гасим флаг
                    ReceiveLoop();
                    return;
                }
                catch (Exception e)
                {
                    Debug.Log($"AP: {scheme} failed — {e.Message}");
                }
            }
            // обе схемы не удались
            if (isAutoReconnect)
            {
                needReconnect = true;   // держим флаг — таймер повторит
                Debug.Log("AP: Reconnect attempt failed, will retry");
            }
            else
            {
                BrutalAPMod.OnConnectFailed("Cannot reach " + host);
            }
        }

        private async void ReceiveLoop()
        {
            var buffer = new byte[4096];
            var messageBuffer = new StringBuilder();

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
            }

            if (!userDisconnected)
            {
                needReconnect = true;
                Debug.Log("AP: Connection dropped, reconnect scheduled");
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
                    {
                        BrutalAPMod.InitSlotData(json.Substring(bracketOpen, bracketClose - bracketOpen + 1));
                        var checkedIds = ParseLongArray(json, "\"checked_locations\":[");
                        BrutalAPMod.SyncCheckedFromServer(checkedIds);
                    }
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
        
        public void Disconnect()
        {
            try
            {
                if (socket != null && (socket.State == WebSocketState.Open || socket.State == WebSocketState.Connecting))
                    socket.CloseAsync(WebSocketCloseStatus.NormalClosure, "user disconnect", CancellationToken.None);
            }
            catch (Exception e) { Debug.Log("AP: Disconnect error — " + e.Message); }
            Debug.Log("AP: Disconnected by user");
        }
        
        
        
    }
}