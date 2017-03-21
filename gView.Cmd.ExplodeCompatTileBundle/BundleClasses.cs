using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace gView.Cmd.CompactTileBundle
{
    public class BundleIndex
    {
        public BundleIndex(string filename)
        {
            this.Filename = filename;
        }

        public string Filename { get; private set; }

        public int TilePosition(int row, int col, out int tileLength)
        {
            if (row < 0 || row > 128 || col < 0 || col > 128)
                throw new ArgumentException("Compact Tile Index out of range");

            int indexPosition = ((row * 128) + col) * 8;

            using (FileStream fs = new FileStream(this.Filename, FileMode.Open, FileAccess.Read))
            {
                byte[] data = new byte[8];
                fs.Position = indexPosition;
                fs.Read(data, 0, 8);

                int position = BitConverter.ToInt32(data, 0);
                tileLength = BitConverter.ToInt32(data, 4);

                return position;
            }
        }
    }

    public class Bundle
    {
        public Bundle(string filename)
        {
            this.Filename = filename;
        }

        public string Filename { get; private set; }

        public byte[] ImageData(int pos, int length)
        {
            using (FileStream fs = new FileStream(this.Filename, FileMode.Open, FileAccess.Read))
            {
                byte[] data = new byte[length];
                fs.Position = pos;
                fs.Read(data, 0, data.Length);

                return data;
            }
        }

        public int StartRow
        {
            get
            {
                string fileTitle = (new FileInfo(this.Filename)).Name;

                string rHex = fileTitle.Substring(1, 8);
                int row = int.Parse(rHex, System.Globalization.NumberStyles.HexNumber);

                return row;
            }
        }

        public int StartCol
        {
            get
            {
                string fileTitle = (new FileInfo(this.Filename)).Name;

                string cHex = fileTitle.Substring(10, 8);
                int col = int.Parse(cHex, System.Globalization.NumberStyles.HexNumber);

                return col;
            }
        }
    }
}
