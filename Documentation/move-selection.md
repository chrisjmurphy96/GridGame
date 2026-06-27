# Move selection

After selecting where to move, fire emblem

## Step 1

- Hides the move overlay
- Moves the character to the end of the movement arrow
- Shows the valid attack squares based on currently equipped weapon/spell
- Shows the context menu with valid commands
  - wait
  - attack (if any weapons/spells are in range of an enemy)
  - items
- If the player selects Attack, lets the user select a weapon/spell to complete the attack
  - This updates attack squares based on the selection
- After selecting a weapon/spell, lets the user pick a valid attack square
- After picking a valid attack square, shows a preview of the friendly and enemy character's attacks.
- After confirming, overlays a scene where the attack and health animations play out, resolving combat.

## How to implement all this

### The state machine

- Grid becomes a UIElement
  - This could be a problem if I want to let the player
    do things like toggle enemy attack ranges during other actions?
    What if I made those things things part of the state machine instead?
- MoveOverlay becomes a UIElement
- This allows us to manage focus centrally
  - Stretch goal: Could rework UIElement to not require a Texture.
- TurnStateMachine.cs
  - Holds everything, calling SetFocus on what needs to be active
  - Start with Grid focus
  - If friendly character clicked, switch to MovementArrow focus
    - If this is cancelled, switch back to Grid
  - If valid movement selected, switch to ContextMenu
    - If this is cancelled, switch back to MoveOverlay
  - If a Move is selected:
    - hide the MoveOverlay and MovementArrow
    - move the character to the end of the movement arrow
    - Show the valid attack squares based on currently equipped weapon/spell (new AttackOverlay UIElement?)

### Pathfinding notes

This is an expensive process, so it has to be managed properly. The two effective toggles I've found
are `MAX_ITERATIONS` in `Dijkstra.cs`, and `MAX_WALK_DISTANCE` in `EnemyDecisionMaker.cs`.
`MAX_ITERATIONS` is the more important of the two since it directly controls how much time we spend calculating the path.
`MAX_WALK_DISTANCE` can actually go pretty high and be just fine, since Dijkstra's algorithm uses a priority queue to
explore the available space, we're quite likely to find a pretty decent path.
