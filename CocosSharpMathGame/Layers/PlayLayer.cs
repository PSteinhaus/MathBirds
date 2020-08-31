using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using CocosSharp;
using Microsoft.Xna.Framework;
using System.Linq;
using MathNet.Numerics.Random;

namespace CocosSharpMathGame
{
    public class PlayLayer : MyLayer
    {
        internal CCNode BGNode { get; private protected set; }
        internal enum GameState
        {
            PLANNING, EXECUTING_ORDERS
        }
        internal Team PlayerTeam { get { return Team.PlayerTeam; } }
        internal GameState State { get; private set; } = GameState.PLANNING;
        public GUILayer GUILayer { get; set; }
        private readonly CCDrawNode DrawNode = new CCDrawNode();
        private readonly CCDrawNode DrawNodeBG = new CCDrawNode();
        private readonly CCDrawNode DrawNodeExplosions = new CCDrawNode();
        internal List<Squadron> Squadrons { get; private protected set; } = new List<Squadron>();
        internal List<Aircraft> Aircrafts { get; private protected set; } = new List<Aircraft>();
        internal List<PowerUp> PowerUps { get; private set; } = new List<PowerUp>();
        internal List<Aircraft> PlayerAircrafts { get; private protected set; }
        internal List<Aircraft> DownedAircrafts { get; private protected set; } = new List<Aircraft>();
        internal List<Projectile> Projectiles { get; set; } = new List<Projectile>();
        internal List<IDrawNodeUser> DrawNodeUsers { get; set; } = new List<IDrawNodeUser>();
        internal CCDrawNode HighDrawNode { get; set; }
        internal CCDrawNode LowDrawNode { get; set; }
        public PlayLayer() : base(CCColor4B.Black)
        {
            GUILayer = new GUILayer(this);
            TouchCountSource = GUILayer;
            BGNode = new CCNode();
            AddChild(BGNode);
            BGNode.VertexZ = Constants.VERTEX_Z_GROUND;
            BGNode.AddChild(DrawNode);
            BGNode.AddChild(DrawNodeBG);
            DrawNodeBG.BlendFunc = CCBlendFunc.NonPremultiplied;  // necessary for alpha to work
            BGNode.ZOrder = (int)Constants.VERTEX_Z_GROUND;
            BGNode.Rotation = 10f;

            AddChild(DrawNodeExplosions, -2000);
            DrawNodeExplosions.VertexZ = Constants.VERTEX_Z_GROUND + 1;
            DrawNodeExplosions.BlendFunc = CCBlendFunc.NonPremultiplied;

            HighDrawNode = new CCDrawNode();
            HighDrawNode.BlendFunc = CCBlendFunc.NonPremultiplied;
            AddChild(HighDrawNode, zOrder: 1000);
            LowDrawNode = new CCDrawNode();
            LowDrawNode.BlendFunc = CCBlendFunc.NonPremultiplied;
            AddChild(LowDrawNode, zOrder: -1000);

            MaxCameraWidth = Constants.COCOS_WORLD_WIDTH * 10;
            MaxCameraHeight = Constants.COCOS_WORLD_HEIGHT * 10;

            // add a touch listener
            var touchListener = new CCEventListenerTouchAllAtOnce();
            touchListener.OnTouchesBegan = OnTouchesBegan;
            touchListener.OnTouchesMoved = OnTouchesMovedMoveAndZoom;
            touchListener.OnTouchesEnded = OnTouchesEnded;
            touchListener.OnTouchesCancelled = OnTouchesEnded;
            AddEventListener(touchListener, int.MaxValue);

            // add a mouse listener
            var mouseListener = new CCEventListenerMouse();
            mouseListener.OnMouseScroll = OnMouseScrollZoom;
            AddEventListener(mouseListener, int.MaxValue);
        }

        const float CHUNK_SIZE = 8000f;
        internal static CCPointI PosToWorldChunk(CCPoint position)
        {
            var scaled = position / CHUNK_SIZE;
            return new CCPointI((int)Math.Round(scaled.X, 0), (int)Math.Round(scaled.Y, 0));
        }

        internal static CCPoint ChunkToWorldPos(CCPointI chunkPos)
        {
            return new CCPoint(chunkPos.X * (int)CHUNK_SIZE, chunkPos.Y * (int)CHUNK_SIZE);
        }

