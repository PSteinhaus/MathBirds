using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CocosSharp;

namespace CocosSharpMathGame
{
    /// <summary>
    /// All visible objects in the sky that aren't Sprites are GameObjectNodes (maybe excluding effects like shots or shields)
    /// </summary>
    internal class GameObjectNode : CCNode
    {
        internal GameObjectNode()
        {
            AnchorPoint = CCPoint.AnchorMiddle;
            Scale = 16;
        }
        internal GameObjectNode(CCPoint position, float rotation) : this()
        {
            Position = position;
            Rotation = rotation;
        }
    }
}
