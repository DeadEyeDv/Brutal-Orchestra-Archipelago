using BepInEx;
using HarmonyLib;
using UnityEngine;
using System.Collections.Generic;
using System;
using System.Linq;
using System.Text;

namespace BrutalOrchestraAr
{
    [BepInPlugin("brutal.ap.mod", "Brutal AP Mod", "2.2")]
    public partial class BrutalAPMod : BaseUnityPlugin
    {
        public static APClient apClient;
        public static HashSet<string> receivedAPItems = new HashSet<string>();
        public static HashSet<string> sentChecks = new HashSet<string>();
        public static Queue<string> pendingChecks = new Queue<string>();

        public static int farBattleCount,
            orpBattleCount,
            farMoneyChestCount,
            orpMoneyChestCount,
            farArtifactChestCount,
            orpArtifactChestCount,
            farShopCount,
            orpShopCount,
            buyHeroCount,
            gardenBattleCount,
            gardenMoneyChestCount,
            gardenArtifactChestCount,
            gardenShopCount,
            bossDefeatCount;

        public static int farBattleLimit = 15,
            orpBattleLimit = 15,
            farMoneyLimit = 2,
            orpMoneyLimit = 2,
            farArtifactLimit = 2,
            orpArtifactLimit = 2,
            shopCountFar = 6,
            shopCountOrp = 6,
            buyHeroLimit = 6,
            bossCount = 3,
            gardenBattleLimit = 10,
            gardenMoneyLimit = 2,
            gardenArtifactLimit = 2,
            gardenShopLimit = 6,
            startMoneyLevel = 0,
            goalType = 0,
            pearlCount = 10,
            runCounter = 0;


        public static bool AllowUnlocks = false;
        public static bool IsHardmode = false;
        public static bool DeathLinkEnabled = false;
        public static bool suppressDeathLink = false;
        public static bool InCombat = false;
        public static int processedItemIndex = 0;

        public static void UnlockWithAP(Action a)
        {
            AllowUnlocks = true;
            try
            {
                a();
            }
            finally
            {
                AllowUnlocks = false;
            }
        }

        public static GameInformationHolder cachedHolder = null;
        public static Queue<Action> pendingActions = new Queue<Action>();
        private static Queue<int> pendingCoins = new Queue<int>();

        private static Dictionary<string, UnlockableModData> allUnlockableData =
            new Dictionary<string, UnlockableModData>();

        public static Dictionary<string, long> locationIDs = new Dictionary<string, long>();
        public static Dictionary<long, string> locationIdToName = new Dictionary<long, string>();
        public static Dictionary<int, string> playerIdToName = new Dictionary<int, string>();
        public static Dictionary<int, string> playerIdToGame = new Dictionary<int, string>();
        public static int mySlot = -1;

        public static Dictionary<string, Dictionary<long, string>> dpItems =
            new Dictionary<string, Dictionary<long, string>>();

        public static Dictionary<string, Dictionary<long, string>> dpLocations =
            new Dictionary<string, Dictionary<long, string>>();

        private static readonly HashSet<string> transientItems = new HashSet<string>
            { "5 Coins", "10 Coins", "15 Coins" };

        public static bool abilitySanity = false;
        public static Dictionary<string, int> abilityLevels = new Dictionary<string, int>();
        public static Dictionary<string, List<int>> runAbilityPick = new Dictionary<string, List<int>>();

