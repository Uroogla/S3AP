import typing
from dataclasses import dataclass
from Options import Toggle, DefaultOnToggle, Option, Range, Choice, ItemDict, DeathLink, PerGameCommonOptions, OptionGroup

class GoalOptions():
    SORCERESS_ONE = 0
    EGG_FOR_SALE = 1
    SORCERESS_TWO = 2
    ALL_SKILLPOINTS = 3
    EPILOGUE = 4

class MoneybagsOptions():
    VANILLA = 0
    COMPANIONSANITY = 1
    # Reserve 2 for shuffling moneybags prices on companions
    MONEYBAGSSANITY = 3

class SparxUpgradeOptions():
    OFF = 0
    BLUE = 1
    GREEN = 2
    SPARXLESS = 3
    TRUE_SPARXLESS = 4


class GoalOption(Choice):
    """Lets the user choose the completion goal
    Sorceress 1 - Beat the sorceress *and* obtain 100 eggs
    Egg For Sale - Chase Moneybags after defeating the sorceress the first time.
    Sorceress 2 - Beat the sorceress in Super Bonus Round
    All Skillpoints - Collect all 20 skill points in the game. Excluded locations are still required for this goal.
    Epilogue - Unlock the full epilogue by collecting all 20 skill points and defeating the sorceress. Excluded locations are still required for this goal."""
    display_name = "Completion Goal"
    default = GoalOptions.SORCERESS_ONE
    option_sorceress_1 = GoalOptions.SORCERESS_ONE
    option_egg_for_sale = GoalOptions.EGG_FOR_SALE
    option_sorceress_2 = GoalOptions.SORCERESS_TWO
    option_all_skillpoints = GoalOptions.ALL_SKILLPOINTS
    option_epilogue = GoalOptions.EPILOGUE

class GuaranteedItemsOption(ItemDict):
    """Guarantees that the specified items will be in the item pool"""
    display_name = "Guaranteed Items"

class Enable25PctGemChecksOption(Toggle):
    """Adds checks for getting 25% of the gems in a level"""
    display_name = "Enable 25% Gem Checks"

class Enable50PctGemChecksOption(Toggle):
    """Adds checks for getting 50% of the gems in a level"""
    display_name = "Enable 50% Gem Checks"

class Enable75PctGemChecksOption(Toggle):
    """Adds checks for getting 75% of the gems in a level"""
    display_name = "Enable 75% Gem Checks"

class EnableGemChecksOption(Toggle):
    """Adds checks for getting all gems in a level"""
    display_name = "Enable 100% Gem Checks"

class EnableTotalGemChecksOption(Toggle):
    """Adds checks for every 500 gems you collect total.
    NOTE: Gems currently paid to Moneybags do not count towards your total.
    Logic assumes you pay Moneybags everywhere you can so you cannot be locked out of checks."""
    display_name = "Enable Total Gem Count Checks"

class MaxTotalGemCheckOption(Range):
    """Sets the highest number of total gems that can be required for Total Gem Count checks.
    This has no effect if Enable Total Gem Count Checks is disabled."""
    display_name = "Max for Total Gem Count Checks"
    range_start = 500
    range_end = 20000
    default = 6000

class EnableSkillpointChecksOption(Toggle):
    """Adds checks for getting skill points"""
    display_name = "Enable Skillpoint Checks"

class EnableLifeBottleChecksOption(Toggle):
    """Adds checks for breaking life bottles.
    This does not include the 3 bottles on the impossible island in Midnight Mountain.
    This does include the life bottle stuck in a wall in Fireworks Factory.
    To disable this bottle specifically, disable the location 'Fireworks Factory: Life Bottle Out of Bounds Near Start'"""
    display_name = "Enable Life Bottle Checks"

class MoneybagsSettings(Choice):
    """Determines settings for Moneybags unlocks.
    WARNING - This feature is in beta, and there are a few known locks and crashes based on the timing
    of companion unlocks.  Be sure to have autosave on if using these options.
    Vanilla - Pay Moneybags to progress as usual
    Companionsanity - You cannot pay for side characters and must find unlock items to progress.
    Moneybagssanity - You cannot pay Moneybags at all and must find unlock items to progress."""
    display_name = "Moneybags Settings"
    default = MoneybagsOptions.VANILLA
    option_vanilla = MoneybagsOptions.VANILLA
    option_companionsanity = MoneybagsOptions.COMPANIONSANITY
    option_moneybagssanity = MoneybagsOptions.MONEYBAGSSANITY

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

