from dataclasses import dataclass
from Options import Range, Toggle, PerGameCommonOptions, OptionGroup

class Deathlink(Toggle):
    """WHen you die (whole party wipes), everyone dies. And vice versa"""
    display_name = "Deathlink"

class WinCount(Range):
    """Number of final boss victories required to complete the goal."""
    display_name = "Win Count"
    range_start = 1
    range_end = 10
    default = 1

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

class Hardmode(Toggle):
    """Enable hardmode (Garden) locations and items."""
    display_name = "Hardmode"
    default = True

class GardenBattleCount(Range):
    """Number of battles in the Garden."""
    display_name = "Garden Battles"
    range_start = 0
    range_end = 50
    default = 10

class GardenMoneyChests(Range):
    """Number of money chests in the Garden."""
    display_name = "Garden Money Chests"
    range_start = 0
    range_end = 10
    default = 2

class GardenArtifactChests(Range):
    """Number of artifact chests in the Garden."""
    display_name = "Garden Artifact Chests"
    range_start = 0
    range_end = 10
    default = 2

class GardenShopCount(Range):
    """Number of shop checks in the Garden."""
    display_name = "Garden Shops"
    range_start = 0
    range_end = 100
    default = 6

class MordrakeQuest(Toggle):
    """Include Mordrake's Untold Tale questline."""
    display_name = "Mordrake's Quest"
    default = False

class BronzoQuest(Toggle):
    """Include Bronzo's grinding questline (6 locations)."""
    display_name = "Bronzo's Quest"
    default = False

class DirectorQuest(Toggle):
    """Include The Director's Final Frame quest (1 location)."""
    display_name = "Director's Quest"
    default = False

class StartMoneyCount(Range):
    """Number of Progressive Start Money items in the pool. Each adds +10 starting coins."""
    display_name = "Progressive Start Money items"
    range_start = 0
    range_end = 10
    default = 3

@dataclass
class BrutalOrchestraOptions(PerGameCommonOptions):
    death_link: Deathlink
    win_count: WinCount
    far_battle_count: FarBattleCount
    orp_battle_count: OrpBattleCount
    far_money_chests: FarMoneyChests
    orp_money_chests: OrpMoneyChests
    far_artifact_chests: FarArtifactChests
    orp_artifact_chests: OrpArtifactChests
    far_shop_count: FarShopCount
    orp_shop_count: OrpShopCount
    boss_count: BossCount
    hardmode: Hardmode
    garden_battle_count: GardenBattleCount
    garden_money_chests: GardenMoneyChests
    garden_artifact_chests: GardenArtifactChests
    garden_shop_count: GardenShopCount
    mordrake_quest: MordrakeQuest
    bronzo_quest: BronzoQuest
    director_quest: DirectorQuest
    start_money_count: StartMoneyCount

option_groups = [
    OptionGroup("Goal", [WinCount]),
    OptionGroup("Combat", [FarBattleCount, OrpBattleCount]),
    OptionGroup("Chests", [FarMoneyChests, OrpMoneyChests, FarArtifactChests, OrpArtifactChests]),
    OptionGroup("Shops", [FarShopCount, OrpShopCount]),
    OptionGroup("Bosses", [BossCount]),
    OptionGroup("Hardmode", [Hardmode, GardenBattleCount, GardenMoneyChests, GardenArtifactChests, GardenShopCount]),
    OptionGroup("Optional Quests", [BronzoQuest, DirectorQuest, MordrakeQuest]),
]
