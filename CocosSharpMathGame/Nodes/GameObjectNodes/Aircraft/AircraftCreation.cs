using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CocosSharp;
using MathNet.Numerics.Random;
using Symbolism;

namespace CocosSharpMathGame
{
    internal partial class Aircraft
    {
        internal static Aircraft CreateTestAircraft(int numWeapons = 2, bool withChallenges = true)
        {
            Aircraft aircraft = new Aircraft();
            //IsManeuverPolygonDrawn = true;
            aircraft.Body = new TestBody();
            // mount the wings
            var wings = new TestDoubleWing();
            aircraft.Body.MountPart(wings);
            // mount the rotor
            aircraft.Body.MountPart(new TestRotor());
            // and the rudders
            aircraft.Body.MountPart(new TestRudder());
            aircraft.Body.MountPart(new TestRudder());
            //Body.MountPart(new TestEngineStrong());
            var rng = new Random();
            if (numWeapons == 1)
            {
                if (rng.NextBoolean())
                    wings.MountPart(0, new TestWeapon());
                else
                    wings.MountPart(1, new TestWeapon());
            }
            else if (numWeapons == 2)
            {
                wings.MountPart(new TestWeapon());
                wings.MountPart(new TestWeapon());
            }
            // set the math challenges
            if (withChallenges)
                aircraft.WeightedChallenges = new List<Tuple<int, MathChallenge>> { new Tuple<int, MathChallenge>(1, new MultiplyChallenge(4, 2, -20, 5, dummy: true)),
                                                                                    new Tuple<int, MathChallenge>(1, new DivideChallenge(dummy: true)) };
            // set the awarded powerups
            if (withChallenges)
                aircraft.WeightedPowerUpsAwarded = new Dictionary<PowerUp.PowerType, int> { { PowerUp.PowerType.HEAL, 1 }, { PowerUp.PowerType.SHIELD, 4 }, { PowerUp.PowerType.BACK_TURN, 5 }, { PowerUp.PowerType.BOOST, 5 } };
            return aircraft;
            /*
            foreach (var part in TotalParts)
                Console.WriteLine("Part bounding box: "+part.BoundingBoxTransformedToParent);

            // lets create some maneuver polygon
            var d = ScaledContentSize.Width * 0.5f; // use some relative length as measure
            var reach = 3;
            var controlPoints = new CCPoint[7];
            controlPoints[0] = new CCPoint(3*d, 4*d);
            controlPoints[1] = new CCPoint(3*d *reach, 4*d * reach);
            var angle = controlPoints[1].Angle;
            controlPoints[2] = CCPoint.RotateByAngle(controlPoints[1], CCPoint.Zero, -angle*0.5f);
            controlPoints[3] = CCPoint.RotateByAngle(controlPoints[1], CCPoint.Zero, -angle);
            controlPoints[4] = CCPoint.RotateByAngle(controlPoints[1], CCPoint.Zero, -angle*1.5f);
            controlPoints[5] = new CCPoint(3*d * reach, -4 * d * reach);
            controlPoints[6] = new CCPoint(3*d, -4 * d);
            var maneuverPolygon = new PolygonWithSplines(controlPoints);
            maneuverPolygon.SpecifySpline(1, 5, 25);
            maneuverPolygon.ConstructPolygon();
            */
            /*
            var controlPoints = new CCPoint[]
            { new CCPoint(2*d, 3*d), new CCPoint(0, 3.5f*d), new CCPoint(-2*d, 3*d),
              new CCPoint(-d, d), new CCPoint(0,d*1.2f), new CCPoint(d, d) };
            var maneuverPolygon = new PolygonWithSplines(controlPoints);
            maneuverPolygon.SpecifySpline(0, 2, 15);
            maneuverPolygon.SpecifySpline(3, 5, 15);
            maneuverPolygon.ConstructPolygon();
            // turn it 90 degrees because 0° means EAST now...
            maneuverPolygon.RotateBy(90f);
            */
            /*
            // now let's try a MASSIVE polygon, allowing the plane to move almost anywhere
            var d = ScaledContentSize.Width * 1.5f; // use some relative length as measure
            var controlPoints = new CCPoint[]
            { new CCPoint(-10*d, -10*d), new CCPoint(-10*d, 10*d), new CCPoint(10*d, 10*d), new CCPoint(10*d, -10*d) };
            var maneuverPolygon = new PolygonWithSplines(controlPoints);
            */

            //UpdateManeuverPolygonToThis(maneuverPolygon);
            // this is a test plane so I want to see the polygon too
            //IsManeuverPolygonDrawn = true;

            // ok... since this doesn't work lets try to draw it like this:
            //var drawNode = maneuverPolygon.CreateDrawNode();
            //AddChild(drawNode);
            //drawNode.Scale = 1 / Constants.STANDARD_SCALE;
        }

