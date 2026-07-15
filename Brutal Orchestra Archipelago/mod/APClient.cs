using System;
using System.Text;
using System.Net.WebSockets;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace BrutalOrchestraAr
{
    public class APClient
    {
        private ClientWebSocket socket = new ClientWebSocket();
        private string serverUri;
        private string playerName;
        public static string CurrentSeed = "";

        public APClient(string uri, string name)
        {
            serverUri = uri;
            playerName = name;
        }

        public async void Connect()
        {
            try
            {
                Uri uri = new Uri(serverUri);
                await socket.ConnectAsync(uri, CancellationToken.None);
                Debug.Log("AP Connected");
                ReceiveLoop();
            }
            catch (Exception e)
            {
                Debug.LogError("AP Connect Error: " + e);
            }
        }

        private async void ReceiveLoop()
        {
            var buffer = new byte[4096];
            var messageBuffer = new StringBuilder();
            bool sentConnect = false;

            while (socket.State == WebSocketState.Open)
            {
                var result = await socket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
                string chunk = Encoding.UTF8.GetString(buffer, 0, result.Count);
                Debug.Log("AP RAW: " + chunk);
                messageBuffer.Append(chunk);
                string msg = messageBuffer.ToString();

                while (true)
                {
                    int start = msg.IndexOf('[');
                    if (start == -1) break;
                    int depth = 0;
                    int end = -1;
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

                    if (!sentConnect && json.Contains("\"cmd\":\"RoomInfo\""))
                    {
                        // Parse seed_name for save slot identification
                        int seedIdx = json.IndexOf("\"seed_name\":\"");
                        if (seedIdx != -1)
                        {
                            int seedStart = seedIdx + 13;
                            int seedEnd = json.IndexOf("\"", seedStart);
                            if (seedEnd != -1)
                                CurrentSeed = json.Substring(seedStart, seedEnd - seedStart);
                        }
                        Debug.Log("AP: Got RoomInfo, sending Connect");
                        sentConnect = true;
                        SendConnectPacket();
                    }
                    else if (json.Contains("\"cmd\":\"Connected\""))
                    {
                        Debug.Log("AP: Successfully connected to room!");
                        // Extract slot_data
                        int slotDataIndex = json.IndexOf("\"slot_data\":{");
                        if (slotDataIndex != -1)
                        {
                            int bracketOpen = json.IndexOf("{", slotDataIndex);
                            int sdDepth = 0;
                            int bracketClose = -1;
                            for (int i = bracketOpen; i < json.Length; i++)
                            {
                                if (json[i] == '{') sdDepth++;
                                else if (json[i] == '}') { sdDepth--; if (sdDepth == 0) { bracketClose = i; break; } }
                            }
                            if (bracketClose != -1)
                            {
                                string slotDataJson = json.Substring(bracketOpen, bracketClose - bracketOpen + 1);
                                BrutalAPMod.InitSlotData(slotDataJson);
                            }
                        }
                    }
                    else if (json.Contains("\"cmd\":\"ReceivedItems\""))
                    {
                        ParseReceivedItems(json);
                    }
                    else
                    {
                        Debug.Log("AP RECV: " + json);
                    }
                }
            }
        }

        private async void SendConnectPacket()
        {
            string myUuid = System.Guid.NewGuid().ToString("N");
            var json = "[{\"cmd\":\"Connect\",\"password\":null,\"game\":\"Brutal Orchestra\",\"name\":\"" + playerName + "\",\"uuid\":\"" + myUuid + "\",\"version\":{\"major\":0,\"minor\":6,\"build\":7,\"class\":\"Version\"},\"items_handling\":7,\"tags\":[\"AP\"]}]";
            Debug.Log("AP JSON TO SEND: " + json);
            var bytes = Encoding.UTF8.GetBytes(json);
            await socket.SendAsync(new ArraySegment<byte>(bytes), WebSocketMessageType.Text, true, CancellationToken.None);
            Debug.Log("AP: Sent Connect");
        }

        private void ParseReceivedItems(string json)
        {
            // "index" — позиция первого предмета пакета в общей истории.
            // 0 при коннекте = сервер переигрывает всю историю с начала.
            long packetIndex = 0;
            int idxPos = json.IndexOf("\"index\":");
            if (idxPos != -1)
            {
                int numStart = idxPos + 8;
                while (numStart < json.Length && !char.IsDigit(json[numStart])) numStart++;
                int numEnd = numStart;
                while (numEnd < json.Length && char.IsDigit(json[numEnd])) numEnd++;
                if (numEnd > numStart) long.TryParse(json.Substring(numStart, numEnd - numStart), out packetIndex);
            }

            int itemsIdx = json.IndexOf("\"items\":[");
            if (itemsIdx == -1) return;
            int start = json.IndexOf('[', itemsIdx);
            int end = json.IndexOf(']', start);
            if (start == -1 || end == -1) return;
            string itemsArray = json.Substring(start, end - start + 1);

            long itemPos = packetIndex;
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
                    // Предмет уже обрабатывался в прошлой сессии — переигрываем только разлоки, не монеты
                    bool isReplay = itemPos < BrutalAPMod.processedItemIndex;

                    if (BrutalAPMod.itemIdToName.TryGetValue(itemId, out string itemName))
                        BrutalAPMod.OnItemReceived(itemName, isReplay);
                    else
                        Debug.LogWarning($"Unknown item ID: {itemId}");

                    if (!isReplay)
                    {
                        BrutalAPMod.processedItemIndex = (int)(itemPos + 1);
                        BrutalAPMod.SaveProcessedItemIndex();
                    }
                }
                itemPos++;
                pos = numEnd;
            }
        }

        public async void SendCheck(string locationName)
        {
            if (socket.State != WebSocketState.Open) return;
            if (BrutalAPMod.locationIDs.TryGetValue(locationName, out long id))
            {
                string json = $"[{{\"cmd\":\"LocationChecks\",\"locations\":[{id}]}}]";
                Debug.Log($"AP: Sending check JSON: {json}");
                var bytes = Encoding.UTF8.GetBytes(json);
                await socket.SendAsync(new ArraySegment<byte>(bytes), WebSocketMessageType.Text, true, CancellationToken.None);
                Debug.Log($"AP: Sent check {locationName} ({id})");
            }
            else
            {
                Debug.LogError("Unknown location: " + locationName);
            }
        }
    }
}