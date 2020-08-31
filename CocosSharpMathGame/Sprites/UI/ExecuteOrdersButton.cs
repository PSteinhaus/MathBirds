using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CocosSharp;

namespace CocosSharpMathGame
{
    internal class ExecuteOrdersButton : Button
    {
        internal ExecuteOrdersButton() : base("flightPathHead.png", true)
        {
            Scale = 6f;
            RadiusFactor = 0.7f;
        }

        private protected override void ButtonEnded(CCTouch touch)
        {
            Visible = false;
            // execute orders
            (Layer as GUILayer).ExecuteOrders();
        }
    }
}