        internal static CCColor4B RadiusToColor(float radius, bool groundNotAir)
        {
            int zone = RadiusToZoneNum(radius);
            // calculate the relative difference of the radius to the radius marking the center of the zone (in percent)
            float prevEndRad = zone != 0 ? ZoneEndRadii[zone - 1] : 0;
            float zoneSize = zone != ZoneEndRadii.Length ? ZoneEndRadii[zone] - prevEndRad : 18000f;
            float relDiff = (radius - prevEndRad) / zoneSize - 0.5f;
            const float lerpStart = 0.3f;

            if (relDiff < -lerpStart && zone != 0)
            {
                return CCColor4B.Lerp(groundNotAir ? ZoneColorsGround[zone] : ZoneColorsAir[zone],
                                      groundNotAir ? ZoneColorsGround[zone - 1] : ZoneColorsAir[zone - 1],
                                      (-relDiff - lerpStart) / (0.5f - lerpStart) / 2);
            }
            else if (relDiff > lerpStart && zone != ZoneEndRadii.Length)
            {
                return CCColor4B.Lerp(groundNotAir ? ZoneColorsGround[zone] : ZoneColorsAir[zone],
                                      groundNotAir ? ZoneColorsGround[zone + 1] : ZoneColorsAir[zone + 1],
                                      (relDiff - lerpStart) / (0.5f - lerpStart) / 2);
            }
            else
                return groundNotAir ? ZoneColorsGround[zone] : ZoneColorsAir[zone];
        }

        internal override void Clear()
        {
            Squadrons = null;
            Aircrafts = null;
            PlayerAircrafts = null;
            DownedAircrafts = null;
            Projectiles = null;
            DrawNodeUsers = null;
            GUILayer = null;
            this.ActiveChunks = null;
            this.FirstTouchListener = null;
            this.HighDrawNode = null;
            this.KnownChunks = null;
            this.LowDrawNode = null;
            this.Scroller.MoveFunction = null;
            this.Scroller = null;
            this.StopAllActions();
            this.ResetCleanState();
        }

        internal void InitPlayerAircrafts(List<Aircraft> playerAircrafts)
        {
            const float BORDER = 50f;
            const float theta = (float)Math.PI / 8;
            // add the aircrafts
            foreach (var aircraft in playerAircrafts)
            {
                aircraft.Team = PlayerTeam;
                aircraft.ControlledByPlayer = true;
                AddAircraft(aircraft);
                aircraft.PartsChanged(true);
            }
            PlayerAircrafts = playerAircrafts;
            // place the aircrafts in "v"-formation
            if (playerAircrafts.Count() % 2 == 1)
            {
                // v with pointy head (1 aircrafts)
                var pos = CCPoint.Zero;
                playerAircrafts[0].Position = pos;
                float y = playerAircrafts[0].ScaledContentSize.Height / 2 + BORDER;
                var upPos   = new CCPoint(-(float)Math.Sin(theta) * y, y);
                var downPos = new CCPoint(upPos.X, -upPos.Y);
                bool upside = new Random().NextBoolean();
                for (int i=1; i<playerAircrafts.Count; i++)
                {
                    var aircraft = playerAircrafts[i];
                    if (upside)
                    {
                        y = aircraft.ScaledContentSize.Height / 2;
                        upPos += new CCPoint(-(float)Math.Sin(theta) * y, y);
                        aircraft.Position = upPos;
                        y += BORDER;
                        upPos += new CCPoint(-(float)Math.Sin(theta) * y, y);
                        if (upPos.X < downPos.X)
                            upside = false;
                    }
                    else
                    {
                        y = aircraft.ScaledContentSize.Height / 2;
                        downPos += new CCPoint(-(float)Math.Sin(theta) * y, -y);
                        aircraft.Position = downPos;
                        y += BORDER;
                        downPos += new CCPoint(-(float)Math.Sin(theta) * y, -y);
                        if (downPos.X < upPos.X)
                            upside = true;
                    }
                }
            }
            else
            {
                // v with dull head (2 aircrafts)
                float y = playerAircrafts[0].ScaledContentSize.Height / 2 + BORDER / 2;
                var upPos   = new CCPoint(-(float)Math.Sin(theta) * y, y);
                playerAircrafts[0].Position = upPos;
                y      += playerAircrafts[0].ScaledContentSize.Height / 2 + BORDER;
                upPos       = new CCPoint(-(float)Math.Sin(theta) * y, y);

                y       = playerAircrafts[1].ScaledContentSize.Height / 2 + BORDER / 2;
                var downPos = new CCPoint(-(float)Math.Sin(theta) * y, -y);
                playerAircrafts[1].Position = downPos;
                y      += playerAircrafts[1].ScaledContentSize.Height / 2 + BORDER;
                downPos     = new CCPoint(-(float)Math.Sin(theta) * y, -y);

                bool upside = new Random().NextBoolean();
                for (int i = 2; i < playerAircrafts.Count; i++)
                {
                    var aircraft = playerAircrafts[i];
                    if (upside)
                    {
                        y = aircraft.ScaledContentSize.Height / 2;
                        upPos += new CCPoint(-(float)Math.Sin(theta) * y, y);
                        aircraft.Position = upPos;
                        y += BORDER;
                        upPos += new CCPoint(-(float)Math.Sin(theta) * y, y);
                        if (upPos.X < downPos.X)
                            upside = false;
                    }
                    else
                    {
                        y = aircraft.ScaledContentSize.Height / 2;
                        downPos += new CCPoint(-(float)Math.Sin(theta) * y, -y);
                        aircraft.Position = downPos;
                        y += BORDER;
                        downPos += new CCPoint(-(float)Math.Sin(theta) * y, -y);
                        if (downPos.X < upPos.X)
                            upside = true;
                    }
                }
            }
            CameraPosition = -(CCPoint)CameraSize / 2;
            UpdateCamera();
            StartPlanningPhase();
        }

