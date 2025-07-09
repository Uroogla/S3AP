import typing
from dataclasses import dataclass
from Options import Toggle, DefaultOnToggle, Option, Range, Choice, ItemDict, DeathLink, PerGameCommonOptions

SORCERESS_ONE = 0
EGG_FOR_SALE = 1
SORCERESS_TWO = 2
SUNNY_VILLA = 3

class GuaranteedItemsOption(ItemDict):
    """Guarantees that the specified items will be in the item pool"""
    display_name = "Guaranteed Items"

class EnableGemChecksOption(Toggle):
    """Adds checks for getting all gems in a level"""
    display_name = "Enable Gem Checks"

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
    option_sunny_villa = SUNNY_VILLA

@dataclass
class Spyro3Option(PerGameCommonOptions):
    goal: GoalOption
    guaranteed_items: GuaranteedItemsOption
    enable_gem_checks: EnableGemChecksOption
