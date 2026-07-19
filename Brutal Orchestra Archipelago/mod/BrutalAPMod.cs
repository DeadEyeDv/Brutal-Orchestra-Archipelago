using BepInEx;
using HarmonyLib;
using UnityEngine;
using System.Collections.Generic;
using System;
using System.Linq;
using System.Text;

namespace BrutalOrchestraAr
{
    [BepInPlugin("brutal.ap.mod", "Brutal AP Mod", "1.0.0")]
    public class BrutalAPMod : BaseUnityPlugin
    {
        public static APClient apClient;
        public static HashSet<string> receivedAPItems = new HashSet<string>();
        public static HashSet<string> sentChecks = new HashSet<string>();
        public static Queue<string> pendingChecks = new Queue<string>();

        public static int farBattleCount, orpBattleCount, farMoneyChestCount, orpMoneyChestCount,
            farArtifactChestCount, orpArtifactChestCount, farShopCount, orpShopCount, buyHeroCount,
            gardenBattleCount, gardenMoneyChestCount, gardenArtifactChestCount, gardenShopCount, bossDefeatCount;

        public static int farBattleLimit = 15, orpBattleLimit = 15, farMoneyLimit = 2, orpMoneyLimit = 2,
            farArtifactLimit = 2, orpArtifactLimit = 2, shopCountFar = 6, shopCountOrp = 6, bossCount = 3,
            gardenBattleLimit = 10, gardenMoneyLimit = 2, gardenArtifactLimit = 2, gardenShopLimit = 6, startMoneyLevel = 0;

        public static bool AllowUnlocks = false;
        public static bool IsHardmode = false;
        public static bool DeathLinkEnabled = false;
        public static bool suppressDeathLink = false;
        public static bool InCombat = false;
        public static int processedItemIndex = 0;
        public static int winCount = 1;

        public static void UnlockWithAP(Action a) { AllowUnlocks = true; try { a(); } finally { AllowUnlocks = false; } }

        public static GameInformationHolder cachedHolder = null;
        public static Queue<Action> pendingActions = new Queue<Action>();
        private static Queue<int> pendingCoins = new Queue<int>();
        private static Queue<string> pendingUnlocks = new Queue<string>();
        private static Dictionary<string, UnlockableModData> allUnlockableData = new Dictionary<string, UnlockableModData>();
        public static Dictionary<string, long> locationIDs = new Dictionary<string, long>();
        public static Dictionary<long, string> locationIdToName = new Dictionary<long, string>();
        public static Dictionary<int, string> playerIdToName = new Dictionary<int, string>();
        public static Dictionary<int, string> playerIdToGame = new Dictionary<int, string>();
        public static int mySlot = -1;

        public static Dictionary<string, Dictionary<long, string>> dpItems = new Dictionary<string, Dictionary<long, string>>();
        public static Dictionary<string, Dictionary<long, string>> dpLocations = new Dictionary<string, Dictionary<long, string>>();

        private static readonly HashSet<string> transientItems = new HashSet<string> { "5 Coins", "10 Coins", "15 Coins" };

        public static string LookupItemName(long id, int playerSlot)
        {
            if (playerIdToGame.TryGetValue(playerSlot, out string game)
                && dpItems.TryGetValue(game, out var map)
                && map.TryGetValue(id, out string name)) return name;
            if (itemIdToName.TryGetValue(id, out string own)) return own;
            return id.ToString();
        }

        public static string LookupLocationName(long id, int playerSlot)
        {
            if (playerIdToGame.TryGetValue(playerSlot, out string game)
                && dpLocations.TryGetValue(game, out var map)
                && map.TryGetValue(id, out string name)) return name;
            if (locationIdToName.TryGetValue(id, out string own)) return own;
            return id.ToString();
        }

        public static Dictionary<long, string> itemIdToName = new Dictionary<long, string>
        {
            {10000,"Orpheum Access"},{10001,"Quarry Access"},{10002,"Boss 1"},{10003,"Boss 2"},{10004,"Boss 3"},
            {10005,"Anemone Thread"},{10006,"Beads of Something or Other"},{10007,"Blood Thirsty Idol"},
            {10008,"Boonario"},{10009,"Bosch's Fist"},{10010,"Clash of the Bleached"},{10011,"Crown of Thorns"},
            {10012,"Dew-Covered Sticker"},{10013,"Diseased Bodypart"},{10014,"Dried Paintbrush"},{10015,"Eel's Skin"},
            {10016,"Fennel's Blessing"},{10017,"Flesh-Piercing Skewer"},{10018,"Gilded Rage"},{10019,"Golden Gun"},
            {10020,"Heaven-Bound Noose"},{10021,"Idle Hands"},{10022,"Inhalant"},{10023,"Jumper Cables"},
            {10024,"Lucky Charm"},{10025,"Mangled Messiah"},{10026,"Marrow's Reach"},{10027,"Mirror Shard"},
            {10028,"Mithridatism"},{10029,"Mung Moon"},{10030,"Mutually Assured Destruction"},{10031,"Myopia Glasses"},
            {10032,"Padded Gloves"},{10033,"Pile of Dirt"},{10034,"Pox Blanket"},{10035,"Prayer Beads"},
            {10036,"Rabbit's Foot"},{10037,"Roid Rage"},{10038,"Scab-Knuckled Gloves"},{10039,"Sealed Envelope"},
            {10040,"Shattered Amulet"},{10041,"Silk Tourniquet"},{10042,"Soul Contract"},{10043,"Stimpak"},
            {10044,"Strange Beads"},{10045,"The Unfinished Bolt"},{10046,"Thick Gloves"},{10047,"Thorned Vines"},
            {10048,"Tumor"},{10049,"5 Coins"},{10050,"10 Coins"},{10051,"15 Coins"},{10052,"Hardmode Access"},
            {10053,"Quarry Boss Defeat"},{10054,"Garden Boss Defeat"},{10055,"Rib of Eve"},{10056,"Immolated Fairy"},
            {10057,"Skinned Skate"},{10058,"Strange Fruit"},{10059,"Unfortunate Prophecy"},{10060,"Can o' Worms"},
            {10061,"Box of Medals"},{10062,"Wicker Child"},{10063,"Lady Gloves"},{10064,"Convergent Rage"},
            {10065,"The Ideal Form of Trash"},{10066,"Lust Pudding"},{10067,"Someone Else's Face"},
            {10068,"Counterfeit Medal"},{10069,"Consolation Prize"},{10070,"Egg of Firmament"},{10071,"Ascetic Egg"},
            {10072,"Indulgence"},{10073,"The First Born"},{10074,"Dried Paint"},{10075,"Lil Orro"},
            {10076,"Chain of Command"},{10077,"Gump Ming Goa"},{10078,"Ed's Tags"},{10079,"Tape Worm Pills"},
            {10080,"Shard of Nowak"},{10081,"All that is Mortal"},{10082,"Brigade of Dis"},{10083,"The Brand"},
            {10084,"The Human Soul"},{10085,"Idea of Evil"},{10086,"Mini Mordrake"},{10087,"Mung? Unlock"},
            {10088,"Fog's Prescience (NPC)"},{10089,"Conscription Notice"},{10090,"Ichthys"},{10091,"Bloating Coffers"},
            {10092,"Serpent's Head"},{10093,"Caretaker's Cudgel"},{10094,"Howling Log"},{10095,"Tondal's Vision"},
            {10096,"Peg Leg"},{10097,"Demon Core"},{10098,"Harvest and Plenty"},{10099,"Blood Breathing Bomb"},
            {10100,"Man Made Ovum"},{10101,"Hereafter"},{10102,"Ol Reliable"},{10103,"The Rest of Nowak"},
            {10104,"Stillborn Egg"},{10105,"Arachnid Aphrodisiac"},{10106,"Baltic Brine"},{10107,"Dum-Dum"},
            {10108,"Expired Medicine"},{10109,"Forgotten Pump"},{10110,"Gentlemen's Glove"},{10111,"Littering Leaflets"},
            {10112,"Soap?"},{10113,"Homeless Hotline"},{10114,"Lil Smiley"},{10115,"Pharmaceutical Roller Coaster"},
            {10116,"Used Dog Tags"},{10117,"War Bond"},{10118,"Professional Procrastinator"},{10119,"Vowbreaker"},
            {10120,"Sacrificial Saint"},{10121,"Starving Apples"},{10122,"Gift Box!"},{10123,"Defective Rounds"},
            {10124,"Another Dud"},{10125,"Purple Heart"},{10126,"Rorscach Test"},{10127,"Roentgen Rays"},
            {10128,"Health insurance"},{10129,"A Gift?"},{10130,"Rotund Amphibian"},{10131,"Gamified Cephalopod"},
            {10132,"You Can Do It!"},{10133,"Russki Vampire"},{10134,"Extra Stitching"},{10135,"Pain Killers"},
            {10136,"Lycanthrope's Core"},{10137,"Head of Scrybe"},{10138,"Fishing Rod"},{10139,"Effigy of the Mettle Mother"},
            {10140,"Gilded Mirror"},{10141,"Spiked Collar"},{10142,"The Cougar"},{10143,"Someone Else's Wedding Ring"},
            {10144,"Fist Full of Ash"},{10145,"Czech Hedgehog"},{10146,"Cremation"},{10147,"Deworming Pills"},
            {10148,"Medical Leches"},{10149,"Holy Chalice"},{10150,"Seeds of the Consumed"},{10151,"The Jersey"},
            {10152,"Pontiff's Parade"},{10153,"Mystery Ration"},{10154,"Ol' Stumpy"},{10155,"Iron Necklace"},
            {10156,"The Apple"},{10157,"Trepanation"},{10158,"Wheel of Fortune"},{10159,"Prussian Blue"},{10160,"DDT"},
            {10161,"Blind Faith"},{10162,"Modern Medicine"},{10163,"Divine Mud"},{10164,"Opulent Egg"},{10165,"Cloth Cock"},
            {10166,"Sculptur's Tools"},{10167,"Gospel's Severed Head"},{10168,"Wels Catfish"},{10169,"Left Shoe"},
            {10170,"Meatre Worm"},{10171,"Norris!"},{10172,"Burn-Bottle Batch"},{10173,"Royal Pine"},{10174,"Coelacanth"},
            {10175,"Sacred Shrub"},{10176,"Mordrake"},{10177,"Faulty Land Mine"},{10178,"Bronzo's 2 Cents (Item)"},
            {10179,"Counterfeit coins"},{10180,"Bananas"},{10181,"Bronzo's Stupid Hat"},{10182,"Broken Doll"},
            {10183,"Infernal Eye"},{10184,"Vyacheslav's Last Sip"},{10185,"Wailing Whistle"},{10186,"Cursed Sword"},
            {10187,"Enigma"},{10188,"The Master's Sickle"},{10189,"Esoteric Artifact"},{10190,"Mordrake's Untold Tale"},
            {10191,"Winstreak 2 Bundle"},{10192,"Winstreak 3 Bundle"},{10193,"Winstreak 4 Bundle"},
            {10194,"Winstreak 5 Bundle"},{10195,"HundredPercent Bundle"},
            {10196, "The Gambler"}, {10197, "The Conjoined"}, {10198, "The Glutton"},
            {10199, "The Trickster"}, {10200, "The Naked"}, {10201, "The Failure"},
            {10202, "The Incinerated"}, {10203, "The Parasite"}, {10204, "The Stoic"},
            {10205, "The Zealot"}, {10206, "The Corpse"}, {10207, "The Terminal"},
            {10208, "The Psychic"}, {10209, "The Black Lung"}, {10210, "The Mass"},
            {10211, "The Magnum Opus"}, {10212, "The Immortal"}, {10213, "The Sadist"},
            {10214, "The Impaled"}, {10215, "The Mistake"}, {10216, "The Emotional Disaster"},
            {10217, "The Exhumed"}, {10218, "Progressive Start Money"}, 
            {10219, "Human Canvas"}, {10220, "Egg of Incubus"}, {10221, "Exquisite Corpse"}
        };

