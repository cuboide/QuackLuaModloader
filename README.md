![Quack Modloader 🦆](https://i.imgur.com/UoSO3oD.png)

Hello! This is a simple page to keep docs, mods, and the source code here!

Info like downloads, and other things found [here.](https://desktopgooseunofficial.github.io/ResourceHub/mods/Quack.html)

todo: please tell me what theme to use for GH pages please please :((((

## Documentation

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
