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
    internal class GameObjectNode : CCNode, IGameObject
    {
        public bool MyVisible {
            get
            {
                if (Visible == false)
                    return false;
                else if (Parent != null)
                {
                    if (Parent is IGameObject g)
                        return g.MyVisible;
                    else
                        return Parent.Visible;
                }
                else
                    return true;
            }
        }
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

        public void RotateTowards(float angle, float maxRotationAngle)
        {
            float currentRotation = MyRotation;
            float difference = Constants.AngleFromToDeg(currentRotation, angle);
            if ((float)Math.Abs(difference) <= maxRotationAngle)
                MyRotation = angle;
            else
                MyRotation += maxRotationAngle * Math.Sign(difference);
        }

        public float GetScale()
        {
            float scale = ScaledContentSize.Width / ContentSize.Width;
            if (Parent != null && (Parent is IGameObject g))
                scale *= g.GetScale();
            return scale;
        }
        /// <summary>
        /// sets the scaling to fit a certain width in pixels
        /// </summary>
        /// <param name="width">how wide the sprite shall be (in world pixels)</param>
        public void FitToWidth(float desiredWidth)
        {
            Scale = desiredWidth / ContentSize.Width;
        }

        /// <summary>
        /// sets the scaling to fit a certain height in pixels
        /// </summary>
        /// <param name="height">how high the sprite shall be (in world pixels)</param>
        public void FitToHeight(float desiredHeight)
        {
            Scale = desiredHeight / ContentSize.Height;
        }
        internal GameObjectNode()
        {
            AnchorPoint = CCPoint.AnchorMiddle;
            Scale = Constants.STANDARD_SCALE;
        }

        /// <summary>
        /// Returns a simple collision polygon, that is a diamond based on the ContentSize
        /// </summary>
        /// <returns></returns>
        public CCPoint[] DiamondCollisionPoints()
        {
            return new CCPoint[] { new CCPoint(0, (ContentSize.Height / 2)), new CCPoint((ContentSize.Width / 2), ContentSize.Height), new CCPoint(ContentSize.Width, (ContentSize.Height / 2)), new CCPoint((ContentSize.Width / 2), 0) };
        }
    }


}
