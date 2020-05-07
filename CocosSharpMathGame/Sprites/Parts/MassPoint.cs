using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CocosSharp;

namespace CocosSharpMathGame
{
    /// <summary>
    /// A point in space (2D) that is associated with a certain mass.
    /// </summary>
    internal struct MassPoint
    {
        internal CCPoint Position;
        internal float Mass;
        internal float X
        {
            get
            {
                return Position.X;
            }
            set
            {
                Position.X = value;
            }
        }
        internal float Y
        {
            get
            {
                return Position.Y;
            }
            set
            {
                Position.Y = value;
            }
        }

        internal MassPoint(float x, float y, float mass)
        {
            Position = new CCPoint(x, y);
            Mass = mass;
        }
    }
}
