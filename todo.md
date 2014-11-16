Gameplay
--------

* Input manager
  * Mainly to add support for 360 controller, etc.
* Create physics system to manage all physics objects
  * Contains collection of `IPhysicsObject`
  * Water "physics"
* Some physics kind of breaks with large coordinates
  * Mainly due to `float` limitations ?
* Main menu, etc. for game states
* Player "model"
* Enemies (way later on)

Terrain
-------

* **Improve threading to remove lag spikes**
  * Is this even a software thing? Or is it a hardware thing?
  * I think it's a hardware thing
  * If it's a hardware thing, cache a position when creating chunks and
    if the player's distance from said position > some value, (un)load chunks
  * Currently, rendering is a slave to chunk creation (i.e. only update the chunks
    to be rendered when the chunk creator says it's okay) whereas Minecraft appears
    to be the other way around
* Modifiable terrain
  * Modify ray-intersection to also return normal of closest intersected face
  * Normal could be used to calculate position of new block if placing blocks
* **Try to speed up terrain generation even more**
* More realistic terrain
  * Sand only needs to be around water
  * Water doesn't need to be at a set level
  * Trees!!!

Graphics
--------

* *Create 3D render helper (similar to `Renderer2D`)*
  * Move drawing extensions to this
  * Edit classes to use this
  * `IDrawable3D`
    * `void Draw( GameTime, GraphicsDevice )`
    * `IDrawable2D` ?
* Shadows
* Post-processing
  * Under-water tinting

Scripting
---------

* Think of what to expose to scripts
  * Scripts will be used for modding worlds, etc
  * Move code to be exposed to separate project
* If compiling from source is slow, then think about serializing generated
  assemblies into base-64 strings and saving the strings along with the time
  the source file was last modified and (relative) source path into a JSON
  database
  * Scan source files at startup
    * If the date is newer, re-compile the source
    * If the source doesn't exist, remove the compiled assembly data

Research
--------

* Think about how to save/load a world to/from a file
* Think about how to implement multiplayer
  * Look into [Lindgren.Network](https://code.google.com/p/lidgren-network-gen3/)
  * Possibly have single-player be on a server on `localhost` which can be opened
    to other IPs later