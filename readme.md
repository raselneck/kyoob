Kyoob - XNA Voxel Engine
========================

For some reason I've always wanted to make a voxel engine, so I decided that
I was going to finally start writing one. This is the result.

Also, I apologize for the diff being so large after the initial terrain
commit. I like to keep my code super organized, so I pretty much just moved
everything that was in `source/` to `source/kyoob/` so that I could keep
the code for my partial NoiseUtils port in `source/` as well.

Dependencies
------------

* [XNA 4.0 Refresh](http://www.microsoft.com/en-us/download/details.aspx?id=27599)
  * [See here](http://what-ev.net/2014/02/19/the-xna-enabler-app-xna-in-visual-studio-2012-2013/)
    if you're using VS2012 or VS2013
  * I have no clue if this will work with MonoGame
* [LibNoise for .NET](https://libnoisedotnet.codeplex.com/)

Screenshot
----------

![ScreenShot](https://raw.githubusercontent.com/csdevrich/kyoob/master/screenshot.png)

Bugs
----

None currently known.

The Terminal
------------

The terminal is the equivalent of every other game's console, except I couldn't
call it "Console" because of `System.Console`. Currently it automatically prints
out the current FPS and the average number of chunks drawn in one second as well
as the average amount of time it took to draw said chunks.

You can also enter commands into the console by pressing the tilde (~) key while
running the engine, and you can bind commands using `Terminal.AddCommand`. All
you need to do is give it an object name, a function name, and a callback (which
is any method that returns nothing and takes in an array of strings as the only
parameter).

Currently, commands are parsed in the following format:
```
OBJECT.FUNCTION PARAM [PARAM ...]
```
Where each parameter is separated by a space. (Strings are not yet supported.)

Some built-in methods:
* `terminal.hide` hides the terminal's output, but still allows commands to be
  entered.
* `terminal.show` shows the terminal's output.
* `camera.getpos` prints out the camera's position.
* `camera.setpos X Y Z` sets the camera's position.
* `camera.index` prints the camera's world index.
* `world.reload` reloads the current world.
* `terrain.hbias VALUE` sets the terrain generator's horizontal bias.
* `terrain.vbias VALUE` sets the terrain generator's vertical bias.

License
-------

All of the code in the repo, unless otherwise stated below, is licensed under the
MIT license. See the `licenses` folder for full license texts.

* Sky sphere model and HLSL code taken from [Creating a SkySphere](http://msdn.microsoft.com/en-us/library/bb464016.aspx),
  which is released under the Microsoft Permissive License (Ms-PL).

Images are (currently) taken from Minecraft and are used for educational purposes
only. Nothing malicious was intended.

Skybox generated with [Spacescape](http://alexcpeterson.com/spacescape) and exported
from Photoshop using NVIDIA's Photoshop Plugins. (For reference, the skybox texture
order is: right, left, top, bottom, front, back.)

Change Log
----------

To be written.