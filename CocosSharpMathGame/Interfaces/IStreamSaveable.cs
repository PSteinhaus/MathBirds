using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CocosSharpMathGame
{
    /// <summary>
    /// Stream-saveable objects can write their essential information to a stream. They can also be created from such stream.
    /// </summary>
    interface IStreamSaveable
    {
        void WriteToStream(BinaryWriter writer);
    }

    public static class TypeHelper
    {
        public static object CreateFromTypeName(string typeName)
        {
            // call the default constructor
            object obj = null;
            try
            {
                obj = Activator.CreateInstance(Type.GetType(typeName));
            }
            catch (Exception e)
            {
                obj = null;
            }
            return obj;
        }
    }
}