        public static Dictionary<string, string> itemNameToUnlockableID = new Dictionary<string, string>
        {
            {"Rib of Eve","RibOfEve_TW"},{"Immolated Fairy","ImmolatedFairy_TW"},{"Skinned Skate","SkinnedSkate_TW"},
            {"Strange Fruit","StrangeFruit_TW"},{"Unfortunate Prophecy","UnfortunateProphecy_TW"},
            {"Can o' Worms","CanOfWorms_SW"},{"Box of Medals","BoxOfMedals_SW"},{"Wicker Child","WickerChild_TW"},
            {"Lady Gloves","LadyGloves_SW"},{"Convergent Rage","ConvergentRage_TW"},
            {"The Ideal Form of Trash","TheIdealFormOfTrash_TW"},{"Lust Pudding","LustPudding_TW"},
            {"Someone Else's Face","SomeoneElsesFace_SW"},{"Counterfeit Medal","CounterfeitMedal_SW"},
            {"Consolation Prize","ConsolationPrize_SW"},{"Egg of Firmament","EggOfFirmament_TW"},
            {"Ascetic Egg","AsceticEgg_TW"},{"Indulgence","Indulgence_TW"},{"The First Born","TheFirstBorn_TW"},
            {"Dried Paint","DriedPaint_SW"},{"Lil Orro","LilOrro_TW"},{"Chain of Command","ChainofCommand_SW"},
            {"Gump Ming Goa","GumpMingGoa_TW"},{"Ed's Tags","EdsTags_SW"},{"Tape Worm Pills","TapeWormPills_SW"},
            {"Shard of Nowak","ShardOfNowak_TW"},{"All that is Mortal","AllThatIsMortal_TW"},
            {"Brigade of Dis","BrigadeOfDis_TW"},{"The Brand","TheBrand_TW"},{"The Human Soul","TheHumanSoul_TW"},
            {"Idea of Evil","IdeaOfEvil_TW"},{"Mini Mordrake","MiniMordrake_TW"},
            {"Conscription Notice","ConscriptionNotice_SW"},{"Ichthys","Ichthys_TW"},
            {"Bloating Coffers","BloatingCoffers_TW"},{"Serpent's Head","SerpentsHead_TW"},
            {"Caretaker's Cudgel","CaretakersCudgel_TW"},{"Howling Log","HowlingLong_TW"},
            {"Tondal's Vision","TondalsVision_TW"},{"Peg Leg","PegLeg_TW"},{"Demon Core","DemonCore_SW"},
            {"Harvest and Plenty","HarvestAndPlenty_TW"},{"Blood Breathing Bomb","BloodBreathingBomb_TW"},
            {"Man Made Ovum","ManMadeOvum_TW"},{"Hereafter","Hereafter_TW"},{"Ol Reliable","OlReliable_SW"},
            {"The Rest of Nowak","TheRestOfNowak_TW"},{"Stillborn Egg","StillbornEgg_TW"},
            {"Arachnid Aphrodisiac","ArachnidAphrodisiac_TW"},{"Gift Box!","GiftBox_SW"},
            {"Defective Rounds","DefectiveRounds_SW"},{"Faulty Land Mine","FaultyLandMine_SW"},
            {"Another Dud","AnotherDud_SW"},{"Purple Heart","PurpleHeart_SW"},{"Rorscach Test","RorschachTest_SW"},
            {"Roentgen Rays","RoentgenRays_SW"},{"Health insurance","HealthInsurance_SW"},{"A Gift?","AGift_TW"},
            {"Rotund Amphibian","RotundAmphibian_TW"},{"Gamified Cephalopod","GamifiedSquid_TW"},
            {"You Can Do It!","YouCanDoIt_SW"},{"Russki Vampire","RusskiVampire_SW"},
            {"Extra Stitching","ExtraStitching_SW"},{"Pain Killers","PainKillers_SW"},
            {"Lycanthrope's Core","LycanthropesCore_TW"},{"Head of Scrybe","HeadOfScribe_TW"},
            {"Fishing Rod","FishingRod_TW"},{"Effigy of the Mettle Mother","EffigyOfTheMettleMother_TW"},
            {"Gilded Mirror","GildedMirror_TW"},{"Spiked Collar","SpikedCollar_TW"},
            {"Someone Else's Wedding Ring","SomeonesWeddingRing_TW"},{"Fist Full of Ash","FistFullOfAsh_TW"},
            {"Czech Hedgehog","CzechHedgehog_SW"},{"Cremation","Cremation_TW"},
            {"Deworming Pills","DewormingPills_SW"},{"Medical Leches","MedicalLeeches_SW"},
            {"Holy Chalice","HolyChalice_TW"},{"Seeds of the Consumed","SeedsOfTheConsumed_TW"},
            {"The Jersey","TheJersey_TW"},{"Pontiff's Parade","PontiffsParade_TW"},{"Mystery Ration","MysteryRation_SW"},
            {"Ol' Stumpy","OlStumpy_SW"},{"Iron Necklace","IronNecklace_SW"},{"The Apple","TheApple_TW"},
            {"Trepanation","Trepanation_TW"},{"Wheel of Fortune","WheelOfFortune_TW"},{"Prussian Blue","PrussianBlue_SW"},
            {"DDT","DDT_SW"},{"Blind Faith","BlindFaith_TW"},{"Modern Medicine","ModernMedicine_SW"},
            {"Divine Mud","DivineMud_TW"},{"Opulent Egg","OpulentEgg_TW"},{"Cloth Cock","ClothCock_SW"},
            {"Sculptur's Tools","SculpturesTools_SW"},{"Gospel's Severed Head","GospelsSeveredHead_TW"},
            {"Wels Catfish","WelsCatfish_ExtraW"},{"Left Shoe","LeftShoe_ExtraW"},{"Meatre Worm","MeatreWorm_TW"},
            {"Norris!","Norris_TW"},{"Burn-Bottle Batch","BurnBottleBatch_SW"},{"Royal Pine","RoyalPine_TW"},
            {"Coelacanth","Coelacanth_ExtraW"},{"Sacred Shrub","SacredShrub_TW"},
            {"Bronzo's 2 Cents (Item)","Bronzos2Cents_SW"},{"Counterfeit coins","CounterfeitCoin_SW"},
            {"Bananas","Bananas_TW"},{"Bronzo's Stupid Hat","BronzosStupidHat_TW"},{"Broken Doll","BrokenDoll_TW"},
            {"Infernal Eye","InfernalEye_TW"},{"Vyacheslav's Last Sip","VyacheslavsLastSip_SW"},
            {"Wailing Whistle","WailingWhistle_SW"},{"Cursed Sword","CursedSword_TW"},{"Enigma","Enigma_TW"},
            {"The Master's Sickle","TheMastersSickle_SW"},{"Esoteric Artifact","EsotericArtifact_SW"},
            {"Baltic Brine","BalticBrine_SW"},{"Dum-Dum","DumDum_SW"},
            {"Expired Medicine","ExpiredMedicine_SW"},{"Forgotten Pump","ForgottenPump_SW"},
            {"Gentlemen's Glove","GentlemensGlove_SW"},{"Littering Leaflets","LitteringLeaflets_SW"},
            {"Soap?","Soap_SW"},{"Homeless Hotline","HomelessHotline_SW"},
            {"Lil Smiley","LilSmiley_SW"},{"Pharmaceutical Roller Coaster","PharmaceuticalRollerCoaster_SW"},
            {"Used Dog Tags","UsedDogTags_SW"},{"War Bond","WarBond_SW"},
            {"Professional Procrastinator","ProfessionalProcrastinator_TW"},{"Vowbreaker","Vowbreaker_SW"},
            {"Sacrificial Saint","SacrificialSaint_TW"},{"Starving Apples","StarvingApples_TW"},
            {"Human Canvas","HumanCanvas_TW"},{"Egg of Incubus","EggOfIncubus_TW"},
            {"Exquisite Corpse","ExquisiteCorpse_TW"}
        };

