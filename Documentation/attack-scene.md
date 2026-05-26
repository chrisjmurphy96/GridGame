# Attack Scene

Not a code "Scene", just another complex UI element drawn over the existing grid.

- In older FE, a little rectangle zooms in on the selected units? Not sure it adds much.
- The grid tiles are darkened in the background to let the attack scene pop more.
- Some ground in drawn in perspective so the characters aren't floating in space.
- Enemy UI on the left, friendly UI on the right.
  - Names are at the top in banners
  - Towards the bottom the current move stats are shown
  - To the right of that is the name of the equipped move/weapon (along with weapon triangle green/red arrows indicating effectiveness)
  - Health bars are drawn at the bottom
    - Green rectangles are used as health indicators.
    - Bright means current
    - Dull is missing
    - Max health is the total of Bright and Dull
- Up to two attack animations are shown
  - Attacker goes first
  - Defender goes second
  - Characters are drawn from sideways perspective, but with some techniques to create depth
    - This includes shadows below the characters, that will follow them through the attack animation
  - I think sprites might be unique for character/current class/equipped weapon?
  - Generic enemies share sprites
  - Bosses get a unique one
  - Animations are kept the same regardless of character, so there could be fewer sprites
    than I think, with clever color pallette applications, but I don't think that could explain things
    like hair. Though I'm not sure if I've seen special hair/clothing.
  - Special effects like dust may be used to help convey the action, such as the characters on horseback.
    These seem like they're drawn as part of the animation, instead of a separate fx.
