using BepInEx;
using HarmonyLib;
using UnityEngine;
using System.Collections.Generic;
using System;
using System.Linq;
using System.Timers;

namespace BrutalOrchestraAr
{
    [BepInPlugin("brutal.ap.mod", "Brutal AP Mod", "1.0.0")]
    public class BrutalAPMod : BaseUnityPlugin
    {
        public static APClient apClient;
        public static HashSet<string> receivedAPItems = new HashSet<string>();
        public static HashSet<string> sentChecks = new HashSet<string>();
        public static Queue<string> pendingChecks = new Queue<string>();

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
        public static int gardenBattleCount = 0;
        public static int gardenMoneyChestCount = 0;
        public static int gardenArtifactChestCount = 0;
        public static int gardenShopCount = 0;
        public static int bossDefeatCount = 0;

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
        public static int gardenBattleLimit = 10;
        public static int gardenMoneyLimit = 2;
        public static int gardenArtifactLimit = 2;
        public static int gardenShopLimit = 6;

        public static bool AllowUnlocks = false;
        public static bool IsHardmode = false;

        private static bool dumped = false;

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
            {10000, "Orpheum Access"}, {10001, "Quarry Access"},
            {10002, "Boss 1"}, {10003, "Boss 2"}, {10004, "Boss 3"},
            {10005, "Anemone Thread"}, {10006, "Beads of Something or Other"}, {10007, "Blood Thirsty Idol"},
            {10008, "Boonario"}, {10009, "Bosch's Fist"}, {10010, "Clash of the Bleached"},
            {10011, "Crown of Thorns"}, {10012, "Dew-Covered Sticker"}, {10013, "Diseased Bodypart"},
            {10014, "Dried Paintbrush"}, {10015, "Eel's Skin"}, {10016, "Fennel's Blessing"},
            {10017, "Flesh-Piercing Skewer"}, {10018, "Gilded Rage"}, {10019, "Golden Gun"},
            {10020, "Heaven-Bound Noose"}, {10021, "Idle Hands"}, {10022, "Inhalant"},
            {10023, "Jumper Cables"}, {10024, "Lucky Charm"}, {10025, "Mangled Messiah"},
            {10026, "Marrow's Reach"}, {10027, "Mirror Shard"}, {10028, "Mithridatism"},
            {10029, "Mung Moon"}, {10030, "Mutually Assured Destruction"}, {10031, "Myopia Glasses"},
            {10032, "Padded Gloves"}, {10033, "Pile of Dirt"}, {10034, "Pox Blanket"},
            {10035, "Prayer Beads"}, {10036, "Rabbit's Foot"}, {10037, "Roid Rage"},
            {10038, "Scab-Knuckled Gloves"}, {10039, "Sealed Envelope"}, {10040, "Shattered Amulet"},
            {10041, "Silk Tourniquet"}, {10042, "Soul Contract"}, {10043, "Stimpak"},
            {10044, "Strange Beads"}, {10045, "The Unfinished Bolt"}, {10046, "Thick Gloves"},
            {10047, "Thorned Vines"}, {10048, "Tumor"},
            {10049, "5 Coins"}, {10050, "10 Coins"}, {10051, "15 Coins"},
            {10052, "Hardmode Access"},
            {10053, "Quarry Boss Defeat"}, {10054, "Garden Boss Defeat"},
            {10055, "Rib of Eve"}, {10056, "Immolated Fairy"}, {10057, "Skinned Skate"},
            {10058, "Strange Fruit"}, {10059, "Unfortunate Prophecy"}, {10060, "Can o' Worms"},
            {10061, "Box of Medals"}, {10062, "Wicker Child"}, {10063, "Lady Gloves"},
            {10064, "Convergent Rage"}, {10065, "The Ideal Form of Trash"}, {10066, "Lust Pudding"},
            {10067, "Someone Else's Face"}, {10068, "Counterfeit Medal"}, {10069, "Consolation Prize"},
            {10070, "Egg of Firmament"}, {10071, "Ascetic Egg"}, {10072, "Indulgence"},
            {10073, "The First Born"}, {10074, "Dried Paint"}, {10075, "Lil Orro"},
            {10076, "Chain of Command"}, {10077, "Gump Ming Goa"}, {10078, "Ed's Tags"},
            {10079, "Tape Worm Pills"}, {10080, "Shard of Nowak"}, {10081, "All that is Mortal"},
            {10082, "Brigade of Dis"}, {10083, "The Brand"}, {10084, "The Human Soul"},
            {10085, "Idea of Evil"}, {10086, "Mini Mordrake"}, {10087, "Mung? Unlock"},
            {10088, "Fog's Prescience (NPC)"}, {10089, "Conscription Notice"}, {10090, "Ichthys"},
            {10091, "Bloating Coffers"}, {10092, "Serpent's Head"}, {10093, "Caretaker's Cudgel"},
            {10094, "Howling Log"}, {10095, "Tondal's Vision"}, {10096, "Peg Leg"},
            {10097, "Demon Core"}, {10098, "Harvest and Plenty"}, {10099, "Blood Breathing Bomb"},
            {10100, "Man Made Ovum"}, {10101, "Hereafter"}, {10102, "Ol Reliable"},
            {10103, "The Rest of Nowak"}, {10104, "Stillborn Egg"}, {10105, "Arachnid Aphrodisiac"},
            {10106, "Baltic Brine"}, {10107, "Dum-Dum"}, {10108, "Expired Medicine"},
            {10109, "Forgotten Pump"}, {10110, "Gentlemen's Glove"}, {10111, "Littering Leaflets"},
            {10112, "Soap?"}, {10113, "Homeless Hotline"}, {10114, "Lil Smiley"},
            {10115, "Pharmaceutical Roller Coaster"}, {10116, "Used Dog Tags"}, {10117, "War Bond"},
            {10118, "Professional Procrastinator"}, {10119, "Vowbreaker"},
            {10120, "Sacrificial Saint"}, {10121, "Starving Apples"}, {10122, "Gift Box!"},
            {10123, "Defective Rounds"},
            {10124, "Another Dud"}, {10125, "Purple Heart"}, {10126, "Rorscach Test"},
            {10127, "Roentgen Rays"}, {10128, "Health insurance"}, {10129, "A Gift?"},
            {10130, "Rotund Amphibian"}, {10131, "Gamified Cephalopod"},
            {10132, "You Can Do It!"}, {10133, "Russki Vampire"}, {10134, "Extra Stitching"},
            {10135, "Pain Killers"}, {10136, "Lycanthrope's Core"}, {10137, "Head of Scrybe"},
            {10138, "Fishing Rod"}, {10139, "Effigy of the Mettle Mother"},
            {10140, "Gilded Mirror"}, {10141, "Spiked Collar"},
            {10142, "The Cougar"}, {10143, "Someone Else's Wedding Ring"}, {10144, "Fist Full of Ash"},
            {10145, "Czech Hedgehog"}, {10146, "Cremation"}, {10147, "Deworming Pills"},
            {10148, "Medical Leches"}, {10149, "Holy Chalice"}, {10150, "Seeds of the Consumed"},
            {10151, "The Jersey"}, {10152, "Pontiff's Parade"}, {10153, "Mystery Ration"},
            {10154, "Ol' Stumpy"}, {10155, "Iron Necklace"}, {10156, "The Apple"},
            {10157, "Trepanation"}, {10158, "Wheel of Fortune"}, {10159, "Prussian Blue"},
            {10160, "DDT"}, {10161, "Blind Faith"}, {10162, "Modern Medicine"},
            {10163, "Divine Mud"}, {10164, "Opulent Egg"}, {10165, "Cloth Cock"},
            {10166, "Sculptur's Tools"}, {10167, "Gospel's Severed Head"},
            {10168, "Wels Catfish"}, {10169, "Left Shoe"}, {10170, "Meatre Worm"},
            {10171, "Norris!"}, {10172, "Burn-Bottle Batch"}, {10173, "Royal Pine"},
            {10174, "Coelacanth"}, {10175, "Sacred Shrub"}, {10176, "Mordrake"},
            {10177, "Faulty Land Mine"},
            {10178, "Bronzo's 2 Cents (Item)"}, {10179, "Counterfeit coins"},
            {10180, "Bananas"}, {10181, "Bronzo's Stupid Hat"},
            {10182, "Broken Doll"}, {10183, "Infernal Eye"}, {10184, "Vyacheslav's Last Sip"},
            {10185, "Wailing Whistle"}, {10186, "Cursed Sword"}, {10187, "Enigma"},
            {10188, "The Master's Sickle"}, {10189, "Esoteric Artifact"},
            {10190, "Mordrake's Untold Tale"},
            {10191, "Winstreak 2 Bundle"}, {10192, "Winstreak 3 Bundle"},
            {10193, "Winstreak 4 Bundle"}, {10194, "Winstreak 5 Bundle"},
            {10195, "HundredPercent Bundle"}
        };

