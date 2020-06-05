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
    class MathSprite : GameObjectSprite
    {
        public MathSprite(string _infix) : base(UIElement.spriteSheet.Frames.Find(_ => _.TextureFilename.Equals("testRotor.png"))) // just a placeholder
        {
            // create and set the texture
            MathToTexture.CreateAndAddTexture(_infix, _infix);
            if (CCTextureCache.SharedTextureCache.Contains(_infix))
                ReplaceTexture(CCTextureCache.SharedTextureCache[_infix], new CCRect(0,0,CCTextureCache.SharedTextureCache[_infix].ContentSizeInPixels.Width, CCTextureCache.SharedTextureCache[_infix].ContentSizeInPixels.Height));
            //var texture = MathToTexture.CreateTexture(_infix);
            //ReplaceTexture(texture, new CCRect(0,0,texture.ContentSizeInPixels.Width, texture.ContentSizeInPixels.Height) );
            //for now manages directly in the base constructor

            // manage size
            ContentSize = Texture.ContentSizeInPixels;
            Console.WriteLine(ContentSize);
            //IsAntialiased = false;
            // set the anchor
            AnchorPoint = CCPoint.AnchorMiddle;
        }
    }
}
