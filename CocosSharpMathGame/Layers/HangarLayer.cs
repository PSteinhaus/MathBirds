using System;
using System.Collections.Generic;
using System.Drawing.Text;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CocosSharp;
using Microsoft.Xna.Framework;
using SkiaSharp;
using Symbolism;
using PCLStorage;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using CSharpMath.Atom.Atoms;
using System.Threading;
using MathNet.Symbolics;
using Xamarin.Essentials;
using Xamarin.Forms;
using System.IO.Compression;

namespace CocosSharpMathGame
{
    public class HangarLayer : MyLayer
    {
        public static HangarLayer GlobalHangarLayer { get; private set; }
        internal static int UnlockedPlaneSlots
        {
            get;
            set;
        } = 2;
        const float TRANSITION_TIME = 0.5f;

        internal enum HangarState
        {
            TRANSITION, HANGAR, WORKSHOP, SCRAPYARD,
            MODIFY_AIRCRAFT, SCRAPYARD_CHALLENGE
        }
        internal HangarState State = HangarState.HANGAR;
        internal CCDrawNode HighDrawNode { get; private protected set; } = new CCDrawNode();
        internal CCDrawNode LowDrawNode { get; private protected set; }  = new CCDrawNode();
        public HangarGUILayer GUILayer { get; set; }
        internal CCNode BGNode { get; private protected set; }
        private CCDrawNode BGDrawNode { get; set; }
        private CCColor4B BGColor { get; set; } = new CCColor4B(50, 50, 50);
        private IGameObject RotationSelectedNode;
        internal List<Aircraft> Aircrafts = new List<Aircraft>();
        internal List<Part> Parts = new List<Part>();
        public HangarLayer(bool keepCurrentMath = false) : base(CCColor4B.Black)
        {
            GlobalHangarLayer = this;
            GUILayer = new HangarGUILayer(this);
            TouchCountSource = GUILayer;
            var challengeModels = MathChallenge.GetAllChallengeModels();
            ScrapyardButtons = new ScrapyardButton[challengeModels.Length];
            NewAircraftButton = new NewAircraftButton(this);
            NewAircraftButton.Visible = false;
            AddChild(NewAircraftButton);
            AddChild(LowDrawNode,  int.MinValue);
            LowDrawNode.BlendFunc = CCBlendFunc.NonPremultiplied;
            AddChild(HighDrawNode, int.MaxValue);
            HighDrawNode.BlendFunc = CCBlendFunc.NonPremultiplied;
            // a double tap starts the transition back the workshop state (of course only coming from the modify-aircraft state)
            DoubleTapEvent += (sender, args) => { if(State == HangarState.MODIFY_AIRCRAFT) StartTransition(HangarState.WORKSHOP); }; 
            BGNode = new CCNode();
            AddChild(BGNode);
            BGDrawNode = new CCDrawNode();
            BGDrawNode.BlendFunc = CCBlendFunc.NonPremultiplied;
            BGNode.AddChild(BGDrawNode);
            DrawBG();
            BGNode.ZOrder = -20;
            //BGNode.Rotation = 45f;
            // listen to the MathChallengeNode class for possible plane slot unlocks
            MathChallengeNode.UnlockedAddSubSlotEvent += UnlockSlot;
            MathChallengeNode.UnlockedMulDivSlotEvent += UnlockSlot;
            MathChallengeNode.UnlockedSolveSlotEvent  += UnlockSlot;
            // add a touch listener
            var touchListener = new CCEventListenerTouchAllAtOnce();
            touchListener.OnTouchesBegan = OnTouchesBegan;
            touchListener.OnTouchesMoved = OnTouchesMoved;
            touchListener.OnTouchesEnded = OnTouchesEnded;
            touchListener.OnTouchesCancelled = OnTouchesEnded;
            AddEventListener(touchListener, this);

            // add a mouse listener
            var mouseListener = new CCEventListenerMouse();
            mouseListener.OnMouseScroll = OnMouseScrollZoom;
            AddEventListener(mouseListener, this);

            CameraSizeHangar = new CCSize(MaxCameraWidth, MaxCameraHeight) / 12;
            CameraPositionHangar = new CCPoint(CameraSizeHangar.Width * 1.0f, CameraSizeHangar.Height * 1.0f);

            // Load the saved state
            LoadFromFile(keepCurrentMath).Wait();

            var rng = new Random();
            for (int i = 0; i < challengeModels.Length; i++)
            {
                var button = challengeModels[i].CreateScrapyardButton();
                ScrapyardButtons[i] = button;
                button.Position = ScrapyardButtonPosition(i);
                button.Visible = false;
                button.RewardEvent += (sender, rewardPart) => { AddPart(rewardPart); };
                // roll the loot chances
                if (challengeModels[i] is AddChallenge && !CrappyPartsCheck())
                {
                    // make sure that the player can always earn enough parts to go again
                    button.LootboxCount = 2;
                }
                else
                    button.LootboxCount = (rng.Next(4) == 0) ? rng.Next(1, 3) : 0;
                AddChild(button, 2);
            }

            CameraSize = CameraSizeHangar;
            CameraPosition = CameraPositionHangar;
        }

        /// <summary>
        /// Unlock a new plane slot (allowing the player to take one more plane into battle).
        /// </summary>
        private void UnlockSlot(object sender, EventArgs empty)
        {
            GUILayer.TakeoffCollectionNode.Columns = ++UnlockedPlaneSlots;
            // show the unlock message
            PopUp.ShowPopUp(GUILayer, PopUp.Enum.TRIGGERED_SLOTUNLOCK);
        }

        /// <summary>
        /// Check whether the player owns a (somewhat) complete potato-set
        /// </summary>
        /// <returns></returns>
        internal bool CrappyPartsCheck()
        {
            var parts = GetParts();
            return parts.OfType<BodyPotato>().Any() &&
                   parts.OfType<RotorPotato>().Any() &&
                   parts.OfType<RudderPotato>().Any() &&
                   parts.OfType<WeaponPotato>().Any() &&
                   parts.OfType<WingPotato>().Any();
        }

        /// <summary>
        /// Check whether the player owns a complete scrap-set.
        /// This check also checks all aircrafts for parts.
        /// </summary>
        /// <returns></returns>
        internal void ScrapPartsCheck(out bool hasBody, out bool hasDoubleWing, out bool hasRotor, out int rudders, out bool hasWeapon)
        {
            var parts = GetParts();
            rudders = 0;
            hasBody       = parts.OfType<BodyScrap>().Any();
            hasDoubleWing = parts.OfType<DoubleWingScrap>().Any();
            hasRotor      = parts.OfType<RotorScrap>().Any();
            rudders       = parts.OfType<RudderScrap>().Count();
            bool hasRudders    = rudders >= 2;
            hasWeapon     = parts.OfType<WeaponScrap>().Any();
            foreach (var aircraft in Aircrafts)
            {
                if (hasBody && hasDoubleWing && hasRotor && hasRudders && hasWeapon) return;
                if (!hasBody       && aircraft.TotalParts.OfType<BodyScrap>().Any()) hasBody = true;
                if (!hasDoubleWing && aircraft.TotalParts.OfType<DoubleWingScrap>().Any()) hasDoubleWing = true;
                if (!hasRotor      && aircraft.TotalParts.OfType<RotorScrap>().Any()) hasRotor = true;
                if (!hasWeapon     && aircraft.TotalParts.OfType<WeaponScrap>().Any()) hasWeapon = true;
                if (!hasRudders    && aircraft.TotalParts.OfType<RudderScrap>().Any())
                {
                    rudders += aircraft.TotalParts.OfType<RudderScrap>().Count();
                    hasRudders = rudders >= 2;
                }
            }
        }

        private CCPoint ScrapyardButtonPosition(int i)
        {
            float bSize = ScrapyardButton.ButtonSize.Width;
            float totalSpacing = (Constants.COCOS_WORLD_WIDTH - 2 * bSize) / 3;
            float spacing = (totalSpacing + bSize) / 2;
            return new CCPoint(i % 2 == 0 ? -spacing : spacing, -((i/2) * spacing*2));
        }

        internal List<Aircraft> TakeoffAircrafts
        {
            get
            {
                return GUILayer.TakeoffCollectionNode.Collection.Cast<Aircraft>().ToList();
            }
        }
        /// <summary>
        /// Saves and removes the HangarLayer and creates and adds the PlayLayer.
        /// </summary>
        internal async void StartGame()
        {
            var playLayer = new PlayLayer();
            var activeAircrafts = new List<Aircraft>(TakeoffAircrafts);
            // remove the active aircrafts from the hangar
            foreach (var aircraft in activeAircrafts)
                RemoveAircraft(aircraft);
            // save the hangar
            await SaveToFile();
            // unsubscribe from all events
            MathChallengeNode.UnlockedAddSubSlotEvent -= UnlockSlot;
            MathChallengeNode.UnlockedMulDivSlotEvent -= UnlockSlot;
            MathChallengeNode.UnlockedSolveSlotEvent  -= UnlockSlot;

            HangarLayer.GlobalHangarLayer = null;
            TransitionFadingFromTo(this.GUILayer, playLayer.GUILayer, this, playLayer, 2f);
            //var parent = Parent;
            //RemoveAllListeners();
            //GUILayer.RemoveAllListeners();
            //Parent.RemoveChild(GUILayer);
            //Parent.RemoveChild(this);
            // save the hangar (concurrently)
            //var saveTask = SaveToFile();
            //parent.AddChild(playLayer.GUILayer);
            //parent.AddChild(playLayer, zOrder: int.MinValue);
            // place the aircrafts and add them as children
            playLayer.AddAction(new CCCallFunc(() => playLayer.InitPlayerAircrafts(activeAircrafts)));
        }

