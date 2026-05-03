using System.Collections.Generic;
using System.Numerics;
using Microsoft.Xna.Framework.Graphics;

namespace GridLibrary.Graphics;

// Singleton? We should only have one arrow at a time
public class MovementArrow(SpriteBatch spriteBatch, Camera camera)
{
    private LinkedList<MovementArrowSegment> _arrowSegments = [];

    public int Step { get; set; } = 64;

    public void Move()
    {

    }
}

public class MovementArrowSegment
{

}