from enum import IntEnum
from typing import Optional, NamedTuple, Dict

from BaseClasses import Location, Region
from .Items import Spyro3Item

class Spyro3LocationCategory(IntEnum):
    EGG = 0,
    SKIP = 1,
    EVENT = 2


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
            "Sunrise Springs","Sunny Villa","Cloud Spires","Molten Crater","Seashell Shore","Mushroom Speedway","Shiela's Alp", "Buzz", "Crawdad Farm",
            "Midday Garden","Icy Peak","Enchanted Towers","Spooky Swamp","Bamboo Terrace","Country Speedway","Sgt. Byrd's Base","Spike","Spider Town",
            "Evening Lake","Frozen Altars","Lost Fleet","Fireworks Factory","Charmed Ridge","Honey Speedway","Bentley's Outpost","Scorch","Starfish Reef",
            "Midnight Mountain","Crystal Islands","Desert Ruins","Haunted Tomb","Dino Mines","Harbor Speedway","Agent 9's Lab","Sorceress","Bugbot Factory","Super Bonus Round"
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
#Homeworld 1
"Sunrise Springs": [
    Spyro3LocationData(f"Egg 1", f"Egg 1", Spyro3LocationCategory.EGG),
    Spyro3LocationData(f"Egg 2", f"Egg 2", Spyro3LocationCategory.EGG),
    Spyro3LocationData(f"Egg 3", f"Egg 3", Spyro3LocationCategory.EGG),
    Spyro3LocationData(f"Egg 4", f"Egg 4", Spyro3LocationCategory.EGG),
    Spyro3LocationData(f"Egg 5", f"Egg 5", Spyro3LocationCategory.EGG)    
],
"Sunny Villa": [
    Spyro3LocationData(f"Egg 6", f"Egg 6", Spyro3LocationCategory.EGG),
    Spyro3LocationData(f"Egg 7", f"Egg 7", Spyro3LocationCategory.EGG),
    Spyro3LocationData(f"Egg 8", f"Egg 8", Spyro3LocationCategory.EGG),
    Spyro3LocationData(f"Egg 9", f"Egg 9", Spyro3LocationCategory.EGG),
    Spyro3LocationData(f"Egg 10", f"Egg 10", Spyro3LocationCategory.EGG),
    Spyro3LocationData(f"Egg 11", f"Egg 11", Spyro3LocationCategory.EGG),
    Spyro3LocationData("Sunny Villa Complete", "Sunny Villa Complete", Spyro3LocationCategory.EVENT)
],
"Cloud Spires": [
    Spyro3LocationData(f"Egg 12", f"Egg 12", Spyro3LocationCategory.EGG),
    Spyro3LocationData(f"Egg 13", f"Egg 13", Spyro3LocationCategory.EGG),
    Spyro3LocationData(f"Egg 14", f"Egg 14", Spyro3LocationCategory.EGG),
    Spyro3LocationData(f"Egg 15", f"Egg 15", Spyro3LocationCategory.EGG),
    Spyro3LocationData(f"Egg 16", f"Egg 16", Spyro3LocationCategory.EGG),
    Spyro3LocationData(f"Egg 17", f"Egg 17", Spyro3LocationCategory.EGG),
    Spyro3LocationData("Cloud Spires Complete", "Cloud Spires Complete", Spyro3LocationCategory.EVENT)
],
"Molten Crater": [
    Spyro3LocationData(f"Egg 18", f"Egg 18", Spyro3LocationCategory.EGG),
    Spyro3LocationData(f"Egg 19", f"Egg 19", Spyro3LocationCategory.EGG),
    Spyro3LocationData(f"Egg 20", f"Egg 20", Spyro3LocationCategory.EGG),
    Spyro3LocationData(f"Egg 21", f"Egg 21", Spyro3LocationCategory.EGG),
    Spyro3LocationData(f"Egg 22", f"Egg 22", Spyro3LocationCategory.EGG),
    Spyro3LocationData(f"Egg 23", f"Egg 23", Spyro3LocationCategory.EGG),
    Spyro3LocationData("Molten Crater Complete", "Molten Crater Complete", Spyro3LocationCategory.EVENT)
],
"Seashell Shore": [
    Spyro3LocationData(f"Egg 24", f"Egg 24", Spyro3LocationCategory.EGG),
    Spyro3LocationData(f"Egg 25", f"Egg 25", Spyro3LocationCategory.EGG),
    Spyro3LocationData(f"Egg 26", f"Egg 26", Spyro3LocationCategory.EGG),
    Spyro3LocationData(f"Egg 27", f"Egg 27", Spyro3LocationCategory.EGG),
    Spyro3LocationData(f"Egg 28", f"Egg 28", Spyro3LocationCategory.EGG),
    Spyro3LocationData(f"Egg 29", f"Egg 29", Spyro3LocationCategory.EGG),
    Spyro3LocationData("Seashell Shore Complete", "Seashell Shore Complete", Spyro3LocationCategory.EVENT)
],
"Mushroom Speedway": [
    Spyro3LocationData(f"Egg 30", f"Egg 30", Spyro3LocationCategory.EGG),
    Spyro3LocationData(f"Egg 31", f"Egg 31", Spyro3LocationCategory.EGG),
    Spyro3LocationData(f"Egg 32", f"Egg 32", Spyro3LocationCategory.EGG)
],
"Shiela's Alp": [
    Spyro3LocationData(f"Egg 33", f"Egg 33", Spyro3LocationCategory.EGG),
    Spyro3LocationData(f"Egg 34", f"Egg 34", Spyro3LocationCategory.EGG),
    Spyro3LocationData(f"Egg 35", f"Egg 35", Spyro3LocationCategory.EGG),
    Spyro3LocationData("Shiela's Alp Complete", "Shiela's Alp Complete", Spyro3LocationCategory.EVENT)
],
"Buzz": [
    Spyro3LocationData(f"Egg 36", f"Egg 36", Spyro3LocationCategory.EGG),
    Spyro3LocationData("Buzz Defeated", "Buzz Defeated", Spyro3LocationCategory.EVENT)
],
"Crawdad Farm": [
    Spyro3LocationData(f"Egg 37", f"Egg 37", Spyro3LocationCategory.EGG),
    Spyro3LocationData("Crawdad Farm Complete", "Crawdad Farm Complete", Spyro3LocationCategory.EVENT)
],
#Homeworld 2
"Midday Garden": [
    Spyro3LocationData(f"Egg 38", f"Egg 38", Spyro3LocationCategory.EGG),
    Spyro3LocationData(f"Egg 39", f"Egg 39", Spyro3LocationCategory.EGG),
    Spyro3LocationData(f"Egg 40", f"Egg 40", Spyro3LocationCategory.EGG),
    Spyro3LocationData(f"Egg 41", f"Egg 41", Spyro3LocationCategory.EGG),
    Spyro3LocationData(f"Egg 42", f"Egg 42", Spyro3LocationCategory.EGG)
],
"Icy Peak": [
    Spyro3LocationData(f"Egg 43", f"Egg 43", Spyro3LocationCategory.EGG),
    Spyro3LocationData(f"Egg 44", f"Egg 44", Spyro3LocationCategory.EGG),
    Spyro3LocationData(f"Egg 45", f"Egg 45", Spyro3LocationCategory.EGG),
    Spyro3LocationData(f"Egg 46", f"Egg 46", Spyro3LocationCategory.EGG),
    Spyro3LocationData(f"Egg 47", f"Egg 47", Spyro3LocationCategory.EGG),
    Spyro3LocationData(f"Egg 48", f"Egg 48", Spyro3LocationCategory.EGG),
    Spyro3LocationData("Icy Peak Complete", "Icy Peak Complete", Spyro3LocationCategory.EVENT)
],
"Enchanted Towers": [
    Spyro3LocationData(f"Egg 49", f"Egg 49", Spyro3LocationCategory.EGG),
    Spyro3LocationData(f"Egg 50", f"Egg 50", Spyro3LocationCategory.EGG),
    Spyro3LocationData(f"Egg 51", f"Egg 51", Spyro3LocationCategory.EGG),
    Spyro3LocationData(f"Egg 52", f"Egg 52", Spyro3LocationCategory.EGG),
    Spyro3LocationData(f"Egg 53", f"Egg 53", Spyro3LocationCategory.EGG),
    Spyro3LocationData(f"Egg 54", f"Egg 54", Spyro3LocationCategory.EGG),
    Spyro3LocationData("Enchanted Towers Complete", "Enchanted Towers Complete", Spyro3LocationCategory.EVENT)
],
"Spooky Swamp": [
    Spyro3LocationData(f"Egg 55", f"Egg 55", Spyro3LocationCategory.EGG),
    Spyro3LocationData(f"Egg 56", f"Egg 56", Spyro3LocationCategory.EGG),
    Spyro3LocationData(f"Egg 57", f"Egg 57", Spyro3LocationCategory.EGG),
    Spyro3LocationData(f"Egg 58", f"Egg 58", Spyro3LocationCategory.EGG),
    Spyro3LocationData(f"Egg 59", f"Egg 59", Spyro3LocationCategory.EGG),
    Spyro3LocationData(f"Egg 60", f"Egg 60", Spyro3LocationCategory.EGG),
    Spyro3LocationData("Spooky Swamp Complete", "Spooky Swamp Complete", Spyro3LocationCategory.EVENT)
],
"Bamboo Terrace": [
    Spyro3LocationData(f"Egg 61", f"Egg 61", Spyro3LocationCategory.EGG),
    Spyro3LocationData(f"Egg 62", f"Egg 62", Spyro3LocationCategory.EGG),
    Spyro3LocationData(f"Egg 63", f"Egg 63", Spyro3LocationCategory.EGG),
    Spyro3LocationData(f"Egg 64", f"Egg 64", Spyro3LocationCategory.EGG),
    Spyro3LocationData(f"Egg 65", f"Egg 65", Spyro3LocationCategory.EGG),
    Spyro3LocationData(f"Egg 66", f"Egg 66", Spyro3LocationCategory.EGG),
    Spyro3LocationData("Bamboo Terrace Complete", "Bamboo Terrace Complete", Spyro3LocationCategory.EVENT)
],
"Country Speedway": [
    Spyro3LocationData(f"Egg 67", f"Egg 67", Spyro3LocationCategory.EGG),
    Spyro3LocationData(f"Egg 68", f"Egg 68", Spyro3LocationCategory.EGG),
    Spyro3LocationData(f"Egg 69", f"Egg 69", Spyro3LocationCategory.EGG)
],
"Sgt. Byrd's Base": [
    Spyro3LocationData(f"Egg 70", f"Egg 70", Spyro3LocationCategory.EGG),
    Spyro3LocationData(f"Egg 71", f"Egg 71", Spyro3LocationCategory.EGG),
    Spyro3LocationData(f"Egg 72", f"Egg 72", Spyro3LocationCategory.EGG),
    Spyro3LocationData("Sgt. Byrd's Base Complete", "Sgt. Byrd's Base Complete", Spyro3LocationCategory.EVENT)
],
"Spike": [
    Spyro3LocationData(f"Egg 73", f"Egg 73", Spyro3LocationCategory.EGG),
    Spyro3LocationData("Spike Defeated", "Spike Defeated", Spyro3LocationCategory.EVENT)
],
"Spider Town": [
    Spyro3LocationData(f"Egg 74", f"Egg 74", Spyro3LocationCategory.EGG),
    Spyro3LocationData("Spider Town Complete", "Spider Town Complete", Spyro3LocationCategory.EVENT)
],
#Homeworld 3
"Evening Lake": [
    Spyro3LocationData(f"Egg 75", f"Egg 75", Spyro3LocationCategory.EGG),
    Spyro3LocationData(f"Egg 76", f"Egg 76", Spyro3LocationCategory.EGG),
    Spyro3LocationData(f"Egg 77", f"Egg 77", Spyro3LocationCategory.EGG),
    Spyro3LocationData(f"Egg 78", f"Egg 78", Spyro3LocationCategory.EGG),
    Spyro3LocationData(f"Egg 79", f"Egg 79", Spyro3LocationCategory.EGG)
],
"Frozen Altars": [
    Spyro3LocationData(f"Egg 80", f"Egg 80", Spyro3LocationCategory.EGG),
    Spyro3LocationData(f"Egg 81", f"Egg 81", Spyro3LocationCategory.EGG),
    Spyro3LocationData(f"Egg 82", f"Egg 82", Spyro3LocationCategory.EGG),
    Spyro3LocationData(f"Egg 83", f"Egg 83", Spyro3LocationCategory.EGG),
    Spyro3LocationData(f"Egg 84", f"Egg 84", Spyro3LocationCategory.EGG),
    Spyro3LocationData(f"Egg 85", f"Egg 85", Spyro3LocationCategory.EGG),
    Spyro3LocationData("Frozen Altars Complete", "Frozen Altars Complete", Spyro3LocationCategory.EVENT)
],
"Lost Fleet": [
    Spyro3LocationData(f"Egg 86", f"Egg 86", Spyro3LocationCategory.EGG),
    Spyro3LocationData(f"Egg 87", f"Egg 87", Spyro3LocationCategory.EGG),
    Spyro3LocationData(f"Egg 88", f"Egg 88", Spyro3LocationCategory.EGG),
    Spyro3LocationData(f"Egg 89", f"Egg 89", Spyro3LocationCategory.EGG),
    Spyro3LocationData(f"Egg 90", f"Egg 90", Spyro3LocationCategory.EGG),
    Spyro3LocationData(f"Egg 91", f"Egg 91", Spyro3LocationCategory.EGG),
    Spyro3LocationData("Lost Fleet Complete", "Lost Fleet Complete", Spyro3LocationCategory.EVENT)
],
"Fireworks Factory": [
    Spyro3LocationData(f"Egg 92", f"Egg 92", Spyro3LocationCategory.EGG),
    Spyro3LocationData(f"Egg 93", f"Egg 93", Spyro3LocationCategory.EGG),
    Spyro3LocationData(f"Egg 94", f"Egg 94", Spyro3LocationCategory.EGG),
    Spyro3LocationData(f"Egg 95", f"Egg 95", Spyro3LocationCategory.EGG),
    Spyro3LocationData(f"Egg 96", f"Egg 96", Spyro3LocationCategory.EGG),
    Spyro3LocationData(f"Egg 97", f"Egg 97", Spyro3LocationCategory.EGG),
    Spyro3LocationData("Fireworks Factory Complete", "Fireworks Factory Complete", Spyro3LocationCategory.EVENT)
],
"Charmed Ridge": [
    Spyro3LocationData(f"Egg 98", f"Egg 98", Spyro3LocationCategory.EGG),
    Spyro3LocationData(f"Egg 99", f"Egg 99", Spyro3LocationCategory.EGG),
    Spyro3LocationData(f"Egg 100", f"Egg 100", Spyro3LocationCategory.EGG),
    Spyro3LocationData(f"Egg 101", f"Egg 101", Spyro3LocationCategory.EGG),
    Spyro3LocationData(f"Egg 102", f"Egg 102", Spyro3LocationCategory.EGG),
    Spyro3LocationData(f"Egg 103", f"Egg 103", Spyro3LocationCategory.EGG),
    Spyro3LocationData("Charmed Ridge Complete", "Charmed Ridge Complete", Spyro3LocationCategory.EVENT)
],
"Honey Speedway": [
    Spyro3LocationData(f"Egg 104", f"Egg 104", Spyro3LocationCategory.EGG),
    Spyro3LocationData(f"Egg 105", f"Egg 105", Spyro3LocationCategory.EGG),
    Spyro3LocationData(f"Egg 106", f"Egg 106", Spyro3LocationCategory.EGG)
],
"Bentley's Outpost": [
    Spyro3LocationData(f"Egg 107", f"Egg 107", Spyro3LocationCategory.EGG),
    Spyro3LocationData(f"Egg 108", f"Egg 108", Spyro3LocationCategory.EGG),
    Spyro3LocationData(f"Egg 109", f"Egg 109", Spyro3LocationCategory.EGG),
    Spyro3LocationData("Bentley's Outpost Complete", "Bentley's Outpost Complete", Spyro3LocationCategory.EVENT)
],
"Scorch": [
    Spyro3LocationData(f"Egg 110", f"Egg 110", Spyro3LocationCategory.EGG),
    Spyro3LocationData("Scorch Defeated", "Scorch Defeated", Spyro3LocationCategory.EVENT)
],
"Starfish Reef": [
    Spyro3LocationData(f"Egg 111", f"Egg 111", Spyro3LocationCategory.EGG),
    Spyro3LocationData("Starfish Reef Complete", "Starfish Reef Complete", Spyro3LocationCategory.EVENT)
],
#Homeworld 4
"Midnight Mountain": [
    Spyro3LocationData(f"Egg 112", f"Egg 112", Spyro3LocationCategory.EGG),
    Spyro3LocationData(f"Egg 113", f"Egg 113", Spyro3LocationCategory.EGG),
    Spyro3LocationData(f"Egg 114", f"Egg 114", Spyro3LocationCategory.EGG),
    Spyro3LocationData(f"Egg 115", f"Egg 115", Spyro3LocationCategory.EGG),
    Spyro3LocationData(f"Egg 116", f"Egg 116", Spyro3LocationCategory.EGG)
],
"Crystal Islands": [
    Spyro3LocationData(f"Egg 117", f"Egg 117", Spyro3LocationCategory.EGG),
    Spyro3LocationData(f"Egg 118", f"Egg 118", Spyro3LocationCategory.EGG),
    Spyro3LocationData(f"Egg 119", f"Egg 119", Spyro3LocationCategory.EGG),
    Spyro3LocationData(f"Egg 120", f"Egg 120", Spyro3LocationCategory.EGG),
    Spyro3LocationData(f"Egg 121", f"Egg 121", Spyro3LocationCategory.EGG),
    Spyro3LocationData(f"Egg 122", f"Egg 122", Spyro3LocationCategory.EGG),
    Spyro3LocationData("Crystal Islands Complete", "Crystal Islands Complete", Spyro3LocationCategory.EVENT)
],
"Desert Ruins": [
    Spyro3LocationData(f"Egg 123", f"Egg 123", Spyro3LocationCategory.EGG),
    Spyro3LocationData(f"Egg 124", f"Egg 124", Spyro3LocationCategory.EGG),
    Spyro3LocationData(f"Egg 125", f"Egg 125", Spyro3LocationCategory.EGG),
    Spyro3LocationData(f"Egg 126", f"Egg 126", Spyro3LocationCategory.EGG),
    Spyro3LocationData(f"Egg 127", f"Egg 127", Spyro3LocationCategory.EGG),
    Spyro3LocationData(f"Egg 128", f"Egg 128", Spyro3LocationCategory.EGG),
    Spyro3LocationData("Desert Ruins Complete", "Desert Ruins Complete", Spyro3LocationCategory.EVENT)
],
"Haunted Tomb": [
    Spyro3LocationData(f"Egg 129", f"Egg 129", Spyro3LocationCategory.EGG),
    Spyro3LocationData(f"Egg 130", f"Egg 130", Spyro3LocationCategory.EGG),
    Spyro3LocationData(f"Egg 131", f"Egg 131", Spyro3LocationCategory.EGG),
    Spyro3LocationData(f"Egg 132", f"Egg 132", Spyro3LocationCategory.EGG),
    Spyro3LocationData(f"Egg 133", f"Egg 133", Spyro3LocationCategory.EGG),
    Spyro3LocationData(f"Egg 134", f"Egg 134", Spyro3LocationCategory.EGG),
    Spyro3LocationData("Haunted Tomb Complete", "Haunted Tomb Complete", Spyro3LocationCategory.EVENT)
],
"Dino Mines": [
    Spyro3LocationData(f"Egg 135", f"Egg 135", Spyro3LocationCategory.EGG),
    Spyro3LocationData(f"Egg 136", f"Egg 136", Spyro3LocationCategory.EGG),
    Spyro3LocationData(f"Egg 137", f"Egg 137", Spyro3LocationCategory.EGG),
    Spyro3LocationData(f"Egg 138", f"Egg 138", Spyro3LocationCategory.EGG),
    Spyro3LocationData(f"Egg 139", f"Egg 139", Spyro3LocationCategory.EGG),
    Spyro3LocationData(f"Egg 140", f"Egg 140", Spyro3LocationCategory.EGG),
    Spyro3LocationData("Dino Mines Complete", "Dino Mines Complete", Spyro3LocationCategory.EVENT)
],
"Harbor Speedway": [
    Spyro3LocationData(f"Egg 141", f"Egg 141", Spyro3LocationCategory.EGG),
    Spyro3LocationData(f"Egg 142", f"Egg 142", Spyro3LocationCategory.EGG),
    Spyro3LocationData(f"Egg 143", f"Egg 143", Spyro3LocationCategory.EGG)
],
"Agent 9's Lab": [
    Spyro3LocationData(f"Egg 144", f"Egg 144", Spyro3LocationCategory.EGG),
    Spyro3LocationData(f"Egg 145", f"Egg 145", Spyro3LocationCategory.EGG),
    Spyro3LocationData(f"Egg 146", f"Egg 146", Spyro3LocationCategory.EGG),
    Spyro3LocationData("Agent 9's Lab Complete", "Agent 9's Lab Complete", Spyro3LocationCategory.EVENT)
],
"Sorceress": [
    Spyro3LocationData(f"Egg 147", f"Egg 147", Spyro3LocationCategory.EGG),
    Spyro3LocationData("Sorceress Defeated", "Sorceress Defeated", Spyro3LocationCategory.EVENT)
],
"Bugbot Factory": [
    Spyro3LocationData(f"Egg 148", f"Egg 148", Spyro3LocationCategory.EGG),
    Spyro3LocationData("Bugbot Factory Complete", "Bugbot Factory Complete", Spyro3LocationCategory.EVENT)
],
"Super Bonus Round": [
    Spyro3LocationData(f"Egg 149", f"Egg 149", Spyro3LocationCategory.EGG),
    Spyro3LocationData("Super Bonus Round Complete", "Super Bonus Round Complete", Spyro3LocationCategory.EVENT)
]

}

location_dictionary: Dict[str, Spyro3LocationData] = {}
for location_table in location_tables.values():
    location_dictionary.update({location_data.name: location_data for location_data in location_table})
