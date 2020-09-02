using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using CocosSharp;
using MathNet.Numerics.Random;
using Symbolism;

namespace CocosSharpMathGame
{
    internal partial class Aircraft
    {
        /// <summary>
        /// Randomly generates the answer to the question: should the next aircraft be a shiny?
        /// </summary>
        /// <returns></returns>
        internal static bool ShinyCoinThrow(Random rng)
        {
            return rng.Next(1000) < 5;
        }
        internal static Aircraft CreateTestAircraft(int numWeapons = 2, bool withChallenges = true, Random rng = null)
        {
            Aircraft aircraft = new Aircraft();
            //IsManeuverPolygonDrawn = true;
            if (rng != null && ShinyCoinThrow(rng))
            {
                aircraft.Body = new TestBodyShiny();
                // mount the wings
                var wings = new TestDoubleWingShiny();
                aircraft.Body.MountPart(wings);
                // mount the rotor
                aircraft.Body.MountPart(new TestRotorShiny());
                // and the rudders
                aircraft.Body.MountPart(new TestRudderShiny());
                aircraft.Body.MountPart(new TestRudderShiny());
                wings.MountPart(new TestWeaponShiny());
                wings.MountPart(new TestWeaponShiny());
            }
            else
            {
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
                if (rng == null)
                    rng = new Random();
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
            }
            // set the math challenges
            if (withChallenges)
                aircraft.WeightedChallenges = new List<Tuple<int, MathChallenge>> { new Tuple<int, MathChallenge>(1, new MultiplyChallenge(4, 2, -20, 5, dummy: true)),
                                                                                    new Tuple<int, MathChallenge>(1, new DivideChallenge(dummy: true)) };
            // set the awarded powerups
            if (withChallenges)
                aircraft.WeightedPowerUpsAwarded = new Dictionary<PowerUp.PowerType, int> { { PowerUp.PowerType.HEAL, 1 }, { PowerUp.PowerType.SHIELD, 4 }, { PowerUp.PowerType.BACK_TURN, 5 }, { PowerUp.PowerType.BOOST, 5 } };
            return aircraft;
        }

        internal static Aircraft CreateBalloon(bool withWeapon = true, Random rng = null)
        {
            Aircraft aircraft = new Aircraft();
            if (rng != null && ShinyCoinThrow(rng))
            {
                aircraft.Body = new BodyBalloonShiny();
                // mount the rotors
                aircraft.Body.MountPart(new RotorBalloonShiny());
                aircraft.Body.MountPart(new RotorBalloonShiny());
                aircraft.Body.MountPart(new WeaponBalloonShiny());
            }
            else
            {
                aircraft.Body = new BodyBalloon();
                // mount the rotors
                aircraft.Body.MountPart(new RotorBalloon());
                aircraft.Body.MountPart(new RotorBalloon());
                if (withWeapon)
                {
                    aircraft.Body.MountPart(new WeaponBalloon());
                }
            }
            // set the math challenges
            aircraft.WeightedChallenges = new List<Tuple<int, MathChallenge>> { new Tuple<int, MathChallenge>(1, new MultiplyChallenge(4, 3, -5, 9, dummy: true)),
                                                                                new Tuple<int, MathChallenge>(1, new DivideChallenge(dummy: true)) };
            // set the awarded powerups
            aircraft.WeightedPowerUpsAwarded = new Dictionary<PowerUp.PowerType, int> { { PowerUp.PowerType.SHIELD, 3 }, { PowerUp.PowerType.BACK_TURN, 2 }, { PowerUp.PowerType.BOOST, 3 } };
            return aircraft;
        }

