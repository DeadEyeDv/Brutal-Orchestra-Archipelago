from dataclasses import dataclass
from Options import Range, Toggle, Choice, PerGameCommonOptions, OptionGroup

class Deathlink(Toggle):
    """When you die (whole party wipes), everyone dies. And vice versa"""
    display_name = "Deathlink"

class GoalType(Choice):
    """normal: win game on normal difficulty. pearls: collect N pearls. brutal_orchestra: get achievement Brutal Orchestra (100%)."""
    display_name = "Goal Type"
    option_normal = 0
    option_pearls = 1
    option_brutal_orchestra = 2
    default = 1

class PearlCount(Range):
    """Pearl amount to win"""
    display_name = "Pearl Count"
    range_start = 1
    range_end = 50
    default = 6

class LongLocations(Toggle):
    """Enable long locations (Winstreak, Quests Bronzo/Mordrake/Director, 100%)."""
    display_name = "Long Locations"
    default = 0

class AbilitySanity(Toggle):
    """Randomize amount of slots in the pool. 2 Progressive Ability per charachter."""
    display_name = "Ability Sanity"
    default = 0

class FarBattleCount(Range):
    """Number of regular battles in Far Shore."""
    display_name = "Far Shore Battles"
    range_start = 6
    range_end = 50
    default = 15

class OrpBattleCount(Range):
    """Number of regular battles in Orpheum."""
    display_name = "Orpheum Battles"
    range_start = 6
    range_end = 50
    default = 15

class FarMoneyChests(Range):
    """Number of money chests in Far Shore."""
    display_name = "Far Shore Money Chests"
    range_start = 2
    range_end = 10
    default = 2

class OrpMoneyChests(Range):
    """Number of money chests in Orpheum."""
    display_name = "Orpheum Money Chests"
    range_start = 2
    range_end = 10
    default = 2

class FarArtifactChests(Range):
    """Number of artifact chests in Far Shore."""
    display_name = "Far Shore Artifact Chests"
    range_start = 2
    range_end = 10
    default = 2

class OrpArtifactChests(Range):
    """Number of artifact chests in Orpheum."""
    display_name = "Orpheum Artifact Chests"
    range_start = 2
    range_end = 10
    default = 2

class FarShopCount(Range):
    """Number of shop checks in Far Shore."""
    display_name = "Far Shore Shop Checks"
    range_start = 6
    range_end = 100
    default = 6

class OrpShopCount(Range):
    """Number of shop checks in Orpheum."""
    display_name = "Orpheum Shop Checks"
    range_start = 6
    range_end = 100
    default = 6

class BossCount(Range):
    """Number of boss checks (0–3)."""
    display_name = "Boss Count"
    range_start = 0
    range_end = 3
    default = 3

class Hardmode(Toggle):
    """Enable hardmode (Garden) locations and items."""
    display_name = "Hardmode"
    default = True

class GardenBattleCount(Range):
    """Number of battles in the Garden."""
    display_name = "Garden Battles"
    range_start = 6
    range_end = 50
    default = 10

class GardenMoneyChests(Range):
    """Number of money chests in the Garden."""
    display_name = "Garden Money Chests"
    range_start = 2
    range_end = 10
    default = 2

class GardenArtifactChests(Range):
    """Number of artifact chests in the Garden."""
    display_name = "Garden Artifact Chests"
    range_start = 2
    range_end = 10
    default = 2

class StartMoneyCount(Range):
    """Number of Progressive Start Money items in the pool. Each adds +10 starting coins."""
    display_name = "Progressive Start Money items"
    range_start = 0
    range_end = 10
    default = 3

@dataclass
class BrutalOrchestraOptions(PerGameCommonOptions):
    death_link: Deathlink
    goal_type: GoalType
    pearl_count: PearlCount
    long_locations: LongLocations
    ability_sanity: AbilitySanity
    far_battle_count: FarBattleCount
    orp_battle_count: OrpBattleCount
    far_money_chests: FarMoneyChests
    orp_money_chests: OrpMoneyChests
    far_artifact_chests: FarArtifactChests
    orp_artifact_chests: OrpArtifactChests
    far_shop_count: FarShopCount
    orp_shop_count: OrpShopCount
    hardmode: Hardmode
    garden_battle_count: GardenBattleCount
    garden_money_chests: GardenMoneyChests
    garden_artifact_chests: GardenArtifactChests
    start_money_count: StartMoneyCount

option_groups = [
    OptionGroup("Locations", [
        FarBattleCount, OrpBattleCount, GardenBattleCount,
        FarMoneyChests, OrpMoneyChests, GardenMoneyChests,
        FarArtifactChests, OrpArtifactChests, GardenArtifactChests,
        FarShopCount, OrpShopCount,
    ]),
]
