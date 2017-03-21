using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using gView.Framework.Geometry;

namespace gView.Interoperability.Misc.Request
{
    class MiscParameterDescriptor
    {
        private static IFormatProvider _nhi = System.Globalization.CultureInfo.InvariantCulture.NumberFormat;

        public IEnvelope BBOX = null;
        public int MaxFeatures = 1000;
        public string SRS = "EPSG:4326";
        public string [] LAYERS = new string[0];

        public bool ParseParameters(string[] parameters)
        {
            ParseParameters(new Parameters(parameters));
            return true;
        }
        private void ParseParameters(Parameters Request)
        {
            if (Request["BBOX"] != null)
            {
                string[] bbox = Request["BBOX"].Split(",".ToCharArray());

                double MinX = double.Parse(bbox[0], _nhi);
                double MinY = double.Parse(bbox[1], _nhi);
                double MaxX = double.Parse(bbox[2], _nhi);
                double MaxY = double.Parse(bbox[3], _nhi);
                BBOX = new Envelope(MinX, MinY, MaxX, MaxY);
            }
            if (Request["SRS"] != null)
            {
                SRS = Request["SRS"];
            }
            if (Request["LAYERS"] != null)
            {
                LAYERS = Request["LAYERS"].Split(',');
            }
        }
    
        private class Parameters
        {
            private Dictionary<string, string> _parameters = new Dictionary<string, string>();

            public Parameters(string[] list)
            {
                if (list == null) return;

                foreach (string l in list)
                {
                    string[] p = l.Split('=');

                    string p1 = p[0].Trim().ToUpper(), pp;
                    string p2 = ((p.Length > 1) ? p[1].Trim() : "");

                    if (_parameters.TryGetValue(p1, out pp)) continue;

                    _parameters.Add(p1, p2);
                }
            }

            public string this[string parameter]
            {
                get
                {
                    string o;
                    if (!_parameters.TryGetValue(parameter.ToUpper(), out o))
                        return null;

                    return o;
                }
            }
        }
    }
}
