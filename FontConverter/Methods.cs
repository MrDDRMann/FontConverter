using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Drawing.Text;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Media;
using Color = System.Drawing.Color;
using FontFamily = System.Drawing.FontFamily;
using FontStyle = System.Drawing.FontStyle;

namespace FontConverter
{
    internal class Methods
    {
        public SizeF GetStringSize(string value, Font font, Graphics g)
        {
            return g.MeasureString(value, font);
        }

        public char[] GetAllChar(List<FontFamily> fontsFamilies, int index)
        {
            System.Windows.Media.FontFamily mfont = new System.Windows.Media.FontFamily(fontsFamilies[index].Name);
            Typeface typeface = new Typeface(mfont,
                FontStyles.Italic,
                FontWeights.Normal,
                FontStretches.Normal);
            GlyphTypeface glyph;
            if (!typeface.TryGetGlyphTypeface(out glyph))
                throw new InvalidOperationException("No glyphtypeface found");
            IDictionary<int, ushort> glyphs = glyph.CharacterToGlyphMap;
            char[] allChar = new char[glyphs.Count];
            for (int i = 0; i < glyphs.Count; i++)
            {
                allChar[i] = Convert.ToChar(glyphs.Keys.ElementAt(i));
            }
            return allChar;
        }

        public Bitmap ResizeBitmap(Bitmap sourceBmp, int width, int height)
        {
            Bitmap result = new Bitmap(width, height);
            using (Graphics g = Graphics.FromImage(result))
                g.DrawImage(sourceBmp, 0, 0, width, height);
            return result;
        }

        public string CreatePath(List<FontFamily> fontsFamilies, int index, string path, int fontSize, FontStyle fontStyle)
        {
            path = path + "\\" + fontsFamilies[index].Name.Replace(" ", "") + "-" + fontSize + "-" + fontStyle;
            bool exists = Directory.Exists(path);

            if (!exists)
                Directory.CreateDirectory(path);
            return path;
        }

        public void DrawImage(List<FontFamily> fontsFamilies, int index, string path, int fontsize, FontStyle fontStyle, Color background,
                Color forground, char[] allChar)
        {
            int i = 0;
            foreach (char printableChar in allChar)
            {
                using (Bitmap b = new Bitmap(500, 500))
                {
                    using (Graphics g = Graphics.FromImage(b))
                    {
                        g.Clear(background);
                        using (
                            Font font = new Font(fontsFamilies[index], fontsize, fontStyle,
                                GraphicsUnit.Pixel))
                        {
                            SizeF size = GetStringSize(printableChar.ToString(), font, g);
                            using (Bitmap bitmap = new Bitmap((int) size.Width, (int) size.Height))
                            {
                                using (Graphics graphics = Graphics.FromImage(bitmap))
                                {
                                    graphics.Clear(background);
                                    using (SolidBrush solidBrush = new SolidBrush(forground))
                                    {
                                        graphics.InterpolationMode = InterpolationMode.High;
                                        graphics.CompositingQuality = CompositingQuality.HighQuality;
                                        graphics.SmoothingMode = SmoothingMode.AntiAlias;
                                        graphics.TextRenderingHint = TextRenderingHint.AntiAlias;
                                        graphics.DrawString(printableChar.ToString(), font, solidBrush,
                                            new PointF(0, 0));
                                        bitmap.Save(path + "\\" + i + ".png", ImageFormat.Png);
                                        bitmap.Dispose();
                                        graphics.Dispose();
                                    }
                                }
                            }
                        }
                        b.Dispose();
                        g.Dispose();
                    }
                }
                i++;
            }
        }
    }
}