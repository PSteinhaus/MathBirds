using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using CocosSharp;
using MathNet.Symbolics;

namespace CocosSharpMathGame
{
    internal class ScrapyardButtonButton : Button
    {
        internal ScrapyardButtonButton(string textureName) : base(textureName, false)
        {
            FitToBox(ScrapyardButton.ButtonSize);
        }

        private protected override void ButtonEnded(CCTouch touch)
        {
            ((HangarLayer)Layer).EnterScrapyardChallengeState(((ScrapyardButton)Parent));
        }
    }
    /// <summary>
    /// these are the buttons that the player can click on in the scrapyard
    /// </summary>
    internal class ScrapyardButton : GameObjectNode
    {
        internal static readonly CCSize ButtonSize = new CCSize(200, 200);
        private protected CCDrawNode MyDrawNode;
        internal string[] PartRewardsTypeNames { get; private protected set; }
        internal int LootboxCount { get; set; }
        internal float LootboxProgress { get; private protected set; }
        internal float LootboxProgressGoal { get; private protected set; }
        internal bool Pressable
        {
            get { return Button.Pressable; }
            set { Button.Pressable = value; }
        }
        internal MathChallenge ChallengeModel { get; private protected set; }
        internal MathChallengeNode CurrentMathChallengeNode { get; private set; }
        internal Button Button { get; private set; }
        internal ScrapyardButton(MathChallenge challengeModel, string textureName)
        {
            Scale = 1f;
            AnchorPoint = CCPoint.AnchorMiddle;
            Button = new ScrapyardButtonButton(textureName);
            Button.AnchorPoint = CCPoint.AnchorLowerLeft;
            AddChild(Button);
            ContentSize = Button.ScaledContentSize;
            ChallengeModel = challengeModel;
            // check if the challenge type is locked
            if (ChallengeModel.Locked)
            {
                var frame = UIElement.spriteSheet.Frames.Find(_ => _.TextureFilename.Equals("scrapyardLockedv2.png"));
                Button.ReplaceTexture(frame.Texture, frame.TextureRectInPixels);
                Button.Pressable = false;
                Button.Color = CCColor3B.Gray;
            }
            else
            {
                // generate and add your drawnode (for drawing the progress bar and other things)
                MyDrawNode = new CCDrawNode();
                MyDrawNode.BlendFunc = CCBlendFunc.NonPremultiplied;
                MyDrawNode.Position = (CCPoint)ContentSize / 2;
                AddChild(MyDrawNode, -100000);
            }
            // set the rewards
            switch (ChallengeModel)
            {
                case AddChallenge a:
                    {
                        PartRewardsTypeNames = new string[] { typeof(BodyPotato).AssemblyQualifiedName, typeof(RotorPotato).AssemblyQualifiedName,
                                                              typeof(RudderPotato).AssemblyQualifiedName, typeof(WeaponPotato).AssemblyQualifiedName,
                                                              typeof(WingPotato).AssemblyQualifiedName };
                    }
                    break;
                case SubChallenge s:
                    {
                        PartRewardsTypeNames = new string[] { typeof(BodyBat).AssemblyQualifiedName, typeof(RotorBat).AssemblyQualifiedName,
                                                              typeof(WeaponBat).AssemblyQualifiedName, typeof(WingBat).AssemblyQualifiedName };
                    }
                    break;
                case MultiplyChallenge m:
                    {
                        PartRewardsTypeNames = new string[] { typeof(TestRotor).AssemblyQualifiedName, typeof(TestBody).AssemblyQualifiedName,
                                                              typeof(TestDoubleWing).AssemblyQualifiedName, typeof(TestRudder).AssemblyQualifiedName,
                                                              typeof(TestWeapon).AssemblyQualifiedName };
                    }
                    break;
                case DivideChallenge d:
                    {
                        PartRewardsTypeNames = new string[] { typeof(BodyBalloon).AssemblyQualifiedName, typeof(RotorBalloon).AssemblyQualifiedName,
                                                              typeof(WeaponBalloon).AssemblyQualifiedName };
                    }
                    break;
                case SolveChallenge s:
                    {
                        PartRewardsTypeNames = new string[] { typeof(BodyFighter).AssemblyQualifiedName, typeof(RotorFighter).AssemblyQualifiedName,
                                                              typeof(RudderFighter).AssemblyQualifiedName, typeof(WeaponFighter).AssemblyQualifiedName,
                                                              typeof(WingFighter).AssemblyQualifiedName };
                    }
                    break;
                default:
                    break;
            }
        }

