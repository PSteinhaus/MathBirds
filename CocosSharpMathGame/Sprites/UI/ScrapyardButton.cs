using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using CocosSharp;

namespace CocosSharpMathGame
{
    /// <summary>
    /// these are the buttons that the player can click on in the scrapyard
    /// </summary>
    internal class ScrapyardButton : Button
    {
        internal static readonly CCSize ButtonSize = new CCSize(200,200);
        internal MathChallenge ChallengeModel { get; private protected set; }
        internal MathChallengeNode CurrentMathChallengeNode { get; private set; }
        internal ScrapyardButton(MathChallenge challengeModel, string textureName) : base(textureName, false)
        {
            ChallengeModel = challengeModel;
            FitToBox(ButtonSize);
            // check if the challenge type is locked
            if (ChallengeModel.Locked)
            {
                var frame = UIElement.spriteSheet.Frames.Find(_ => _.TextureFilename.Equals("scrapyardLockedv2.png"));
                ReplaceTexture(frame.Texture, frame.TextureRectInPixels);
                Pressable = false;
                Color = CCColor3B.Gray;
            }
        }

        private protected override void ButtonEnded(CCTouch touch)
        {
            ((HangarLayer)Layer).EnterScrapyardChallengeState(this);
        }

        internal MathChallengeNode GenerateMathChallengeNode()
        {
            return new MathChallengeNode(ChallengeModel.CreateFromSelf());
        }

        internal void CreateNextChallenge()
        {
            CurrentMathChallengeNode = GenerateMathChallengeNode();
        }
    }
}
