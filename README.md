# GridGame

A proof of concept repo to prove that I can make a Tactics game in the style of
something like Fire Emblem. All sprites/textures are made by me in [Aseprite](https://www.aseprite.org/), and maps
are created in [LDTK](https://ldtk.io/). The code is written in C# for the
[Monogame](https://monogame.net/) framework. This is a level lower than game engines like Unity, [Monogame](https://monogame.net/)
really only provides the basic game loop, content management, and a graphics layer abstraction.
Even something as simple as a camera needs to be coded (or sourced from a library like [MonoGame.Extended](https://www.monogameextended.net/)).
To spite the AI trends, and because I enjoy this, I have chosen to not use any code libraries besides
the standard [Monogame](https://monogame.net/) ones, and to code it all by hand. A big thanks to
[AristurtleDev](https://github.com/AristurtleDev) for a fantastic tutorial that made something as scary as
[Monogame](https://monogame.net/) understandable.

GridLibrary should be stocked with tools that will
make this proof of concept repeatable, at scale, and with enough breathing room
to customize the look and feel between games.

GridGame.Core is where all the specific game logic lives. Currently, only
keyboard inputs are supported, and the game has only been tested on a Windows machine.

Story/gameplay will be inspired by my DnD groups' ongoing campaign. Thank you DM and fellow adventurers.
Menzobara deserves to live on infamy!