        protected override void AddedToScene()
        {
            base.AddedToScene();
            // if there are no aircrafts and no scrap parts add a weak scrap-aircraft
            ScrapPartsCheck(out bool hasBody, out bool hasDoubleWing, out bool hasRotor, out int rudders, out bool hasWeapon);
            if (!Aircrafts.Any() && !hasBody && !hasDoubleWing && !hasRotor && rudders == 0 && !hasWeapon)
                AddAircraft(Aircraft.CreateScrapAircraft(), CCPoint.Zero);
            else  // if there are aircrafts add all missing scrap parts
            {
                if (!hasBody)       AddPart(new BodyScrap());
                if (!hasDoubleWing) AddPart(new DoubleWingScrap());
                if (!hasRotor)      AddPart(new RotorScrap());
                if (!hasWeapon)     AddPart(new WeaponScrap());
                for (; rudders < 2; rudders++)  AddPart(new RudderScrap());
            }
            CalcBoundaries();
            CameraPosition = CameraPosition;
            UpdateCamera();
            CreateActions();
            // show the welcome popup
            if (!PopUp.TriggeredWelcome)
                PopUp.ShowPopUp(GUILayer, PopUp.Enum.TRIGGERED_WELCOME);
        }

        public override void Update(float dt)
        {
            base.Update(dt);
            if (State == HangarState.TRANSITION)
            {
                TimeInTransition += dt;
                MoveCameraInTransition();
            }
        }

        internal void IncreaseBGAlpha()
        {
            BGColor = new CCColor4B(BGColor.R, BGColor.G, BGColor.B, (byte)(BGColor.A + 1 <= byte.MaxValue ? BGColor.A + 1 : byte.MaxValue));
            DrawBG();
        }

        internal void DecreaseBGAlpha()
        {
            BGColor = new CCColor4B(BGColor.R, BGColor.G, BGColor.B, (byte)(BGColor.A - 1 >= 0 ? BGColor.A - 1 : 0));
            DrawBG();
        }

        internal void DrawBG()
        {
            BGDrawNode.Clear();
            const float bgSize = 6000f;
            for (int i = -40; i < 40; i++)
            {
                BGDrawNode.DrawLine(new CCPoint(i * bgSize / 40, -bgSize), new CCPoint(i * bgSize / 40, bgSize), 4f, BGColor);
                BGDrawNode.DrawLine(new CCPoint(-bgSize, i * bgSize / 40), new CCPoint(bgSize, i * bgSize / 40), 4f, BGColor);
            }
        }

        private CCAction CreateUIFadeOutAndDisableAction(ScrapyardButton scrapyardButton)
        {
            var action = new CCSpawn( new CCSequence(new CCCallFunc(() => { scrapyardButton.Pressable = false; }), FadeOut, new CCCallFunc(() => { scrapyardButton.Visible = false; })),
                                      new CCCallFiniteTimeFunc(TRANSITION_TIME, (prog, duration) => { scrapyardButton.DrawNodeAlpha = 1 - prog; scrapyardButton.UpdateDrawNode(true); }));
            action.Tag = FadeActionTag;
            return action;
        }
        private CCAction CreateUIFadeInAndEnableAction(ScrapyardButton scrapyardButton)
        {
            var action = new CCSpawn( new CCSequence(new CCCallFunc(() => { scrapyardButton.Visible = true; }), FadeIn, new CCCallFunc(() => { if (!scrapyardButton.ChallengeModel.Locked) scrapyardButton.Pressable = true; })),
                                      new CCCallFiniteTimeFunc(TRANSITION_TIME, (prog, duration) => { scrapyardButton.DrawNodeAlpha = prog; scrapyardButton.UpdateDrawNode(true); }));
            action.Tag = FadeActionTag;
            return action;
        }
        private CCAction CreateUIFadeOutAndDisableAction(UIElement uIElement)
        {
            var action = new CCSequence(new CCCallFunc(() => { uIElement.Pressable = false; }), FadeOut, new CCCallFunc(() => { uIElement.Visible = false; }));
            action.Tag = FadeActionTag;
            return action;
        }
        private CCAction CreateUIFadeInAndEnableAction(UIElement uIElement)
        {
            var action = new CCSequence(new CCCallFunc(() => { uIElement.Visible = true; }), FadeIn, new CCCallFunc(() => { uIElement.Pressable = true; }));
            action.Tag = FadeActionTag;
            return action;
        }
        private CCAction CreateUIFadeOutAndDisableAction(UIElementNode uIElement)
        {
            var action = new CCSequence(new CCCallFunc(() => { uIElement.Pressable = false; }), FadeOut, new CCCallFunc(() => { uIElement.Visible = false; }));
            action.Tag = FadeActionTag;
            return action;
        }
        private CCAction CreateUIFadeInAndEnableAction(UIElementNode uIElement)
        {
            var action = new CCSequence(new CCCallFunc(() => { uIElement.Visible = true; }), FadeIn, new CCCallFunc(() => { uIElement.Pressable = true; }));
            action.Tag = FadeActionTag;
            return action;
        }
        private void CreateActions()
        {
            const float easeRate = 0.6f;
            TakeoffNodeToHangar = new CCSequence(new CCCallFunc(() => { GUILayer.TakeoffNode.Visible = true; }), new CCEaseIn(new CCMoveTo(TRANSITION_TIME, new CCPoint(0, 8f)), easeRate) );
            TakeoffNodeLeave    = new CCSequence(new CCEaseIn(new CCMoveTo(TRANSITION_TIME, new CCPoint(0, -GUILayer.TakeoffNode.BoundingBoxTransformedToWorld.Size.Height)), easeRate), new CCCallFunc(() => { GUILayer.TakeoffNode.Visible = false; }));
            BGFadeOut           = new CCRepeat(new CCSequence(new CCCallFunc(DecreaseBGAlpha), new CCDelayTime(TRANSITION_TIME / 255)), 255);
            BGFadeIn            = new CCRepeat(new CCSequence(new CCCallFunc(IncreaseBGAlpha), new CCDelayTime(TRANSITION_TIME / 255)), 255);
            FadeOut             = new CCEaseIn(new CCFadeTo(TRANSITION_TIME, 0),   easeRate);
            FadeIn              = new CCEaseIn(new CCFadeTo(TRANSITION_TIME, 255), easeRate);
            RemoveCarousel      = new CCSequence(new CCEaseIn(new CCMoveBy(TRANSITION_TIME, new CCPoint(0, GUILayer.HangarOptionCarousel.BoundingBoxTransformedToWorld.Size.Height + 0f)), easeRate*0.7f), new CCCallFunc(() => { GUILayer.HangarOptionCarousel.Visible = false; }));
            AddCarousel         = new CCSequence(new CCCallFunc(() => { GUILayer.HangarOptionCarousel.Visible = true; }), new CCEaseIn(new CCMoveTo(TRANSITION_TIME, new CCPoint(0, GUILayer.VisibleBoundsWorldspace.MaxY)), easeRate*0.7f));
            RemovePartCarousel  = new CCSequence(new CCEaseIn(new CCMoveBy(TRANSITION_TIME, new CCPoint(0, GUILayer.PartCarousel.ContentSize.Height)), easeRate * 0.7f), new CCCallFunc(() => { GUILayer.PartCarousel.Visible = false; }));
            AddPartCarousel     = new CCSequence(new CCDelayTime(TRANSITION_TIME), new CCCallFunc(() => { GUILayer.PartCarousel.Visible = true; }), new CCEaseIn(new CCMoveTo(TRANSITION_TIME, new CCPoint(0, GUILayer.VisibleBoundsWorldspace.MaxY)), easeRate * 0.7f));
            AddNewAircraftButton = new CCSequence(new CCCallFunc(() => { NewAircraftButton.Visible = true; }), FadeIn, new CCCallFunc(() => { NewAircraftButton.Pressable = true; }));
            RemoveNewAircraftButton = new CCSequence(new CCCallFunc(() => { NewAircraftButton.Pressable = false; }), FadeOut, new CCCallFunc(() => { NewAircraftButton.Visible = false; }));
            AddGOButton         = new CCSequence(new CCCallFunc(() => { GUILayer.GOButton.Visible = true; }), new CCEaseOut(new CCMoveTo(TRANSITION_TIME, GUILayer.GOButtonInPosition), 20f));
            RemoveGOButton      = new CCSequence(new CCEaseIn(new CCMoveTo(TRANSITION_TIME, GUILayer.GOButtonOutPosition), 10f), new CCCallFunc(() => { GUILayer.GOButton.Visible = false; }));
        }

        internal float TimeInTransition = 0f;

        internal CCAction AddGOButton;
        internal CCAction RemoveGOButton;
        private CCAction AddNewAircraftButton;
        private CCAction RemoveNewAircraftButton;
        private CCAction AddPartCarousel;
        private CCAction RemovePartCarousel;
        private CCAction AddCarousel;
        private CCAction RemoveCarousel;
        private CCFiniteTimeAction FadeOut;
        private CCFiniteTimeAction FadeIn;
        private CCAction TransistionAction;
        private CCAction TakeoffNodeToHangar;
        private CCAction TakeoffNodeLeave;
        private CCAction BGFadeOut;
        private CCAction BGFadeIn;
        private const int MoveAircraftTag   = 73828192;
        private const int ScaleAircraftTag  = 73828193;
        private const int RotateAircraftTag = 73828194;
        private const int MoveActionTag = 99764356;
        private const int FadeActionTag = 99764357;

        private CCPoint CameraPositionHangar;
        private CCSize  CameraSizeHangar;