        protected override void AddedToScene()
        {
            base.AddedToScene();
            // show the welcome message
            if (!PopUp.TriggeredPlayLayer)
                PopUp.ShowPopUp(GUILayer, PopUp.Enum.TRIGGERED_PLAYLAYER);  
            UpdateCamera();
        }

        internal void AddAircraft(Aircraft aircraft)
        {
            Aircrafts.Add(aircraft);
            AddChild(aircraft);
        }
        internal void RemoveAircraft(Aircraft aircraft)
        {
            var squadron = aircraft.Squadron;
            if (squadron != null)
            {
                squadron.RemoveAircraft(aircraft);
                if (squadron.Leader == null)
                    RemoveSquadron(squadron);
            }
            Aircrafts.Remove(aircraft);
            aircraft.VertexZ = 0f;  // reset vertexZ
            if (PlayerAircrafts.Contains(aircraft))
            {
                PlayerAircrafts.Remove(aircraft);
                // check if the player now has lost all his aircrafts
                if (!PlayerAircrafts.Any())
                {
                    // and if true stop the PlayLayer and enter the WreckageLayer
                    EnterWreckageLayer();
                }
            }
            RemoveChild(aircraft);
        }

        internal void AddProjectile(Projectile projectile)
        {
            Projectiles.Add(projectile);
            DrawNodeUsers.Add(projectile);
            AddChild(projectile);
        }
        internal void RemoveProjectile(Projectile projectile)
        {
            Projectiles.Remove(projectile);
            DrawNodeUsers.Remove(projectile);
            RemoveChild(projectile);
        }

        private float TimeLeftExecutingOrders { get; set; }

        internal void ExecuteOrders()
        {
            State = GameState.EXECUTING_ORDERS;
            foreach (var aircraft in PlayerAircrafts)
                aircraft.PrepareForExecuteOrders();
            TimeLeftExecutingOrders = Constants.TURN_DURATION;
        }
        internal bool PlayerIsAlive
        {
            get
            {
                bool alive = false;
                foreach (var aircraft in PlayerAircrafts)
                    if (aircraft.MyState.Equals(Aircraft.State.ACTIVE))
                    {
                        alive = true;
                        break;
                    }
                return alive;
            }
        }

        internal List<CCPointI> ActiveChunks { get; private protected set; } = new List<CCPointI>();
        internal List<CCPointI> KnownChunks { get; private protected set; } = new List<CCPointI>();

