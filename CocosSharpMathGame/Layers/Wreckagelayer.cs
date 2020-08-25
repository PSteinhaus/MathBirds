using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using CocosSharp;
using MathNet.Numerics.Random;
using MathNet.Symbolics;
using Xamarin.Essentials;

namespace CocosSharpMathGame
{
    public class WreckageLayer : MyLayer
    {
        internal enum WreckageState
        {
            CAROUSEL, SALVAGING, SALVAGED, REPAIR
        }
        private WreckageState state = WreckageState.CAROUSEL;
        internal WreckageState State
        {
            get { return state; }
            private protected set
            {
                WreckCarousel.Pressable = false;
                WreckCarousel.Pressed = false;
                Console.WriteLine(value);
                switch (value)
                {
                    case WreckageState.CAROUSEL:
                        {
                            WreckCarousel.Pressable = true;
                            // check if all wrecks have been processed and return to the hangar if true
                            if (!WreckCarousel.Collection.Any())
                                ReturnToHangar();
                            break;
                        }
                    case WreckageState.SALVAGING:
                        {
                            break;
                        }
                    case WreckageState.SALVAGED:
                        {
                            break;
                        }
                    case WreckageState.REPAIR:
                        {
                            break;
                        }
                    default:
                        break;
                }
                state = value;
            }
        }



        private void ReturnToHangar()
        {
            var hangarLayer = new HangarLayer();
            foreach (var part in TotalSalvagedParts)
                hangarLayer.AddPart(part);
            TransitionFadingFromTo(this.GUILayer, hangarLayer.GUILayer, this, hangarLayer, 2f);
            /*
            var parent = Parent;
            RemoveAllListeners();
            GUILayer.RemoveAllListeners();
            Parent.RemoveChild(GUILayer);
            Parent.RemoveChild(this);
            parent.AddChild(HangarLayer.GlobalHangarLayer.GUILayer);
            parent.AddChild(HangarLayer.GlobalHangarLayer, zOrder: int.MinValue);
            */
        }

        public WreckageGUILayer GUILayer { get; private protected set; }
        internal List<Aircraft> Wrecks
        {
            get
            {
                //return WreckCarousel.Collection.Cast<Aircraft>().ToList();
                
                var wrecks = new List<Aircraft>();
                foreach (var node in WreckCarousel.Collection)
                    foreach (var child in ((CCNode)node).Children)
                        if (child is Aircraft a)
                            wrecks.Add(a);
                return wrecks;
            }
            private protected set
            {
                WreckCarousel.ClearCollection();
                foreach (var aircraft in value)
                {
                    
                    // create a container node
                    var container = new GameObjectNode();
                    container.Scale = 1f;
                    aircraft.AnchorPoint = CCPoint.AnchorMiddle;
                    //aircraft.Position = CCPoint.Zero;
                    container.AddChild(aircraft);
                    container.ContentSize = aircraft.ContentSize;
                    aircraft.Position = (CCPoint)container.ContentSize / 2;
                    container.AnchorPoint = CCPoint.AnchorMiddle;
                    // add a label displaying the percentage
                    var label = new CCLabel("100%", "alphbeta", 16, CCLabelFormat.SpriteFont);
                    container.AddChild(label);
                    label.AnchorPoint = CCPoint.AnchorMiddleTop;
                    label.Position = aircraft.Position;
                    label.Position -= new CCPoint(0, aircraft.ContentSize.Height / 2 + 100f);
                    label.Color = CCColor3B.White;
                    label.Scale = 2f;
                    label.IsAntialiased = false;
                    // DEBUG
                    //var drawNode = new CCDrawNode();
                    //container.AddChild(drawNode, 1000);
                    //drawNode.DrawSolidCircle(CCPoint.Zero, 10f, CCColor4B.Red);
                    //drawNode.DrawSolidCircle(container.BoundingBox.Center, 10f, CCColor4B.Red);
                    //drawNode.DrawSolidCircle((CCPoint)container.ContentSize/2, 10f, CCColor4B.Red);
                    WreckCarousel.AddToCollection(container);
                }
            }
        }
        internal const float MIN_START_WRECKAGE_PERCENTILE = 0.1f;
        internal const float MAX_START_WRECKAGE_PERCENTILE = 0.4f;

