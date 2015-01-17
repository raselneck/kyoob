# Kyoob v0.3.0 Alpha

Ever since first playing Minecraft, I've toyed around with the idea of making
my very own voxel engine. Kyoob is the result of my turning that idea into
something tangible.

[![screenshot of Kyoob](kyoob.png)](kyoob.png)

## Requirements

* [XNA 4.0 Refresh](http://www.microsoft.com/en-us/download/details.aspx?id=27599) ([redistributable](http://www.microsoft.com/en-us/download/details.aspx?id=27598))
  * [See here](http://what-ev.net/2014/02/19/the-xna-enabler-app-xna-in-visual-studio-2012-2013/)
    if you're using VS2012 or VS2013
  * Also .NET Framework 4.0
  * As of v0.3.0, HiDef profile is no longer required
  * This may work with MonoGame, or it may not. I have no clue

## Todo

* Improve lighting
* Add GUI items (button, etc.)
* Prevent walking off map (mainly for slower computers)
* Fix gravity problems when vsync is disabled
* Fix physics for far away places (coordinates in the thousands)
  * "Normalize" the block and player positions for physics calculations
* Improve world generation, taking hints [from Notch](http://n0tch.tumblr.com/post/4231184692/terrain-generation-part-1)
* Loading and unloading of chunks from and to disk
* Water rendering
* Block editing
* Motion blur?
* Re-implement the console/terminal?

## Special Thanks

* Sean James for writing [3D Graphics with XNA Game Studio 4.0](http://www.amazon.com/Graphics-XNA-Game-Studio-4-0/dp/1849690049) (I highly recommend picking it up if you plan on doing
  serious XNA development)
* Microsoft for being such swell people and providing so many free XNA samples
* [Charles Barros](http://gamecoderbr.blogspot.com/) for his [Voxel Engine series](http://gamecoderbr.blogspot.com/2013/03/voxel-engine-part-1.html)
* Nick and Jessie for helping me test this

## License

All of the code in this repository, unless otherwise stated, is released under the
[MIT license](license.md).