class EnableProgressiveSparxHealth(Choice):
    """Start the game with lower max health and add items to the pool to increase your max health.
    The Starfish Reef health upgrade will have no effect until you find all Progressive Sparx Health Upgrade items.
    Off - The game behaves normally.
    Blue - Your max health starts at blue Sparx, and 1 upgrade is added to the pool.
    Green - Your max health starts at green Sparx, and 2 upgrades are added to the pool.
    Sparxless - Your max health starts at no Sparx, and 3 upgrades are added to the pool.
    True Sparxless - Your max health is permanently Sparxless.  No upgrades are added to the pool."""
    display_name = "Enable Progressive Sparx Health Upgrades"
    default = SparxUpgradeOptions.OFF
    option_off = SparxUpgradeOptions.OFF
    option_blue = SparxUpgradeOptions.BLUE
    option_green = SparxUpgradeOptions.GREEN
    option_sparxless = SparxUpgradeOptions.SPARXLESS
    option_true_sparxless = SparxUpgradeOptions.TRUE_SPARXLESS

class ZoeGivesHints(Range):
    """Enables some or all of the 11 Tutorial Zoes across Sunrise Spring and its levels giving hints.
    Which Zoes give hints are random.  Those in Crawdad Farm never will, as this tutorial can be accessed only once.
    Hints fit into 3 categories, with hints evenly distributed between the categories:
    - Difficult or slow locations
    - Progression items in your world, with a preference for non-eggs.
    - Joke hints."""
    display_name = "Number of Zoe Hints"
    range_start = 0
    range_end = 11
    default = 0

class LogicSunnySheilaEarly(Toggle):
    """Puts entering the Sheila sub-area of Sunny Villa without completing Sheila into logic.
    This requires jumps to the top of the side area "hut" or entering from behind.
    NOTE: Entering this area from behind may crash the game if done incorrectly.
    This option only matters is Companionsanity or Moneybagssanity is turned on."""
    display_name = "Enter Sunny Villa Sheila Area Early"

class LogicCloudBackwards(Toggle):
    """Puts completing Cloud Spires backwards without paying Moneybags into logic.
    This requires one of two jumps to the end of the level.
    For all gems, an additional jump from near the egg 'Cloud Spires: Glide to the island. (Clare)' is needed.
    This option only matters if Moneybagssanity is turned on."""
    display_name = "Cloud Spires Backwards"

class LogicMoltenEarly(Toggle):
    """Puts entering Molten Crater from out of bounds without 10 eggs into logic.
    This requires either a swim in air trick, or getting onto the wall alongside Molten."""
    display_name = "Enter Molten Crater Early"

class LogicMoltenByrdEarly(Toggle):
    """Puts entering Molten Crater's Sgt. Byrd sub-area without completing Sgt. Byrd into logic.
    This requires jumping on the bridge's posts and then onto the top of his hut."""
    display_name = "Enter Molten Crater Sgt. Byrd Area Early"

class LogicMoltenThievesNoMoneybags(Toggle):
    """Puts entering Molten Crater's thieves sub-area without paying Moneybags into logic.
    This requires bouncing off the first boar in the level above the trees and entering the area from behind.
    This option only matters if Moneybagssanity is turned on."""
    display_name = "Enter Molten Crater Thieves without Moneybags"

class LogicSeashellEarly(Toggle):
    """Puts entering Seashell Shores from out of bounds without 14 eggs into logic.
    This requires a swim in air trick."""
    display_name = "Enter Seashell Shores Early"

class LogicMushroomEarly(Toggle):
    """Puts entering Mushroom Speedway from out of bounds without 20 eggs into logic.
    This requires a swim in air trick or getting onto the wall by Mushroom Speedway and Molten Crater."""
    display_name = "Enter Mushroom Speedway Early"

class LogicSheilaEarly(Toggle):
    """Puts entering Sheila's Alp from out of bounds without paying Moneybags into logic.
    This requires a swim in air trick and can result in a softlock if you end up in Sheila's cage.
    This option only matters if Companionsanity or Moneybagssanity is turned on."""
    display_name = "Enter Sheila's Alp Early"

class LogicSpookyEarly(Toggle):
    """Puts entering Spooky Swamp from out of bounds without 25 eggs into logic.
    This requires a swim in air trick or one of various glides from out of bounds."""
    display_name = "Enter Spooky Swamp Early"