        private CCPoint LastCameraPosition;
        private CCPoint NextCameraPosition;
        private CCSize  LastCameraSize;
        private CCSize  NextCameraSize;
        internal float TransitionTime { get; private protected set; }
        internal float CameraMoveTime { get; private protected set; }
        private void MoveCameraInTransition()
        {
            float ratio = Constants.Clamp(TimeInTransition / CameraMoveTime, 0, 1);
            CameraPosition = LastCameraPosition + (NextCameraPosition - LastCameraPosition) * ratio;
            CameraSize = new CCSize(LastCameraSize.Width  + (NextCameraSize.Width  - LastCameraSize.Width)  * ratio,
                                    LastCameraSize.Height + (NextCameraSize.Height - LastCameraSize.Height) * ratio);
            UpdateCamera();
        }
        internal void MiddleNodeChanged(object sender, EventArgs args)
        {
            var state = HangarState.HANGAR;
            // get the state to go to
            var carousel = (Carousel)sender;
            var middle = carousel.MiddleNode;
            if (middle == GUILayer.HangarOptionHangar)
            {
                state = HangarState.HANGAR;
            }
            else if (middle == GUILayer.HangarOptionWorkshop)
            {
                state = HangarState.WORKSHOP;
            }
            else if (middle == GUILayer.HangarOptionScrapyard)
            {
                state = HangarState.SCRAPYARD;
            }
            StartTransition(state);
        }

        private Dictionary<Aircraft, CCPoint> HangarPositions = new Dictionary<Aircraft, CCPoint>();
        private Dictionary<Aircraft, float>   HangarRotations = new Dictionary<Aircraft, float>();
        private const float WorkshopBoxBorderY = 120f;
        internal NewAircraftButton NewAircraftButton { get; private set; }
        private CCPoint WorkshopPosition(Aircraft aircraft)
        {
            CCPoint pos = CCPoint.Zero;
            // the first position is taken by the button for creating a new aircraft
            pos -= new CCPoint(0, NewAircraftButton.BoundingBoxTransformedToWorld.Size.Height + WorkshopBoxBorderY);
            Aircraft lastAircraft = null;
            foreach (var aircr in Aircrafts)
            {
                if (lastAircraft != null)
                {
                    float scaleLast = WorkshopScale(lastAircraft);
                    pos -= new CCPoint(0, lastAircraft.ContentSize.Height * scaleLast / 2);
                    float scale = WorkshopScale(aircr);
                    pos -= new CCPoint(0, aircr.ContentSize.Height * scale / 2 + WorkshopBoxBorderY);
                }
                if (aircr == aircraft)
                    return pos;
                lastAircraft = aircr;
            }
            return CCPoint.Zero;
        }
        private readonly CCPoint CameraPositionWorkshop  = new CCPoint(-Constants.COCOS_WORLD_WIDTH / 2, -Constants.COCOS_WORLD_HEIGHT * 0.75f);
        private readonly CCPoint CameraPositionScrapyard = new CCPoint(-Constants.COCOS_WORLD_WIDTH / 2, -Constants.COCOS_WORLD_HEIGHT * 0.75f);
        internal void StartTransition(HangarState state)
        {
            var oldState = State;
            TransitionTime = TRANSITION_TIME; // usually the transition takes TRANSITION_TIME seconds
            CameraMoveTime = TRANSITION_TIME; // the camera move (if there is one) as well
            LastCameraPosition = CameraPosition;
            LastCameraSize = CameraSize;
            if (State == HangarState.HANGAR)
            {
                CameraPositionHangar = CameraPosition;
                CameraSizeHangar = CameraSize;
            }
            State = HangarState.TRANSITION;
            CalcBoundaries(); // allow the camera to move freely
            // stop all current transition actions
            StopAllTransitionActions();
            // disable the touchBegan listeners for all gui elements (exept the carousel usually)
            GUILayer.DisableTouchBegan(state == HangarState.MODIFY_AIRCRAFT || oldState == HangarState.MODIFY_AIRCRAFT || state == HangarState.SCRAPYARD_CHALLENGE || oldState == HangarState.SCRAPYARD_CHALLENGE);
            // state leave actions
            if (state != HangarState.HANGAR)
            {
                GUILayer.GOButton.AddAction(RemoveGOButton);
                GUILayer.TakeoffNode.AddAction(TakeoffNodeLeave);
                BGNode.AddAction(BGFadeOut);
            }
            if (state != HangarState.WORKSHOP)
            {
                NewAircraftButton.AddAction(RemoveNewAircraftButton);
            }
            if (state != HangarState.SCRAPYARD)
            {
                // fade out scrapyard buttons and make them invisible
                foreach (var button in ScrapyardButtons)
                {
                    button.AddAction(CreateUIFadeOutAndDisableAction(button));
                }
            }
            // state enter actions
            if (state == HangarState.HANGAR)
            {
                GUILayer.TakeoffNode.AddAction(TakeoffNodeToHangar);
                // if the takeoffnode holds at least one aircraft add the GO-button
                if (GUILayer.TakeoffCollectionNode.Collection.Any())
                    GUILayer.GOButton.AddAction(AddGOButton);
                BGNode.AddAction(BGFadeIn);
                foreach (var aircraft in Aircrafts)
                {
                    if (aircraft.Parent != this) continue;
                    var moveAction = new CCMoveTo(TRANSITION_TIME, HangarPositions[aircraft]);
                    moveAction.Tag = MoveAircraftTag;
                    aircraft.AddAction(moveAction);
                    var scaleAction = new CCScaleTo(TRANSITION_TIME, Constants.STANDARD_SCALE);
                    scaleAction.Tag = ScaleAircraftTag;
                    aircraft.AddAction(scaleAction);
                    var rotateAction = new CCMyRotateTo(TRANSITION_TIME, HangarRotations[aircraft]);
                    rotateAction.Tag = RotateAircraftTag;
                    aircraft.AddAction(rotateAction);
                }
                NextCameraPosition = CameraPositionHangar;
                NextCameraSize = CameraSizeHangar;
            }
            else if (state == HangarState.WORKSHOP)
            {
                NextCameraSize = new CCSize(Constants.COCOS_WORLD_WIDTH, Constants.COCOS_WORLD_HEIGHT);
                if (oldState != HangarState.MODIFY_AIRCRAFT)
                {
                    // check if this is the first time the player enters the assembly and show the popup if necessary
                    if (!PopUp.TriggeredAssembly)
                    {
                        GUILayer.HangarOptionCarousel.Pressed = false;
                        GUILayer.HangarOptionCarousel.StopCurrentScrolling();
                        PopUp.ShowPopUp(GUILayer, PopUp.Enum.TRIGGERED_ASSEMBLY);
                    }
                    NextCameraPosition = CameraPositionWorkshop;
                }
                else
                {
                    // the transition starts off at MODIFY_AIRCRAFT, so (of course) do a different kind of transition
                    // the carousel was removed so add it again
                    GUILayer.HangarOptionCarousel.AddAction(AddCarousel);
                    // remove the part carousel
                    GUILayer.PartCarousel.AddAction(RemovePartCarousel);
                    // stop drawing the connections
                    HighDrawNode.Clear();
                    LowDrawNode.Clear();
                    // if the ModifiedAircraft has no Body it needs to be removed
                    if (ModifiedAircraft.Body == null)
                    {
                        RemoveAircraft(ModifiedAircraft);
                    }
                    // the aircraft has (probably) been modified, so it should run through the placement algorithm once more
                    var currentPos = ModifiedAircraft.Position;
                    PlaceAircraft(ModifiedAircraft, HangarPositions[ModifiedAircraft]);
                    ModifiedAircraft.Position = currentPos;
                    // get the standard configuration positions
                    ModifiedAircraft.InWorkshopConfiguration = false;
                    var totalParts = ModifiedAircraft.TotalParts;
                    CCPoint[] standardConfigurationsPositions = new CCPoint[totalParts.Count()];
                    for (int i = 0; i < totalParts.Count(); i++)
                        standardConfigurationsPositions[i] = totalParts.ElementAt(i).Position;
                    ModifiedAircraft.InWorkshopConfiguration = true;
                    // now move the parts slowly to these positions
                    for (int i = 0; i < totalParts.Count(); i++)
                        totalParts.ElementAt(i).AddAction(new CCEaseIn(new CCMoveTo(TRANSITION_TIME, standardConfigurationsPositions[i]), 2.6f));
                    ModifiedAircraft.AddAction(new CCSequence(new CCDelayTime(TRANSITION_TIME), new CCCallFunc(() => { ModifiedAircraft.InWorkshopConfiguration = false; ModifiedAircraft = null; })));
                    // focus on the selected aircraft
                    NextCameraPosition = new CCPoint(CameraPositionWorkshop.X, WorkshopPosition(ModifiedAircraft).Y - NextCameraSize.Height / 2);
                }
                foreach (var aircraft in Aircrafts)
                {
                    if (aircraft.Parent != this) continue;
                    aircraft.AddAction(FadeIn);
                    var moveAction = new CCMoveTo(TRANSITION_TIME, WorkshopPosition(aircraft));
                    moveAction.Tag = MoveAircraftTag;
                    aircraft.AddAction(moveAction);
                    var scaleAction = new CCScaleTo(TRANSITION_TIME, WorkshopScale(aircraft));
                    scaleAction.Tag = ScaleAircraftTag;
                    aircraft.AddAction(scaleAction);
                    var rotateAction = new CCMyRotateTo(TRANSITION_TIME, 0f);
                    rotateAction.Tag = RotateAircraftTag;
                    aircraft.AddAction(rotateAction);
                }
                NewAircraftButton.AddAction(AddNewAircraftButton);
            }
            else if (state == HangarState.MODIFY_AIRCRAFT)
            {
                TransitionTime = TRANSITION_TIME * 2 + 0.0001f;
                // disable and remove the carousel
                GUILayer.HangarOptionCarousel.AddAction(RemoveCarousel);
                // fade out all aircrafts but the selected one
                foreach (var aircraft in Aircrafts)
                {
                    if (aircraft.Parent != this || aircraft == ModifiedAircraft) continue;
                    aircraft.AddAction(FadeOut);
                }
                // add the part carousel
                GUILayer.PartCarousel.AddAction(AddPartCarousel);
                // get the workshop configuration positions
                ModifiedAircraft.InWorkshopConfiguration = true;
                var totalParts = ModifiedAircraft.TotalParts;
                // and the workshop configuration size
                float newCamWidth = ModifyAircraftWidth();
                if (totalParts != null)
                {
                    CCPoint[] workshopConfigurationsPositions = new CCPoint[totalParts.Count()];
                    for (int i = 0; i < totalParts.Count(); i++)
                        workshopConfigurationsPositions[i] = totalParts.ElementAt(i).Position;
                    ModifiedAircraft.InWorkshopConfiguration = false;
                    // now move the parts slowly to these positions
                    for (int i = 0; i < totalParts.Count(); i++)
                        totalParts.ElementAt(i).AddAction(new CCSequence(new CCDelayTime(TRANSITION_TIME), new CCEaseIn(new CCMoveTo(TRANSITION_TIME, workshopConfigurationsPositions[i]), 2.6f)));
                }
                ModifiedAircraft.InWorkshopConfiguration = false;
                ModifiedAircraft.AddAction(new CCSequence(new CCDelayTime(TRANSITION_TIME*2), new CCCallFunc(() => { ModifiedAircraft.InWorkshopConfiguration = true; })));
                NextCameraSize = new CCSize(newCamWidth, CameraSize.Height * newCamWidth / CameraSize.Width);
                // focus on the selected aircraft
                // (the last summand is there to take the space into account which is taken by the part carousel)
                NextCameraPosition = ModifiedAircraft.Position - ((CCPoint)NextCameraSize / 2) + new CCPoint(0, NextCameraSize.Height / 6);
                // this special transition takes twice the time
            }
            else if (state == HangarState.SCRAPYARD)
            {
                NextCameraPosition = CameraPositionScrapyard;
                NextCameraSize = new CCSize(Constants.COCOS_WORLD_WIDTH, Constants.COCOS_WORLD_HEIGHT);
                if (oldState == HangarState.SCRAPYARD_CHALLENGE)
                {
                    // the transition starts off at SCRAPYARD_CHALLENGE, so do some more
                    // the carousel was removed so add it again
                    GUILayer.HangarOptionCarousel.AddAction(AddCarousel);
                    // remove the challenge node
                    GUILayer.ChallengeNode = CurrentScrapyardButton.CurrentMathChallengeNode;
                    var challenge = GUILayer.ChallengeNode;
                    challenge.Pressable = false;
                    var moveAction = new CCEaseIn(new CCMoveTo(TransitionTime, new CCPoint(0, -challenge.BoundingBoxTransformedToWorld.Size.Height - 1f)), 0.6f);
                    challenge.AddAction(moveAction);
                    // at the end of the transition make the current button pressable again
                    var button = CurrentScrapyardButton;
                    button.AddAction(new CCSequence(new CCDelayTime(TransitionTime), new CCCallFunc(() => { button.Pressable = true; })));
                    // make visible and fade in the other scrapyard buttons
                    foreach (var otherButton in ScrapyardButtons)
                    {
                        if (otherButton != button)
                            otherButton.AddAction(CreateUIFadeInAndEnableAction(otherButton));
                    }
                }
                else
                {
                    if (!PopUp.TriggeredScrapyard)
                    {
                        GUILayer.HangarOptionCarousel.Pressed = false;
                        GUILayer.HangarOptionCarousel.StopCurrentScrolling();
                        PopUp.ShowPopUp(GUILayer, PopUp.Enum.TRIGGERED_SCRAPYARD);
                    }
                    // move all aircrafts away
                    foreach (var aircraft in Aircrafts)
                    {
                        if (aircraft.Parent != this) continue;
                        MoveAircraftOutOfView(aircraft, TransitionTime);
                    }
                    // make visible and fade in the scrapyard buttons
                    foreach (var button in ScrapyardButtons)
                    {
                        button.AddAction(CreateUIFadeInAndEnableAction(button));
                    }
                }
            }
            else if (state == HangarState.SCRAPYARD_CHALLENGE)
            {
                // disable and remove the carousel
                GUILayer.HangarOptionCarousel.AddAction(RemoveCarousel);
                // camera
                var nextRect = ScrapyardButtonCameraRect(CurrentScrapyardButton);
                NextCameraPosition = nextRect.Origin;
                NextCameraSize = nextRect.Size;
                // stop the current button from going invisible
                CurrentScrapyardButton.StopAction(FadeActionTag);
                // also make it unpressable since there is no reason to press it again right now
                CurrentScrapyardButton.Pressable = false;
                // get and show the math challenge
                if (CurrentScrapyardButton.CurrentMathChallengeNode == null)
                {
                    CurrentScrapyardButton.CreateNextChallenge();
                    CurrentScrapyardButton.CurrentMathChallengeNode.AnswerChosenEvent += ScrapyardChallengeCallback;
                }
                else
                {
                    CurrentScrapyardButton.CreateSameChallenge();
                    CurrentScrapyardButton.CurrentMathChallengeNode.AnswerChosenEvent += ScrapyardChallengeCallback;
                }
                GUILayer.ChallengeNode = CurrentScrapyardButton.CurrentMathChallengeNode;
                GUILayer.ChallengeNode.Pressable = true;
                GUILayer.ChallengeNode.Position = new CCPoint(0, -GUILayer.ChallengeNode.BoundingBoxTransformedToWorld.Size.Height - 1f);
                var moveAction = new CCEaseIn(new CCMoveTo(TransitionTime, CCPoint.Zero), 0.6f);
                GUILayer.ChallengeNode.AddAction(moveAction);
            }
            // start transition actions
            TransistionAction = new CCSequence(new CCDelayTime(TransitionTime), new CCCallFunc(() => FinalizeTransition(state)));
            AddAction(TransistionAction);
        }

