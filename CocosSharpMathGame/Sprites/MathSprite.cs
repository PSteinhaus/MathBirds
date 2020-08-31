using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CocosSharp;

namespace CocosSharpMathGame
{
    /// <summary>
    /// a class, turning math into a drawable sprite
    /// </summary>
    internal class MathSprite : CCSprite
    {
        internal string MathInfix { get; set; }
        internal MathSprite(string infix, string latex) : base()
        {
            MathInfix = infix;
            // create and set the texture
            MathToTexture.CreateAndAddTexture(latex, latex);
            if (CCTextureCache.SharedTextureCache.Contains(latex))
            {
                var texture = CCTextureCache.SharedTextureCache[latex];
                ReplaceTexture(texture, new CCRect(0, 0, texture.ContentSizeInPixels.Width, texture.ContentSizeInPixels.Height));
            }
            //var texture = MathToTexture.CreateTexture(_infix);
            //ReplaceTexture(texture, new CCRect(0,0,texture.ContentSizeInPixels.Width, texture.ContentSizeInPixels.Height) );
            //for now manages directly in the base constructor

            // manage size
            //ContentSize = Texture.ContentSizeInPixels;
            Scale = 1f;
            //Console.WriteLine(ContentSize);
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

        public float Area
        {
            get { return BoundingBoxTransformedToWorld.Size.Width * BoundingBoxTransformedToWorld.Size.Height; }
        }

        public void FitToBox(CCSize box)
        {
            if (ContentSize.Width / box.Width > ContentSize.Height / box.Height)
                FitToWidth(box.Width);
            else
                FitToHeight(box.Height);
        }
    }
}
