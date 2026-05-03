using System;
using System.Collections.Generic;

namespace GridLibrary.Graphics;

public class Animation 
{
    /// <summary>
    /// The texture regions that make up the frames of this animation.  The order of the regions within the collection
    /// are the order that the frames should be displayed in.
    /// TODO: sorted list might be neat here? Maybe doesn't make much sense, if we have more than a few frames.
    /// </summary>
    public IReadOnlyList<TextureRegion> Frames { get; init; } = [];

    /// <summary>
    /// The amount of time to delay between each frame before moving to the next frame for this animation.
    /// </summary>
    public TimeSpan Delay { get; set; } = TimeSpan.FromMilliseconds(100);

    /// <summary>
    /// 100 Milliseconds
    /// </summary>
    public static TimeSpan DefaultDelay => TimeSpan.FromMilliseconds(100);
}