        // Словарь: AP-имя предмета → реальный ID wearable-предмета (используется через Game.TryUnlockItem)
        public static Dictionary<string, string> itemNameToUnlockableID = new Dictionary<string, string>
        {
            {"Rib of Eve", "RibOfEve_TW"},
            {"Immolated Fairy", "ImmolatedFairy_TW"},
            {"Skinned Skate", "SkinnedSkate_TW"},
            {"Strange Fruit", "StrangeFruit_TW"},
            {"Unfortunate Prophecy", "UnfortunateProphecy_TW"},
            {"Can o' Worms", "CanOfWorms_SW"},
            {"Box of Medals", "BoxOfMedals_SW"},
            {"Wicker Child", "WickerChild_TW"},
            {"Lady Gloves", "LadyGloves_SW"},
            {"Convergent Rage", "ConvergentRage_TW"},
            {"The Ideal Form of Trash", "TheIdealFormOfTrash_TW"},
            {"Lust Pudding", "LustPudding_TW"},
            {"Someone Else's Face", "SomeoneElsesFace_SW"},
            {"Counterfeit Medal", "CounterfeitMedal_SW"},
            {"Consolation Prize", "ConsolationPrize_SW"},
            {"Egg of Firmament", "EggOfFirmament_TW"},
            {"Ascetic Egg", "AsceticEgg_TW"},
            {"Indulgence", "Indulgence_TW"},
            {"The First Born", "TheFirstBorn_TW"},
            {"Dried Paint", "DriedPaint_SW"},
            {"Lil Orro", "LilOrro_TW"},
            {"Chain of Command", "ChainofCommand_SW"},
            {"Gump Ming Goa", "GumpMingGoa_TW"},
            {"Ed's Tags", "EdsTags_SW"},
            {"Tape Worm Pills", "TapeWormPills_SW"},
            {"Shard of Nowak", "ShardOfNowak_TW"},
            {"All that is Mortal", "AllThatIsMortal_TW"},
            {"Brigade of Dis", "BrigadeOfDis_TW"},
            {"The Brand", "TheBrand_TW"},
            {"The Human Soul", "TheHumanSoul_TW"},
            {"Idea of Evil", "IdeaOfEvil_TW"},
            {"Mini Mordrake", "MiniMordrake_TW"},
            {"Conscription Notice", "ConscriptionNotice_SW"},
            {"Ichthys", "Ichthys_TW"},
            {"Bloating Coffers", "BloatingCoffers_TW"},
            {"Serpent's Head", "SerpentsHead_TW"},
            {"Caretaker's Cudgel", "CaretakersCudgel_TW"},
            {"Howling Log", "HowlingLong_TW"},
            {"Tondal's Vision", "TondalsVision_TW"},
            {"Peg Leg", "PegLeg_TW"},
            {"Demon Core", "DemonCore_SW"},
            {"Harvest and Plenty", "HarvestAndPlenty_TW"},
            {"Blood Breathing Bomb", "BloodBreathingBomb_TW"},
            {"Man Made Ovum", "ManMadeOvum_TW"},
            {"Hereafter", "Hereafter_TW"},
            {"Ol Reliable", "OlReliable_SW"},
            {"The Rest of Nowak", "TheRestOfNowak_TW"},
            {"Stillborn Egg", "StillbornEgg_TW"},
            {"Arachnid Aphrodisiac", "ArachnidAphrodisiac_TW"},
            {"Gift Box!", "GiftBox_SW"},
            {"Defective Rounds", "DefectiveRounds_SW"},
            {"Faulty Land Mine", "FaultyLandMine_SW"},
            {"Another Dud", "AnotherDud_SW"},
            {"Purple Heart", "PurpleHeart_SW"},
            {"Rorscach Test", "RorschachTest_SW"},
            {"Roentgen Rays", "RoentgenRays_SW"},
            {"Health insurance", "HealthInsurance_SW"},
            {"A Gift?", "AGift_TW"},
            {"Rotund Amphibian", "RotundAmphibian_TW"},
            {"Gamified Cephalopod", "GamifiedSquid_TW"},
            {"You Can Do It!", "YouCanDoIt_SW"},
            {"Russki Vampire", "RusskiVampire_SW"},
            {"Extra Stitching", "ExtraStitching_SW"},
            {"Pain Killers", "PainKillers_SW"},
            {"Lycanthrope's Core", "LycanthropesCore_TW"},
            {"Head of Scrybe", "HeadOfScribe_TW"},
            {"Fishing Rod", "FishingRod_TW"},
            {"Effigy of the Mettle Mother", "EffigyOfTheMettleMother_TW"},
            {"Gilded Mirror", "GildedMirror_TW"},
            {"Spiked Collar", "SpikedCollar_TW"},
            {"Someone Else's Wedding Ring", "SomeonesWeddingRing_TW"},
            {"Fist Full of Ash", "FistFullOfAsh_TW"},
            {"Czech Hedgehog", "CzechHedgehog_SW"},
            {"Cremation", "Cremation_TW"},
            {"Deworming Pills", "DewormingPills_SW"},
            {"Medical Leches", "MedicalLeeches_SW"},
            {"Holy Chalice", "HolyChalice_TW"},
            {"Seeds of the Consumed", "SeedsOfTheConsumed_TW"},
            {"The Jersey", "TheJersey_TW"},
            {"Pontiff's Parade", "PontiffsParade_TW"},
            {"Mystery Ration", "MysteryRation_SW"},
            {"Ol' Stumpy", "OlStumpy_SW"},
            {"Iron Necklace", "IronNecklace_SW"},
            {"The Apple", "TheApple_TW"},
            {"Trepanation", "Trepanation_TW"},
            {"Wheel of Fortune", "WheelOfFortune_TW"},
            {"Prussian Blue", "PrussianBlue_SW"},
            {"DDT", "DDT_SW"},
            {"Blind Faith", "BlindFaith_TW"},
            {"Modern Medicine", "ModernMedicine_SW"},
            {"Divine Mud", "DivineMud_TW"},
            {"Opulent Egg", "OpulentEgg_TW"},
            {"Cloth Cock", "ClothCock_SW"},
            {"Sculptur's Tools", "SculpturesTools_SW"},
            {"Gospel's Severed Head", "GospelsSeveredHead_TW"},
            {"Wels Catfish", "WelsCatfish_ExtraW"},
            {"Left Shoe", "LeftShoe_ExtraW"},
            {"Meatre Worm", "MeatreWorm_TW"},
            {"Norris!", "Norris_TW"},
            {"Burn-Bottle Batch", "BurnBottleBatch_SW"},
            {"Royal Pine", "RoyalPine_TW"},
            {"Coelacanth", "Coelacanth_ExtraW"},
            {"Sacred Shrub", "SacredShrub_TW"},
            {"Bronzo's 2 Cents (Item)", "Bronzos2Cents_SW"},
            {"Counterfeit coins", "CounterfeitCoin_SW"},
            {"Bananas", "Bananas_TW"},
            {"Bronzo's Stupid Hat", "BronzosStupidHat_TW"},
            {"Broken Doll", "BrokenDoll_TW"},
            {"Infernal Eye", "InfernalEye_TW"},
            {"Vyacheslav's Last Sip", "VyacheslavsLastSip_SW"},
            {"Wailing Whistle", "WailingWhistle_SW"},
            {"Cursed Sword", "CursedSword_TW"},
            {"Enigma", "Enigma_TW"},
            {"The Master's Sickle", "TheMastersSickle_SW"},
            {"Esoteric Artifact", "EsotericArtifact_SW"}
        };

