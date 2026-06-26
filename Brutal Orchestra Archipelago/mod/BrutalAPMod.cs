using BepInEx;
using BepInEx.Configuration;
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
        private ConfigEntry<string> serverUrl;
        private ConfigEntry<string> slotName;
        public ConfigEntry<int> configShopFar;
        public ConfigEntry<int> configShopOrp;
        public ConfigEntry<int> configBattleCount;
        public ConfigEntry<int> configBossCount;

        public static APClient apClient;
        public static HashSet<string> receivedAPItems = new HashSet<string>();
        public static HashSet<string> sentChecks = new HashSet<string>();

        // Counters (no longer reset on new run)
        public static int battleCount = 0;
        public static int farMoneyChestCount = 0;
        public static int orpMoneyChestCount = 0;
        public static int farArtifactChestCount = 0;
        public static int orpArtifactChestCount = 0;
        public static int buyHeroCount = 0;
        public static int farShopCount = 0;
        public static int orpShopCount = 0;

        // Limits from server (slot_data)
        public static int shopCountFar = 6;
        public static int shopCountOrp = 6;
        public static int battleLimit = 12;
        public static int bossCount = 3;
        // Fallback values from config (used if slot_data not received)
        public static int fallbackShopFar = 6;
        public static int fallbackShopOrp = 6;
        public static int fallbackBattleCount = 12;
        public static int fallbackBossCount = 3;

        public static GameInformationHolder cachedHolder = null;
        public static Queue<Action> pendingActions = new Queue<Action>();
        private static bool runInitialized = false;

        private static readonly HashSet<string> transientItems = new HashSet<string>
        {
            "5 Coins", "10 Coins", "15 Coins"
        };

        public static Dictionary<string, long> locationIDs = new Dictionary<string, long>();

        public static Dictionary<long, string> itemIdToName = new Dictionary<long, string>
        {
            {10000, "Orpheum Access"}, {10001, "Garden Access"},
            {10002, "Boss 1"}, {10003, "Boss 2"}, {10004, "Boss 3"},
            {10005, "Hero_Boyle"}, {10006, "Hero_Hans"}, {10007, "Hero_Anton"},
            {10008, "Hero_Splig"}, {10009, "Hero_Pearl"}, {10010, "Hero_Thype"},
            {10011, "Hero_Griffin"}, {10012, "Hero_Arnold"}, {10013, "Hero_Dimitri"},
            {10014, "Hero_LongLiver"}, {10015, "Hero_Clive"}, {10016, "Hero_Kleiver"},
            {10017, "Hero_Cranes"}, {10018, "Hero_Agon"}, {10019, "Hero_Rags"},
            {10020, "Hero_SmokeStacks"}, {10021, "Hero_Leviat"}, {10022, "Hero_Gospel"},
            {10023, "Hero_Bimini"}, {10024, "Hero_Burnout"}, {10025, "Hero_Fennec"},
            {10026, "Hero_Mordrake"}, {10027, "Hero_Mung"}, {10028, "Hero_ShellyK"},
            {10029, "Hero_Formosus"},
            {10030, "Anemone Thread"}, {10031, "Beads of Something or Other"},
            {10032, "Blood Thirsty Idol"}, {10033, "Boonario"}, {10034, "Bosch's Fist"},
            {10035, "Clash of the Bleached"}, {10036, "Crown of Thorns"},
            {10037, "Dew-Covered Sticker"}, {10038, "Diseased Bodypart"},
            {10039, "Dried Paintbrush"}, {10040, "Eel's Skin"},
            {10041, "Fennel's Blessing"}, {10042, "Flesh-Piercing Skewer"},
            {10043, "Gilded Rage"}, {10044, "Golden Gun"}, {10045, "Heaven-Bound Noose"},
            {10046, "Idle Hands"}, {10047, "Inhalant"}, {10048, "Jumper Cables"},
            {10049, "Lucky Charm"}, {10050, "Mangled Messiah"}, {10051, "Marrow's Reach"},
            {10052, "Mirror Shard"}, {10053, "Mithridatism"}, {10054, "Mung Moon"},
            {10055, "Mutually Assured Destruction"}, {10056, "Myopia Glasses"},
            {10057, "Padded Gloves"}, {10058, "Pile of Dirt"}, {10059, "Pox Blanket"},
            {10060, "Prayer Beads"}, {10061, "Rabbit's Foot"}, {10062, "Roid Rage"},
            {10063, "Scab-Knuckled Gloves"}, {10064, "Sealed Envelope"},
            {10065, "Shattered Amulet"}, {10066, "Silk Tourniquet"},
            {10067, "Soul Contract"}, {10068, "Stimpak"}, {10069, "Strange Beads"},
            {10070, "The Unfinished Bolt"}, {10071, "Thick Gloves"},
            {10072, "Thorned Vines"}, {10073, "Tumor"},
            {10074, "Black Paint"}, {10075, "Blue Paint"}, {10076, "Brown Paint"},
            {10077, "Cyan Paint"}, {10078, "Gray Paint"}, {10079, "Green Paint"},
            {10080, "Lime Paint"}, {10081, "Magenta Paint"}, {10082, "Orange Paint"},
            {10083, "Pink Paint"}, {10084, "Purple Paint"}, {10085, "Red Paint"},
            {10086, "Teal Paint"}, {10087, "White Paint"}, {10088, "Yellow Paint"},
            {10089, "5 Coins"}, {10090, "10 Coins"}, {10091, "15 Coins"},
        };

        // Handlers
        public static Dictionary<string, Action> itemHandlers = new Dictionary<string, Action>()
        {
            // Heroes
            {"Hero_Boyle", UnlockNextHero}, {"Hero_Hans", UnlockNextHero},
            {"Hero_Anton", UnlockNextHero}, {"Hero_Splig", UnlockNextHero},
            {"Hero_Pearl", UnlockNextHero}, {"Hero_Thype", UnlockNextHero},
            {"Hero_Griffin", UnlockNextHero}, {"Hero_Arnold", UnlockNextHero},
            {"Hero_Dimitri", UnlockNextHero}, {"Hero_LongLiver", UnlockNextHero},
            {"Hero_Clive", UnlockNextHero}, {"Hero_Kleiver", UnlockNextHero},
            {"Hero_Cranes", UnlockNextHero}, {"Hero_Agon", UnlockNextHero},
            {"Hero_Rags", UnlockNextHero}, {"Hero_SmokeStacks", UnlockNextHero},
            {"Hero_Leviat", UnlockNextHero}, {"Hero_Gospel", UnlockNextHero},
            {"Hero_Bimini", UnlockNextHero}, {"Hero_Burnout", UnlockNextHero},
            {"Hero_Fennec", UnlockNextHero}, {"Hero_Mordrake", UnlockNextHero},
            {"Hero_Mung", UnlockNextHero}, {"Hero_ShellyK", UnlockNextHero},
            {"Hero_Formosus", UnlockNextHero},

            // Items
            {"Anemone Thread", UnlockRandomItem}, {"Beads of Something or Other", UnlockRandomItem},
            {"Blood Thirsty Idol", UnlockRandomItem}, {"Boonario", UnlockRandomItem},
            {"Bosch's Fist", UnlockRandomItem}, {"Clash of the Bleached", UnlockRandomItem},
            {"Crown of Thorns", UnlockRandomItem}, {"Dew-Covered Sticker", UnlockRandomItem},
            {"Diseased Bodypart", UnlockRandomItem}, {"Dried Paintbrush", UnlockRandomItem},
            {"Eel's Skin", UnlockRandomItem}, {"Fennel's Blessing", UnlockRandomItem},
            {"Flesh-Piercing Skewer", UnlockRandomItem}, {"Gilded Rage", UnlockRandomItem},
            {"Golden Gun", UnlockRandomItem}, {"Heaven-Bound Noose", UnlockRandomItem},
            {"Idle Hands", UnlockRandomItem}, {"Inhalant", UnlockRandomItem},
            {"Jumper Cables", UnlockRandomItem}, {"Lucky Charm", UnlockRandomItem},
            {"Mangled Messiah", UnlockRandomItem}, {"Marrow's Reach", UnlockRandomItem},
            {"Mirror Shard", UnlockRandomItem}, {"Mithridatism", UnlockRandomItem},
            {"Mung Moon", UnlockRandomItem}, {"Mutually Assured Destruction", UnlockRandomItem},
            {"Myopia Glasses", UnlockRandomItem}, {"Padded Gloves", UnlockRandomItem},
            {"Pile of Dirt", UnlockRandomItem}, {"Pox Blanket", UnlockRandomItem},
            {"Prayer Beads", UnlockRandomItem}, {"Rabbit's Foot", UnlockRandomItem},
            {"Roid Rage", UnlockRandomItem}, {"Scab-Knuckled Gloves", UnlockRandomItem},
            {"Sealed Envelope", UnlockRandomItem}, {"Shattered Amulet", UnlockRandomItem},
            {"Silk Tourniquet", UnlockRandomItem}, {"Soul Contract", UnlockRandomItem},
            {"Stimpak", UnlockRandomItem}, {"Strange Beads", UnlockRandomItem},
            {"The Unfinished Bolt", UnlockRandomItem}, {"Thick Gloves", UnlockRandomItem},
            {"Thorned Vines", UnlockRandomItem}, {"Tumor", UnlockRandomItem},

            // Paints
            {"Black Paint", UnlockRandomItem}, {"Blue Paint", UnlockRandomItem},
            {"Brown Paint", UnlockRandomItem}, {"Cyan Paint", UnlockRandomItem},
            {"Gray Paint", UnlockRandomItem}, {"Green Paint", UnlockRandomItem},
            {"Lime Paint", UnlockRandomItem}, {"Magenta Paint", UnlockRandomItem},
            {"Orange Paint", UnlockRandomItem}, {"Pink Paint", UnlockRandomItem},
            {"Purple Paint", UnlockRandomItem}, {"Red Paint", UnlockRandomItem},
            {"Teal Paint", UnlockRandomItem}, {"White Paint", UnlockRandomItem},
            {"Yellow Paint", UnlockRandomItem},

            // Coins
            {"5 Coins", () => AddCoins(5)},
            {"10 Coins", () => AddCoins(10)},
            {"15 Coins", () => AddCoins(15)},

            // Accesses & Boss keys
            {"Far Shore Access", () => {}}, {"Orpheum Access", () => {}},
            {"Garden Access", () => {}}, {"Boss 1", () => {}},
            {"Boss 2", () => {}}, {"Boss 3", () => {}},
        };

        private const string PlayerPrefsKey = "BrutalAP_ReceivedItems";
        private const string CountersKey = "BrutalAP_Counters";
        
        public static void SaveCounters()
        {
            var counters = new Dictionary<string, int>
            {
                {"battle", battleCount},
                {"farMoney", farMoneyChestCount},
                {"orpMoney", orpMoneyChestCount},
                {"farArtifact", farArtifactChestCount},
                {"orpArtifact", orpArtifactChestCount},
                {"buyHero", buyHeroCount},
                {"farShop", farShopCount},
                {"orpShop", orpShopCount},
            };
            // Save as simple string: key=value;key=value...
            string data = string.Join(";", counters.Select(kv => kv.Key + "=" + kv.Value));
            PlayerPrefs.SetString(CountersKey, data);
            PlayerPrefs.Save();
            Debug.Log("AP: Saved counters: " + data);
        }

        public static void LoadCounters()
        {
            if (PlayerPrefs.HasKey(CountersKey))
            {
                string data = PlayerPrefs.GetString(CountersKey, "");
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
                            case "battle": battleCount = val; break;
                            case "farMoney": farMoneyChestCount = val; break;
                            case "orpMoney": orpMoneyChestCount = val; break;
                            case "farArtifact": farArtifactChestCount = val; break;
                            case "orpArtifact": orpArtifactChestCount = val; break;
                            case "buyHero": buyHeroCount = val; break;
                            case "farShop": farShopCount = val; break;
                            case "orpShop": orpShopCount = val; break;
                        }
                    }
                    Debug.Log("AP: Loaded counters: " + data);
                }
            }
        }

        // Unlock methods (unchanged)
        static void UnlockNextHero()
        {
            Debug.Log("AP: UnlockNextHero called");
            if (cachedHolder == null || cachedHolder.UnlockableManager == null) return;
            try
            {
                AchievementsManagerData achDB = null;
                var achField = typeof(UnlockablesManager).GetField("_achievementDB",
                    System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                if (achField != null)
                    achDB = achField.GetValue(cachedHolder.UnlockableManager) as AchievementsManagerData;
                if (achDB == null)
                {
                    var holderAchField = typeof(GameInformationHolder).GetField("_achievementDB",
                        System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                    if (holderAchField != null)
                        achDB = holderAchField.GetValue(cachedHolder) as AchievementsManagerData;
                }
                if (achDB == null)
                    achDB = Resources.FindObjectsOfTypeAll<AchievementsManagerData>().FirstOrDefault();
                if (achDB == null)
                {
                    Debug.LogError("AP: AchievementsManagerData not found anywhere");
                    return;
                }
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
                foreach (var item in enumerable)
                {
                    UnlockableModData data = null;
                    if (item is KeyValuePair<string, UnlockableModData> kv)
                        data = kv.Value;
                    else if (item is UnlockableModData direct)
                        data = direct;
                    else continue;
                    if (data == null || !data.hasCharacterUnlock || data.character == null) continue;
                    if (!game.IsCharacterUnlocked(data.character))
                    {
                        string charName = Traverse.Create(data.character).Field("_characterName")?.GetValue() as string
                                          ?? Traverse.Create(data.character).Property("characterName")?.GetValue() as string
                                          ?? (data.character as string)
                                          ?? data.character.ToString();
                        if (heroAchievementMap.TryGetValue(charName, out Achievement ach))
                        {
                            UnlockAchievementLocally(achDB, ach);
                            Debug.Log($"AP: Unlocked hero {charName} via achievement");
                            return;
                        }
                        else
                        {
                            string id = Traverse.Create(data).Field("id")?.GetValue() as string;
                            if (!string.IsNullOrEmpty(id))
                            {
                                cachedHolder.UnlockableManager.TryUnlockFromID(id);
                                Debug.Log($"AP: Unlocked hero {charName} via TryUnlockFromID({id})");
                                return;
                            }
                        }
                    }
                }
            }
            catch (Exception e) { Debug.LogError("UnlockNextHero error: " + e); }
        }
        
        private static void UnlockAchievementLocally(AchievementsManagerData achDB, Achievement achievement)
        {
            // Get the dictionary of achievements
            var dictField = typeof(AchievementsManagerData).GetField("m_achievementDict",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            if (dictField == null) return;

            var dict = dictField.GetValue(achDB) as Dictionary<Achievement, Achievement_t>;
            if (dict == null) return;

            if (!dict.TryGetValue(achievement, out var achievementData)) return;
            if (achievementData.m_offlinebAchieved) return; // already unlocked

            // Mark as unlocked offline
            achievementData.m_offlinebAchieved = true;

            // Add to loaded achievements
            var loadedAchievementsField = typeof(AchievementsManagerData).GetField("_LoadedAchievements",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            if (loadedAchievementsField != null)
            {
                var loadedAchievements = loadedAchievementsField.GetValue(achDB);
                if (loadedAchievements != null)
                {
                    var achievementIDsField = loadedAchievements.GetType().GetField("achievementIDs",
                        System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);
                    if (achievementIDsField != null)
                    {
                        var ids = achievementIDsField.GetValue(loadedAchievements) as HashSet<int>;
                        ids?.Add((int)achievement);
                    }

                    var needsSaveField = loadedAchievements.GetType().GetField("NeedsToBeSaved",
                        System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);
                    if (needsSaveField != null)
                        needsSaveField.SetValue(loadedAchievements, true);
                }
            }

            // Add to boss achievements if needed (not necessary for heroes, but keep for completeness)
            if ((int)achievement > 1000 && (int)achievement <= 10000)
            {
                var bossField = typeof(AchievementsManagerData).GetField("_completedBossAchievements",
                    System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                if (bossField != null)
                {
                    var bossSet = bossField.GetValue(achDB) as HashSet<int>;
                    bossSet?.Add((int)achievement);
                }
            }
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
                    cachedHolder.UnlockableManager.TryUnlockFromID(pick.Key);
                    Debug.Log($"AP: Unlocked items via {pick.Key}");
                }
            }
            catch (Exception e) { Debug.LogError("UnlockRandomItem error: " + e); }
        }

        static void AddCoins(int amount)
        {
            if (cachedHolder == null || cachedHolder.Run == null || cachedHolder.Run.playerData == null)
            {
                Debug.LogError("AP: GameInformationHolder or playerData not ready!");
                return;
            }
            cachedHolder.Run.playerData.AddCurrency(amount);
            Debug.Log($"AP: Added {amount} coins");
        }

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

        void Start()
        {
            serverUrl = Config.Bind("Server", "URL", "ws://localhost:38281",
                "Archipelago server WebSocket URL");
            slotName = Config.Bind("Server", "SlotName", "Test1",
                "Your player slot name on the server");
            var cfgShopFar = Config.Bind("Server", "shop_far", 6, "Fallback Far shop count");
            var cfgShopOrp = Config.Bind("Server", "shop_orp", 6, "Fallback Orp shop count");
            var cfgBattle = Config.Bind("Server", "battle_count", 12, "Fallback battle count");
            var cfgBoss = Config.Bind("Server", "boss_count", 3, "Fallback boss count");
            
            fallbackShopFar = cfgShopFar.Value;
            fallbackShopOrp = cfgShopOrp.Value;
            fallbackBattleCount = cfgBattle.Value;
            fallbackBossCount = cfgBoss.Value;

            LoadReceivedItems();
            LoadCounters();
            SkipTutorial();

            var harmony = new Harmony("brutal.ap.mod");
            harmony.PatchAll();

            apClient = new APClient(serverUrl.Value, slotName.Value);
            apClient.Connect();
            Debug.Log("MOD LOADED");
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
                int shopFar = 0, shopOrp = 0, battles = 0, bosses = 0;

                // Parse slot_data if available
                if (!string.IsNullOrEmpty(slotDataJson))
                {
                    ParseInt(slotDataJson, "\"shop_far\"", ref shopFar);
                    ParseInt(slotDataJson, "\"shop_orp\"", ref shopOrp);
                    ParseInt(slotDataJson, "\"battle_count\"", ref battles);
                    ParseInt(slotDataJson, "\"boss_count\"", ref bosses);
                }

                // Fallback to config if slot_data didn't provide values (0)
                if (shopFar == 0) shopFar = fallbackShopFar;
                if (shopOrp == 0) shopOrp = fallbackShopOrp;
                if (battles == 0) battles = fallbackBattleCount;
                if (bosses == 0) bosses = fallbackBossCount;

                shopCountFar = shopFar;
                shopCountOrp = shopOrp;
                battleLimit = battles;
                bossCount = bosses;

                locationIDs.Clear();

                // Fixed locations
                int id = 100;
                locationIDs["Far Shore Access"] = id++;
                locationIDs["Orpheum Access"] = id++;
                locationIDs["Garden Access"] = id++;
                locationIDs["Far MoneyChest_1"] = id++;
                locationIDs["Far MoneyChest_2"] = id++;
                locationIDs["Far ArtifactChest_1"] = id++;
                locationIDs["Far ArtifactChest_2"] = id++;
                locationIDs["Orp MoneyChest_1"] = id++;
                locationIDs["Orp MoneyChest_2"] = id++;
                locationIDs["Orp ArtifactChest_1"] = id++;
                locationIDs["Orp ArtifactChest_2"] = id++;
                locationIDs["BuyHero_1"] = id++;
                locationIDs["BuyHero_2"] = id++;
                locationIDs["BuyHero_3"] = id++;
                locationIDs["BuyHero_4"] = id++;

                // Battles
                for (int i = 1; i <= battleLimit; i++)
                    locationIDs[$"Battle_{i}"] = id++;

                // Bosses
                if (bossCount >= 1) locationIDs["Far Boss"] = id++;
                if (bossCount >= 2) locationIDs["Orp Boss"] = id++;
                if (bossCount >= 3) locationIDs["Garden Boss"] = id++;

                // Shops
                for (int i = 1; i <= shopCountFar; i++)
                    locationIDs[$"Shop_Far_{i}"] = id++;
                for (int i = 1; i <= shopCountOrp; i++)
                    locationIDs[$"Shop_Orp_{i}"] = id++;

                Debug.Log($"AP: Initialized location IDs. Battles: {battleLimit}, Bosses: {bossCount}, Far Shops: {shopCountFar}, Orp Shops: {shopCountOrp}");

                InitStartingChecks();
            }
            catch (Exception e) { Debug.LogError("InitSlotData error: " + e); }
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
            // Save everything that is not transient (coins) and not one-time unlock triggers
            var itemsToSave = receivedAPItems.Where(item =>
                !transientItems.Contains(item) && 
                item != "Hero Unlock" && item != "Item Unlock"
            ).ToArray();
            string data = string.Join(";", itemsToSave);
            PlayerPrefs.SetString(PlayerPrefsKey, data);
            PlayerPrefs.Save();
            Debug.Log("AP: Saved received items: " + data);
        }

        static void LoadReceivedItems()
        {
            if (PlayerPrefs.HasKey(PlayerPrefsKey))
            {
                string data = PlayerPrefs.GetString(PlayerPrefsKey, "");
                if (!string.IsNullOrEmpty(data))
                {
                    string[] items = data.Split(';');
                    foreach (string item in items)
                    {
                        if (!string.IsNullOrEmpty(item) && item != "Hero Unlock" && item != "Item Unlock")
                            receivedAPItems.Add(item);
                    }
                    Debug.Log("AP: Loaded received items: " + data);
                }
            }
        }

        public static void SendCheck(string locationName)
        {
            if (apClient == null) return;
            if (sentChecks.Contains(locationName)) return;
            sentChecks.Add(locationName);
            Debug.Log("CHECK: " + locationName);
            apClient.SendCheck(locationName);
        }

        public static void OnItemReceived(string itemName)
        {
            Debug.Log($"AP: Received item: {itemName}");
            if (itemHandlers.TryGetValue(itemName, out var handler))
            {
                if (cachedHolder != null)
                {
                    Debug.Log("AP: Executing action immediately (holder exists)");
                    handler.Invoke();
                }
                else
                {
                    pendingActions.Enqueue(handler);
                    Debug.Log($"AP: Queued handler for {itemName}");
                }
            }

            // Always add to runtime set (except transient coins)
            if (!transientItems.Contains(itemName))
            {
                receivedAPItems.Add(itemName);
                // Save only what should persist (everything except one-time unboxing items)
                if (itemName != "Hero Unlock" && itemName != "Item Unlock")
                {
                    SaveReceivedItems();
                }
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

        public static void ApplyAllReceivedItems()
        {
            if (cachedHolder == null) return;
            Debug.Log("AP: Applying all received items");
            foreach (var itemName in receivedAPItems)
            {
                if (transientItems.Contains(itemName) || itemName == "Hero Unlock" || itemName == "Item Unlock" ||
                    itemName == "Orpheum Access" || itemName == "Garden Access")
                    continue;
                if (itemHandlers.TryGetValue(itemName, out var handler))
                {
                    Debug.Log($"AP: Re-applying {itemName}");
                    handler.Invoke();
                }
            }
            runInitialized = true;
        }

        public static void ResetRunState()
        {
            runInitialized = false;
            Debug.Log("AP: Run state reset (counters preserved)");
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
                    BrutalAPMod.ResetRunState();
                    BrutalAPMod.ProcessPendingActions();
                }

                if (!BrutalAPMod.runInitialized)
                    BrutalAPMod.ApplyAllReceivedItems();

                RunDataSO run = __instance.Run;
                if (run != null && run.IsCurrentCardType(CardType.Boss))
                {
                    int zone = run.CurrentZoneID;
                    string bossLoc = zone == 0 ? "Far Boss" : (zone == 1 ? "Orp Boss" : "Garden Boss");
                    if (BrutalAPMod.locationIDs.ContainsKey(bossLoc) &&
                        !BrutalAPMod.sentChecks.Contains(bossLoc))
                        BrutalAPMod.SendCheck(bossLoc);
                    return;
                }

                if (BrutalAPMod.battleCount >= BrutalAPMod.battleLimit) return;
                BrutalAPMod.battleCount++;
                string loc = "Battle_" + BrutalAPMod.battleCount;
                if (!BrutalAPMod.sentChecks.Contains(loc))
                    BrutalAPMod.SendCheck(loc);
            }
        }

        [HarmonyPatch(typeof(MoneyChestContentData), "OpenTreasure")]
        class MoneyChestPatch
        {
            static void Postfix()
            {
                int zone = BrutalAPMod.cachedHolder?.Run?.CurrentZoneID ?? -1;
                if (zone == 0)
                {
                    if (BrutalAPMod.farMoneyChestCount >= 2) return;
                    BrutalAPMod.farMoneyChestCount++;
                    BrutalAPMod.SendCheck("Far MoneyChest_" + BrutalAPMod.farMoneyChestCount);
                }
                else if (zone == 1)
                {
                    if (BrutalAPMod.orpMoneyChestCount >= 2) return;
                    BrutalAPMod.orpMoneyChestCount++;
                    BrutalAPMod.SendCheck("Orp MoneyChest_" + BrutalAPMod.orpMoneyChestCount);
                }
            }
        }

        [HarmonyPatch(typeof(PrizeContentData), "OpenTreasure")]
        class ArtifactChestPatch
        {
            static void Postfix()
            {
                int zone = BrutalAPMod.cachedHolder?.Run?.CurrentZoneID ?? -1;
                if (zone == 0)
                {
                    if (BrutalAPMod.farArtifactChestCount >= 2) return;
                    BrutalAPMod.farArtifactChestCount++;
                    BrutalAPMod.SendCheck("Far ArtifactChest_" + BrutalAPMod.farArtifactChestCount);
                }
                else if (zone == 1)
                {
                    if (BrutalAPMod.orpArtifactChestCount >= 2) return;
                    BrutalAPMod.orpArtifactChestCount++;
                    BrutalAPMod.SendCheck("Orp ArtifactChest_" + BrutalAPMod.orpArtifactChestCount);
                }
            }
        }

        [HarmonyPatch(typeof(RunDataSO), "BuyFoolCharacter")]
        class BuyFoolPatch
        {
            static void Postfix(CharacterSO __result)
            {
                if (__result == null) return;
                if (BrutalAPMod.buyHeroCount >= 4) return;
                BrutalAPMod.buyHeroCount++;
                BrutalAPMod.SendCheck("BuyHero_" + BrutalAPMod.buyHeroCount);
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
                if (nextZone == 2 && !BrutalAPMod.receivedAPItems.Contains("Garden Access"))
                {
                    Debug.Log("AP BLOCKED: Need Garden Access");
                    return false;
                }
                return true;
            }
        }
    }
}