from worlds.AutoWorld import World
from BaseClasses import Item, ItemClassification, Location, Region
from .options import BrutalOrchestraOptions

class BrutalOrchestraWorld(World):
    game = "Brutal Orchestra"

    options_dataclass = BrutalOrchestraOptions
    options: BrutalOrchestraOptions

    item_names = [
        "Orpheum Access", "Garden Access",
        "Boss 1", "Boss 2", "Boss 3",
        "Hero_Boyle", "Hero_Hans", "Hero_Anton", "Hero_Splig", "Hero_Pearl",
        "Hero_Thype", "Hero_Griffin", "Hero_Arnold", "Hero_Dimitri",
        "Hero_LongLiver", "Hero_Clive", "Hero_Kleiver", "Hero_Cranes",
        "Hero_Agon", "Hero_Rags", "Hero_SmokeStacks", "Hero_Leviat",
        "Hero_Gospel", "Hero_Bimini", "Hero_Burnout", "Hero_Fennec",
        "Hero_Mordrake", "Hero_Mung", "Hero_ShellyK", "Hero_Formosus",
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

    _max_battles = 100
    _max_bosses = 3
    _max_shop_far = 100
    _max_shop_orp = 100

    _all_battle_names = [f"Battle_{i+1}" for i in range(_max_battles)]
    _all_boss_names = ["Far Boss", "Orp Boss", "Garden Boss"]
    _all_shop_far = [f"Shop_Far_{i+1}" for i in range(_max_shop_far)]
    _all_shop_orp = [f"Shop_Orp_{i+1}" for i in range(_max_shop_orp)]

    _base_locations = [
        "Far Shore Access",
        "Orpheum Access",
        "Garden Access",
        "Far MoneyChest_1", "Far MoneyChest_2",
        "Far ArtifactChest_1", "Far ArtifactChest_2",
        "Orp MoneyChest_1", "Orp MoneyChest_2",
        "Orp ArtifactChest_1", "Orp ArtifactChest_2",
        "BuyHero_1", "BuyHero_2", "BuyHero_3", "BuyHero_4",
        # Victory event NOT in static list – it will be created manually
    ]

    _all_locations = (_base_locations + _all_battle_names + _all_boss_names +
                      _all_shop_far + _all_shop_orp)
    location_name_to_id = {name: 100 + i for i, name in enumerate(_all_locations)}

    def generate_early(self):
        self.shop_count_far = self.options.far_shop_count.value
        self.shop_count_orp = self.options.orp_shop_count.value
        self.battle_count = self.options.battle_count.value
        self.boss_count = self.options.boss_count.value

        active_battles = [f"Battle_{i+1}" for i in range(self.battle_count)]
        active_bosses = self._all_boss_names[:self.boss_count]
        active_far_shops = [f"Shop_Far_{i+1}" for i in range(self.shop_count_far)]
        active_orp_shops = [f"Shop_Orp_{i+1}" for i in range(self.shop_count_orp)]

        all_active = (self._base_locations + active_battles + active_bosses +
                      active_far_shops + active_orp_shops)
        self.location_name_to_id = {name: 100 + i for i, name in enumerate(all_active)}

        self.slot_data = {
            "shop_far": self.shop_count_far,
            "shop_orp": self.shop_count_orp,
            "battle_count": self.battle_count,
            "boss_count": self.boss_count,
        }

    def create_regions(self):
        menu = Region("Menu", self.player, self.multiworld)
        far = Region("Far Shore", self.player, self.multiworld)
        orp = Region("Orpheum", self.player, self.multiworld)
        gar = Region("Garden", self.player, self.multiworld)
        vic = Region("Victory", self.player, self.multiworld)

        self.multiworld.regions += [menu, far, orp, gar, vic]
        menu.connect(far)
        far.connect(orp)
        orp.connect(gar)
        gar.connect(vic)

        active_battles = [f"Battle_{i+1}" for i in range(self.battle_count)]
        active_bosses = self._all_boss_names[:self.boss_count]
        active_far_shops = [f"Shop_Far_{i+1}" for i in range(self.shop_count_far)]
        active_orp_shops = [f"Shop_Orp_{i+1}" for i in range(self.shop_count_orp)]

        half = self.battle_count // 2
        far_battles = active_battles[:half]
        orp_battles = active_battles[half:]

        far_locs = (["Far Shore Access"] +
                    far_battles +
                    (["Far Boss"] if "Far Boss" in active_bosses else []) +
                    ["Far MoneyChest_1", "Far MoneyChest_2",
                     "Far ArtifactChest_1", "Far ArtifactChest_2",
                     "BuyHero_1", "BuyHero_2"] +
                    active_far_shops)

        orp_locs = (["Orpheum Access"] +
                    orp_battles +
                    (["Orp Boss"] if "Orp Boss" in active_bosses else []) +
                    ["Orp MoneyChest_1", "Orp MoneyChest_2",
                     "Orp ArtifactChest_1", "Orp ArtifactChest_2",
                     "BuyHero_3", "BuyHero_4"] +
                    active_orp_shops)

        gar_locs = ["Garden Access"] + (["Garden Boss"] if "Garden Boss" in active_bosses else [])

        for name, lid in self.location_name_to_id.items():
            if name in far_locs:
                region = far
            elif name in orp_locs:
                region = orp
            elif name in gar_locs:
                region = gar
            else:
                continue
            loc = Location(self.player, name, lid, region)
            region.locations.append(loc)

        # Create victory event location manually (no ID)
        loc = Location(self.player, "Garden Boss Defeat", None, gar)
        gar.locations.append(loc)

    def create_items(self):
        for name in self.item_names:
            item = Item(name, ItemClassification.progression,
                       self.item_name_to_id[name], self.player)
            self.multiworld.itempool.append(item)

        total_active = len(self.location_name_to_id)
        # No need to reserve a slot for the event – it's not in location_name_to_id
        needed = total_active - len(self.item_names)

        for i in range(needed):
            name = "5 Coins" if i % 2 == 0 else "10 Coins"
            item = Item(name, ItemClassification.filler,
                       self.item_name_to_id[name], self.player)
            self.multiworld.itempool.append(item)

        # Place the victory event item
        self.multiworld.get_location("Garden Boss Defeat", self.player).place_locked_item(
            Item("Garden Boss Defeat", ItemClassification.progression, None, self.player)
        )

    def set_rules(self):
        self.multiworld.get_entrance("Far Shore -> Orpheum", self.player).access_rule = \
            lambda state: state.has("Orpheum Access", self.player)
        self.multiworld.get_entrance("Orpheum -> Garden", self.player).access_rule = \
            lambda state: state.has("Garden Access", self.player)

        if "Orp Boss" in self.location_name_to_id:
            self.multiworld.get_location("Orp Boss", self.player).access_rule = \
                lambda state: state.has("Boss 1", self.player)
        if "Garden Boss" in self.location_name_to_id:
            self.multiworld.get_location("Garden Boss", self.player).access_rule = \
                lambda state: state.has("Boss 2", self.player)

        self.multiworld.completion_condition[self.player] = \
            lambda state: state.has("Garden Boss Defeat", self.player)
