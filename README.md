Space and Blocks
Eli Asimow
10/29/2019

This project was made over the course of a week in October, 2019.
It took roughly 15 hours of work. 
Although I was familiar with the engine, a good portion of my time was dedicated to understanding new concepts such as
perlin noise, mesh filter and coroutine optimizations, and mass storage of the map's data.

Features:

 - Place and destroy blocks! There are four different colors to choose from. Scroll through your options with the mouse wheel.
 
 - A fully random and infinite world! Generated with Perlin noise, this map will continue to lay out new hills and deserts for as long 
 as you explore. Other fun additions to the world include trees and different biomes.
 
 - A fully optimized chunk handler. Only 121 chunks of 16x16 blocks are loaded at any time. 
 In those chunks, all cubes are combined into one cohesive mesh. 
 By doing so, the game can run incredibly smooth and load new chunks without any noticeable drop in framerate. 


Implementation:

Unity Basic Assets used:

First Person Controller

UI Canvas system

Outside of this, all logic was structured and programmed by myself. No code was based on or copied from any other source, Scripts I wrote include, but are not limited to:

Chunk Generator,
Terrain Generator,
Player Raycast

Feel free to look inside them to check my work!
If you have any questions, please contact me at eliasimow@gmail.com