        private void ScrapyardChallengeCallback(object sender, bool isSolution)
        {
            // check if the player chose the right answer
            if (isSolution)
            {
                // Advance the lootbox-meter
                CurrentScrapyardButton.ChallengeSolved();
            }
            else
            {
                // Reset the lootbox-meter
                CurrentScrapyardButton.ChallengeFailed();
                GUILayer.AddScreenShake(38f, 38f);
            }
            // generate and show the next challenge
            CurrentScrapyardButton.CreateNextChallenge(CurrentScrapyardButton.CurrentMathChallengeNode.MultiplVisible);
            GUILayer.ChallengeNode = CurrentScrapyardButton.CurrentMathChallengeNode;
            GUILayer.ChallengeNode.AnswerChosenEvent += ScrapyardChallengeCallback;
        }

        private void MoveAircraftOutOfView(Aircraft aircraft, float duration)
        {
            var bounds = VisibleBoundsWorldspace;
            var vec = aircraft.PositionWorldspace - bounds.Center;
            if (vec.Equals(CCPoint.Zero)) vec = new CCPoint(0, 1);
            vec = CCPoint.Normalize(vec);
            vec = vec * bounds.Size.Height * 4;
            // move it
            var moveAction = new CCMoveBy(duration, aircraft.Position + vec);
            moveAction.Tag = MoveAircraftTag;
            // just to be sure stop any current move action
            aircraft.StopAction(MoveAircraftTag);
            aircraft.AddAction(moveAction);
        }

        /// <summary>
        /// Add a part to the collection of parts owned by the player
        /// </summary>
        /// <param name="part"></param>
        internal void AddPart(Part part)
        {
            // find the correct collection
            foreach (var node in GUILayer.PartCarousel.CollectionNode.Children)
            {
                var pNode = (PartCarouselNode)node;
                if (part.Types.Contains(pNode.PartType))
                {
                    pNode.AddPart(part);
                    break;
                }
            }
        }

        internal List<Part> GetParts()
        {
            // collect the parts
            List<Part> parts = new List<Part>();
            foreach (var node in GUILayer.PartCarousel.CollectionNode.Children)
            {
                var pNode = (PartCarouselNode)node;
                foreach(var p in pNode.GetParts())
                    parts.Add(p);
            }
            return parts;
        }

        internal float ModifyAircraftWidth()
        {
            var totalParts = ModifiedAircraft.TotalParts;
            float xMin = float.PositiveInfinity;
            float xMax = float.NegativeInfinity;
            if (totalParts.Any())
            {
                foreach (var part in totalParts)
                {
                    CCRect box = part.BoundingBoxTransformedToWorld;
                    if (box.MinX < xMin) xMin = box.MinX;
                    if (box.MaxX > xMax) xMax = box.MaxX;
                }
                return Math.Max(Math.Abs(xMin), Math.Abs(xMax)) * 2 + 400f - Math.Abs(Math.Abs(xMin) - Math.Abs(xMax)) * 3f; // the last value is an additional border to the edge of the screen
            }
            else
                return 600f;
        }

        internal Aircraft ModifiedAircraft { get; set; }

        private float WorkshopScale(Aircraft aircraft)
        {
            float scale = Constants.STANDARD_SCALE;
            float maxWidth = Constants.COCOS_WORLD_WIDTH * 0.8f;
            if (aircraft.ContentSize.Width * scale > maxWidth)
                scale = maxWidth / aircraft.ContentSize.Width;
            return scale;
        }

        private float WorkshopHeight()
        {
            if (!Aircrafts.Any())
                return 0f;
            return -WorkshopPosition(Aircrafts.Last()).Y;
        }