        // Словари для героев и чеков
        public static readonly Dictionary<string, string> heroUnlockID = new Dictionary<string, string>
        {
            {"Hero_Anton", "Anton"}, {"Hero_Splig", "Splig"}, {"Hero_Pearl", "Pearl"},
            {"Hero_Thype", "Thype"}, {"Hero_Griffin", "Griffin"}, {"Hero_Arnold", "Arnold"},
            {"Hero_Dimitri", "Dimitri"}, {"Hero_LongLiver", "LongLiver"}, {"Hero_Clive", "Clive"},
            {"Hero_Kleiver", "Kleiver"}, {"Hero_Cranes", "Cranes"}, {"Hero_Agon", "Agon"},
            {"Hero_Rags", "Rags"}, {"Hero_SmokeStacks", "SmokeStacks"}, {"Hero_Leviat", "Leviat"},
            {"Hero_Gospel", "Gospel"}, {"Hero_Bimini", "Bimini"}, {"Hero_Burnout", "Burnout"},
            {"Hero_Fennec", "Fennec"}, {"Hero_Mordrake", "MordrakeCH"}, {"Hero_Mung", "MungCH"},
            {"Hero_ShellyK", "ShellyK"}, {"Hero_Formosus", "Formosus"}
        };

        public static readonly Dictionary<string, string> heroCheckName = new Dictionary<string, string>
        {
            {"Anton", "The Gambler"}, {"Splig", "The Conjoined"}, {"Pearl", "The Glutton"},
            {"Thype", "The Trickster"}, {"Griffin", "The Naked"}, {"Arnold", "The Failure"},
            {"Dimitri", "The Incinerated"}, {"LongLiver", "The Parasite"}, {"Clive", "The Stoic"},
            {"Kleiver", "The Zealot"}, {"Cranes", "The Corpse"}, {"Agon", "The Terminal"},
            {"Rags", "The Psychic"}, {"SmokeStacks", "The Black Lung"}, {"Leviat", "The Mass"},
            {"Gospel", "The Magnum Opus"}, {"Bimini", "The Immortal"}, {"Burnout", "The Sadist"},
            {"Fennec", "The Impaled"}, {"Mordrake", "The Mistake"}, {"Mung", "The Emotional Disaster"},
            {"ShellyK", "The Cougar"}, {"Formosus", "The Exhumed"}
        };

