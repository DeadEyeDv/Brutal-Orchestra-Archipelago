from dataclasses import dataclass
from Options import Range, PerGameCommonOptions, OptionGroup

class FarShopCount(Range):
    """Number of shop checks in Far Shore zone."""
    display_name = "Far Shore Shop Checks"
    range_start = 0
    range_end = 100
    default = 6

class OrpShopCount(Range):
    """Number of shop checks in Orpheum zone."""
    display_name = "Orpheum Shop Checks"
    range_start = 0
    range_end = 100
    default = 6

class BattleCount(Range):
    """Total number of regular battle checks."""
    display_name = "Battle Count"
    range_start = 0
    range_end = 100
    default = 12

class BossCount(Range):
    """Number of boss checks (0-3)."""
    display_name = "Boss Count"
    range_start = 0
    range_end = 3
    default = 3

@dataclass
class BrutalOrchestraOptions(PerGameCommonOptions):
    far_shop_count: FarShopCount
    orp_shop_count: OrpShopCount
    battle_count: BattleCount
    boss_count: BossCount

option_groups = [
    OptionGroup(
        "Shop Options",
        [FarShopCount, OrpShopCount],
    ),
    OptionGroup(
        "Combat Options",
        [BattleCount, BossCount],
    ),
]