        private static readonly string[] allUnlockIDs =
        {
            "Roids", "Hickory", "Mobius", "TheOuroboros", "Smoothskin", "FarShore", "Orpheum", "ZoneExplorer",
            "BossSlap",
            "RoidsMissTurn", "SmoothskinTrauma", "DontAct", "FoolsDepleted", "ShopDepleted", "Ending_GameOver",
            "LoseToBoss",
            "ManaCostDeath", "OverflowDeath", "OrroSmooch", "Ending_CorpseKill", "SurviveStarvation", "Over100Coins",
            "OneTurnHardKill", "FriendlyFireDeath", "ParasiteJourney", "Ending_CorpseSave", "Ending_HardMode",
            "TriggerFingers", "Charcarrion", "OsmanSinnoks", "Heaven", "AllBosses", "Ichor1", "Ichor2", "Ichor3",
            "Ichor4",
            "Fogs1", "Fogs2", "Fogs3", "UngodEmissary", "Garden", "FarShoreNoCasualtiesHard", "OrpheumNoCasualtiesHard",
            "GardenNoCasualtiesHard", "AllZonesNoCasualtiesHard", "WorldExplorer", "AllBossesSlap", "BasicSpeedrun",
            "UngodKill", "SepulchreKill", "XiphactinusKill", "UnfinishedHeirKill", "CharcarrionDecomposition",
            "NoNowakHardEnd", "NowakLoneSurvivor", "HeavenDoubleSacrifice", "CasualtiesCharacter5", "CasualtiesEnemy30",
            "HeadshotDeath", "DamageDealt100", "AntonSad",
            "Boyle_Osman", "Boyle_Heaven", "Hans_Osman", "Hans_Heaven", "Burnout_Osman", "Burnout_Heaven",
            "Fennec_Osman",
            "Fennec_Heaven", "Anton_Osman", "Anton_Heaven", "Splig_Osman", "Splig_Heaven", "Pearl_Osman",
            "Pearl_Heaven",
            "Thype_Osman", "Thype_Heaven", "Griffin_Osman", "Griffin_Heaven", "Arnold_Osman", "Arnold_Heaven",
            "Dimitri_Osman", "Dimitri_Heaven", "LongLiver_Osman", "LongLiver_Heaven", "Clive_Osman", "Clive_Heaven",
            "Kleiver_Osman", "Kleiver_Heaven", "Cranes_Osman", "Cranes_Heaven", "Agon_Osman", "Agon_Heaven",
            "Rags_Osman",
            "Rags_Heaven", "SmokeStacks_Osman", "SmokeStacks_Heaven", "Leviat_Osman", "Leviat_Heaven", "Gospel_Osman",
            "Gospel_Heaven", "Bimini_Osman", "Bimini_Heaven", "Mung_Osman", "Mung_Heaven", "Mordrake_Osman",
            "Mordrake_Heaven", "ShellyK_Osman", "ShellyK_Heaven", "Formosus_Osman", "Formosus_Heaven", "ProdigalFlee",
            "MordrakeCH",
            "Bronzo1", "Bronzo2", "BronzoBossPhase1", "Bronzo3", "BronzoBossPhase2", "Bronzo4", "BronzoBossPhase3",
            "Bronzo5", "BronzoBossPhase4", "Bronzo6", "BronzoBoss",
            "Mordrake1", "Mordrake2", "Mordrake3", "Mordrake4", "Mordrake5", "Mordrake6",
            "VHSTask0", "VHSTask1", "VHSTask2", "VHSTask3", "VHSTask4", "VHSTask5", "VHSTask6",
            "Winstreak2", "Winstreak3", "Winstreak4", "Winstreak5", "HundredPercent"
        };

        private static bool showGui = true;
        private static string guiServer = "localhost:38281";
        private static string guiSlot = "Test1";
        private static string guiPassword = "";
        private static string connectStatus = "";
        private static bool isConnecting = false;
        private static bool showChat = false;
        private Rect windowRect = new Rect(20, 20, 320, 190);
        private Rect chatRect = new Rect(20, 230, 420, 260);
        private bool chatCollapsed = false;
        private string chatInput = "";
        private Vector2 chatScroll = Vector2.zero;
        private bool chatAutoScroll = true;
        private float chatContentHeight = 0f;
        private const float chatViewportHeight = 160f;

        private const string PrefServer = "BrutalAP_LastServer";
        private const string PrefSlot = "BrutalAP_LastSlot";

        void Start()
        {
            new Harmony("brutal.ap.mod").PatchAll();
            ForceCacheUnlockableData();
            DumpUnlockableIDs();
            guiServer = PlayerPrefs.GetString(PrefServer, "localhost:38281");
            guiSlot = PlayerPrefs.GetString(PrefSlot, "");
            showGui = true;
            Debug.Log("MOD LOADED – waiting for user to connect...");
        }
        
        void OnGUI()
        {
            if (Event.current.type == EventType.KeyDown && Event.current.keyCode == KeyCode.F1 && apClient != null)
            {
                showChat = !showChat;
                Event.current.Use();
            }

            if (showGui)
                windowRect = GUILayout.Window(123456, windowRect, DrawConnectWindow, "Archipelago Connect");

            if (showChat)
                chatRect = GUILayout.Window(123457, chatRect, DrawChatWindow, "Archipelago");
        }

