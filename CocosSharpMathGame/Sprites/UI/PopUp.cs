using CocosSharp;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace CocosSharpMathGame
{
    internal class PopUp : VerticalScalingButton
    {
        internal static bool TriggeredWelcome = false;
        internal static bool TriggeredAssembly = false;
        internal static bool TriggeredScrapyard = false;
        internal static bool TriggeredPowerUp = false;
        internal static bool TriggeredWreckLayer = false;
        internal static bool TriggeredPlayLayer = false;
        internal static bool TriggeredNewAircraft = false;
        internal static bool TriggeredZoom = false;

        private string text;
        private protected CCLabel label;
        private protected CCLabel label2;
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
                const float BORDER = 100f;
                FitToHeight(label.ContentSize.Height + BORDER);
                label.Position = (CCPoint)ContentSize / 2 + new CCPoint(0,20f);
            }
        }
        internal PopUp(string text) : base("popUpStart.png", "popUpMid.png", "popUpEnd.png")
        {
            Scale = 2.3f;
            AnchorPoint = CCPoint.AnchorMiddle;

            label = new CCLabel("", "EarlyGameBoy", 12, CCLabelFormat.SpriteFont);
            label.Scale = 0.95f;
            AddChild(label);
            label.IsAntialiased = false;
            label.Color = CCColor3B.White;
            label.AnchorPoint = CCPoint.AnchorMiddle;
            label.HorizontalAlignment = CCTextAlignment.Center;
            label.LineBreak = CCLabelLineBreak.Character;
            label.LineHeight = 30f;
            Text = text;

            label2 = new CCLabel("Tap to continue", "EarlyGameBoy", 12, CCLabelFormat.SpriteFont);
            AddChild(label2);
            label2.IsAntialiased = false;
            label2.Color = CCColor3B.White;
            label2.AnchorPoint = CCPoint.AnchorMiddleBottom;
            label2.HorizontalAlignment = CCTextAlignment.Center;
            label2.Scale = 0.75f;
            label2.Position = new CCPoint(ContentSize.Width / 2, StartSprite.ContentSize.Height * 3);

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
                label2.Color = CCColor3B.Gray;
            }
        }

        internal event EventHandler ClickedEvent;

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
                label2.Color = CCColor3B.White;
                var touch = touches[0];
                if (TouchIsOnIt(touch))
                {
                    // move the popUp out of view and remove it
                    RemovePerAction();
                    Pressable = false;
                    ClickedEvent?.Invoke(this, EventArgs.Empty);
                }
            }
        }

        const float MOVE_DURATION = 0.35f;
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
        internal static PopUp ShowPopUp(CCLayer layerToShowOn, string text)
        {
            var popUp = new PopUp(text);
            layerToShowOn.AddChild(popUp, int.MaxValue);
            var bounds = layerToShowOn.VisibleBoundsWorldspace;
            popUp.Position = new CCPoint(bounds.Center.X, -bounds.Center.Y);
            // move in
            popUp.AddAction(new CCEaseOut(new CCMoveTo(MOVE_DURATION, bounds.Center), MOVE_EASE_RATE));
            return popUp;
        }

        internal static PopUp ShowPopUp(CCLayer layerToShowOn, Enum en)
        {
            string text = null;
            switch (en)
            {
                case Enum.STOP:
                default:
                    return null;
                case Enum.TRIGGERED_WELCOME:
                    // ß is no visible char in the early-gameboy font; this is a workaround to allow for double linebreaks;
                    text = "Welcome to\nMathBirds!\nß\nThis is your hangar.\nß\nPlace your aircrafts\non the bar below\nto start playing.";
                    TriggeredWelcome = true;
                    break;
                case Enum.TRIGGERED_PLAYLAYER:
                    text = "MathBirds is a\nturn based game.\nß\nPress the button\non the top left\nto start the\nround as soon\nas you're ready.\nß\nGood luck.";
                    TriggeredPlayLayer = true;
                    break;
                case Enum.TRIGGERED_ASSEMBLY:
                    text = "This is the assembly.\nß\nYou can modify\nyour aircrafts here.\nAnd you can also\ncreate new ones!";
                    TriggeredAssembly = true;
                    break;
                case Enum.TRIGGERED_SCRAPYARD:
                    text = "This is the scrapyard.\nß\nFrom time to time\nspare parts may\nend up here,\nwaiting to be found.\nß\nAlso, all kinds of\nmath challenges\nwhich have yet\nbeen discovered\ncan be found\nat this place.";
                    TriggeredScrapyard = true;
                    break;
                case Enum.TRIGGERED_POWERUP:
                    text = "An aircraft has\njust picked up\na power-up.\nß\nPress on its\ncursor to access\nall available\npower-ups";
                    TriggeredPowerUp = true;
                    break;
                case Enum.TRIGGERED_WRECKAGELAYER:
                    text = "Well done.\nß\nAfter each flight\nyou have the chance\nto access the wrecks\nof all aircrafts\nthat have been\nshot down.";
                    TriggeredWreckLayer = true;
                    break;
                case Enum.TRIGGERED_WRECKAGELAYER2:
                    text = "You may be able\nto obtain some of\ntheir parts by\nsalvaging them.\nß\nYou may also try\nto repair them first,\n" +
                           "but be warned:\nif you choose to\nrepair, proceed\nwith care.";
                    break;
                case Enum.TRIGGERED_SLOTUNLOCK:
                    text = "Congratulations!\nß\nYou've just unlocked\na new plane slot.\nß\nNow you can lead\nup to " + HangarLayer.UnlockedPlaneSlots + " aircrafts\ninto battle!";
                    break;
                case Enum.TRIGGERED_NEWAIRCRAFT:
                    text = "First select\nthe body of\nyour new aircraft.\nß\nTo do so,\nscroll to the\ntop category and\nselect one of the\navailable bodies.";
                    TriggeredNewAircraft = true;
                    break;
                case Enum.TRIGGERED_ZOOM:
                    text = "You can also\nzoom in and out,\nusing two fingers.\nß\nTry it right now\nand gain some\nmore oversight over\nthe battlefield.";
                    TriggeredZoom = true;
                    break;
            }
            return ShowPopUp(layerToShowOn, text);
        }

        internal enum Enum : byte
        {
            STOP = 0, TRIGGERED_WELCOME = 1, TRIGGERED_ASSEMBLY = 2, TRIGGERED_SCRAPYARD = 3,
            TRIGGERED_POWERUP = 4, TRIGGERED_WRECKAGELAYER = 5, TRIGGERED_SLOTUNLOCK = 6,
            TRIGGERED_PLAYLAYER = 7, TRIGGERED_WRECKAGELAYER2 = 8, TRIGGERED_NEWAIRCRAFT = 9,
            TRIGGERED_ZOOM = 10
        }
        public static void WriteToStream(BinaryWriter writer)
        {
            // write down which popups were already triggered
            writer.Write((byte)Enum.TRIGGERED_WELCOME);
            writer.Write(TriggeredWelcome);
            writer.Write((byte)Enum.TRIGGERED_ASSEMBLY);
            writer.Write(TriggeredAssembly);
            writer.Write((byte)Enum.TRIGGERED_SCRAPYARD);
            writer.Write(TriggeredScrapyard);
            writer.Write((byte)Enum.TRIGGERED_PLAYLAYER);
            writer.Write(TriggeredPlayLayer);
            writer.Write((byte)Enum.TRIGGERED_POWERUP);
            writer.Write(TriggeredPowerUp);
            writer.Write((byte)Enum.TRIGGERED_WRECKAGELAYER);
            writer.Write(TriggeredWreckLayer);
            writer.Write((byte)Enum.TRIGGERED_NEWAIRCRAFT);
            writer.Write(TriggeredNewAircraft);
            writer.Write((byte)Enum.TRIGGERED_ZOOM);
            writer.Write(TriggeredZoom);

            writer.Write((byte)Enum.STOP);
        }

        public static void CreateFromStream(BinaryReader reader, bool keepCurrent)
        {
            bool reading = true;
            bool triggered;
            while (reading)
            {
                Enum nextEnum = (Enum)reader.ReadByte();
                switch (nextEnum)
                {
                    case Enum.TRIGGERED_WELCOME:
                        {
                            triggered = reader.ReadBoolean();
                            if (!keepCurrent)
                                TriggeredWelcome = triggered;
                        }
                        break;
                    case Enum.TRIGGERED_ASSEMBLY:
                        {
                            triggered = reader.ReadBoolean();
                            if (!keepCurrent)
                                TriggeredAssembly = triggered;
                        }
                        break;
                    case Enum.TRIGGERED_SCRAPYARD:
                        {
                            triggered = reader.ReadBoolean();
                            if (!keepCurrent)
                                TriggeredScrapyard = triggered;
                        }
                        break;
                    case Enum.TRIGGERED_PLAYLAYER:
                        {
                            triggered = reader.ReadBoolean();
                            if (!keepCurrent)
                                TriggeredPlayLayer = triggered;
                        }
                        break;
                    case Enum.TRIGGERED_POWERUP:
                        {
                            triggered = reader.ReadBoolean();
                            if (!keepCurrent)
                                TriggeredPowerUp = triggered;
                        }
                        break;
                    case Enum.TRIGGERED_WRECKAGELAYER:
                        {
                            triggered = reader.ReadBoolean();
                            if (!keepCurrent)
                                TriggeredWreckLayer = triggered;
                        }
                        break;
                    case Enum.TRIGGERED_NEWAIRCRAFT:
                        {
                            triggered = reader.ReadBoolean();
                            if (!keepCurrent)
                                TriggeredNewAircraft = triggered;
                        }
                        break;
                    case Enum.TRIGGERED_ZOOM:
                        {
                            triggered = reader.ReadBoolean();
                            if (!keepCurrent)
                                TriggeredZoom = triggered;
                        }
                        break;
                    case Enum.STOP:
                    default:
                        reading = false;
                        break;
                }
            }
        }
    }
}