        internal void StartPlanningPhase()
        {
            State = GameState.PLANNING;
            // find all active chunks
            // for that first find the player chunks and then grow around them
            // also calculate the new camera boundaries based on the plane positions
            float minX = float.PositiveInfinity;
            float minY = float.PositiveInfinity;
            float maxX = float.NegativeInfinity;
            float maxY = float.NegativeInfinity;
            var activeChunksBefore = new CCPointI[ActiveChunks.Count];
            ActiveChunks.CopyTo(activeChunksBefore);
            ActiveChunks.Clear();
            foreach (var aircraft in PlayerAircrafts)
            {
                if (aircraft.Position.X < minX) minX = aircraft.Position.X;
                if (aircraft.Position.X > maxX) maxX = aircraft.Position.X;
                if (aircraft.Position.Y < minY) minY = aircraft.Position.Y;
                if (aircraft.Position.Y > maxY) maxY = aircraft.Position.Y;
                var aircraftChunk = PosToWorldChunk(aircraft.Position);
                for (int dx = -1; dx <= 1; dx++)
                    for (int dy = -1; dy <= 1; dy++)
                    {
                        var activeChunk = aircraftChunk + new CCPointI(dx, dy);
                        if (!ActiveChunks.Contains(activeChunk))
                            ActiveChunks.Add(activeChunk);
                    }
            }
            
            var noLongerActiveChunks = new List<CCPointI>();
            foreach (CCPointI chunkPoint in activeChunksBefore)
                if (!ActiveChunks.Contains(chunkPoint))
                    noLongerActiveChunks.Add(chunkPoint);
                    
            const float BORDER = 5500f;
            CameraSpace = new CCRect(minX - BORDER, minY - BORDER, maxX - minX + BORDER * 2, maxY - minY + BORDER * 2);
            // check if there are any new chunks
            // if there are generate their contents (i.e. the enemies that are supposed to be there)
            foreach (var chunkPoint in ActiveChunks)
                if (!KnownChunks.Contains(chunkPoint))
                    InitiateChunk(chunkPoint);
            // prepare the squadrons
            foreach (var squadron in Squadrons)
            {
                var chunkPoint = PosToWorldChunk(squadron.Position);
                if (ActiveChunks.Contains(chunkPoint))
                    squadron.PrepareForPlanningPhase(this);
                else if (noLongerActiveChunks.Contains(chunkPoint))
                    foreach (var aircraft in squadron.AircraftsWithRelPositions.Keys)
                        aircraft.PrepareForStandby();
            }
            // prepare the player-aircrafts
            foreach (var aircraft in PlayerAircrafts)
                aircraft.PrepareForPlanningPhase();
            // only go back to normal if the player is still alive
            if (PlayerIsAlive)
            {
                // make the ExecuteOrderButton visible again
                GUILayer.ExecuteOrdersButton.Visible = true;
            }
            else
            {
                ExecuteOrders();
                //AddAction(new CCSequence(new CCDelayTime(0.25f), new CCCallFunc( () => ExecuteOrders() )));
            }
            
        }

        static readonly float[] ZoneEndRadii = new float[] { 1500f, 7000f, 14000f, 30000f };
        static readonly CCColor4B[] ZoneColorsGround = new CCColor4B[] { new CCColor4B(0f, 0f, 0f, 0.1f), new CCColor4B(1f, 0.4f, 0f, 0.1f), new CCColor4B(1f, 1f, 0f, 0.1f), new CCColor4B(0f, 1f, 0f, 0.1f), new CCColor4B(0f, 0f, 1f, 0.1f) };
        static readonly CCColor4B[] ZoneColorsAir = new CCColor4B[]    { CCColor4B.White, new CCColor4B(1f, 0.4f, 0.0f, 1f), new CCColor4B(1f, 1f, 0.0f, 1f), new CCColor4B(0.0f, 1f, 0.0f, 1f), new CCColor4B(0.0f, 0.0f, 1f, 1f) };
        internal static int RadiusToZoneNum(float radius)
        {
            for (int i = 0; i < ZoneEndRadii.Length; i++)
                if (radius < ZoneEndRadii[i])
                    return i;
            return ZoneEndRadii.Length;
        }

        internal void AddSquadron(Squadron squadron, CCPoint position, float CCdirection)
        {
            squadron.Leader.Position = position;
            squadron.Leader.RotateTo(CCdirection);
            // rotate all formation positions correctly around the leader and set everyone to their positions
            float angle = Constants.CCDegreesToMathRadians(CCdirection);
            foreach (var entry in squadron.AircraftsWithRelPositions)
            {
                var formationPoint = CCPoint.RotateByAngle(entry.Value, CCPoint.Zero, angle);
                entry.Key.Position = position + formationPoint;
                entry.Key.RotateTo(CCdirection);
                AddAircraft(entry.Key);
            }
            squadron.GenerateWayPoint();
            Squadrons.Add(squadron);
        }