        public static void InitSlotData(string slotDataJson)
        {
            try
            {
                int farBat = 24,
                    orpBat = 24,
                    farMon = 2,
                    orpMon = 2,
                    farArt = 2,
                    orpArt = 2,
                    shopFar = 24,
                    shopOrp = 24,
                    bosses = 3,
                    gardenBat = 10,
                    gardenMon = 2,
                    gardenArt = 2;
                bool hardmode = true;
                bool deathLink = false;

                if (!string.IsNullOrEmpty(slotDataJson))
                {
                    ParseInt(slotDataJson, "\"far_battle_count\"", ref farBat);
                    ParseInt(slotDataJson, "\"orp_battle_count\"", ref orpBat);
                    ParseInt(slotDataJson, "\"far_money_chests\"", ref farMon);
                    ParseInt(slotDataJson, "\"orp_money_chests\"", ref orpMon);
                    ParseInt(slotDataJson, "\"far_artifact_chests\"", ref farArt);
                    ParseInt(slotDataJson, "\"orp_artifact_chests\"", ref orpArt);
                    ParseInt(slotDataJson, "\"shop_far\"", ref shopFar);
                    ParseInt(slotDataJson, "\"shop_orp\"", ref shopOrp);
                    ParseInt(slotDataJson, "\"goal_type\"", ref goalType);
                    ParseInt(slotDataJson, "\"pearl_count\"", ref pearlCount);
                    ParseBool(slotDataJson, "\"hardmode\"", ref hardmode);
                    ParseBool(slotDataJson, "\"death_link\"", ref deathLink);
                    ParseBool(slotDataJson, "\"ability_sanity\"", ref abilitySanity);
                    ParseInt(slotDataJson, "\"buy_fool_count\"", ref buyHeroLimit);
                    if (hardmode)
                    {
                        ParseInt(slotDataJson, "\"garden_battle_count\"", ref gardenBat);
                        ParseInt(slotDataJson, "\"garden_money_chests\"", ref gardenMon);
                        ParseInt(slotDataJson, "\"garden_artifact_chests\"", ref gardenArt);
                    }
                }

                DeathLinkEnabled = deathLink;
                Debug.Log($"AP: DeathLinkEnabled = {DeathLinkEnabled}");
                if (DeathLinkEnabled) apClient?.SendConnectUpdate(true);
                Debug.Log($"AP: DeathLinkEnabled = {DeathLinkEnabled}");

                farBattleLimit = farBat;
                orpBattleLimit = orpBat;
                farMoneyLimit = farMon;
                orpMoneyLimit = orpMon;
                farArtifactLimit = farArt;
                orpArtifactLimit = orpArt;
                shopCountFar = shopFar;
                shopCountOrp = shopOrp;
                bossCount = bosses;
                gardenBattleLimit = hardmode ? gardenBat : 0;
                gardenMoneyLimit = hardmode ? gardenMon : 0;
                gardenArtifactLimit = hardmode ? gardenArt : 0;
                gardenShopLimit = 0;

                locationIDs.Clear();

                int baseId = 100;
                locationIDs["Far Shore Access"] = baseId++;
                locationIDs["Orpheum Access"] = baseId++;
                locationIDs["Quarry Access"] = baseId++;
                for (int i = 1; i <= 15; i++)
                    locationIDs["BuyHero_" + i] = 103 + i - 1;

                int farBattleId = 200;
                for (int i = 1; i <= farBattleLimit; i++) locationIDs[$"Far_Battle_{i}"] = farBattleId++;
                int orpBattleId = 300;
                for (int i = 1; i <= orpBattleLimit; i++) locationIDs[$"Orp_Battle_{i}"] = orpBattleId++;
                int farMoneyId = 400;
                for (int i = 1; i <= farMoneyLimit; i++) locationIDs[$"Far_MoneyChest_{i}"] = farMoneyId++;
                int orpMoneyId = 500;
                for (int i = 1; i <= orpMoneyLimit; i++) locationIDs[$"Orp_MoneyChest_{i}"] = orpMoneyId++;
                int farArtifactId = 600;
                for (int i = 1; i <= farArtifactLimit; i++) locationIDs[$"Far_ArtifactChest_{i}"] = farArtifactId++;
                int orpArtifactId = 700;
                for (int i = 1; i <= orpArtifactLimit; i++) locationIDs[$"Orp_ArtifactChest_{i}"] = orpArtifactId++;

                int bossId = 800;
                if (bossCount >= 1) locationIDs["Far Boss"] = bossId++;
                if (bossCount >= 2) locationIDs["Orp Boss"] = bossId++;
                if (bossCount >= 3) locationIDs["Quarry Boss"] = bossId++;

                int farShopId = 900;
                for (int i = 1; i <= shopCountFar; i++) locationIDs[$"Shop_Far_{i}"] = farShopId++;
                int orpShopId = 1000;
                for (int i = 1; i <= shopCountOrp; i++) locationIDs[$"Shop_Orp_{i}"] = orpShopId++;

                int heroId = 1100;
                string[] heroNames =
                {
                    "Anton", "Splig", "Pearl", "Thype", "Griffin", "Arnold", "Dimitri", "LongLiver", "Clive",
                    "Kleiver", "Cranes", "Agon", "Rags", "SmokeStacks", "Leviat", "Gospel", "Bimini", "Burnout",
                    "Fennec",
                    "Mordrake", "Mung", "ShellyK", "Formosus"
                };
                foreach (var name in heroNames) locationIDs[heroCheckName[name]] = heroId++;

                int itemId = 1300;
                HashSet<string> added = new HashSet<string>();
                foreach (string uid in allUnlockIDs)
                {
                    string cname = itemCheckNames.TryGetValue(uid, out string n) ? n : ("Item_" + uid);
                    if (added.Add(cname)) locationIDs[cname] = itemId++;
                }

                locationIDs["Quarry_Boss_Spared"] = 2700;
                locationIDs["Mordrake's Untold Tale"] = 2800;
                locationIDs["The Director's Final Frame"] = 2900;

                if (hardmode)
                {
                    int gardenBattleId = 3000;
                    for (int i = 1; i <= gardenBattleLimit; i++) locationIDs[$"Garden_Battle_{i}"] = gardenBattleId++;
                    int gardenMoneyId = 3100;
                    for (int i = 1; i <= gardenMoneyLimit; i++) locationIDs[$"Garden_MoneyChest_{i}"] = gardenMoneyId++;
                    int gardenArtifactId = 3200;
                    for (int i = 1; i <= gardenArtifactLimit; i++)
                        locationIDs[$"Garden_ArtifactChest_{i}"] = gardenArtifactId++;
                    locationIDs["Garden Boss"] = 3400;
                }

                for (int i = 1; i <= 10; i++) locationIDs[$"Quarry Boss Defeat {i}"] = 4000 + i;
                for (int i = 1; i <= 10; i++) locationIDs[$"Garden Boss Defeat {i}"] = 4100 + i;

                locationIdToName.Clear();
                foreach (var kv in locationIDs) locationIdToName[kv.Value] = kv.Key;

                Debug.Log($"AP: Initialized location IDs. Hardmode: {hardmode}");

                EnsureMinimumHeroes();
                LoadCountersForCurrentSeed();
                LoadSentChecks();
                LoadAbilityLevels();
                LoadRunAbilityPick();
                LoadRunCounter();
                LoadBossDefeatCount();
                LoadProcessedItemIndex();
                LoadReceivedItemsForCurrentSeed();

                if (cachedHolder == null)
                    cachedHolder = Resources.FindObjectsOfTypeAll<GameInformationHolder>().FirstOrDefault();
                ApplyPendingCoins();
                ProcessPendingActions();
            }
            catch (Exception e)
            {
                Debug.LogError("InitSlotData error: " + e);
            }
        }

