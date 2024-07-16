________________________________________________________________________________________
                         Ultimate Fracturing & Destruction Tool
                          Copyright © 2013 Ultimate Game Tools
                            http://www.ultimategametools.com
                               info@ultimategametools.com
________________________________________________________________________________________
Version 1.04


________________________________________________________________________________________
Introduction

The Ultimate Fracturing & Destruction Tool is a Unity3D editor extension that allows
you to fracture, slice and explode meshes. It also allows to visually edit behaviors
for all in-game destruction events like collisions, making it possible to trigger sounds
and particles when collisions occur without the need to script anything.

The Ultimate Fracturing & Destruction's main features are:
-Includes different sample scenes.
-Includes full source code.
-BSP (recursive slicing) & Voronoi fracturing algorithms.
-Can also import fractured objects from external tools (RayFire, Blender...).
-Can generate chunk connection graph to enable structural behavior. Destruction will
 behave intelligently depending on how the chunks are interconnected and also connected
 to the world.
-Can detect mesh islands.
-Automatically maps the interior of the fractured mesh. You assign the material.
-Visual editor to handle all events allowing to specify particle systems, sounds or
 any prefabs that need to be instanced when detaching, collisions, bullet impacts or
 explosions occur.
-Full UI integration allowing you to visualize and edit everything in the editor.
-Includes our Mesh Combiner utility to enable compound object fracturing as well.
-If you have our Concave Collider tool, you have additional control over the colliders
 generated.
-Many many paramters to play with!


________________________________________________________________________________________
Requirements

Supports Unity 3.5+ and 4.x
Sample scenes have been created using Unity 3.5.5f3.


________________________________________________________________________________________
Help

For up to date help: http://www.ultimategametools.com/products/fracturing/help
For additional support contact us at http://www.ultimategametools.com/contact


________________________________________________________________________________________
Acknowledgements

-3D Models:
   Temple model by xadmax2: http://www.turbosquid.com/FullPreview/Index.cfm/ID/548946
   Gun model by psionic: http://www.psionic3d.co.uk/?page_id=25

-The fracturing algorithm uses Poly2Tri for Delaunay triangulation on mesh capping:
 http://code.google.com/p/poly2tri/


________________________________________________________________________________________
Version history

V1.04 - 14/09/2013:

[FIX] Now support planes are correctly saved to disk when "Save Mesh Data To Asset" is
      enabled. Before, a NullReferenceException was thrown from the method
      UnityEngine.Graphics.Internal_DrawMeshNow2.
[FIX] Now prefab destructible objects don't spawn at (0, 0, 0).
[FIX] Got rid of the gameObject.active obsolete warning on Unity 4.x

V1.03 - 24/06/2013:

[ADD] Now can detect individual chunks when a Combined Mesh is the input. This allows
      to import objects from external fracturing tools like RayFire.
      Chunk connection graph is also generated in this case.
[DEL] Removed FracturedObject.EnableRandomColoredChunks().

V1.02 - 16/06/2013:

[FIX] Changed FracturedObject.cs GUID because it collided with UltimateRope.cs from our
      Ultimate Rope Editor package. It caused erroneous imports when both packages
      were added to the same project.
[DEL] Removed an empty Fractured Object in the first scene.

V1.00 - 29/05/2013:

[---] Initial release