        public static readonly Dictionary<string, Achievement> heroAchievementMap = new Dictionary<string, Achievement>
        {
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

        private static readonly string[] allUnlockIDs = {
            "Roids","Hickory","Mobius","TheOuroboros","Smoothskin",
            "FarShore","Orpheum","ZoneExplorer","BossSlap","RoidsMissTurn",
            "SmoothskinTrauma","DontAct","FoolsDepleted","ShopDepleted",
            "Ending_GameOver","LoseToBoss","ManaCostDeath","OverflowDeath",
            "OrroSmooch","Ending_CorpseKill","SurviveStarvation",
            "Over100Coins","OneTurnHardKill","FriendlyFireDeath","ParasiteJourney",
            "Ending_CorpseSave",
            "Ending_HardMode",
            "TriggerFingers","Charcarrion","OsmanSinnoks","Heaven",
            "AllBosses","Ichor1","Ichor2","Ichor3","Ichor4",
            "Fogs1","Fogs2","Fogs3","UngodEmissary",
            "Garden","FarShoreNoCasualtiesHard","OrpheumNoCasualtiesHard",
            "GardenNoCasualtiesHard","AllZonesNoCasualtiesHard","WorldExplorer",
            "AllBossesSlap","BasicSpeedrun","UngodKill","SepulchreKill",
            "XiphactinusKill","UnfinishedHeirKill","CharcarrionDecomposition",
            "NoNowakHardEnd","NowakLoneSurvivor","HeavenDoubleSacrifice",
            "CasualtiesCharacter5","CasualtiesEnemy30","HeadshotDeath",
            "DamageDealt100","AntonSad",
            "Boyle_Osman","Boyle_Heaven","Hans_Osman","Hans_Heaven",
            "Burnout_Osman","Burnout_Heaven","Fennec_Osman","Fennec_Heaven",
            "Anton_Osman","Anton_Heaven","Splig_Osman","Splig_Heaven",
            "Pearl_Osman","Pearl_Heaven","Thype_Osman","Thype_Heaven",
            "Griffin_Osman","Griffin_Heaven","Arnold_Osman","Arnold_Heaven",
            "Dimitri_Osman","Dimitri_Heaven","LongLiver_Osman","LongLiver_Heaven",
            "Clive_Osman","Clive_Heaven","Kleiver_Osman","Kleiver_Heaven",
            "Cranes_Osman","Cranes_Heaven","Agon_Osman","Agon_Heaven",
            "Rags_Osman","Rags_Heaven","SmokeStacks_Osman","SmokeStacks_Heaven",
            "Leviat_Osman","Leviat_Heaven","Gospel_Osman","Gospel_Heaven",
            "Bimini_Osman","Bimini_Heaven","Mung_Osman","Mung_Heaven",
            "Mordrake_Osman","Mordrake_Heaven","ShellyK_Osman","ShellyK_Heaven",
            "Formosus_Osman","Formosus_Heaven","ProdigalFlee","MordrakeCH",
            "Bronzo1","Bronzo2","BronzoBossPhase1","Bronzo3","BronzoBossPhase2",
            "Bronzo4","BronzoBossPhase3","Bronzo5","BronzoBossPhase4","Bronzo6","BronzoBoss",
            "Mordrake1","Mordrake2","Mordrake3","Mordrake4","Mordrake5","Mordrake6",
            "VHSTask0","VHSTask1","VHSTask2","VHSTask3","VHSTask4","VHSTask5","VHSTask6",
            "Winstreak2","Winstreak3","Winstreak4","Winstreak5","HundredPercent"
        };

        private bool showGui = true;
        private string guiServer = "ws://localhost:38281";
        private string guiSlot = "Test1";
        private Rect windowRect = new Rect(20, 20, 300, 130);

        void Start()
        {
            SkipTutorial();
            var harmony = new Harmony("brutal.ap.mod");
            harmony.PatchAll();
            BrutalAPMod.ForceCacheUnlockableData();
            DumpUnlockableIDs();
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
                FlushPendingChecks();
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
                        if (!string.IsNullOrEmpty(item))
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
                int gardenBat = 10, gardenMon = 2, gardenArt = 2, gardenShops = 6;
                bool hardmode = true;

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
                    ParseBool(slotDataJson, "\"hardmode\"", ref hardmode);
                    if (hardmode)
                    {
                        ParseInt(slotDataJson, "\"garden_battle_count\"", ref gardenBat);
                        ParseInt(slotDataJson, "\"garden_money_chests\"", ref gardenMon);
                        ParseInt(slotDataJson, "\"garden_artifact_chests\"", ref gardenArt);
                        ParseInt(slotDataJson, "\"garden_shop_count\"", ref gardenShops);
                    }
                }

                farBattleLimit = farBat; orpBattleLimit = orpBat;
                farMoneyLimit = farMon; orpMoneyLimit = orpMon;
                farArtifactLimit = farArt; orpArtifactLimit = orpArt;
                shopCountFar = shopFar; shopCountOrp = shopOrp;
                bossCount = bosses;
                gardenBattleLimit = hardmode ? gardenBat : 0;
                gardenMoneyLimit = hardmode ? gardenMon : 0;
                gardenArtifactLimit = hardmode ? gardenArt : 0;
                gardenShopLimit = hardmode ? gardenShops : 0;

                locationIDs.Clear();

                int baseId = 100;
                locationIDs["Far Shore Access"] = baseId++;
                locationIDs["Orpheum Access"] = baseId++;
                locationIDs["Quarry Access"] = baseId++;
                for (int i = 1; i <= 4; i++)
                    locationIDs[$"BuyHero_{i}"] = baseId++;

                int farBattleId = 200;
                for (int i = 1; i <= farBattleLimit; i++)
                    locationIDs[$"Far_Battle_{i}"] = farBattleId++;
                int orpBattleId = 300;
                for (int i = 1; i <= orpBattleLimit; i++)
                    locationIDs[$"Orp_Battle_{i}"] = orpBattleId++;

                int farMoneyId = 400;
                for (int i = 1; i <= farMoneyLimit; i++)
                    locationIDs[$"Far_MoneyChest_{i}"] = farMoneyId++;
                int orpMoneyId = 500;
                for (int i = 1; i <= orpMoneyLimit; i++)
                    locationIDs[$"Orp_MoneyChest_{i}"] = orpMoneyId++;

                int farArtifactId = 600;
                for (int i = 1; i <= farArtifactLimit; i++)
                    locationIDs[$"Far_ArtifactChest_{i}"] = farArtifactId++;
                int orpArtifactId = 700;
                for (int i = 1; i <= orpArtifactLimit; i++)
                    locationIDs[$"Orp_ArtifactChest_{i}"] = orpArtifactId++;

                int bossId = 800;
                if (bossCount >= 1) locationIDs["Far Boss"] = bossId++;
                if (bossCount >= 2) locationIDs["Orp Boss"] = bossId++;
                if (bossCount >= 3) locationIDs["Quarry Boss"] = bossId++;

                int farShopId = 900;
                for (int i = 1; i <= shopCountFar; i++)
                    locationIDs[$"Shop_Far_{i}"] = farShopId++;
                int orpShopId = 1000;
                for (int i = 1; i <= shopCountOrp; i++)
                    locationIDs[$"Shop_Orp_{i}"] = orpShopId++;

                int heroId = 1100;
                string[] heroNames = {"Anton","Splig","Pearl","Thype","Griffin","Arnold",
                    "Dimitri","LongLiver","Clive","Kleiver","Cranes","Agon",
                    "Rags","SmokeStacks","Leviat","Gospel","Bimini","Burnout",
                    "Fennec","Mordrake","Mung","ShellyK","Formosus"};
                foreach (var name in heroNames)
                    locationIDs[heroCheckName[name]] = heroId++;

                Dictionary<string, string> itemCheckNames = new Dictionary<string, string>
                {
                    {"Roids","The Juggernaut"},{"Hickory","The Fire and Flames"},{"Mobius","The Widower"},
                    {"TheOuroboros","The Leviathan"},{"Smoothskin","The Orphan"},
                    {"FarShore","Beyond the Dunes"},{"Orpheum","Above the Mountains"},
                    {"ZoneExplorer","Thorough Explorer"},{"BossSlap","Throw Hands"},
                    {"RoidsMissTurn","Heavyweight Champion"},
                    {"SmoothskinTrauma","Emotional and Physical Annihilation"},
                    {"DontAct","Nary a Finger Lifted"},{"FoolsDepleted","Human Resources"},
                    {"ShopDepleted","Deep Pockets"},{"Ending_GameOver","It Happens to the Best of Us"},
                    {"LoseToBoss","Honest Effort"},{"ManaCostDeath","Tactical Miscalculation"},
                    {"OverflowDeath","Drowning in Pigment"},{"OrroSmooch","Smooch!"},
                    {"Ending_CorpseKill","The End?"},{"SurviveStarvation","Swallowed by the Sea"},
                    {"Over100Coins","Capital is King"},{"OneTurnHardKill","Brutality"},
                    {"FriendlyFireDeath","What are You Doing?"},{"ParasiteJourney","So Long Liver"},
                    {"Ending_CorpseSave","I'll Make You Regret This"},
                    {"Ending_HardMode","All that is Mortal"},
                    {"TriggerFingers","The Coward"},{"Charcarrion","The Messiah"},
                    {"OsmanSinnoks","The Witness"},{"Heaven","The Divine"},
                    {"AllBosses","Kingslayer"},
                    {"Ichor1","Ichor's Last Wish"},{"Ichor2","Ichor's Last Wish"},{"Ichor3","Ichor's Last Wish"},{"Ichor4","Ichor's Last Wish"},
                    {"Fogs1","Fog's Prescience"},{"Fogs2","Fog's Prescience"},{"Fogs3","Fog's Prescience"},
                    {"UngodEmissary","The Ungod's Demand"},
                    {"Garden","Within Yourself"},
                    {"FarShoreNoCasualtiesHard","Duke of the Dunes"},
                    {"OrpheumNoCasualtiesHard","Master of the Mountains"},
                    {"GardenNoCasualtiesHard","Garden of Earthly Delights"},
                    {"AllZonesNoCasualtiesHard","The Work of an Artists"},
                    {"WorldExplorer","Every Stone Tuned"},
                    {"AllBossesSlap","God of Phalanges, Palms and Pain"},
                    {"BasicSpeedrun","Decisive and Concise"},
                    {"UngodKill","God is Dead and We Have Killed Him"},
                    {"SepulchreKill","Somebody Call the Vatican"},
                    {"XiphactinusKill","Bit off More Than You Can Chew"},
                    {"UnfinishedHeirKill","Bloodline Drinker"},
                    {"CharcarrionDecomposition","Crisis of Faith"},
                    {"NoNowakHardEnd","Plot Armor"},
                    {"NowakLoneSurvivor","Worthy Successor"},
                    {"HeavenDoubleSacrifice","The Second Coming"},
                    {"CasualtiesCharacter5","Month of Funerals"},
                    {"CasualtiesEnemy30","Mass Grave Matters"},
                    {"HeadshotDeath","Boom, Headshot"},
                    {"DamageDealt100","War Criminal"},
                    {"AntonSad","Plenty of Fish in the Desert"},
                    {"Boyle_Osman","Another Dud"},{"Boyle_Heaven","Purple Heart"},
                    {"Hans_Osman","Rorscach Test"},{"Hans_Heaven","Roentgen Rays"},
                    {"Burnout_Osman","Health Insurance"},{"Burnout_Heaven","A Gift?"},
                    {"Fennec_Osman","Rotund Amphibian"},{"Fennec_Heaven","Gamified Cephalopod"},
                    {"Anton_Osman","You Can Do It!"},{"Anton_Heaven","Russki Vampire"},
                    {"Splig_Osman","Extra Stitching"},{"Splig_Heaven","Pain Killers"},
                    {"Pearl_Osman","Lycanthrope's Core"},{"Pearl_Heaven","Head of Scrybe"},
                    {"Thype_Osman","Fishing Rod"},{"Thype_Heaven","Effigy of the Mettle Mother"},
                    {"Griffin_Osman","Gilded Mirror"},{"Griffin_Heaven","Spiked Collar"},
                    {"Arnold_Osman","Someone Else's Wedding Ring"},{"Arnold_Heaven","Fist Full of Ash"},
                    {"Dimitri_Osman","Czech Hedgehog"},{"Dimitri_Heaven","Cremation"},
                    {"LongLiver_Osman","Deworming Pills"},{"LongLiver_Heaven","Medical Leches"},
                    {"Clive_Osman","Holy Chalice"},{"Clive_Heaven","Seeds of the Consumed"},
                    {"Kleiver_Osman","The Jersey"},{"Kleiver_Heaven","Pontiff's Parade"},
                    {"Cranes_Osman","Mystery Ration"},{"Cranes_Heaven","Ol' Stumpy"},
                    {"Agon_Osman","Iron Necklace"},{"Agon_Heaven","The Apple"},
                    {"Rags_Osman","Trepanation"},{"Rags_Heaven","Wheel of Fortune"},
                    {"SmokeStacks_Osman","Prussian Blue"},{"SmokeStacks_Heaven","DDT"},
                    {"Leviat_Osman","Blind Faith"},{"Leviat_Heaven","Modern Medicine"},
                    {"Bimini_Osman","Divine Mud"},{"Bimini_Heaven","Opulent Egg"},
                    {"Gospel_Osman","Sculptur's Tools"},{"Gospel_Heaven","Gospel's Severed Head"},
                    {"Mung_Osman","Wels Catfish"},{"Mung_Heaven","Left Shoe"},
                    {"Mordrake_Osman","Meatre Worm"},{"Mordrake_Heaven","Norris!"},
                    {"ShellyK_Osman","Burn-Bottle Batch"},{"ShellyK_Heaven","Royal Pine"},
                    {"Formosus_Osman","Coelacanth"},{"Formosus_Heaven","Sacred Shrub"},
                    {"ProdigalFlee","Fear of Gods Above"},{"MordrakeCH","The Mistake"},
                    {"Bronzo1","Bronzo's 2 Cents"},{"Bronzo2","What the !@#$ Nowak?"},
                    {"BronzoBossPhase1","What the !@#$ Nowak?"},{"Bronzo3","Okay Nowak, Seriously Stop!"},
                    {"BronzoBossPhase2","Okay Nowak, Seriously Stop!"},{"Bronzo4","That's it Nowak!"},
                    {"BronzoBossPhase3","That's it Nowak!"},{"Bronzo5","Time to Die!"},
                    {"BronzoBossPhase4","Time to Die!"},{"Bronzo6","The Shyster"},{"BronzoBoss","The Shyster"},
                    {"Mordrake1","Mordrake's Untold Tale"},{"Mordrake2","Mordrake's Untold Tale"},
                    {"Mordrake3","Mordrake's Untold Tale"},{"Mordrake4","Mordrake's Untold Tale"},
                    {"Mordrake5","Mordrake's Untold Tale"},{"Mordrake6","Mordrake's Untold Tale"},
                    {"VHSTask0","The Director's Final Frame"},{"VHSTask1","The Director's Final Frame"},
                    {"VHSTask2","The Director's Final Frame"},{"VHSTask3","The Director's Final Frame"},
                    {"VHSTask4","The Director's Final Frame"},{"VHSTask5","The Director's Final Frame"},
                    {"VHSTask6","The Director's Final Frame"},
                    {"Winstreak2","Dumb Luck"},{"Winstreak3","Notable Skill"},
                    {"Winstreak4","Burgeoning Expertise"},{"Winstreak5","Total and Absolute Mastery"},
                    {"HundredPercent","Brutal Orchestra"}
                };
                int itemId = 1300;
                HashSet<string> added = new HashSet<string>();
                foreach (string uid in allUnlockIDs)
                {
                    string cname = itemCheckNames.TryGetValue(uid, out string n) ? n : ("Item_" + uid);
                    if (!added.Contains(cname))
                    {
                        locationIDs[cname] = itemId++;
                        added.Add(cname);
                    }
                }

                locationIDs["Quarry_Boss_Spared"] = 2700;
                locationIDs["Mordrake's Untold Tale"] = 2800;
                locationIDs["The Director's Final Frame"] = 2900;

                if (hardmode)
                {
                    int gardenBattleId = 3000;
                    for (int i = 1; i <= gardenBattleLimit; i++)
                        locationIDs[$"Garden_Battle_{i}"] = gardenBattleId++;
                    int gardenMoneyId = 3100;
                    for (int i = 1; i <= gardenMoneyLimit; i++)
                        locationIDs[$"Garden_MoneyChest_{i}"] = gardenMoneyId++;
                    int gardenArtifactId = 3200;
                    for (int i = 1; i <= gardenArtifactLimit; i++)
                        locationIDs[$"Garden_ArtifactChest_{i}"] = gardenArtifactId++;
                    int gardenShopId = 3300;
                    for (int i = 1; i <= gardenShopLimit; i++)
                        locationIDs[$"Garden_Shop_{i}"] = gardenShopId++;
                    locationIDs["Garden Boss"] = 3400;
                }

                for (int i = 1; i <= 10; i++)
                    locationIDs[$"Quarry Boss Defeat {i}"] = 4000 + i;
                for (int i = 1; i <= 10; i++)
                    locationIDs[$"Garden Boss Defeat {i}"] = 4100 + i;

                Debug.Log($"AP: Initialized location IDs. Hardmode: {hardmode}");

                EnsureMinimumHeroes();
                LoadCountersForCurrentSeed();
                LoadSentChecks();
                LoadBossDefeatCount();
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
        
        public static void ForceCacheUnlockableData()
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
            Debug.Log($"AP: Force-cached {allUnlockableData.Count} unlockable entries");
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
                            case "gardenBattle": gardenBattleCount = val; break;
                            case "gardenMoney": gardenMoneyChestCount = val; break;
                            case "gardenArtifact": gardenArtifactChestCount = val; break;
                            case "gardenShop": gardenShopCount = val; break;
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

        private static void ParseBool(string json, string key, ref bool value)
        {
            int idx = json.IndexOf(key);
            if (idx == -1) return;
            int colon = json.IndexOf(':', idx);
            if (colon == -1) return;
            int start = colon + 1;
            while (start < json.Length && (json[start] == ' ' || json[start] == '"')) start++;
            if (start + 3 < json.Length && json.Substring(start, 4).ToLower() == "true")
                value = true;
            else if (start + 4 < json.Length && json.Substring(start, 5).ToLower() == "false")
                value = false;
        }

        static void SaveReceivedItems()
        {
            string data = string.Join(";", receivedAPItems);
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
                {"buyHero", buyHeroCount},
                {"gardenBattle", gardenBattleCount}, {"gardenMoney", gardenMoneyChestCount},
                {"gardenArtifact", gardenArtifactChestCount}, {"gardenShop", gardenShopCount}
            };
            string data = string.Join(";", counters.Select(kv => kv.Key + "=" + kv.Value));
            string key = "BrutalAP_Counters_" + (APClient.CurrentSeed ?? "default");
            PlayerPrefs.SetString(key, data);
            PlayerPrefs.Save();
            Debug.Log("AP: Saved counters: " + data);
        }

