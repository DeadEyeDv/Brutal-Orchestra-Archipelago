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
                        Debug.Log("AP: Got RoomInfo, sending Connect");
                        sentConnect = true;
                        SendConnectPacket();
                    }
                    else if (json.Contains("\"cmd\":\"Connected\""))
                    {
                        Debug.Log("AP: Successfully connected to room!");
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
            int itemsIndex = json.IndexOf("\"items\":[");
            if (itemsIndex == -1) return;

            int bracketOpen = json.IndexOf('[', itemsIndex);
            if (bracketOpen == -1) return;

            int depth = 0;
            int bracketClose = -1;
            for (int i = bracketOpen; i < json.Length; i++)
            {
                if (json[i] == '[') depth++;
                else if (json[i] == ']') { depth--; if (depth == 0) { bracketClose = i; break; } }
            }
            if (bracketClose == -1) return;

            string itemsArray = json.Substring(bracketOpen + 1, bracketClose - bracketOpen - 1);

            int pos = 0;
            while (pos < itemsArray.Length)
            {
                int objStart = itemsArray.IndexOf('{', pos);
                if (objStart == -1) break;
                int objDepth = 0;
                int objEnd = -1;
                for (int i = objStart; i < itemsArray.Length; i++)
                {
                    if (itemsArray[i] == '{') objDepth++;
                    else if (itemsArray[i] == '}') { objDepth--; if (objDepth == 0) { objEnd = i; break; } }
                }
                if (objEnd == -1) break;
                string itemObj = itemsArray.Substring(objStart, objEnd - objStart + 1);

                int itemIdKey = itemObj.IndexOf("\"item\":");
                if (itemIdKey != -1)
                {
                    int valStart = itemIdKey + 7;
                    while (valStart < itemObj.Length && (itemObj[valStart] == ' ' || itemObj[valStart] == ':')) valStart++;
                    int valEnd = valStart;
                    while (valEnd < itemObj.Length && (char.IsDigit(itemObj[valEnd]) || itemObj[valEnd] == '-')) valEnd++;
                    if (valEnd > valStart && long.TryParse(itemObj.Substring(valStart, valEnd - valStart), out long itemId))
                    {
                        if (BrutalAPMod.itemIdToName.TryGetValue(itemId, out string itemName))
                        {
                            Debug.Log($"AP: Parsed item from ReceivedItems: {itemName} (ID {itemId})");
                            BrutalAPMod.OnItemReceived(itemName);
                        }
                        else
                            Debug.Log($"AP: Unknown item ID {itemId}");
                    }
                }
                pos = objEnd + 1;
            }
        }

        public async void SendCheck(string locationName)
        {
            if (socket.State != WebSocketState.Open) return;
            if (BrutalAPMod.locationIDs.TryGetValue(locationName, out long id))
            {
                string json = $"[{{\"cmd\":\"LocationChecks\",\"locations\":[{id}]}}]";
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