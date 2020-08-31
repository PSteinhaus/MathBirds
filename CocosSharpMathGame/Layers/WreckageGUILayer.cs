using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CocosSharp;
using Typography.TextBreak;

namespace CocosSharpMathGame
{
    public class WreckageGUILayer : MyLayer
    {
        private protected WreckageLayer WreckageLayer { get; set; }
        private MathChallengeNode mathChallengeNode;
        internal MathChallengeNode MathChallengeNode
        {
            get { return mathChallengeNode; }
            set
            {
                if (value != mathChallengeNode)
                {
                    if (mathChallengeNode != null)
                        RemoveChild(mathChallengeNode);
                    mathChallengeNode = value;
                    AddChild(mathChallengeNode);
                }
            }
        }
        public WreckageGUILayer(WreckageLayer wreckageLayer) : base(CCColor4B.Transparent, true)
        {
            WreckageLayer = wreckageLayer;
            Scroller = null;
            FirstTouchListener.OnTouchesBegan += OnTouchesBegan;
            FirstTouchListener.OnTouchesMoved += SwipeUpDown;
            FirstTouchListener.OnTouchesEnded += StopSwiping;
            FirstTouchListener.OnTouchesCancelled += StopSwiping;
        }

        private protected new void OnTouchesBegan(List<CCTouch> touches, CCEvent touchEvent)
        {
            if (WreckageLayer.State == WreckageLayer.WreckageState.SALVAGED)
            {
                // switch back to carousel mode
                WreckageLayer.EndSalvage();
            }
        }
        private bool SwipeIsUndecided = true;
        private bool SwipingUpDown = false;
        /// <summary>
        /// Check whether the swipe type has to be decided and if it is an up/down-swipe intercept the event and use it.
        /// </summary>
        /// <param name="touches"></param>
        /// <param name="touchEvent"></param>
        private protected void SwipeUpDown(List<CCTouch> touches, CCEvent touchEvent)
        {
            if (touches.Count == 1)
            {
                if (WreckageLayer.State == WreckageLayer.WreckageState.CAROUSEL && WreckageLayer.WreckCarousel.MiddleNode != null)
                {
                    var touch = touches[0];
                    if (SwipeIsUndecided)
                    {
                        if (touch.Delta != CCPoint.Zero)
                        {
                            SwipingUpDown = Math.Abs(touch.Delta.Y) > Math.Abs(touch.Delta.X);
                            SwipeIsUndecided = false;
                        }
                        // disable the carousel when swiping up/down
                        if (SwipingUpDown)
                            WreckageLayer.WreckCarousel.Pressable = false;
                        if (SwipingUpDown && WreckageLayer.MiddleAircraft != null && (WreckageLayer.MiddleAircraft.GetActionState(ActionTag) == null || WreckageLayer.MiddleAircraft.GetActionState(ActionTag).IsDone))
                            WreckPositionOriginal = WreckageLayer.MiddleAircraft.Position;
                    }
                    if (SwipingUpDown)
                    {
                        // move the wreckage middle node
                        WreckageLayer.MiddleAircraft.PositionY += touch.Delta.Y * 2;
                        // stop the current move-action if there is one
                        WreckageLayer.MiddleAircraft.StopAction(ActionTag);
                    }
                }
            }
            if (SwipingUpDown)
            {
                // swallow the touch event
                touchEvent.StopPropogation();
            }
        }
        private CCPoint WreckPositionOriginal;
        private int ActionTag = 47810023;
        private protected void StopSwiping(List<CCTouch> touches, CCEvent touchEvent)
        {
            if (TouchCount == 0)
            {
                if (SwipingUpDown)
                {
                    touchEvent.StopPropogation();
                    // since the event is stopped here the WreckCarousel has to be informed manually
                    WreckageLayer.WreckCarousel.Pressed = false;
                    const float actionRate = 2f;
                    const float actionTime = 0.25f;
                    CCAction action;
                    var reenableCarousel = new CCCallFunc(() => { if(WreckageLayer.State == WreckageLayer.WreckageState.CAROUSEL) WreckageLayer.WreckCarousel.Pressable = true; });
                    var bounds = WreckageLayer.VisibleBoundsWorldspace;
                    // release the wreck
                    var wreck = WreckageLayer.MiddleAircraft;
                    if (wreck.PositionWorldspace.Y > bounds.Center.Y + bounds.Size.Height*0.25f)
                    {
                        // salvage
                        WreckageLayer.WreckCarousel.Pressable = false;
                        action = new CCSequence(new CCEaseOut(new CCMoveTo(actionTime, WreckPositionOriginal + new CCPoint(0, VisibleBoundsWorldspace.Size.Height)), actionRate),
                                                new CCCallFunc(WreckageLayer.Salvage)/*,
                                                reenableCarousel*/);
                    }
                    else if (wreck.PositionWorldspace.Y < bounds.Center.Y - bounds.Size.Height * 0.25f &&
                             WreckageLayer.GetWreckPercentile(WreckageLayer.MiddleAircraft) < WreckageLayer.GetWreckMaxPercentile(WreckageLayer.MiddleAircraft))
                    {
                        // repair
                        WreckageLayer.WreckCarousel.Pressable = false;
                        action = new CCSequence(new CCEaseOut(new CCMoveTo(actionTime, WreckPositionOriginal), actionRate),
                                                new CCCallFunc(WreckageLayer.StartRepair)/*,
                                                reenableCarousel*/);
                    }
                    else
                    {
                        // reset
                        action = new CCSequence(new CCEaseOut(new CCMoveTo(actionTime, WreckPositionOriginal), actionRate),
                                                reenableCarousel);
                    }
                    action.Tag = ActionTag;
                    wreck.AddAction(action);
                }
                // reset flags
                SwipeIsUndecided = true;
                SwipingUpDown = false;
            }
        }

        internal override void Clear()
        {
            WreckageLayer = null;
            this.mathChallengeNode = null;
            this.FirstTouchListener = null;
            //this.Scroller.MoveFunction = null;
            this.Scroller = null;
            this.StopAllActions();
            this.ResetCleanState();
        }

        internal void MoveMathChallengeNodeAway()
        {
            var currentMathChallengeNode = MathChallengeNode;
            currentMathChallengeNode.Pressable = false;
            currentMathChallengeNode.AddAction(new CCSequence(new CCEaseIn(new CCMoveBy(1f, new CCPoint(0, -currentMathChallengeNode.ContentSize.Height)), 2f),
                                                              new CCCallFunc(() => currentMathChallengeNode.RemoveFromParent())));
        }
    }
}
