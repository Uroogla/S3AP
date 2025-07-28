from enum import IntEnum
from typing import NamedTuple
from BaseClasses import Item
from .Options import MoneybagsOptions, SparxUpgradeOptions


class Spyro3ItemCategory(IntEnum):
    EGG = 0,
    SKIP = 1,
    EVENT = 2,
    MISC = 3,
    TRAP = 4,
    SKILLPOINT_GOAL = 5,
    MONEYBAGS = 6,
    HINT = 7


class Spyro3ItemData(NamedTuple):
    name: str
    s3_code: int
    category: Spyro3ItemCategory


class Spyro3Item(Item):
    game: str = "Spyro 3"

    @staticmethod
    def get_name_to_id() -> dict:
        base_id = 1230000
        return {item_data.name: (base_id + item_data.s3_code if item_data.s3_code is not None else None) for item_data in _all_items}


key_item_names = {
    "Egg"
}


_all_items = [Spyro3ItemData(row[0], row[1], row[2]) for row in [    
    ("Sunny Villa Complete", 2000, Spyro3ItemCategory.EVENT),
    ("Cloud Spires Complete", 2001, Spyro3ItemCategory.EVENT),
    ("Molten Crater Complete", 2002, Spyro3ItemCategory.EVENT),
    ("Seashell Shore Complete", 2003, Spyro3ItemCategory.EVENT),
    ("Sheila's Alp Complete", 2004, Spyro3ItemCategory.EVENT),
    ("Buzz Defeated", 2005, Spyro3ItemCategory.EVENT),
    ("Crawdad Farm Complete", 2006, Spyro3ItemCategory.EVENT),
    
    ("Icy Peak Complete", 2007, Spyro3ItemCategory.EVENT),
    ("Enchanted Towers Complete", 2008, Spyro3ItemCategory.EVENT),
    ("Spooky Swamp Complete", 2009, Spyro3ItemCategory.EVENT),
    ("Bamboo Terrace Complete", 2010, Spyro3ItemCategory.EVENT),
    ("Sgt. Byrd's Base Complete", 2011, Spyro3ItemCategory.EVENT),
    ("Spike Defeated", 2012, Spyro3ItemCategory.EVENT),
    ("Spider Town Complete", 2013, Spyro3ItemCategory.EVENT),
    
    ("Frozen Altars Complete", 2014, Spyro3ItemCategory.EVENT),
    ("Lost Fleet Complete", 2015, Spyro3ItemCategory.EVENT),
    ("Fireworks Factory Complete", 2016, Spyro3ItemCategory.EVENT),
    ("Charmed Ridge Complete", 2017, Spyro3ItemCategory.EVENT),
    ("Bentley's Outpost Complete", 2018, Spyro3ItemCategory.EVENT),
    ("Scorch Defeated", 2019, Spyro3ItemCategory.EVENT),
    ("Starfish Reef Complete", 2020, Spyro3ItemCategory.EVENT),
    
    ("Crystal Islands Complete", 2021, Spyro3ItemCategory.EVENT),
    ("Desert Ruins Complete", 2022, Spyro3ItemCategory.EVENT),
    ("Haunted Tomb Complete", 2023, Spyro3ItemCategory.EVENT),
    ("Dino Mines Complete", 2024, Spyro3ItemCategory.EVENT),
    ("Agent 9's Lab Complete", 2025, Spyro3ItemCategory.EVENT),
    ("Sorceress Defeated", 2026, Spyro3ItemCategory.EVENT),
    ("Bugbot Factory Complete", 2027, Spyro3ItemCategory.EVENT),
    ("Super Bonus Round Complete", 2028, Spyro3ItemCategory.EVENT),
    ("Moneybags Chase Complete", 2029, Spyro3ItemCategory.EVENT),
    
    
    ("Egg", 1000, Spyro3ItemCategory.EGG),
    ("Extra Life", 1001, Spyro3ItemCategory.MISC),
    ("Lag Trap", 1002, Spyro3ItemCategory.TRAP),
    ("Filler", 1003, Spyro3ItemCategory.MISC),
    ("Damage Sparx Trap", 1004, Spyro3ItemCategory.TRAP),
    ("Sparxless Trap", 1005, Spyro3ItemCategory.TRAP),
    ("Invincibility (15 seconds)", 1006, Spyro3ItemCategory.MISC),
    ("Invincibility (30 seconds)", 1007, Spyro3ItemCategory.MISC),
    ("Turn Spyro Red", 1008, Spyro3ItemCategory.MISC),
    ("Turn Spyro Blue", 1009, Spyro3ItemCategory.MISC),
    ("Turn Spyro Pink", 1010, Spyro3ItemCategory.MISC),
    ("Turn Spyro Yellow", 1011, Spyro3ItemCategory.MISC),
    ("Turn Spyro Green", 1012, Spyro3ItemCategory.MISC),
    ("Turn Spyro Black", 1013, Spyro3ItemCategory.MISC),
    ("Big Head Mode", 1014, Spyro3ItemCategory.MISC),
    ("Flat Spyro Mode", 1015, Spyro3ItemCategory.MISC),
    ("(Over)heal Sparx", 1016, Spyro3ItemCategory.MISC),
    ("Progressive Sparx Health Upgrade", 1017, Spyro3ItemCategory.MISC),
    ("Skill Point", 1018, Spyro3ItemCategory.SKILLPOINT_GOAL),

    ("Moneybags Unlock - Cloud Spires Bellows", 3000, Spyro3ItemCategory.MONEYBAGS),
    ("Moneybags Unlock - Spooky Swamp Door", 3001, Spyro3ItemCategory.MONEYBAGS),
    ("Moneybags Unlock - Sheila", 3002, Spyro3ItemCategory.MONEYBAGS),
    ("Moneybags Unlock - Icy Peak Nancy Door", 3003, Spyro3ItemCategory.MONEYBAGS),
    ("Moneybags Unlock - Molten Crater Thieves Door", 3004, Spyro3ItemCategory.MONEYBAGS),
    ("Moneybags Unlock - Charmed Ridge Stairs", 3005, Spyro3ItemCategory.MONEYBAGS),
    ("Moneybags Unlock - Sgt. Byrd", 3006, Spyro3ItemCategory.MONEYBAGS),
    ("Moneybags Unlock - Bentley", 3007, Spyro3ItemCategory.MONEYBAGS),
    ("Moneybags Unlock - Desert Ruins Door", 3008, Spyro3ItemCategory.MONEYBAGS),
    ("Moneybags Unlock - Agent 9", 3009, Spyro3ItemCategory.MONEYBAGS),
    ("Moneybags Unlock - Frozen Altars Cat Hockey Door", 3010, Spyro3ItemCategory.MONEYBAGS),
    ("Moneybags Unlock - Crystal Islands Bridge", 3011, Spyro3ItemCategory.MONEYBAGS),

    ("Hint 1", 4000, Spyro3ItemCategory.HINT),
    ("Hint 2", 4001, Spyro3ItemCategory.HINT),
    ("Hint 3", 4002, Spyro3ItemCategory.HINT),
    ("Hint 4", 4003, Spyro3ItemCategory.HINT),
    ("Hint 5", 4004, Spyro3ItemCategory.HINT),
    ("Hint 6", 4005, Spyro3ItemCategory.HINT),
    ("Hint 7", 4006, Spyro3ItemCategory.HINT),
    ("Hint 8", 4007, Spyro3ItemCategory.HINT),
    ("Hint 9", 4008, Spyro3ItemCategory.HINT),
    ("Hint 10", 4009, Spyro3ItemCategory.HINT),
    ("Hint 11", 4010, Spyro3ItemCategory.HINT),
]]