        internal void EndSalvage()
        {
            CCPoint posChange = WreckCarousel.RemoveFromCollection((IGameObject)WreckCarousel.MiddleNode);
            WreckCarousel.MoveCollectionNode(-posChange);
            // remove the salvaged parts from screen
            CCPoint outPoint = VisibleBoundsWorldspace.Center - new CCPoint(0, VisibleBoundsWorldspace.Size.Height);
            foreach (var part in SalvagedParts)
            {
                // stop the endless rotation
                part.StopAction(RotationTag);
                part.AddAction(new CCSequence(new CCEaseIn(new CCMoveTo(1f, outPoint), 2f),
                                              new CCCallFunc(() => { RemoveChild(part); })));
            }
            TotalSalvagedParts.AddRange(SalvagedParts);
            SalvagedParts.Clear();
            if (WreckCarousel.Collection.Any())
                State = WreckageState.CAROUSEL;
            else
                AddAction(new CCSequence(new CCDelayTime(1.1f),
                                         new CCCallFunc(() => { State = WreckageState.CAROUSEL; })));
        }

        internal const float MIN_END_WRECKAGE_BONUS_PERCENTILE = 0.5f;
        internal const float MAX_END_WRECKAGE_PERCENTILE   = 1.0f;
        private Dictionary<Aircraft, float> wreckPercentile = new Dictionary<Aircraft, float>();
        internal float GetWreckPercentile(Aircraft aircraft)
        {
            return wreckPercentile[aircraft];
        }
        internal float GetWreckMaxPercentile(Aircraft aircraft)
        {
            return WreckMaxPercentile[aircraft];
        }
        internal void SetWreckPercentile(Aircraft aircraft, float percent)
        {
            if (aircraft == null || (aircraft.Layer != this && aircraft.Layer != GUILayer))
                return;
            wreckPercentile[aircraft] = percent;
            // update colors
            CCColor4B darkColor = new CCColor4B(50, 50, 50);
            CCColor3B color = new CCColor3B(CCColor4B.Lerp(darkColor, CCColor4B.White, percent));
            CCColor3B labelColor = new CCColor3B(CCColor4B.Lerp(darkColor, CCColor4B.White, percent + (1-percent)*0.40f));
            aircraft.ChangeColor(color);
            // update the label
            foreach (var child in aircraft.Parent.Children)
                if (child is CCLabel label)
                {
                    label.Text = (percent*100).ToString("n2")+"%";
                    label.Color = labelColor;
                    break;
                }
        }
        internal Dictionary<Aircraft, float> WreckMaxPercentile { get; private protected set; } = new Dictionary<Aircraft, float>();
        internal List<Part> SalvagedParts = new List<Part>();
        internal List<Part> TotalSalvagedParts = new List<Part>();
        internal Carousel WreckCarousel { get; private protected set; }
        internal Aircraft MiddleAircraft
        {
            get
            {
                Aircraft mAircraft = null;
                foreach (var child in WreckCarousel.MiddleNode.Children)
                    if (child is Aircraft a)
                    {
                        mAircraft = a;
                        break;
                    }
                return mAircraft;
            }
        }

        public bool DEBUG = false;
        internal GameObjectNode Hints { get; private set; }
        public WreckageLayer() : base(CCColor4B.Black)
        {
            GUILayer = new WreckageGUILayer(this);
            Scroller = null;
            // add a touch Listener
            var touchListener = new CCEventListenerTouchAllAtOnce();
            touchListener.OnTouchesBegan = OnTouchesBegan;
            touchListener.OnTouchesMoved = OnTouchesMoved;
            touchListener.OnTouchesEnded = OnTouchesEnded;
            touchListener.OnTouchesCancelled = OnTouchesEnded;
            AddEventListener(touchListener, int.MaxValue);
        }

