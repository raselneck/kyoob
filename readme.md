Kyoob - XNA Voxel Engine
========================

For some reason I've always wanted to make a voxel engine, so I decided that
I was going to finally start writing one. This is the result.

Dependencies
------------

* [XNA 4.0 Refresh](http://www.microsoft.com/en-us/download/details.aspx?id=27599)
  * [See here](http://what-ev.net/2014/02/19/the-xna-enabler-app-xna-in-visual-studio-2012-2013/)
    if you're using VS2012 or VS2013
  * I have no clue if this will work with MonoGame
* [LibNoise for .NET](https://libnoisedotnet.codeplex.com/)
* [Newtonsoft.Json](http://james.newtonking.com/json) (I use 6.0r3)

Screenshot
----------

![ScreenShot](https://raw.githubusercontent.com/csdevrich/kyoob/master/screenshot.png)

The Terminal
------------

The terminal is the equivalent of every other game's console, except I couldn't
call it "Console" because of `System.Console`.

You can enter commands into the console by pressing the tilde (~) key while running,
and you can bind commands using `Terminal.AddCommand`. All you need to do is give it
an object name, a function name, and a callback (which is any method that returns nothing
and takes in an array of strings as the only parameter).

Currently, commands are parsed in the following format:
```
OBJECT.FUNCTION PARAM [PARAM ...]
```
Where each parameter is separated by a space. (Quoted strings are not yet supported.)

License
-------

All of the code in the repo, unless otherwise stated below, is licensed under the
MIT license. See the `licenses` folder for full license texts.

* Sky box/sphere model and HLSL code adapted from [Creating a SkySphere](http://msdn.microsoft.com/en-us/library/bb464016.aspx),
  which is released under the Microsoft Permissive License (Ms-PL).

Acknowledgments
---------------

* Skybox generated with [SkyGen](http://www.nutty.ca/?p=381), and the resulting
  images were exported from Photoshop with NVIDIA's Photoshop Plugins.
* Special thanks to [Sam Willis](https://github.com/Swillis57) for help with terrain.

Change Log
----------

**May 31, 2014**
* Bunch of small improvements
* Added system console re-router for terminal
* Added support for no-object functions for terminal
  * I.e. `listcmd`
* Added dedicated 2D renderer
* Started Kyoob.Lua (as in I created the project)
* Added settings
* Consolidated some code

**May 30, 2014**
* Much better physics
* Finally updated screenshot and readme
* A bunch of small fixes
* Moved physics from `Player` to `Entity`

**May 26, 2014**
* Some minor improvements

**May 24, 2014**
* Implemented basic physics
* Improved basic physics a bit

**May 20, 2014**
* Did some minor cleanup before implementing player

**May 19, 2014**
* Removed Minecraft textures
* Minor speed increases
* Temporarily fixed weird texture glitch

**May 17, 2014**
* Started messing with cooler terrain

**May 16, 2014**
* Added VertexBuffer for drawing bounding boxes
* Fixed terrain so that it is actually what it should be at height levels
* Changed "SkySphere" to "SkyBox"
* Used [SkyGen](http://www.nutty.ca/?p=381) for realistic sky box

**May 15, 2014**
* Remove NoiseUtil port dependency (no more LGPL :D)

**May 13, 2014**
* Added some terminal commands for modifying terrain
* Implemented infinite world generation
* Fixed crash when the window closes
* Formatted some code

**May 12, 2014**
* Added terminal commands
* Implemented semi-transparent water
* Formatted some code

**May 11, 2014**
* Implemented terrain

**May 10, 2014**
* Improved threading

**May 09, 2014**
* Started threading
* Implemented ability to load/save chunks/worlds
* Implemented hidden face exclusion
* Implemented actual voxels

**May 07, 2014**
* Lots of speed increases
* Implemented point light for camera

**May 06, 2014**
* Implemented effect wrappers
* Initial commit (draws a bunch of cubes)