        public static void SaveSentChecks()
        {
            string key = "BrutalAP_SentChecks_" + (APClient.CurrentSeed ?? "default");
            string data = string.Join(";", sentChecks.ToArray());
            PlayerPrefs.SetString(key, data);
            PlayerPrefs.Save();
            Debug.Log("AP: Saved sent checks: " + data);
        }

        public static void LoadSentChecks()
        {
            string key = "BrutalAP_SentChecks_" + (APClient.CurrentSeed ?? "default");
            if (PlayerPrefs.HasKey(key))
            {
                string data = PlayerPrefs.GetString(key, "");
                if (!string.IsNullOrEmpty(data))
                {
                    string[] checks = data.Split(';');
                    foreach (string c in checks)
                        if (!string.IsNullOrEmpty(c))
                            sentChecks.Add(c);
                    Debug.Log("AP: Loaded sent checks: " + string.Join(", ", checks));
                }
            }
        }

        public static void LoadBossDefeatCount()
        {
            string key = "BrutalAP_BossDefeat_" + (APClient.CurrentSeed ?? "default");
            if (PlayerPrefs.HasKey(key))
                bossDefeatCount = PlayerPrefs.GetInt(key);
        }

        public static void SaveBossDefeatCount()
        {
            string key = "BrutalAP_BossDefeat_" + (APClient.CurrentSeed ?? "default");
            PlayerPrefs.SetInt(key, bossDefeatCount);
            PlayerPrefs.Save();
        }