        protected override void AddedToScene()
        {
            base.AddedToScene();
            var bounds = VisibleBoundsWorldspace;

            // add the hints
            WreckageHint upNode = new WreckageHint("Salvage", true);
            WreckageHint downNode = new WreckageHint("Repair", false);
            upNode.AnchorPoint = CCPoint.AnchorMiddleTop;
            downNode.AnchorPoint = CCPoint.AnchorMiddleBottom;
            AddChild(upNode, -1000);
            AddChild(downNode, -1000);
            const float HINT_BORDER = 64f;
            upNode.Position   = new CCPoint(bounds.Center.X, bounds.Size.Height - HINT_BORDER);
            downNode.Position = new CCPoint(bounds.Center.X, HINT_BORDER);
            /*
            var drawNode = new CCDrawNode();
            drawNode.DrawSolidCircle(bounds.Center, 20f, CCColor4B.AliceBlue);
            AddChild(drawNode, zOrder: int.MaxValue);
            */
            WreckCarousel = new Carousel(new CCSize(bounds.Size.Width, bounds.Size.Height / 2));
            WreckCarousel.SpacingFactor = 12f;
            WreckCarousel.ScaleFactor = 2f;
            WreckCarousel.MinYMod = -bounds.Size.Height / 5;
            WreckCarousel.AnchorPoint = CCPoint.AnchorMiddle;
            WreckCarousel.Position = bounds.Center;
            AddChild(WreckCarousel);

            if (DEBUG)
                InitWreckage(new List<Aircraft>() { Aircraft.CreateTestAircraft(), Aircraft.CreateTestAircraft() });
        }

