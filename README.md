![Quack Modloader 🦆](https://i.imgur.com/UoSO3oD.png)

## Features

Ever hankered for a goose modloader in [Lua](https://www.lua.org/)? Well, here you go!

Get it: [Quack Lua](https://github.com/DesktopGooseUnofficial/ResourceHub/releases/download/quack-0.11/quack.zip)

**⚠ SUPER EARLY ALPHA**. CONTACT MOD AUTHOR IF ISSUES OR SUGGESTIONS/QUESTIONS APPEAR.

Goose Version: **v0.3**

Mod Version: **v0.11**

Author: **easy bake oven#6781**

You **must** extract this straight to Mods for this to work. Do not put it in a new folder, do not move or rename mod.lua.

## Mods
* The sample mod, nametag. (procrastinating with linking files even)
* ...none. Submit some mods to the mod author!

---
Functions:

| Function          | Arguments                                                                       | Purpose                                                                   |
|-------------------|---------------------------------------------------------------------------------|---------------------------------------------------------------------------|
| GetGooseProp      | (string property) returns *something*                                           | Gets the property inside the goose (position, direction, etc)             |
| SetGooseProp      | (string property, object value)                                                 | Sets the property inside the goose (position, direction, etc)             |
| DrawRect          | (table position, table size, string[color] color) returns bool                  | Puts a rectangle on screen with the specified position, size, and color.  |
| DrawText          | (table position, string content, string[color] color, number size) returns bool | Puts text on screen with the specified position, content, color, and size |
| MeasureText       | Same as above, returns table                                                    | Gets the measurements of the specified text.                              |
| GetMousePos       | None, returns table                                                             | Gets the mouse's position on the screen.                                  |
| GetMouseHeld      | None, returns bool                                                              | True if left mouse button is held, otherwise false.                       |
| MessageBox        | (string message)                                                                | Shows a textbox on the user's screen. Limit of 3 at once.                 |
| MessageBoxAsk     | (string message) returns bool                                                   | Shows a textbox on the user's screen with yes and no. True if yes.        |
| MessageBoxIcon    | (string message, string icon)                                                   | Shows a textbox on the user's screen with specified icon. Icons at bottom.|
| MessageBoxIconAsk | (string message, string icon) returns bool                                      | Same as MessageBoxAsk, but with an icon.                                  |
| MessageBoxInput   | (string message, string default) returns string                                 | Asks user for text input with message.                                    |

Some variables accessible to GetGooseProp and SetGooseProp:
| Variable Name   | Purpose                                                   |
|-----------------|-----------------------------------------------------------|
| position        | Where the goose is on your screen. (table)                |
| velocity        | The goose's speed. (table)                                |
| direction       | The way the goose is facing, in degrees. (number)         |
| targetDirection | The way the goose is trying to face, in degrees. (number) |
| targetPos       | The position the goose is trying to get to. (table)       |
| currentSpeed    | The current maximum speed of the goose. (number)          |

---

## Color Table

![Has a list of colors that can be used for the goose's color palette here https://docs.microsoft.com/en-us/dotnet/api/system.windows.media.colors?view=netframework-4.8](https://docs.microsoft.com/en-us/dotnet/media/art-color-table.png?view=netframework-4.8 "A  list of color that can be used")

If you are unable to see the image you can click [here](https://docs.microsoft.com/en-us/dotnet/api/system.windows.media.colors?view=netframework-4.8)

## Icons

Error:
![Error icon](https://docs.microsoft.com/en-us/dotnet/media/messagebox-error.png?view=netframework-4.8 "Error icon")

Warning:
![Warning icon](https://docs.microsoft.com/en-us/dotnet/media/messagebox-warning.png?view=netframework-4.8 "Warning icon")

Information:
![Information icon](https://docs.microsoft.com/en-us/dotnet/media/messagebox-information.png?view=netframework-4.8 "Information icon")
