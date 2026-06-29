from worlds.AutoWorld import World
from BaseClasses import Item, ItemClassification, Location, Region
from .options import BrutalOrchestraOptions, FarBattleCount, OrpBattleCount, \
    FarMoneyChests, OrpMoneyChests, FarArtifactChests, OrpArtifactChests, \
    FarShopCount, OrpShopCount, BossCount

class BrutalOrchestraWorld(World):
    game = "Brutal Orchestra"

    options_dataclass = BrutalOrchestraOptions
    options: BrutalOrchestraOptions

    item_names = [
        "Orpheum Access", "Sepulchre Access",
        "Boss 1", "Boss 2", "Boss 3",
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
        "Black Paint", "Blue Paint", "Brown Paint", "Cyan Paint",
        "Gray Paint", "Green Paint", "Lime Paint", "Magenta Paint",
        "Orange Paint", "Pink Paint", "Purple Paint", "Red Paint",
        "Teal Paint", "White Paint", "Yellow Paint",
        "5 Coins", "10 Coins", "15 Coins",
    ]

    item_name_to_id = {name: 10000 + i for i, name in enumerate(item_names)}

    hero_names = ["Boyle","Hans","Anton","Splig","Pearl","Thype","Griffin","Arnold","Dimitri",
                  "LongLiver","Clive","Kleiver","Cranes","Agon","Rags","SmokeStacks","Leviat",
                  "Gospel","Bimini","Burnout","Fennec","Mordrake","Mung","ShellyK","Formosus"]

    item_unlock_ids = [
        "Ending_CorpseKill","Ending_CorpseSave","ShopDepleted","FoolsDepleted",
        "HeavenDoubleSacrifice","RoidsMissTurn","OrroSmooch","SurviveStarvation",
        "VHSTask0","VHSTask1","VHSTask2","VHSTask3","VHSTask4","VHSTask5","VHSTask6",
        "HundredPercent","UngodEmissary","AntonSad","ProdigalFlee"
    ]

    # Статический словарь со всеми возможными локациями (максимальные значения опций)
    location_name_to_id = {
        "Far Shore Access": 100,
        "Orpheum Access": 101,
        "Sepulchre Access": 102,
        "BuyHero_1": 103,
        "BuyHero_2": 104,
        "BuyHero_3": 105,
        "BuyHero_4": 106
    }

    # Far Battles
    fid = 200
    for i in range(1, FarBattleCount.range_end + 1):
        location_name_to_id[f"Far_Battle_{i}"] = fid
        fid += 1

    # Orp Battles
    oid = 300
    for i in range(1, OrpBattleCount.range_end + 1):
        location_name_to_id[f"Orp_Battle_{i}"] = oid
        oid += 1

    # Far Money Chests
    fmid = 400
    for i in range(1, FarMoneyChests.range_end + 1):
        location_name_to_id[f"Far_MoneyChest_{i}"] = fmid
        fmid += 1

    # Orp Money Chests
    omid = 500
    for i in range(1, OrpMoneyChests.range_end + 1):
        location_name_to_id[f"Orp_MoneyChest_{i}"] = omid
        omid += 1

    # Far Artifact Chests
    faid = 600
    for i in range(1, FarArtifactChests.range_end + 1):
        location_name_to_id[f"Far_ArtifactChest_{i}"] = faid
        faid += 1

    # Orp Artifact Chests
    oaid = 700
    for i in range(1, OrpArtifactChests.range_end + 1):
        location_name_to_id[f"Orp_ArtifactChest_{i}"] = oaid
        oaid += 1

    # Bosses
    bid = 800
    location_name_to_id["Far Boss"] = bid
    bid += 1
    location_name_to_id["Orp Boss"] = bid
    bid += 1
    location_name_to_id["Sepulchre Boss"] = bid
    bid += 1

    # Far Shops
    fsid = 900
    for i in range(1, FarShopCount.range_end + 1):
        location_name_to_id[f"Shop_Far_{i}"] = fsid
        fsid += 1

    # Orp Shops
    osid = 1000
    for i in range(1, OrpShopCount.range_end + 1):
        location_name_to_id[f"Shop_Orp_{i}"] = osid
        osid += 1

    # Heroes
    hid = 1100
    for name in hero_names:
        location_name_to_id[f"Hero_{name}"] = hid
        hid += 1

    # Item Unlocks
    iid = 1200
    for uid in item_unlock_ids:
        location_name_to_id[f"Item_{uid}"] = iid
        iid += 1

    def generate_early(self):
        # Запоминаем, какие локации реально используются (на основе опций)
        self.active_locations = set()
        # Базовые всегда
        self.active_locations.update([
            "Far Shore Access", "Orpheum Access", "Sepulchre Access",
            "BuyHero_1", "BuyHero_2", "BuyHero_3", "BuyHero_4"
        ])
        # Добавляем выбранные опциями локации
        for i in range(1, self.options.far_battle_count.value + 1):
            self.active_locations.add(f"Far_Battle_{i}")
        for i in range(1, self.options.orp_battle_count.value + 1):
            self.active_locations.add(f"Orp_Battle_{i}")
        for i in range(1, self.options.far_money_chests.value + 1):
            self.active_locations.add(f"Far_MoneyChest_{i}")
        for i in range(1, self.options.orp_money_chests.value + 1):
            self.active_locations.add(f"Orp_MoneyChest_{i}")
        for i in range(1, self.options.far_artifact_chests.value + 1):
            self.active_locations.add(f"Far_ArtifactChest_{i}")
        for i in range(1, self.options.orp_artifact_chests.value + 1):
            self.active_locations.add(f"Orp_ArtifactChest_{i}")
        for i in range(1, self.options.far_shop_count.value + 1):
            self.active_locations.add(f"Shop_Far_{i}")
        for i in range(1, self.options.orp_shop_count.value + 1):
            self.active_locations.add(f"Shop_Orp_{i}")
        if self.options.boss_count.value >= 1:
            self.active_locations.add("Far Boss")
        if self.options.boss_count.value >= 2:
            self.active_locations.add("Orp Boss")
        if self.options.boss_count.value >= 3:
            self.active_locations.add("Sepulchre Boss")
        for name in self.hero_names:
            self.active_locations.add(f"Hero_{name}")
        for uid in self.item_unlock_ids:
            self.active_locations.add(f"Item_{uid}")

    def fill_slot_data(self):
        return {
            "far_battle_count": self.options.far_battle_count.value,
            "orp_battle_count": self.options.orp_battle_count.value,
            "far_money_chests": self.options.far_money_chests.value,
            "orp_money_chests": self.options.orp_money_chests.value,
            "far_artifact_chests": self.options.far_artifact_chests.value,
            "orp_artifact_chests": self.options.orp_artifact_chests.value,
            "shop_far": self.options.far_shop_count.value,
            "shop_orp": self.options.orp_shop_count.value,
            "boss_count": self.options.boss_count.value,
        }

    def create_regions(self):
        menu = Region("Menu", self.player, self.multiworld)
        far = Region("Far Shore", self.player, self.multiworld)
        orp = Region("Orpheum", self.player, self.multiworld)
        sep = Region("Sepulchre", self.player, self.multiworld)
        vic = Region("Victory", self.player, self.multiworld)

        self.multiworld.regions += [menu, far, orp, sep, vic]
        menu.connect(far)
        far.connect(orp)
        orp.connect(sep)
        sep.connect(vic)

        # Создаём локации только из активного набора
        far_prefixes = ("Far Shore Access", "Far_Battle", "Far_MoneyChest", "Far_ArtifactChest",
                        "BuyHero_1", "BuyHero_2", "Shop_Far", "Far Boss",
                        "Hero_", "Item_")
        orp_prefixes = ("Orpheum Access", "Orp_Battle", "Orp_MoneyChest", "Orp_ArtifactChest",
                        "BuyHero_3", "BuyHero_4", "Shop_Orp", "Orp Boss")
        sep_prefixes = ("Sepulchre Access", "Sepulchre Boss")

        for name in self.active_locations:
            if name.startswith(far_prefixes):
                region = far
            elif name.startswith(orp_prefixes):
                region = orp
            elif name.startswith(sep_prefixes):
                region = sep
            else:
                continue
            # id берём из общего статического словаря
            loc = Location(self.player, name, self.location_name_to_id[name], region)
            region.locations.append(loc)

        # Sepulchre Boss Defeat
        loc = Location(self.player, "Sepulchre Boss Defeat", None, sep)
        sep.locations.append(loc)

    def create_items(self):
        # Предметы только из исходного списка (всегда одинаковые)
        for name in self.item_names:
            item = Item(name, ItemClassification.progression,
                       self.item_name_to_id[name], self.player)
            self.multiworld.itempool.append(item)

        total_active = len(self.active_locations)
        needed = total_active - len(self.item_names)
        for i in range(needed):
            name = "5 Coins" if i % 2 == 0 else "10 Coins"
            item = Item(name, ItemClassification.filler,
                       self.item_name_to_id[name], self.player)
            self.multiworld.itempool.append(item)

        self.multiworld.get_location("Sepulchre Boss Defeat", self.player).place_locked_item(
            Item("Sepulchre Boss Defeat", ItemClassification.progression, None, self.player)
        )

    def set_rules(self):
        self.multiworld.get_entrance("Far Shore -> Orpheum", self.player).access_rule = \
            lambda state: state.has("Orpheum Access", self.player)
        self.multiworld.get_entrance("Orpheum -> Sepulchre", self.player).access_rule = \
            lambda state: state.has("Sepulchre Access", self.player)

        if "Orp Boss" in self.active_locations:
            self.multiworld.get_location("Orp Boss", self.player).access_rule = \
                lambda state: state.has("Boss 1", self.player)
        if "Sepulchre Boss" in self.active_locations:
            self.multiworld.get_location("Sepulchre Boss", self.player).access_rule = \
                lambda state: state.has("Boss 2", self.player)

        self.multiworld.completion_condition[self.player] = \
            lambda state: state.has("Sepulchre Boss Defeat", self.player)
