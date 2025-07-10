from enum import IntEnum
from typing import NamedTuple
from BaseClasses import Item


class Spyro3ItemCategory(IntEnum):
    EGG = 0,
    SKIP = 1,
    EVENT = 2,
    MISC = 3,
    TRAP = 4


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
    ("Filler", 1002, Spyro3ItemCategory.MISC),
    ("Lag Trap", 1003, Spyro3ItemCategory.TRAP),
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
]]

item_descriptions = {
}

item_dictionary = {item_data.name: item_data for item_data in _all_items}

def BuildItemPool(multiworld, count, eggsToPlace, options):
    item_pool = []
    included_itemcount = 0

    if options.guaranteed_items.value:
        for item_name in options.guaranteed_items.value:
            item = item_dictionary[item_name]
            item_pool.append(item)
            included_itemcount = included_itemcount + 1
    remaining_count = count - included_itemcount
    for i in range(eggsToPlace):
        item_pool.append(item_dictionary["Egg"])
    remaining_count = remaining_count - eggsToPlace

    # TODO: Determine fallback cases
    #if remaining_count > 0 and not options.enable_filler_extra_lives and not options.enable_filler_invincibility and not options.enable_filler_color_change:
    #    print("No filler items are enabled, but filler checks are present.  Defaulting to extra lives.")
    #    options.enable_filler_extra_lives = 1

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

    # Get the correct blend of traps and filler items.
    for i in range((100 - options.trap_filler_percent) * 50):
        itemList = [item for item in allowed_misc_items]
        item = multiworld.random.choice(itemList)
        allowed_filler_items.append(item)
    for i in range(options.trap_filler_percent * 50):
        itemList = [item for item in allowed_trap_items]
        item = multiworld.random.choice(itemList)
        allowed_filler_items.append(item)
      
    for i in range(remaining_count):
        itemList = [item for item in allowed_filler_items]
        item = multiworld.random.choice(itemList)
        item_pool.append(item)
    
    multiworld.random.shuffle(item_pool)
    return item_pool
