import typing
from dataclasses import dataclass
from Options import Toggle, DefaultOnToggle, Option, Range, Choice, ItemDict, DeathLink, PerGameCommonOptions



class GuaranteedItemsOption(ItemDict):
    """Guarantees that the specified items will be in the item pool"""
    display_name = "Guaranteed Items"

class EnableGemChecksOption(Toggle):
    """Adds checks for getting all gems in a level"""
    display_name = "Enable Gem Checks"

@dataclass
class Spyro3Option(PerGameCommonOptions):
    #goal: GoalOption
    guaranteed_items: GuaranteedItemsOption
    enable_gem_checks: EnableGemChecksOption