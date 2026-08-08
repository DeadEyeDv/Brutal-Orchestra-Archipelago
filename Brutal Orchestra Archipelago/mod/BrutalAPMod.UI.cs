using BepInEx;
using HarmonyLib;
using UnityEngine;
using System.Collections.Generic;
using System;
using System.Linq;
using System.Text;

namespace BrutalOrchestraAr
{
    public partial class BrutalAPMod : BaseUnityPlugin
    {
        private static readonly List<string> chatLines = new List<string>();
        private static readonly object chatLock = new object();
        
        public static void AddChatLine(string line)
        {
            lock (chatLock)
            {
                chatLines.Add(line);
                if (chatLines.Count > 200) chatLines.RemoveAt(0);
            }
        }
        
        void DrawConnectWindow(int id)
        {
            GUI.enabled = !isConnecting;

            GUILayout.Label("Server (host:port):");
            guiServer = GUILayout.TextField(guiServer);
            GUILayout.Label("Slot Name:");
            guiSlot = GUILayout.TextField(guiSlot);
            GUILayout.Label("Password (optional):");
            guiPassword = GUILayout.PasswordField(guiPassword, '*');

            if (!string.IsNullOrEmpty(connectStatus))
                GUILayout.Label(connectStatus);

            if (GUILayout.Button(isConnecting ? "Connecting..." : "Connect"))
            {
                string server = guiServer.Trim();
                string slot = guiSlot.Trim();

                if (string.IsNullOrEmpty(server) || string.IsNullOrEmpty(slot))
                {
                    connectStatus = "Enter server and slot name.";
                }
                else
                {
                    server = StripScheme(server);
                    PlayerPrefs.SetString(PrefServer, server);
                    PlayerPrefs.SetString(PrefSlot, slot);
                    PlayerPrefs.Save();

                    isConnecting = true;
                    connectStatus = "Connecting...";
                    apClient = new APClient(server, slot, string.IsNullOrEmpty(guiPassword) ? null : guiPassword);
                    apClient.isAutoReconnect = false;
                    apClient.Connect();
                }
            }

            GUI.enabled = true;
            GUI.DragWindow();
        }

        void DrawChatWindow(int id)
        {

            GUILayout.BeginHorizontal();
            if (GUILayout.Button(chatCollapsed ? "▼ Expand" : "▲ Collapse"))
            {
                chatCollapsed = !chatCollapsed;
                chatRect.height = chatCollapsed ? 60 : 260;
            }

            if (GUILayout.Button(DeathLinkEnabled ? "DeathLink: ON" : "DeathLink: OFF", GUILayout.Width(120)))
            {
                DeathLinkEnabled = !DeathLinkEnabled;
                apClient?.SendConnectUpdate(DeathLinkEnabled);
                AddChatLine("*** DeathLink " + (DeathLinkEnabled ? "enabled" : "disabled") + " ***");
            }

            GUILayout.EndHorizontal();

            if (!chatCollapsed)
            {

                float prevScrollY = chatScroll.y;

                chatScroll = GUILayout.BeginScrollView(chatScroll, GUILayout.Height(chatViewportHeight));
                var richLabel = new GUIStyle(GUI.skin.label) { richText = true, wordWrap = true };
                GUILayout.BeginVertical();
                lock (chatLock)
                    foreach (var line in chatLines)
                        GUILayout.Label(line, richLabel);
                GUILayout.EndVertical();

                if (Event.current.type == EventType.Repaint)
                    chatContentHeight = GUILayoutUtility.GetLastRect().yMax;

                GUILayout.EndScrollView();

                float maxScroll = Mathf.Max(0f, chatContentHeight - chatViewportHeight);

                if (chatScroll.y != prevScrollY)
                    chatAutoScroll = chatScroll.y >= maxScroll - 10f;

                if (chatAutoScroll && Event.current.type == EventType.Repaint)
                    chatScroll.y = maxScroll;

                GUILayout.BeginHorizontal();

                GUI.SetNextControlName("ChatInput");
                chatInput = GUILayout.TextField(chatInput);

                bool enterPressed = Event.current.type == EventType.KeyDown
                                    && (Event.current.keyCode == KeyCode.Return
                                        || Event.current.keyCode == KeyCode.KeypadEnter
                                        || Event.current.character == '\n'
                                        || Event.current.character == '\r')
                                    && GUI.GetNameOfFocusedControl() == "ChatInput";

                if ((GUILayout.Button("Send", GUILayout.Width(60)) || enterPressed) && !string.IsNullOrEmpty(chatInput))
                {
                    if (chatInput.Trim().ToLower() == "/disconnect" || chatInput.Trim().ToLower() == "/dc")
                    {
                        APClient.userDisconnected = true;
                        apClient?.Disconnect();
                        apClient = null;
                        AddChatLine("*** Disconnected ***");
                        showGui = true;   // сразу показать окно коннекта
                        showChat = false;
                    }
                    else
                    {
                        apClient?.SendChat(chatInput);
                    }
                    chatInput = "";
                    if (enterPressed) Event.current.Use();
                    GUI.FocusControl("ChatInput");
                }

                GUILayout.EndHorizontal();
            }

            GUI.DragWindow();
        }
        
        private static string StripScheme(string s)
        {
            foreach (var p in new[] { "wss://", "ws://", "https://", "http://" })
                if (s.StartsWith(p, StringComparison.OrdinalIgnoreCase))
                    return s.Substring(p.Length);
            return s;
        }
        
        public static void OnConnectSucceeded()
        {
            if (apClient != null) apClient.needReconnect = false;
            isConnecting = false;
            connectStatus = "";
            showGui = false;
            showChat = true;
            FlushPendingChecks();
            ResyncAllChecks();
            AddChatLine("*** Connected ***");
            Debug.Log("AP: Connected successfully");
        }

        public static void OnConnectFailed(string reason)
        {
            isConnecting = false;
            connectStatus = "Failed: " + reason;
            showGui = true;
            Debug.LogWarning("AP: Connect failed — " + reason);
        }
    }
}