        private void StopAllTransitionActions()
        {
            // stop all actions that could be happening right now
            if (TransistionAction != null) StopAction(TransistionAction.Tag);
            GUILayer.HangarOptionCarousel.StopAction(AddCarousel.Tag);
            GUILayer.HangarOptionCarousel.StopAction(RemoveCarousel.Tag);
            GUILayer.TakeoffNode.StopAction(TakeoffNodeToHangar.Tag);
            GUILayer.TakeoffNode.StopAction(TakeoffNodeLeave.Tag);
            GUILayer.PartCarousel.StopAction(AddPartCarousel.Tag);
            GUILayer.PartCarousel.StopAction(RemovePartCarousel.Tag);
            GUILayer.GOButton.StopAction(AddGOButton.Tag);
            GUILayer.GOButton.StopAction(RemoveGOButton.Tag);
            BGNode.StopAction(BGFadeOut.Tag);
            BGNode.StopAction(BGFadeIn.Tag);
            NewAircraftButton.StopAction(AddNewAircraftButton.Tag);
            NewAircraftButton.StopAction(RemoveNewAircraftButton.Tag);
            foreach (var aircraft in Aircrafts)
            {
                aircraft.StopAction(MoveAircraftTag);
                aircraft.StopAction(ScaleAircraftTag);
                aircraft.StopAction(RotateAircraftTag);
                aircraft.StopAction(FadeOut.Tag);
                aircraft.StopAction(FadeIn.Tag);
            }
            foreach (var scrapyardButton in ScrapyardButtons)
            {
                scrapyardButton.StopAction(FadeActionTag);
            }
            TimeInTransition = 0; // reset transition time
        }

        private PartMount ClosestMount { get; set; } = null;
        internal void DrawInModifyAircraftState()
        {
            HighDrawNode.Clear();
            LowDrawNode.Clear();
            // draw the connections between mount points and mounted parts
            const float LINE_WIDTH = 4f;
            const float RADIUS = 5f;
            CCColor4B LINE_COLOR = new CCColor4B(50, 50, 50);
            CCColor4B EMPTY_MOUNT_COLOR = new CCColor4B(150, 150, 150);
            void DrawMountCircle(CCPoint pos, CCColor4B color, CCDrawNode drawNode)
            {
                drawNode.DrawSolidCircle(pos, RADIUS + LINE_WIDTH, color);
                drawNode.DrawSolidCircle(pos, RADIUS, CCColor4B.Black);
            }
            void DrawBodyMount(CCPoint pos, CCColor4B color, CCDrawNode drawNode)
            {
                drawNode.DrawLine(pos - new CCPoint(RADIUS * 3, 0), pos + new CCPoint(RADIUS * 3, 0), LINE_WIDTH, color, CCLineCap.Square);
                drawNode.DrawLine(pos - new CCPoint(0, RADIUS * 3), pos + new CCPoint(0, RADIUS * 3), LINE_WIDTH, color, CCLineCap.Square);
                drawNode.DrawSolidCircle(pos, RADIUS * 2 + LINE_WIDTH, color);
                drawNode.DrawSolidCircle(pos, RADIUS * 2, CCColor4B.Black);
            }
            void DrawMountLine(CCPoint start, CCPoint end, CCColor4B color, CCDrawNode drawNode)
            {
                // first draw the diagonal segment
                CCPoint diff = end - start;
                CCPoint middle = CCPoint.Zero;
                if (Math.Abs(diff.X) > Math.Abs(diff.Y))
                    middle = new CCPoint(start.X + Math.Sign(diff.X) * Math.Abs(diff.Y), end.Y);
                else
                    middle = new CCPoint(end.X, start.Y + Math.Sign(diff.Y) * Math.Abs(diff.X));
                drawNode.DrawLine(start, middle, LINE_WIDTH, color, CCLineCap.Round);
                drawNode.DrawLine(middle, end, LINE_WIDTH, color, CCLineCap.Round);
            }
            var closestMountBefore = ClosestMount;
            ClosestMount = null;
            float minDistance = float.PositiveInfinity;
            foreach (var part in ModifiedAircraft.TotalParts)
            {
                foreach (var mountPoint in part.PartMounts)
                {
                    var mountedPart = mountPoint.MountedPart;
                    if (mountedPart != null)
                    {
                    }
                    else if (mountPoint.Available)
                    {
                        // find the closest possible mount point
                        if (GUILayer.DragAndDropObject != null && mountPoint.CanMount((Part)GUILayer.DragAndDropObject) && CCPoint.IsNear(mountPoint.PositionModifyAircraft, GUICoordinatesToHangar(((Part)GUILayer.DragAndDropObject).PositionWorldspace), HangarGUILayer.MOUNT_DISTANCE))
                        {
                            float distance = CCPoint.Distance(mountPoint.PositionModifyAircraft, GUICoordinatesToHangar(((Part)GUILayer.DragAndDropObject).PositionWorldspace));
                            if (distance < minDistance)
                            {
                                minDistance = distance;
                                ClosestMount = mountPoint;
                            }
                        }
                    }
                }
            }
            foreach (var part in ModifiedAircraft.TotalParts)
            {
                foreach (var mountPoint in part.PartMounts)
                {
                    var mountedPart = mountPoint.MountedPart;
                    if (mountedPart != null)
                    {
                        DrawMountLine(mountPoint.PositionWorldspace, mountedPart.PositionWorldspace, LINE_COLOR, LowDrawNode);
                        //DrawMountCircle(mountedPart.PositionWorldspace, CCColor4B.White, LowDrawNode);
                    }
                    else if (mountPoint.Available)
                    {
                        // use different colors, depending on whether the mount point is fitting for the part type of the part carousel current middle node
                        // and also depending on whether there is a drag-and-drop part that would be mounted here if dropped now
                        CCColor4B color = LINE_COLOR;
                        if ((GUILayer.DragAndDropObject == null && mountPoint.AllowedTypes.Contains(((PartCarouselNode)GUILayer.PartCarousel.MiddleNode).PartType)) ||
                            (GUILayer.DragAndDropObject != null && mountPoint.CanMount((Part)GUILayer.DragAndDropObject)))
                            color = EMPTY_MOUNT_COLOR;
                        if (mountPoint == ClosestMount)
                            color = CCColor4B.White;
                        // draw a line to the where the mount point is visualized
                        DrawMountLine(mountPoint.PositionWorldspace, mountPoint.PositionModifyAircraft, color, LowDrawNode);
                        DrawMountCircle(mountPoint.PositionModifyAircraft, color, HighDrawNode);
                    }
                }
            }
            if (ClosestMount != closestMountBefore && ClosestMount != null && Constants.oS != Constants.OS.WINDOWS)
                Vibration.Vibrate(20);
            if (!ModifiedAircraft.TotalParts.Any())
            {
                // if the aircraft has no body...
                CCColor4B color = LINE_COLOR;
                if ((GUILayer.DragAndDropObject == null && ((PartCarouselNode)GUILayer.PartCarousel.MiddleNode).PartType == Part.Type.BODY) ||
                    (GUILayer.DragAndDropObject != null && ((Part)GUILayer.DragAndDropObject).Types.Contains(Part.Type.BODY)))
                    color = EMPTY_MOUNT_COLOR;
                if (GUILayer.DragAndDropObject != null && ((Part)GUILayer.DragAndDropObject).Types.Contains(Part.Type.BODY) && CCPoint.IsNear(ModifiedAircraft.PositionWorldspace, GUICoordinatesToHangar(((Part)GUILayer.DragAndDropObject).PositionWorldspace), HangarGUILayer.MOUNT_DISTANCE))
                    color = CCColor4B.White;
                DrawBodyMount(ModifiedAircraft.PositionWorldspace, color, HighDrawNode);
            }
        }
        private void FinalizeTransition(HangarState state)
        {
            State = state;
            switch (State)
            {
                case HangarState.MODIFY_AIRCRAFT:
                    {
                        DrawInModifyAircraftState();
                    }
                    break;
            }
            CameraPosition = NextCameraPosition;
            CameraSize = NextCameraSize;
            UpdateCamera();
            CalcBoundaries();
            GUILayer.EnableTouchBegan(state);
        }
        internal CCPoint GUICoordinatesToHangar(CCPoint pointInGUICoord)
        {
            return CameraPosition + pointInGUICoord * GUIScaleToHangar();
        }

        internal float GUIScaleToHangar()
        {
            return VisibleBoundsWorldspace.Size.Width / GUILayer.VisibleBoundsWorldspace.Size.Width;
        }

        internal void ReceiveAircraftFromCollection(object sender, ScrollableCollectionNode.CollectionRemovalEventArgs e)
        {
            Aircraft aircraft = (Aircraft)e.RemovedNode;
            aircraft.ResetAnchorPoint();
            aircraft.Position = e.TouchOnRemove.Location;
            GUILayer.DragAndDropObject = aircraft;
            // check if the takeoffnode still holds any aircrafts
            if (!GUILayer.TakeoffCollectionNode.Collection.Any())
                GUILayer.GOButton.AddAction(RemoveGOButton);
        }

        internal void ReceivePartFromCollection(object sender, ScrollableCollectionNode.CollectionRemovalEventArgs e)
        {
            var node = e.RemovedNode;
            var gameObject = (IGameObject)e.RemovedNode;
            node.Position = e.TouchOnRemove.Location;
            node.Scale = GUILayer.HangarScaleToGUI() * Constants.STANDARD_SCALE;
            GUILayer.DragAndDropObject = gameObject;
        }
        internal void AddAircraft(Aircraft aircraft, CCPoint hangarPos, float hangarRot=0f, int insertAt=0)
        {
            if (insertAt == -1)
                Aircrafts.Add(aircraft);
            else
                Aircrafts.Insert(insertAt, aircraft);
            AddAircraftChild(aircraft);
            HangarRotations[aircraft] = hangarRot;
            PlaceAircraft(aircraft, hangarPos);
        }