        internal void RemoveSquadron(Squadron squadron)
        {
            foreach (var entry in squadron.AircraftsWithRelPositions)
            {
                RemoveAircraft(entry.Key);
            }
            Squadrons.Remove(squadron);
        }

        /// <summary>
        /// Returned Squadron can be null!
        /// </summary>
        /// <param name="zoneNum"></param>
        /// <returns></returns>
        internal Squadron GenerateSquadron(int zoneNum, Random rng)
        {
            Squadron newSquadron = null;
            switch (zoneNum)
            {
                case 0:
                    {
                        // no squadrons are present in the first zone
                    }
                    break;
                case 1:
                    {
                        // second zone: potatos and bats
                        switch (rng.Next(4))
                        {
                            case 0:
                                {
                                    // create a peaceful Potato
                                    var leader = Aircraft.CreatePotato();
                                    leader.AI = new PeacefulFleeingAI();
                                    newSquadron = new Squadron();
                                    newSquadron.AddAircraft(leader, CCPoint.Zero);
                                }
                                break;
                            case 1:
                                {
                                    // create a non-peaceful Potato
                                    var leader = Aircraft.CreatePotato(true);
                                    leader.AI = new StandardAI();
                                    newSquadron = new Squadron();
                                    newSquadron.AddAircraft(leader, CCPoint.Zero);
                                }
                                break;
                            case 2:
                                {
                                    // create a bat
                                    var leader = Aircraft.CreateBat();
                                    leader.AI = new StandardAI();
                                    newSquadron = new Squadron();
                                    newSquadron.AddAircraft(leader, CCPoint.Zero);
                                }
                                break;
                            case 3:
                                {
                                    // create a 3-potato squad
                                    var leader = Aircraft.CreatePotato(true);
                                    var ally1 = Aircraft.CreatePotato();
                                    var ally2 = Aircraft.CreatePotato();
                                    leader.AI = new StandardAI();
                                    ally1.AI = new PeacefulFleeingAI();
                                    ally2.AI = new PeacefulFleeingAI();
                                    newSquadron = new Squadron();
                                    newSquadron.AddAircraft(leader, CCPoint.Zero);
                                    newSquadron.AddAircraft(ally1, new CCPoint(-90f, 145f));
                                    newSquadron.AddAircraft(ally2, new CCPoint(-90f, -145f));
                                }
                                break;
                        }
                    }
                    break;
                case 2:
                    {
                        // third zone: TestAircrafts and Balloons
                        switch (rng.Next(3))
                        {
                            case 0:
                                {
                                    // create a lone TestAircraft
                                    var leader = Aircraft.CreateTestAircraft();
                                    leader.AI = new StandardAI();
                                    newSquadron = new Squadron();
                                    newSquadron.AddAircraft(leader, CCPoint.Zero);
                                }
                                break;
                            case 1:
                                {
                                    // create a Balloon
                                    var leader = Aircraft.CreateBalloon(true);
                                    leader.AI = new StandardAI();
                                    newSquadron = new Squadron();
                                    newSquadron.AddAircraft(leader, CCPoint.Zero);
                                }
                                break;
                            case 2:
                                {
                                    // create a squad of 3 TestAircrafts
                                    newSquadron = new Squadron();
                                    Aircraft[] planes = new Aircraft[3];
                                    for (int i = 0; i < 3; i++)
                                    {
                                        int weaponNum = rng.Next(3);
                                        planes[i] = Aircraft.CreateTestAircraft(weaponNum);
                                        planes[i].AI = weaponNum != 0 ? (AI)(new StandardAI()) : (AI)(new PeacefulFleeingAI());
                                    }
                                    newSquadron.AddAircraft(planes[0], CCPoint.Zero);
                                    newSquadron.AddAircraft(planes[1], new CCPoint(-250f, 255f));
                                    newSquadron.AddAircraft(planes[2], new CCPoint(-250f,-255f));
                                }
                                break;
                        }
                    }
                    break;
                case 3:
                    {
                        // fourth zone: Fighters and BigBombers
                        switch (rng.Next(5))
                        {
                            case 0:
                            case 1:
                                {
                                    // create a lone Fighter
                                    var leader = Aircraft.CreateFighter();
                                    leader.AI = new StandardAI();
                                    newSquadron = new Squadron();
                                    newSquadron.AddAircraft(leader, CCPoint.Zero);
                                }
                                break;
                            case 2:
                                {
                                    // create a Fighter Squad of 2 Fighters
                                    var leader = Aircraft.CreateFighter();
                                    leader.AI = new StandardAI();
                                    var plane2 = Aircraft.CreateFighter();
                                    plane2.Team = Team.EnemyTeam;
                                    plane2.AI = new StandardAI();
                                    newSquadron = new Squadron();
                                    newSquadron.AddAircraft(leader, new CCPoint(0, 270f));
                                    newSquadron.AddAircraft(plane2, new CCPoint(0,-270f));
                                }
                                break;
                            case 3:
                                {
                                    // create a squad of a BigBomber and some Fighters
                                    newSquadron = new Squadron();
                                    newSquadron.AddAircraft(Aircraft.CreateBigBomber(), CCPoint.Zero);
                                    int fighterCount = rng.Next(5) == 0 ? 4 : 2;
                                    Aircraft[] fighter = new Aircraft[fighterCount];
                                    for (int i = 0; i < fighterCount; i++)
                                    {
                                        fighter[i] = Aircraft.CreateFighter();
                                        fighter[i].AI = new StandardAI();
                                    }
                                    if (fighterCount >= 2)
                                    {
                                        newSquadron.AddAircraft(fighter[0], new CCPoint(330f, 255f));
                                        newSquadron.AddAircraft(fighter[1], new CCPoint(330f, -255f));
                                    }
                                    if (fighterCount == 4)
                                    {
                                        newSquadron.AddAircraft(fighter[2], new CCPoint(-330f, 255f));
                                        newSquadron.AddAircraft(fighter[3], new CCPoint(-330f, -255f));
                                    }
                                }
                                break;
                        }
                    }
                    break;
                case 4:
                default:    // this is also the default case because this is the last zone
                    {
                        // fifth zone: Jets, Jets and more Jets
                        var leader = Aircraft.CreateJet();
                        leader.Team = Team.EnemyTeam;
                        leader.AI = new StandardAI();
                        newSquadron = new Squadron();
                        newSquadron.AddAircraft(leader, CCPoint.Zero);
                    }
                    break;
            }
            if (newSquadron != null)
            {
                newSquadron.MinR = ZoneEndRadii[zoneNum - 1];
                newSquadron.MaxR = (zoneNum < ZoneEndRadii.Length ? ZoneEndRadii[zoneNum] : float.PositiveInfinity);
            }
            return newSquadron;
        }

