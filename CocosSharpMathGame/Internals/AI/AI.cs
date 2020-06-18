using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CocosSharpMathGame
{
    /// <summary>
    /// Aircraft Artificial Intelligence
    /// </summary>
    internal abstract class AI : IStreamSaveable
    {
        /// <summary>
        /// the aircraft that this AI belongs to
        /// </summary>
        internal Aircraft Aircraft { get; set; }
        internal AI()
        {
        }
        /// <summary>
        /// Do your move.
        /// </summary>
        /// <param name="aircrafts">The list of all aircrafts in the level</param>
        internal abstract void ActInPlanningPhase(IEnumerable<Aircraft> aircrafts);

        protected enum StreamEnum : byte
        {
            STOP = 0, NAME = 1
        }
        public void WriteToStream(BinaryWriter writer)
        {
            // write the type name
            writer.Write((byte)StreamEnum.NAME);
            writer.Write(GetType().AssemblyQualifiedName);
            writer.Write((byte)StreamEnum.STOP);
        }

        public static AI CreateFromStream(BinaryReader reader)
        {
            AI createdAI = null;
            bool reading = true;
            while (reading)
            {
                StreamEnum nextEnum = (StreamEnum)reader.ReadByte();
                switch (nextEnum)
                {
                    case StreamEnum.NAME:
                        {
                            // parse the type name
                            createdAI = (AI)TypeHelper.CreateFromTypeName(reader.ReadString());
                        }
                        break;
                    case StreamEnum.STOP:
                    default:
                        reading = false;
                        break;
                }
            }
            return createdAI;
        }
    }
}
