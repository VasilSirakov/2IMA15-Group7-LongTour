# 2IMA15-Group7-LongTour
This repository is used for our course project for the course 2IMA15 Geometric Algorithms at the TU/e.
The project consists of adding a new game to the Ruler of the Plane set of games, found in this repository [kbuchin/ruler](https://github.com/kbuchin/ruler).
## LongTour
Our game is called LongTour and it is based on finding the longest simple tour. In the game, the player is presented with an arbitrary number of vertices and he is able to create a simple tour that visits all vertices. The aim of the player would be to make this tour as long as possible, while keeping it simple (containing no intersections). Furthermore, the game will include a heuristic algorithm, which will calculate a long tour and will show the length of it to the player. The player must then create a tour, which has a greater length than the heuristic one. To determine that the created tours are simple, we are going to employ a plane sweep algorithm for finding intersections. This algorithm will be run on both the player created tour and the heuristic one, in order to ensure that both are simple.
## Algorithms in use
The main geometric algorithm used in the game is a Plane Sweep algorithm used to determine if a tour of points is simple. There is also a heuristic algorithm for generating a long tour over those same points. This algorithm is used to provide the player with a challenge, which they can attempt to beat.
## Data structures in use
Balanced Binary Search Tree - data structure in use for the Plane Sweep algorithm; used both as an event queue and as a status structure.