        private protected void InitiateChunk(CCPointI chunkPoint)
        {
            
            // add some squadrons randomly
            var rng = new Random();
            /*
            const int MIN_SQUADS_PER_CHUNK = 4;
            const int MAX_SQUADS_PER_CHUNK = 5;
            int squadCount = rng.Next(MIN_SQUADS_PER_CHUNK, MAX_SQUADS_PER_CHUNK + 1);
            */
            int squadCount = 7;
            // choose random positions inside of this chunk
            CCPoint chunkMiddle = ChunkToWorldPos(chunkPoint);
            for (int i=0; i<squadCount; i++)
            {
                var randomPos = Constants.RandomPointBoxnear(chunkMiddle, CHUNK_SIZE / 2, rng);
                int zone = RadiusToZoneNum(randomPos.Length);
                // place more squads in zone 1 and less in zone 3 (statistically)
                switch (zone)
                {
                    case 1:
                        if (rng.Next(3) == 0)
                            i--;
                        break;
                    case 3:
                        if (rng.Next(3) == 0)
                            i++;
                        break;
                    default:
                        break;
                }
                var newSquadron = GenerateSquadron(zone, rng);
                if (newSquadron != null)
                {
                    // choose a random orientation
                    var CCdirection = (float)rng.NextDouble() * 360f;
                    AddSquadron(newSquadron, randomPos, CCdirection);
                }
            }
            
            KnownChunks.Add(chunkPoint);
        }

        private protected void EnterWreckageLayer()
        {
            var wreckageLayer = new WreckageLayer();
            TransitionFadingFromTo(this.GUILayer, wreckageLayer.GUILayer, this, wreckageLayer, 0.8f);
            GUILayer.PlayLayer = null;
            /*
            var parent = Parent;
            RemoveAllListeners();
            GUILayer.RemoveAllListeners();
            Parent.RemoveChild(GUILayer);
            Parent.RemoveChild(this);
            parent.AddChild(wreckageLayer.GUILayer);
            parent.AddChild(wreckageLayer, zOrder: int.MinValue);
            */
            // place the aircrafts and add them as children
            var list = DownedAircrafts.ToArray().ToList();
            wreckageLayer.AddAction(new CCCallFunc(() => { wreckageLayer.InitWreckage(list); wreckageLayer.CheckForWelcome(); }));
        }

