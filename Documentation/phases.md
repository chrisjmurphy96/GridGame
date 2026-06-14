# Phases

## State machine

- Player phase start animation
- Player moves 0-all friendly entities
- Player manually clicks end turn when they're satisfied
- Enemy phase start animation
- All enemy entities decide where to move/who to attack
  - how is this decided? Who moves first? What are some good heuristics?
- Enemy phase ends, loop back to start

- If Player kills all enemies, go to victory screen/dialogue
- If enemy kills all friendly units, go to defeat screen/dialogue

## Phase start animations

- Could be an Animated UIElement that we switch focus to under specific conditions?

## Keeping track of phases

- Property on GridState?
- New PhaseState?

## Enemy phase changes

- The cursor needs to be hidden/disabled while the enemy units make their moves.
- The camera needs to be able to jump around and follow enemy units.
  - We need to be able to wait for a single enemy to complete their turn before selecting the next
  - Attack scene needs logic to handle enemy attackers

### Enemy movement heuristics

- Can you reach an enemy?
- Can you kill an enemy?
  - What are the odds of that (crit/hit)?
- Will the friendly unit kill you if you attack?
