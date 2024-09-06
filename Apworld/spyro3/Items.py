from enum import IntEnum
from typing import NamedTuple
import random
from BaseClasses import Item


class Spyro3ItemCategory(IntEnum):
    EGG = 0,
    SKIP = 1


class Spyro3ItemData(NamedTuple):
    name: str
    s3_code: int
    category: Spyro3ItemCategory


class Spyro3Item(Item):
    game: str = "Spyro 3"

    @staticmethod
    def get_name_to_id() -> dict:
        base_id = 1230000
        return {item_data.name: base_id + item_data.s3_code for item_data in _all_items}


key_item_names = {
}


_all_items = [
    Spyro3ItemData(f"Egg", 1000, Spyro3ItemCategory.EGG)
]


item_descriptions = {
}

item_dictionary = {item_data.name: item_data for item_data in _all_items}

def BuildItemPool(count, options):
    item_pool = []
    included_itemcount = 0

    if options.guaranteed_items.value:
        for item_name in options.guaranteed_items.value:
            item = item_dictionary[item_name]
            item_pool.append(item)
            included_itemcount = included_itemcount + 1
    remaining_count = count - included_itemcount
    
    for i in range(remaining_count):
        itemList = [item for item in _all_items]
        item = random.choice(itemList)
        item_pool.append(item)
    
    random.shuffle(item_pool)
    return item_pool
