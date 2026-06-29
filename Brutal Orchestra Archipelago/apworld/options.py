from dataclasses import dataclass
from Options import Range, PerGameCommonOptions, OptionGroup

class FarBattleCount(Range):
    """Number of regular battles in Far Shore."""
    display_name = "Far Shore Battles"
    range_start = 0
    range_end = 50
    default = 15

class OrpBattleCount(Range):
    """Number of regular battles in Orpheum."""
    display_name = "Orpheum Battles"
    range_start = 0
    range_end = 50
    default = 15

class FarMoneyChests(Range):
    """Number of money chests in Far Shore."""
    display_name = "Far Shore Money Chests"
    range_start = 0
    range_end = 10
    default = 2

class OrpMoneyChests(Range):
    """Number of money chests in Orpheum."""
    display_name = "Orpheum Money Chests"
    range_start = 0
    range_end = 10
    default = 2

class FarArtifactChests(Range):
    """Number of artifact chests in Far Shore."""
    display_name = "Far Shore Artifact Chests"
    range_start = 0
    range_end = 10
    default = 2

class OrpArtifactChests(Range):
    """Number of artifact chests in Orpheum."""
    display_name = "Orpheum Artifact Chests"
    range_start = 0
    range_end = 10
    default = 2

class FarShopCount(Range):
    """Number of shop checks in Far Shore."""
    display_name = "Far Shore Shop Checks"
    range_start = 0
    range_end = 100
    default = 6

class OrpShopCount(Range):
    """Number of shop checks in Orpheum."""
    display_name = "Orpheum Shop Checks"
    range_start = 0
    range_end = 100
    default = 6

class BossCount(Range):
    """Number of boss checks (0–3)."""
    display_name = "Boss Count"
    range_start = 0
    range_end = 3
    default = 3

@dataclass
class BrutalOrchestraOptions(PerGameCommonOptions):
    far_battle_count: FarBattleCount
    orp_battle_count: OrpBattleCount
    far_money_chests: FarMoneyChests
    orp_money_chests: OrpMoneyChests
    far_artifact_chests: FarArtifactChests
    orp_artifact_chests: OrpArtifactChests
    far_shop_count: FarShopCount
    orp_shop_count: OrpShopCount
    boss_count: BossCount

option_groups = [
    OptionGroup("Combat", [FarBattleCount, OrpBattleCount]),
    OptionGroup("Chests", [FarMoneyChests, OrpMoneyChests, FarArtifactChests, OrpArtifactChests]),
    OptionGroup("Shops", [FarShopCount, OrpShopCount]),
    OptionGroup("Bosses", [BossCount]),
]