        internal override void UpdateCamera()
        {
            base.UpdateCamera();
            // additionally color the background and the planes according to the position of the center of the camera relative to the zones
            CCPoint camMid = CameraPosition + (CCPoint)CameraSize / 2;
            float radius = camMid.Length;
            var bgColor = RadiusToColor(radius, groundNotAir: true);
            var planeColor = RadiusToColor(radius, groundNotAir: false);
            // draw the bg lines
            DrawNode.Clear();
            const float bgsize = 600000f;
            const int lineCount = 200;
            var bgLineColor = CCColor4B.Lerp(planeColor, CCColor4B.Black, 0.75f);
            for (int i = -lineCount; i < lineCount; i++)
            {
                DrawNode.DrawLine(new CCPoint(i * bgsize / lineCount, -bgsize), new CCPoint(i * bgsize / lineCount, bgsize), 20f, bgLineColor);
                DrawNode.DrawLine(new CCPoint(-bgsize, i * bgsize / lineCount), new CCPoint(bgsize, i * bgsize / lineCount), 20f, bgLineColor);
            }
            // draw the bg color
            DrawNodeBG.Clear();
            DrawNodeBG.DrawRect(new CCRect(-999999999f, -999999999f, 999999999f * 2, 999999999f * 2), bgColor);
            // now the planes
            // only color the enemies
            foreach (var squadron in Squadrons)
                if (squadron.IsActive(this))
                    foreach (var aircraft in squadron.AircraftsWithRelPositions.Keys)
                        aircraft.ColorByPlayLayer(planeColor);
        }

        internal CCColor4B CurrentPlaneColor
        {
            get
            {
                CCPoint camMid = CameraPosition + (CCPoint)CameraSize / 2;
                float radius = camMid.Length;
                return RadiusToColor(radius, groundNotAir: false);
            }
        }

        private int powerUpCheckCountDown = 0;
        private const int PU_CHECK_COUNTDOWN = 10;

        internal List<CloudTailNode> ExplosionNodes { get; private set; } = new List<CloudTailNode>();