item_descriptions = {}

item_dictionary = {item_data.name: item_data for item_data in _all_items}

def BuildItemPool(multiworld, count, preplaced_eggs, options):
    item_pool = []
    included_itemcount = 0

    if options.guaranteed_items.value:
        for item_name in options.guaranteed_items.value:
            item = item_dictionary[item_name]
            item_pool.append(item)
            included_itemcount = included_itemcount + 1
    remaining_count = count - included_itemcount
    eggs_to_place = 150 - preplaced_eggs
    for i in range(eggs_to_place):
        item_pool.append(item_dictionary["Egg"])
    remaining_count = remaining_count - eggs_to_place

    if options.moneybags_settings.value in [MoneybagsOptions.COMPANIONSANITY, MoneybagsOptions.MONEYBAGSSANITY]:
        item_pool.append(item_dictionary["Moneybags Unlock - Sheila"])
        item_pool.append(item_dictionary["Moneybags Unlock - Sgt. Byrd"])
        item_pool.append(item_dictionary["Moneybags Unlock - Bentley"])
        item_pool.append(item_dictionary["Moneybags Unlock - Agent 9"])
        remaining_count = remaining_count - 4;
    if options.moneybags_settings.value == MoneybagsOptions.MONEYBAGSSANITY:
        item_pool.append(item_dictionary["Moneybags Unlock - Cloud Spires Bellows"])
        item_pool.append(item_dictionary["Moneybags Unlock - Spooky Swamp Door"])
        item_pool.append(item_dictionary["Moneybags Unlock - Icy Peak Nancy Door"])
        item_pool.append(item_dictionary["Moneybags Unlock - Molten Crater Thieves Door"])
        item_pool.append(item_dictionary["Moneybags Unlock - Charmed Ridge Stairs"])
        item_pool.append(item_dictionary["Moneybags Unlock - Desert Ruins Door"])
        item_pool.append(item_dictionary["Moneybags Unlock - Frozen Altars Cat Hockey Door"])
        item_pool.append(item_dictionary["Moneybags Unlock - Crystal Islands Bridge"])
        remaining_count = remaining_count - 8;
    
    if options.enable_progressive_sparx_health in [SparxUpgradeOptions.BLUE, SparxUpgradeOptions.GREEN, SparxUpgradeOptions.SPARXLESS]:
        item_pool.append(item_dictionary["Progressive Sparx Health Upgrade"])
        remaining_count = remaining_count - 1
    if options.enable_progressive_sparx_health in [SparxUpgradeOptions.GREEN, SparxUpgradeOptions.SPARXLESS]:
        item_pool.append(item_dictionary["Progressive Sparx Health Upgrade"])
        remaining_count = remaining_count - 1
    if options.enable_progressive_sparx_health in [SparxUpgradeOptions.SPARXLESS]:
        item_pool.append(item_dictionary["Progressive Sparx Health Upgrade"])
        remaining_count = remaining_count - 1


    if remaining_count < 0:
        raise Exception(f"The options you have selected require at least {remaining_count * -1} more checks to be enabled.")

    # Build a weighted list of allowed filler items.  Make changing Spyro's color in general the same weight as other items.
    allowed_filler_items = []
    allowed_misc_items = []
    allowed_trap_items = []

    for item in _all_items:
        if item.name == 'Extra Life' and options.enable_filler_extra_lives:
            for i in range(0, 6):
                allowed_misc_items.append(item)
        elif item.name.startswith('Invincibility (') and options.enable_filler_invincibility:
            for i in range(0, 3):
                allowed_misc_items.append(item)
        elif item.name.startswith('Turn Spyro ') and options.enable_filler_color_change:
            allowed_misc_items.append(item)
        elif (item.name == 'Big Head Mode' or item.name == 'Flat Spyro Mode') and options.enable_filler_big_head_mode:
            for i in range(0, 3):
                allowed_misc_items.append(item)
        elif item.name == '(Over)heal Sparx' and options.enable_filler_heal_sparx:
            for i in range(0, 6):
                allowed_misc_items.append(item)
        elif item.name == 'Damage Sparx Trap' and options.enable_trap_damage_sparx:
            allowed_trap_items.append(item)
        elif item.name == 'Sparxless Trap' and options.enable_trap_sparxless:
            allowed_trap_items.append(item)

    if remaining_count > 0 and options.trap_filler_percent.value > 0 and len(allowed_trap_items) == 0:
        raise Exception(f"Trap percentage is set to {options.trap_filler_percent.value}, but none have been turned on.")
    if remaining_count > 0 and options.trap_filler_percent.value < 100 and len(allowed_misc_items) == 0:
        raise Exception(f"{100 - options.trap_filler_percent.value} percent of filler items are meant to be non-traps, but no non-trap items have been turned on.")

    # Get the correct blend of traps and filler items.
    for i in range(remaining_count):
        if multiworld.random.random() * 100 < options.trap_filler_percent.value:
            itemList = [item for item in allowed_trap_items]
        else:
            itemList = [item for item in allowed_misc_items]
        item = multiworld.random.choice(itemList)
        item_pool.append(item)
    
    multiworld.random.shuffle(item_pool)
    return item_pool
