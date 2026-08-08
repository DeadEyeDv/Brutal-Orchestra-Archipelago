using System;
using System.Collections.Generic;
using System.Text;
using System.Net.WebSockets;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace BrutalOrchestraAr
{
    public partial class APClient
    {
        private readonly SemaphoreSlim sendLock = new SemaphoreSlim(1, 1);
        
        public async void SendConnectUpdate(bool deathLinkOn)
        {
            string tags = deathLinkOn ? "[\"AP\",\"DeathLink\"]" : "[\"AP\"]";
            await SendRaw("[{\"cmd\":\"ConnectUpdate\",\"tags\":" + tags + "}]");
            Debug.Log("AP: DeathLink " + (deathLinkOn ? "enabled" : "disabled"));
        }
        
        private async void SendConnectPacket()
        {
            string myUuid = Guid.NewGuid().ToString("N");
            string pwdField = password == null ? "null" : "\"" + password.Replace("\"", "\\\"") + "\"";
            var json = "[{\"cmd\":\"Connect\",\"password\":" + pwdField + ",\"game\":\"Brutal Orchestra\",\"name\":\"" + playerName + "\",\"uuid\":\"" + myUuid + "\",\"version\":{\"major\":0,\"minor\":6,\"build\":7,\"class\":\"Version\"},\"items_handling\":7,\"tags\":[\"AP\"]}]";
            CurrentSlot = playerName;
            await SendRaw(json);
            Debug.Log("AP: Sent Connect");
        }
        
        public async void SendChat(string text)
        {
            string escaped = text.Replace("\\", "\\\\").Replace("\"", "\\\"");
            await SendRaw("[{\"cmd\":\"Say\",\"text\":\"" + escaped + "\"}]");
        }
        
        public async void SendCheck(string locationName)
        {
            if (!IsOpen)
            {
                BrutalAPMod.pendingChecks.Enqueue(locationName);
                Debug.Log("AP: socket closed, queued check " + locationName);
                return;
            }
            if (BrutalAPMod.locationIDs.TryGetValue(locationName, out long id))
            {
                await SendRaw($"[{{\"cmd\":\"LocationChecks\",\"locations\":[{id}]}}]");
                Debug.Log($"AP: Sent check {locationName} ({id})");
            }
            else Debug.LogError("Unknown location: " + locationName);
        }
        
        public async void SendChecks(List<long> locationIds)
        {
            if (locationIds == null || locationIds.Count == 0) return;
            string ids = string.Join(",", locationIds);
            await SendRaw("[{\"cmd\":\"LocationChecks\",\"locations\":[" + ids + "]}]");
        }
        
        public async void SendDeathLink(string cause)
        {
            if (!BrutalAPMod.DeathLinkEnabled) return;
            string src = BrutalAPMod.playerIdToName.TryGetValue(BrutalAPMod.mySlot, out var n) ? n : "Player";
            double ts = (DateTime.UtcNow - new DateTime(1970, 1, 1)).TotalSeconds;
            string json = "[{\"cmd\":\"Bounce\",\"tags\":[\"DeathLink\"],\"data\":{"
                          + "\"time\":" + ts.ToString(System.Globalization.CultureInfo.InvariantCulture) + ","
                          + "\"source\":\"" + src.Replace("\"", "\\\"") + "\","
                          + "\"cause\":\"" + cause.Replace("\"", "\\\"") + "\"}}]";
            await SendRaw(json);
            Debug.Log("AP: Sent DeathLink — " + cause);
        }
        
        public async void SendGoal()
        {
            await SendRaw("[{\"cmd\":\"StatusUpdate\",\"status\":30}]");
            Debug.Log("AP: Sent goal complete!");
        }
        
        private async void SendGetDataPackage()
        {
            await SendRaw("[{\"cmd\":\"GetDataPackage\"}]");
            Debug.Log("AP: Sent GetDataPackage");
        }
        
        private async Task SendRaw(string json)
        {
            if (!IsOpen) return;
            var bytes = Encoding.UTF8.GetBytes(json);
            await sendLock.WaitAsync();
            try
            {
                await socket.SendAsync(new ArraySegment<byte>(bytes), WebSocketMessageType.Text, true, CancellationToken.None);
            }
            finally { sendLock.Release(); }
        }
    }
}