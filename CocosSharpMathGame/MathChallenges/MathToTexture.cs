using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CocosSharp;
using CSharpMath.SkiaSharp;
using CSharpMath;
using SkiaSharp;
using System.IO;
using MathNet.Symbolics;
using CSharpMath.Rendering.FrontEnd;

namespace CocosSharpMathGame
{
    /// <summary>
    /// Provides methods to create textures from strings containing math 
    /// </summary>
    internal static class MathToTexture
    {
        internal static string InfixToLatex(string infix)
        {
            return LaTeX.Format(Infix.ParseOrUndefined(infix));
        }
        private static MathPainter CreatePainterWithInfix(string infix)
        {
            return new MathPainter { TextColor = SKColor.FromHsl(0, 0, 100), FontSize = 8, LaTeX = InfixToLatex(infix) };
        }
        private static MathPainter CreatePainterWithLatex(string latex)
        {
            return new MathPainter { TextColor = SKColor.FromHsl(0, 0, 100), FontSize = 100, LaTeX = latex };
        }
        /// <summary>
        /// Creates a CCTexture2D from a string containing a math expression in infix (or LaTeX) notation
        /// and returns it
        /// </summary>
        internal static CCTexture2D CreateTexture(string math, bool isLatex=false)
        {
            CCTexture2D texture;
            using (var png = (isLatex ? CreatePainterWithLatex(math) : CreatePainterWithInfix(math)).DrawAsStream(format: SkiaSharp.SKEncodedImageFormat.Png))
            {
                using (var memoryStream = new MemoryStream())
                {
                    png.CopyTo(memoryStream);
                    texture = new CCTexture2D(memoryStream);
                }
            }
            return texture;
        }

        /// <summary>
        /// Creates a CCTexture2D from a string containing a math expression in infix (or LaTeX) notation
        /// and adds it to the shared texture cache
        /// </summary>
        internal static void CreateAndAddTexture(string math, string texName, bool isLatex = true)
        {
            using (var png = (isLatex ? CreatePainterWithLatex(math) : CreatePainterWithInfix(math)).DrawAsStream(format: SkiaSharp.SKEncodedImageFormat.Png))
            {
                using (var memoryStream = new MemoryStream())
                {
                    png.CopyTo(memoryStream);
                    CCTextureCache.SharedTextureCache.AddImage(memoryStream.ToArray(), texName, CCSurfaceFormat.Color);
                }
            }
        }
    }
}
