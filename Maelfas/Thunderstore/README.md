# Templates

This is a simple template for Heroes.

See my guide in the [Across the Obelisk Discord](https://discord.gg/across-the-obelisk-679706811108163701) or below more information.

<details>
	<summary> Full Guide</summary>

# How to Create a Custom Hero

Hi everyone, with the release of a couple new heroes, I thought it would be a good idea to teach everyone how to do it themselves. If I am missing out on anything in the guide or anything is unclear, let me know.

Before I begin, the big difficulty with creating heroes is creating their levels 1, 3, and 5 traits (which I will just call Traits from now on, I will refer to the level 2 and level 4 upgrades as Enchantments). These Traits need to be coded in C# and are essentially arbitrary code execution. So they can do practically anything you want. Because of this, I would recommend designing the Traits and then pinging me to actually do the coding for you :) 

Now for the actual guide (adapted and updated from: https://code.secretsisters.gay/AtO_How_To):


## Guide 

For those of you who want more control over how you create your hero (or don’t want me to do as much work), here is a more in-depth guide. This mainly consists of json files that will be directly loaded by Obeliskial Content/Essentials into the game.

To simplify this process, I created a [template](<URL>) of all the files necessary to create a hero.

For your convenience, all of the fields which are mandatory/highly recommended for you to fill out have a field have been replaced by a description inside <>s.

For instance, if your character is named **Ursur**, then `<charactername>` should be replaced by `ursur` while `<CharacterName>` would be replaced with `Ursur`. Hopefully it is intuitive. If you need help, just ping me.

Additionally, I highly recommend

## Step 1: - Setup
Manually Download the [**Hero Template**](<URL_HERE>) from Thunderstore. This is a zip that contains the following `BepInEx\config\Obeliskial_importing` folders:

#### The folders:   1. audio 
2. card 
3. cardback
4. gameObject
5. pack
6. skin
7. sprite
8. subclass
9. trait

#### Plugins
There is also an empty `BepInEx\plugins` folder. This is where you will place the `.dll` that includes the traits/some bonus code. I will send this `.dll` file to you. If you have C# experience, you can create your own traits using the following [**Template**](https://github.com/stiffmeds/TraitMod)

## Step 2: Creating your subclass

The most important file in the template is the one found in the `subclass` folder. This pretty much contains all of the information needed for your Hero. Everything else is just refining and filling out the remainder.

These are the following properties you will need to add/change for your Hero:

 - **ActionSound:** Skip this one, it isn’t used
 - **AutoUnlock**: Sets whether your hero is unlocked by default or needs to be unlocked via quest. Just leave this one as true
 - **Cards:** An array showing the 15 cards that the subclass starts with. There can be a **maximum** of 7 *unique* cards. By convention, at least one of these should be a unique/starter card.  - **ChallengePack[0-6]:** These are the packs offered in Obelisk Challenge. Pack0 is your starter pack, and packs 1-6 are the six remaining packs. See the Packs section for more info.
 - **CharacterDescription:** A short history of the character (approximately 2 paragraphs). Appears when the character is selected in the Hero Selection screen.
 - **CharacterDescriptionStrength:** A one-line description of the character’s strengths.
 - **CharacterName:** The character’s name.
 - **Energy:** The character’s starting energy. (This is 1 for every hero unless you specifically want to change that)
 - **EnergyTurn:** The amount of energy the character gains each turn. (This is 3 for every hero unless you specifically want to change that)
 - **Female:** Set to *true* if the character should use female action sounds on cards where available (e.g. Battle Cry makes a different noise when Bree plays it than when Magnus plays it).
 - **FluffOffsetX:** Horizontal offset for fluff text on played cards - pretty much ignore this one
 - **FluffOffsetY:** Vertical offset for fluff text on played cards - pretty much ignore this one
 - **HeroClass:** The character’s class. (i.e. Warrior, Scout, Mage, Healer)
 - **HeroClassSecondary:** Secondary class for dual-class heroes, or “None” if they have only one class.
 - **HitSound:** Sound made when the character takes damage.
 - **HP:** Base max HP.
 - **ID:** The name of your subclass. I would keep it the same as your file name. Lowercase only, no spaces. **Must** be the same as the SubclassName
 - **Item:** The ID for their unique item
 - **MaxHP:** An array of numbers corresponding to the increase in max HP per level.
 - **OrderInList:** The order that this character appears within its class. Slots 1-4 (corresponding to values 0-3 in this property) are used for vanilla classes. Choosing a value of 0-3 will replace a vanilla hero. Choosing 4 or 5 will put them into the corresponding “modded” hero slot. Numbers ≥6 will not work (for now)
 - **Resist[DamageType]:** Base resistances in %.
 - **Speed:** Base speed.
 - **SpriteBorderLocked:** The filename for the image shown on the Hero Selection screen when the character has not yet been unlocked. 
 Do not include the `.png` file extension in the name! I.e if your sprite is named *mySpriteBorderLocked.png*, enter *mySpriteBorderLocked*.
 - **Sticker[Type]:** The filename for the character’s stickers (used with the “target” and “heal” emotes). 
 Do not include the `.png` file extension!
 - **StickerOffsetX:** Horizontal offset for stickers.
 - **SubclassName:** The name of the character’s class (e.g. Mercenary, Cleric, Elementalist), shown on the Hero Selection screen when they are selected. **Must** be the same as the ID (but can be capitalized)
 - **Trait[s]:** The ID for the character’s traits and enchantments. 0 is the innate, 1 is level 2, 2 is level 3 and so forth. A is left hand side, B is right hand side.

## Step 3: Visuals

### Sprite

Into this folder load your custom images as `.png` files. Thats pretty much it.
These are the following sprites you will need:
 - 1 sprite for each card/enchantment - these must be 256 × 256 px pngs
 - 1 sprite for each cardback - these must be 172 × 256 px pngs
 - The following skin sprites:
•	SpritePortrait: a 110 × 172 px png. This is used at the top of the screen during combat.
* SpritePortraitGrande: a 140 × 219 png. This is used in the map screen/on the side in town.
* SpriteSilueta: a 172 × 112 png. This is used during hero selection.
* SpriteSiluetaGrande: a 512 × 512 png. This is used during reward screens.

As well as any optional skin sprites


#### Skin json

This represents the texture/images used for your skin

#### Game json

This is the actual model used by your skin.
Json Properties: 
 - **BaseGameObjectID:** the ID of the vanilla GameObject that you are making a new version of. 
 A list of hero skin GameObjects can be found in the [DataText Reference](https://docs.google.com/spreadsheets/d/1CokEi8RY33KTwKccprNvr4nnRk2y-HQGUZC_YB9exzo/edit#gid=789548583) *SkinData* tab in the *SkinGO* column. 
 A list of enemy GameObjects can be found in the [DataText Reference](https://docs.google.com/spreadsheets/d/1CokEi8RY33KTwKccprNvr4nnRk2y-HQGUZC_YB9exzo/edit#gid=789548583) _NPCData * tab in the  *GameObjectAnimated* column.
 - **NewGameObjectID:** a unique identifier for your new GameObject. Lowercase only, no spaces. This will be used as the SkinGO in the Skin json
 - **SpriteToUse:** the filename for the Skin sprite. Unless you are changing the skin itself, just leave this blank. If you want to change the skin, see the [How To](https://code.secretsisters.gay/AtO_How_To) guide on that or ping me.
 **Do not** include the `.png` file extension! 
 If left blank, the sprite will not be replaced (i.e. the GameObject will look the same as the original).
 - **ScaleX:** multiplier for the width of the GameObject (e.g. making this 0.5 would make the character model half as wide).
 - **ScaleY:** multiplier for the height of the GameObject (e.g. making this 2 would make the character model twice as tall).
 - **Flip:** if set to true, flips the character model from left to right (or vice versa). 
 By default, heroes face right and enemies face left, so if you want to use an enemy as a hero skin you need to flip them around to face the right (as you can see in the example file).

### Cardback

To upload a cardback you need to do two things. 
1. Create a 256x256 png of the cardback. This file should be uploaded to the sprites folder.
2. Create a cardback json for that png


-   Cardback json properties:
    -   **AdventureLevel:**  the Adventure Mode madness level that must be completed to unlock the cardback.
    -   **BaseCardback:**  set to true if this is the default cardback for the given class (i.e. for Warriors/Healers etc)
    -   **CardbackID:**  a unique identifier for your cardback. Lowercase only, no spaces.
    -   **CardbackName:**  the name of the cardback, displayed in the cardback selection screen.
    -   **CardbackOrder:**  the order in which this cardback appears in the cardback selection screen.
    -   **CardbackSprite:**  the filename for the cardback sprite that you created in the previous step.  
        Do not include the  `.png`  file extension!
    -   **CardbackSubclass:**  the ID of the class that the skin is for; leave blank to make this cardback available to all classes. (e.g. mercenary for Magnus)
    -   **Locked:**  set to false to have the cardback unlocked by default.
    -   **ObeliskLevel:**  the Obelisk Challenge madness level that must be completed to unlock the cardback.
    -   **RankLevel:**  the subclass rank required to unlock this class. Vanilla AtO unlocks new cardbacks at ranks 8, 24 and 40.
    -   **ShowIfLocked:**  set to false to hide the cardback from the cardback selection screen if it hasn’t been unlocked.
    -   **Sku:**  ID of the DLC required to unlock this skin - only use if you want your cardback to be specific to a DLC
    -   **SteamStat:**  leave this one blank

Make sure that the filename matches **CardbackID**

### Pack

This folder should contain json files for any new Obelisk Challenge Packs that the character would use.

There are two types of packs. Starter packs and Choice packs. Starter packs include the first few base cards that a character starts with, while choice pacts represent the other packs that the cards will be drawn from during the selection process. 

You will typically want at least 1 custom Starter pack. (Otherwise the character cannot play in OC)

 -   _Starter_  Packs
    -   Properties:
        -   **Card0**  through  **Card4:**  IDs for the five cards that the hero will always start with.
        -   **PackClass:**  Warrior/Scout/Mage/Healer. For dual-class characters, use the primary class.
        -   **PackID:**  unique identifier for this card pack; lowercase only, no spaces. Ideally the character’s name.
        -   **PackName:**  Character’s name.
        -   **RequiredClass:**  ID for the character’s subclass (e.g. mercenary for Magnus).
        -   Leave the  **Card5**,  **CardSpecial0**,  **CardSpecial1**  and  **PerkList**  properties blank.
    -   Make sure the filename matches the  _ID_.
-  Choice Packs
    -   Properties:
        -   **Card0**  through  **Card5:**  IDs for the base versions of six cards that chosen in each OC pack.
        -   **CardSpecial0**  and  **CardSpecial1:**  IDs for the base versions of two cards that are given as choices in the final/“special” card selection. 
        -   **PackClass:**  Warrior/Scout/Mage/Healer. For dual-class characters, use the primary class.
        -   **PackID:**  unique identifier for this card pack; lowercase only, no spaces.
        -   **PackName:**  Name of the pack, shown on the pack selection screen.
        -   **PerkList:**  Skip this. It isn’t used.
        -   **RequiredClass:**  Leave Blank.
    -   Make sure the filename matches the  _ID_.

## Step 4: Cards

I highly recommend using StiffMed’s [**Custom Content Creator**](https://code.secretsisters.gay/AtO_Custom) to make your cards

After that, make sure to load all 4 versions of the card (white/base, blue/a, yellow/b, and corrupt/rare) to the cards folder.
Make sure all the appropriate sprites are in the sprites folder and all appropriate sound files are in the audio folder.


## Step 5: Traits

There are two different types of traits
1. Enchantment or Card Traits - These are your level 2 and 4 traits. My convention they give you an enchantment, though they can be set to give you or all heroes any card. They must give a card of some sort and should not have additional effects.
2. Effect traits (or just Traits) - These traits are essentially arbitrary code execution. You can do anything that is within your coding ability with them. There are a bunch of pre-made/simple effects that they can have, but it is usually easiest to just let me handle the creation of these, so all I really need is a description. If you are interested in creating your own Traits, let me know and I can walk you through how to do it. 


### Enchantment/Card Traits

Enchantment traits (Level 2 and 4) need two things. 
1. A set of 3 cards (easiest if made using the [**Custom Content Creator**](https://code.secretsisters.gay/AtO_Custom)) 
2. The appropriate json file.

This json file needs to only have 4 properties filled out:

* ID: Unique identifier for your trait. Lowercase only, no spaces.
* TraitName: The name of the trait. Appears on the level up screen and when the trait is activated.
* TraitCard: The ID of the card to be given to this hero only when this trait is selected.
* TraitCardForAllHeroes: The ID of the card to be given to all heroes when this trait is selected.

Leave the rest of the properties in their default value.
Make sure that the **ID** matches both the filename and the appropriate **TraitID** found in the subclass json.
 

### Traits

These effect traits or just Traits will need to be coded in C# (which I will happily do for you). However, to make my life easier, I would recommend that you create an appropriate json for the Trait. I really only need you to fill out three properties for this (ignore the rest), I will fill them out as needed.

* ID: Unique identifier for your trait. Lowercase only, no spaces.
* TraitName: The name of the trait. Appears on the level up screen and when the trait is activated.
* Description: A short description of the trait; appears on the level up screen.

If you want this trait to also add a card (Like Nezgleckt’s level 5 for instance), you can specify the following properties 
* TraitCard: The ID of the card to be given to this hero only when this trait is selected.
* TraitCardForAllHeroes: The ID of the card to be given to all heroes when this trait is selected.

Make sure that the **ID** matches both the filename and the appropriate **TraitID** found in the subclass json.


## Packaging your mod

If you want me to upload and maintain your Hero, just let me know, I am more than happy to do that.

However, if you want to upload the mod yourself, then in order to upload your mod to Thunderstore, it needs a few extra files (also included in the template).

These are:
- manifest.json - a json file that is mostly used by thunderstore to display the name, description, version, and dependencies for the mod.
- README.md - a markdown file that is displayed in thunderstore when someone clicks on your mod
- CHANGELOG.md - a markdown file that is displayed in thunderstore under the Changelog tab. Pretty much just details the different versions/what you change when you upload a new version
- icon.png - a 256x256 png that will be the display image of your mod

If you have any difficulty with any steps of the process, feel free to message me and I will do my best to help!

## In the future:

In the near future, my goal is to create a website that allows you to streamline this process. That will take a bit of work and probably take a good deal of time. As a half-way point, I was thinking of uploading a spreadsheet that would have most of the information available, and you could just copy down what you want there. I would then be able to use that info to create a hero for you. Note that this is significantly more labor intensive for me than the previous method, so I will prioritize creating traits/heroes that were mostly created by others.

To create this hero, all you have to do is make a **copy** of this [**spreadsheet**](<URL_HERE>), (spreadsheet is still in the works right now) and fill out all of the appropriate fields. I gave some examples of what your responses should look like, and hovering over any of the categories will give you an explanation. These responses will then by copied by me into the relevant places and I will use your Trait designs to create the rest of the hero. Nice and straight-forward.

I highly recommend that you create your own Enchantments and Starting Card/Item using StiffMed’s [**Custom Content Creator**](https://code.secretsisters.gay/AtO_Custom). Just choose a similar-ish enchantment or card/item and then work based off the options given there.



</details>

This mod relies on [Obeliskial Content](https://across-the-obelisk.thunderstore.io/package/meds/Obeliskial_Content/).

Please use StiffMed's [Content Creator](https://code.secretsisters.gay/AtO_Custom) to create cards/items/enchantments.


## Installation (manual)

1. Install [Obeliskial Essentials](https://across-the-obelisk.thunderstore.io/package/meds/Obeliskial_Essentials/) and [Obeliskial Content](https://across-the-obelisk.thunderstore.io/package/meds/Obeliskial_Content/).
2. Click _Manual Download_ at the top of the page.
3. In Steam, right-click Across the Obelisk and select _Manage_->_Browse local files_.
4. Extract the archive into the game folder. Your _Across the Obelisk_ folder should now contain a `BepInEx` folder and a `doorstop\libs` folder.
5. Run the game. If everything runs correctly, you will see this mod in the list of registered mods on the main menu.
6. Press F5 to open/close the Config Manager and F1 to show/hide mod version information.
7. Note: I am not certain about these install instructions. In the worst case, just copy the <CharacterName> folder (the one with the subfolders containing the json files) into `BepInEx\config\Obeliskial\_importing`

## Installation (automatic)

1. Download and install [Thunderstore Mod Manager](https://www.overwolf.com/app/Thunderstore-Thunderstore_Mod_Manager) or [r2modman](https://across-the-obelisk.thunderstore.io/package/ebkr/r2modman/).
2. Click **Install with Mod Manager** button on top of the page.
3. Run the game via the mod manager.

## Support

This has been updated for version 1.4.

Hope you enjoy it and if have any issues, ping me in Discord or make a post in the **modding #support-and-requests** channel of the [official Across the Obelisk Discord](https://discord.gg/across-the-obelisk-679706811108163701).

## Donation

Please do not donate to me. If you wish to support me, I would prefer it if you just gave me feedback. 