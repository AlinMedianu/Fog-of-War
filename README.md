# Fog-of-War
A personal project that will serve as a template for future RTS games. Fog of war was achieved by creating a polygon around the player
with the use of rays and passing this polygon to two shaders. One of them is responsible for breaking the fog around the player, and the 
other to remember where the player was, through the use of blitting textures on top of each other. The polygon was necessary so the 
terrain is not revealed if a wall is in the way.
