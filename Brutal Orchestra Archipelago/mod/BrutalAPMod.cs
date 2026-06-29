using BepInEx;
using HarmonyLib;
using UnityEngine;
using System.Collections.Generic;
using System;
using System.Linq;

namespace BrutalOrchestraAr
{
    [BepInPlugin("brutal.ap.mod", "Brutal AP Mod", "1.0.0")]
    public class BrutalAPMod : BaseUnityPlugin
    {
        public static APClient apClient;
        public static HashSet<string> receivedAPItems = new HashSet<string>();
        public static HashSet<string> sentChecks = new HashSet<string>();

        // ---- Counters ----
        public static int farBattleCount = 0;
        public static int orpBattleCount = 0;
        public static int farMoneyChestCount = 0;
        public static int orpMoneyChestCount = 0;
        public static int farArtifactChestCount = 0;
        public static int orpArtifactChestCount = 0;
        public static int farShopCount = 0;
        public static int orpShopCount = 0;
        public static int buyHeroCount = 0;

        // ---- Limits (set from slot_data) ----
        public static int farBattleLimit = 15;
        public static int orpBattleLimit = 15;
        public static int farMoneyLimit = 2;
        public static int orpMoneyLimit = 2;
        public static int farArtifactLimit = 2;
        public static int orpArtifactLimit = 2;
        public static int shopCountFar = 6;
        public static int shopCountOrp = 6;
        public static int bossCount = 3;

        public static bool AllowUnlocks = false;

        public static void UnlockWithAP(Action unlockAction)
        {
            AllowUnlocks = true;
            try { unlockAction(); }
            finally { AllowUnlocks = false; }
        }

        public static GameInformationHolder cachedHolder = null;
        public static Queue<Action> pendingActions = new Queue<Action>();
        private static Queue<int> pendingCoins = new Queue<int>();

        private static readonly HashSet<string> transientItems = new HashSet<string>
        {
            "5 Coins", "10 Coins", "15 Coins"
        };

        private static Dictionary<string, UnlockableModData> allUnlockableData = new Dictionary<string, UnlockableModData>();

        public static Dictionary<string, long> locationIDs = new Dictionary<string, long>();

        public static Dictionary<long, string> itemIdToName = new Dictionary<long, string>
        {
            {10000, "Orpheum Access"}, {10001, "Sepulchre Access"},
            {10002, "Boss 1"}, {10003, "Boss 2"}, {10004, "Boss 3"},
            {10005, "Anemone Thread"}, {10006, "Beads of Something or Other"},
            {10007, "Blood Thirsty Idol"}, {10008, "Boonario"}, {10009, "Bosch's Fist"},
            {10010, "Clash of the Bleached"}, {10011, "Crown of Thorns"},
            {10012, "Dew-Covered Sticker"}, {10013, "Diseased Bodypart"},
            {10014, "Dried Paintbrush"}, {10015, "Eel's Skin"},
            {10016, "Fennel's Blessing"}, {10017, "Flesh-Piercing Skewer"},
            {10018, "Gilded Rage"}, {10019, "Golden Gun"}, {10020, "Heaven-Bound Noose"},
            {10021, "Idle Hands"}, {10022, "Inhalant"}, {10023, "Jumper Cables"},
            {10024, "Lucky Charm"}, {10025, "Mangled Messiah"}, {10026, "Marrow's Reach"},
            {10027, "Mirror Shard"}, {10028, "Mithridatism"}, {10029, "Mung Moon"},
            {10030, "Mutually Assured Destruction"}, {10031, "Myopia Glasses"},
            {10032, "Padded Gloves"}, {10033, "Pile of Dirt"}, {10034, "Pox Blanket"},
            {10035, "Prayer Beads"}, {10036, "Rabbit's Foot"}, {10037, "Roid Rage"},
            {10038, "Scab-Knuckled Gloves"}, {10039, "Sealed Envelope"},
            {10040, "Shattered Amulet"}, {10041, "Silk Tourniquet"},
            {10042, "Soul Contract"}, {10043, "Stimpak"}, {10044, "Strange Beads"},
            {10045, "The Unfinished Bolt"}, {10046, "Thick Gloves"},
            {10047, "Thorned Vines"}, {10048, "Tumor"},
            {10049, "Black Paint"}, {10050, "Blue Paint"}, {10051, "Brown Paint"},
            {10052, "Cyan Paint"}, {10053, "Gray Paint"}, {10054, "Green Paint"},
            {10055, "Lime Paint"}, {10056, "Magenta Paint"}, {10057, "Orange Paint"},
            {10058, "Pink Paint"}, {10059, "Purple Paint"}, {10060, "Red Paint"},
            {10061, "Teal Paint"}, {10062, "White Paint"}, {10063, "Yellow Paint"},
            {10064, "5 Coins"}, {10065, "10 Coins"}, {10066, "15 Coins"},
        };