        internal void RemoveAircraft(Aircraft aircraft)
        {
            Aircrafts.Remove(aircraft);
            if (aircraft.Parent == this)
            {
                RemoveChild(aircraft);
            }
            else if (aircraft.Parent == GUILayer.TakeoffCollectionNode)
            {
                GUILayer.TakeoffCollectionNode.RemoveFromCollection(aircraft);
            }
        }

        internal void AddAircraftChild(Aircraft aircraft)
        {
            AddChild(aircraft, (int)aircraft.Area);
        }
        internal ScrapyardButton[] ScrapyardButtons { get; private protected set; }
        internal ScrapyardButton CurrentScrapyardButton { get; private protected set; }
        internal void EnterScrapyardChallengeState(ScrapyardButton scrapyardButton)
        {
            if (State != HangarState.SCRAPYARD) return;
            CurrentScrapyardButton = scrapyardButton;
            StartTransition(HangarState.SCRAPYARD_CHALLENGE);
        }

        internal void ModifyNewAircraft()
        {
            Aircraft newAircraft = new Aircraft();
            AddAircraft(newAircraft, CCPoint.Zero);
            newAircraft.Position = CCPoint.Zero;
            ModifiedAircraft = newAircraft;
            StartTransition(HangarState.MODIFY_AIRCRAFT);
        }

        internal void PlaceAircraft(Aircraft aircraft, CCPoint hangarPos)
        {
            const float SAFETY = 0.001f; // numeric safety
            aircraft.Position = hangarPos;
            HangarPositions[aircraft] = aircraft.Position;
            var placementRect = PlacementRect(aircraft);
            if (RectAvailable(placementRect, out CCRect blockingRect, aircraft))
            {
                // all is well, the position is available
            }
            else
            {
                // the position is not available
                // move against the (4-way) direction in which the center of the blocking rect lies
                CCPoint movement;
                CCPoint myCenter = placementRect.Center;
                CCPoint bCenter  = blockingRect.Center;
                float dx = bCenter.X - myCenter.X;
                float dy = bCenter.Y - myCenter.Y;
                if (Math.Abs(dx) > Math.Abs(dy))
                    if (dx > 0)
                        movement = new CCPoint(blockingRect.MinX - placementRect.MaxX - SAFETY, 0);
                    else
                        movement = new CCPoint(blockingRect.MaxX - placementRect.MinX + SAFETY, 0);
                else
                    if (dy > 0)
                        movement = new CCPoint(0, blockingRect.MinY - placementRect.MaxY - SAFETY);
                    else
                        movement = new CCPoint(0, blockingRect.MaxY - placementRect.MinY + SAFETY);
                aircraft.Position += movement;
                HangarPositions[aircraft] = aircraft.Position;
                placementRect = PlacementRect(aircraft);
                // check whether the new position is available
                while (!RectAvailable(placementRect, out blockingRect, aircraft))
                {
                    // it's not, so move further (into the same direction as before)
                    if (movement.Y == 0)
                        movement = movement.X < 0 ?
                                    new CCPoint(blockingRect.MinX - placementRect.MaxX - SAFETY, 0) :
                                    new CCPoint(blockingRect.MaxX - placementRect.MinX + SAFETY, 0);
                    else
                        movement = movement.Y < 0 ?
                                    new CCPoint(0, blockingRect.MinY - placementRect.MaxY - SAFETY) :
                                    new CCPoint(0, blockingRect.MaxY - placementRect.MinY + SAFETY);
                    aircraft.Position += movement;
                    HangarPositions[aircraft] = aircraft.Position;
                    placementRect = PlacementRect(aircraft);
                }
            }
            // update the saved hangar position
            HangarPositions[aircraft] = aircraft.Position;
            // update the camera boundary variables
            CalcBoundaries();
            // and also the camera itself
            CameraPosition = CameraPosition;
            CameraSize = CameraSize;
            UpdateCamera();
        }

        internal void CalcBoundaries()
        {
            switch(State)
            {
                case HangarState.TRANSITION:
                    {
                        // during a transition everything goes (because the camera is moved by the program, not the player)
                        CameraSpace = new CCRect(float.MinValue, float.MinValue, float.PositiveInfinity, float.PositiveInfinity);
                        MaxCameraWidth = float.PositiveInfinity;
                        MaxCameraHeight = float.PositiveInfinity;
                    }
                    break;
                case HangarState.HANGAR:
                    {
                        const float BORDER = 300f;
                        float takeoffNodeHeight = GUILayer.TakeoffNode.ContentSize.Height * VisibleBoundsWorldspace.Size.Width / GUILayer.VisibleBoundsWorldspace.Size.Width;
                        if (Aircrafts.Any())
                        {
                            float minX = float.PositiveInfinity;
                            float minY = float.PositiveInfinity;
                            float maxX = float.NegativeInfinity;
                            float maxY = float.NegativeInfinity;
                            foreach (var aircraft in Aircrafts)
                            {
                                if (aircraft.Parent != this) continue;
                                var rect = aircraft.BoundingBoxTransformedToWorld;
                                if (rect.MinX < minX) minX = rect.MinX;
                                if (rect.MinY < minY) minY = rect.MinY;
                                if (rect.MaxX > maxX) maxX = rect.MaxX;
                                if (rect.MaxY > maxY) maxY = rect.MaxY;
                            }
                            CameraSpace = new CCRect(minX - BORDER, minY - BORDER - takeoffNodeHeight, maxX - minX + BORDER * 2, maxY - minY + BORDER * 2 + takeoffNodeHeight);
                            var size = LargestAircraftSize();
                            float widthRel = size.Width / Constants.COCOS_WORLD_WIDTH;
                            float heightRel = size.Height / Constants.COCOS_WORLD_HEIGHT;
                            float max = widthRel > heightRel ? widthRel : heightRel;
                            MaxCameraWidth = Constants.COCOS_WORLD_WIDTH * max * 8;
                            MaxCameraHeight = Constants.COCOS_WORLD_HEIGHT * max * 8;
                        }
                        else
                        {
                            CameraSpace = new CCRect(0 - BORDER, 0 - BORDER - takeoffNodeHeight, BORDER * 2, BORDER * 3 + takeoffNodeHeight);
                            MaxCameraWidth = Constants.COCOS_WORLD_WIDTH;
                            MaxCameraHeight = Constants.COCOS_WORLD_HEIGHT;
                        }
                    }
                    break;
                case HangarState.WORKSHOP:
                    {
                        float cameraMinY = CameraPositionWorkshop.Y - WorkshopHeight() + Constants.COCOS_WORLD_HEIGHT * 0.25f;
                        if (cameraMinY > CameraPositionWorkshop.Y) cameraMinY = CameraPositionWorkshop.Y;
                        CameraSpace = new CCRect(CameraPositionWorkshop.X, cameraMinY, Constants.COCOS_WORLD_WIDTH, Constants.COCOS_WORLD_HEIGHT + Math.Abs(CameraPositionWorkshop.Y - cameraMinY) + Constants.COCOS_WORLD_HEIGHT * 0.25f);
                        MaxCameraWidth = Constants.COCOS_WORLD_WIDTH;
                        MaxCameraHeight = Constants.COCOS_WORLD_HEIGHT;
                    }
                    break;
                case HangarState.MODIFY_AIRCRAFT:
                    {
                        float width = ModifyAircraftWidth();
                        var  bounds = VisibleBoundsWorldspace;
                        float ratio = bounds.Size.Height / bounds.Size.Width;
                        CameraSpace = new CCRect(-width * 2, ModifiedAircraft.Position.Y - width * 2 * ratio, width * 4, width * 4 * ratio);
                    }
                    break;
                case HangarState.SCRAPYARD:
                    {
                        float cameraMinY = CameraPositionScrapyard.Y - ScrapyardHeight() + Constants.COCOS_WORLD_HEIGHT * 0.25f;
                        if (cameraMinY > CameraPositionScrapyard.Y) cameraMinY = CameraPositionScrapyard.Y;
                        CameraSpace = new CCRect(CameraPositionScrapyard.X, cameraMinY, Constants.COCOS_WORLD_WIDTH, Constants.COCOS_WORLD_HEIGHT + Math.Abs(CameraPositionScrapyard.Y - cameraMinY) + Constants.COCOS_WORLD_HEIGHT * 0.25f);
                        MaxCameraWidth = Constants.COCOS_WORLD_WIDTH;
                        MaxCameraHeight = Constants.COCOS_WORLD_HEIGHT;
                    }
                    break;
                case HangarState.SCRAPYARD_CHALLENGE:
                    {
                        CameraSpace = ScrapyardButtonCameraRect(CurrentScrapyardButton);
                    }
                    break;
            }
            
        }

        private float ScrapyardHeight()
        {
            return Math.Abs(ScrapyardButtons.First().Position.Y - ScrapyardButtons.Last().Position.Y) + ScrapyardButton.ButtonSize.Height;
        }
        private CCRect ScrapyardButtonCameraRect(ScrapyardButton scrapyardButton)
        {
            var pos = scrapyardButton.PositionWorldspace;
            const float FACTOR = 2f;
            var size = ScrapyardButton.ButtonSize * FACTOR;
            size.Height = size.Width * Constants.COCOS_WORLD_HEIGHT / Constants.COCOS_WORLD_WIDTH;
            return new CCRect(pos.X - size.Width / 2, pos.Y - size.Height / 2 - size.Height * 0.2f, size.Width, size.Height);
        }