        private static IEnumerable<(string key, UnlockableModData data)> EnumerateUnlockables(
            UnlockablesManager manager)
        {
            var dbField = typeof(UnlockablesManager).GetField("_newUnlockableDB",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            var unlockableDB = dbField?.GetValue(manager) as UnlockablesDatabase;
            if (unlockableDB == null) yield break;
            var byIDField = typeof(UnlockablesDatabase).GetField("m_ByIDUnlockables",
                System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);
            var dictObj = byIDField?.GetValue(unlockableDB);
            var enumerable = dictObj as System.Collections.IEnumerable
                             ?? Traverse.Create(dictObj).Property("Values")?.GetValue() as
                                 System.Collections.IEnumerable;
            if (enumerable == null) yield break;

            foreach (var item in enumerable)
            {
                string key = null;
                UnlockableModData data = null;
                if (item is KeyValuePair<string, UnlockableModData> kv)
                {
                    key = kv.Key;
                    data = kv.Value;
                }
                else if (item is UnlockableModData direct)
                {
                    data = direct;
                    key = Traverse.Create(data).Field("id")?.GetValue() as string;
                }

                if (data != null && !string.IsNullOrEmpty(key)) yield return (key, data);
            }
        }

        public static void EnsureMinimumHeroes()
        {
            Action grantAction = null;
            grantAction = () =>
            {
                var game = cachedHolder?.Game;
                if (game == null)
                {
                    Debug.LogWarning("AP: Game is null, re-queuing starter heroes");
                    pendingActions.Enqueue(grantAction);
                    return;
                }

                UnlockWithAP(() =>
                {
                    foreach (var id in new[] { "Boyle_CH", "Hans_CH" })
                        Debug.Log($"AP: Starter hero '{id}' -> newly unlocked: {game.TryUnlockCharacter(id)}");
                });
            };

            if (cachedHolder?.Game != null) grantAction();
            else pendingActions.Enqueue(grantAction);
        }

        public static void ForceCacheUnlockableData()
        {
            var unlockManager = Resources.FindObjectsOfTypeAll<UnlockablesManager>().FirstOrDefault();
            if (unlockManager == null) return;
            allUnlockableData.Clear();
            foreach (var (key, data) in EnumerateUnlockables(unlockManager))
                allUnlockableData[key] = data;
            Debug.Log($"AP: Force-cached {allUnlockableData.Count} unlockable entries");
        }

        private static void ParseInt(string json, string key, ref int value)
        {
            int idx = json.IndexOf(key);
            if (idx == -1) return;
            int colon = json.IndexOf(':', idx);
            if (colon == -1) return;
            int numStart = colon + 1;
            while (numStart < json.Length && !char.IsDigit(json[numStart])) numStart++;
            int numEnd = numStart;
            while (numEnd < json.Length && char.IsDigit(json[numEnd])) numEnd++;
            if (numEnd > numStart) int.TryParse(json.Substring(numStart, numEnd - numStart), out value);
        }

        private static void ParseBool(string json, string key, ref bool value)
        {
            int idx = json.IndexOf(key);
            if (idx == -1) return;
            int colon = json.IndexOf(':', idx);
            if (colon == -1) return;
            int start = colon + 1;
            while (start < json.Length && (json[start] == ' ' || json[start] == '"')) start++;
            if (start + 3 < json.Length && json.Substring(start, 4).ToLower() == "true") value = true;
            else if (start + 4 < json.Length && json.Substring(start, 5).ToLower() == "false") value = false;
        }

        public static void FlushPendingChecks()
        {
            while (pendingChecks.Count > 0) SendCheck(pendingChecks.Dequeue());
        }
        
        public static void ResyncAllChecks()
        {
            if (apClient == null) return;
            var ids = new List<long>();
            foreach (var loc in sentChecks)
                if (locationIDs.TryGetValue(loc, out long id))
                    ids.Add(id);
            apClient.SendChecks(ids);
            Debug.Log($"AP: Resynced {ids.Count} checks to server");
        }

        public static void SendCheck(string locationName)
        {
            if (apClient == null)
            {
                pendingChecks.Enqueue(locationName);
                Debug.Log("AP: Queued check " + locationName);
                return;
            }

            if (!sentChecks.Contains(locationName))
            {
                sentChecks.Add(locationName);
                SaveSentChecks();
            }
            Debug.Log("CHECK: " + locationName);
            apClient.SendCheck(locationName);   // шлём всегда — сервер сам отсеет дубли
            SaveCounters();
            if (goalType == 0 && (locationName == "The End?" || locationName == "I'll Make You Regret This"))
                apClient?.SendGoal();
            if (goalType == 1 && pearlCheckNames.Contains(locationName) && CountPearls() >= pearlCount)
                apClient?.SendGoal();
            if (goalType == 2 && locationName == "Brutal Orchestra")
                apClient?.SendGoal();
        }

        static void UnlockHeroByAPName(string itemName)
        {
            if (!heroItemToCharacterID.TryGetValue(itemName, out string charID)) return;

            Action unlockAction = null;
            unlockAction = () =>
            {
                var game = cachedHolder?.Game;
                if (game == null)
                {
                    Debug.LogWarning($"AP: Game is null, re-queuing hero unlock for '{itemName}' ({charID})");
                    pendingActions.Enqueue(unlockAction);
                    return;
                }

                UnlockWithAP(() =>
                {
                    bool newly = game.TryUnlockCharacter(charID);
                    Debug.Log($"AP: TryUnlockCharacter('{charID}') for '{itemName}' -> newly unlocked: {newly}");
                });
            };

            if (cachedHolder?.Game != null) unlockAction();
            else pendingActions.Enqueue(unlockAction);
        }

        static void AddCoins(int amount)
        {
            if (cachedHolder?.Run?.playerData == null)
            {
                Debug.Log($"AP: Holder not ready, queuing {amount} coins");
                pendingCoins.Enqueue(amount);
                return;
            }

            cachedHolder.Run.playerData.AddCurrency(amount);
            Debug.Log($"AP: Added {amount} coins");
        }

        public static void ApplyPendingCoins()
        {
            while (pendingCoins.Count > 0 && cachedHolder?.Run?.playerData != null)
            {
                int amount = pendingCoins.Dequeue();
                cachedHolder.Run.playerData.AddCurrency(amount);
                Debug.Log($"AP: Applied queued {amount} coins");
            }
        }

        private static void ApplyItem(string itemName)
        {
            Debug.Log($"AP: Applying item: {itemName}");

            if (heroItemToCharacterID.ContainsKey(itemName))
            {
                UnlockHeroByAPName(itemName);
                return;
            }

            if (itemName == "5 Coins" || itemName == "10 Coins" || itemName == "15 Coins")
            {
                AddCoins(itemName == "5 Coins" ? 5 : itemName == "10 Coins" ? 10 : 15);
                return;
            }

            if (itemName == "Hardmode Access")
            {
                try
                {
                    var thresholdsProp = typeof(SaveDataManager_2024).GetProperty("Thresholds",
                        System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static);
                    var thresholds = thresholdsProp?.GetValue(null);
                    thresholds?.GetType().GetField("hardModeUnlocked",
                            System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance)
                        ?.SetValue(thresholds, true);

                    typeof(SaveDataManager_2024).GetMethod("Update_GameOptions",
                            System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static)
                        ?.Invoke(null, null);

                    Debug.Log("AP: Hardmode unlocked and activated. Start a new run to enter the Garden.");
                }
                catch (Exception e)
                {
                    Debug.LogError("AP: Failed to activate hardmode: " + e);
                }

                return;
            }

            if (itemName == "Orpheum Access" || itemName == "Quarry Access" || itemName == "Garden Access" ||
                itemName == "Far Shore Access" || itemName.StartsWith("Boss ")) return;

            if (itemNameToUnlockableID.TryGetValue(itemName, out string unlockID))
            {
                Action unlockAction = null;
                unlockAction = () =>
                {
                    var game = cachedHolder?.Game;
                    if (game == null)
                    {
                        Debug.LogWarning($"AP: Game is null, re-queuing unlock for '{itemName}' ({unlockID})");
                        pendingActions.Enqueue(unlockAction);
                        return;
                    }

                    UnlockWithAP(() =>
                    {
                        bool wasNewlyUnlocked = game.TryUnlockItem(unlockID);
                        Debug.Log(
                            $"AP: TryUnlockItem('{unlockID}') for '{itemName}' -> newly unlocked: {wasNewlyUnlocked}");
                    });
                };

                if (cachedHolder?.Game != null) unlockAction();
                else pendingActions.Enqueue(unlockAction);
            }
            else
            {
                Debug.LogWarning($"AP: No unlockable ID mapping found for item '{itemName}'");
            }
        }

        public static void OnItemReceived(string itemName, bool applyTransient)
        {
            if (itemName == "Progressive Start Money")
            {
                startMoneyLevel++;
                Debug.Log($"AP: Start Money level = {startMoneyLevel}");
                return;
            }

            if (itemName.EndsWith(" Progressive Ability"))
            {
                abilityLevels.TryGetValue(itemName, out int lvl);
                abilityLevels[itemName] = lvl + 1;
                SaveAbilityLevels();
                Debug.Log($"AP: {itemName} -> level {lvl + 1}");
                return;
            }

            if (itemName == "Quarry Boss Defeat" || itemName == "Garden Boss Defeat") return;

            if (transientItems.Contains(itemName))
            {
                if (!applyTransient) return;
                try
                {
                    ApplyItem(itemName);
                }
                catch (Exception e)
                {
                    Debug.LogError($"AP: ApplyItem({itemName}) failed: {e}");
                }

                return;
            }

            try
            {
                ApplyItem(itemName);
            }
            catch (Exception e)
            {
                Debug.LogError($"AP: ApplyItem({itemName}) failed: {e}");
            }

            if (!receivedAPItems.Contains(itemName))
            {
                receivedAPItems.Add(itemName);
                SaveReceivedItems();
            }


        }

        private float reconnectTimer = 0f;
        void Update()
        {
            if (pendingDeath && cachedHolder != null)
                TryProcessPendingDeath();

            if (apClient != null && apClient.needReconnect && !APClient.userDisconnected)
            {
                reconnectTimer += Time.deltaTime;
                if (reconnectTimer >= 4f)
                {
                    reconnectTimer = 0f;
                    Debug.Log("AP: Attempting reconnect...");
                    AddChatLine("*** Reconnecting... ***");
                    apClient.isAutoReconnect = true;   // это авто-реконнект
                    apClient.Connect();
                }
            }
            else reconnectTimer = 0f;
        }

        public static void DumpWearableObject(string objectName)
        {
            var allObjects = Resources.FindObjectsOfTypeAll<UnityEngine.Object>();
            bool found = false;
            foreach (var obj in allObjects)
            {
                if (obj == null || obj.name != objectName) continue;
                found = true;
                var type = obj.GetType();
                Debug.Log($"=== FOUND '{objectName}' ===");
                Debug.Log($"Real C# Type: {type.FullName} (Assembly: {type.Assembly.GetName().Name})");
                foreach (var f in type.GetFields(System.Reflection.BindingFlags.Public |
                                                 System.Reflection.BindingFlags.NonPublic |
                                                 System.Reflection.BindingFlags.Instance))
                {
                    object val;
                    try
                    {
                        val = f.GetValue(obj);
                    }
                    catch (Exception e)
                    {
                        val = $"<error: {e.Message}>";
                    }

                    Debug.Log($"  Field: {f.FieldType.Name} {f.Name} = {val}");
                }
            }

            if (!found)
                Debug.LogWarning(
                    $"AP: DumpWearableObject: '{objectName}' NOT found among {allObjects.Length} loaded objects.");
        }

        public static void ProcessPendingActions()
        {
            while (pendingActions.Count > 0 && cachedHolder != null)
            {
                Debug.Log("AP: Executing queued action");
                pendingActions.Dequeue()?.Invoke();
            }
        }

        // ===== DeathLink =====
        private static bool pendingDeath = false;

        public static void QueueIncomingDeath() => pendingDeath = true;

        public static bool TryProcessPendingDeath()
        {
            if (!pendingDeath) return false;

            try
            {
                if (InCombat)
                {
                    var cm = CombatManager.Instance;
                    var chars = cm?._stats?.CharactersOnField;
                    if (chars == null)
                    {
                        Debug.LogWarning("AP: DeathLink in combat but stats not ready, will retry");
                        return false;
                    }

                    suppressDeathLink = true;
                    int killed = 0;
                    foreach (var kv in chars)
                    {
                        if (kv.Value != null && kv.Value.IsAlive)
                        {
                            int hp;
                            kv.Value.DirectDeath(null, false, out hp);
                            killed++;
                        }
                    }

                    Debug.Log($"AP: DeathLink — DirectDeath for {killed} characters");
                    pendingDeath = false;
                    return true;
                }

                var pd = cachedHolder?.Run?.playerData;
                if (pd == null)
                {
                    Debug.LogWarning("AP: DeathLink pending but playerData is null, will retry");
                    return false;
                }

                suppressDeathLink = true; // сбросится в DeathLinkSendPatch после нашего вайпа

                int count = 0;
                foreach (var info in pd.CharacterListData)
                {
                    if (info is CharacterInGameData ch && ch.IsInParty && ch.CurrentHealth > 0)
                    {
                        ch.SetCurrentHealth(0);
                        count++;
                    }
                }

                Debug.Log($"AP: DeathLink — set HP to 0 for {count} party members");
            }
            catch (Exception e)
            {
                Debug.LogError("AP: DeathLink processing failed: " + e);
                suppressDeathLink = false;
            }

            pendingDeath = false;
            return true;
        }
        
        public static void SyncCheckedFromServer(List<long> checkedIds)
        {
            if (checkedIds == null || checkedIds.Count == 0) return;
            var idToName = new Dictionary<long, string>();
            foreach (var kv in locationIDs) idToName[kv.Value] = kv.Key;

            int added = 0;
            foreach (var id in checkedIds)
                if (idToName.TryGetValue(id, out string name) && sentChecks.Add(name))
                    added++;

            if (added > 0) SaveSentChecks();
            Debug.Log($"AP: Synced {checkedIds.Count} checked from server ({added} new into local)");
        }

        // ==========Хелперы==========

        static List<int> GetOrCreatePick(CharacterCombat inst, string item, bool hasSlap, int rankCount)
        {
            string entity = inst.EntityID;
            if (runAbilityPick.TryGetValue(entity, out var existing))
            {
                abilityLevels.TryGetValue(item, out int lvlNow);
                int want = Mathf.Min(lvlNow + 1, (hasSlap ? 1 : 0) + rankCount);
                if (existing.Count >= want) return existing;
                // дополняем недостающие слоты из оставшегося пула (тем же rng-порядком)
                var full = new List<int>();
                if (hasSlap) full.Add(0);
                for (int r = 0; r < rankCount; r++) full.Add(100 + r);
                int seed2 = ((APClient.CurrentSeed ?? "0").GetHashCode() ^ entity.GetHashCode()) + runCounter * 7919;
                var rng2 = new System.Random(seed2);
                for (int i = full.Count - 1; i > 0; i--) { int j = rng2.Next(i + 1); var t = full[i]; full[i] = full[j]; full[j] = t; }
                if (entity == "Nowak" && full.Count > 1 && full[0] == 102)
                {
                    for (int k = 1; k < full.Count; k++)
                        if (full[k] != 102) { var t2 = full[0]; full[0] = full[k]; full[k] = t2; break; }
                }
                existing = full.Take(want).ToList();
                runAbilityPick[entity] = existing;
                SaveRunAbilityPick();
                return existing;
            }

            var pool = new List<int>();
            if (hasSlap) pool.Add(0);
            for (int r = 0; r < rankCount; r++) pool.Add(100 + r);

            abilityLevels.TryGetValue(item, out int level);
            int slots = Mathf.Min(level + 1, pool.Count);

            int seed = ((APClient.CurrentSeed ?? "0").GetHashCode() ^ entity.GetHashCode()) + runCounter * 7919;
            var rng = new System.Random(seed);
            for (int i = pool.Count - 1; i > 0; i--)
            {
                int j = rng.Next(i + 1);
                var t = pool[i]; pool[i] = pool[j]; pool[j] = t;
            }
            // Nowak: фокус (rank-абилка №2) не может быть первой
            if (entity == "Nowak" && pool.Count > 1 && pool[0] == 102)
            {
                for (int k = 1; k < pool.Count; k++)
                    if (pool[k] != 102) { var t = pool[0]; pool[0] = pool[k]; pool[k] = t; break; }
            }
            var pick = pool.Take(slots).ToList();

            runAbilityPick[entity] = pick;
            SaveRunAbilityPick();
            return pick;
        }

        static CharacterAbility GetRankAbility(CharacterCombat inst, int r)
        {
            var ch = inst.Character;
            if (ch == null || !ch.HasRankedData) return null;
            int rank = inst.ClampedRank;
            if (rank < 0 || rank >= ch.rankedData.Count) return null;      // Count
            var arr = ch.rankedData[rank].rankAbilities;
            if (arr == null || r >= arr.Length) return null;
            return arr[r];
        }

        static int GetRankAbilityCount(CharacterCombat inst)
        {
            var ch = inst.Character;
            if (ch == null || !ch.HasRankedData) return 0;
            int rank = inst.ClampedRank;
            if (rank < 0 || rank >= ch.rankedData.Count) return 0;          // Count
            return ch.rankedData[rank].rankAbilities?.Length ?? 0;
        }
        
        public static bool IsGardenZone()
        {
            var run = cachedHolder?.Run;
            if (run == null || run.CurrentZoneID != 2) return false;
            return (run.CurrentZoneDB?.ZoneName ?? "") != "Pit";
        }
        
        static bool IsBossFight(RunDataSO run)
        {
            try
            {
                var card = run.zoneData[run.CurrentZoneID].GetCard(run.CurrentCardID);
                return card != null && card.CardType == CardType.Boss;
            }
            catch { return false; }
        }
    }
}