        public static readonly Dictionary<string, string> heroUnlockID = new Dictionary<string, string>
        {
            {"Hero_Boyle", "Boyle"}, {"Hero_Hans", "Hans"}, {"Hero_Anton", "Anton"},
            {"Hero_Splig", "Splig"}, {"Hero_Pearl", "Pearl"}, {"Hero_Thype", "Thype"},
            {"Hero_Griffin", "Griffin"}, {"Hero_Arnold", "Arnold"}, {"Hero_Dimitri", "Dimitri"},
            {"Hero_LongLiver", "LongLiver"}, {"Hero_Clive", "Clive"}, {"Hero_Kleiver", "Kleiver"},
            {"Hero_Cranes", "Cranes"}, {"Hero_Agon", "Agon"}, {"Hero_Rags", "Rags"},
            {"Hero_SmokeStacks", "SmokeStacks"}, {"Hero_Leviat", "Leviat"}, {"Hero_Gospel", "Gospel"},
            {"Hero_Bimini", "Bimini"}, {"Hero_Burnout", "Burnout"}, {"Hero_Fennec", "Fennec"},
            {"Hero_Mordrake", "MordrakeCH"}, {"Hero_Mung", "MungCH"}, {"Hero_ShellyK", "ShellyK"},
            {"Hero_Formosus", "Formosus"}
        };

        private static readonly Dictionary<string, Achievement> heroAchievementMap = new Dictionary<string, Achievement>
        {
            {"Boyle", Achievement.ACH_PartyMember_Boyle},
            {"Hans", Achievement.ACH_PartyMember_Hans},
            {"Anton", Achievement.ACH_PartyMember_Anton},
            {"Splig", Achievement.ACH_PartyMember_Splig},
            {"Pearl", Achievement.ACH_PartyMember_Pearl},
            {"Thype", Achievement.ACH_PartyMember_Thype},
            {"Griffin", Achievement.ACH_PartyMember_Griffin},
            {"Arnold", Achievement.ACH_PartyMember_Arnold},
            {"Dimitri", Achievement.ACH_PartyMember_Dimitri},
            {"LongLiver", Achievement.ACH_PartyMember_LongLiver},
            {"Clive", Achievement.ACH_PartyMember_Clive},
            {"Kleiver", Achievement.ACH_PartyMember_Kleiver},
            {"Cranes", Achievement.ACH_PartyMember_Cranes},
            {"Agon", Achievement.ACH_PartyMember_Agon},
            {"Rags", Achievement.ACH_PartyMember_Rags},
            {"SmokeStacks", Achievement.ACH_PartyMember_SmokeStacks},
            {"Leviat", Achievement.ACH_PartyMember_Leviat},
            {"Gospel", Achievement.ACH_PartyMember_Gospel},
            {"Bimini", Achievement.ACH_PartyMember_Bimini},
            {"Burnout", Achievement.ACH_PartyMember_Burnout},
            {"Fennec", Achievement.ACH_PartyMember_Fennec},
            {"Mordrake", Achievement.ACH_PartyMember_Mordrake},
            {"Mung", Achievement.ACH_PartyMember_Mung},
            {"ShellyK", Achievement.ACH_PartyMember_ShellyK},
            {"Formosus", Achievement.ACH_PartyMember_Formosus}
        };

        // ---- GUI ----
        private bool showGui = true;
        private string guiServer = "ws://localhost:38281";
        private string guiSlot = "Test1";
        private Rect windowRect = new Rect(20, 20, 300, 130);

        void Start()
        {
            SkipTutorial();
            var harmony = new Harmony("brutal.ap.mod");
            harmony.PatchAll();
            showGui = true;
            Debug.Log("MOD LOADED – waiting for user to connect...");
        }

        void OnGUI()
        {
            if (!showGui) return;
            windowRect = GUILayout.Window(123456, windowRect, DrawConnectWindow, "Archipelago Connect");
        }