        public override void Update(float dt)
        {
            base.Update(dt);
            switch (State)
            {
                case GameState.PLANNING:
                    break;
                case GameState.EXECUTING_ORDERS:
                    {
                        TimeLeftExecutingOrders -= dt;
                        if (Aircraft.CloudFrameCountdown != 0) Aircraft.CloudFrameCountdown--;
                        else Aircraft.CloudFrameCountdown = Aircraft.CLOUD_FRAME_COUNTDOWN;
                        if (powerUpCheckCountDown != 0) powerUpCheckCountDown--;
                        else
                        {
                            powerUpCheckCountDown = PU_CHECK_COUNTDOWN;
                            // check if any powerup is close enough to be picked up
                            foreach (var aircraft in PlayerAircrafts)
                            {
                                if (aircraft.MyState == Aircraft.State.SHOT_DOWN) continue;
                                CCPoint aPos = aircraft.Position;
                                foreach (var powerUp in PowerUps)
                                {
                                    // only pick up power-ups that are close in x-y but also in z direction
                                    if (powerUp.VertexZ > -10f && CCPoint.Distance(powerUp.Position, aPos) < PowerUp.PICKUP_DISTANCE)
                                    {
                                        // pick up the powerup
                                        if (!PopUp.TriggeredPowerUp)
                                            PopUp.ShowPopUp(GUILayer, PopUp.Enum.TRIGGERED_POWERUP);
                                        powerUp.StartPickupAnimation();
                                        aircraft.ChangePowerUpCount(powerUp.Power, 1);
                                        PowerUps.Remove(powerUp);
                                        break;
                                    }
                                }
                            }
                        }
                        // DEBUG: Console.WriteLine("EXECUTING ORDERS; dt: " + dt);
                        // go through all aircrafts and let them execute their orders
                        List<Aircraft> aircraftToBeRemoved = new List<Aircraft>();
                        // first the enemies (organized into squadrons)
                        foreach (var squadron in Squadrons)
                            if (squadron.IsActive(this))
                            {
                                foreach (var aircraft in squadron.AircraftsWithRelPositions.Keys)
                                {
                                    aircraft.ExecuteOrders(dt);
                                    if (aircraft.ToBeRemoved)
                                        aircraftToBeRemoved.Add(aircraft);
                                }
                            }
                        // then the player aircrafts
                        foreach (var aircraft in PlayerAircrafts)
                        {
                            aircraft.ExecuteOrders(dt);
                            if (aircraft.ToBeRemoved)
                                aircraftToBeRemoved.Add(aircraft);
                        }
                        // remove aircrafts that have to be removed
                        foreach (var aircraft in aircraftToBeRemoved)
                        {
                            if (!aircraft.ControlledByPlayer)
                            {
                                // add a powerup
                                if ((new Random()).Next(3) < 2)
                                {
                                    var powerUp = aircraft.GeneratePowerUp();
                                    if (powerUp != null)
                                    {
                                        powerUp.Position = aircraft.Position;
                                        AddPowerUp(powerUp);
                                    }
                                }
                            }
                            // add an explosion
                            var cloudNode = new CloudTailNode();
                            cloudNode.AddCloud(new CircleCloud(aircraft.Position, 0, CCColor4B.White, true, aircraft.ContentSize.Width * 16, 6f));
                            ExplosionNodes.Add(cloudNode);

                            DownedAircrafts.Add(aircraft);
                            RemoveAircraft(aircraft);
                        }
                        // go through all projectiles and let them advance
                        // check whether a projectile needs to be removed
                        List<Projectile> projectilesToBeRemoved = new List<Projectile>();
                        foreach (var projectile in Projectiles)
                        {
                           projectile.Advance(dt);
                            if (projectile.CanBeRemoved()) projectilesToBeRemoved.Add(projectile);
                        }
                        // go through all clouds that are directly bounds to you
                        List<CloudTailNode> cloudsToBeRemoved = new List<CloudTailNode>();
                        foreach (var cloudNode in ExplosionNodes)
                        {
                            cloudNode.Advance(dt);
                            if (!cloudNode.HasClouds()) cloudsToBeRemoved.Add(cloudNode);
                        }
                        foreach (var cloudNode in cloudsToBeRemoved)
                            ExplosionNodes.Remove(cloudNode);
                        UpdateDrawNodes();
                        if (TimeLeftExecutingOrders <= 0)
                            StartPlanningPhase();
                    }
                    break;
            }
        }

        private void AddPowerUp(PowerUp powerUp)
        {
            PowerUps.Add(powerUp);
            AddChild(powerUp, -30);
        }

        internal bool PosIsActive(CCPoint position)
        {
            return ActiveChunks.Contains(PosToWorldChunk(position));
        }

        /// <summary>
        /// Draw everything that is supposed to be drawn by the DrawNode
        /// </summary>
        private void UpdateDrawNodes()
        {
            HighDrawNode.Clear();
            LowDrawNode.Clear();
            DrawNodeExplosions.Clear();
            // draw the projectiles
            foreach (var drawNodeUser in DrawNodeUsers)
                drawNodeUser.UseDrawNodes(HighDrawNode, LowDrawNode);
            // draw the clouds
            foreach (var cloudNode in ExplosionNodes)
                cloudNode.UseDrawNodes(DrawNodeExplosions, DrawNodeExplosions);
            // draw everything directly related to the aircrafts
            foreach (var aircraft in Aircrafts)
                if (PosIsActive(aircraft.Position))
                    aircraft.UseDrawNodes(HighDrawNode, LowDrawNode);
        }

        internal void TellFlightPathHeadsToShowHeadsOnly(Aircraft exceptedOne = null)
        {
            // tell all player aircrafts flight-path-heads to no longer draw their options (in case they do)
            // since something different than them was clicked
            foreach (var aircraft in PlayerAircrafts)
                if (aircraft != exceptedOne)
                    aircraft.FlightPathHeadOnly();
        }

        new void OnTouchesBegan(List<CCTouch> touches, CCEvent touchEvent)
        {
            base.OnTouchesBegan(touches, touchEvent);
            if (touches.Count > 0)
            {
                TellFlightPathHeadsToShowHeadsOnly();
            }
        }

        new private protected void OnTouchesEnded(List<CCTouch> touches, CCEvent touchEvent)
        {
            base.OnTouchesEnded(touches, touchEvent);
            if (touches.Count > 0)
            {
                //drawNode.Visible = true;
                //testAircraft.IsManeuverPolygonDrawn = true;
                //Console.WriteLine("Released: "+touches[0].Location.ToString());
            }
        }
    }
}

