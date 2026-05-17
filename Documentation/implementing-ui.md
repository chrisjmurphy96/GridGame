# UI Implementation

It probably makes sense to use HTML/CSS principles as a guide.

## Focus

The idea of focus in HTML is global. Only one element can have focus at a time.
Therefore, instead of a `HasFocus` on each element, I want a global UI manager that
can have a reference to a focused UI element, or be null.

- Only the focused element should receive inputs
- Should the Grid itself also be a UI element? The focus concept certainly applies.

## Positioning

- Can be static or relative, to parent element or global screen
- Can be a relative unit like percentage or VH/VW, or precise like pixels
- The UI should also resize itself based on current viewport dimensions,
  but that feels like a nice-to-have at the moment.

Some example syntax, to start getting an idea

```cs
uiElement.Position.X.Percentage(50).RelativeTo(viewPort);
```