        internal static Aircraft CreateBat(bool withWeapon = true, Random rng = null)
        {
            Aircraft aircraft = new Aircraft();
            if (rng != null && ShinyCoinThrow(rng))
            {
                aircraft.Body = new BodyBatShiny();
                // mount the wings
                aircraft.Body.MountPart(new WingBatShiny());
                aircraft.Body.MountPart(new WingBatShiny());
                // mount the rotor and the weapon
                aircraft.Body.MountPart(new RotorBatShiny());
                aircraft.Body.MountPart(new WeaponBatShiny());
                // mount the rudders
                aircraft.Body.MountPart(new RudderBatShiny());
                aircraft.Body.MountPart(new RudderBatShiny());
            }
            else
            {
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
            }
            // set the math challenges
            if (rng == null)
                rng = new Random();
            aircraft.WeightedChallenges = new List<Tuple<int, MathChallenge>> { new Tuple<int, MathChallenge>(1, new AddChallenge(4, rng.Next(2,4), 0, 51, dummy: true)),
                                                                                new Tuple<int, MathChallenge>(1, new SubChallenge(4, 2, 0, 50, dummy: true)) };
            // set the awarded powerups
            aircraft.WeightedPowerUpsAwarded = new Dictionary<PowerUp.PowerType, int> { { PowerUp.PowerType.SHIELD, 1 }, { PowerUp.PowerType.BACK_TURN, 5 }, { PowerUp.PowerType.BOOST, 6 } };
            return aircraft;
        }

        internal static Aircraft CreatePotato(bool withWeapon = false, Random rng = null)
        {
            Aircraft aircraft = new Aircraft();
            if (rng != null && ShinyCoinThrow(rng))
            {
                aircraft.Body = new BodyPotatoShiny();
                // mount the wings
                aircraft.Body.MountPart(new WingPotatoShiny());
                aircraft.Body.MountPart(new WingPotatoShiny());
                // mount the rotor or the weapon
                if (withWeapon)
                {
                    aircraft.Body.MountPart(new WeaponPotatoShiny());
                }
                else
                {
                    aircraft.Body.MountPart(new RotorPotatoShiny());
                }
                // mount the rudders
                aircraft.Body.MountPart(new RudderPotatoShiny());
                aircraft.Body.MountPart(new RudderPotatoShiny());
            }
            else
            {
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
            }
            // set the math challenges
            if (rng == null)
                rng = new Random();
            aircraft.WeightedChallenges = new List<Tuple<int, MathChallenge>> { new Tuple<int, MathChallenge>(1, new AddChallenge(4, 2, 0, 81, dummy: true)),
                                                                                new Tuple<int, MathChallenge>(1, new SubChallenge(4, rng.Next(2,4), 0, 50, dummy: true)) };
            // set the awarded powerups
            aircraft.WeightedPowerUpsAwarded = new Dictionary<PowerUp.PowerType, int> { { PowerUp.PowerType.SHIELD, 1 }, { PowerUp.PowerType.BACK_TURN, 9 }, { PowerUp.PowerType.BOOST, 7 } };
            return aircraft;
        }

        internal static Aircraft CreateBigBomber(bool withWeapon = true, Random rng = null)
        {
            Aircraft aircraft = new Aircraft();
            if (rng != null && ShinyCoinThrow(rng))
            {
                aircraft.Body = new BodyBigBomberShiny();
                // mount the wings
                var wing1 = new WingBigBomberShiny();
                var wing2 = new WingBigBomberShiny();
                aircraft.Body.MountPart(wing1);
                aircraft.Body.MountPart(wing2);
                // mount the weapon
                aircraft.Body.MountPart(new WeaponBigBomberShiny());
                // mount 4 rotors (2 per wing)
                wing1.MountPart(new RotorBigBomberShiny());
                wing1.MountPart(new RotorBigBomberShiny());
                wing1.MountPart(new RotorBigBomberShiny());
                wing2.MountPart(new RotorBigBomberShiny());
                wing2.MountPart(new RotorBigBomberShiny());
                wing2.MountPart(new RotorBigBomberShiny());
                // mount the rudders
                aircraft.Body.MountPart(new RudderBigBomberShiny());
                aircraft.Body.MountPart(new RudderBigBomberShiny());
            }
            else
            {
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
            }
            // set the math challenges
            aircraft.WeightedChallenges = new List<Tuple<int, MathChallenge>> { new Tuple<int, MathChallenge>(1, new SolveChallenge(dummy: true)) };
            // set the awarded powerups
            aircraft.WeightedPowerUpsAwarded = new Dictionary<PowerUp.PowerType, int> { { PowerUp.PowerType.HEAL, 3 }, { PowerUp.PowerType.SHIELD, 2 }, { PowerUp.PowerType.BOOST, 1 } };
            return aircraft;
        }

