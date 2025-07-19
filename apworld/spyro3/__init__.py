# world/dc2/__init__.py
from typing import Dict, Set, List
from enum import IntEnum

from BaseClasses import MultiWorld, Region, Item, Entrance, Tutorial, ItemClassification
from Options import Toggle

from worlds.AutoWorld import World, WebWorld
from worlds.generic.Rules import set_rule, add_rule, add_item_rule, forbid_item

from .Items import Spyro3Item, Spyro3ItemCategory, item_dictionary, key_item_names, item_descriptions, BuildItemPool
from .Locations import Spyro3Location, Spyro3LocationCategory, location_tables, location_dictionary
from .Options import Spyro3Option, GoalOptions

class Spyro3Web(WebWorld):
    bug_report_page = ""
    theme = "stone"
    setup_en = Tutorial(
        "Multiworld Setup Guide",
        "A guide to setting up the Archipelago Spyro 3 randomizer on your computer.",
        "English",
        "setup_en.md",
        "setup/en",
        ["ArsonAssassin"]
    )


    tutorials = [setup_en]


class Spyro3World(World):
    """
    Spyro 3 is a game about a purple dragon who likes eggs.
    """

    game: str = "Spyro 3"
    options_dataclass = Spyro3Option
    options: Spyro3Option
    topology_present: bool = True
    web = Spyro3Web()
    data_version = 0
    base_id = 1230000
    enabled_location_categories: Set[Spyro3LocationCategory]
    required_client_version = (0, 5, 0)
    item_name_to_id = Spyro3Item.get_name_to_id()
    location_name_to_id = Spyro3Location.get_name_to_id()
    item_name_groups = {
    }
    item_descriptions = item_descriptions


    def __init__(self, multiworld: MultiWorld, player: int):
        super().__init__(multiworld, player)
        self.locked_items = []
        self.locked_locations = []
        self.main_path_locations = []
        self.enabled_location_categories = set()

    def generate_early(self):
        self.enabled_location_categories.add(Spyro3LocationCategory.EGG),
        self.enabled_location_categories.add(Spyro3LocationCategory.EVENT),
        if self.options.enable_gem_checks.value:
            self.enabled_location_categories.add(Spyro3LocationCategory.GEM),
        if self.options.enable_skillpoint_checks.value:
            self.enabled_location_categories.add(Spyro3LocationCategory.SKILLPOINT)

    def create_regions(self):
        # Create Regions
        regions: Dict[str, Region] = {}
        regions["Menu"] = self.create_region("Menu", [])
        regions.update({region_name: self.create_region(region_name, location_tables[region_name]) for region_name in [
            "Sunrise Spring","Sunny Villa","Cloud Spires","Molten Crater","Seashell Shore","Mushroom Speedway","Sheila's Alp", "Buzz", "Crawdad Farm",
            "Midday Gardens","Icy Peak","Enchanted Towers","Spooky Swamp","Bamboo Terrace","Country Speedway","Sgt. Byrd's Base","Spike","Spider Town",
            "Evening Lake","Frozen Altars","Lost Fleet","Fireworks Factory","Charmed Ridge","Honey Speedway","Bentley's Outpost","Scorch","Starfish Reef",
            "Midnight Mountain","Crystal Islands","Desert Ruins","Haunted Tomb","Dino Mines","Harbor Speedway","Agent 9's Lab","Sorceress","Bugbot Factory","Super Bonus Round"
        ]})
        
        # Connect Regions
        def create_connection(from_region: str, to_region: str):
            connection = Entrance(self.player, f"{to_region}", regions[from_region])
            regions[from_region].exits.append(connection)
            connection.connect(regions[to_region])
            #print(f"Connecting {from_region} to {to_region} Using entrance: " + connection.name)
            
        create_connection("Menu", "Sunrise Spring")
                
        create_connection("Sunrise Spring", "Sunny Villa")
        create_connection("Sunrise Spring", "Cloud Spires")
        create_connection("Sunrise Spring", "Molten Crater")
        create_connection("Sunrise Spring", "Seashell Shore")
        create_connection("Sunrise Spring", "Mushroom Speedway")
        create_connection("Sunrise Spring", "Sheila's Alp")
             
        create_connection("Sunrise Spring", "Buzz")
        create_connection("Sunrise Spring", "Crawdad Farm")
        create_connection("Sunrise Spring", "Midday Gardens")
        
        create_connection("Midday Gardens", "Icy Peak")
        create_connection("Midday Gardens", "Enchanted Towers")
        create_connection("Midday Gardens", "Spooky Swamp")
        create_connection("Midday Gardens", "Bamboo Terrace")
        create_connection("Midday Gardens", "Country Speedway")
        create_connection("Midday Gardens", "Sgt. Byrd's Base")

        create_connection("Midday Gardens", "Spike")
        create_connection("Midday Gardens", "Spider Town")
        create_connection("Midday Gardens", "Evening Lake")
        
        create_connection("Evening Lake", "Frozen Altars")
        create_connection("Evening Lake", "Lost Fleet")
        create_connection("Evening Lake", "Fireworks Factory")
        create_connection("Evening Lake", "Charmed Ridge")
        create_connection("Evening Lake", "Honey Speedway")
        create_connection("Evening Lake", "Bentley's Outpost")

        create_connection("Evening Lake", "Scorch")
        create_connection("Evening Lake", "Starfish Reef")        
        create_connection("Evening Lake", "Midnight Mountain")   
        
        create_connection("Midnight Mountain", "Crystal Islands")
        create_connection("Midnight Mountain", "Desert Ruins")
        create_connection("Midnight Mountain", "Haunted Tomb")
        create_connection("Midnight Mountain", "Dino Mines")
        create_connection("Midnight Mountain", "Harbor Speedway")
        create_connection("Midnight Mountain", "Agent 9's Lab")

        create_connection("Midnight Mountain", "Sorceress")
        create_connection("Midnight Mountain", "Bugbot Factory")
        create_connection("Midnight Mountain", "Super Bonus Round")
        
        
    # For each region, add the associated locations retrieved from the corresponding location_table
    def create_region(self, region_name, location_table) -> Region:
        new_region = Region(region_name, self.player, self.multiworld)
        #print("location table size: " + str(len(location_table)))
        for location in location_table:
            #print("Creating location: " + location.name)
            if location.category in self.enabled_location_categories and location.category != Spyro3LocationCategory.EVENT:
                #print("Adding location: " + location.name + " with default item " + location.default_item)
                new_location = Spyro3Location(
                    self.player,
                    location.name,
                    location.category,
                    location.default_item,
                    self.location_name_to_id[location.name],
                    new_region
                )
                new_region.locations.append(new_location)
            elif location.category == Spyro3LocationCategory.EVENT:
                # Remove non-randomized progression items as checks because of the use of a "filler" fake item.
                # Replace events with event items for spoiler log readability.
                event_item = self.create_item(location.default_item)
                #if event_item.classification != ItemClassification.progression:
                #    continue
                #print("Adding Location: " + location.name + " as an event with default item " + location.default_item)
                new_location = Spyro3Location(
                    self.player,
                    location.name,
                    location.category,
                    location.default_item,
                    None,
                    new_region
                )
                event_item.code = None
                new_location.place_locked_item(event_item)
                #print("Placing event: " + event_item.name + " in location: " + location.name)
                new_region.locations.append(new_location)

        #print("created " + str(len(new_region.locations)) + " locations")
        self.multiworld.regions.append(new_region)
        #print("adding region: " + region_name)
        return new_region


    def create_items(self):
        itempool: List[Spyro3Item] = []
        itempoolSize = 0
        placedEggs = 0
        
        #print("Creating items")
        for location in self.multiworld.get_locations(self.player):
                #print("found item in category: " + str(location.category))
                item_data = item_dictionary[location.default_item_name]
                # There is a bug with the current client implementation where another player auto-collecting an item on the
                # goal condition results in the client thinking the player has completed the goal.
                # To avoid this, ensure the goal item is always vanilla.  Manually placed items exist outside the item pool.
                # TODO: Remove this restriction after implementing a better client solution.
                if item_data.category in [Spyro3ItemCategory.SKIP] or \
                        location.category in [Spyro3LocationCategory.EVENT] or \
                        (self.options.goal.value == GoalOptions.SORCERESS_ONE and location.name == "Sorceress's Lair: Defeat the Sorceress? (George)") or \
                        (self.options.goal.value == GoalOptions.EGG_FOR_SALE and location.name == "Midnight Mountain Home: Egg for sale. (Al)") or \
                        (self.options.goal.value == GoalOptions.SORCERESS_TWO and location.name == "Super Bonus Round: Woo, a secret egg. (Yin Yang)"): #or \
                        # Test goal for ease of debugging
                        #(self.options.goal.value == SUNNY_VILLA and location.name == "Sunny Villa: Rescue the mayor. (Sanders)"):
                    #print(f"Adding vanilla item/event {location.default_item_name} to {location.name}")
                    item = self.create_item(location.default_item_name)
                    self.multiworld.get_location(location.name, self.player).place_locked_item(item)
                    if location.default_item_name == 'Egg':
                        placedEggs = placedEggs + 1
                elif location.category in self.enabled_location_categories:
                    #print("Adding item: " + location.default_item_name)
                    itempoolSize += 1
                    #itempool.append(self.create_item(location.default_item_name))
        
        #print("Requesting itempool size: " + str(itempoolSize))
        foo = BuildItemPool(self.multiworld, itempoolSize, placedEggs, self.options)
        #print("Created item pool size: " + str(len(foo)))
        #for item in foo:
            #print(f"{item.name}")

        for item in foo:
            #print("Adding regular item: " + item.name)
            itempool.append(self.create_item(item.name))

        # Add regular items to itempool
        self.multiworld.itempool += itempool
        
        #print("Final Item pool: ")
        #for item in self.multiworld.itempool:
        #    print(item.name)


    def create_item(self, name: str) -> Item:
        useful_categories = {
        }
        data = self.item_name_to_id[name]

        if name in key_item_names or item_dictionary[name].category == Spyro3ItemCategory.EGG or item_dictionary[name].category == Spyro3ItemCategory.EVENT:
            item_classification = ItemClassification.progression
        elif item_dictionary[name].category in useful_categories:
            item_classification = ItemClassification.useful
        elif item_dictionary[name].category == Spyro3ItemCategory.TRAP:
            item_classification = ItemClassification.trap
        else:
            item_classification = ItemClassification.filler

        return Spyro3Item(name, item_classification, data, self.player)


    def get_filler_item_name(self) -> str:
        return "Egg"
    
    def set_rules(self) -> None:          
        def is_level_completed(self, level, state):        
            return state.has(level + " Complete", self.player)
        
        def is_boss_defeated(self, boss, state):
            return state.has(boss + " Defeated", self.player)    
            
        def set_indirect_rule(self, regionName, rule):
            region = self.multiworld.get_region(regionName, self.player)
            entrance = self.multiworld.get_entrance(regionName, self.player)
            set_rule(entrance, rule)
            self.multiworld.register_indirect_condition(region, entrance)
         
        #print("Setting rules")   
        for region in self.multiworld.get_regions(self.player):
            for location in region.locations:
                    set_rule(location, lambda state: True)
        if self.options.goal.value == GoalOptions.SORCERESS_TWO:
            self.multiworld.completion_condition[self.player] = lambda state: state.has("Super Bonus Round Complete", self.player)
        # Test goal for ease of debugging
        #elif self.options.goal.value == SUNNY_VILLA:
        #    self.multiworld.completion_condition[self.player] = lambda state: state.has("Sunny Villa Complete", self.player)
        elif self.options.goal.value == GoalOptions.EGG_FOR_SALE:
            self.multiworld.completion_condition[self.player] = lambda state: state.has("Moneybags Chase Complete", self.player)
        else:
            self.multiworld.completion_condition[self.player] = lambda state: is_boss_defeated(self, "Sorceress", state) and state.has("Egg", self.player, 100)

        set_rule(self.multiworld.get_location("Sunny Villa: Hop to Rapunzel. (Lucy)", self.player), lambda state: is_level_completed(self,"Sheila's Alp", state))
        
        set_indirect_rule(self, "Molten Crater", lambda state: state.has("Egg", self.player, 10))
        set_rule(self.multiworld.get_location("Molten Crater: Replace idol heads. (Ryan)", self.player), lambda state: is_level_completed(self,"Sgt. Byrd's Base", state))
        set_rule(self.multiworld.get_location("Molten Crater: Sgt. Byrd blows up a wall. (Luna)", self.player), lambda state: is_level_completed(self,"Sgt. Byrd's Base", state))
        
        set_indirect_rule(self, "Seashell Shore", lambda state: state.has("Egg", self.player, 14))
        set_rule(self.multiworld.get_location("Seashell Shore: Destroy the sand castle. (Mollie)", self.player), lambda state: is_level_completed(self,"Sheila's Alp", state))
        set_rule(self.multiworld.get_location("Seashell Shore: Hop to the secret cave. (Jared)", self.player), lambda state: is_level_completed(self,"Sheila's Alp", state))
        
        set_indirect_rule(self, "Mushroom Speedway", lambda state: state.has("Egg", self.player, 20)) 
                  
        set_indirect_rule(self, "Buzz", lambda state: is_level_completed(self,"Sunny Villa", state) and \
                is_level_completed(self,"Cloud Spires", state) and \
                is_level_completed(self,"Molten Crater", state) and \
                is_level_completed(self,"Seashell Shore", state) and \
                is_level_completed(self,"Sheila's Alp", state))

        set_indirect_rule(self, "Crawdad Farm", lambda state: is_boss_defeated(self,"Buzz", state))

        set_indirect_rule(self, "Midday Gardens", lambda state: is_boss_defeated(self,"Buzz", state))

        set_rule(self.multiworld.get_location("Enchanted Towers: Collect the bones. (Ralph)", self.player), lambda state: is_level_completed(self,"Sgt. Byrd's Base", state))        
        
        set_indirect_rule(self, "Spooky Swamp", lambda state: state.has("Egg", self.player,25))
        set_rule(self.multiworld.get_location("Spooky Swamp: Escort the twins I. (Peggy)", self.player), lambda state: is_level_completed(self,"Sheila's Alp", state))
        set_rule(self.multiworld.get_location("Spooky Swamp: Escort the twins II. (Michele)", self.player), lambda state: is_level_completed(self,"Sheila's Alp", state) and state.can_reach_location("Spooky Swamp: Escort the twins I. (Peggy)", self.player))

        set_indirect_rule(self, "Bamboo Terrace", lambda state: state.has("Egg", self.player,30))
        set_rule(self.multiworld.get_location("Bamboo Terrace: Smash to the mountain top. (Brubeck)", self.player), lambda state: is_level_completed(self,"Bentley's Outpost", state))
        
        set_indirect_rule(self, "Country Speedway", lambda state: state.has("Egg", self.player,36))

        set_indirect_rule(self, "Spike", lambda state: is_level_completed(self,"Icy Peak", state) and \
                is_level_completed(self,"Enchanted Towers", state) and \
                is_level_completed(self,"Spooky Swamp", state) and \
                is_level_completed(self,"Bamboo Terrace", state) and \
                is_level_completed(self,"Sgt. Byrd's Base", state))
        
        set_indirect_rule(self, "Spider Town", lambda state: is_boss_defeated(self,"Spike", state))
        set_indirect_rule(self, "Evening Lake", lambda state: is_boss_defeated(self,"Spike", state))     

        set_rule(self.multiworld.get_location("Frozen Altars: Box the yeti. (Aly)", self.player), lambda state: is_level_completed(self,"Bentley's Outpost", state))
        set_rule(self.multiworld.get_location("Frozen Altars: Box the yeti again! (Ricco)", self.player), lambda state: is_level_completed(self,"Bentley's Outpost", state) and state.can_reach_location("Frozen Altars: Box the yeti. (Aly)", self.player))
        set_indirect_rule(self, "Fireworks Factory", lambda state: state.has("Egg", self.player,50))
        set_rule(self.multiworld.get_location("Fireworks Factory: You're doomed! (Patty)", self.player), lambda state: is_level_completed(self,"Agent 9's Lab", state))
        set_rule(self.multiworld.get_location("Fireworks Factory: You're still doomed! (Donovan)", self.player), lambda state: is_level_completed(self,"Agent 9's Lab", state) and state.can_reach_location("Fireworks Factory: You're doomed! (Patty)", self.player))

        set_indirect_rule(self, "Charmed Ridge", lambda state: state.has("Egg", self.player,58))
        set_rule(self.multiworld.get_location("Charmed Ridge: Cat witch chaos. (Abby)", self.player), lambda state: is_level_completed(self,"Sgt. Byrd's Base", state))
        
        set_indirect_rule(self, "Honey Speedway", lambda state: state.has("Egg", self.player,65))

        set_indirect_rule(self, "Scorch", lambda state: is_level_completed(self,"Frozen Altars", state) and \
                is_level_completed(self,"Lost Fleet", state) and \
                is_level_completed(self,"Fireworks Factory", state) and \
                is_level_completed(self,"Charmed Ridge", state) and \
                is_level_completed(self,"Bentley's Outpost", state))

        # After completing 3 levels in Evening Lake, the player is unable to complete any Hunter challenges until defeating Scorch.
        # To prevent the player from locking themselves out of progression, these must be logically locked behind Scorch.
        # Note: The egg "Sunrise Spring Home: Learn Gliding (Coltrane)" is not affected by this - Hunter remains in Sunrise Spring home.
        # Most, if not all gems, in skateboarding areas can be collected without the skateboard, but leave out of base logic.
        set_rule(self.multiworld.get_location("Sunny Villa: Lizard skating I. (Emily)", self.player), lambda state: is_boss_defeated(self, "Scorch", state))
        set_rule(self.multiworld.get_location("Sunny Villa: Lizard skating II. (Daisy)", self.player), lambda state: is_boss_defeated(self, "Scorch", state))
        set_rule(self.multiworld.get_location("Mushroom Speedway: Hunter's dogfight. (Tater)", self.player), lambda state: is_boss_defeated(self, "Scorch", state))
        set_rule(self.multiworld.get_location("Enchanted Towers: Trick skater I. (Caroline)", self.player), lambda state: is_boss_defeated(self, "Scorch", state))
        set_rule(self.multiworld.get_location("Enchanted Towers: Trick skater II. (Alex)", self.player), lambda state: is_boss_defeated(self, "Scorch", state))
        set_rule(self.multiworld.get_location("Country Speedway: Hunter's rescue mission. (Roberto)", self.player), lambda state: is_boss_defeated(self, "Scorch", state))
        set_rule(self.multiworld.get_location("Lost Fleet: Skate race the rhynocs. (Oliver)", self.player), lambda state: is_boss_defeated(self, "Scorch", state))
        set_rule(self.multiworld.get_location("Lost Fleet: Skate race Hunter. (Aiden)", self.player), lambda state: is_boss_defeated(self, "Scorch", state))
        set_rule(self.multiworld.get_location("Honey Speedway: Hunter's narrow escape. (Nori)", self.player), lambda state: is_boss_defeated(self, "Scorch", state))

        set_indirect_rule(self, "Starfish Reef", lambda state: is_boss_defeated(self,"Scorch", state))
        set_indirect_rule(self, "Midnight Mountain", lambda state: is_boss_defeated(self,"Scorch", state))
        set_rule(self.multiworld.get_location("Midnight Mountain Home: Egg for sale. (Al)", self.player), lambda state: is_boss_defeated(self,"Sorceress", state))
        set_rule(self.multiworld.get_location("Midnight Mountain Home: Moneybags Chase Complete", self.player), lambda state: is_boss_defeated(self, "Sorceress", state))

        set_rule(self.multiworld.get_location("Crystal Islands: Whack a mole. (Hank)", self.player), lambda state: is_level_completed(self,"Bentley's Outpost", state))

        set_rule(self.multiworld.get_location("Desert Ruins: Krash Kangaroo I. (Lester)", self.player), lambda state: is_level_completed(self,"Sheila's Alp", state))
        set_rule(self.multiworld.get_location("Desert Ruins: Krash Kangaroo II. (Pete)", self.player), lambda state: is_level_completed(self,"Sheila's Alp", state))
        set_indirect_rule(self, "Haunted Tomb", lambda state: state.has("Egg", self.player,70))
        set_rule(self.multiworld.get_location("Haunted Tomb: Clear the caves. (Roxy)", self.player), lambda state: is_level_completed(self,"Agent 9's Lab", state))
        set_indirect_rule(self, "Dino Mines", lambda state: state.has("Egg", self.player,80))
        set_rule(self.multiworld.get_location("Dino Mines: Gunfight at the Jurassic Corral. (Sharon)", self.player), lambda state: is_level_completed(self,"Agent 9's Lab", state))
        set_rule(self.multiworld.get_location("Dino Mines: Take it to the bank. (Sergio)", self.player), lambda state: is_level_completed(self,"Agent 9's Lab", state) and state.can_reach_location("Dino Mines: Gunfight at the Jurassic Corral. (Sharon)", self.player))
        
        set_indirect_rule(self, "Harbor Speedway", lambda state: state.has("Egg", self.player,90))

        set_indirect_rule(self, "Sorceress", lambda state: state.has("Egg", self.player,100))

        set_indirect_rule(self, "Bugbot Factory", lambda state: is_boss_defeated(self,"Sorceress", state))
        set_indirect_rule(self, "Super Bonus Round", lambda state: is_boss_defeated(self,"Sorceress", state) and state.has("Egg", self.player,149))

        if Spyro3LocationCategory.GEM in self.enabled_location_categories:
            set_rule(self.multiworld.get_location("Molten Crater: All Gems", self.player), lambda state: is_level_completed(self, "Sgt. Byrd's Base", state))
            set_rule(self.multiworld.get_location("Seashell Shore: All Gems", self.player), lambda state: is_level_completed(self, "Sheila's Alp", state))
            set_rule(self.multiworld.get_location("Spooky Swamp: All Gems", self.player), lambda state: is_level_completed(self, "Sheila's Alp", state))
            set_rule(self.multiworld.get_location("Bamboo Terrace: All Gems", self.player), lambda state: is_level_completed(self, "Bentley's Outpost", state))
            set_rule(self.multiworld.get_location("Fireworks Factory: All Gems", self.player), lambda state: is_level_completed(self, "Agent 9's Lab", state))
            set_rule(self.multiworld.get_location("Sunny Villa: All Gems", self.player), lambda state: is_level_completed(self, "Sheila's Alp", state) and is_boss_defeated(self, "Scorch", state))
            set_rule(self.multiworld.get_location("Enchanted Towers: All Gems", self.player), lambda state: is_level_completed(self, "Sgt. Byrd's Base", state) and is_boss_defeated(self, "Scorch", state))
            set_rule(self.multiworld.get_location("Lost Fleet: All Gems", self.player), lambda state: is_boss_defeated(self, "Scorch", state))
            set_rule(self.multiworld.get_location("Crystal Islands: All Gems", self.player), lambda state: is_level_completed(self, "Bentley's Outpost", state))
            set_rule(self.multiworld.get_location("Desert Ruins: All Gems", self.player), lambda state: is_level_completed(self, "Sheila's Alp", state))
            set_rule(self.multiworld.get_location("Dino Mines: All Gems", self.player), lambda state: is_level_completed(self, "Agent 9's Lab", state))

        if Spyro3LocationCategory.SKILLPOINT in self.enabled_location_categories:
            set_rule(self.multiworld.get_location("Molten Crater: Assemble tiki heads (Skill Point)", self.player), lambda state: is_level_completed(self, "Sgt. Byrd's Base", state))
            set_rule(self.multiworld.get_location("Frozen Altars: Beat yeti in two rounds (Skill Point)", self.player), lambda state: is_level_completed(self, "Bentley's Outpost", state) and state.can_reach_location("Frozen Altars: Box the yeti. (Aly)", self.player))
            set_rule(self.multiworld.get_location("Fireworks Factory: Find Agent 9's powerup (Skill Point)", self.player), lambda state: is_level_completed(self, "Agent 9's Lab", state))
            set_rule(self.multiworld.get_location("Charmed Ridge: Shoot the temple windows (Skill Point)", self.player), lambda state: is_level_completed(self, "Sgt. Byrd's Base", state))
            set_rule(self.multiworld.get_location("Sunny Villa: Skateboard course record I (Skill Point)", self.player), lambda state: is_boss_defeated(self, "Scorch", state))
            set_rule(self.multiworld.get_location("Enchanted Towers: Skateboard course record II (Skill Point)", self.player), lambda state: is_boss_defeated(self, "Scorch", state))
            set_rule(self.multiworld.get_location("Lost Fleet: Skateboard record time (Skill Point)", self.player), lambda state: is_boss_defeated(self, "Scorch", state))
            set_rule(self.multiworld.get_location("Dino Mines: Hit the secret dino (Skill Point)", self.player), lambda state: is_level_completed(self, "Agent 9's Lab", state))
                
    def fill_slot_data(self) -> Dict[str, object]:
        slot_data: Dict[str, object] = {}


        name_to_s3_code = {item.name: item.s3_code for item in item_dictionary.values()}
        # Create the mandatory lists to generate the player's output file
        items_id = []
        items_address = []
        locations_id = []
        locations_address = []
        locations_target = []
        for location in self.multiworld.get_filled_locations():


            if location.item.player == self.player:
                #we are the receiver of the item
                items_id.append(location.item.code)
                items_address.append(name_to_s3_code[location.item.name])


            if location.player == self.player:
                #we are the sender of the location check
                locations_address.append(item_dictionary[location_dictionary[location.name].default_item].s3_code)
                locations_id.append(location.address)
                if location.item.player == self.player:
                    locations_target.append(name_to_s3_code[location.item.name])
                else:
                    locations_target.append(0)

        slot_data = {
            "options": {
                "goal": self.options.goal.value,
                "guaranteed_items": self.options.guaranteed_items.value,
                "enable_gem_checks": self.options.enable_gem_checks.value,
                "enable_skillpoint_checks": self.options.enable_skillpoint_checks.value,
                "enable_filler_extra_lives": self.options.enable_filler_extra_lives.value,
                "enable_filler_invincibility": self.options.enable_filler_invincibility.value,
                "enable_filler_color_change": self.options.enable_filler_color_change.value,
                "enable_filler_big_head_mode": self.options.enable_filler_big_head_mode.value,
                "enable_filler_heal_sparx": self.options.enable_filler_heal_sparx.value,
                "trap_filler_percent": self.options.trap_filler_percent.value,
                "enable_trap_damage_sparx": self.options.enable_trap_damage_sparx.value,
                "enable_trap_sparxless": self.options.enable_trap_sparxless.value,
            },
            "seed": self.multiworld.seed_name,  # to verify the server's multiworld
            "slot": self.multiworld.player_name[self.player],  # to connect to server
            "base_id": self.base_id,  # to merge location and items lists
            "locationsId": locations_id,
            "locationsAddress": locations_address,
            "locationsTarget": locations_target,
            "itemsId": items_id,
            "itemsAddress": items_address
        }

        return slot_data