        public static void FlushPendingChecks()
        {
            while (pendingChecks.Count > 0)
            {
                string loc = pendingChecks.Dequeue();
                SendCheck(loc);
            }
        }

        public static void SendCheck(string locationName)
        {
            if (apClient == null)
            {
                pendingChecks.Enqueue(locationName);
                Debug.Log("AP: Queued check " + locationName);
                return;
            }
            if (sentChecks.Contains(locationName)) return;
            sentChecks.Add(locationName);
            SaveSentChecks();
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

            // Бандлы
            if (itemName == "Winstreak 2 Bundle") {
                foreach (var sub in new[] { "BalticBrine", "Dum-Dum", "ExpiredMedicine", "ForgottenPump", "GentlemensGlove", "LitteringLeaflets", "Soap" })
                    ApplyItem(sub);
                return;
            }
            if (itemName == "Winstreak 3 Bundle") {
                foreach (var sub in new[] { "HomelessHotline", "LilSmiley", "PharmaceuticalRollerCoaster", "UsedDogTags", "WarBond" })
                    ApplyItem(sub);
                return;
            }
            if (itemName == "Winstreak 4 Bundle") {
                foreach (var sub in new[] { "ProfessionalProcrastinator", "Vowbreaker" })
                    ApplyItem(sub);
                return;
            }
            if (itemName == "Winstreak 5 Bundle") {
                foreach (var sub in new[] { "SacrificialSaint", "StarvingApples" })
                    ApplyItem(sub);
                return;
            }
            if (itemName == "HundredPercent Bundle") {
                foreach (var sub in new[] { "HumanCanvas", "EggOfIncubus", "ExquisiteCorpse" })
                    ApplyItem(sub);
                return;
            }

            // Герои
            if (itemName.StartsWith("Hero_"))
            {
                UnlockHeroByAPName(itemName);
                return;
            }

            // Монеты
            if (itemName == "5 Coins" || itemName == "10 Coins" || itemName == "15 Coins")
            {
                AddCoins(itemName == "5 Coins" ? 5 : (itemName == "10 Coins" ? 10 : 15));
                return;
            }

            // Хардмод
            if (itemName == "Hardmode Access")
            {
                IsHardmode = true;
                try
                {
                    var thresholdsProp = typeof(SaveDataManager_2024).GetProperty("Thresholds",
                        System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static);
                    if (thresholdsProp != null)
                    {
                        var thresholds = thresholdsProp.GetValue(null);
                        if (thresholds != null)
                        {
                            var hardModeUnlockedField = thresholds.GetType().GetField("hardModeUnlocked",
                                System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);
                            if (hardModeUnlockedField != null)
                                hardModeUnlockedField.SetValue(thresholds, true);
                        }
                    }

                    var optionsProp = typeof(SaveDataManager_2024).GetProperty("Options",
                        System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static);
                    if (optionsProp != null)
                    {
                        var options = optionsProp.GetValue(null);
                        if (options != null)
                        {
                            var hardModeActiveField = options.GetType().GetField("hardModeActive",
                                System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);
                            if (hardModeActiveField != null)
                                hardModeActiveField.SetValue(options, true);
                        }
                    }

                    var updateMethod = typeof(SaveDataManager_2024).GetMethod("Update_GameOptions",
                        System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static);
                    if (updateMethod != null)
                        updateMethod.Invoke(null, null);

                    Debug.Log("AP: Hardmode unlocked and activated. Start a new run to enter the Garden.");
                }
                catch (Exception e) { Debug.LogError("AP: Failed to activate hardmode: " + e); }
                return;
            }

            // Доступ к зонам и боссы
            if (itemName == "Orpheum Access" || itemName == "Quarry Access" ||
                itemName == "Far Shore Access" || itemName.StartsWith("Boss "))
                return;

            // Предметы (wearables) – через Game.TryUnlockItem
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

                    bool wasNewlyUnlocked = game.TryUnlockItem(unlockID);
                    Debug.Log($"AP: TryUnlockItem('{unlockID}') for '{itemName}' -> newly unlocked: {wasNewlyUnlocked}");
                };