        internal static Aircraft CreateFighter(int numWeapons = 2, Random rng = null)
        {
            Aircraft aircraft = new Aircraft();
            if (rng != null && ShinyCoinThrow(rng))
            {
                aircraft.Body = new BodyFighterShiny();
                // mount the wings
                var wing1 = new WingFighterShiny();
                var wing2 = new WingFighterShiny();
                aircraft.Body.MountPart(wing1);
                aircraft.Body.MountPart(wing2);
                // mount the weapons
                wing1.MountPart(new WeaponFighterShiny());
                wing2.MountPart(new WeaponFighterShiny());
                // mount the rotor
                aircraft.Body.MountPart(new RotorFighterShiny());
                // mount the rudders
                aircraft.Body.MountPart(new RudderFighterShiny());
                aircraft.Body.MountPart(new RudderFighterShiny());
            }
            else
            {
                aircraft.Body = new BodyFighter();
                // mount the wings
                var wing1 = new WingFighter();
                var wing2 = new WingFighter();
                aircraft.Body.MountPart(wing1);
                aircraft.Body.MountPart(wing2);
                // mount the weapons
                if (rng == null)
                    rng = new Random();
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
            }
            // set the math challenges
            aircraft.WeightedChallenges = new List<Tuple<int, MathChallenge>> { new Tuple<int, MathChallenge>(1, new SolveChallenge(dummy: true)),
                                                                                new Tuple<int, MathChallenge>(1, new MultiplyChallenge(dummy: true))};
            // set the awarded powerups
            aircraft.WeightedPowerUpsAwarded = new Dictionary<PowerUp.PowerType, int> { { PowerUp.PowerType.HEAL, 1 }, { PowerUp.PowerType.SHIELD, 3 }, { PowerUp.PowerType.BACK_TURN, 2 }, { PowerUp.PowerType.BOOST, 3 } };
            return aircraft;
        }

        internal static Aircraft CreateJet(Random rng = null)
        {
            Aircraft aircraft = new Aircraft();
            if (rng != null && ShinyCoinThrow(rng))
            {
                // create an angel!
                aircraft.Body = new BodyJet();
                // mount the wings
                aircraft.Body.MountPart(new WingAngel());
                aircraft.Body.MountPart(new WingAngel());
                // mount the rudders
                aircraft.Body.MountPart(new RudderJet());
                aircraft.Body.MountPart(new RudderJet());
            }
            else
            {
                aircraft.Body = new BodyJet();
                // mount the wings
                aircraft.Body.MountPart(new WingJet());
                aircraft.Body.MountPart(new WingJet());
                // mount the rudders
                aircraft.Body.MountPart(new RudderJet());
                aircraft.Body.MountPart(new RudderJet());
            }
            // set the math challenges
            aircraft.WeightedChallenges = new List<Tuple<int, MathChallenge>> { new Tuple<int, MathChallenge>(1, new SolveChallenge(4, 1, 20, dummy: true)) };
            // set the awarded powerups
            aircraft.WeightedPowerUpsAwarded = new Dictionary<PowerUp.PowerType, int> { { PowerUp.PowerType.SHIELD, 3 }, { PowerUp.PowerType.BACK_TURN, 2 }, { PowerUp.PowerType.BOOST, 3 } };
            return aircraft;
        }

        internal PowerUp GeneratePowerUp()
        {
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
