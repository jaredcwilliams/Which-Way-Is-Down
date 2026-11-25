# Which Way Is Down

## About

Which Way Is Down is a platformer game created on Unity centered around gravity control. Our team members are Jared Williams, Guga Tchaturia, Ana Kopadze, Andria Utiashvili, and Malkhaz Goderdzishvili. The current target for this game is to create a tall character with movement and jumping, a camera that does not clip through walls, a sample level with a gravity switch and goal, and working rotation of the room when the gravity switch is activated.

## Group Member Goals

Repository, README, integration, and presentation - Jared Williams
- Choose the game name
- Make the README
- Make and manage the repository
- Test and merge branches
- Prepare the Unity demonstration/video
- Make sure at least two people have gameplay code committed to Git

Player movement and third-person camera - Guga Tchaturia
- Create the player
- Add basic movement and jumping
- Set up the third-person camera

Gravity prototype - Ana Kopadze
- Implement one working gravity switch
- Change the player’s gravity/orientation
- Demonstrate the player moving onto another surface

Prototype level - Andria Utiashvili
- Build one simple room or test area
- Add floors, walls, and cubes
- Place the player and gravity switch

Interaction and gameplay objective - Malkhaz Goderdzishvili
- Add a simple objective
- Add an endpoint
- Add a reset button

## Game Overview
3D puzzle-platforming game in which the player must navigate through black-and-white rooms by changing the direction of gravity. Each room contains cubes attached to the floors, walls, and ceilings. The player must use gravity switches to rotate the room and reach switches that are difficult to access.
The game does not include enemies or combat. The main challenge is understanding the room’s layout, choosing the correct gravity direction, and finding a path to the objective switch.

## Game Objective
The player begins each level inside a closed room. The exit door is locked, and the player must reach and activate a specific objective switch to complete the level.
Some objective switches may be:
- Hidden behind cube structures
- Located on a wall or ceiling
- Accessible only after rotating the room
- Revealed after activating another switch
- Located at the end of a difficult platforming path

After the correct switch is activated, the exit door opens. The player must then reach the exit and move to the next level.

## Core Gameplay
The main gameplay loop is:
1. Explore the room.
2. Identify the available gravity switches.
3. Press a gravity switch.
4. Rotate the room or change the direction of gravity.
5. Land on a new wall, floor, or ceiling.
6. Navigate across the cubes and platforms.
7. Find and activate the objective switch.
8. Reach the exit door.
9. Continue to the next level.

The player must carefully observe the room before changing gravity because each rotation changes which surfaces can be used as platforms.

## Gravity Mechanic
Gravity switches are placed throughout each room. When the player activates one, the room rotates in a specific direction, such as:
- 90 degrees to the left
- 90 degrees to the right
- 180 degrees upside down

The player is affected by the rotation and lands on the new floor. The previous wall may become the new floor, while the previous floor may become a wall.


## Player Controls
- WASD: Move the player
- Mouse: Look around
- Space: Jump
- E: Interact with a nearby switch or door
- R: Restart the current level

## Visual Style
The game will use a minimal black-and-white visual style.
The environment will include:
- Black walls and backgrounds
- White or light-gray cubes
- A slender white player character
- Simple geometric switches
- Strong shadows and lighting
- No detailed furniture or realistic objects
- Simple cube-based rooms and platforms

The gravity switches and objective switches can be visually different through their shapes, symbols, or lighting. For example, gravity switches may have arrows showing the direction of rotation, while the objective switch may have a brighter outline.

## Audio Assets:
- Jumping and Landing: Sounds when the player jumps and lands or maybe falls from one wall to another when the room rotates. 
- Objective Switch: A distinct sound when the correct objective switch is activated.
- Game: A sound for completing a room and a separate sound for completing the final room.

We can add more sounds as we build the game.