        protected override void AddedToScene()
        {
            base.AddedToScene();
            if (!ChallengeModel.Locked)
            {
                Schedule();
                CreateNextChallenge(); ;   // initialize it
                UpdateDrawNode(useMultiplGoal: true); // use the de facto value since the visual value is not up to date
            }
        }

        public override void Update(float dt)
        {
            base.Update(dt);
            // move towards the LootboxProgressGoal
            if (LootboxProgress != LootboxProgressGoal)
            {
                const float LOOTBOX_PROGRESS_INCREASE_RATE = 0.1f; // per 1/60 s
                const float BREAKOFF = 0.0001f;
                float diff = LootboxProgressGoal - LootboxProgress;
                if (Math.Abs(diff) > BREAKOFF)
                    LootboxProgress += LOOTBOX_PROGRESS_INCREASE_RATE * diff;
                else
                    LootboxProgress = LootboxProgressGoal;
                // if the progress bar is filled reward the player and decrease the progress and goal by 1
                if (LootboxProgress >= 1f)
                {
                    if (LootboxCount > 0)
                    {
                        Reward();
                        if (!(ChallengeModel is AddChallenge) || ((HangarLayer)Layer).CrappyPartsCheck())
                            LootboxCount -= 1;
                    }
                    LootboxProgress -= 1f;
                    LootboxProgressGoal -= 1f;
                }
                // visualize (update the DrawNode)
                UpdateDrawNode();
            }
        }

        internal float DrawNodeAlpha { get; set; } = 1f;
        internal void UpdateDrawNode(bool useMultiplGoal = false)
        {
            if (MyDrawNode == null) return;
            MyDrawNode.Clear();
            if (LootboxCount != 0)
            {
                // visualize lootbar-progress
                const float GROWTH_FACTOR = 1.3f;
                const float BORDER_WIDTH = 4f;
                CCColor4B white = new CCColor4B(1f, 1f, 1f, DrawNodeAlpha);
                CCColor4B bright = new CCColor4B(1f, 1f, 1f, DrawNodeAlpha);
                CCRect progressRect = new CCRect(-(ContentSize.Width * GROWTH_FACTOR) / 2, -(ContentSize.Height * GROWTH_FACTOR) / 2, ContentSize.Width * GROWTH_FACTOR, ContentSize.Height * GROWTH_FACTOR);
                // draw the progress bar
                var progressPoly = new List<CCPoint> { CCPoint.Zero, new CCPoint(0, (ContentSize.Height * GROWTH_FACTOR) / 2) };
                if (LootboxProgress >= 0.125f)
                {
                    progressPoly.Add(progressRect.UpperRight);
                }
                else
                {
                    progressPoly.Add(new CCPoint(LootboxProgress / 0.125f * progressRect.MaxX, progressRect.MaxY));
                    goto done;
                }
                if (LootboxProgress >= 0.375f)
                {
                    progressPoly.Add(new CCPoint(progressRect.MaxX, progressRect.MinY));
                }
                else
                {
                    progressPoly.Add(new CCPoint(progressRect.MaxX, progressRect.MaxY - progressRect.Size.Height * ((LootboxProgress - 0.125f) * 4)));
                    goto done;
                }
                if (LootboxProgress >= 0.625f)
                {
                    progressPoly.Add(new CCPoint(progressRect.MinX, progressRect.MinY));
                }
                else
                {
                    progressPoly.Add(new CCPoint(progressRect.MaxX - progressRect.Size.Width * ((LootboxProgress - 0.375f) * 4), progressRect.MinY));
                    goto done;
                }
                if (LootboxProgress >= 0.875f)
                {
                    progressPoly.Add(new CCPoint(progressRect.MinX, progressRect.MaxY));
                }
                else
                {
                    progressPoly.Add(new CCPoint(progressRect.MinX, progressRect.MinY + progressRect.Size.Height * ((LootboxProgress - 0.625f) * 4)));
                    goto done;
                }
                if (LootboxProgress > 0.875f)
                {
                    progressPoly.Add(new CCPoint(progressRect.MinX + progressRect.Size.Width * (LootboxProgress - 0.875f) * 2, progressRect.MaxY));
                }
            done:
                var progressPolyPoints = progressPoly.ToArray();
                MyDrawNode.DrawPolygon(progressPolyPoints, progressPolyPoints.Length, bright, 0f, CCColor4B.Transparent);
                // draw the progress bar border
                MyDrawNode.DrawRect(progressRect, CCColor4B.Transparent, BORDER_WIDTH, white);
            }

            // additionally color the button depending on the multiplier progress
            int multiplier = 1;
            float progress = 0;
            if (useMultiplGoal)
            {
                multiplier = (int)CurrentMathChallengeNode.MultiplGoal;
                progress = CurrentMathChallengeNode.MathChallenge.MultiplProgress;
            }
            else
            {
                multiplier = CurrentMathChallengeNode.Multiplier;
                progress = CurrentMathChallengeNode.MultiplProgressVisible;
            }
            var lastColor = MathChallengeNode.MultiplierBarColor(multiplier - 1);
            var nextColor = MathChallengeNode.MultiplierBarColor(multiplier);
            var currentColor = multiplier <= 5 ? CCColor4B.Lerp(lastColor, nextColor, progress) : lastColor;
            var darkenedColor = CCColor4B.Lerp(currentColor, CCColor4B.Black, 0.25f);
            darkenedColor.A = (byte)(DrawNodeAlpha * byte.MaxValue);
            MyDrawNode.DrawRect(new CCRect(-(ContentSize.Width) / 2, -(ContentSize.Height) / 2, ContentSize.Width, ContentSize.Height), darkenedColor);
        }

