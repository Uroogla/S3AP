from enum import IntEnum
from typing import Optional, NamedTuple, Dict

from BaseClasses import Location, Region
from .Items import Spyro3Item

class Spyro3LocationCategory(IntEnum):
    EGG = 0,
    SKIP = 1


class Spyro3LocationData(NamedTuple):
    name: str
    default_item: str
    category: Spyro3LocationCategory


class Spyro3Location(Location):
    game: str = "Spyro 3"
    category: Spyro3LocationCategory
    default_item_name: str

    def __init__(
            self,
            player: int,
            name: str,
            category: Spyro3LocationCategory,
            default_item_name: str,
            address: Optional[int] = None,
            parent: Optional[Region] = None):
        super().__init__(player, name, address, parent)
        self.default_item_name = default_item_name
        self.category = category

    @staticmethod
    def get_name_to_id() -> dict:
        base_id = 1230000
        table_offset = 1000

        table_order = [
            "Eggs"
        ]

        output = {}
        for i, region_name in enumerate(table_order):
            if len(location_tables[region_name]) > table_offset:
                raise Exception("A location table has {} entries, that is more than {} entries (table #{})".format(len(location_tables[region_name]), table_offset, i))

            output.update({location_data.name: id for id, location_data in enumerate(location_tables[region_name], base_id + (table_offset * i))})

        return output

    def place_locked_item(self, item: Spyro3Item):
        self.item = item
        self.locked = True
        item.location = self

location_tables = {
"Eggs": [
    Spyro3LocationData(f"Egg {i}", f"Egg", Spyro3LocationCategory.EGG) for i in range(1, 150)
]
}

location_dictionary: Dict[str, Spyro3LocationData] = {}
for location_table in location_tables.values():
    location_dictionary.update({location_data.name: location_data for location_data in location_table})
