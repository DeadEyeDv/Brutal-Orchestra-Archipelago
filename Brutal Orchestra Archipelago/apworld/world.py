from worlds.AutoWorld import World, WebWorld
from BaseClasses import Item, ItemClassification, Location, Region, LocationProgressType
from .options import BrutalOrchestraOptions, option_groups, FarBattleCount, OrpBattleCount, \
    FarMoneyChests, OrpMoneyChests, FarArtifactChests, OrpArtifactChests, \
    FarShopCount, OrpShopCount, GardenBattleCount, GardenMoneyChests, \
    GardenArtifactChests, BuyFoolCount
from .items import item_names, hero_names, item_unlock_ids
from .locations import (hero_check_names, item_check_names, LOCATION_ZONES,
                        PEARL_LOCATIONS, LONG_LOCATIONS, ALL_LOCATIONS_MUTUAL_EXCLUDE)
from .rules import ITEM_ACCESS_RULES

class BrutalOrchestraWeb(WebWorld):
    option_groups = option_groups 

class BrutalOrchestraWorld(World):
    game = "Brutal Orchestra"
    web = BrutalOrchestraWeb()

    options_dataclass = BrutalOrchestraOptions
    options: BrutalOrchestraOptions

    item_names = item_names
    hero_names = hero_names
    item_unlock_ids = item_unlock_ids

    hero_check_names = hero_check_names
    item_check_names = item_check_names
    LOCATION_ZONES = LOCATION_ZONES
    PEARL_LOCATIONS = PEARL_LOCATIONS
    LONG_LOCATIONS = LONG_LOCATIONS
    ALL_LOCATIONS_MUTUAL_EXCLUDE = ALL_LOCATIONS_MUTUAL_EXCLUDE

    ITEM_ACCESS_RULES = ITEM_ACCESS_RULES

    item_name_to_id = {name: 10000 + i for i, name in enumerate(item_names)}
    
    location_name_to_id = {
        "Far Shore Access": 100,
        "Orpheum Access": 101,
        "Quarry Access": 102,
    }
    for i in range(1, BuyFoolCount.range_end + 1):
        location_name_to_id[f"BuyHero_{i}"] = 103 + i - 1
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
    location_name_to_id["Garden Boss"] = 3400

    # Victory locations (numbered, up to 10)
    for i in range(1, 11):
        location_name_to_id[f"Quarry Boss Defeat {i}"] = 4000 + i
    for i in range(1, 11):
        location_name_to_id[f"Garden Boss Defeat {i}"] = 4100 + i

    def generate_early(self):
        self.far_battle_count = self.options.far_battle_count.value
        self.orp_battle_count = self.options.orp_battle_count.value
        self.far_money_chests = self.options.far_money_chests.value
        self.orp_money_chests = self.options.orp_money_chests.value
        self.far_artifact_chests = self.options.far_artifact_chests.value
        self.orp_artifact_chests = self.options.orp_artifact_chests.value
        self.shop_count_far = self.options.far_shop_count.value
        self.shop_count_orp = self.options.orp_shop_count.value
        self.hardmode = self.options.hardmode.value
        self.death_link = self.options.death_link.value
        self.long_locations = self.options.long_locations.value
        self.goal_type = self.options.goal_type.value
        self.pearl_count = self.options.pearl_count.value
        if self.goal_type in (1, 2):   # pearls / brutal_orchestra требуют Garden
            self.hardmode = True
            if self.goal_type == 2:
                self.long_locations = 1
        self.pearl_count = self.options.pearl_count.value
        self.ability_sanity = self.options.ability_sanity.value

        self.active_locations = set()
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
        for i in range(1, self.options.buy_fool_count.value + 1):
            self.active_locations.add(f"BuyHero_{i}")

        for name in self.hero_names:
            self.active_locations.add(self.hero_check_names[name])

        bronzo_uids = {"Bronzo1", "Bronzo2", "BronzoBossPhase1", "Bronzo3", "BronzoBossPhase2",
                       "Bronzo4", "BronzoBossPhase3", "Bronzo5", "BronzoBossPhase4",
                       "Bronzo6", "BronzoBoss"}
        director_uids = {"VHSTask0", "VHSTask1", "VHSTask2", "VHSTask3",
                          "VHSTask4", "VHSTask5", "VHSTask6"}
        mordrake_uids = {"Mordrake1", "Mordrake2", "Mordrake3",
                          "Mordrake4", "Mordrake5", "Mordrake6"}
        quest_uids = bronzo_uids | director_uids | mordrake_uids

        for uid in self.item_unlock_ids:
            cname = self.item_check_names.get(uid, f"Item_{uid}")
            self.active_locations.add(cname)

        self.active_locations.add("Mordrake's Untold Tale")
        self.active_locations.add("The Director's Final Frame")

        if self.hardmode:
            for i in range(1, self.options.garden_battle_count.value + 1):
                self.active_locations.add(f"Garden_Battle_{i}")
            for i in range(1, self.options.garden_money_chests.value + 1):
                self.active_locations.add(f"Garden_MoneyChest_{i}")
            for i in range(1, self.options.garden_artifact_chests.value + 1):
                self.active_locations.add(f"Garden_ArtifactChest_{i}")

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
            "hardmode": bool(self.hardmode),
            "death_link": bool(self.death_link),
            "goal_type": self.goal_type,
            "pearl_count": self.pearl_count,
            "ability_sanity": bool(self.ability_sanity),
            "buy_fool_count": self.options.buy_fool_count.value,
            "long_locations": bool(self.long_locations)
        }
        if self.hardmode:
            data.update({
                "garden_battle_count": self.options.garden_battle_count.value,
                "garden_money_chests": self.options.garden_money_chests.value,
                "garden_artifact_chests": self.options.garden_artifact_chests.value
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
        orp.connect(garden)
        quarry.connect(vic)
        garden.connect(vic)

        far_prefixes = ("Far Shore Access", "Far_Battle", "Far_MoneyChest", "Far_ArtifactChest",
                        "BuyHero_1", "BuyHero_2", "Shop_Far")
        orp_prefixes = ("Orpheum Access", "Orp_Battle", "Orp_MoneyChest", "Orp_ArtifactChest",
                        "BuyHero_3", "BuyHero_4", "Shop_Orp")
        quarry_prefixes = ("Quarry Access")
        garden_prefixes = ("Garden_Battle", "Garden_MoneyChest", "Garden_ArtifactChest")

        zone_map = {"far": far, "orp": orp, "quarry": quarry, "garden": garden}
        for name in self.active_locations:
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

    def create_item(self, name: str) -> Item:
        progression_items = {"Orpheum Access", "Quarry Access", "Garden Access", "Hardmode Access"}
        if name in ("5 Coins", "10 Coins", "15 Coins"):
            classification = ItemClassification.filler
        elif name == "Progressive Start Money":
            classification = ItemClassification.useful
        elif name in progression_items or name.endswith(" Progressive Ability") \
                or name in self.hero_check_names.values():
            classification = ItemClassification.progression
        else:
            classification = ItemClassification.useful
        return Item(name, classification, self.item_name_to_id[name], self.player)

    def create_items(self):
        skip_always = {"5 Coins", "10 Coins", "15 Coins", "Boss 1", "Boss 2", "Boss 3", "Quarry Boss Defeat", "Garden Boss Defeat", "Progressive Start Money",
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
        created_items = []
        for name in self.item_names:
            if name in skip_always or name.endswith(" Progressive Ability"):
                continue
            created_items.append(self.create_item(name))

        for _ in range(self.options.start_money_count.value):
            created_items.append(self.create_item("Progressive Start Money"))

        if self.ability_sanity:
            for name in self.item_names:
                if name.endswith(" Progressive Ability") and name != "Mung Progressive Ability":
                    created_items.append(self.create_item(name))
                    created_items.append(self.create_item(name))

        total_active = len(self.active_locations)
        capacity = total_active
        needed = capacity - len(created_items)

        if needed < 0:
            excess = -needed
            while excess > 0:
                idx = next((i for i, it in enumerate(created_items) if it.name == "Progressive Start Money"), None)
                if idx is None:
                    break
                created_items.pop(idx)
                excess -= 1
            if excess > 0:
                seen = set()
                for i in range(len(created_items) - 1, -1, -1):
                    if excess <= 0:
                        break
                    it = created_items[i]
                    if it.name.endswith(" Progressive Ability"):
                        if it.name in seen:
                            created_items.pop(i)
                            excess -= 1
                        else:
                            seen.add(it.name)
            if excess > 0:
                # крайняя мера: срезаем и первые копии абилок
                for i in range(len(created_items) - 1, -1, -1):
                    if excess <= 0:
                        break
                    if created_items[i].name.endswith(" Progressive Ability"):
                        created_items.pop(i)
                        excess -= 1
                        print(f"[BruOrch] WARNING: pool trimmed by {(-needed)} items to fit locations")
            if excess > 0:
                raise Exception("Brutal Orchestra: not enough locations for required items. "
                                "Increase location counts or disable ability_sanity.")
            needed = 0

        for i in range(needed):
            coin = ("5 Coins", "10 Coins", "15 Coins")[i % 3]
            created_items.append(self.create_item(coin))

        self.multiworld.itempool += created_items
            
        print(f"[BruOrch DEBUG] itempool count for this player: {len(created_items)}")
        print(f"[BruOrch DEBUG] total_active: {total_active},needed: {needed}")        

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
        abilities = spec.get("abilities")
        hero_abilities = spec.get("hero_abilities")
        self_loc_name = spec.get("_self_loc_name")
        min_from = spec.get("min_from")   

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
            if abilities and self.ability_sanity:
                for item_name, count in abilities:
                    if not state.has(item_name, self.player, count):
                        return False
            if hero_abilities:
                def hero_ok(hero_short, ab_count):
                    if not self._hero_reachable(state, hero_short):
                        return False
                    if self.ability_sanity and ab_count > 0:
                        return state.has(f"{hero_short} Progressive Ability", self.player, ab_count)
                    return True
                if not any(hero_ok(h, c) for h, c in hero_abilities):
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
        self.multiworld.get_entrance("Orpheum -> Garden", self.player).access_rule = \
            lambda state: state.has("Hardmode Access", self.player) and state.has("Garden Access", self.player)

        # Item/achievement access rules from the req. table
        for loc_name, spec in self.ITEM_ACCESS_RULES.items():
            if loc_name in self.active_locations:
                spec_with_self = dict(spec)
                spec_with_self["_self_loc_name"] = loc_name
                self.multiworld.get_location(loc_name, self.player).access_rule = \
                    self._make_access_rule(spec_with_self)

        if self.goal_type == 2:
            self.multiworld.completion_condition[self.player] = \
                lambda state: state.can_reach_location("Brutal Orchestra", self.player)
        elif self.goal_type == 1:
            self.multiworld.completion_condition[self.player] = \
                lambda state: sum(1 for loc in self.PEARL_LOCATIONS
                                  if state.can_reach_location(loc, self.player)) >= self.pearl_count
        else:  # normal
            self.multiworld.completion_condition[self.player] = \
                lambda state: state.can_reach_location("The End?", self.player) \
                              or state.can_reach_location("I'll Make You Regret This", self.player)

        if not self.hardmode:
            for name in self.active_locations:
                zone = self.LOCATION_ZONES.get(name)
                spec = self.ITEM_ACCESS_RULES.get(name, {})
                if zone == "garden" or spec.get("hardmode"):
                    self.multiworld.get_location(name, self.player).progress_type = \
                        LocationProgressType.EXCLUDED

        if not self.long_locations:
            for name in self.LONG_LOCATIONS:
                if name in self.active_locations:
                    self.multiworld.get_location(name, self.player).progress_type = \
                        LocationProgressType.EXCLUDED