        public static readonly Dictionary<string, string> heroItemToCharacterID = new Dictionary<string, string>
        {
            {"The Gambler","Anton_CH"},{"The Conjoined","Splig_CH"},{"The Glutton","Pearl_CH"},
            {"The Trickster","Thype_CH"},{"The Naked","Griffin_CH"},{"The Failure","Arnold_CH"},
            {"The Incinerated","Dimitri_CH"},{"The Parasite","LongLiver_CH"},{"The Stoic","Clive_CH"},
            {"The Zealot","Kleiver_CH"},{"The Corpse","Cranes_CH"},{"The Terminal","Agon_CH"},
            {"The Psychic","Rags_CH"},{"The Black Lung","SmokeStacks_CH"},{"The Mass","Leviat_CH"},
            {"The Magnum Opus","Gospel_CH"},{"The Immortal","Bimini_CH"},{"The Sadist","Burnout_CH"},
            {"The Impaled","Fennec_CH"},{"The Mistake","Mordrake_CH"},{"The Emotional Disaster","Mung_CH"},
            {"The Cougar","ShellyK_CH"},{"The Exhumed","Formosus_CH"}
        };

        public static readonly Dictionary<string, string> heroCheckName = new Dictionary<string, string>
        {
            {"Anton","The Gambler"},{"Splig","The Conjoined"},{"Pearl","The Glutton"},{"Thype","The Trickster"},
            {"Griffin","The Naked"},{"Arnold","The Failure"},{"Dimitri","The Incinerated"},{"LongLiver","The Parasite"},
            {"Clive","The Stoic"},{"Kleiver","The Zealot"},{"Cranes","The Corpse"},{"Agon","The Terminal"},
            {"Rags","The Psychic"},{"SmokeStacks","The Black Lung"},{"Leviat","The Mass"},{"Gospel","The Magnum Opus"},
            {"Bimini","The Immortal"},{"Burnout","The Sadist"},{"Fennec","The Impaled"},{"Mordrake","The Mistake"},
            {"Mung","The Emotional Disaster"},{"ShellyK","The Cougar"},{"Formosus","The Exhumed"}
        };