                if (cachedHolder?.Game != null)
                    unlockAction();
                else
                    pendingActions.Enqueue(unlockAction);
            }
            else
            {
                Debug.LogWarning($"AP: No unlockable ID mapping found for item '{itemName}'");
            }
        }

        public static void OnItemReceived(string itemName)
        {
            if (receivedAPItems.Contains(itemName)) return;
            try { ApplyItem(itemName); }
            catch (Exception e) { Debug.LogError($"AP: ApplyItem({itemName}) failed: {e}"); }
            receivedAPItems.Add(itemName);
            SaveReceivedItems();
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

                var fields = type.GetFields(System.Reflection.BindingFlags.Public |
                                            System.Reflection.BindingFlags.NonPublic |
                                            System.Reflection.BindingFlags.Instance);
                foreach (var f in fields)
                {
                    object val;
                    try { val = f.GetValue(obj); }
                    catch (Exception e) { val = $"<error: {e.Message}>"; }
                    Debug.Log($"  Field: {f.FieldType.Name} {f.Name} = {val}");
                }
            }
            if (!found)
                Debug.LogWarning($"AP: DumpWearableObject: '{objectName}' NOT found among {allObjects.Length} loaded objects.");
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

        // ========== ПАТЧИ ==========
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
                    BrutalAPMod.ApplyPendingCoins();
                    BrutalAPMod.ProcessPendingActions();
                }

                RunDataSO run = __instance.Run;
                if (run != null && run.IsCurrentCardType(CardType.Boss))
                {
                    int zone = run.CurrentZoneID;
                    string bossLoc = zone == 0 ? "Far Boss" : (zone == 1 ? "Orp Boss" : "Quarry Boss");
                    BrutalAPMod.SendCheck(bossLoc);
                    if (zone == 2)
                    {
                        BrutalAPMod.bossDefeatCount++;
                        BrutalAPMod.SaveBossDefeatCount();
                        if (BrutalAPMod.IsHardmode)
                            BrutalAPMod.SendCheck($"Garden Boss Defeat {BrutalAPMod.bossDefeatCount}");
                        else
                            BrutalAPMod.SendCheck($"Quarry Boss Defeat {BrutalAPMod.bossDefeatCount}");
                    }
                    return;
                }

                int zoneID = run?.CurrentZoneID ?? -1;
                if (BrutalAPMod.IsHardmode && zoneID == 2)
                {
                    if (BrutalAPMod.gardenBattleCount < BrutalAPMod.gardenBattleLimit)
                    {
                        BrutalAPMod.gardenBattleCount++;
                        BrutalAPMod.SendCheck("Garden_Battle_" + BrutalAPMod.gardenBattleCount);
                    }
                    return;
                }

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
                if (BrutalAPMod.IsHardmode && zone == 2)
                {
                    if (BrutalAPMod.gardenMoneyChestCount >= BrutalAPMod.gardenMoneyLimit) return;
                    BrutalAPMod.gardenMoneyChestCount++;
                    BrutalAPMod.SendCheck("Garden_MoneyChest_" + BrutalAPMod.gardenMoneyChestCount);
                    return;
                }

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
                if (BrutalAPMod.IsHardmode && zone == 2)
                {
                    if (BrutalAPMod.gardenArtifactChestCount >= BrutalAPMod.gardenArtifactLimit) return;
                    BrutalAPMod.gardenArtifactChestCount++;
                    BrutalAPMod.SendCheck("Garden_ArtifactChest_" + BrutalAPMod.gardenArtifactChestCount);
                    return;
                }

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
                if (BrutalAPMod.IsHardmode && zone == 2)
                {
                    if (BrutalAPMod.gardenShopCount >= BrutalAPMod.gardenShopLimit) return;
                    BrutalAPMod.gardenShopCount++;
                    BrutalAPMod.SendCheck("Garden_Shop_" + BrutalAPMod.gardenShopCount);
                    return;
                }

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
                if (nextZone == 2 && !BrutalAPMod.receivedAPItems.Contains("Quarry Access"))
                {
                    Debug.Log("AP BLOCKED: Need Quarry Access");
                    return false;
                }
                return true;
            }
        }

        [HarmonyPatch(typeof(AchievementsManagerData), "TryUnlockAchievement", new Type[] { typeof(Achievement) })]
        class BlockAchievementPatch
        {
            static readonly HashSet<Achievement> questHeroes = new HashSet<Achievement>
            {
                Achievement.ACH_PartyMember_Anton,
                Achievement.ACH_PartyMember_Pearl,
                Achievement.ACH_PartyMember_Burnout
            };

            static bool Prefix(Achievement achievement)
            {
                if (BrutalAPMod.AllowUnlocks)
                    return true;

                if (achievement == Achievement.ACH_Ending_Easy_EnterHardmode)
                {
                    BrutalAPMod.SendCheck("Quarry_Boss_Spared");
                    return true;
                }

                if (questHeroes.Contains(achievement))
                    return true;

                foreach (var kvp in BrutalAPMod.heroAchievementMap)
                {
                    if (kvp.Value == achievement)
                    {
                        if (BrutalAPMod.heroCheckName.TryGetValue(kvp.Key, out string checkName))
                            BrutalAPMod.SendCheck(checkName);
                        return false;
                    }
                }

                // Для предметов больше не обрабатываем – они открываются через Game.TryUnlockItem
                return false;
            }

            static void Postfix(Achievement achievement)
            {
                if (questHeroes.Contains(achievement) && !BrutalAPMod.AllowUnlocks)
                {
                    var achDB = Resources.FindObjectsOfTypeAll<AchievementsManagerData>().FirstOrDefault();
                    if (achDB != null)
                    {
                        var dictField = typeof(AchievementsManagerData).GetField("m_achievementDict",
                            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                        if (dictField != null)
                        {
                            var dict = dictField.GetValue(achDB) as Dictionary<Achievement, Achievement_t>;
                            if (dict != null && dict.TryGetValue(achievement, out var data))
                            {
                                data.m_offlinebAchieved = false;
                            }
                        }
                    }
                    foreach (var kvp in BrutalAPMod.heroAchievementMap)
                        if (kvp.Value == achievement)
                        {
                            BrutalAPMod.SendCheck("Hero_" + kvp.Key);
                            break;
                        }
                }
                if (achievement == Achievement.ACH_Ending_Easy_EnterHardmode && !BrutalAPMod.AllowUnlocks)
                {
                    BrutalAPMod.IsHardmode = true;
                    Debug.Log("AP: Hardmode activated via achievement");
                }
            }
        }

        [HarmonyPatch(typeof(UnlockablesManager), "TryUnlockFromID", new Type[] { typeof(string) })]
        class BlockUnlockPatch
        {
            static bool Prefix(string id)
            {
                if (BrutalAPMod.AllowUnlocks)
                    return true;

                if (id == "Boyle" || id == "Hans")
                    return true;

                if (BrutalAPMod.heroUnlockID.Values.Contains(id))
                {
                    string heroAPName = null;
                    foreach (var kvp in BrutalAPMod.heroUnlockID)
                        if (kvp.Value == id) { heroAPName = kvp.Key; break; }

                    if (BrutalAPMod.heroCheckName.TryGetValue(id, out string checkName))
                        BrutalAPMod.SendCheck(checkName);
                    return false;
                }

                // Для предметов (если вдруг вызовется) – отправляем чек
                BrutalAPMod.SendCheck("Item_" + id);
                return false;
            }
        }

        // Дамп для отладки (можно оставить)
        private static void DumpUnlockableIDs()
        {
            if (cachedHolder?.UnlockableManager == null)
            {
                var um = Resources.FindObjectsOfTypeAll<UnlockablesManager>().FirstOrDefault();
                if (um == null) { Debug.Log("No UnlockablesManager found"); return; }
                var dbField = typeof(UnlockablesManager).GetField("_newUnlockableDB",
                    System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                var db = dbField?.GetValue(um) as UnlockablesDatabase;
                if (db == null) return;
                var byID = typeof(UnlockablesDatabase).GetField("m_ByIDUnlockables",
                    System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);
                var dict = byID?.GetValue(db) as System.Collections.IDictionary;
                if (dict == null) return;
                Debug.Log("=== UNLOCKABLE IDs IN MANAGER ===");
                foreach (System.Collections.DictionaryEntry kv in dict)
                    Debug.Log($"ID: {kv.Key}");
                return;
            }
            var dbField2 = typeof(UnlockablesManager).GetField("_newUnlockableDB",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            var db2 = dbField2?.GetValue(cachedHolder.UnlockableManager) as UnlockablesDatabase;
            if (db2 == null) return;
            var byID2 = typeof(UnlockablesDatabase).GetField("m_ByIDUnlockables",
                System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);
            var dict2 = byID2?.GetValue(db2) as System.Collections.IDictionary;
            if (dict2 == null) return;
            Debug.Log("=== UNLOCKABLE IDs IN MANAGER ===");
            foreach (System.Collections.DictionaryEntry kv in dict2)
                Debug.Log($"ID: {kv.Key}");
        }
    }
}