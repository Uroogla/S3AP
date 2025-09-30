# Instructions:

[Click Here](https://github.com/ArsonAssassin/Archipelago.Core/wiki/How-to-start-playing-a-game-using-this-library) for
general instructions.

## Playing a Game with Spyro 3

### Required Software

Important: As the mandatory client runs only on Windows, no other systems are supported.

- [Duckstation](https://www.duckstation.org) - Detailed installation instructions for Duckstation can be found at the above link.
- Archipelago version 0.6.1 or later.
- The [Spyro 3 Archipelago Client and .apworld](https://github.com/Uroogla/S3AP/releases)
- A legal US Spyro: Year of the Dragon NTSC-U (US version) ROM.  We cannot help with this step.
### Create a Config (.yaml) File

#### What is a config file and why do I need one?

See the guide on setting up a basic YAML at the Archipelago setup guide: [Basic Multiworld Setup Guide](/tutorial/Archipelago/setup/en)

This also includes instructions on generating and hosting the file.  The "On your local installation" instructions
are particularly important.

#### Where do I get a config file?

Run `ArchipelagoLauncher.exe` and generate template files.  Copy `Spyro 3.yaml`, fill it out, and place
it in the `players` folder.

Alternatively, if you are using the local Webhost rather than [archipelago.gg](archipelago.gg), the Player Options page allows you to configure
your personal options and export a config file from them. Player options page: [Spyro 3 Player Options Page](/games/Spyro%203/player-options).

#### Verifying your config file

If you would like to validate your config file to make sure it works and are using the local Webhost,
you may do so on the YAML Validator page. YAML validator page: [YAML Validation page](/mysterycheck).

### Generate and host your world

Run `ArchipelagoGenerate.exe` to build a world from the YAML files in your `players` folder.  This places
a `.zip` file in the `output` folder.

You may upload this to [the Archipelago website](https://archipelago.gg/uploads) or host the game locally with
`ArchipelagoHost.exe`.

### Setting Up Spyro 3 for Archipelago

1. Download the S3AP.zip and spyro3.apworld from the GitHub page linked above.
2. Double click the apworld to install to your Archipelago installation.
3. Extract S3AP.zip and note where S3AP.Desktop.exe is.
4. Open Duckstation and load into Spyro: Year of the Dragon.
5. Start a new game (or if continuing an existing seed, load into that save file).
6. Open S3AP.Desktop.exe, the Spyro 3 client.  You will likely want to do so as an administrator.
7. In the top left of the Spyro 3 client, click the "burger" menu to open the settings page.
8. Enter your host, slot, and optionally your password.
9. Click Connect. The first time you connect, a few error messages may appear - these are okay.
10. Start playing!

## What does randomization do to this game?

When the player completes a task (such as collecting an egg), an item is sent. Collecting an egg may not increment the player's egg counter,
while a check received from another game may do so.

This does not randomize the location of eggs or gems, shuffle entrances, or make large-scale cosmetic changes to the game.
While previous versions of this Archipelago implementation were compatible with the standalone Spyro 3 randomizer,
support has been removed based on community feedback and developer time constraints.

When open world mode is disabled, the underlying game logic is unchanged.  For instance, accessing the balloon to Buzz in Sunrise Spring requires
helping all 5 NPCs in this world, whether or not they give an egg for the assistance.  The HUD's egg count
shows how many egg items you have received, while the in game Atlas shows which checks you have completed.

Open World mode gives access to all 4 homeworlds immediately, removing some eggs from the game.

It is possible to change the access restrictions for normal levels, as well.

## What items and locations get shuffled?
Eggs are always shuffled.  Based on the player's options, skill points, life bottles, individual gems,
and milestones for reaching certain numbers of gems per level or overall may also release checks.

The item pool will contain 150 eggs, unless the goal is Egg Hunt.
Depending on the player's options, companion unlocks or all Moneybags unlocks may
be shuffled into the pool, rather than having the player pay Moneybags.  Leftover items will be "filler", based on the player's
options.  Examples include giving extra lives, temporary invincibility, changing Spyro's color, or making the player Sparxless.

## Which items can be in another player's world?

Any of the items which can be shuffled may also be placed into another player's world.

## What does another world's item look like in Spyro 3?

The visuals of the game are unchanged by the Archipelago randomization.  The Spyro 3 Archipelago Client
will display the obtained item and to whom it belongs.

## When the player receives an item, what happens?

The player's game and HUD will update accordingly, provided that they are in their save file.  Some effects,
such as healing Sparx, may operate with a delay to avoid unintended interactions in game.

Receiving a Moneybags unlock while not in the same zone as him will complete the unlock automatically.
Doing so while in the same zone as him will require you to speak with him (or leave the zone) to finalize
the unlock.  Unlocks completed in this way cost 0 gems.

If for any reason the player is not in their save file when items come in, there may be a temporary desync.
Egg count will update the next time the player completes a check or receives an item.  Missed Moneybags
unlocks require the `clearSpyroGameState` command to be entered into the client.

## Unique Local Commands

The following command (without a slash or exclamation point) is available when using the S3AP client to play with Archipelago.

- `clearSpyroGameState` Resync your save file's received items with the server.  This may result in duplicate filler items.
If playing on a new save file, you will still need to get to the end of each level and defeat the bosses to progress in the game.
- `useQuietHints` Suppresses hints for found locations to make the client easier to read. On by default.
- `useVerboseHints` Include found locations in hint lists. Due to Archipelago Server limitations, only applies to hints requested after this change.
- `showGoal` Show what your completion goal is.

## Known Issues

- Leaving the client and game open for extended periods of time may result in errors.
- Receiving invincibility in a time attack stops the timer and doesn't automatically complete until invincibility runs out, you give up (which counts as victory), or crash (counts as victory)

## Credits

ArsonAssassin built a significant portion of this randomizer, as well as the Memory and Client frameworks it uses.  This project would not be possible without them.