        public static readonly Dictionary<string, string> itemCheckNames = new Dictionary<string, string>
        {
            {"Roids","The Juggernaut"},{"Hickory","The Fire and Flames"},{"Mobius","The Widower"},
            {"TheOuroboros","The Leviathan"},{"Smoothskin","The Orphan"},{"FarShore","Beyond the Dunes"},
            {"Orpheum","Above the Mountains"},{"ZoneExplorer","Thorough Explorer"},{"BossSlap","Throw Hands"},
            {"RoidsMissTurn","Heavyweight Champion"},{"SmoothskinTrauma","Emotional and Physical Annihilation"},
            {"DontAct","Nary a Finger Lifted"},{"FoolsDepleted","Human Resources"},{"ShopDepleted","Deep Pockets"},
            {"Ending_GameOver","It Happens to the Best of Us"},{"LoseToBoss","Honest Effort"},
            {"ManaCostDeath","Tactical Miscalculation"},{"OverflowDeath","Drowning in Pigment"},
            {"OrroSmooch","Smooch!"},{"Ending_CorpseKill","The End?"},{"SurviveStarvation","Swallowed by the Sea"},
            {"Over100Coins","Capital is King"},{"OneTurnHardKill","Brutality"},
            {"FriendlyFireDeath","What are You Doing?"},{"ParasiteJourney","So Long Liver"},
            {"Ending_CorpseSave","I'll Make You Regret This"},{"Ending_HardMode","All that is Mortal"},
            {"TriggerFingers","The Coward"},{"Charcarrion","The Messiah"},{"OsmanSinnoks","The Witness"},
            {"Heaven","The Divine"},{"AllBosses","Kingslayer"},
            {"Ichor1","Ichor's Last Wish"},{"Ichor2","Ichor's Last Wish"},{"Ichor3","Ichor's Last Wish"},{"Ichor4","Ichor's Last Wish"},
            {"Fogs1","Fog's Prescience"},{"Fogs2","Fog's Prescience"},{"Fogs3","Fog's Prescience"},
            {"UngodEmissary","The Ungod's Demand"},{"Garden","Within Yourself"},
            {"FarShoreNoCasualtiesHard","Duke of the Dunes"},{"OrpheumNoCasualtiesHard","Master of the Mountains"},
            {"GardenNoCasualtiesHard","Garden of Earthly Delights"},{"AllZonesNoCasualtiesHard","The Work of an Artists"},
            {"WorldExplorer","Every Stone Tuned"},{"AllBossesSlap","God of Phalanges, Palms and Pain"},
            {"BasicSpeedrun","Decisive and Concise"},{"UngodKill","God is Dead and We Have Killed Him"},
            {"SepulchreKill","Somebody Call the Vatican"},{"XiphactinusKill","Bit off More Than You Can Chew"},
            {"UnfinishedHeirKill","Bloodline Drinker"},{"CharcarrionDecomposition","Crisis of Faith"},
            {"NoNowakHardEnd","Plot Armor"},{"NowakLoneSurvivor","Worthy Successor"},
            {"HeavenDoubleSacrifice","The Second Coming"},{"CasualtiesCharacter5","Month of Funerals"},
            {"CasualtiesEnemy30","Mass Grave Matters"},{"HeadshotDeath","Boom, Headshot"},
            {"DamageDealt100","War Criminal"},{"AntonSad","Plenty of Fish in the Desert"},
            {"Boyle_Osman","Another Dud"},{"Boyle_Heaven","Purple Heart"},{"Hans_Osman","Rorscach Test"},
            {"Hans_Heaven","Roentgen Rays"},{"Burnout_Osman","Health Insurance"},{"Burnout_Heaven","A Gift?"},
            {"Fennec_Osman","Rotund Amphibian"},{"Fennec_Heaven","Gamified Cephalopod"},
            {"Anton_Osman","You Can Do It!"},{"Anton_Heaven","Russki Vampire"},{"Splig_Osman","Extra Stitching"},
            {"Splig_Heaven","Pain Killers"},{"Pearl_Osman","Lycanthrope's Core"},{"Pearl_Heaven","Head of Scrybe"},
            {"Thype_Osman","Fishing Rod"},{"Thype_Heaven","Effigy of the Mettle Mother"},
            {"Griffin_Osman","Gilded Mirror"},{"Griffin_Heaven","Spiked Collar"},
            {"Arnold_Osman","Someone Else's Wedding Ring"},{"Arnold_Heaven","Fist Full of Ash"},
            {"Dimitri_Osman","Czech Hedgehog"},{"Dimitri_Heaven","Cremation"},
            {"LongLiver_Osman","Deworming Pills"},{"LongLiver_Heaven","Medical Leches"},
            {"Clive_Osman","Holy Chalice"},{"Clive_Heaven","Seeds of the Consumed"},{"Kleiver_Osman","The Jersey"},
            {"Kleiver_Heaven","Pontiff's Parade"},{"Cranes_Osman","Mystery Ration"},{"Cranes_Heaven","Ol' Stumpy"},
            {"Agon_Osman","Iron Necklace"},{"Agon_Heaven","The Apple"},{"Rags_Osman","Trepanation"},
            {"Rags_Heaven","Wheel of Fortune"},{"SmokeStacks_Osman","Prussian Blue"},{"SmokeStacks_Heaven","DDT"},
            {"Leviat_Osman","Blind Faith"},{"Leviat_Heaven","Modern Medicine"},{"Bimini_Osman","Divine Mud"},
            {"Bimini_Heaven","Opulent Egg"},{"Gospel_Osman","Sculptur's Tools"},{"Gospel_Heaven","Gospel's Severed Head"},
            {"Mung_Osman","Wels Catfish"},{"Mung_Heaven","Left Shoe"},{"Mordrake_Osman","Meatre Worm"},
            {"Mordrake_Heaven","Norris!"},{"ShellyK_Osman","Burn-Bottle Batch"},{"ShellyK_Heaven","Royal Pine"},
            {"Formosus_Osman","Coelacanth"},{"Formosus_Heaven","Sacred Shrub"},{"ProdigalFlee","Fear of Gods Above"},
            {"MordrakeCH","The Mistake"},
            {"Bronzo1","Bronzo's 2 Cents"},{"Bronzo2","What the !@#$ Nowak?"},{"BronzoBossPhase1","What the !@#$ Nowak?"},
            {"Bronzo3","Okay Nowak, Seriously Stop!"},{"BronzoBossPhase2","Okay Nowak, Seriously Stop!"},
            {"Bronzo4","That's it Nowak!"},{"BronzoBossPhase3","That's it Nowak!"},{"Bronzo5","Time to Die!"},
            {"BronzoBossPhase4","Time to Die!"},{"Bronzo6","The Shyster"},{"BronzoBoss","The Shyster"},
            {"Mordrake1","Mordrake's Untold Tale"},{"Mordrake2","Mordrake's Untold Tale"},
            {"Mordrake3","Mordrake's Untold Tale"},{"Mordrake4","Mordrake's Untold Tale"},
            {"Mordrake5","Mordrake's Untold Tale"},{"Mordrake6","Mordrake's Untold Tale"},
            {"VHSTask0","The Director's Final Frame"},{"VHSTask1","The Director's Final Frame"},
            {"VHSTask2","The Director's Final Frame"},{"VHSTask3","The Director's Final Frame"},
            {"VHSTask4","The Director's Final Frame"},{"VHSTask5","The Director's Final Frame"},
            {"VHSTask6","The Director's Final Frame"},
            {"Winstreak2","Dumb Luck"},{"Winstreak3","Notable Skill"},{"Winstreak4","Burgeoning Expertise"},
            {"Winstreak5","Total and Absolute Mastery"},{"HundredPercent","Brutal Orchestra"}
        };

