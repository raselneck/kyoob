Kyoob - XNA Voxel Engine
========================

For some reason I've always wanted to make a voxel engine, so I decided that
I was going to finally start writing one. This is the result.

![ScreenShot](https://raw.githubusercontent.com/fastinvsqrt/kyoob/master/screenshot.png)

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

Dependencies
------------

* [XNA 4.0 Refresh](http://www.microsoft.com/en-us/download/details.aspx?id=27599)
  * [See here](http://what-ev.net/2014/02/19/the-xna-enabler-app-xna-in-visual-studio-2012-2013/)
    if you're using VS2012 or VS2013
  * Reach profile is fine as I saw no noticeable benefit using HiDef
  * I have no clue if this will work with MonoGame
* [LibNoise for .NET](https://libnoisedotnet.codeplex.com/)
* [Newtonsoft.Json](http://james.newtonking.com/json) (v6.0r3+)

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