        internal CCRect PlacementRect(Aircraft aircraft)
        {
            // the aircraft has to be temporarily moved to its hangar position for this to work as intended
            // it also needs to be rotated correctly as well
            CCPoint currentPos = aircraft.Position;
            float currentRot = aircraft.MyRotation;
            aircraft.Position = HangarPositions[aircraft];
            aircraft.MyRotation = HangarRotations[aircraft];
            const float BORDER = 10f;
            var rect = aircraft.BoundingBoxTransformedToParent;
            aircraft.Position = currentPos;
            aircraft.MyRotation = currentRot;
            return new CCRect(rect.MinX - BORDER, rect.MinY - BORDER, rect.Size.Width + BORDER * 2, rect.Size.Height + BORDER * 2);
        }

        internal bool RectAvailable(CCRect rect, out CCRect blockingRect, Aircraft exceptedAircraft=null)
        {
            blockingRect = CCRect.Zero;
            foreach (var aircraft in Aircrafts)
            {
                if ((exceptedAircraft != null && aircraft == exceptedAircraft) || aircraft.Parent != this) continue;
                var placementRect = PlacementRect(aircraft);
                if (placementRect.IntersectsRect(rect))
                {
                    blockingRect = placementRect;
                    return false;
                }
            }
            return true;
        }

        internal CCSize LargestAircraftSize()
        {
            var size = new CCSize(0,0);
            var sizeArea = 0f;
            foreach (var aircraft in Aircrafts)
            {
                if (aircraft.Parent != this) continue;
                var aircraftSize = aircraft.ScaledContentSize;
                var aircraftSizeArea = aircraftSize.Width * aircraftSize.Height;
                if (aircraftSizeArea > sizeArea)
                {
                    size = aircraftSize;
                    sizeArea = aircraftSizeArea;
                }
            }
            return size;
        }
        internal Aircraft SelectedAircraft { get; set; } = null;
        new private protected void OnTouchesBegan(List<CCTouch> touches, CCEvent touchEvent)
        {
            base.OnTouchesBegan(touches, touchEvent);
            if (GUILayer.DragAndDropObject != null)
            {
                touchEvent.StopPropogation();
                return;
            }
            switch (touches.Count)
            {
                case 1:
                    {
                        var touch = touches[0];
                        switch(State)
                        {
                            case HangarState.HANGAR:
                                // if the touch is upon an aircraft, select it
                                foreach (var aircraft in Aircrafts)
                                    if (aircraft.Parent == this && aircraft.BoundingBoxTransformedToWorld.ContainsPoint(touch.StartLocation))
                                    {
                                        CCRect box = aircraft.BoundingBoxTransformedToWorld;
                                        float borderFactor = 0.15f;
                                        CCRect reducedBoundingBox = new CCRect(box.MinX + box.Size.Width  * borderFactor,
                                                                               box.MinY + box.Size.Height * borderFactor,
                                                                               box.Size.Width  - box.Size.Width * borderFactor * 2,
                                                                               box.Size.Height - box.Size.Width * borderFactor * 2);
                                        if (reducedBoundingBox.ContainsPoint(touch.StartLocation))
                                        {
                                            GUILayer.SetDragAndDropObjectWithRelativeTouchPos(aircraft, touch);
                                        }
                                        else
                                        {
                                            // rotate the aircraft!
                                            RotationSelectedNode = aircraft;
                                        }
                                        break;
                                    }
                                break;
                            case HangarState.WORKSHOP:
                                // if the touch is upon an aircraft, select it
                                foreach (var aircraft in Aircrafts)
                                    if (aircraft.Parent == this && aircraft.BoundingBoxTransformedToWorld.ContainsPoint(touch.StartLocation))
                                    {
                                        ModifiedAircraft = aircraft;
                                        StartTransition(HangarState.MODIFY_AIRCRAFT);
                                        break;
                                    }
                                break;
                            case HangarState.MODIFY_AIRCRAFT:
                                {
                                    // if the touch is upon a part unmount it and drag it
                                    foreach (Part part in ModifiedAircraft.TotalParts)
                                    {
                                        var rect = part.BoundingBoxTransformedToWorld;
                                        // grow the rect a little to make it easier to grab
                                        const float BORDER = 30f;
                                        rect = new CCRect(rect.MinX - BORDER, rect.MinY - BORDER, rect.Size.Width + 2*BORDER, rect.Size.Height + 2*BORDER);
                                        if (rect.ContainsPoint(touch.Location))
                                        {
                                            // change into standard state for unmounting first (god knows what would happen else)
                                            CCPoint realPos = part.PositionWorldspace;
                                            ModifiedAircraft.InWorkshopConfiguration = false;
                                            var mountParent = part.MountParent;
                                            bool flipped = part.Flipped;
                                            if (mountParent != null)
                                            {
                                                mountParent.Unmount(part);
                                            }
                                            else
                                            {
                                                // the part is the body (as only the body has no MountParent)
                                                part.Aircraft.Body = null;
                                            }
                                            ModifiedAircraft.RestrictChildrenToTotalParts();
                                            if (flipped) part.Flip();
                                            part.Position = GUILayer.HangarCoordinatesToGUI(realPos);// + (flipped ? new CCPoint(0, (part.AnchorPoint.Y - 0.5f) * 8 * part.BoundingBoxTransformedToWorld.Size.Height) : CCPoint.Zero));
                                            part.Scale = GUILayer.HangarScaleToGUI() * Constants.STANDARD_SCALE;
                                            GUILayer.SetDragAndDropObjectWithRelativeTouchPos(part, touch);
                                            ModifiedAircraft.InWorkshopConfiguration = true;
                                            DrawInModifyAircraftState();
                                            break;
                                        }
                                    }
                                }
                                break;
                            case HangarState.SCRAPYARD_CHALLENGE:
                                {
                                    // return to the scrapyard
                                    if (GUILayer.ChallengeNode == null || !GUILayer.ChallengeNode.BoundingBoxTransformedToWorld.ContainsPoint(GUILayer.HangarCoordinatesToGUI(touch.Location)))
                                        StartTransition(HangarState.SCRAPYARD);
                                }
                                break;
                        }
                    }
                    break;
                default:
                    break;
            }
        }
        private protected void OnTouchesMoved(List<CCTouch> touches, CCEvent touchEvent)
        {
            if (State != HangarState.TRANSITION)
                switch (TouchCount)
                {
                    case 1:
                        {
                            var touch = touches[0];
                            bool moveCam = true;
                            // if you are currently rotating a node rotate it
                            if (RotationSelectedNode != null)
                            {
                                moveCam = false;
                                CCPoint pos = ((CCNode)RotationSelectedNode).BoundingBoxTransformedToWorld.Center;
                                CCPoint vecPosToPrevTouch = pos - touch.PreviousLocation;
                                CCPoint vecPosToTouch = pos - touch.Location;
                                float previousAngle = Constants.DxDyToCCDegrees(vecPosToPrevTouch.X, vecPosToPrevTouch.Y);
                                float currentAngle  = Constants.DxDyToCCDegrees(vecPosToTouch.X, vecPosToTouch.Y);
                                float dAngle = Constants.AngleFromToDeg(previousAngle, currentAngle);
                                RotationSelectedNode.MyRotation += dAngle;
                            }
                            if (GUILayer.DragAndDropObject != null)
                                moveCam = false;
                            // move the camera
                            if (moveCam)
                                OnTouchesMovedMoveAndZoom(touches, touchEvent);
                        }
                        break;
                    case 2:
                        {
                            if (State == HangarState.HANGAR || State == HangarState.MODIFY_AIRCRAFT)
                                OnTouchesMovedMoveAndZoom(touches, touchEvent);
                        }
                        break;
                    default:
                        break;
                }
        }

        new private protected void OnTouchesEnded(List<CCTouch> touches, CCEvent touchEvent)
        {
            Scroller.ListenForTouches = false;
            if (State != HangarState.TRANSITION)
                switch (touches.Count)
                {
                    case 1:
                        {
                            var touch = touches[0];
                            // if a node is selected for rotation deselect it
                            if (RotationSelectedNode != null)
                            {
                                if (State == HangarState.HANGAR)
                                    HangarRotations[(Aircraft)RotationSelectedNode] = RotationSelectedNode.MyRotation;
                                RotationSelectedNode = null;
                            }
                            else // else scroll with inertia
                                Scroller.ListenForTouches = true;
                        }
                        break;
                    default:
                        break;
                }
            base.OnTouchesEnded(touches, touchEvent);
            Scroller.ListenForTouches = true;
        }
        