        public static readonly Dictionary<Achievement, string> achievementCheckMap = new Dictionary<Achievement, string>
        {
            {Achievement.ACH_PartyMember_Anton,"The Gambler"},{Achievement.ACH_PartyMember_Splig,"The Conjoined"},
            {Achievement.ACH_PartyMember_Pearl,"The Glutton"},{Achievement.ACH_PartyMember_Thype,"The Trickster"},
            {Achievement.ACH_PartyMember_Griffin,"The Naked"},{Achievement.ACH_PartyMember_Arnold,"The Failure"},
            {Achievement.ACH_PartyMember_Dimitri,"The Incinerated"},{Achievement.ACH_PartyMember_LongLiver,"The Parasite"},
            {Achievement.ACH_PartyMember_Clive,"The Stoic"},{Achievement.ACH_PartyMember_Kleiver,"The Zealot"},
            {Achievement.ACH_PartyMember_Cranes,"The Corpse"},{Achievement.ACH_PartyMember_Agon,"The Terminal"},
            {Achievement.ACH_PartyMember_Rags,"The Psychic"},{Achievement.ACH_PartyMember_SmokeStacks,"The Black Lung"},
            {Achievement.ACH_PartyMember_Leviat,"The Mass"},{Achievement.ACH_PartyMember_Gospel,"The Magnum Opus"},
            {Achievement.ACH_PartyMember_Bimini,"The Immortal"},{Achievement.ACH_PartyMember_Burnout,"The Sadist"},
            {Achievement.ACH_PartyMember_Fennec,"The Impaled"},{Achievement.ACH_PartyMember_Mordrake,"The Mistake"},
            {Achievement.ACH_PartyMember_Mung,"The Emotional Disaster"},{Achievement.ACH_PartyMember_ShellyK,"The Cougar"},
            {Achievement.ACH_PartyMember_Formosus,"The Exhumed"},
            {Achievement.ACH_Area_1,"Beyond the Dunes"},{Achievement.ACH_Area_2,"Above the Mountains"},
            {Achievement.ACH_Area_3,"Within Yourself"},
            {Achievement.ACH_Area_1_Perfect,"Duke of the Dunes"},{Achievement.ACH_Area_2_Perfect,"Master of the Mountains"},
            {Achievement.ACH_Area_3_Perfect,"Garden of Earthly Delights"},{Achievement.ACH_Area_All_Perfect,"The Work of an Artists"},
            {Achievement.ACH_Area_FullZoneExplore,"Thorough Explorer"},{Achievement.ACH_Area_FullWorldExplore,"Every Stone Tuned"},
            {Achievement.ACH_Ending_Easy_CorpseKill,"The End?"},{Achievement.ACH_Ending_Hard,"All that is Mortal"},
            {Achievement.ACH_Boss_All_Main,"Kingslayer"},{Achievement.ACH_Boss_Roids,"The Juggernaut"},
            {Achievement.ACH_Boss_TriggerFingers,"The Coward"},{Achievement.ACH_Boss_Hickory,"The Fire and Flames"},
            {Achievement.ACH_Boss_Mobius,"The Widower"},{Achievement.ACH_Boss_Ouroboros,"The Leviathan"},
            {Achievement.ACH_Boss_Charcarrion,"The Messiah"},{Achievement.ACH_Boss_Smoothskin,"The Orphan"},
            {Achievement.ACH_Boss_Osman,"The Witness"},{Achievement.ACH_Boss_Heaven,"The Divine"},
            {Achievement.ACH_Boss_Bronzo,"The Shyster"},
            {Achievement.ACH_NPC_Ichor,"Ichor's Last Wish"},{Achievement.ACH_NPC_Bronzo,"Bronzo's 2 Cents"},
            {Achievement.ACH_NPC_Fogs,"Fog's Prescience"},{Achievement.ACH_NPC_Mordrake,"Mordrake's Untold Tale"},
            {Achievement.ACH_NPC_Emissary,"The Ungod's Demand"},{Achievement.ACH_NPC_Dollmaster,"The Director's Final Frame"},
            {Achievement.ACH_Unlock_Boyle_Osman,"Another Dud"},{Achievement.ACH_Unlock_Boyle_Heaven,"Purple Heart"},
            {Achievement.ACH_Unlock_Hans_Osman,"Rorscach Test"},{Achievement.ACH_Unlock_Hans_Heaven,"Roentgen Rays"},
            {Achievement.ACH_Unlock_Anton_Osman,"You Can Do It!"},{Achievement.ACH_Unlock_Anton_Heaven,"Russki Vampire"},
            {Achievement.ACH_Unlock_Splig_Osman,"Extra Stitching"},{Achievement.ACH_Unlock_Splig_Heaven,"Pain Killers"},
            {Achievement.ACH_Unlock_Pearl_Osman,"Lycanthrope's Core"},{Achievement.ACH_Unlock_Pearl_Heaven,"Head of Scrybe"},
            {Achievement.ACH_Unlock_Thype_Osman,"Fishing Rod"},{Achievement.ACH_Unlock_Thype_Heaven,"Effigy of the Mettle Mother"},
            {Achievement.ACH_Unlock_Griffin_Osman,"Gilded Mirror"},{Achievement.ACH_Unlock_Griffin_Heaven,"Spiked Collar"},
            {Achievement.ACH_Unlock_Arnold_Osman,"Someone Else's Wedding Ring"},{Achievement.ACH_Unlock_Arnold_Heaven,"Fist Full of Ash"},
            {Achievement.ACH_Unlock_Dimitri_Osman,"Czech Hedgehog"},{Achievement.ACH_Unlock_Dimitri_Heaven,"Cremation"},
            {Achievement.ACH_Unlock_Longliver_Osman,"Deworming Pills"},{Achievement.ACH_Unlock_Longliver_Heaven,"Medical Leches"},
            {Achievement.ACH_Unlock_Clive_Osman,"Holy Chalice"},{Achievement.ACH_Unlock_Clive_Heaven,"Seeds of the Consumed"},
            {Achievement.ACH_Unlock_Kleiver_Osman,"The Jersey"},{Achievement.ACH_Unlock_Kleiver_Heaven,"Pontiff's Parade"},
            {Achievement.ACH_Unlock_Cranes_Osman,"Mystery Ration"},{Achievement.ACH_Unlock_Cranes_Heaven,"Ol' Stumpy"},
            {Achievement.ACH_Unlock_Agon_Osman,"Iron Necklace"},{Achievement.ACH_Unlocked_Agon_Heaven,"The Apple"},
            {Achievement.ACH_Unlocked_Rags_Osman,"Trepanation"},{Achievement.ACH_Unlocked_Rags_Heaven,"Wheel of Fortune"},
            {Achievement.ACH_Unlocked_SmokeStacks_Osman,"Prussian Blue"},{Achievement.ACH_Unlocked_SmokeStacks_Heaven,"DDT"},
            {Achievement.ACH_Unlocked_Leviat_Osman,"Blind Faith"},{Achievement.ACH_Unlocked_Leviat_Heaven,"Modern Medicine"},
            {Achievement.ACH_Unlocked_Gospel_Osman,"Sculptur's Tools"},{Achievement.ACH_Unlocked_Gospel_Heaven,"Gospel's Severed Head"},
            {Achievement.ACH_Unlocked_Bimini_Osman,"Divine Mud"},{Achievement.ACH_Unlocked_Bimini_Heaven,"Opulent Egg"},
            {Achievement.ACH_Unlocked_Burnout_Osman,"Health Insurance"},{Achievement.ACH_Unlocked_Burnout_Heaven,"A Gift?"},
            {Achievement.ACH_Unlocked_Fennec_Osman,"Rotund Amphibian"},{Achievement.ACH_Unlocked_Fennec_Heaven,"Gamified Cephalopod"},
            {Achievement.ACH_Unlocked_Mordrake_Osman,"Meatre Worm"},{Achievement.ACH_Unlocked_Mordrake_Heaven,"Norris!"},
            {Achievement.ACH_Unlocked_Mung_Osman,"Wels Catfish"},{Achievement.ACH_Unlocked_Mung_Heaven,"Left Shoe"},
            {Achievement.ACH_Unlocked_ShellyK_Osman,"Burn-Bottle Batch"},{Achievement.ACH_Unlocked_ShellyK_Heaven,"Royal Pine"},
            {Achievement.ACH_Unlocked_Formosus_Osman,"Coelacanth"},{Achievement.ACH_Unlocked_Formosus_Heaven,"Sacred Shrub"},
            {Achievement.ACH_100Percent,"Brutal Orchestra"},
            {Achievement.ACH_Misc_UngodKill,"God is Dead and We Have Killed Him"},
            {Achievement.ACH_Misc_SlapBossKill,"Throw Hands"},{Achievement.ACH_Misc_SlapBossKillAll,"God of Phalanges, Palms and Pain"},
            {Achievement.ACH_Misc_Headshot,"Boom, Headshot"},{Achievement.ACH_Misc_Trauma,"Emotional and Physical Annihilation"},
            {Achievement.ACH_MISC_Decomposition,"Crisis of Faith"},{Achievement.ACH_MISC_NoNowak,"Plot Armor"},
            {Achievement.ACH_MISC_Overflow,"Drowning in Pigment"},{Achievement.ACH_MISC_FriendlyFire,"What are You Doing?"},
            {Achievement.ACH_Misc_WrongPigment,"Tactical Miscalculation"},{Achievement.ACH_Misc_Lose,"It Happens to the Best of Us"},
            {Achievement.ACH_Misc_Speedrun,"Decisive and Concise"},{Achievement.ACH_Misc_OuroKiss,"Smooch!"},
            {Achievement.ACH_Misc_UnfinishedHeirKill,"Bloodline Drinker"},{Achievement.ACH_Misc_RoidsCancel,"Heavyweight Champion"},
            {Achievement.ACH_Misc_NowakSurviveOsman,"Worthy Successor"},{Achievement.ACH_Misc_HeavenRebirth,"The Second Coming"},
            {Achievement.ACH_Misc_BuyAllShop,"Deep Pockets"},{Achievement.ACH_Misc_BuyAllParty,"Human Resources"},
            {Achievement.ACH_Misc_HardKill,"Brutality"},{Achievement.ACH_Misc_DontAct,"Nary a Finger Lifted"},
            {Achievement.ACH_Misc_LoseLongLiver,"So Long Liver"},{Achievement.ACH_Misc_100DamageDealt,"War Criminal"},
            {Achievement.ACH_Misc_SurviveStarvation,"Swallowed by the Sea"},
            {Achievement.ACH_Misc_Winstreak2,"Dumb Luck"},{Achievement.ACH_Misc_Winstreak3,"Notable Skill"},
            {Achievement.ACH_Misc_Winstreak4,"Burgeoning Expertise"},{Achievement.ACH_Misc_Winstreak5,"Total and Absolute Mastery"},
            {Achievement.ACH_Misc_100Coins,"Capital is King"},{Achievement.ACH_Misc_LoseToBoss,"Honest Effort"},
            {Achievement.ACH_Misc_Bronzo_1,"What the !@#$ Nowak?"},{Achievement.ACH_Misc_Bronzo_2,"Okay Nowak, Seriously Stop!"},
            {Achievement.ACH_Misc_Bronzo_3,"That's it Nowak!"},{Achievement.ACH_Misc_Bronzo_4,"Time to Die!"},
            {Achievement.ACH_Misc_SepulchreKill,"Somebody Call the Vatican"},{Achievement.ACH_Misc_XiphactinusKill,"Bit off More Than You Can Chew"},
            {Achievement.ACH_Misc_Casualties_CH_5,"Month of Funerals"},{Achievement.ACH_Misc_Casualties_EN_30,"Mass Grave Matters"},
            {Achievement.ACH_Misc_Prodigal_Flee,"Fear of Gods Above"},{Achievement.ACH_Misc_Anton_Sad,"Plenty of Fish in the Desert"}
        };