class LogicSpookyNoMoneybags(Toggle):
    """Puts skipping Moneybags in Spooky Swamp into logic.
    This can be done by damage boosting across the water near 'Spooky Swamp: Jump to the island. (Michael)' to the end of level.
    This option only matters if Moneybagssanity is turned on."""
    display_name = "Skip Moneybags in Spooky Swamp"

class LogicBambooEarly(Toggle):
    """Puts entering Bamboo Terrace from out of bounds without 30 eggs into logic.
    This requires a swim in air trick or a glide from out of bounds."""
    display_name = "Enter Bamboo Terrace Early"

class LogicBambooBentleyEarly(Toggle):
    """Puts entering the Bentley sub-area of Bamboo Terrace without completing Bentley's Outpost into logic.
    This can be done with a swim in air."""
    display_name = "Enter Bamboo Terrace Bentley Area Early"

class LogicCountryEarly(Toggle):
    """Puts entering Country Speedway from out of bounds without 36 eggs into logic.
    This requires a swim in air trick."""
    display_name = "Enter Country Speedway Early"

class LogicByrdEarly(Toggle):
    """Puts entering Sgt. Byrd's Base from out of bounds without paying Moneybags into logic.
    This requires a swim in air trick or a glide out of bounds.
    This option only matters if Companionsanity or Moneybagssanity is turned on."""
    display_name = "Enter Sgt. Byrd's Base Early"

class LogicFrozenBentleyEarly(Toggle):
    """Puts entering Frozen Altars' Bentley sub-area from out of bounds without completing Bentley's Outpost into logic.
    This requires a proxy off a mammoth or getting onto the wall near the area and gliding from out of bounds.
    This option only matters if Companionsanity or Moneybagssaniity is turned on."""
    display_name = "Enter Frozen Altars Bentley Area Early"

class LogicFrozenCatHockeyNoMoneybags(Toggle):
    """Puts entering Frozen Altars' Cat Hockey sub-area from out of bounds without paying Moneybags into logic.
    This requires a proxy.
    This option only matters if Moneybagssanity is turned on."""
    display_name = "Enter Frozen Altars Cat Hockey Area without Moneybags"

class LogicFireworksEarly(Toggle):
    """Puts entering Fireworks Factory from out of bounds without 50 eggs into logic.
    This requires a zombie swim in air or a glide out of bounds."""
    display_name = "Enter Fireworks Factory Early"

class LogicFireworksAgent9Early(Toggle):
    """Puts entering Fireworks Factory's Agent 9 sub-area without completing Agent 9's Lab into logic.
    This requires a careful glide to the right 'antenna' of the sub-area hut."""
    display_name = "Enter Fireworks Factory Agent 9 Area Early"

class LogicCharmedEarly(Toggle):
    """Puts entering Charmed Ridge from out of bounds without 58 eggs into logic.
    This requires a zombie swim in air or a glide out of bounds."""
    display_name = "Enter Charmed Ridge Early"

class LogicCharmedNoMoneybags(Toggle):
    """Puts getting past the stairs in Charmed Ridge without paying Moneybags into logic.
    This requires gliding through a section of wall with no collision.
    NOTE: A proxy allows partial access to the second half of the level but not full access.
    This option only matters if Moneybagssanity is turned on."""
    display_name = "Pass Charmed Ridge Stairs without Moneybags"

class LogicHoneyEarly(Toggle):
    """Puts entering Honey Speedway from out of bounds without 65 eggs into logic.
    This requires a zombie swim in air or a glide out of bounds."""
    display_name = "Enter Honey Speedway Early"

class LogicBentleyEarly(Toggle):
    """Puts entering Bentley's Outpost from out of bounds without paying Moneybags into logic.
    This requires a zombie swim in air or a glide out of bounds.
    This option only matters if Companionsanity or Moneybagssanity is turned on."""
    display_name = "Enter Bentley's Outpost Early"

class LogicCrystalNoMoneybags(Toggle):
    """Puts fully completing Crystal Islands without paying Moneybags or beating the Sorceress into logic.
    This requires a swim in air in the pool before the whirlwind.
    This option only matters if Moneybagssanity is turned on."""
    display_name = "Complete Crystal Islands without Moneybags"

class LogicDesertNoMoneybags(Toggle):
    """Puts fully completing Desert Ruins without paying Moneybags or beating the Sorceress into logic.
    This requires a proxy off a scorpion or one of several terrain jumps to get to the end of level.
    This option only matters if Moneybagssanity is turned on."""
    display_name = "Complete Desert Ruins without Moneybags"