        internal void InitWreckage(List<Aircraft> downedAircrafts)
        {
            if (downedAircrafts.Any())
            {
                Wrecks = downedAircrafts;
                // roll the repair percentages
                var rng = new Random();
                foreach (var aircraft in downedAircrafts)
                {
                    float startP = MIN_START_WRECKAGE_PERCENTILE + (float)rng.NextDouble() * (MAX_START_WRECKAGE_PERCENTILE - MIN_START_WRECKAGE_PERCENTILE);
                    SetWreckPercentile(aircraft, startP);
                    float minEndP = startP + MIN_END_WRECKAGE_BONUS_PERCENTILE;
                    WreckMaxPercentile[aircraft] = Constants.Clamp(minEndP + (float)rng.NextDouble() * (MAX_END_WRECKAGE_PERCENTILE - minEndP), minEndP, MAX_END_WRECKAGE_PERCENTILE);
                }
            }
            else
                ReturnToHangar();
        }
        int RotationTag = 73465253;
        internal void Salvage()
        {
            State = WreckageState.SALVAGING;
            var mAircraft = MiddleAircraft;
            var totalScale = mAircraft.GetTotalScale();
            mAircraft.Visible = false;
            List<Part> totalparts = mAircraft.TotalParts;
            // unmount and disassemble
            mAircraft.Body = null;
            foreach (var part in totalparts)
            {
                part.Disassemble();
                foreach (var singlePart in totalparts)
                {
                    if (singlePart.Flipped) singlePart.Flip();
                }
                // repair the part fully
                part.Reinitialize();
            }
            // choose the parts that will be salvaged
            SalvagedParts = new List<Part>();
            // how many?
            var rng = new Random();
            int salvageCount = (int)(GetWreckPercentile(mAircraft) * totalparts.Count());
            if (salvageCount != totalparts.Count() && rng.NextDouble() <= (GetWreckPercentile(mAircraft) * totalparts.Count()) % 1)
                salvageCount ++;
            // chose random parts
            for (int i=0; i<salvageCount; i++)
            {
                var index = rng.Next(totalparts.Count());
                SalvagedParts.Add(totalparts.ElementAt(index));
                totalparts.RemoveAt(index);
            }
            float delay = SalvagedParts.Count * 1000;
            float delaySec = delay / 1000;
            // vibrate
            if (Constants.oS != Constants.OS.WINDOWS)
                Vibration.Vibrate(delay * 0.8);
            // visualize
            var boundsCenter = VisibleBoundsWorldspace.Center;
            CCPoint pointIn = boundsCenter + new CCPoint(0, VisibleBoundsWorldspace.Size.Height * 0.6f);
            var width = VisibleBoundsWorldspace.Size.Width / 3;
            float inMoveTime = 1f;
            Dictionary<Part, CCPoint> destinations = new Dictionary<Part, CCPoint>();
            foreach (var part in SalvagedParts)
            {
                part.Visible = true;
                AddChild(part, -10);
                part.AnchorPoint = CCPoint.AnchorMiddle;
                var rotation = new CCRepeatForever(new CCRotateBy(1, rng.Next(20, 30) * 0.5f));
                rotation.Tag = RotationTag;
                part.AddAction(rotation);
                // find a free spot
                part.Scale = totalScale;
                const float BORDER = 32f;
                bool notFound = true;
                // first try to find a free space where the part can rotate without touching anything
                for (int tries=0; tries<40 && notFound; tries++)
                {
                    destinations[part] = Constants.RandomPointBoxnear(boundsCenter, width, rng);
                    // check whether the space is free
                    part.Position = destinations[part];
                    CCRect bbox = part.BoundingBoxTransformedToWorld;
                    // construct a bounding square
                    float size = bbox.Size.Height > bbox.Size.Width ? bbox.Size.Height : bbox.Size.Width;
                    bbox = new CCRect(bbox.Center.X - size / 2, bbox.Center.Y - size / 2, size, size);
                    // add a bit of padding
                    CCRect box = new CCRect(bbox.MinX - BORDER, bbox.MinY - BORDER, bbox.Size.Width + 2 * BORDER, bbox.Size.Height + 2 * BORDER);
                    notFound = false;
                    foreach (var otherPart in SalvagedParts)
                    {
                        if (otherPart == part) continue;
                        if (box.IntersectsRect(otherPart.BoundingBoxTransformedToWorld))
                        {
                            notFound = true;
                            break;
                        }
                    }
                }
                // if this failed try to find a free space where the parts at least do not touch in starting configuration
                for (int tries = 0; tries < 40 && notFound; tries++)
                {
                    destinations[part] = Constants.RandomPointBoxnear(boundsCenter, width, rng);
                    // check whether the space is free
                    part.Position = destinations[part];
                    CCRect bbox = part.BoundingBoxTransformedToWorld;
                    // add a bit of padding
                    CCRect box = new CCRect(bbox.MinX - BORDER, bbox.MinY - BORDER, bbox.Size.Width + 2 * BORDER, bbox.Size.Height + 2 * BORDER);
                    notFound = false;
                    foreach (var otherPart in SalvagedParts)
                    {
                        if (otherPart == part) continue;
                        if (box.IntersectsRect(otherPart.BoundingBoxTransformedToWorld))
                        {
                            notFound = true;
                            break;
                        }
                    }
                }
            }
            foreach (var part in SalvagedParts)
            {
                part.Position = pointIn;
                part.AddAction(new CCSequence(new CCDelayTime(delaySec), new CCEaseOut(new CCMoveTo(inMoveTime, destinations[part]), 2f)));
            }
            // count down the percentage
            float startP = GetWreckPercentile(mAircraft);
            CCLabel percentLabel = GetPercentLabel(mAircraft);
            AddAction(new CCSequence(new CCEaseIn(new CCCallFiniteTimeFunc(delaySec, (progress, duration) => { SetWreckPercentile(mAircraft, startP*(1 - progress)); }), 4f), new CCCallFunc(() => { percentLabel.Visible = false; })));
            AddAction(new CCSequence(new CCDelayTime(delaySec + inMoveTime), new CCCallFunc(() => { State = WreckageState.SALVAGED; })));
        }

        CCLabel GetPercentLabel(Aircraft wreck)
        {
            CCLabel label = null;
            foreach (var child in wreck.Parent.Children)
                if (child is CCLabel l)
                {
                    label = l;
                }
            return label;
        }
        
