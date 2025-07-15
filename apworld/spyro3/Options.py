import typing
from dataclasses import dataclass
from Options import Toggle, DefaultOnToggle, Option, Range, Choice, ItemDict, DeathLink, PerGameCommonOptions

SORCERESS_ONE = 0
EGG_FOR_SALE = 1
SORCERESS_TWO = 2
# Test goal for ease of debugging
#SUNNY_VILLA = 3

class GuaranteedItemsOption(ItemDict):
    """Guarantees that the specified items will be in the item pool"""
    display_name = "Guaranteed Items"

class EnableGemChecksOption(Toggle):
    """Adds checks for getting all gems in a level"""
    display_name = "Enable Gem Checks"

class EnableGemsanityOption(Toggle):
    """Adds each gem in the game as a check.  Currently proof of concept, for Sunrise Spring Home only."""
    display_name = "Enable Full Gemsanity"

class EnableSkillpointChecksOptions(Toggle):
    """Adds checks for getting skill points"""
    display_name = "Enable Skillpoint Checks"

class EnableFillerExtraLives(DefaultOnToggle):
    """Allows filler items to include extra lives"""
    display_name = "Enable Extra Lives Filler"

class EnableFillerInvincibility(Toggle):
    """Allows filler items to include temporary invincibility"""
    display_name = "Enable Temporary Invincibility Filler"

class EnableFillerColorChange(Toggle):
    """Allows filler items to include changing Spyro's color"""
    display_name = "Enable Changing Spyro's Color Filler"

class EnableFillerBigHeadMode(Toggle):
    """Allows filler items to include turning on Big Head Mode and Flat Spyro Mode"""
    display_name = "Enable Big Head and Flat Spyro Filler"

class EnableFillerHealSparx(Toggle):
    """Allows filler items to include healing Sparx. Can exceed max health."""
    display_name = "Enable (over)healing Sparx Filler"

class TrapFillerPercent(Range):
    """Determines the percentage of filler items that will be traps."""
    display_name = "Trap Percentage of Filler"
    range_start = 0
    range_end = 100
    default = 0

class EnableTrapDamageSparx(Toggle):
    """Allows filler items to include damaging Sparx. Cannot directly kill Spyro."""
    display_name = "Enable Hurting Sparx Trap"

class EnableTrapSparxless(Toggle):
    """Allows filler items to include removing Sparx."""
    display_name = "Enable Sparxless Trap"

class GoalOption(Choice):
    """Lets the user choose the completion goal
    Sorceress 1 - Beat the sorceress and obtain 100 eggs
    Egg For Sale - Chase Moneybags after defeating the sorceress the first time.
    Sorceress 2 - Beat the sorceress a second time"""
    display_name = "Completion Goal"
    default = SORCERESS_ONE
    option_sorceress_1 = SORCERESS_ONE
    option_egg_for_sale = EGG_FOR_SALE
    option_sorceress_2 = SORCERESS_TWO
    # Test goal for ease of debugging
    #option_sunny_villa = SUNNY_VILLA

@dataclass
class Spyro3Option(PerGameCommonOptions):
    goal: GoalOption
    guaranteed_items: GuaranteedItemsOption
    enable_gem_checks: EnableGemChecksOption
    enable_gemsanity_checks: EnableGemsanityOption
    enable_skillpoint_checks: EnableSkillpointChecksOptions
    enable_filler_extra_lives: EnableFillerExtraLives
    enable_filler_invincibility: EnableFillerInvincibility
    enable_filler_color_change: EnableFillerColorChange
    enable_filler_big_head_mode: EnableFillerBigHeadMode
    enable_filler_heal_sparx: EnableFillerHealSparx
    trap_filler_percent: TrapFillerPercent
    enable_trap_damage_sparx: EnableTrapDamageSparx
    enable_trap_sparxless: EnableTrapSparxless