        private static readonly string[] allUnlockIDs = {
            "Roids","Hickory","Mobius","TheOuroboros","Smoothskin","FarShore","Orpheum","ZoneExplorer","BossSlap",
            "RoidsMissTurn","SmoothskinTrauma","DontAct","FoolsDepleted","ShopDepleted","Ending_GameOver","LoseToBoss",
            "ManaCostDeath","OverflowDeath","OrroSmooch","Ending_CorpseKill","SurviveStarvation","Over100Coins",
            "OneTurnHardKill","FriendlyFireDeath","ParasiteJourney","Ending_CorpseSave","Ending_HardMode",
            "TriggerFingers","Charcarrion","OsmanSinnoks","Heaven","AllBosses","Ichor1","Ichor2","Ichor3","Ichor4",
            "Fogs1","Fogs2","Fogs3","UngodEmissary","Garden","FarShoreNoCasualtiesHard","OrpheumNoCasualtiesHard",
            "GardenNoCasualtiesHard","AllZonesNoCasualtiesHard","WorldExplorer","AllBossesSlap","BasicSpeedrun",
            "UngodKill","SepulchreKill","XiphactinusKill","UnfinishedHeirKill","CharcarrionDecomposition",
            "NoNowakHardEnd","NowakLoneSurvivor","HeavenDoubleSacrifice","CasualtiesCharacter5","CasualtiesEnemy30",
            "HeadshotDeath","DamageDealt100","AntonSad",
            "Boyle_Osman","Boyle_Heaven","Hans_Osman","Hans_Heaven","Burnout_Osman","Burnout_Heaven","Fennec_Osman",
            "Fennec_Heaven","Anton_Osman","Anton_Heaven","Splig_Osman","Splig_Heaven","Pearl_Osman","Pearl_Heaven",
            "Thype_Osman","Thype_Heaven","Griffin_Osman","Griffin_Heaven","Arnold_Osman","Arnold_Heaven",
            "Dimitri_Osman","Dimitri_Heaven","LongLiver_Osman","LongLiver_Heaven","Clive_Osman","Clive_Heaven",
            "Kleiver_Osman","Kleiver_Heaven","Cranes_Osman","Cranes_Heaven","Agon_Osman","Agon_Heaven","Rags_Osman",
            "Rags_Heaven","SmokeStacks_Osman","SmokeStacks_Heaven","Leviat_Osman","Leviat_Heaven","Gospel_Osman",
            "Gospel_Heaven","Bimini_Osman","Bimini_Heaven","Mung_Osman","Mung_Heaven","Mordrake_Osman",
            "Mordrake_Heaven","ShellyK_Osman","ShellyK_Heaven","Formosus_Osman","Formosus_Heaven","ProdigalFlee",
            "MordrakeCH",
            "Bronzo1","Bronzo2","BronzoBossPhase1","Bronzo3","BronzoBossPhase2","Bronzo4","BronzoBossPhase3",
            "Bronzo5","BronzoBossPhase4","Bronzo6","BronzoBoss",
            "Mordrake1","Mordrake2","Mordrake3","Mordrake4","Mordrake5","Mordrake6",
            "VHSTask0","VHSTask1","VHSTask2","VHSTask3","VHSTask4","VHSTask5","VHSTask6",
            "Winstreak2","Winstreak3","Winstreak4","Winstreak5","HundredPercent"
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

        private const string PrefServer = "BrutalAP_LastServer";
        private const string PrefSlot = "BrutalAP_LastSlot";

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

        void Start()
        {
            SkipTutorial();
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
                chatScroll = GUILayout.BeginScrollView(chatScroll, GUILayout.Height(160));
                var richLabel = new GUIStyle(GUI.skin.label) { richText = true, wordWrap = true };
                lock (chatLock)
                    foreach (var line in chatLines)
                        GUILayout.Label(line, richLabel);
                GUILayout.EndScrollView();

                GUILayout.BeginHorizontal();
                chatInput = GUILayout.TextField(chatInput);
                bool enter = Event.current.type == EventType.KeyDown && Event.current.keyCode == KeyCode.Return;
                if ((GUILayout.Button("Send", GUILayout.Width(60)) || enter) && !string.IsNullOrEmpty(chatInput))
                {
                    apClient?.SendChat(chatInput);
                    chatInput = "";
                }
                GUILayout.EndHorizontal();
            }

            GUI.DragWindow();
        }

        private static string StripScheme(string s)
        {
            foreach (var p in new[] { "wss://", "ws://", "https://", "http://" })
                if (s.StartsWith(p, StringComparison.OrdinalIgnoreCase)) return s.Substring(p.Length);
            return s;
        }

        public static void OnConnectSucceeded()
        {
            isConnecting = false;
            connectStatus = "";
            showGui = false;
            showChat = true;
            FlushPendingChecks();
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

        private static string GetPlayerPrefsKey() =>
            !string.IsNullOrEmpty(APClient.CurrentSeed) ? "BrutalAP_" + APClient.CurrentSeed : "BrutalAP_ReceivedItems";

        public static void LoadReceivedItemsForCurrentSeed()
        {
            string key = GetPlayerPrefsKey();
            if (!PlayerPrefs.HasKey(key)) return;
            string data = PlayerPrefs.GetString(key, "");
            if (string.IsNullOrEmpty(data)) return;
            foreach (var item in data.Split(';'))
                if (!string.IsNullOrEmpty(item)) receivedAPItems.Add(item);
            Debug.Log("AP: Loaded received items for seed " + APClient.CurrentSeed + ": " + data);
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
                int farBat = 24, orpBat = 24, farMon = 2, orpMon = 2, farArt = 2, orpArt = 2,
                    shopFar = 24, shopOrp = 24, bosses = 3, gardenBat = 10, gardenMon = 2, gardenArt = 2, gardenShops = 6;
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
                    ParseInt(slotDataJson, "\"boss_count\"", ref bosses);
                    int winC = 1;
                    ParseInt(slotDataJson, "\"win_count\"", ref winC);
                    winCount = winC;
                    ParseBool(slotDataJson, "\"hardmode\"", ref hardmode);
                    ParseBool(slotDataJson, "\"death_link\"", ref deathLink);
                    if (hardmode)
                    {
                        ParseInt(slotDataJson, "\"garden_battle_count\"", ref gardenBat);
                        ParseInt(slotDataJson, "\"garden_money_chests\"", ref gardenMon);
                        ParseInt(slotDataJson, "\"garden_artifact_chests\"", ref gardenArt);
                        ParseInt(slotDataJson, "\"garden_shop_count\"", ref gardenShops);
                    }
                }

                DeathLinkEnabled = deathLink;
                Debug.Log($"AP: DeathLinkEnabled = {DeathLinkEnabled}");
                if (DeathLinkEnabled) apClient?.SendConnectUpdate(true);
                Debug.Log($"AP: DeathLinkEnabled = {DeathLinkEnabled}");

                farBattleLimit = farBat; orpBattleLimit = orpBat; farMoneyLimit = farMon; orpMoneyLimit = orpMon;
                farArtifactLimit = farArt; orpArtifactLimit = orpArt; shopCountFar = shopFar; shopCountOrp = shopOrp;
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
                for (int i = 1; i <= 4; i++) locationIDs[$"BuyHero_{i}"] = baseId++;

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
                string[] heroNames = {"Anton","Splig","Pearl","Thype","Griffin","Arnold","Dimitri","LongLiver","Clive",
                    "Kleiver","Cranes","Agon","Rags","SmokeStacks","Leviat","Gospel","Bimini","Burnout","Fennec",
                    "Mordrake","Mung","ShellyK","Formosus"};
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
                    for (int i = 1; i <= gardenArtifactLimit; i++) locationIDs[$"Garden_ArtifactChest_{i}"] = gardenArtifactId++;
                    int gardenShopId = 3300;
                    for (int i = 1; i <= gardenShopLimit; i++) locationIDs[$"Garden_Shop_{i}"] = gardenShopId++;
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
                LoadBossDefeatCount();
                LoadProcessedItemIndex();
                LoadReceivedItemsForCurrentSeed();
                InitStartingChecks();

                if (cachedHolder == null)
                    cachedHolder = Resources.FindObjectsOfTypeAll<GameInformationHolder>().FirstOrDefault();
                ApplyPendingCoins();
                ProcessPendingActions();
            }
            catch (Exception e) { Debug.LogError("InitSlotData error: " + e); }
        }

