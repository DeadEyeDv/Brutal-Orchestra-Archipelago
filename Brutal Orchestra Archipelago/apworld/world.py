from worlds.AutoWorld import World
from BaseClasses import Item, ItemClassification, Location, Region
from .options import BrutalOrchestraOptions

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

    location_name_to_id = {}  # Будет заполнено в generate_early

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

        # Заполняем location_name_to_id с фиксированными смещениями (как в клиенте)
        self.location_name_to_id.clear()

        # Базовые (100-106)
        base = 100
        self.location_name_to_id["Far Shore Access"] = base; base += 1
        self.location_name_to_id["Orpheum Access"] = base; base += 1
        self.location_name_to_id["Sepulchre Access"] = base; base += 1
        for i in range(1, 5):
            self.location_name_to_id[f"BuyHero_{i}"] = base; base += 1

        # Битвы: Far 200+, Orp 300+
        fid = 200
        for i in range(1, self.far_battle_count + 1):
            self.location_name_to_id[f"Far_Battle_{i}"] = fid; fid += 1
        oid = 300
        for i in range(1, self.orp_battle_count + 1):
            self.location_name_to_id[f"Orp_Battle_{i}"] = oid; oid += 1

        # Денежные сундуки: Far 400+, Orp 500+
        fmid = 400
        for i in range(1, self.far_money_chests + 1):
            self.location_name_to_id[f"Far_MoneyChest_{i}"] = fmid; fmid += 1
        omid = 500
        for i in range(1, self.orp_money_chests + 1):
            self.location_name_to_id[f"Orp_MoneyChest_{i}"] = omid; omid += 1

        # Артефактные сундуки: Far 600+, Orp 700+
        faid = 600
        for i in range(1, self.far_artifact_chests + 1):
            self.location_name_to_id[f"Far_ArtifactChest_{i}"] = faid; faid += 1
        oaid = 700
        for i in range(1, self.orp_artifact_chests + 1):
            self.location_name_to_id[f"Orp_ArtifactChest_{i}"] = oaid; oaid += 1

        # Боссы: 800+
        bid = 800
        if self.boss_count >= 1:
            self.location_name_to_id["Far Boss"] = bid; bid += 1
        if self.boss_count >= 2:
            self.location_name_to_id["Orp Boss"] = bid; bid += 1
        if self.boss_count >= 3:
            self.location_name_to_id["Sepulchre Boss"] = bid; bid += 1

        # Магазины: Far 900+, Orp 1000+
        fsid = 900
        for i in range(1, self.shop_count_far + 1):
            self.location_name_to_id[f"Shop_Far_{i}"] = fsid; fsid += 1
        osid = 1000
        for i in range(1, self.shop_count_orp + 1):
            self.location_name_to_id[f"Shop_Orp_{i}"] = osid; osid += 1

        # Герои (все 25): 1100+
        hid = 1100
        for name in self.hero_names:
            self.location_name_to_id[f"Hero_{name}"] = hid; hid += 1

        # Предметы (19): 1200+
        iid = 1200
        for uid in self.item_unlock_ids:
            self.location_name_to_id[f"Item_{uid}"] = iid; iid += 1

    def fill_slot_data(self):
        return {
            "far_battle_count": self.far_battle_count,
            "orp_battle_count": self.orp_battle_count,
            "far_money_chests": self.far_money_chests,
            "orp_money_chests": self.orp_money_chests,
            "far_artifact_chests": self.far_artifact_chests,
            "orp_artifact_chests": self.orp_artifact_chests,
            "shop_far": self.shop_count_far,
            "shop_orp": self.shop_count_orp,
            "boss_count": self.boss_count,
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

        # Распределяем локации по префиксам
        far_prefixes = ("Far Shore Access", "Far_Battle", "Far_MoneyChest", "Far_ArtifactChest",
                        "BuyHero_1", "BuyHero_2", "Shop_Far", "Far Boss",
                        "Hero_", "Item_")
        orp_prefixes = ("Orpheum Access", "Orp_Battle", "Orp_MoneyChest", "Orp_ArtifactChest",
                        "BuyHero_3", "BuyHero_4", "Shop_Orp", "Orp Boss")
        sep_prefixes = ("Sepulchre Access", "Sepulchre Boss")

        for name, lid in self.location_name_to_id.items():
            if name.startswith(far_prefixes):
                region = far
            elif name.startswith(orp_prefixes):
                region = orp
            elif name.startswith(sep_prefixes):
                region = sep
            else:
                continue
            loc = Location(self.player, name, lid, region)
            region.locations.append(loc)

        loc = Location(self.player, "Sepulchre Boss Defeat", None, sep)
        sep.locations.append(loc)

    def create_items(self):
        for name in self.item_names:
            item = Item(name, ItemClassification.progression,
                       self.item_name_to_id[name], self.player)
            self.multiworld.itempool.append(item)

        total_active = len(self.location_name_to_id)
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

        if "Orp Boss" in self.location_name_to_id:
            self.multiworld.get_location("Orp Boss", self.player).access_rule = \
                lambda state: state.has("Boss 1", self.player)
        if "Sepulchre Boss" in self.location_name_to_id:
            self.multiworld.get_location("Sepulchre Boss", self.player).access_rule = \
                lambda state: state.has("Boss 2", self.player)

        self.multiworld.completion_condition[self.player] = \
            lambda state: state.has("Sepulchre Boss Defeat", self.player)
