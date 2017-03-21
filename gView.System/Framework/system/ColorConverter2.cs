using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;

namespace gView.Framework.system
{
    public class ColorConverter2
    {
        static public Color ConvertFrom(string colString)
        {
            colString = colString.Trim().ToLower();

            try
            {
                if (colString.StartsWith("#"))
                {
                    ColorConverter conv = new ColorConverter();
                    object col = conv.ConvertFrom(colString);
                    if (col is Color) return (Color)col;
                }
                else if (colString.StartsWith("rgb("))
                {
                    string[] rgb = colString.Replace("rgb(", "").Replace(")", "").Trim().Split(',');
                    if (rgb.Length == 3)
                    {
                        return Color.FromArgb(
                            int.Parse(rgb[0]),
                            int.Parse(rgb[1]),
                            int.Parse(rgb[2]));

                    }
                }
                else
                {
                    string[] rgb = colString.Trim().Split(',');
                    if (rgb.Length == 3)
                    {
                        return Color.FromArgb(
                            int.Parse(rgb[0]),
                            int.Parse(rgb[1]),
                            int.Parse(rgb[2]));
                    }
                    else if (rgb.Length == 4)
                    {
                        return Color.FromArgb(
                            int.Parse(rgb[0]),
                            int.Parse(rgb[1]),
                            int.Parse(rgb[2]),
                            int.Parse(rgb[3]));
                    }
                }
            }
            catch { }

            return Color.Red;
        }
    }
}