        private static IEnumerable<(string key, UnlockableModData data)> EnumerateUnlockables(UnlockablesManager manager)
        {
            var dbField = typeof(UnlockablesManager).GetField("_newUnlockableDB",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            var unlockableDB = dbField?.GetValue(manager) as UnlockablesDatabase;
            if (unlockableDB == null) yield break;
            var byIDField = typeof(UnlockablesDatabase).GetField("m_ByIDUnlockables",
                System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);
            var dictObj = byIDField?.GetValue(unlockableDB);
            var enumerable = dictObj as System.Collections.IEnumerable
                ?? Traverse.Create(dictObj).Property("Values")?.GetValue() as System.Collections.IEnumerable;
            if (enumerable == null) yield break;

            foreach (var item in enumerable)
            {
                string key = null; UnlockableModData data = null;
                if (item is KeyValuePair<string, UnlockableModData> kv) { key = kv.Key; data = kv.Value; }
                else if (item is UnlockableModData direct) { data = direct; key = Traverse.Create(data).Field("id")?.GetValue() as string; }
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

        public static void LoadCountersForCurrentSeed()
        {
            string key = "BrutalAP_Counters_" + (APClient.CurrentSeed ?? "default");
            if (!PlayerPrefs.HasKey(key)) return;
            string data = PlayerPrefs.GetString(key, "");
            if (string.IsNullOrEmpty(data)) return;
            foreach (var pair in data.Split(';'))
            {
                var parts = pair.Split('=');
                if (parts.Length != 2 || !int.TryParse(parts[1], out int val)) continue;
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

        private static void ParseInt(string json, string key, ref int value)
        {
            int idx = json.IndexOf(key); if (idx == -1) return;
            int colon = json.IndexOf(':', idx); if (colon == -1) return;
            int numStart = colon + 1;
            while (numStart < json.Length && !char.IsDigit(json[numStart])) numStart++;
            int numEnd = numStart;
            while (numEnd < json.Length && char.IsDigit(json[numEnd])) numEnd++;
            if (numEnd > numStart) int.TryParse(json.Substring(numStart, numEnd - numStart), out value);
        }

        private static void ParseBool(string json, string key, ref bool value)
        {
            int idx = json.IndexOf(key); if (idx == -1) return;
            int colon = json.IndexOf(':', idx); if (colon == -1) return;
            int start = colon + 1;
            while (start < json.Length && (json[start] == ' ' || json[start] == '"')) start++;
            if (start + 3 < json.Length && json.Substring(start, 4).ToLower() == "true") value = true;
            else if (start + 4 < json.Length && json.Substring(start, 5).ToLower() == "false") value = false;
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
                {"farShop", farShopCount}, {"orpShop", orpShopCount}, {"buyHero", buyHeroCount},
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
            PlayerPrefs.SetString(key, string.Join(";", sentChecks.ToArray()));
            PlayerPrefs.Save();
        }

        public static void LoadSentChecks()
        {
            string key = "BrutalAP_SentChecks_" + (APClient.CurrentSeed ?? "default");
            if (!PlayerPrefs.HasKey(key)) return;
            string data = PlayerPrefs.GetString(key, "");
            if (string.IsNullOrEmpty(data)) return;
            foreach (var c in data.Split(';'))
                if (!string.IsNullOrEmpty(c)) sentChecks.Add(c);
            Debug.Log("AP: Loaded sent checks: " + data);
        }

        public static void LoadBossDefeatCount()
        {
            string key = "BrutalAP_BossDefeat_" + (APClient.CurrentSeed ?? "default");
            if (PlayerPrefs.HasKey(key)) bossDefeatCount = PlayerPrefs.GetInt(key);
        }

        public static void SaveBossDefeatCount()
        {
            string key = "BrutalAP_BossDefeat_" + (APClient.CurrentSeed ?? "default");
            PlayerPrefs.SetInt(key, bossDefeatCount);
            PlayerPrefs.Save();
        }

        public static void LoadProcessedItemIndex()
        {
            string key = "BrutalAP_ItemIndex_" + (APClient.CurrentSeed);
            if (PlayerPrefs.HasKey(key)) processedItemIndex = PlayerPrefs.GetInt(key);
        }

        public static void SaveProcessedItemIndex()
        {
            if (string.IsNullOrEmpty(APClient.CurrentSeed)) return; // сид ещё не известен — не пишем
            PlayerPrefs.SetInt("BrutalAP_ItemIndex_" + APClient.CurrentSeed, processedItemIndex);
            PlayerPrefs.Save();
        }

        public static void FlushPendingChecks()
        {
            while (pendingChecks.Count > 0) SendCheck(pendingChecks.Dequeue());
        }

        public static void SendCheck(string locationName)
        {
            if (apClient == null) { pendingChecks.Enqueue(locationName); Debug.Log("AP: Queued check " + locationName); return; }
            if (sentChecks.Contains(locationName)) return;
            sentChecks.Add(locationName);
            SaveSentChecks();
            Debug.Log("CHECK: " + locationName);
            apClient.SendCheck(locationName);
            SaveCounters();
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

        private static readonly string[] winstreak2Sub = { "BalticBrine","Dum-Dum","ExpiredMedicine","ForgottenPump","GentlemensGlove","LitteringLeaflets","Soap" };
        private static readonly string[] winstreak3Sub = { "HomelessHotline","LilSmiley","PharmaceuticalRollerCoaster","UsedDogTags","WarBond" };
        private static readonly string[] winstreak4Sub = { "ProfessionalProcrastinator","Vowbreaker" };
        private static readonly string[] winstreak5Sub = { "SacrificialSaint","StarvingApples" };
        private static readonly string[] hundredPercentSub = { "HumanCanvas","EggOfIncubus","ExquisiteCorpse" };

        private static void ApplyItem(string itemName)
        {
            Debug.Log($"AP: Applying item: {itemName}");

            if (heroItemToCharacterID.ContainsKey(itemName)) { UnlockHeroByAPName(itemName); return; }

            if (itemName == "5 Coins" || itemName == "10 Coins" || itemName == "15 Coins")
            {
                AddCoins(itemName == "5 Coins" ? 5 : itemName == "10 Coins" ? 10 : 15);
                return;
            }

            if (itemName == "Hardmode Access")
            {
                IsHardmode = true;
                try
                {
                    var thresholdsProp = typeof(SaveDataManager_2024).GetProperty("Thresholds",
                        System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static);
                    var thresholds = thresholdsProp?.GetValue(null);
                    thresholds?.GetType().GetField("hardModeUnlocked",
                        System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance)?.SetValue(thresholds, true);

                    var optionsProp = typeof(SaveDataManager_2024).GetProperty("Options",
                        System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static);
                    var options = optionsProp?.GetValue(null);
                    options?.GetType().GetField("hardModeActive",
                        System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance)?.SetValue(options, true);

                    typeof(SaveDataManager_2024).GetMethod("Update_GameOptions",
                        System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static)?.Invoke(null, null);

                    Debug.Log("AP: Hardmode unlocked and activated. Start a new run to enter the Garden.");
                }
                catch (Exception e) { Debug.LogError("AP: Failed to activate hardmode: " + e); }
                return;
            }

            if (itemName == "Orpheum Access" || itemName == "Quarry Access" ||
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
                        Debug.Log($"AP: TryUnlockItem('{unlockID}') for '{itemName}' -> newly unlocked: {wasNewlyUnlocked}");
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
            
            if (itemName == "Quarry Boss Defeat" || itemName == "Garden Boss Defeat") return;
            
            if (transientItems.Contains(itemName))
            {
                if (!applyTransient) return;
                try { ApplyItem(itemName); }
                catch (Exception e) { Debug.LogError($"AP: ApplyItem({itemName}) failed: {e}"); }
                return;
            }

            try { ApplyItem(itemName); }
            catch (Exception e) { Debug.LogError($"AP: ApplyItem({itemName}) failed: {e}"); }
            if (!receivedAPItems.Contains(itemName))
            {
                receivedAPItems.Add(itemName);
                SaveReceivedItems();
            }
            
            
        }

        void Update()
        {
            if (pendingDeath && cachedHolder != null)
                TryProcessPendingDeath();
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
                foreach (var f in type.GetFields(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance))
                {
                    object val;
                    try { val = f.GetValue(obj); } catch (Exception e) { val = $"<error: {e.Message}>"; }
                    Debug.Log($"  Field: {f.FieldType.Name} {f.Name} = {val}");
                }
            }
            if (!found) Debug.LogWarning($"AP: DumpWearableObject: '{objectName}' NOT found among {allObjects.Length} loaded objects.");
        }

        public static void InitStartingChecks() => SendCheck("Far Shore Access");

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

        // ========== ПАТЧИ ==========
        [HarmonyPatch(typeof(GameInformationHolder), "PostCombatProcess")]
        class BattlePatch
        {
            static void Postfix(GameInformationHolder __instance)
            {
                if (__instance == null) return;

                InCombat = false;
                if (pendingDeath)
                {
                    TryProcessPendingDeath();
                    return;
                }

                if (cachedHolder != __instance)
                {
                    cachedHolder = __instance;
                    Debug.Log("AP: Holder cached");
                }
                ApplyPendingCoins();
                ProcessPendingActions();

                RunDataSO run = __instance.Run;
                if (run != null && run.IsCurrentCardType(CardType.Boss))
                {
                    int zone = run.CurrentZoneID;
                    SendCheck(zone == 0 ? "Far Boss" : zone == 1 ? "Orp Boss" : "Quarry Boss");
                    if (zone == 2)
                    {
                        bossDefeatCount++;
                        SaveBossDefeatCount();
                        SendCheck(IsHardmode ? $"Garden Boss Defeat {bossDefeatCount}" : $"Quarry Boss Defeat {bossDefeatCount}");
                        if (bossDefeatCount >= winCount) apClient?.SendGoal();
                    }
                    return;
                }

                int zoneID = run?.CurrentZoneID ?? -1;
                if (IsHardmode && zoneID == 2)
                {
                    if (gardenBattleCount < gardenBattleLimit)
                    {
                        gardenBattleCount++;
                        SendCheck("Garden_Battle_" + gardenBattleCount);
                    }
                    return;
                }

                if (zoneID == 0)
                {
                    if (farBattleCount >= farBattleLimit) return;
                    farBattleCount++;
                    SendCheck("Far_Battle_" + farBattleCount);
                }
                else if (zoneID == 1)
                {
                    if (orpBattleCount >= orpBattleLimit) return;
                    orpBattleCount++;
                    SendCheck("Orp_Battle_" + orpBattleCount);
                }
            }
        }

        [HarmonyPatch(typeof(CombatManager), "InitializeCombat")]
        class CombatStartPatch
        {
            static void Postfix() { InCombat = true; }
        }

        [HarmonyPatch(typeof(CombatManager), "ProcessGameOverEnding")]
        class DeathLinkSendPatch
        {
            static void Postfix()
            {
                if (!DeathLinkEnabled) return;
                if (suppressDeathLink) { suppressDeathLink = false; return; }
                string src = playerIdToName.TryGetValue(mySlot, out var n) ? n : "Player";
                apClient?.SendDeathLink(src + "'s party was wiped");
            }
        }

        [HarmonyPatch(typeof(MoneyChestContentData), "OpenTreasure")]
        class MoneyChestPatch
        {
            static void Postfix()
            {
                Debug.Log("AP: MoneyChestPatch triggered");
                if (cachedHolder == null)
                {
                    cachedHolder = Resources.FindObjectsOfTypeAll<GameInformationHolder>().FirstOrDefault();
                }
                if (cachedHolder?.Run == null) { Debug.Log("AP: Holder or Run is null, aborting check"); return; }

                int zone = cachedHolder.Run.CurrentZoneID;
                if (IsHardmode && zone == 2)
                {
                    if (gardenMoneyChestCount >= gardenMoneyLimit) return;
                    gardenMoneyChestCount++;
                    SendCheck("Garden_MoneyChest_" + gardenMoneyChestCount);
                    return;
                }
                if (zone == 0)
                {
                    if (farMoneyChestCount >= farMoneyLimit) return;
                    farMoneyChestCount++;
                    SendCheck("Far_MoneyChest_" + farMoneyChestCount);
                }
                else if (zone == 1)
                {
                    if (orpMoneyChestCount >= orpMoneyLimit) return;
                    orpMoneyChestCount++;
                    SendCheck("Orp_MoneyChest_" + orpMoneyChestCount);
                }
            }
        }

        [HarmonyPatch(typeof(PrizeContentData), "OpenTreasure")]
        class ArtifactChestPatch
        {
            static void Postfix()
            {
                Debug.Log("AP: ArtifactChestPatch triggered");
                if (cachedHolder == null)
                {
                    cachedHolder = Resources.FindObjectsOfTypeAll<GameInformationHolder>().FirstOrDefault();
                }
                if (cachedHolder?.Run == null) { Debug.Log("AP: Holder or Run is null, aborting check"); return; }

                int zone = cachedHolder.Run.CurrentZoneID;
                if (IsHardmode && zone == 2)
                {
                    if (gardenArtifactChestCount >= gardenArtifactLimit) return;
                    gardenArtifactChestCount++;
                    SendCheck("Garden_ArtifactChest_" + gardenArtifactChestCount);
                    return;
                }
                if (zone == 0)
                {
                    if (farArtifactChestCount >= farArtifactLimit) return;
                    farArtifactChestCount++;
                    SendCheck("Far_ArtifactChest_" + farArtifactChestCount);
                }
                else if (zone == 1)
                {
                    if (orpArtifactChestCount >= orpArtifactLimit) return;
                    orpArtifactChestCount++;
                    SendCheck("Orp_ArtifactChest_" + orpArtifactChestCount);
                }
            }
        }

        [HarmonyPatch(typeof(RunDataSO), "BuyFoolCharacter")]
        class BuyFoolPatch
        {
            static void Prefix() => AllowUnlocks = true;
            static void Postfix(CharacterSO __result)
            {
                AllowUnlocks = false;
                if (__result == null || buyHeroCount >= 4) return;
                buyHeroCount++;
                SendCheck("BuyHero_" + buyHeroCount);
            }
        }

        [HarmonyPatch(typeof(RunDataSO), "BuyShopItem")]
        class ShopPatch
        {
            static void Postfix()
            {
                int zone = cachedHolder?.Run?.CurrentZoneID ?? -1;
                if (IsHardmode && zone == 2)
                {
                    if (gardenShopCount >= gardenShopLimit) return;
                    gardenShopCount++;
                    SendCheck("Garden_Shop_" + gardenShopCount);
                    return;
                }
                if (zone == 0)
                {
                    if (farShopCount >= shopCountFar) return;
                    farShopCount++;
                    SendCheck("Shop_Far_" + farShopCount);
                }
                else if (zone == 1)
                {
                    if (orpShopCount >= shopCountOrp) return;
                    orpShopCount++;
                    SendCheck("Shop_Orp_" + orpShopCount);
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
                int nextZone = holder.Run.CurrentZoneID + 1;

                if (nextZone == 1 && !receivedAPItems.Contains("Orpheum Access")) { Debug.Log("AP BLOCKED: Need Orpheum Access"); return false; }
                if (nextZone == 2 && !receivedAPItems.Contains("Quarry Access")) { Debug.Log("AP BLOCKED: Need Quarry Access"); return false; }
                return true;
            }
        }

        [HarmonyPatch(typeof(AchievementsManagerData), "TryUnlockAchievement", new Type[] { typeof(Achievement) })]
        class AchievementCheckPatch
        {
            static void Postfix(Achievement achievement)
            {
                if (AllowUnlocks) return;

                if (achievement == Achievement.ACH_Ending_Easy_EnterHardmode)
                {
                    SendCheck("Quarry_Boss_Spared");
                    SendCheck("I'll Make You Regret This");
                    return;
                }

                if (achievementCheckMap.TryGetValue(achievement, out string checkName))
                    SendCheck(checkName);
            }
        }

        [HarmonyPatch(typeof(UnlockablesManager), "TryUnlockContent")]
        class BlockUnlockContentPatch
        {
            static bool Prefix(UnlockablesManager __instance, UnlockableModData data)
            {
                if (AllowUnlocks) return true;
                if (data == null) return false;

                string id = Traverse.Create(data).Field("id").GetValue() as string;
                if (!string.IsNullOrEmpty(id) && itemCheckNames.TryGetValue(id, out string loc))
                    SendCheck(loc);

                if (data.hasQuestCompletion)
                {
                    var game = Traverse.Create(__instance).Field("_game").GetValue<InGameDataSO>();
                    game?.TryCompleteQuest(data.questID);
                }

                if (data.HasAchievementUnlock)
                {
                    var steamAch = Traverse.Create(__instance).Field("_steamAchievements").GetValue<AchievementsManagerData>();
                    steamAch?.TryUnlockAchievement(data.AchievementID);
                }

                return false;
            }
        }

        [HarmonyPatch(typeof(UnlockablesManager), "TryUnlockHardMode")]
        class BlockHardmodePatch
        {
            static bool Prefix() => AllowUnlocks;
        }

        private static void DumpUnlockableIDs()
        {
            var manager = cachedHolder?.UnlockableManager ?? Resources.FindObjectsOfTypeAll<UnlockablesManager>().FirstOrDefault();
            if (manager == null) { Debug.Log("No UnlockablesManager found"); return; }
            Debug.Log("=== UNLOCKABLE IDs IN MANAGER ===");
            foreach (var (key, _) in EnumerateUnlockables(manager))
                Debug.Log($"ID: {key}");
        }
        
        [HarmonyPatch(typeof(InGameDataSO), "TryUnlockItem")]
        class BlockItemUnlockPatch
        {
            static bool Prefix() => BrutalAPMod.AllowUnlocks; // false = вызов заблокирован
        }

        [HarmonyPatch(typeof(InGameDataSO), "TryUnlockCharacter")]
        class BlockCharacterUnlockPatch
        {
            static bool Prefix() => BrutalAPMod.AllowUnlocks;
        }
        
        [HarmonyPatch(typeof(GameInformationHolder), "PrepareGameRun")]
        class NewRunPatch
        {
            static void Postfix(RunDataSO run)
            {
                if (run?.playerData == null || startMoneyLevel <= 0) return;
                run.playerData.AddCurrency(startMoneyLevel * 10);
                Debug.Log($"AP: Granted start money: {startMoneyLevel * 10}");
            }
        }
    }
}