class LogicSorceressEarly(Toggle):
    """Puts defeating the Sorceress without 100 eggs into logic.
    This requires a proxy on the Desert Ruins helmet, or a series of difficult terrain jumps.
    100 eggs are still required for the Sorceress 1 goal, but this setting probably only makes sense for Sorceress 2 or other niche goals."""
    display_name = "Enter Sorceress' Lair Early"


@dataclass
class Spyro3Option(PerGameCommonOptions):
    goal: GoalOption
    guaranteed_items: GuaranteedItemsOption
    enable_25_pct_gem_checks: Enable25PctGemChecksOption
    enable_50_pct_gem_checks: Enable50PctGemChecksOption
    enable_75_pct_gem_checks: Enable75PctGemChecksOption
    enable_gem_checks: EnableGemChecksOption
    enable_total_gem_checks: EnableTotalGemChecksOption
    max_total_gem_checks: MaxTotalGemCheckOption
    enable_skillpoint_checks: EnableSkillpointChecksOption
    # TODO: Enable once memory addresses are fixed in client.
    #enable_life_bottle_checks: EnableLifeBottleChecksOption
    moneybags_settings: MoneybagsSettings
    enable_filler_extra_lives: EnableFillerExtraLives
    enable_filler_invincibility: EnableFillerInvincibility
    enable_filler_color_change: EnableFillerColorChange
    enable_filler_big_head_mode: EnableFillerBigHeadMode
    enable_filler_heal_sparx: EnableFillerHealSparx
    trap_filler_percent: TrapFillerPercent
    enable_trap_damage_sparx: EnableTrapDamageSparx
    enable_trap_sparxless: EnableTrapSparxless
    enable_progressive_sparx_health: EnableProgressiveSparxHealth
    zoe_gives_hints: ZoeGivesHints
    logic_sunny_sheila_early: LogicSunnySheilaEarly
    logic_cloud_backwards: LogicCloudBackwards
    logic_molten_early: LogicMoltenEarly
    logic_molten_byrd_early: LogicMoltenByrdEarly
    logic_molten_thieves_no_moneybags: LogicMoltenThievesNoMoneybags
    logic_seashell_early: LogicSeashellEarly
    logic_mushroom_early: LogicMushroomEarly
    logic_sheila_early: LogicSheilaEarly
    logic_spooky_early: LogicSpookyEarly
    logic_spooky_no_moneybags: LogicSpookyNoMoneybags
    logic_bamboo_early: LogicBambooEarly
    logic_bamboo_bentley_early: LogicBambooBentleyEarly
    logic_country_early: LogicCountryEarly
    logic_byrd_early: LogicByrdEarly
    logic_frozen_bentley_early: LogicFrozenBentleyEarly
    logic_frozen_cat_hockey_no_moneybags: LogicFrozenCatHockeyNoMoneybags
    logic_fireworks_early: LogicFireworksEarly
    logic_fireworks_agent_9_early: LogicFireworksAgent9Early
    logic_charmed_early: LogicCharmedEarly
    logic_charmed_no_moneybags: LogicCharmedNoMoneybags
    logic_honey_early: LogicHoneyEarly
    logic_bentley_early: LogicBentleyEarly
    logic_crystal_no_moneybags: LogicCrystalNoMoneybags
    logic_desert_no_moneybags: LogicDesertNoMoneybags
    logic_sorceress_early: LogicSorceressEarly

# Group logic/trick options together, espeically for the local WebHost.
spyro_options_groups = [
    OptionGroup(
        "Tricks",
        [
            LogicSunnySheilaEarly,
            LogicCloudBackwards,
            LogicMoltenEarly,
            LogicMoltenByrdEarly,
            LogicMoltenThievesNoMoneybags,
            LogicSeashellEarly,
            LogicMushroomEarly,
            LogicSheilaEarly,
            LogicSpookyEarly,
            LogicSpookyNoMoneybags,
            LogicBambooEarly,
            LogicBambooBentleyEarly,
            LogicCountryEarly,
            LogicByrdEarly,
            LogicFrozenBentleyEarly,
            LogicFrozenCatHockeyNoMoneybags,
            LogicFireworksEarly,
            LogicFireworksAgent9Early,
            LogicCharmedEarly,
            LogicCharmedNoMoneybags,
            LogicHoneyEarly,
            LogicBentleyEarly,
            LogicCrystalNoMoneybags,
            LogicDesertNoMoneybags,
            LogicSorceressEarly
        ],
        True
    ),
]