        internal static Aircraft CreateBalloon(bool withWeapon = true)
        {
            Aircraft aircraft = new Aircraft();
            aircraft.Body = new BodyBalloon();
            // mount the rotors
            aircraft.Body.MountPart(new RotorBalloon());
            aircraft.Body.MountPart(new RotorBalloon());
            if (withWeapon)
            {
                aircraft.Body.MountPart(new WeaponBalloon());
            }
            // set the math challenges
            aircraft.WeightedChallenges = new List<Tuple<int, MathChallenge>> { new Tuple<int, MathChallenge>(1, new MultiplyChallenge(4, 3, -5, 9, dummy: true)),
                                                                                new Tuple<int, MathChallenge>(1, new DivideChallenge(dummy: true)) };
            // set the awarded powerups
            aircraft.WeightedPowerUpsAwarded = new Dictionary<PowerUp.PowerType, int> { { PowerUp.PowerType.SHIELD, 3 }, { PowerUp.PowerType.BACK_TURN, 2 }, { PowerUp.PowerType.BOOST, 3 } };
            return aircraft;
        }

        internal static Aircraft CreateBat(bool withWeapon = true)
        {
            Aircraft aircraft = new Aircraft();
            aircraft.Body = new BodyBat();
            // mount the wings
            aircraft.Body.MountPart(new WingBat());
            aircraft.Body.MountPart(new WingBat());
            // mount the rotor and the weapon
            aircraft.Body.MountPart(new RotorBat());
            if (withWeapon)
            {
                aircraft.Body.MountPart(new WeaponBat());
            }
            // mount the rudders
            aircraft.Body.MountPart(new RudderPotato());
            aircraft.Body.MountPart(new RudderPotato());
            // set the math challenges
            var rng = new Random();
            aircraft.WeightedChallenges = new List<Tuple<int, MathChallenge>> { new Tuple<int, MathChallenge>(1, new AddChallenge(4, rng.Next(2,4), 0, 51, dummy: true)),
                                                                                new Tuple<int, MathChallenge>(1, new SubChallenge(4, 2, 0, 50, dummy: true)) };
            // set the awarded powerups
            aircraft.WeightedPowerUpsAwarded = new Dictionary<PowerUp.PowerType, int> { { PowerUp.PowerType.SHIELD, 1 }, { PowerUp.PowerType.BACK_TURN, 5 }, { PowerUp.PowerType.BOOST, 6 } };
            return aircraft;
        }

        internal static Aircraft CreatePotato(bool withWeapon = false)
        {
            Aircraft aircraft = new Aircraft();
            aircraft.Body = new BodyPotato();
            // mount the wings
            aircraft.Body.MountPart(new WingPotato());
            aircraft.Body.MountPart(new WingPotato());
            // mount the rotor or the weapon
            if (withWeapon)
            {
                aircraft.Body.MountPart(new WeaponPotato());
            }
            else
            {
                aircraft.Body.MountPart(new RotorPotato());
            }
            // mount the rudders
            aircraft.Body.MountPart(new RudderPotato());
            aircraft.Body.MountPart(new RudderPotato());
            // set the math challenges
            var rng = new Random();
            aircraft.WeightedChallenges = new List<Tuple<int, MathChallenge>> { new Tuple<int, MathChallenge>(1, new AddChallenge(4, 2, 0, 81, dummy: true)),
                                                                                new Tuple<int, MathChallenge>(1, new SubChallenge(4, rng.Next(2,4), 0, 50, dummy: true)) };
            // set the awarded powerups
            aircraft.WeightedPowerUpsAwarded = new Dictionary<PowerUp.PowerType, int> { { PowerUp.PowerType.SHIELD, 1 }, { PowerUp.PowerType.BACK_TURN, 9 }, { PowerUp.PowerType.BOOST, 7 } };
            return aircraft;
        }

