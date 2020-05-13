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
    internal abstract class GameObjectNode : CCNode, IGameObject
    {
        // 0 is considered EAST; rotation is clockwise
        private float myRotation = 0;
        public float MyRotation
        {
            get
            {
                return myRotation;
            }
            set
            {
                myRotation = value % 360;
                Rotation = myRotation;  // set the "actual" rotation to match
            }
        }

        public float GetScale()
        {
            return ScaledContentSize.Width / ContentSize.Width;
        }
        internal GameObjectNode()
        {
            AnchorPoint = CCPoint.AnchorMiddle;
            Scale = Constants.STANDARD_SCALE;
        }
    }
}
