Change Log
----------

**June 5, 2014**
* Removed Boo dependencies (temporary)
  * Files are still intact

**June 3, 2014**
* Implemented thread pool for chunk creation (much faster)
* Created [To-Do list](todo.md) in case anyone decided to fork
  and help work on this

**June 2, 2014**
* Added state system

**June 1, 2014**
* Made location conversions static
* Speed increase for chunk generation
* Temporarily removed world/chunk file support
* Added chunk manager
* Added Boo dependencies (to prepare for scripting)

**May 31, 2014**
* Bunch of small improvements
* Added system console re-router for terminal
* Added support for no-object functions for terminal
  * See `listcmd`
* Added dedicated 2D renderer
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