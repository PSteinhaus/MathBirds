using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CocosSharp;

namespace CocosSharpMathGame
{
    internal abstract class PowerUp : GameObjectSprite
    {
        const float SCALE_DURATION = 2.5f;
        const float SCALE_NORMAL = Constants.STANDARD_SCALE * 1.5f;
        const float SCALE_DIFF = 0.75f;
        const float EASE_RATE = 2f;
        internal const float PICKUP_DISTANCE = 340f;
        const int ACTION_TAG = 45786622;
        internal PowerUp(string textureName, bool riseFromGround=true) : base(UIElement.spriteSheet.Frames.Find(_ => _.TextureFilename.Equals(textureName)))
        {
            // usually power-ups are dropped by exploding aircrafts that just hit the ground, so they should rise to the air visually
            if (riseFromGround)
            {
                const float RISE_DUR = 4f;
                VertexZ = Constants.VERTEX_Z_GROUND;
                AddAction(new CCCallFiniteTimeFunc(RISE_DUR, (prog, duration) => { VertexZ = (1 - prog * prog) * Constants.VERTEX_Z_GROUND; }));
            }
        }
        protected override void AddedToScene()
        {
            base.AddedToScene();
            MyRotation = -90f;
            // add a simple animation
            var action = new CCRepeatForever(new CCEaseInOut(new CCScaleTo(SCALE_DURATION, SCALE_NORMAL + SCALE_DIFF), EASE_RATE),
                                          new CCEaseInOut(new CCScaleTo(SCALE_DURATION, SCALE_NORMAL - SCALE_DIFF), EASE_RATE));
            action.Tag = ACTION_TAG;
            AddAction(action);
        }
        internal void StartPickupAnimation()
        {
            // stop the current animation
            StopAction(ACTION_TAG);
            // add a simple pickup animation
            const float PICKUP_DURATION = 1f;
            var action = new CCSequence(new CCEaseBackOut(new CCScaleTo(PICKUP_DURATION, SCALE_NORMAL * 2)),
                                        new CCSpawn(new CCFadeOut(1f), new CCScaleTo(1f, 0.00001f)),
                                        new CCRemoveSelf());
            action.Tag = ACTION_TAG;
            AddAction(action);
        }

        internal static FlightPathHeadOption FlightPathHeadOptionFromType(PowerType powUpT)
        {
            switch (powUpT)
            {
                case PowerType.NORMAL:
                    return new FlightPathHeadOption("flightPathHead.png", PowerType.NORMAL);
                case PowerType.SHIELD:
                    return new FlightPathHeadOption("shield.png", PowerType.SHIELD);
                case PowerType.BOOST:
                    return new FlightPathHeadOption("boost.png", PowerType.BOOST);
                case PowerType.HEAL:
                    return new FlightPathHeadOption("heal.png", PowerType.HEAL);
                case PowerType.BACK_TURN:
                    return new FlightPathHeadOption("backTurn.png", PowerType.BACK_TURN);
                default:
                    return null;
            }
        }

        internal enum PowerType : byte
        {
            NORMAL, SHIELD, BOOST, BACK_TURN, HEAL
        }
        internal abstract PowerType Power { get; }
        internal static PowerUp PowerUpFromType(PowerType pType)
        {
            switch (pType)
            {
                case PowerType.SHIELD:
                    return new PowerUpShield();
                case PowerType.BOOST:
                    return new PowerUpBoost();
                case PowerType.HEAL:
                    return new PowerUpHeal();
                case PowerType.BACK_TURN:
                    return new PowerUpBackTurn();
                default:
                    return null;
            }
        }
    }

    internal class PowerUpShield : PowerUp
    {
        internal override PowerType Power => PowerType.SHIELD;
        internal PowerUpShield() : base("shield.png")
        { }
    }
    internal class PowerUpBoost : PowerUp
    {
        internal override PowerType Power => PowerType.BOOST;
        internal PowerUpBoost() : base("boost.png")
        { }
    }
    internal class PowerUpHeal : PowerUp
    {
        internal override PowerType Power => PowerType.HEAL;
        internal PowerUpHeal() : base("heal.png")
        { }
    }
    internal class PowerUpBackTurn : PowerUp
    {
        internal override PowerType Power => PowerType.BACK_TURN;
        internal PowerUpBackTurn() : base("backTurn.png")
        { }
    }
}