        internal static Aircraft CreateBigBomber(bool withWeapon = true)
        {
            Aircraft aircraft = new Aircraft();
            aircraft.Body = new BodyBigBomber();
            // mount the wings
            var wing1 = new WingBigBomber();
            var wing2 = new WingBigBomber();
            aircraft.Body.MountPart(wing1);
            aircraft.Body.MountPart(wing2);
            // mount the weapon
            if (withWeapon)
            {
                aircraft.Body.MountPart(new WeaponBigBomber());
            }
            // mount 4 rotors (2 per wing)
            wing1.MountPart(new RotorBigBomber());
            wing1.MountPart(new RotorBigBomber());
            wing2.MountPart(new RotorBigBomber());
            wing2.MountPart(new RotorBigBomber());
            // mount the rudders
            aircraft.Body.MountPart(new RudderBigBomber());
            aircraft.Body.MountPart(new RudderBigBomber());
            // set the math challenges
            aircraft.WeightedChallenges = new List<Tuple<int, MathChallenge>> { new Tuple<int, MathChallenge>(1, new SolveChallenge(dummy: true)) };
            // set the awarded powerups
            aircraft.WeightedPowerUpsAwarded = new Dictionary<PowerUp.PowerType, int> { { PowerUp.PowerType.HEAL, 3 }, { PowerUp.PowerType.SHIELD, 2 }, { PowerUp.PowerType.BOOST, 1 } };
            return aircraft;
        }

        internal static Aircraft CreateFighter(int numWeapons = 2)
        {
            Aircraft aircraft = new Aircraft();
            aircraft.Body = new BodyFighter();
            // mount the wings
            var wing1 = new WingFighter();
            var wing2 = new WingFighter();
            aircraft.Body.MountPart(wing1);
            aircraft.Body.MountPart(wing2);
            // mount the weapons
            var rng = new Random();
            if (numWeapons == 1)
            {
                if (rng.NextBoolean())
                    wing1.MountPart(new WeaponFighter());
                else
                    wing2.MountPart(new WeaponFighter());
            }
            else if (numWeapons == 2)
            {
                wing1.MountPart(new WeaponFighter());
                wing2.MountPart(new WeaponFighter());
            }
            // mount the rotor
            aircraft.Body.MountPart(new RotorFighter());
            // mount the rudders
            aircraft.Body.MountPart(new RudderFighter());
            aircraft.Body.MountPart(new RudderFighter());
            // set the math challenges
            aircraft.WeightedChallenges = new List<Tuple<int, MathChallenge>> { new Tuple<int, MathChallenge>(1, new SolveChallenge(dummy: true)),
                                                                                new Tuple<int, MathChallenge>(1, new MultiplyChallenge(dummy: true))};
            // set the awarded powerups
            aircraft.WeightedPowerUpsAwarded = new Dictionary<PowerUp.PowerType, int> { { PowerUp.PowerType.HEAL, 1 }, { PowerUp.PowerType.SHIELD, 3 }, { PowerUp.PowerType.BACK_TURN, 2 }, { PowerUp.PowerType.BOOST, 3 } };
            return aircraft;
        }

        internal static Aircraft CreateJet()
        {
            Aircraft aircraft = new Aircraft();
            aircraft.Body = new BodyJet();
            // mount the wings
            aircraft.Body.MountPart(new WingJet());
            aircraft.Body.MountPart(new WingJet());
            // mount the rudders
            aircraft.Body.MountPart(new RudderJet());
            aircraft.Body.MountPart(new RudderJet());
            // set the math challenges
            aircraft.WeightedChallenges = new List<Tuple<int, MathChallenge>> { new Tuple<int, MathChallenge>(1, new SolveChallenge(4, 1, 20, dummy: true)) };
            // set the awarded powerups
            aircraft.WeightedPowerUpsAwarded = new Dictionary<PowerUp.PowerType, int> { { PowerUp.PowerType.SHIELD, 3 }, { PowerUp.PowerType.BACK_TURN, 2 }, { PowerUp.PowerType.BOOST, 3 } };
            return aircraft;
        }

        internal PowerUp GeneratePowerUp()
        {
            Console.WriteLine("CALLED GENERATOR!");
            if (WeightedPowerUpsAwarded != null && WeightedPowerUpsAwarded.Any())
            {
                // get the weight sum
                int sum = WeightedPowerUpsAwarded.Values.Sum();
                // choose
                int choice = new Random().Next(sum);
                sum = 0;
                foreach (var powerUpTuple in WeightedPowerUpsAwarded)
                {
                    if (choice <= sum)
                        return PowerUp.PowerUpFromType(powerUpTuple.Key);
                    sum += powerUpTuple.Value;
                }
            }
            return null;
        }

        internal void FlightPathHeadOnly()
        {
            FlightPathControlNode.FlightPathHeadOnly();
        }
    }
}