        private enum StreamEnum : byte
        {
            STOP, AIRCRAFTS, PARTS, CAMINFO, UNLOCKS, CHALLENGES, POPUPS
        }
        public async Task SaveToFile()
        {
            int tries = 0;
            start:
            Console.WriteLine("saving started");
            IFolder localFolder = PCLStorage.FileSystem.Current.LocalStorage;
            IFile saveFile = null;
            // create a file, overwriting any existing file
            try
            {
                Console.WriteLine("overwriting");
                saveFile = await localFolder.CreateFileAsync(Constants.SAVE_NAME, CreationCollisionOption.ReplaceExisting).ConfigureAwait(false);
                Console.WriteLine("done overwriting");
                using (MemoryStream mStream = new MemoryStream())
                {
                    BinaryWriter writer = new BinaryWriter(mStream);
                    // start challenge section
                    writer.Write((byte)StreamEnum.CHALLENGES);
                    // save which kinds of math challenges are unlocked already
                    // save how many challenges there are (as this could change)
                    writer.Write((int)5);
                    (new AddChallenge(dummy: true)).WriteToStream(writer);
                    (new SubChallenge(dummy: true)).WriteToStream(writer);
                    (new MultiplyChallenge(dummy: true)).WriteToStream(writer);
                    (new DivideChallenge(dummy: true)).WriteToStream(writer);
                    (new SolveChallenge(dummy: true)).WriteToStream(writer);
                    // start unlocks sections
                    writer.Write((byte)StreamEnum.UNLOCKS);
                    // save which kinds of plane slots are unlocked already
                    // first write an int specifying the version of this protocol (in case I change the plane slot behaviour later on)
                    // the current version is v0
                    writer.Write((int)0);
                    writer.Write(MathChallengeNode.UnlockedAddSubSlot);
                    writer.Write(MathChallengeNode.UnlockedMulDivSlot);
                    writer.Write(MathChallengeNode.UnlockedSolveSlot);
                    // start aircraft section
                    writer.Write((byte)StreamEnum.AIRCRAFTS);
                    // save how many aircrafts there are
                    // first filter out any possible defect (or at least unusable) aircrafts
                    int aCount = Aircrafts.Count;
                    foreach (var aircraft in Aircrafts)
                    {
                        if (aircraft.Body == null)
                            aCount--;
                    }
                    writer.Write(aCount);
                    // save the aircrafts
                    foreach (var aircraft in Aircrafts)
                    {
                        if (aircraft.Body == null)
                            continue;
                        CCPoint hPos = HangarPositions[aircraft];
                        writer.Write(hPos.X);
                        writer.Write(hPos.Y);
                        writer.Write(HangarRotations[aircraft]);
                        aircraft.WriteToStream(writer);
                    }
                    // start part section
                    writer.Write((byte)StreamEnum.PARTS);
                    // save the parts
                    var parts = GetParts();
                    writer.Write(parts.Count);
                    foreach (var part in parts)
                    {
                        part.WriteToStream(writer);
                    }
                    // start cam info section
                    writer.Write((byte)StreamEnum.CAMINFO);
                    if (State == HangarState.HANGAR)
                    {
                        writer.Write(CameraPosition.X);
                        writer.Write(CameraPosition.Y);
                        writer.Write(CameraSize.Width);
                        writer.Write(CameraSize.Height);
                    }
                    else
                    {
                        writer.Write(CameraPositionHangar.X);
                        writer.Write(CameraPositionHangar.Y);
                        writer.Write(CameraSizeHangar.Width);
                        writer.Write(CameraSizeHangar.Height);
                    }
                    // start the popup section
                    writer.Write((byte)StreamEnum.POPUPS);
                    PopUp.WriteToStream(writer);

                    writer.Write((byte)StreamEnum.STOP);

                    using (Stream stream = await saveFile.OpenAsync(PCLStorage.FileAccess.ReadAndWrite).ConfigureAwait(false))
                    {
                        Console.WriteLine("writing save!");
                        using (GZipStream compressionStream = new GZipStream(stream, CompressionMode.Compress))
                        {
                            mStream.Seek(0, SeekOrigin.Begin);
                            mStream.CopyTo(compressionStream);
                        }
                        Console.WriteLine("done writing");
                    }
                }
            }
            catch (Exception)
            {
                // try it again
                if (tries++ < 30)
                {
                    Thread.Sleep(5);
                    goto start;
                }
            }
        }

        public async Task LoadFromFile(bool keepCurrentMath = false)
        {
            bool init = true;
            try
            {
                IFolder localFolder = PCLStorage.FileSystem.Current.LocalStorage;
                bool saveExists = false;
                saveExists = await PCLHelper.IsFileExistAsync(Constants.SAVE_NAME, localFolder).ConfigureAwait(false);
                if (saveExists)
                {
                    // a save exists, create the hangar from the save
                    init = false;
                    using (MemoryStream decompressedStream = new MemoryStream())
                    {
                        using (Stream stream = await PCLHelper.ReadStreamAsync(Constants.SAVE_NAME, localFolder).ConfigureAwait(false))
                        {
                            using (GZipStream decompressionStream = new GZipStream(stream, CompressionMode.Decompress))
                            {
                                decompressionStream.CopyTo(decompressedStream);
                            }
                        }
                        decompressedStream.Seek(0, SeekOrigin.Begin);
                        BinaryReader reader = new BinaryReader(decompressedStream);
                        bool reading = true;
                        while (reading)
                        {
                            StreamEnum nextEnum = (StreamEnum)reader.ReadByte();
                            switch(nextEnum)
                            {
                                case StreamEnum.CHALLENGES:
                                    {
                                        // save which kinds of math challenges are unlocked already
                                        // load how many challenges there were when the save was written (as this could change)
                                        int challengeCount = reader.ReadInt32();
                                        for (int i=0; i<challengeCount; i++)
                                        {
                                            MathChallenge.CreateFromStream(reader, keepCurrentMath);
                                        }
                                    }
                                    break;
                                case StreamEnum.UNLOCKS:
                                    {
                                        // load which kinds of plane slots are unlocked already
                                        // first load an int specifying the version of this protocol (in case I change the plane slot behaviour later on)
                                        // the current version is v0
                                        int version = reader.ReadInt32();
                                        switch (version)
                                        {
                                            case 0:
                                                {
                                                    bool unlockedAddSub = reader.ReadBoolean();
                                                    bool unlockedMulDiv = reader.ReadBoolean();
                                                    bool unlockedSolve  = reader.ReadBoolean();
                                                    if (!keepCurrentMath)
                                                    {
                                                        MathChallengeNode.UnlockedAddSubSlot = unlockedAddSub;
                                                        MathChallengeNode.UnlockedMulDivSlot = unlockedMulDiv;
                                                        MathChallengeNode.UnlockedSolveSlot  = unlockedSolve;
                                                        UnlockedPlaneSlots = 2;
                                                        if (MathChallengeNode.UnlockedAddSubSlot) UnlockedPlaneSlots++;
                                                        if (MathChallengeNode.UnlockedMulDivSlot) UnlockedPlaneSlots++;
                                                        if (MathChallengeNode.UnlockedSolveSlot)  UnlockedPlaneSlots++;
                                                    }
                                                }
                                                break;
                                            default:
                                                break;
                                        }
                                    }
                                    break;
                                case StreamEnum.AIRCRAFTS:
                                    {
                                        // load the aircrafts
                                        int aircraftCount = reader.ReadInt32();
                                        for (int i = 0; i < aircraftCount; i++)
                                        {
                                            float hangarX = reader.ReadSingle();
                                            float hangarY = reader.ReadSingle();
                                            float hangarRot = reader.ReadSingle();
                                            Aircraft aircraft = Aircraft.CreateFromStream(reader);
                                            aircraft.MyRotation = hangarRot;    // because it starts in HANGAR-state
                                            AddAircraft(aircraft, new CCPoint(hangarX, hangarY), hangarRot, insertAt: -1);
                                        }
                                    }
                                    break;
                                case StreamEnum.PARTS:
                                    {
                                        // load the parts
                                        int partCount = reader.ReadInt32();
                                        for (int i = 0; i < partCount; i++)
                                        {
                                            Part part = Part.CreateFromStream(reader);
                                            AddPart(part);
                                        }
                                    }
                                    break;
                                case StreamEnum.CAMINFO:
                                    {
                                        // load the hangar camera
                                        float camX = reader.ReadSingle();
                                        float camY = reader.ReadSingle();
                                        CameraPositionHangar = new CCPoint(camX, camY);
                                        float camWidth  = reader.ReadSingle();
                                        float camHeight = reader.ReadSingle();
                                        CameraSizeHangar = new CCSize(camWidth, camHeight);
                                    }
                                    break;
                                case StreamEnum.POPUPS:
                                    {
                                        // load the popup-info
                                        PopUp.CreateFromStream(reader, keepCurrentMath);
                                    }
                                    break;
                                default:
                                case StreamEnum.STOP:
                                    reading = false;
                                    break;
                            }
                        }
                    }
                }
            }
            catch (Exception e)
            {
                // something went wrong, simply initialize the hangar instead
                Console.WriteLine("loading failed");
                if (!PopUp.TriggeredWelcome && !keepCurrentMath)    // only trigger it once at best
                    init = true;
            }
            if (init) Init();
        }

        internal override void Clear()
        {
            Aircrafts = null;
            GUILayer = null;
            TouchCountSource = null;
            this.ModifiedAircraft = null;
            this.NewAircraftButton = null;
            foreach (var b in ScrapyardButtons)
                if (b.CurrentMathChallengeNode != null)
                    b.CurrentMathChallengeNode.AnswerChosenEvent -= ScrapyardChallengeCallback;
            ScrapyardButtons = null;
            this.SelectedAircraft = null;
            this.FirstTouchListener = null;
            this.HighDrawNode = null;
            this.LowDrawNode = null;
            this.Scroller.MoveFunction = null;
            this.Scroller = null;
            
            this.StopAllActions();
            this.ResetCleanState();
        }

        private void Init()
        {
            // add some aircrafts
            AddAircraft(Aircraft.CreateTestAircraft(2, false), CCPoint.Zero);
            //AddAircraft(Aircraft.CreateTestAircraft(2, false), CCPoint.Zero);
            //AddAircraft(Aircraft.CreateTestAircraft(2, false), CCPoint.Zero);
            /*
            AddAircraft(Aircraft.CreateBalloon(), CCPoint.Zero);
            AddAircraft(Aircraft.CreateBat(), CCPoint.Zero);
            AddAircraft(Aircraft.CreateBigBomber(), CCPoint.Zero);
            AddAircraft(Aircraft.CreateFighter(), CCPoint.Zero);
            AddAircraft(Aircraft.CreateJet(), CCPoint.Zero);
            */
            // add some parts
            for (int i = 0; i < 1; i++)
            {
                AddPart(new TestBody());
                AddPart(new TestDoubleWing());
                AddPart(new TestRotor());
                AddPart(new TestWeapon());
                AddPart(new TestRudder());
            }
            // update the camera
            CalcBoundaries();
            UpdateCamera();
        }
    }
}
