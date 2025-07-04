from enum import IntEnum
from typing import NamedTuple
from BaseClasses import Item


class Spyro3ItemCategory(IntEnum):
    EGG = 0,
    SKIP = 1,
    EVENT = 2,
    MISC = 3,
    TRAP = 4,
    MISCEVENT = 5


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
    ("Crawdad Farm Complete", 2006, Spyro3ItemCategory.MISCEVENT),
    
    ("Icy Peak Complete", 2007, Spyro3ItemCategory.EVENT),
    ("Enchanted Towers Complete", 2008, Spyro3ItemCategory.EVENT),
    ("Spooky Swamp Complete", 2009, Spyro3ItemCategory.EVENT),
    ("Bamboo Terrace Complete", 2010, Spyro3ItemCategory.EVENT),
    ("Sgt. Byrd's Base Complete", 2011, Spyro3ItemCategory.EVENT),
    ("Spike Defeated", 2012, Spyro3ItemCategory.EVENT),
    ("Spider Town Complete", 2013, Spyro3ItemCategory.MISCEVENT),
    
    ("Frozen Altars Complete", 2014, Spyro3ItemCategory.EVENT),
    ("Lost Fleet Complete", 2015, Spyro3ItemCategory.EVENT),
    ("Fireworks Factory Complete", 2016, Spyro3ItemCategory.EVENT),
    ("Charmed Ridge Complete", 2017, Spyro3ItemCategory.EVENT),
    ("Bentley's Outpost Complete", 2018, Spyro3ItemCategory.EVENT),
    ("Scorch Defeated", 2019, Spyro3ItemCategory.EVENT),
    ("Starfish Reef Complete", 2020, Spyro3ItemCategory.MISCEVENT),
    
    ("Crystal Islands Complete", 2021, Spyro3ItemCategory.MISCEVENT),
    ("Desert Ruins Complete", 2022, Spyro3ItemCategory.MISCEVENT),
    ("Haunted Tomb Complete", 2023, Spyro3ItemCategory.MISCEVENT),
    ("Dino Mines Complete", 2024, Spyro3ItemCategory.MISCEVENT),
    ("Agent 9's Lab Complete", 2025, Spyro3ItemCategory.EVENT),
    ("Sorceress Defeated", 2026, Spyro3ItemCategory.EVENT),
    ("Bugbot Factory Complete", 2027, Spyro3ItemCategory.MISCEVENT),
    ("Super Bonus Round Complete", 2028, Spyro3ItemCategory.MISCEVENT),
    
    
    ("Egg", 1000, Spyro3ItemCategory.EGG),
    ("Extra Life", 1001, Spyro3ItemCategory.MISC),
    ("Lag Trap", 1002, Spyro3ItemCategory.TRAP),
    
]]

item_descriptions = {
}

item_dictionary = {item_data.name: item_data for item_data in _all_items}

def BuildItemPool(multiworld, count, options):
    item_pool = []
    included_itemcount = 0

    if options.guaranteed_items.value:
        for item_name in options.guaranteed_items.value:
            item = item_dictionary[item_name]
            item_pool.append(item)
            included_itemcount = included_itemcount + 1
    remaining_count = count - included_itemcount
    for i in range(150):
        item_pool.append(item_dictionary["Egg"])
    remaining_count = remaining_count - 150
    
    allowed_items = [item for item in _all_items if item.category not in [Spyro3ItemCategory.MISCEVENT, Spyro3ItemCategory.EVENT, Spyro3ItemCategory.TRAP, Spyro3ItemCategory.EGG]]
      
    for i in range(remaining_count):
        itemList = [item for item in allowed_items]
        item = multiworld.random.choice(itemList)
        item_pool.append(item)
    
    multiworld.random.shuffle(item_pool)
    return item_pool
