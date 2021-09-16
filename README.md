# Dots Classic Clone

## Description

The rule of the game is to connect dots together as long as a dot hits this criteria:
 - Dot is to the left, right, above, or below of another dot
 - Dot is the same color as another dot

After the connecting the dots, they are removed out of the board and the dots below fall into the empty spaces. \
After all of the dots fall to the bottom, new dots generate with a random color at the top of the board. \
The board auto shuffles any time when there is no possible connections that the player could make

## Development Notes
 - Unity Version 2020.1.6 was used to develop this clone

## How to Play
| Action  | Results |
| ---------------- | ------------- |
| Clicking left mouse button on dot    | Highlights the dot  |
| Clicking right mouse button on dot   | Removes the dot out of the board  |
| Holding left mouse button out of dot | Highlights the dot then creates a new line starting at that dot  |
| Holding left mouse button into dot   | Adds the dot into the line, thus connecting two dots from one to the other  |
| Stop holding left mouse button       | Removes the dots in the line or square. If it is a square, then all the dots with the same color and the dots inside the square are removed as well. |

## Game Programming Patterns Used
### Singleton
The [Singleton](https://github.com/dots-jalen-jackson/dots-clone/blob/main/Assets/Scripts/Global/Singleton.cs) script has a type parameter of a ``Component`` that ensures that component is only in a game object once in the active scene.

In general, singletons restrict a class to one instance so no other class creates it and provides a global point of access whenever we need to use a function or variable in that singleton in other scripts.Also, they are initalized at runtime if its instance has not been discovered yet.

In this game, the singleton for these scripts:
 - [DotsBoard](https://github.com/dots-jalen-jackson/dots-clone/blob/main/Assets/Scripts/Dots/DotsBoard.cs)
 - [DotsBoardUpdater](https://github.com/dots-jalen-jackson/dots-clone/blob/main/Assets/Scripts/Dots/DotsBoardUpdater.cs)
 - [DotsLineRenderer](https://github.com/dots-jalen-jackson/dots-clone/blob/main/Assets/Scripts/Dots/DotsLineRenderer.cs)
 - [DotsInputHandler](https://github.com/dots-jalen-jackson/dots-clone/blob/main/Assets/Scripts/Dots/DotsInputHandler.cs)
 - [DotsGenerator](https://github.com/dots-jalen-jackson/dots-clone/blob/main/Assets/Scripts/Dots/DotsBoard.cs)

### Object Pool

The [ObjectPooler](https://github.com/dots-jalen-jackson/dots-clone/blob/main/Assets/Scripts/Global/ObjectPooler.cs) script maintains a collection of the same prefab and take it in or out of the pool depending if we want to use the game object.

If we do, then we take it out of the pool and activate it. Otherwise, bring it back in and deactivate it.

Since ``Instantiate`` and ``Destroy`` uses more resource heavy than toggling the activation of a game object, the Object Pooler is must more useful and avoids memory fragmentation whenever we want to respawn & destroy a game object multiple times

The only time in the clone that the Object Pool was used is the [DotsGenerator](https://github.com/dots-jalen-jackson/dots-clone/blob/main/Assets/Scripts/Dots/DotsBoard.cs) for create an Object pool for the dots on the board.

### Command

There are functions, specifically the ienumerators, that have an action parameter that is executed once the animation is completed. For example, the Dot has an IEnumerator called ``OnDotRemoved(Action onShrinkCompleted)``. In this function, the dot scales down to 0 at a fixed amount of speed. Once the dot's scale hits 0, the ``onShrinkCompleted`` function will be invoked. Finally, when the ``StartShrinkingDot(Dot dot)`` in the ``DotsBoardUpdater`` script gets called, it starts the ``OnDotRemoved`` coroutine on that dot. This is what the ``onShrinkCompleted`` parameter's function does in this example:
 - Returns the dot into the object pool (which deactivates the dot)
 - Places a ``null`` reference type variable in the dot's board matrix at its row and column
 - Starts the `DropDotsDown(int col)` coroutine at that column