        void DrawConnectWindow(int id)
        {
            GUILayout.Label("Server URL (ws://...):");
            guiServer = GUILayout.TextField(guiServer);
            GUILayout.Label("Slot Name:");
            guiSlot = GUILayout.TextField(guiSlot);
            if (GUILayout.Button("Connect") && !string.IsNullOrEmpty(guiServer) && !string.IsNullOrEmpty(guiSlot))
            {
                apClient = new APClient(guiServer, guiSlot);
                apClient.Connect();
                showGui = false;
                Debug.Log("AP: Connecting to " + guiServer + " as " + guiSlot);
            }
        }

        private static string GetPlayerPrefsKey()
        {
            if (!string.IsNullOrEmpty(APClient.CurrentSeed))
                return "BrutalAP_" + APClient.CurrentSeed;
            return "BrutalAP_ReceivedItems";
        }

        public static void LoadReceivedItemsForCurrentSeed()
        {
            string key = GetPlayerPrefsKey();
            if (PlayerPrefs.HasKey(key))
            {
                string data = PlayerPrefs.GetString(key, "");
                if (!string.IsNullOrEmpty(data))
                {
                    string[] items = data.Split(';');
                    foreach (string item in items)
                    {
                        if (!string.IsNullOrEmpty(item) &&
                            item != "Hero Unlock" && item != "Item Unlock" &&
                            item != "Orpheum Access" && item != "Sepulchre Access")
                            receivedAPItems.Add(item);
                    }
                    Debug.Log("AP: Loaded received items for seed " + APClient.CurrentSeed + ": " + data);
                }
            }
        }

        public static void SkipTutorial()
        {
            try
            {
                var thresholds = Traverse.Create(typeof(SaveDataManager_2024)).Type("Thresholds");
                if (thresholds != null)
                {
                    thresholds.Field("firstTutorialState").SetValue(true);
                    thresholds.Field("NeedsToBeSaved").SetValue(true);
                    Debug.Log("AP: Tutorial skipped (set firstTutorialState and NeedsToBeSaved)");
                }
            }
            catch (Exception e) { Debug.LogError("SkipTutorial failed: " + e); }
        }

        public static void InitSlotData(string slotDataJson)
        {
            try
            {
                int farBat = 24, orpBat = 24;
                int farMon = 2, orpMon = 2;
                int farArt = 2, orpArt = 2;
                int shopFar = 24, shopOrp = 24;
                int bosses = 3;

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
                    ParseInt(slotDataJson, "\"boss_count\"", ref bosses);
                }

                farBattleLimit = farBat; orpBattleLimit = orpBat;
                farMoneyLimit = farMon; orpMoneyLimit = orpMon;
                farArtifactLimit = farArt; orpArtifactLimit = orpArt;
                shopCountFar = shopFar; shopCountOrp = shopOrp;
                bossCount = bosses;

                locationIDs.Clear();

                // Базовые (100-106)
                int baseId = 100;
                locationIDs["Far Shore Access"] = baseId++;
                locationIDs["Orpheum Access"] = baseId++;
                locationIDs["Sepulchre Access"] = baseId++;
                for (int i = 1; i <= 4; i++)
                    locationIDs[$"BuyHero_{i}"] = baseId++;

                // Битвы: Far 200+, Orp 300+
                int farBattleId = 200;
                for (int i = 1; i <= farBattleLimit; i++)
                    locationIDs[$"Far_Battle_{i}"] = farBattleId++;
                int orpBattleId = 300;
                for (int i = 1; i <= orpBattleLimit; i++)
                    locationIDs[$"Orp_Battle_{i}"] = orpBattleId++;

                // Денежные сундуки: Far 400+, Orp 500+
                int farMoneyId = 400;
                for (int i = 1; i <= farMoneyLimit; i++)
                    locationIDs[$"Far_MoneyChest_{i}"] = farMoneyId++;
                int orpMoneyId = 500;
                for (int i = 1; i <= orpMoneyLimit; i++)
                    locationIDs[$"Orp_MoneyChest_{i}"] = orpMoneyId++;

                // Артефактные сундуки: Far 600+, Orp 700+
                int farArtifactId = 600;
                for (int i = 1; i <= farArtifactLimit; i++)
                    locationIDs[$"Far_ArtifactChest_{i}"] = farArtifactId++;
                int orpArtifactId = 700;
                for (int i = 1; i <= orpArtifactLimit; i++)
                    locationIDs[$"Orp_ArtifactChest_{i}"] = orpArtifactId++;

                // Боссы: 800+
                int bossId = 800;
                if (bossCount >= 1) locationIDs["Far Boss"] = bossId++;
                if (bossCount >= 2) locationIDs["Orp Boss"] = bossId++;
                if (bossCount >= 3) locationIDs["Sepulchre Boss"] = bossId++;

                // Магазины: Far 900+, Orp 1000+
                int farShopId = 900;
                for (int i = 1; i <= shopCountFar; i++)
                    locationIDs[$"Shop_Far_{i}"] = farShopId++;
                int orpShopId = 1000;
                for (int i = 1; i <= shopCountOrp; i++)
                    locationIDs[$"Shop_Orp_{i}"] = orpShopId++;

                // Герои (все 25): 1100+
                int heroId = 1100;
                string[] heroNames = {"Boyle","Hans","Anton","Splig","Pearl","Thype","Griffin","Arnold",
                                      "Dimitri","LongLiver","Clive","Kleiver","Cranes","Agon","Rags",
                                      "SmokeStacks","Leviat","Gospel","Bimini","Burnout","Fennec",
                                      "Mordrake","Mung","ShellyK","Formosus"};
                foreach (var name in heroNames)
                    locationIDs[$"Hero_{name}"] = heroId++;

                // Предметы (19): 1200+
                int itemId = 1200;
                string[] itemUnlockIDs = {
                    "Ending_CorpseKill","Ending_CorpseSave","ShopDepleted","FoolsDepleted",
                    "HeavenDoubleSacrifice","RoidsMissTurn","OrroSmooch","SurviveStarvation",
                    "VHSTask0","VHSTask1","VHSTask2","VHSTask3","VHSTask4","VHSTask5","VHSTask6",
                    "HundredPercent","UngodEmissary","AntonSad","ProdigalFlee"
                };
                foreach (var uid in itemUnlockIDs)
                    locationIDs[$"Item_{uid}"] = itemId++;

                Debug.Log($"AP: Initialized location IDs. Far Battles: {farBat}, Orp Battles: {orpBat}, Far Money: {farMon}, Orp Money: {orpMon}, Far Artifact: {farArt}, Orp Artifact: {orpArt}, Far Shops: {shopFar}, Orp Shops: {shopOrp}, Bosses: {bosses}");

                EnsureMinimumHeroes();
                LoadCountersForCurrentSeed();
                LoadReceivedItemsForCurrentSeed();
                InitStartingChecks();
            }
            catch (Exception e) { Debug.LogError("InitSlotData error: " + e); }
        }

