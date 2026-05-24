# Game TODO's

## Context menu

- Attack should only be available if at least one move is in range
- This shouldn't be populated via the Moves list, the options are static.
  Instead if something is unavailable, it is removed from the list.
- Wait skips the MovePreview. Perhaps that needs to be a check in MovePreview?

## Move preview

- Should only let players select a square if the attack will hit an enemy

## Grid

## Global

- The enemy attack overlay should be a different color than
  normal (in this case, I'm already using a shade of purple)
- Selecting an enemy with the cursor highlights them red and
  shows their attack range in red.
- Overlap between the two ranges is shown in red.
- Hovering a unit shows their movement/attack range, but at much lower opacity.
- Ranges for enemies are a full fill with a brighter, opaque border.
  This is probably not absolutely necessary, but something to keep in mind.
  It's probably better but this is a proof of concept.
