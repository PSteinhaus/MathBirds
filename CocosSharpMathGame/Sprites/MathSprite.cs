using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CocosSharp;

namespace CocosSharpMathGame
{
    /// <summary>
    /// For now, a test class, turning math into a drawable sprite
    /// </summary>
    class MathSprite : CCSprite
    {
        public MathSprite(string _infix) : base(MathToTexture.CreateTexture(_infix))
        {
            // create and set the texture
            //for now manages directly in the base constructor
            IsAntialiased = false;
            // set the anchor
            AnchorPoint = CCPoint.AnchorMiddle;
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
    }
}