        public static void EnsureMinimumHeroes()
        {
            var unlockManager = Resources.FindObjectsOfTypeAll<UnlockablesManager>().FirstOrDefault();
            if (unlockManager == null) return;
            var dbField = typeof(UnlockablesManager).GetField("_newUnlockableDB",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            var unlockableDB = dbField?.GetValue(unlockManager) as UnlockablesDatabase;
            if (unlockableDB == null) return;
            var byIDField = typeof(UnlockablesDatabase).GetField("m_ByIDUnlockables",
                System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);
            if (byIDField == null) return;
            var dictObj = byIDField.GetValue(unlockableDB);
            var enumerable = dictObj as System.Collections.IEnumerable;
            if (enumerable == null)
                enumerable = Traverse.Create(dictObj).Property("Values")?.GetValue() as System.Collections.IEnumerable;
            if (enumerable == null) return;

            var characterIDs = new List<string>();
            foreach (var item in enumerable)
            {
                string key = null;
                UnlockableModData data = null;
                if (item is KeyValuePair<string, UnlockableModData> kv) { key = kv.Key; data = kv.Value; }
                else if (item is UnlockableModData direct) { data = direct; key = Traverse.Create(data).Field("id")?.GetValue() as string; }
                if (data != null && data.hasCharacterUnlock && !string.IsNullOrEmpty(key))
                    characterIDs.Add(key);
            }

            int granted = 0;
            foreach (var id in characterIDs)
            {
                if (granted >= 2) break;
                UnlockWithAP(() => unlockManager.TryUnlockFromID(id));
                granted++;
                Debug.Log($"AP: Granted starter hero ID {id}");
            }
        }

        public static void CacheUnlockableData()
        {
            if (cachedHolder?.UnlockableManager == null) return;
            var dbField = typeof(UnlockablesManager).GetField("_newUnlockableDB",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            var unlockableDB = dbField?.GetValue(cachedHolder.UnlockableManager) as UnlockablesDatabase;
            if (unlockableDB == null) return;
            var byIDField = typeof(UnlockablesDatabase).GetField("m_ByIDUnlockables",
                System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);
            if (byIDField == null) return;
            var dictObj = byIDField.GetValue(unlockableDB);
            var enumerable = dictObj as System.Collections.IEnumerable;
            if (enumerable == null)
                enumerable = Traverse.Create(dictObj).Property("Values")?.GetValue() as System.Collections.IEnumerable;
            if (enumerable == null) return;

            allUnlockableData.Clear();
            foreach (var item in enumerable)
            {
                string key = null;
                UnlockableModData data = null;
                if (item is KeyValuePair<string, UnlockableModData> kv) { key = kv.Key; data = kv.Value; }
                else if (item is UnlockableModData direct) { data = direct; key = Traverse.Create(data).Field("id")?.GetValue() as string; }
                if (data != null && !string.IsNullOrEmpty(key))
                    allUnlockableData[key] = data;
            }
        }

        static bool IsCharacterAlreadyUnlocked(string id)
        {
            if (cachedHolder?.Game == null) return false;
            if (allUnlockableData.TryGetValue(id, out var data) && data.hasCharacterUnlock && data.character != null)
                return cachedHolder.Game.IsCharacterUnlocked(data.character);
            return false;
        }

        public static void LoadCountersForCurrentSeed()
        {
            string key = "BrutalAP_Counters_" + (APClient.CurrentSeed ?? "default");
            if (PlayerPrefs.HasKey(key))
            {
                string data = PlayerPrefs.GetString(key, "");
                if (!string.IsNullOrEmpty(data))
                {
                    foreach (var pair in data.Split(';'))
                    {
                        var parts = pair.Split('=');
                        if (parts.Length != 2) continue;
                        int val;
                        if (!int.TryParse(parts[1], out val)) continue;
                        switch (parts[0])
                        {
                            case "farBattle": farBattleCount = val; break;
                            case "orpBattle": orpBattleCount = val; break;
                            case "farMoney": farMoneyChestCount = val; break;
                            case "orpMoney": orpMoneyChestCount = val; break;
                            case "farArtifact": farArtifactChestCount = val; break;
                            case "orpArtifact": orpArtifactChestCount = val; break;
                            case "farShop": farShopCount = val; break;
                            case "orpShop": orpShopCount = val; break;
                            case "buyHero": buyHeroCount = val; break;
                        }
                    }
                    Debug.Log("AP: Loaded counters for seed " + APClient.CurrentSeed);
                }
            }
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
            if (numEnd > numStart)
                int.TryParse(json.Substring(numStart, numEnd - numStart), out value);
        }

        static void SaveReceivedItems()
        {
            var itemsToSave = receivedAPItems.Where(item =>
                !transientItems.Contains(item) &&
                item != "Hero Unlock" && item != "Item Unlock" &&
                item != "Orpheum Access" && item != "Sepulchre Access"
            ).ToArray();
            string data = string.Join(";", itemsToSave);
            PlayerPrefs.SetString(GetPlayerPrefsKey(), data);
            PlayerPrefs.Save();
            Debug.Log("AP: Saved received items: " + data);
        }

        public static void SaveCounters()
        {
            var counters = new Dictionary<string, int>
            {
                {"farBattle", farBattleCount}, {"orpBattle", orpBattleCount},
                {"farMoney", farMoneyChestCount}, {"orpMoney", orpMoneyChestCount},
                {"farArtifact", farArtifactChestCount}, {"orpArtifact", orpArtifactChestCount},
                {"farShop", farShopCount}, {"orpShop", orpShopCount},
                {"buyHero", buyHeroCount}
            };
            string data = string.Join(";", counters.Select(kv => kv.Key + "=" + kv.Value));
            string key = "BrutalAP_Counters_" + (APClient.CurrentSeed ?? "default");
            PlayerPrefs.SetString(key, data);
            PlayerPrefs.Save();
            Debug.Log("AP: Saved counters: " + data);
        }

        public static void SendCheck(string locationName)
        {
            if (apClient == null) return;
            if (sentChecks.Contains(locationName)) return;
            sentChecks.Add(locationName);
            Debug.Log("CHECK: " + locationName);
            apClient.SendCheck(locationName);
            SaveCounters();
        }

        static void UnlockHeroByAPName(string heroAPName)
        {
            if (cachedHolder?.UnlockableManager == null) return;
            if (!heroUnlockID.TryGetValue(heroAPName, out string id)) return;
            UnlockWithAP(() => cachedHolder.UnlockableManager.TryUnlockFromID(id));
            Debug.Log($"AP: Unlocked hero {heroAPName} via ID {id}");
        }

        static void UnlockRandomItem()
        {
            Debug.Log("AP: UnlockRandomItem called");
            if (cachedHolder == null || cachedHolder.UnlockableManager == null) return;
            try
            {
                var dbField = typeof(UnlockablesManager).GetField("_newUnlockableDB",
                    System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                var unlockableDB = dbField?.GetValue(cachedHolder.UnlockableManager) as UnlockablesDatabase;
                if (unlockableDB == null) return;
                var byIDField = typeof(UnlockablesDatabase).GetField("m_ByIDUnlockables",
                    System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);
                if (byIDField == null) return;
                var dictObj = byIDField.GetValue(unlockableDB);
                var enumerable = dictObj as System.Collections.IEnumerable;
                if (enumerable == null)
                    enumerable = Traverse.Create(dictObj).Property("Values")?.GetValue() as System.Collections.IEnumerable;
                if (enumerable == null) return;
                var game = cachedHolder.Game;
                if (game == null) return;
                var locked = new List<KeyValuePair<string, UnlockableModData>>();
                foreach (var item in enumerable)
                {
                    UnlockableModData data = null;
                    string key = null;
                    if (item is KeyValuePair<string, UnlockableModData> kv)
                    {
                        data = kv.Value;
                        key = kv.Key;
                    }
                    else if (item is UnlockableModData direct)
                    {
                        data = direct;
                        key = Traverse.Create(data).Field("id")?.GetValue() as string;
                    }
                    else continue;
                    if (data == null || !data.hasItemUnlock || data.items == null) continue;
                    bool allLocked = true;
                    foreach (var it in data.items)
                        if (it != null && game.IsItemUnlocked(it)) { allLocked = false; break; }
                    if (allLocked && !string.IsNullOrEmpty(key))
                        locked.Add(new KeyValuePair<string, UnlockableModData>(key, data));
                }
                if (locked.Count > 0)
                {
                    var pick = locked[UnityEngine.Random.Range(0, locked.Count)];
                    UnlockWithAP(() => cachedHolder.UnlockableManager.TryUnlockFromID(pick.Key));
                    Debug.Log($"AP: Unlocked items via {pick.Key}");
                }
            }
            catch (Exception e) { Debug.LogError("UnlockRandomItem error: " + e); }
        }

        static void AddCoins(int amount)
        {
            if (cachedHolder == null || cachedHolder.Run == null || cachedHolder.Run.playerData == null)
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
            if (itemName.StartsWith("Hero_"))
                UnlockHeroByAPName(itemName);
            else if (itemName == "5 Coins" || itemName == "10 Coins" || itemName == "15 Coins")
                AddCoins(itemName == "5 Coins" ? 5 : (itemName == "10 Coins" ? 10 : 15));
            else if (itemName == "Orpheum Access" || itemName == "Sepulchre Access" ||
                     itemName == "Far Shore Access" || itemName.StartsWith("Boss "))
            {
                // Доступы и боссы — просто сохраняем
            }
            else
                UnlockRandomItem();
        }

        public static void OnItemReceived(string itemName)
        {
            Debug.Log($"AP: Received item: {itemName}");
            ApplyItem(itemName);

            if (!transientItems.Contains(itemName) &&
                itemName != "Hero Unlock" && itemName != "Item Unlock" &&
                itemName != "Orpheum Access" && itemName != "Sepulchre Access")
            {
                receivedAPItems.Add(itemName);
                SaveReceivedItems();
            }
        }

        public static void InitStartingChecks() { SendCheck("Far Shore Access"); }

        public static void ProcessPendingActions()
        {
            while (pendingActions.Count > 0 && cachedHolder != null)
            {
                var action = pendingActions.Dequeue();
                Debug.Log("AP: Executing queued action");
                action?.Invoke();
            }
        }

        // ========== PATCHES ==========
        [HarmonyPatch(typeof(GameInformationHolder), "PostCombatProcess")]
        class BattlePatch
        {
            static void Postfix(GameInformationHolder __instance)
            {
                if (__instance == null) return;
                if (BrutalAPMod.cachedHolder != __instance)
                {
                    BrutalAPMod.cachedHolder = __instance;
                    Debug.Log("AP: Holder cached");
                    BrutalAPMod.CacheUnlockableData();
                    BrutalAPMod.ApplyPendingCoins();
                    BrutalAPMod.ProcessPendingActions();
                }

                RunDataSO run = __instance.Run;
                if (run != null && run.IsCurrentCardType(CardType.Boss))
                {
                    int zone = run.CurrentZoneID;
                    string bossLoc = zone == 0 ? "Far Boss" : (zone == 1 ? "Orp Boss" : "Sepulchre Boss");
                    if (BrutalAPMod.locationIDs.ContainsKey(bossLoc) && !BrutalAPMod.sentChecks.Contains(bossLoc))
                        BrutalAPMod.SendCheck(bossLoc);
                    return;
                }

                int zoneID = run?.CurrentZoneID ?? -1;
                if (zoneID == 0)
                {
                    if (BrutalAPMod.farBattleCount >= BrutalAPMod.farBattleLimit) return;
                    BrutalAPMod.farBattleCount++;
                    BrutalAPMod.SendCheck("Far_Battle_" + BrutalAPMod.farBattleCount);
                }
                else if (zoneID == 1)
                {
                    if (BrutalAPMod.orpBattleCount >= BrutalAPMod.orpBattleLimit) return;
                    BrutalAPMod.orpBattleCount++;
                    BrutalAPMod.SendCheck("Orp_Battle_" + BrutalAPMod.orpBattleCount);
                }
            }
        }

        [HarmonyPatch(typeof(MoneyChestContentData), "OpenTreasure")]
        class MoneyChestPatch
        {
            static void Postfix()
            {
                Debug.Log("AP: MoneyChestPatch triggered");
                if (BrutalAPMod.cachedHolder == null)
                {
                    BrutalAPMod.cachedHolder = Resources.FindObjectsOfTypeAll<GameInformationHolder>().FirstOrDefault();
                    Debug.Log("AP: cachedHolder was null, tried to find via Resources: " + (BrutalAPMod.cachedHolder != null));
                }
                if (BrutalAPMod.cachedHolder == null || BrutalAPMod.cachedHolder.Run == null)
                {
                    Debug.Log("AP: Holder or Run is null, aborting check");
                    return;
                }

                int zone = BrutalAPMod.cachedHolder.Run.CurrentZoneID;
                Debug.Log($"AP: MoneyChest zone={zone}, count={BrutalAPMod.farMoneyChestCount}/{BrutalAPMod.farMoneyLimit}");
                if (zone == 0)
                {
                    if (BrutalAPMod.farMoneyChestCount >= BrutalAPMod.farMoneyLimit) return;
                    BrutalAPMod.farMoneyChestCount++;
                    BrutalAPMod.SendCheck("Far_MoneyChest_" + BrutalAPMod.farMoneyChestCount);
                }
                else if (zone == 1)
                {
                    if (BrutalAPMod.orpMoneyChestCount >= BrutalAPMod.orpMoneyLimit) return;
                    BrutalAPMod.orpMoneyChestCount++;
                    BrutalAPMod.SendCheck("Orp_MoneyChest_" + BrutalAPMod.orpMoneyChestCount);
                }
                else
                {
                    Debug.Log("AP: MoneyChest zone not 0 or 1, no check sent");
                }
            }
        }

        [HarmonyPatch(typeof(PrizeContentData), "OpenTreasure")]
        class ArtifactChestPatch
        {
            static void Postfix()
            {
                Debug.Log("AP: ArtifactChestPatch triggered");
                if (BrutalAPMod.cachedHolder == null)
                {
                    BrutalAPMod.cachedHolder = Resources.FindObjectsOfTypeAll<GameInformationHolder>().FirstOrDefault();
                    Debug.Log("AP: cachedHolder was null, tried to find via Resources: " + (BrutalAPMod.cachedHolder != null));
                }
                if (BrutalAPMod.cachedHolder == null || BrutalAPMod.cachedHolder.Run == null)
                {
                    Debug.Log("AP: Holder or Run is null, aborting check");
                    return;
                }

                int zone = BrutalAPMod.cachedHolder.Run.CurrentZoneID;
                Debug.Log($"AP: ArtifactChest zone={zone}, count={BrutalAPMod.farArtifactChestCount}/{BrutalAPMod.farArtifactLimit}");
                if (zone == 0)
                {
                    if (BrutalAPMod.farArtifactChestCount >= BrutalAPMod.farArtifactLimit) return;
                    BrutalAPMod.farArtifactChestCount++;
                    BrutalAPMod.SendCheck("Far_ArtifactChest_" + BrutalAPMod.farArtifactChestCount);
                }
                else if (zone == 1)
                {
                    if (BrutalAPMod.orpArtifactChestCount >= BrutalAPMod.orpArtifactLimit) return;
                    BrutalAPMod.orpArtifactChestCount++;
                    BrutalAPMod.SendCheck("Orp_ArtifactChest_" + BrutalAPMod.orpArtifactChestCount);
                }
                else
                {
                    Debug.Log("AP: ArtifactChest zone not 0 or 1, no check sent");
                }
            }
        }

        [HarmonyPatch(typeof(RunDataSO), "BuyFoolCharacter")]
        class BuyFoolPatch
        {
            static void Prefix()
            {
                BrutalAPMod.AllowUnlocks = true;
            }

            static void Postfix(CharacterSO __result)
            {
                BrutalAPMod.AllowUnlocks = false;
                if (__result != null)
                {
                    if (BrutalAPMod.buyHeroCount >= 4) return;
                    BrutalAPMod.buyHeroCount++;
                    BrutalAPMod.SendCheck("BuyHero_" + BrutalAPMod.buyHeroCount);
                }
            }
        }

        [HarmonyPatch(typeof(RunDataSO), "BuyShopItem")]
        class ShopPatch
        {
            static void Postfix()
            {
                int zone = BrutalAPMod.cachedHolder?.Run?.CurrentZoneID ?? -1;
                if (zone == 0)
                {
                    if (BrutalAPMod.farShopCount >= BrutalAPMod.shopCountFar) return;
                    BrutalAPMod.farShopCount++;
                    BrutalAPMod.SendCheck("Shop_Far_" + BrutalAPMod.farShopCount);
                }
                else if (zone == 1)
                {
                    if (BrutalAPMod.orpShopCount >= BrutalAPMod.shopCountOrp) return;
                    BrutalAPMod.orpShopCount++;
                    BrutalAPMod.SendCheck("Shop_Orp_" + BrutalAPMod.orpShopCount);
                }
            }
        }

        [HarmonyPatch(typeof(OverworldManagerBG), "ChangeZone")]
        class ZoneLockPatch
        {
            static bool Prefix(OverworldManagerBG __instance)
            {
                var holderField = typeof(OverworldManagerBG).GetField("_informationHolder",
                    System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                var holder = holderField.GetValue(__instance) as GameInformationHolder;
                var run = holder.Run;
                int nextZone = run.CurrentZoneID + 1;

                if (nextZone == 1 && !BrutalAPMod.receivedAPItems.Contains("Orpheum Access"))
                {
                    Debug.Log("AP BLOCKED: Need Orpheum Access");
                    return false;
                }
                if (nextZone == 2 && !BrutalAPMod.receivedAPItems.Contains("Sepulchre Access"))
                {
                    Debug.Log("AP BLOCKED: Need Sepulchre Access");
                    return false;
                }
                return true;
            }
        }

        [HarmonyPatch(typeof(AchievementsManagerData), "TryUnlockAchievement", new Type[] { typeof(Achievement) })]
        class BlockAchievementPatch
        {
            static bool Prefix(Achievement achievement)
            {
                if (BrutalAPMod.AllowUnlocks)
                    return true;

                // Если игра ещё не загрузилась, разрешаем (стартовые герои)
                if (BrutalAPMod.cachedHolder == null || BrutalAPMod.cachedHolder.Game == null)
                    return true;

                foreach (var kvp in BrutalAPMod.heroAchievementMap)
                {
                    if (kvp.Value == achievement)
                    {
                        string heroName = kvp.Key;
                        string heroID;
                        if (!BrutalAPMod.heroUnlockID.TryGetValue("Hero_" + heroName, out heroID))
                            return false;

                        if (IsCharacterAlreadyUnlocked(heroID))
                            return true;   // уже открыт — не шлём чек

                        BrutalAPMod.SendCheck("Hero_" + heroName);
                        return false;
                    }
                }
                return false;
            }
        }

        [HarmonyPatch(typeof(UnlockablesManager), "TryUnlockFromID", new Type[] { typeof(string) })]
        class BlockUnlockPatch
        {
            static bool Prefix(string id)
            {
                if (BrutalAPMod.AllowUnlocks)
                    return true;

                if (BrutalAPMod.cachedHolder == null || BrutalAPMod.cachedHolder.Game == null)
                    return true;

                // Герой?
                if (BrutalAPMod.heroUnlockID.Values.Contains(id))
                {
                    string heroAPName = null;
                    foreach (var kvp in BrutalAPMod.heroUnlockID)
                        if (kvp.Value == id) { heroAPName = kvp.Key; break; }

                    if (IsCharacterAlreadyUnlocked(id))
                        return true;   // уже открыт — пропускаем

                    BrutalAPMod.SendCheck(heroAPName);
                    return false;
                }

                // Предмет – всегда чек
                BrutalAPMod.SendCheck("Item_" + id);
                return false;
            }
        }
    }
}