        internal void StartRepair()
        {
            State = WreckageState.REPAIR;
            // only go into repair-mode when the wreck-percentile can still actually be increased 
            if (GetWreckPercentile(MiddleAircraft) < GetWreckMaxPercentile(MiddleAircraft))
            {
                // generate and show math
                var mathChNode = new MathChallengeNode(MiddleAircraft.GetChallenge());
                GUILayer.MathChallengeNode = mathChNode;
                mathChNode.Position = new CCPoint(0, -mathChNode.ContentSize.Height);
                mathChNode.AddAction(new CCEaseOut(new CCMoveTo(0.5f, CCPoint.Zero), 3f));
                mathChNode.AnswerChosenEvent += AnswerChosen;
            }
            else
                EndRepair(false);
        }

        private void AnswerChosen(object sender, bool answerIsCorrect)
        {
            if (answerIsCorrect)
            {
                var mAircraft = MiddleAircraft;
                float oldP = GetWreckPercentile(mAircraft);
                float newP = Math.Min(oldP + 0.1f * ((MathChallengeNode)sender).Multiplier, GetWreckMaxPercentile(mAircraft));
                float diff = newP - oldP;
                AddAction(new CCEaseIn(new CCCallFiniteTimeFunc(1f, (progress, duration) => { SetWreckPercentile(mAircraft, newP - diff * (1 - progress)); }), 4f));
                EndRepair(false);
            }
            else
            {
                WasteCurrentWreck();
            }
            // move the challengeNode away
            GUILayer.MoveMathChallengeNodeAway();
        }

        private void WasteCurrentWreck()
        {
            GUILayer.AddScreenShake(50f, 50f);
            var mAircraft = MiddleAircraft;
            float startP = GetWreckPercentile(mAircraft);
            var percentLabel = GetPercentLabel(mAircraft);
            const float DURATION = 1.5f;
            AddAction(new CCSequence(new CCEaseIn(new CCCallFiniteTimeFunc(DURATION, (progress, duration) => { SetWreckPercentile(mAircraft, startP * (1 - progress)); }), 4f),
                                     new CCCallFunc(() => { EndRepair(true); })));
            // let the aircraft visually crumble to pieces
            var totalParts = mAircraft.TotalParts;
            var currentConf = new Dictionary<Part, Tuple<CCPoint, float, float>>();
            foreach (var part in totalParts)
                currentConf[part] = new Tuple<CCPoint, float, float>(part.PositionWorldspace, part.MyRotation, part.GetTotalScale());
            mAircraft.Body.Disassemble();
            mAircraft.Body = null;
            var rng = new Random();
            foreach (var part in totalParts)
            {
                AddChild(part, 99999);
                // place them so that the position is (visually) the same
                var config = currentConf[part];
                part.Position = config.Item1;
                part.Rotation = config.Item2;
                part.Scale = config.Item3;
                // add an action to let it spin around a bit randomly
                part.AddAction(new CCSpawn(new CCEaseOut(new CCMoveTo(DURATION * 2, Constants.RandomPointNear(part.Position, 600f, rng)), 2f),
                                           new CCFadeOut(DURATION * 2)));
                part.AddAction(new CCRepeatForever(new CCRotateBy(1f, (rng.NextBoolean() ? -1 : 1) * (50f + (float)rng.NextDouble() * 100f))));
                part.AddAction(new CCSequence(new CCDelayTime(DURATION * 2 + 0.1f), new CCRemoveSelf()));
            }
        }

        private void EndRepair(bool wasted)
        {
            if (wasted)
                WreckCarousel.RemoveFromCollection((IGameObject)WreckCarousel.MiddleNode);
            State = WreckageState.CAROUSEL;
        }
        protected new void OnTouchesBegan(List<CCTouch> touches, CCEvent touchEvent)
        {

        }
        protected void OnTouchesMoved(List<CCTouch> touches, CCEvent touchEvent)
        {

        }
        protected new void OnTouchesEnded(List<CCTouch> touches, CCEvent touchEvent)
        {

        }
    }
}
