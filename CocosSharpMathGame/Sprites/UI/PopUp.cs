using CocosSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CocosSharpMathGame
{
    internal class PopUp : VerticalScalingButton
    {
        private string text;
        private protected CCLabel label;
        internal string Text
        {
            get
            {
                return text;
            }
            set
            {
                // update the label
                text = value;
                label.Text = text;
                const float BORDER = 50f;
                FitToHeight(label.ContentSize.Height + BORDER);
                label.Position = (CCPoint)ContentSize / 2;
            }
        }
        internal PopUp(string text) : base("popUpStart.png", "popUpMid.png", "popUpEnd.png")
        {
            Scale = 2.3f;
            AnchorPoint = CCPoint.AnchorMiddle;
            label = new CCLabel("", "EarlyGameBoy", 12, CCLabelFormat.SpriteFont);
            AddChild(label);
            label.IsAntialiased = false;
            label.Color = CCColor3B.White;
            label.AnchorPoint = CCPoint.AnchorMiddle;
            label.HorizontalAlignment = CCTextAlignment.Center;
            Text = text;
            MakeClickable(touchMustEndOnIt: false);
        }

        private protected override void OnTouchesBeganUI(List<CCTouch> touches, CCEvent touchEvent)
        {
            touchEvent.StopPropogation();
            if (touches.Count > 0)
            {
                // turn darker when pressed
                StartSprite.Color = CCColor3B.Gray;
                MiddleSprite.Color = CCColor3B.Gray;
                EndSprite.Color = CCColor3B.Gray;
                label.Color = CCColor3B.Gray;
            }
        }

        private protected override void OnTouchesEndedUI(List<CCTouch> touches, CCEvent touchEvent)
        {
            touchEvent.StopPropogation();
            if (touches.Count > 0)
            {
                // turn back to original color when released
                StartSprite.Color = CCColor3B.White;
                MiddleSprite.Color = CCColor3B.White;
                EndSprite.Color = CCColor3B.White;
                label.Color = CCColor3B.White;
                var touch = touches[0];
                if (TouchIsOnIt(touch))
                {
                    // move the popUp out of view and remove it
                    RemovePerAction();
                    Pressable = false;
                }
            }
        }

        const float MOVE_DURATION = 0.75f;
        const float MOVE_EASE_RATE = 3f;

        private protected void RemovePerAction()
        {
            var bounds = VisibleBoundsWorldspace;
            // move out and remove yourself afterwards
            AddAction(new CCSequence(new CCEaseIn(new CCMoveTo(MOVE_DURATION, new CCPoint(bounds.Center.X, bounds.Size.Height + bounds.Center.Y)), MOVE_EASE_RATE), new CCRemoveSelf()));
        }

        /// <summary>
        /// Adds a popup to the given layer. It moves in from below, placing itself in the center of the layer bounds.
        /// </summary>
        /// <param name="layerToShowOn"></param>
        /// <param name="text"></param>
        internal static void ShowPopUp(CCLayer layerToShowOn, string text)
        {
            var popUp = new PopUp(text);
            layerToShowOn.AddChild(popUp, int.MaxValue);
            var bounds = layerToShowOn.VisibleBoundsWorldspace;
            popUp.Position = new CCPoint(bounds.Center.X, -bounds.Center.Y);
            // move in
            popUp.AddAction(new CCEaseOut(new CCMoveTo(MOVE_DURATION, bounds.Center), MOVE_EASE_RATE));
        }
    }
}
