using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CocosSharp;

namespace CocosSharpMathGame
{
    /// <summary>
    /// All visible objects in the sky that aren't pure Nodes are GameObjectSprites (maybe excluding effects like shots or shields)
    /// </summary>
    internal abstract class GameObjectSprite : CCSprite, IGameObject
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

        public void RotateTowards(float angle, float maxRotationAngle)
        {
            float currentRotation = MyRotation;
            float difference = Constants.AngleFromToDeg(currentRotation, angle);
            if ((float)Math.Abs(difference) <= maxRotationAngle)
                MyRotation = angle;
            else
                MyRotation += maxRotationAngle * Math.Sign(difference);
        }

        public float TotalRotation
        {
            get
            {
                var totalRotation = MyRotation;
                if (Parent != null && Parent is IGameObject)
                    totalRotation += ((IGameObject)Parent).TotalRotation;
                return totalRotation;
            }
        }

        public float GetScale()
        {
            return ScaledContentSize.Width / ContentSize.Width;
        }
        internal GameObjectSprite(CCSpriteFrame spriteFrame) : base(spriteFrame)
        {
            IsAntialiased = false;
            AnchorPoint = CCPoint.AnchorMiddle;
            Scale = Constants.STANDARD_SCALE;
        }
    }
}
