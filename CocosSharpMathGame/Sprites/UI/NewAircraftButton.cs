using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CocosSharp;

namespace CocosSharpMathGame
{
    internal class NewAircraftButton : Button
    {
        private HangarLayer HangarLayer { get; set; }
        internal NewAircraftButton(HangarLayer hangarLayer) : base("newAircraftButton.png", true)
        {
            HangarLayer = hangarLayer;
            RadiusFactor = 0.75f;
        }

        private protected override void ButtonEnded(CCTouch touch)
        {
            // add a new aircraft and start the transition towards the aircraft creation stage
            HangarLayer.ModifyNewAircraft();
        }
    }
}
