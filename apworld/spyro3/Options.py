import typing
from dataclasses import dataclass
from Options import Toggle, DefaultOnToggle, Option, Range, Choice, ItemDict, DeathLink, PerGameCommonOptions



class GuaranteedItemsOption(ItemDict):
    """Guarantees that the specified items will be in the item pool"""
    display_name = "Guaranteed Items"

class EnableGemChecksOption(Toggle):
    """Adds checks for getting all gems in a level"""
    display_name = "Enable Gem Checks"

class GoalOption(Choice):
    """Lets the user choose the completion goal
    Sorceress 1 - Beat the sorceress and obtain 100 eggs
    Sorceress 2 - Beat the sorceress a second time"""
    display_name = "Completion Goal"
    default = 0
    option_sorceress_1 = 0
    option_sunny_villa = 1
    option_sorceress_2 = 2

@dataclass
class Spyro3Option(PerGameCommonOptions):
    goal: GoalOption
    guaranteed_items: GuaranteedItemsOption
    enable_gem_checks: EnableGemChecksOption