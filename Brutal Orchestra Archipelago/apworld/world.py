from worlds.AutoWorld import World
from BaseClasses import Item, ItemClassification, Location, Region, LocationProgressType
from .options import BrutalOrchestraOptions, FarBattleCount, OrpBattleCount, \
    FarMoneyChests, OrpMoneyChests, FarArtifactChests, OrpArtifactChests, \
    FarShopCount, OrpShopCount, BossCount, GardenBattleCount, GardenMoneyChests, \
    GardenArtifactChests, GardenShopCount

class BrutalOrchestraWorld(World):
    game = "Brutal Orchestra"

    options_dataclass = BrutalOrchestraOptions
    options: BrutalOrchestraOptions

    item_names = [
        "Orpheum Access", "Quarry Access",
        "Boss 1", "Boss 2", "Boss 3",
        # Standard items (paints removed)
        "Anemone Thread", "Beads of Something or Other", "Blood Thirsty Idol",
        "Boonario", "Bosch's Fist", "Clash of the Bleached", "Crown of Thorns",
        "Dew-Covered Sticker", "Diseased Bodypart", "Dried Paintbrush",
        "Eel's Skin", "Fennel's Blessing", "Flesh-Piercing Skewer",
        "Gilded Rage", "Golden Gun", "Heaven-Bound Noose", "Idle Hands",
        "Inhalant", "Jumper Cables", "Lucky Charm", "Mangled Messiah",
        "Marrow's Reach", "Mirror Shard", "Mithridatism", "Mung Moon",
        "Mutually Assured Destruction", "Myopia Glasses", "Padded Gloves",
        "Pile of Dirt", "Pox Blanket", "Prayer Beads", "Rabbit's Foot",
        "Roid Rage", "Scab-Knuckled Gloves", "Sealed Envelope",
        "Shattered Amulet", "Silk Tourniquet", "Soul Contract", "Stimpak",
        "Strange Beads", "The Unfinished Bolt", "Thick Gloves",
        "Thorned Vines", "Tumor",
        "5 Coins", "10 Coins", "15 Coins",
        "Hardmode Access",
        # Victory items
        "Quarry Boss Defeat", "Garden Boss Defeat",
        # Unlock items (AP progression items that directly unlock game content)
        "Rib of Eve", "Immolated Fairy", "Skinned Skate", "Strange Fruit",
        "Unfortunate Prophecy", "Can o' Worms", "Box of Medals", "Wicker Child",
        "Lady Gloves", "Convergent Rage", "The Ideal Form of Trash", "Lust Pudding",
        "Someone Else's Face", "Counterfeit Medal", "Consolation Prize",
        "Egg of Firmament", "Ascetic Egg", "Indulgence", "The First Born",
        "Dried Paint", "Lil Orro", "Chain of Command", "Gump Ming Goa",
        "Ed's Tags", "Tape Worm Pills", "Shard of Nowak", "All that is Mortal",
        "Brigade of Dis", "The Brand", "The Human Soul", "Idea of Evil",
        "Mini Mordrake", "Mung? Unlock", "Fog's Prescience (NPC)",
        "Conscription Notice", "Ichthys", "Bloating Coffers", "Serpent's Head",
        "Caretaker's Cudgel", "Howling Log", "Tondal's Vision", "Peg Leg",
        "Demon Core", "Harvest and Plenty", "Blood Breathing Bomb",
        "Man Made Ovum", "Hereafter", "Ol Reliable", "The Rest of Nowak",
        "Stillborn Egg", "Arachnid Aphrodisiac",
        "Baltic Brine", "Dum-Dum", "Expired Medicine", "Forgotten Pump",
        "Gentlemen's Glove", "Littering Leaflets", "Soap?",
        "Homeless Hotline", "Lil Smiley", "Pharmaceutical Roller Coaster",
        "Used Dog Tags", "War Bond", "Professional Procrastinator", "Vowbreaker",
        "Sacrificial Saint", "Starving Apples", "Gift Box!", "Defective Rounds",
        "Another Dud", "Purple Heart", "Rorscach Test", "Roentgen Rays",
        "Health insurance", "A Gift?", "Rotund Amphibian", "Gamified Cephalopod",
        "You Can Do It!", "Russki Vampire", "Extra Stitching", "Pain Killers",
        "Lycanthrope's Core", "Head of Scrybe", "Fishing Rod",
        "Effigy of the Mettle Mother", "Gilded Mirror", "Spiked Collar",
        "The Cougar", "Someone Else's Wedding Ring", "Fist Full of Ash",
        "Czech Hedgehog", "Cremation", "Deworming Pills", "Medical Leches",
        "Holy Chalice", "Seeds of the Consumed", "The Jersey",
        "Pontiff's Parade", "Mystery Ration", "Ol' Stumpy", "Iron Necklace",
        "The Apple", "Trepanation", "Wheel of Fortune", "Prussian Blue", "DDT",
        "Blind Faith", "Modern Medicine", "Divine Mud", "Opulent Egg",
        "Cloth Cock", "Sculptur's Tools", "Gospel's Severed Head",
        "Wels Catfish", "Left Shoe", "Meatre Worm", "Norris!",
        "Burn-Bottle Batch", "Royal Pine", "Coelacanth", "Sacred Shrub",
        "Mordrake", "Faulty Land Mine",
        # Bronzo quest items (optional)
        "Bronzo's 2 Cents (Item)", "Counterfeit coins", "Bananas", "Bronzo's Stupid Hat",
        # Director's Cut (VHS) items (optional)
        "Broken Doll", "Infernal Eye", "Vyacheslav's Last Sip", "Wailing Whistle",
        "Cursed Sword", "Enigma", "The Master's Sickle", "Esoteric Artifact",
        # Mordrake's Untold Tale items (optional)
        "Mordrake's Untold Tale",
        # Combined bundles
        "Winstreak 2 Bundle", "Winstreak 3 Bundle", "Winstreak 4 Bundle", "Winstreak 5 Bundle",
        "HundredPercent Bundle",
        # Hero unlock items (achievement names, appended at end to keep existing IDs stable)
        "The Gambler", "The Conjoined", "The Glutton", "The Trickster", "The Naked",
        "The Failure", "The Incinerated", "The Parasite", "The Stoic", "The Zealot",
        "The Corpse", "The Terminal", "The Psychic", "The Black Lung", "The Mass",
        "The Magnum Opus", "The Immortal", "The Sadist", "The Impaled", "The Mistake",
        "The Emotional Disaster", "The Exhumed",
        "Progressive Start Money",
        "Human Canvas", "Egg of Incubus", "Exquisite Corpse"
    ]

    item_name_to_id = {name: 10000 + i for i, name in enumerate(item_names)}

    # All heroes except starters Boyle and Hans
    hero_names = ["Anton","Splig","Pearl","Thype","Griffin","Arnold","Dimitri",
                  "LongLiver","Clive","Kleiver","Cranes","Agon","Rags","SmokeStacks","Leviat",
                  "Gospel","Bimini","Burnout","Fennec","Mordrake","Mung","ShellyK","Formosus"]

    # Unlockable IDs that correspond to achievements
    item_unlock_ids = [
        "Roids","Hickory","Mobius","TheOuroboros","Smoothskin",
        "FarShore","Orpheum","ZoneExplorer","BossSlap","RoidsMissTurn",
        "SmoothskinTrauma","DontAct","FoolsDepleted","ShopDepleted",
        "Ending_GameOver","LoseToBoss","ManaCostDeath","OverflowDeath",
        "OrroSmooch","Ending_CorpseKill","SurviveStarvation",
        "Over100Coins","OneTurnHardKill","FriendlyFireDeath","ParasiteJourney",
        "Ending_CorpseSave",
        # Hardmode
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
        # Character-specific
        "Boyle_Osman","Boyle_Heaven",
        "Hans_Osman","Hans_Heaven",
        "Burnout_Osman","Burnout_Heaven",
        "Fennec_Osman","Fennec_Heaven",
        "Anton_Osman","Anton_Heaven",
        "Splig_Osman","Splig_Heaven",
        "Pearl_Osman","Pearl_Heaven",
        "Thype_Osman","Thype_Heaven",
        "Griffin_Osman","Griffin_Heaven",
        "Arnold_Osman","Arnold_Heaven",
        "Dimitri_Osman","Dimitri_Heaven",
        "LongLiver_Osman","LongLiver_Heaven",
        "Clive_Osman","Clive_Heaven",
        "Kleiver_Osman","Kleiver_Heaven",
        "Cranes_Osman","Cranes_Heaven",
        "Agon_Osman","Agon_Heaven",
        "Rags_Osman","Rags_Heaven",
        "SmokeStacks_Osman","SmokeStacks_Heaven",
        "Leviat_Osman","Leviat_Heaven",
        "Gospel_Osman","Gospel_Heaven",
        "Bimini_Osman","Bimini_Heaven",
        "Mung_Osman","Mung_Heaven",
        "Mordrake_Osman","Mordrake_Heaven",
        "ShellyK_Osman","ShellyK_Heaven",
        "Formosus_Osman","Formosus_Heaven",
        "ProdigalFlee",
        "MordrakeCH",
        # Bronzo (optional)
        "Bronzo1","Bronzo2","BronzoBossPhase1","Bronzo3","BronzoBossPhase2",
        "Bronzo4","BronzoBossPhase3","Bronzo5","BronzoBossPhase4",
        "Bronzo6","BronzoBoss",
        # Mordrake (optional)
        "Mordrake1","Mordrake2","Mordrake3","Mordrake4","Mordrake5","Mordrake6",
        # Director's Cut (optional)
        "VHSTask0","VHSTask1","VHSTask2","VHSTask3","VHSTask4","VHSTask5","VHSTask6",
        # Winstreaks / HundredPercent
        "Winstreak2","Winstreak3","Winstreak4","Winstreak5",
        "HundredPercent"
    ]

    hero_check_names = {
        "Anton": "The Gambler", "Splig": "The Conjoined", "Pearl": "The Glutton",
        "Thype": "The Trickster", "Griffin": "The Naked", "Arnold": "The Failure",
        "Dimitri": "The Incinerated", "LongLiver": "The Parasite", "Clive": "The Stoic",
        "Kleiver": "The Zealot", "Cranes": "The Corpse", "Agon": "The Terminal",
        "Rags": "The Psychic", "SmokeStacks": "The Black Lung", "Leviat": "The Mass",
        "Gospel": "The Magnum Opus", "Bimini": "The Immortal", "Burnout": "The Sadist",
        "Fennec": "The Impaled", "Mordrake": "The Mistake", "Mung": "The Emotional Disaster",
        "ShellyK": "The Cougar", "Formosus": "The Exhumed"
    }

    item_check_names = {
        "Roids": "The Juggernaut",
        "Hickory": "The Fire and Flames",
        "Mobius": "The Widower",
        "TheOuroboros": "The Leviathan",
        "Smoothskin": "The Orphan",
        "FarShore": "Beyond the Dunes",
        "Orpheum": "Above the Mountains",
        "ZoneExplorer": "Thorough Explorer",
        "BossSlap": "Throw Hands",
        "RoidsMissTurn": "Heavyweight Champion",
        "SmoothskinTrauma": "Emotional and Physical Annihilation",
        "DontAct": "Nary a Finger Lifted",
        "FoolsDepleted": "Human Resources",
        "ShopDepleted": "Deep Pockets",
        "Ending_GameOver": "It Happens to the Best of Us",
        "LoseToBoss": "Honest Effort",
        "ManaCostDeath": "Tactical Miscalculation",
        "OverflowDeath": "Drowning in Pigment",
        "OrroSmooch": "Smooch!",
        "Ending_CorpseKill": "The End?",
        "SurviveStarvation": "Swallowed by the Sea",
        "Over100Coins": "Capital is King",
        "OneTurnHardKill": "Brutality",
        "FriendlyFireDeath": "What are You Doing?",
        "ParasiteJourney": "So Long Liver",
        "Ending_CorpseSave": "I'll Make You Regret This",
        "Ending_HardMode": "All that is Mortal",
        "TriggerFingers": "The Coward",
        "Charcarrion": "The Messiah",
        "OsmanSinnoks": "The Witness",
        "Heaven": "The Divine",
        "AllBosses": "Kingslayer",
        "Ichor1": "Ichor's Last Wish", "Ichor2": "Ichor's Last Wish", "Ichor3": "Ichor's Last Wish", "Ichor4": "Ichor's Last Wish",
        "Fogs1": "Fog's Prescience", "Fogs2": "Fog's Prescience", "Fogs3": "Fog's Prescience",
        "UngodEmissary": "The Ungod's Demand",
        "Garden": "Within Yourself",
        "FarShoreNoCasualtiesHard": "Duke of the Dunes",
        "OrpheumNoCasualtiesHard": "Master of the Mountains",
        "GardenNoCasualtiesHard": "Garden of Earthly Delights",
        "AllZonesNoCasualtiesHard": "The Work of an Artists",
        "WorldExplorer": "Every Stone Tuned",
        "AllBossesSlap": "God of Phalanges, Palms and Pain",
        "BasicSpeedrun": "Decisive and Concise",
        "UngodKill": "God is Dead and We Have Killed Him",
        "SepulchreKill": "Somebody Call the Vatican",
        "XiphactinusKill": "Bit off More Than You Can Chew",
        "UnfinishedHeirKill": "Bloodline Drinker",
        "CharcarrionDecomposition": "Crisis of Faith",
        "NoNowakHardEnd": "Plot Armor",
        "NowakLoneSurvivor": "Worthy Successor",
        "HeavenDoubleSacrifice": "The Second Coming",
        "CasualtiesCharacter5": "Month of Funerals",
        "CasualtiesEnemy30": "Mass Grave Matters",
        "HeadshotDeath": "Boom, Headshot",
        "DamageDealt100": "War Criminal",
        "AntonSad": "Plenty of Fish in the Desert",
        # Character-specific
        "Boyle_Osman": "Another Dud",
        "Boyle_Heaven": "Purple Heart",
        "Hans_Osman": "Rorscach Test",
        "Hans_Heaven": "Roentgen Rays",
        "Burnout_Osman": "Health Insurance",
        "Burnout_Heaven": "A Gift?",
        "Fennec_Osman": "Rotund Amphibian",
        "Fennec_Heaven": "Gamified Cephalopod",
        "Anton_Osman": "You Can Do It!",
        "Anton_Heaven": "Russki Vampire",
        "Splig_Osman": "Extra Stitching",
        "Splig_Heaven": "Pain Killers",
        "Pearl_Osman": "Lycanthrope's Core",
        "Pearl_Heaven": "Head of Scrybe",
        "Thype_Osman": "Fishing Rod",
        "Thype_Heaven": "Effigy of the Mettle Mother",
        "Griffin_Osman": "Gilded Mirror",
        "Griffin_Heaven": "Spiked Collar",
        "Arnold_Osman": "Someone Else's Wedding Ring",
        "Arnold_Heaven": "Fist Full of Ash",
        "Dimitri_Osman": "Czech Hedgehog",
        "Dimitri_Heaven": "Cremation",
        "LongLiver_Osman": "Deworming Pills",
        "LongLiver_Heaven": "Medical Leches",
        "Clive_Osman": "Holy Chalice",
        "Clive_Heaven": "Seeds of the Consumed",
        "Kleiver_Osman": "The Jersey",
        "Kleiver_Heaven": "Pontiff's Parade",
        "Cranes_Osman": "Mystery Ration",
        "Cranes_Heaven": "Ol' Stumpy",
        "Agon_Osman": "Iron Necklace",
        "Agon_Heaven": "The Apple",
        "Rags_Osman": "Trepanation",
        "Rags_Heaven": "Wheel of Fortune",
        "SmokeStacks_Osman": "Prussian Blue",
        "SmokeStacks_Heaven": "DDT",
        "Leviat_Osman": "Blind Faith",
        "Leviat_Heaven": "Modern Medicine",
        "Bimini_Osman": "Divine Mud",
        "Bimini_Heaven": "Opulent Egg",
        "Gospel_Osman": "Sculptur's Tools",
        "Gospel_Heaven": "Gospel's Severed Head",
        "Mung_Osman": "Wels Catfish",
        "Mung_Heaven": "Left Shoe",
        "Mordrake_Osman": "Meatre Worm",
        "Mordrake_Heaven": "Norris!",
        "ShellyK_Osman": "Burn-Bottle Batch",
        "ShellyK_Heaven": "Royal Pine",
        "Formosus_Osman": "Coelacanth",
        "Formosus_Heaven": "Sacred Shrub",
        "ProdigalFlee": "Fear of Gods Above",
        "MordrakeCH": "The Mistake",
        # Bronzo
        "Bronzo1": "Bronzo's 2 Cents",
        "Bronzo2": "What the !@#$ Nowak?",
        "BronzoBossPhase1": "What the !@#$ Nowak?",
        "Bronzo3": "Okay Nowak, Seriously Stop!",
        "BronzoBossPhase2": "Okay Nowak, Seriously Stop!",
        "Bronzo4": "That's it Nowak!",
        "BronzoBossPhase3": "That's it Nowak!",
        "Bronzo5": "Time to Die!",
        "BronzoBossPhase4": "Time to Die!",
        "Bronzo6": "The Shyster",
        "BronzoBoss": "The Shyster",
        # Mordrake
        "Mordrake1": "Mordrake's Untold Tale",
        "Mordrake2": "Mordrake's Untold Tale",
        "Mordrake3": "Mordrake's Untold Tale",
        "Mordrake4": "Mordrake's Untold Tale",
        "Mordrake5": "Mordrake's Untold Tale",
        "Mordrake6": "Mordrake's Untold Tale",
        # Director's Cut
        "VHSTask0": "The Director's Final Frame",
        "VHSTask1": "The Director's Final Frame",
        "VHSTask2": "The Director's Final Frame",
        "VHSTask3": "The Director's Final Frame",
        "VHSTask4": "The Director's Final Frame",
        "VHSTask5": "The Director's Final Frame",
        "VHSTask6": "The Director's Final Frame",
        # Winstreaks
        "Winstreak2": "Dumb Luck",
        "Winstreak3": "Notable Skill",
        "Winstreak4": "Burgeoning Expertise",
        "Winstreak5": "Total and Absolute Mastery",
        "HundredPercent": "Brutal Orchestra"
    }

    location_name_to_id = {
        "Far Shore Access": 100,
        "Orpheum Access": 101,
        "Quarry Access": 102,
        "BuyHero_1": 103, "BuyHero_2": 104, "BuyHero_3": 105, "BuyHero_4": 106
    }

    for i in range(1, FarBattleCount.range_end + 1):
        location_name_to_id[f"Far_Battle_{i}"] = 200 + i - 1
    for i in range(1, OrpBattleCount.range_end + 1):
        location_name_to_id[f"Orp_Battle_{i}"] = 300 + i - 1
    for i in range(1, FarMoneyChests.range_end + 1):
        location_name_to_id[f"Far_MoneyChest_{i}"] = 400 + i - 1
    for i in range(1, OrpMoneyChests.range_end + 1):
        location_name_to_id[f"Orp_MoneyChest_{i}"] = 500 + i - 1
    for i in range(1, FarArtifactChests.range_end + 1):
        location_name_to_id[f"Far_ArtifactChest_{i}"] = 600 + i - 1
    for i in range(1, OrpArtifactChests.range_end + 1):
        location_name_to_id[f"Orp_ArtifactChest_{i}"] = 700 + i - 1

    location_name_to_id["Far Boss"] = 800
    location_name_to_id["Orp Boss"] = 801
    location_name_to_id["Quarry Boss"] = 802

    for i in range(1, FarShopCount.range_end + 1):
        location_name_to_id[f"Shop_Far_{i}"] = 900 + i - 1
    for i in range(1, OrpShopCount.range_end + 1):
        location_name_to_id[f"Shop_Orp_{i}"] = 1000 + i - 1

    for i, name in enumerate(hero_names):
        location_name_to_id[hero_check_names[name]] = 1100 + i

    used_check_names = set()
    next_item_id = 1300
    for uid in item_unlock_ids:
        cname = item_check_names.get(uid, f"Item_{uid}")
        if cname not in used_check_names:
            location_name_to_id[cname] = next_item_id
            next_item_id += 1
            used_check_names.add(cname)

    location_name_to_id["Quarry_Boss_Spared"] = 2700
    location_name_to_id["Mordrake's Untold Tale"] = 2800
    location_name_to_id["The Director's Final Frame"] = 2900

    for i in range(1, GardenBattleCount.range_end + 1):
        location_name_to_id[f"Garden_Battle_{i}"] = 3000 + i - 1
    for i in range(1, GardenMoneyChests.range_end + 1):
        location_name_to_id[f"Garden_MoneyChest_{i}"] = 3100 + i - 1
    for i in range(1, GardenArtifactChests.range_end + 1):
        location_name_to_id[f"Garden_ArtifactChest_{i}"] = 3200 + i - 1
    for i in range(1, GardenShopCount.range_end + 1):
        location_name_to_id[f"Garden_Shop_{i}"] = 3300 + i - 1
    location_name_to_id["Garden Boss"] = 3400

    # Victory locations (numbered, up to 10)
    for i in range(1, 11):
        location_name_to_id[f"Quarry Boss Defeat {i}"] = 4000 + i
    for i in range(1, 11):
        location_name_to_id[f"Garden Boss Defeat {i}"] = 4100 + i

    # ------------------------------------------------------------------
    # Access rules based on the achievement/req table.
    # Each entry may contain:
    #   'hardmode'  -> bool, requires "Hardmode Access" item
    #   'heroes'    -> list of hero short names, ALL must be reachable
    #   'any_heroes'-> list of hero short names, AT LEAST ONE must be reachable
    #   'items'     -> list of item names, ALL must be held (state.has_all)
    #   'or_items'  -> used together with 'any_heroes' as an OR condition
    #   'prior'     -> location name that must be reachable first (quest chains)
    #   'min_heroes'-> minimum count of unlocked heroes required
    #   'all_heroes'-> bool, ALL heroes must be reachable
    #   'all_locations' -> bool, ALL active_locations must be reachable (except self)
    # ------------------------------------------------------------------

    # Локации с "требует всё" — взаимно исключаем их друг у друга в all_locations,
    # иначе получится циклическая зависимость (A требует B, B требует A).
    ALL_LOCATIONS_MUTUAL_EXCLUDE = {
        "Mordrake's Untold Tale", "Brutal Orchestra", "The Director's Final Frame"
    }
    
    LOCATION_ZONES = {
        # Far Shore (без хардмода)
        "The Gambler": "far", "The Conjoined": "far", "The Glutton": "far",
        "The Sadist": "far", "The Impaled": "far",
        "The Juggernaut": "far", "The Fire and Flames": "far", "The Widower": "far",
        "Heavyweight Champion": "far",
        # Any → far
        "Throw Hands": "far", "Nary a Finger Lifted": "far", "Human Resources": "far",
        "Deep Pockets": "far", "It Happens to the Best of Us": "far", "Honest Effort": "far",
        "Tactical Miscalculation": "far", "Drowning in Pigment": "far",
        "Capital is King": "far", "Brutality": "far", "What are You Doing?": "far",
        "So Long Liver": "far", "War Criminal": "far",
        # Orpheum
        "The Parasite": "orp", "The Stoic": "orp", "The Zealot": "orp", "The Corpse": "orp",
        "The Leviathan": "orp", "The Orphan": "orp", "Beyond the Dunes": "orp",
        "Thorough Explorer": "orp", "Emotional and Physical Annihilation": "orp",
        "Smooch!": "orp", "Swallowed by the Sea": "orp",
        # Quarry
        "Above the Mountains": "quarry", "The End?": "quarry",
        "I'll Make You Regret This": "quarry", "Quarry_Boss_Spared": "quarry",
        # Far/Orp + Hardmode (регион свой, hardmode отдельным правилом)
        "The Trickster": "far", "The Naked": "far", "The Failure": "far",
        "The Incinerated": "far", "The Emotional Disaster": "far",
        "The Coward": "far", "Bloodline Drinker": "far", "Boom, Headshot": "far",
        "Plenty of Fish in the Desert": "far", "Duke of the Dunes": "far",
        "Mass Grave Matters": "far",
        "The Terminal": "orp", "The Psychic": "orp", "The Black Lung": "orp",
        "The Messiah": "orp", "Crisis of Faith": "orp", "Master of the Mountains": "orp",
        # Garden (регион уже требует Orp+Quarry+Hardmode)
        "The Immortal": "garden", "The Magnum Opus": "garden", "The Exhumed": "garden",
        "The Mass": "garden", "The Cougar": "garden", "The Mistake": "garden",
        "All that is Mortal": "garden", "The Witness": "garden", "The Divine": "garden",
        "Within Yourself": "garden", "Garden of Earthly Delights": "garden",
        "Decisive and Concise": "garden", "God is Dead and We Have Killed Him": "garden",
        "Somebody Call the Vatican": "garden", "Bit off More Than You Can Chew": "garden",
        "Plot Armor": "garden", "Worthy Successor": "garden", "The Second Coming": "garden",
        "Month of Funerals": "garden", "Fear of Gods Above": "garden",
        # All areas + HM → garden
        "Kingslayer": "garden", "Ichor's Last Wish": "garden", "Fog's Prescience": "garden",
        "The Ungod's Demand": "garden", "The Work of an Artists": "garden",
        "Every Stone Tuned": "garden", "God of Phalanges, Palms and Pain": "garden",
        "Dumb Luck": "garden", "Notable Skill": "garden",
        "Burgeoning Expertise": "garden", "Total and Absolute Mastery": "garden",
        # Пары героев (Osman/Heaven) — все Garden
        "Another Dud": "garden", "Purple Heart": "garden", "Rorscach Test": "garden",
        "Roentgen Rays": "garden", "Health Insurance": "garden", "A Gift?": "garden",
        "Rotund Amphibian": "garden", "Gamified Cephalopod": "garden",
        "You Can Do It!": "garden", "Russki Vampire": "garden",
        "Extra Stitching": "garden", "Pain Killers": "garden",
        "Lycanthrope's Core": "garden", "Head of Scrybe": "garden",
        "Fishing Rod": "garden", "Effigy of the Mettle Mother": "garden",
        "Gilded Mirror": "garden", "Spiked Collar": "garden",
        "Someone Else's Wedding Ring": "garden", "Fist Full of Ash": "garden",
        "Czech Hedgehog": "garden", "Cremation": "garden",
        "Deworming Pills": "garden", "Medical Leches": "garden",
        "Holy Chalice": "garden", "Seeds of the Consumed": "garden",
        "The Jersey": "garden", "Pontiff's Parade": "garden",
        "Mystery Ration": "garden", "Ol' Stumpy": "garden",
        "Iron Necklace": "garden", "The Apple": "garden",
        "Trepanation": "garden", "Wheel of Fortune": "garden",
        "Prussian Blue": "garden", "DDT": "garden",
        "Blind Faith": "garden", "Modern Medicine": "garden",
        "Divine Mud": "garden", "Opulent Egg": "garden",
        "Sculptur's Tools": "garden", "Gospel's Severed Head": "garden",
        "Wels Catfish": "garden", "Left Shoe": "garden",
        "Meatre Worm": "garden", "Norris!": "garden",
        "Burn-Bottle Batch": "garden", "Royal Pine": "garden",
        "Coelacanth": "garden", "Sacred Shrub": "garden",
        # Bronzo
        "Bronzo's 2 Cents": "garden", "What the !@#$ Nowak?": "orp",
        "Okay Nowak, Seriously Stop!": "far", "That's it Nowak!": "far",
        "Time to Die!": "orp", "The Shyster": "garden",
        # Финальные квесты
        "Mordrake's Untold Tale": "garden", "Brutal Orchestra": "garden",
        "The Director's Final Frame": "garden",
    }

    ITEM_ACCESS_RULES = {
        "What are You Doing?": {"any_heroes": ["Fennec", "Kleiver", "Cranes", "Rags", "Leviat", "Gospel", "Mordrake"]},
        "So Long Liver": {"heroes": ["LongLiver"]},
        "War Criminal": {"any_heroes": ["Arnold"], "or_items": ["Demon Core"]},

        # Far/Orp + Hardmode
        "The Trickster": {"hardmode": True}, "The Naked": {"hardmode": True},
        "The Failure": {"hardmode": True}, "The Incinerated": {"hardmode": True},
        "The Emotional Disaster": {"hardmode": True},
        "The Terminal": {"hardmode": True}, "The Psychic": {"hardmode": True},
        "The Black Lung": {"hardmode": True},
        "The Coward": {"hardmode": True}, "The Messiah": {"hardmode": True},
        "Crisis of Faith": {"hardmode": True}, "Bloodline Drinker": {"hardmode": True},
        "Boom, Headshot": {"hardmode": True}, "Mass Grave Matters": {"hardmode": True},
        "Duke of the Dunes": {"hardmode": True}, "Master of the Mountains": {"hardmode": True},
        "Plenty of Fish in the Desert": {"hardmode": True, "heroes": ["Anton"]},

        # Garden + герои
        "Health Insurance": {"heroes": ["Burnout"]}, "A Gift?": {"heroes": ["Burnout"]},
        "Rotund Amphibian": {"heroes": ["Fennec"]}, "Gamified Cephalopod": {"heroes": ["Fennec"]},
        "You Can Do It!": {"heroes": ["Anton"]}, "Russki Vampire": {"heroes": ["Anton"]},
        "Extra Stitching": {"heroes": ["Splig"]}, "Pain Killers": {"heroes": ["Splig"]},
        "Lycanthrope's Core": {"heroes": ["Pearl"]}, "Head of Scrybe": {"heroes": ["Pearl"]},
        "Fishing Rod": {"heroes": ["Thype"]}, "Effigy of the Mettle Mother": {"heroes": ["Thype"]},
        "Gilded Mirror": {"heroes": ["Griffin"]}, "Spiked Collar": {"heroes": ["Griffin"]},
        "The Cougar": {"heroes": ["Arnold"]},
        "Someone Else's Wedding Ring": {"heroes": ["Arnold"]}, "Fist Full of Ash": {"heroes": ["Arnold"]},
        "Czech Hedgehog": {"heroes": ["Dimitri"]}, "Cremation": {"heroes": ["Dimitri"]},
        "Deworming Pills": {"heroes": ["LongLiver"]}, "Medical Leches": {"heroes": ["LongLiver"]},
        "Holy Chalice": {"heroes": ["Clive"]}, "Seeds of the Consumed": {"heroes": ["Clive"]},
        "The Jersey": {"heroes": ["Kleiver"]}, "Pontiff's Parade": {"heroes": ["Kleiver"]},
        "Mystery Ration": {"heroes": ["Cranes"]}, "Ol' Stumpy": {"heroes": ["Cranes"]},
        "Iron Necklace": {"heroes": ["Agon"]}, "The Apple": {"heroes": ["Agon"]},
        "Trepanation": {"heroes": ["Rags"]}, "Wheel of Fortune": {"heroes": ["Rags"]},
        "Prussian Blue": {"heroes": ["SmokeStacks"]}, "DDT": {"heroes": ["SmokeStacks"]},
        "Blind Faith": {"heroes": ["Leviat"]}, "Modern Medicine": {"heroes": ["Leviat"]},
        "Divine Mud": {"heroes": ["Bimini"]}, "Opulent Egg": {"heroes": ["Bimini"]},
        "Fear of Gods Above": {"heroes": ["Gospel"]},
        "Sculptur's Tools": {"heroes": ["Gospel"]}, "Gospel's Severed Head": {"heroes": ["Gospel"]},
        "Wels Catfish": {"heroes": ["Mung"]}, "Left Shoe": {"heroes": ["Mung"]},
        "Meatre Worm": {"heroes": ["Mordrake"]}, "Norris!": {"heroes": ["Mordrake"]},
        "Burn-Bottle Batch": {"heroes": ["ShellyK"]}, "Royal Pine": {"heroes": ["ShellyK"]},
        "Coelacanth": {"heroes": ["Formosus"]}, "Sacred Shrub": {"heroes": ["Formosus"]},
        "The Mistake": {"min_heroes": 13},

        # Bronzo chain
        "What the !@#$ Nowak?": {"hardmode": True, "prior": "Bronzo's 2 Cents"},
        "Okay Nowak, Seriously Stop!": {"hardmode": True, "prior": "What the !@#$ Nowak?"},
        "That's it Nowak!": {"hardmode": True, "prior": "Okay Nowak, Seriously Stop!"},
        "Time to Die!": {"hardmode": True, "prior": "That's it Nowak!"},
        "The Shyster": {"prior": "Time to Die!"},

        # Финалы
        "Mordrake's Untold Tale": {"all_heroes": True, "all_locations": True},
        "Brutal Orchestra": {"all_heroes": True, "all_locations": True},
        "The Director's Final Frame": {"all_heroes": True, "all_locations": True},
    }

    def generate_early(self):
        self.far_battle_count = self.options.far_battle_count.value
        self.orp_battle_count = self.options.orp_battle_count.value
        self.far_money_chests = self.options.far_money_chests.value
        self.orp_money_chests = self.options.orp_money_chests.value
        self.far_artifact_chests = self.options.far_artifact_chests.value
        self.orp_artifact_chests = self.options.orp_artifact_chests.value
        self.shop_count_far = self.options.far_shop_count.value
        self.shop_count_orp = self.options.orp_shop_count.value
        self.boss_count = self.options.boss_count.value
        self.hardmode = self.options.hardmode.value
        self.bronzo_quest = self.options.bronzo_quest.value
        self.director_quest = self.options.director_quest.value
        self.mordrake_quest = self.options.mordrake_quest.value
        self.win_count = self.options.win_count.value
        self.death_link = self.options.death_link.value

        self.active_locations = set()
        self.active_locations.update([
            "Far Shore Access", "Orpheum Access", "Quarry Access",
            "BuyHero_1", "BuyHero_2", "BuyHero_3", "BuyHero_4"
        ])
        for i in range(1, self.far_battle_count + 1):
            self.active_locations.add(f"Far_Battle_{i}")
        for i in range(1, self.orp_battle_count + 1):
            self.active_locations.add(f"Orp_Battle_{i}")
        for i in range(1, self.far_money_chests + 1):
            self.active_locations.add(f"Far_MoneyChest_{i}")
        for i in range(1, self.orp_money_chests + 1):
            self.active_locations.add(f"Orp_MoneyChest_{i}")
        for i in range(1, self.far_artifact_chests + 1):
            self.active_locations.add(f"Far_ArtifactChest_{i}")
        for i in range(1, self.orp_artifact_chests + 1):
            self.active_locations.add(f"Orp_ArtifactChest_{i}")
        for i in range(1, self.shop_count_far + 1):
            self.active_locations.add(f"Shop_Far_{i}")
        for i in range(1, self.shop_count_orp + 1):
            self.active_locations.add(f"Shop_Orp_{i}")
        if self.boss_count >= 1:
            self.active_locations.add("Far Boss")
        if self.boss_count >= 2:
            self.active_locations.add("Orp Boss")
        if self.boss_count >= 3:
            self.active_locations.add("Quarry Boss")

        for name in self.hero_names:
            self.active_locations.add(self.hero_check_names[name])

        # ID квестовых предметов Bronzo/Director/Mordrake — исключаем их из
        # безусловного добавления ниже, чтобы локация появлялась только
        # когда соответствующий квест реально включён в опциях.
        bronzo_uids = {"Bronzo1", "Bronzo2", "BronzoBossPhase1", "Bronzo3", "BronzoBossPhase2",
                       "Bronzo4", "BronzoBossPhase3", "Bronzo5", "BronzoBossPhase4",
                       "Bronzo6", "BronzoBoss"}
        director_uids = {"VHSTask0", "VHSTask1", "VHSTask2", "VHSTask3",
                          "VHSTask4", "VHSTask5", "VHSTask6"}
        mordrake_uids = {"Mordrake1", "Mordrake2", "Mordrake3",
                          "Mordrake4", "Mordrake5", "Mordrake6"}
        quest_uids = bronzo_uids | director_uids | mordrake_uids

        for uid in self.item_unlock_ids:
            if uid in quest_uids:
                continue
            cname = self.item_check_names.get(uid, f"Item_{uid}")
            self.active_locations.add(cname)

        # Optional quests
        if self.bronzo_quest:
            for uid in bronzo_uids:
                self.active_locations.add(self.item_check_names[uid])
        if self.director_quest:
            for uid in director_uids:
                self.active_locations.add(self.item_check_names[uid])
        if self.mordrake_quest:
            for uid in mordrake_uids:
                self.active_locations.add(self.item_check_names[uid])

        self.active_locations.add("Quarry_Boss_Spared")
        if self.mordrake_quest:
            self.active_locations.add("Mordrake's Untold Tale")
        if self.director_quest:
            self.active_locations.add("The Director's Final Frame")

        if self.hardmode:
            for i in range(1, self.options.garden_battle_count.value + 1):
                self.active_locations.add(f"Garden_Battle_{i}")
            for i in range(1, self.options.garden_money_chests.value + 1):
                self.active_locations.add(f"Garden_MoneyChest_{i}")
            for i in range(1, self.options.garden_artifact_chests.value + 1):
                self.active_locations.add(f"Garden_ArtifactChest_{i}")
            for i in range(1, self.options.garden_shop_count.value + 1):
                self.active_locations.add(f"Garden_Shop_{i}")
            self.active_locations.add("Garden Boss")

        # Victory locations (always numbered)
        for i in range(1, self.win_count + 1):
            self.active_locations.add(f"Garden Boss Defeat {i}" if self.hardmode else f"Quarry Boss Defeat {i}")

                # в конце generate_early, после всех self.active_locations.add(...)
        print(f"[BruOrch DEBUG] active_locations count: {len(self.active_locations)}")

    def fill_slot_data(self):
        data = {
            "far_battle_count": self.far_battle_count,
            "orp_battle_count": self.orp_battle_count,
            "far_money_chests": self.far_money_chests,
            "orp_money_chests": self.orp_money_chests,
            "far_artifact_chests": self.far_artifact_chests,
            "orp_artifact_chests": self.orp_artifact_chests,
            "shop_far": self.shop_count_far,
            "shop_orp": self.shop_count_orp,
            "boss_count": self.boss_count,
            "hardmode": bool(self.hardmode),
            "bronzo_quest": bool(self.bronzo_quest),
            "director_quest": bool(self.director_quest),
            "mordrake_quest": bool(self.mordrake_quest),
            "win_count": self.win_count,
            "death_link": bool(self.death_link)
        }
        if self.hardmode:
            data.update({
                "garden_battle_count": self.options.garden_battle_count.value,
                "garden_money_chests": self.options.garden_money_chests.value,
                "garden_artifact_chests": self.options.garden_artifact_chests.value,
                "garden_shop_count": self.options.garden_shop_count.value,
            })
        return data

    def create_regions(self):
        menu = Region("Menu", self.player, self.multiworld)
        far = Region("Far Shore", self.player, self.multiworld)
        orp = Region("Orpheum", self.player, self.multiworld)
        quarry = Region("Quarry", self.player, self.multiworld)
        garden = Region("Garden", self.player, self.multiworld)
        vic = Region("Victory", self.player, self.multiworld)

        self.multiworld.regions += [menu, far, orp, quarry, garden, vic]
        menu.connect(far)
        far.connect(orp)
        orp.connect(quarry)
        quarry.connect(garden)
        garden.connect(vic)

        far_prefixes = ("Far Shore Access", "Far_Battle", "Far_MoneyChest", "Far_ArtifactChest",
                        "BuyHero_1", "BuyHero_2", "Shop_Far", "Far Boss")
        orp_prefixes = ("Orpheum Access", "Orp_Battle", "Orp_MoneyChest", "Orp_ArtifactChest",
                        "BuyHero_3", "BuyHero_4", "Shop_Orp", "Orp Boss")
        quarry_prefixes = ("Quarry Access", "Quarry Boss")
        garden_prefixes = ("Garden_Battle", "Garden_MoneyChest", "Garden_ArtifactChest",
                           "Garden_Shop", "Garden Boss")

        zone_map = {"far": far, "orp": orp, "quarry": quarry, "garden": garden}
        victory_prefixes = ("Quarry Boss Defeat", "Garden Boss Defeat")
        for name in self.active_locations:
            if name.startswith(victory_prefixes):
                continue
            if name in self.LOCATION_ZONES:
                region = zone_map[self.LOCATION_ZONES[name]]
            elif name.startswith(far_prefixes):
                region = far
            elif name.startswith(orp_prefixes):
                region = orp
            elif name.startswith(quarry_prefixes):
                region = quarry
            elif name.startswith(garden_prefixes):
                region = garden
            else:
                region = far
            loc = Location(self.player, name, self.location_name_to_id[name], region)
            region.locations.append(loc)

        # Create victory locations
        for i in range(1, self.win_count + 1):
            loc_name = f"Garden Boss Defeat {i}" if self.hardmode else f"Quarry Boss Defeat {i}"
            region = garden if self.hardmode else quarry
            loc = Location(self.player, loc_name, None, region)
            region.locations.append(loc)

    def create_item(self, name: str) -> Item:
        if name in ("5 Coins", "10 Coins", "15 Coins"):
            classification = ItemClassification.filler
        elif name == "Progressive Start Money":
            classification = ItemClassification.useful
        else:
            classification = ItemClassification.progression
        return Item(name, classification, self.item_name_to_id[name], self.player)

    def create_items(self):
        skip_always = {"Boss 1", "Boss 2", "Boss 3", "Progressive Start Money",
               "Winstreak 2 Bundle", "Winstreak 3 Bundle", "Winstreak 4 Bundle",
               "Winstreak 5 Bundle", "HundredPercent Bundle",
               # Неиспользуемые базовые шмотки
               "Anemone Thread", "Beads of Something or Other", "Blood Thirsty Idol",
               "Boonario", "Bosch's Fist", "Clash of the Bleached", "Crown of Thorns",
               "Dew-Covered Sticker", "Diseased Bodypart", "Dried Paintbrush",
               "Eel's Skin", "Fennel's Blessing", "Flesh-Piercing Skewer",
               "Gilded Rage", "Golden Gun", "Heaven-Bound Noose", "Idle Hands",
               "Inhalant", "Jumper Cables", "Lucky Charm", "Mangled Messiah",
               "Marrow's Reach", "Mirror Shard", "Mithridatism", "Mung Moon",
               "Mutually Assured Destruction", "Myopia Glasses", "Padded Gloves",
               "Pile of Dirt", "Pox Blanket", "Prayer Beads", "Rabbit's Foot",
               "Roid Rage", "Scab-Knuckled Gloves", "Sealed Envelope",
               "Shattered Amulet", "Silk Tourniquet", "Soul Contract", "Stimpak",
               "Strange Beads", "The Unfinished Bolt", "Thick Gloves",
               "Thorned Vines", "Tumor"}
        skip_if_bronzo_off = {"Bronzo's 2 Cents (Item)", "Counterfeit coins", "Bananas", "Bronzo's Stupid Hat"}
        skip_if_director_off = {"Broken Doll", "Infernal Eye", "Vyacheslav's Last Sip", "Wailing Whistle",
                               "Cursed Sword", "Enigma", "The Master's Sickle", "Esoteric Artifact"}
        skip_if_mordrake_off = {"Mordrake's Untold Tale"}

        created_items = []
        for name in self.item_names:
            if name in skip_always or \
               (not self.bronzo_quest and name in skip_if_bronzo_off) or \
               (not self.director_quest and name in skip_if_director_off) or \
               (not self.mordrake_quest and name in skip_if_mordrake_off):
                continue
            created_items.append(self.create_item(name))

        for _ in range(self.options.start_money_count.value):
            created_items.append(self.create_item("Progressive Start Money"))

        total_active = len(self.active_locations)
        needed = total_active - self.win_count - len(created_items)

        for i in range(needed):
            coin = ("5 Coins", "10 Coins", "15 Coins")[i % 3]
            created_items.append(self.create_item(coin))

        self.multiworld.itempool += created_items

        for i in range(1, self.win_count + 1):
            loc_name = f"Garden Boss Defeat {i}" if self.hardmode else f"Quarry Boss Defeat {i}"
            self.multiworld.get_location(loc_name, self.player).place_locked_item(
                Item("Garden Boss Defeat" if self.hardmode else "Quarry Boss Defeat", ItemClassification.progression, None, self.player)
            )

        print(f"[BruOrch DEBUG] itempool count for this player: {len(created_items)}")
        print(f"[BruOrch DEBUG] total_active: {total_active}, win_count: {self.win_count}, needed: {needed}")        

    def _hero_reachable(self, state, hero_short_name):
        return state.has(self.hero_check_names[hero_short_name], self.player)

    def _make_access_rule(self, spec):
        hardmode = spec.get("hardmode", False)
        heroes = spec.get("heroes")
        any_heroes = spec.get("any_heroes")
        items = spec.get("items")
        or_items = spec.get("or_items")
        prior = spec.get("prior")
        min_heroes = spec.get("min_heroes")
        all_heroes = spec.get("all_heroes", False)
        all_locations = spec.get("all_locations", False)
        self_loc_name = spec.get("_self_loc_name")

        def hero_names_excluding_self():
            for h in self.hero_names:
                if self.hero_check_names.get(h) == self_loc_name:
                    continue
                yield h

        def rule(state):
            if hardmode and not state.has("Hardmode Access", self.player):
                return False
            if items and not state.has_all(items, self.player):
                return False
            if heroes:
                for h in heroes:
                    if self.hero_check_names.get(h) == self_loc_name:
                        continue
                    if not self._hero_reachable(state, h):
                        return False
            if any_heroes:
                relevant_any_heroes = [h for h in any_heroes if self.hero_check_names.get(h) != self_loc_name]
                any_hero_ok = any(self._hero_reachable(state, h) for h in relevant_any_heroes)
                if or_items:
                    if not (any_hero_ok or state.has_any(or_items, self.player)):
                        return False
                elif not any_hero_ok:
                    return False
            if prior and not state.can_reach_location(prior, self.player):
                return False
            if min_heroes:
                count = sum(1 for h in hero_names_excluding_self() if self._hero_reachable(state, h))
                if count < min_heroes:
                    return False
            if all_heroes:
                for h in hero_names_excluding_self():
                    if not self._hero_reachable(state, h):
                        return False
            if all_locations:
                for loc_name in self.active_locations:
                    if loc_name == self_loc_name:
                        continue
                    if loc_name in self.ALL_LOCATIONS_MUTUAL_EXCLUDE:
                        continue
                    if not state.can_reach_location(loc_name, self.player):
                        return False
            return True
        return rule

    def set_rules(self):
        self.multiworld.get_entrance("Far Shore -> Orpheum", self.player).access_rule = \
            lambda state: state.has("Orpheum Access", self.player)
        self.multiworld.get_entrance("Orpheum -> Quarry", self.player).access_rule = \
            lambda state: state.has("Quarry Access", self.player)
        self.multiworld.get_entrance("Quarry -> Garden", self.player).access_rule = \
            lambda state: state.has("Hardmode Access", self.player)

        # Item/achievement access rules from the req. table
        for loc_name, spec in self.ITEM_ACCESS_RULES.items():
            if loc_name in self.active_locations:
                spec_with_self = dict(spec)
                spec_with_self["_self_loc_name"] = loc_name
                self.multiworld.get_location(loc_name, self.player).access_rule = \
                    self._make_access_rule(spec_with_self)

        self.multiworld.completion_condition[self.player] = \
            lambda state: state.has("Garden Boss Defeat" if self.hardmode else "Quarry Boss Defeat", self.player, self.win_count)

        if not self.hardmode:
            for name in self.active_locations:
                zone = self.LOCATION_ZONES.get(name)
                spec = self.ITEM_ACCESS_RULES.get(name, {})
                if zone == "garden" or spec.get("hardmode"):
                    self.multiworld.get_location(name, self.player).progress_type = \
                        LocationProgressType.EXCLUDED
