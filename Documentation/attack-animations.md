# Animations

In a proper FE game each character has the following (I think)

- Attack
- Dodge
- Crit?
- 0 Damage?
- Death

## Animation chaining

- Attack can trigger dodge
- Crit can trigger dodge? Maybe looks silly but I think I'm ok with it?
- After dodge or damage, if enemy alive, enemy attacks

## Animation pool

What about instead of having animations directly tied to a Move,
I have the move reference an identifier that can be retrieved with all the relevant info?
