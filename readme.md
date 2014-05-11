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

* If you close the window before the chunk creation thread is finished, then
  the program will crash with a NullReferenceException as the thread attempts
  to create a chunk's VertexBuffer. I currently do not know why.

License
-------

All code in this repo, except for anything in the Kyoob.NoiseUtils namespace
(the code for this is conveniently located in a separate project and folder),
is licensed under the MIT license. The Kyoob.NoiseUtils namespace contains code
ported from the C++ NoiseUtils library to C#, and is released under the LGPL
v2.1 license. From my understanding, a port is considered a derivative work and
must therefore be released under the same license as the parent work. The original
NoiseUtils library ((c) 2003-2005 Jason Bevins) can be found
[here](http://libnoise.sourceforge.net/downloads/noiseutils.zip).

See the `licenses` folder for full license texts.

Images are (currently) taken from Minecraft and used for educational purposes
only. Nothing malicious was intended.