        internal event EventHandler<Part> RewardEvent;
        internal void Reward()
        {
            if (PartRewardsTypeNames != null && PartRewardsTypeNames.Length != 0)
            {
                // choose a random reward
                Random rng = new Random();
                Type t = Type.GetType(PartRewardsTypeNames[rng.Next(0, PartRewardsTypeNames.Length)]);
                Part reward = ((Part)Activator.CreateInstance(t));
                // start some visualization-actions showing the reward
                Button.AddAction(new CCSequence(new CCFadeTo(0.25f, 0), new CCDelayTime(3f), new CCFadeTo(1f, byte.MaxValue)));
                var guiLayer = ((HangarLayer)Layer).GUILayer;
                reward.Position = guiLayer.HangarCoordinatesToGUI(BoundingBoxTransformedToWorld.Center);
                guiLayer.AddChild(reward);
                reward.FitToBox(Button.BoundingBoxTransformedToWorld.Size * 2);
                var rewardEndScale = reward.GetTotalScale();
                reward.Scale = 0.000001f;
                reward.AddAction(new CCSequence(new CCDelayTime(1.25f),
                                                new CCEaseBackOut(new CCScaleTo(0.5f, rewardEndScale)),
                                                new CCDelayTime(1f),
                                                new CCEaseIn(new CCMoveTo(0.8f, guiLayer.VisibleBoundsWorldspace.Center + new CCPoint(0, guiLayer.VisibleBoundsWorldspace.Size.Height)), 3f),
                                                new CCRemoveSelf(),
                                                new CCCallFunc( () => { RewardEvent?.Invoke(this, reward); } )));
            }
        }

        internal float BaseIncrease { get; set; } = 0.125f;
        internal float BonusIncrease { get; set; } = 0.075f;
        internal void ChallengeSolved()
        {
            // advance the lootbox-meter-goal
            var rng = new Random();
            LootboxProgressGoal += (BaseIncrease + ((float)rng.NextDouble()) * BonusIncrease) * CurrentMathChallengeNode.Multiplier;
        }

        internal void ChallengeFailed()
        {
            // reset the lootbox-meter-goal somewhat
            LootboxProgressGoal *= 0.33f;    // lose 66% of your progress with each wrong answer 
        }

        internal MathChallengeNode GenerateMathChallengeNode(float multiplVisible)
        {
            return new MathChallengeNode(ChallengeModel.CreateFromSelf(), multiplVisible);
        }

        internal void CreateNextChallenge(float multiplVisible = 1)
        {
            CurrentMathChallengeNode = GenerateMathChallengeNode(multiplVisible);
        }

        internal void CreateSameChallenge(float multiplVisible = 1)
        {
            CurrentMathChallengeNode = new MathChallengeNode(CurrentMathChallengeNode.MathChallenge, multiplVisible);
        }
    }
}
