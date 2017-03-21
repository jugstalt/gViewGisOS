using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.InteropServices;
using gView.Framework.Db;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Data;
using gView.DataSources.Raster;
using gView.Framework.Geometry;

namespace gView.DataSources.Raster.File
{
    public class Pyramid
    {
        private string _errMsg="",_filename="";

        public Pyramid()
        {
        }

        public string lastErrorMessage
        {
            get { return _errMsg; }
        }

        public bool Create(string sourceFilename, string destinationFilename)
        {
            return Create(sourceFilename, destinationFilename, 4);
        }
        public bool Create(string sourceFilename, string destinationFilename, int levels)
        {
            return Create(sourceFilename, destinationFilename, levels, ImageFormat.Jpeg);
        }
        /*
        public bool Create_mdb_(string sourceFilename, string destinationFilename, int levels,ImageFormat format)
        {
            Bitmap8pbbIndexed sourceBitmap = null;
            Bitmap image = null;
            try
            {
                FileInfo fi = new FileInfo(sourceFilename);
                string tfwfilename = sourceFilename.Substring(0, sourceFilename.Length - fi.Extension.Length);
                if (fi.Extension.ToLower() == ".jpg" || fi.Extension.ToLower()==".jpeg")
                    tfwfilename += ".jgw";
                else
                    tfwfilename += ".tfw";
                TFWFile tfw = new TFWFile(tfwfilename);

                string ext = "";
                if(format==ImageFormat.Jpeg) 
                {
                    ext = ".jpg.mdb";
                } 
                else if(format==ImageFormat.Png) 
                {
                    ext = ".png.mdb";
                }
                else if (format == ImageFormat.Tiff)
                {
                    ext = ".tif.mdb";
                }
                else
                {
                    format = ImageFormat.Jpeg;
                    ext = ".jpg.mdb";
                }
                fi = new FileInfo(destinationFilename);
                _filename = destinationFilename = destinationFilename.Substring(0, destinationFilename.Length - fi.Extension.Length) + ext;
                fi = new FileInfo(destinationFilename);
                if (fi.Exists) fi.Delete();

                ADOX.CatalogClass cat = new ADOX.CatalogClass();
                cat.Create("Provider=Microsoft.Jet.OLEDB.4.0;Data Source=" + destinationFilename + ";Jet OLEDB:Engine Type=5");
                //System.Runtime.InteropServices.Marshal.ReleaseComObject(cat);
                cat = null;

                DBConnection conn = new DBConnection();
                conn.OleDbConnectionMDB = destinationFilename;

                string[] fields_main ={ "SHAPE", "UNIT", "X", "Y", "dx1", "dx2", "dy1", "dy2", "cellX", "cellY", "iWidth", "iHeight" };
                string[] types_main ={ "OLEOBJECT", "TEXT(50)", "DOUBLE", "DOUBLE", "DOUBLE", "DOUBLE", "DOUBLE", "DOUBLE", "DOUBLE", "DOUBLE", "INTEGER", "INTEGER" };

                string[] fields_img = { "ID", "LEV", "SHAPE", "IMG", "X", "Y", "dx1", "dx2", "dy1", "dy2", "cellX", "cellY", "iWidth", "iHeight" };
                string[] types_img = { "INTEGER NOT NULL IDENTITY(1,1)", "INTEGER", "OLEOBJECT", "OLEOBJECT", "DOUBLE", "DOUBLE", "DOUBLE", "DOUBLE", "DOUBLE", "DOUBLE", "DOUBLE", "DOUBLE", "INTEGER", "INTEGER" };

                if (!conn.createTable("PYRAMID_SHAPE", fields_main, types_main)) return false;
                if (!conn.createTable("PYRAMID_IMG", fields_img, types_img)) return false;

                image = new Bitmap(sourceFilename); //Image.FromFile(sourceFilename);
                Polygon polygon = tfw.CreatePolygon(image.Width, image.Height);

                BinaryWriter writer = new BinaryWriter(new MemoryStream());
                polygon.Serialize(writer,new GeometryDef(geometryType.Polygon,null,true));

                byte[] geometry = new byte[writer.BaseStream.Length];
                writer.BaseStream.Position = 0;
                writer.BaseStream.Read(geometry, (int)0, (int)writer.BaseStream.Length);
                writer.Close();

                DataTable tab = conn.Select("*", "PYRAMID_SHAPE", "", "", true);
                DataRow row = tab.NewRow();
                row["SHAPE"] = geometry;
                row["UNIT"] = "Meters";
                double X, Y, dx1, dx2, dy1, dy2, cellX, cellY;
                int width, height;

                row["X"] = X = tfw.X;
                row["Y"] = Y = tfw.Y;
                row["dx1"] = dx1 = tfw.dx_X;
                row["dx2"] = dx2 = tfw.dx_Y;
                row["dy1"] = dy1 = tfw.dy_X;
                row["dy2"] = dy2 = tfw.dy_Y;
                row["cellX"] = cellX = Math.Sqrt(tfw.dx_X * tfw.dx_X + tfw.dx_Y * tfw.dx_Y);
                row["cellY"] = cellY = Math.Sqrt(tfw.dy_X * tfw.dy_X + tfw.dy_Y * tfw.dy_Y);
                row["iWidth"] = width = image.Width;
                row["iHeight"] = height = image.Height;

                tab.Rows.Add(row);
                conn.Update(tab);

                X = X - dx1 / 2.0 - dy1 / 2.0;  // Auf die linke oberere Ecke setzen
                Y = Y - dx2 / 2.0 - dy2 / 2.0;

                PixelFormat pixFormat = image.PixelFormat;
                
                if (pixFormat != PixelFormat.Format8bppIndexed &&
                    pixFormat != PixelFormat.Format1bppIndexed)
                {
                    pixFormat = PixelFormat.Format32bppArgb;
                }
                else
                {
                    sourceBitmap = new Bitmap8pbbIndexed(image);
                }

                for (int lev = 1; lev <= levels; lev++)
                {
                    int tWidth = width / (int)Math.Pow(2,lev-1), tHeight = height / (int)Math.Pow(2,lev-1);
                    int iWidth = getWidth(tWidth);
                    int iHeight = getHeight(tHeight);
                    double stepX = iWidth*Math.Pow(2,lev-1), stepY = iHeight*Math.Pow(2,lev-1);
                    
                    for (int y = 0,yC=0; y < tHeight; y += iHeight,yC++)
                    {
                        for (int x = 0,xC=0; x < tWidth; x += iWidth,xC++)
                        {
                            //int ww = Math.Min(iWidth, tWidth - x);
                            //int hh = Math.Min(iHeight, tHeight - y);
                            //if (ww < 10 || hh < 10)
                            //{
                            //    continue;
                            //}
                            using (Bitmap bm = new Bitmap(Math.Min(iWidth, tWidth - x), Math.Min(iHeight, tHeight - y),image.PixelFormat))
                            {
                                if ((bm.PixelFormat == PixelFormat.Format1bppIndexed ||
                                    bm.PixelFormat == PixelFormat.Format8bppIndexed) &&
                                    sourceBitmap != null)
                                {
                                    Bitmap8pbbIndexed destBitmap = new Bitmap8pbbIndexed(bm);
                                    destBitmap.DrawImage(sourceBitmap,
                                        new Rectangle(0, 0, bm.Width, bm.Height),
                                        new Rectangle(
                                            (int)(xC * (float)stepX), (int)(yC * (float)stepY),
                                            (int)Math.Min(image.Width - xC * stepX, stepX),
                                            (int)Math.Min(image.Height - yC * stepY, stepY)
                                            ));
                                    destBitmap.Dispose(false);
                                    bm.Palette = image.Palette;
                                }
                                else
                                {
                                    using (System.Drawing.Graphics gr = System.Drawing.Graphics.FromImage(bm))
                                    {
                                        gr.DrawImage(image,
                                            new Rectangle(0, 0, bm.Width, bm.Height),
                                            xC * (float)stepX, yC * (float)stepY,
                                            (float)Math.Min(image.Width - xC * stepX, stepX),
                                            (float)Math.Min(image.Height - yC * stepY, stepY),
                                            GraphicsUnit.Pixel);
                                    }
                                }

                                tab = conn.Select("*", "PYRAMID_IMG", "ID=-1", "", true);
                                row = tab.NewRow();

                                writer = new BinaryWriter(new MemoryStream());
                                bm.Save(writer.BaseStream, format);
                                //bm.Save(sourceFilename + "_" + lev + "_" + xC + "_" + yC + ".jpg", ImageFormat.Jpeg);
                                byte[] img = new byte[writer.BaseStream.Length];
                                writer.BaseStream.Position = 0;
                                writer.BaseStream.Read(img, (int)0, (int)writer.BaseStream.Length);
                                writer.Close();

                                row["LEV"] = lev;
                                row["IMG"] = img;
                                //row["X"] = X + dx1 * stepX * xC + dy1 * stepY * yC;
                                //row["Y"] = Y + dy2 * stepY * yC + dx2 * stepX * xC;

                                row["dx1"] = dx1*Math.Pow(2,lev-1);
                                row["dx2"] = dx2*Math.Pow(2,lev-1);
                                row["dy1"] = dy1*Math.Pow(2,lev-1);
                                row["dy2"] = dy2*Math.Pow(2,lev-1);

                                row["X"] = X + (double)row["dx1"] * x + (double)row["dy1"] * y + (double)row["dx1"] / 2.0 + (double)row["dy1"] / 2.0;
                                row["Y"] = Y + (double)row["dx2"] * x + (double)row["dy2"] * y + (double)row["dx2"] / 2.0 + (double)row["dy2"] / 2.0;

                                row["cellX"] = cellX * Math.Pow(2, lev - 1);
                                row["cellY"] = cellY * Math.Pow(2, lev - 1);
                                row["iWidth"] = bm.Width;
                                row["iHeight"] = bm.Height;

                                tfw = new TFWFile(
                                    (double)row["X"],
                                    (double)row["Y"],
                                    (double)row["dx1"],
                                    (double)row["dx2"],
                                    (double)row["dy1"],
                                    (double)row["dy2"]);

                                polygon = tfw.CreatePolygon(bm.Width, bm.Height);
                                writer = new BinaryWriter(new MemoryStream());
                                polygon.Serialize(writer, new GeometryDef(geometryType.Polygon,null,true));

                                geometry = new byte[writer.BaseStream.Length];
                                writer.BaseStream.Position = 0;
                                writer.BaseStream.Read(geometry, (int)0, (int)writer.BaseStream.Length);
                                writer.Close();
                                row["SHAPE"] = geometry;

                                tab.Rows.Add(row);
                                conn.Update(tab);
                                tab.Dispose();
                                bm.Dispose();
                            }
                        }
                    }
                }

                conn.Dispose();
                conn = null;
                image.Dispose();
                image = null;

                return true;
            }
            catch(Exception ex)
            {
                
                _errMsg = ex.Message;
                if (sourceBitmap != null)
                {
                    sourceBitmap.Dispose(false);
                    sourceBitmap = null;
                }
                if (image != null)
                {
                    image.Dispose();
                    image = null;
                }
                return false;
            }
        }
        */

        public bool Create(int image_id, string imageDSName, Bitmap image, TFWFile TFW, ICommonDbConnection conn, int levels, ImageFormat format)
        {
            if (image == null) return false;

            double X, Y, dx1, dx2, dy1, dy2, cellX, cellY;
            int width = image.Width, height = image.Height;

            X = TFW.X;
            Y = TFW.Y;
            dx1 = TFW.dx_X;
            dx2 = TFW.dx_Y;
            dy1 = TFW.dy_X;
            dy2 = TFW.dy_Y;
            cellX = Math.Sqrt(TFW.dx_X * TFW.dx_X + TFW.dx_Y * TFW.dx_Y);
            cellY = Math.Sqrt(TFW.dy_X * TFW.dy_X + TFW.dy_Y * TFW.dy_Y);

            X = X - dx1 / 2.0 - dy1 / 2.0;  // Auf die linke oberere Ecke setzen
            Y = Y - dx2 / 2.0 - dy2 / 2.0;

            DataTable tab;
            if (conn.dbType == DBType.sql)
            {
                tab = conn.Select("*", imageDSName + "_IMAGE_DATA", "IMAGE_ID=-1", "", true);
            }
            else
            {
                tab = conn.Select("*", "PYRAMID_IMG", "ID=-1", "", true);
            }

            for (int lev = 1; lev <= Math.Max(1,levels); lev++)
            {
                int tWidth = width / (int)Math.Pow(2, lev - 1), tHeight = height / (int)Math.Pow(2, lev - 1);
                int iWidth = (levels>0) ? getWidth(tWidth) : tWidth;
                int iHeight = (levels>0) ? getHeight(tHeight) : tHeight;
                double stepX = iWidth * Math.Pow(2, lev - 1), stepY = iHeight * Math.Pow(2, lev - 1);

                for (int y = 0, yC = 0; y < tHeight; y += iHeight, yC++)
                {
                    for (int x = 0, xC = 0; x < tWidth; x += iWidth, xC++)
                    {
                        //int ww = Math.Min(iWidth, tWidth - x);
                        //int hh = Math.Min(iHeight, tHeight - y);
                        //if (ww < 10 || hh < 10)
                        //{
                        //    continue;
                        //}
                        using (Bitmap bm = new Bitmap(Math.Min(iWidth, tWidth - x), Math.Min(iHeight, tHeight - y)))
                        {
                            using (System.Drawing.Graphics gr = System.Drawing.Graphics.FromImage(bm))
                            {
                                gr.DrawImage(image,
                                    new Rectangle(0, 0, bm.Width, bm.Height),
                                    xC * (float)stepX, yC * (float)stepY,
                                    (float)Math.Min(image.Width - xC * stepX, stepX),
                                    (float)Math.Min(image.Height - yC * stepY, stepY),
                                    GraphicsUnit.Pixel);
                            }

                            /*
                            DataTable tab;
                            if (conn.dbType == DBType.sql)
                            {
                                tab = conn.Select("*", imageDSName+"_IMAGE_DATA", "IMAGE_ID=-1", "", true);  
                            }
                            else
                            {
                                tab = conn.Select("*", "PYRAMID_IMG", "ID=-1", "", true);
                            }
                            */
                            DataRow row = tab.NewRow();

                            BinaryWriter writer = new BinaryWriter(new MemoryStream());
                            bm.Save(writer.BaseStream, format);
                            //bm.Save(sourceFilename + "_" + lev + "_" + xC + "_" + yC + ".jpg", ImageFormat.Jpeg);
                            byte[] img = new byte[writer.BaseStream.Length];
                            writer.BaseStream.Position = 0;
                            writer.BaseStream.Read(img, (int)0, (int)writer.BaseStream.Length);
                            writer.Close();

                            if (conn.dbType == DBType.sql)
                            {
                                row["IMAGE_ID"] = image_id;
                            }

                            row["LEV"] = lev;
                            row[(conn.dbType == DBType.sql) ? "IMAGE" : "IMG"] = img;
                            //row["X"] = X + dx1 * stepX * xC + dy1 * stepY * yC;
                            //row["Y"] = Y + dy2 * stepY * yC + dx2 * stepX * xC;

                            row["dx1"] = dx1 * Math.Pow(2, lev - 1);
                            row["dx2"] = dx2 * Math.Pow(2, lev - 1);
                            row["dy1"] = dy1 * Math.Pow(2, lev - 1);
                            row["dy2"] = dy2 * Math.Pow(2, lev - 1);

                            row["X"] = X + (double)row["dx1"] * x + (double)row["dy1"] * y + (double)row["dx1"] / 2.0 + (double)row["dy1"] / 2.0;
                            row["Y"] = Y + (double)row["dx2"] * x + (double)row["dy2"] * y + (double)row["dx2"] / 2.0 + (double)row["dy2"] / 2.0;

                            row["cellX"] = cellX * Math.Pow(2, lev - 1);
                            row["cellY"] = cellY * Math.Pow(2, lev - 1);
                            row["iWidth"] = bm.Width;
                            row["iHeight"] = bm.Height;

                            TFWFile tfw = new TFWFile(
                                (double)row["X"],
                                (double)row["Y"],
                                (double)row["dx1"],
                                (double)row["dx2"],
                                (double)row["dy1"],
                                (double)row["dy2"]);

                            IPolygon polygon = tfw.CreatePolygon(bm.Width, bm.Height);
                            writer = new BinaryWriter(new MemoryStream());
                            polygon.Serialize(writer, new GeometryDef(geometryType.Polygon,null,true));

                            byte[] geometry = new byte[writer.BaseStream.Length];
                            writer.BaseStream.Position = 0;
                            writer.BaseStream.Read(geometry, (int)0, (int)writer.BaseStream.Length);
                            writer.Close();
                            row["SHAPE"] = geometry;

                            tab.Rows.Add(row);
                            /*
                            conn.Update(tab);
                            tab.Dispose();
                             * */
                            bm.Dispose();
                        }
                    }
                }
            }

            conn.Update(tab);
            tab.Dispose();

            return true;
        }

        public bool Create(string sourceFilename, string destinationFilename, int levels, ImageFormat format)
        {
            Bitmap8pbbIndexed sourceBitmap = null;
            Bitmap image = null;
            try
            {
                FileInfo fi = new FileInfo(sourceFilename);
                string tfwfilename = sourceFilename.Substring(0, sourceFilename.Length - fi.Extension.Length);
                if (fi.Extension.ToLower() == ".jpg" || fi.Extension.ToLower() == ".jpeg")
                    tfwfilename += ".jgw";
                else
                    tfwfilename += ".tfw";
                TFWFile tfw = new TFWFile(tfwfilename);

                image = new Bitmap(sourceFilename); //Image.FromFile(sourceFilename);
                Polygon polygon = tfw.CreatePolygon(image.Width, image.Height);

                StreamWriter writer = new StreamWriter(destinationFilename + ".pyx");

                double X, Y, dx1, dx2, dy1, dy2, cellX, cellY;
                int width, height;

                // Header schreiben
                PyramidFileHeader header = new PyramidFileHeader(true);
                header.iWidth = width = image.Width;
                header.iHeight = height = image.Height;
                header.X = X = tfw.X;
                header.Y = Y = tfw.Y;
                header.dx1 = dx1 = tfw.dx_X;
                header.dx2 = dx2 = tfw.dx_Y;
                header.dy1 = dy1 = tfw.dy_X;
                header.dy2 = dy2 = tfw.dy_Y;
                header.cellX = cellX = tfw.cellX;
                header.cellY = cellY = tfw.cellY;
                header.Levels = levels;
                header.Format = format.Guid;
                header.Save(writer.BaseStream);

                X = X - dx1 / 2.0 - dy1 / 2.0;  // Auf die linke oberere Ecke setzen
                Y = Y - dx2 / 2.0 - dy2 / 2.0;

                PixelFormat pixFormat = image.PixelFormat;

                if (pixFormat != PixelFormat.Format8bppIndexed &&
                    pixFormat != PixelFormat.Format1bppIndexed)
                {
                    pixFormat = PixelFormat.Format32bppArgb;
                }
                else
                {
                    sourceBitmap = new Bitmap8pbbIndexed(image);
                }

                // Levelheader schreiben
                for (int lev = 1; lev <= levels; lev++)
                {
                    int tWidth = width / (int)Math.Pow(2, lev - 1), tHeight = height / (int)Math.Pow(2, lev - 1);
                    int iWidth = getWidth(tWidth);
                    int iHeight = getHeight(tHeight);
                    double stepX = iWidth * Math.Pow(2, lev - 1), stepY = iHeight * Math.Pow(2, lev - 1);

                    PyramidLevelHeader levelHeader;
                    levelHeader.level=lev;
                    levelHeader.numPictures = 0; // ????
                    levelHeader.cellX = cellX * Math.Pow(2, lev - 1); ;
                    levelHeader.cellY = cellY * Math.Pow(2, lev - 1); ;

                    for (int y = 0, yC = 0; y < tHeight; y += iHeight, yC++)
                    {
                        for (int x = 0, xC = 0; x < tWidth; x += iWidth, xC++)
                        {
                            levelHeader.numPictures++;
                        }
                    }
                    levelHeader.Save(writer.BaseStream);
                }

                // Pictureheader und Bilder schreiben
                StreamWriter picWriter = new StreamWriter(destinationFilename + ".pyc");
                for (int lev = 1; lev <= levels; lev++)
                {
                    int tWidth = width / (int)Math.Pow(2, lev - 1), tHeight = height / (int)Math.Pow(2, lev - 1);
                    int iWidth = getWidth(tWidth);
                    int iHeight = getHeight(tHeight);
                    double stepX = iWidth * Math.Pow(2, lev - 1), stepY = iHeight * Math.Pow(2, lev - 1);
                    
                    for (int y = 0, yC = 0; y < tHeight; y += iHeight, yC++)
                    {
                        for (int x = 0, xC = 0; x < tWidth; x += iWidth, xC++)
                        {
                            PyramidPictureHeader picHeader;
                            
                            using (Bitmap bm = new Bitmap(Math.Min(iWidth, tWidth - x), Math.Min(iHeight, tHeight - y), image.PixelFormat))
                            {
                                if ((bm.PixelFormat == PixelFormat.Format1bppIndexed ||
                                    bm.PixelFormat == PixelFormat.Format8bppIndexed) &&
                                    sourceBitmap != null)
                                {
                                    Bitmap8pbbIndexed destBitmap = new Bitmap8pbbIndexed(bm);
                                    destBitmap.DrawImage(sourceBitmap,
                                        new Rectangle(0, 0, bm.Width, bm.Height),
                                        new Rectangle(
                                            (int)(xC * (float)stepX), (int)(yC * (float)stepY),
                                            (int)Math.Min(image.Width - xC * stepX, stepX),
                                            (int)Math.Min(image.Height - yC * stepY, stepY)
                                            ));
                                    destBitmap.Dispose(false);
                                    bm.Palette = image.Palette;
                                }
                                else
                                {
                                    using (System.Drawing.Graphics gr = System.Drawing.Graphics.FromImage(bm))
                                    {
                                        gr.DrawImage(image,
                                            new Rectangle(0, 0, bm.Width, bm.Height),
                                            xC * (float)stepX, yC * (float)stepY,
                                            (float)Math.Min(image.Width - xC * stepX, stepX),
                                            (float)Math.Min(image.Height - yC * stepY, stepY),
                                            GraphicsUnit.Pixel);
                                    }
                                }

                                MemoryStream ms = new MemoryStream();
                                BinaryWriter bw = new BinaryWriter(ms);
                                bm.Save(bw.BaseStream, format);
                                byte[] buffer = new byte[ms.Length];
                                ms.Position = 0;
                                ms.Read(buffer, 0, buffer.Length);
                                bw.Close();
                                ms.Close();
                                bm.Dispose();

                                picHeader.startPosition = picWriter.BaseStream.Position;
                                picHeader.streamLength = buffer.Length;
                                picWriter.BaseStream.Write(buffer, 0, buffer.Length);
                            }

                            picHeader.level=lev;
                            picHeader.iWidth = iWidth;
                            picHeader.iHeight = iHeight;

                            picHeader.dx1 = dx1 * Math.Pow(2, lev - 1);
                            picHeader.dx2 = dx2 * Math.Pow(2, lev - 1);
                            picHeader.dy1 = dy1 * Math.Pow(2, lev - 1);
                            picHeader.dy2 = dy2 * Math.Pow(2, lev - 1);

                            picHeader.X = X + picHeader.dx1 * x + picHeader.dy1 * y + picHeader.dx1 / 2.0 + picHeader.dy1 / 2.0;
                            picHeader.Y = Y + picHeader.dx2 * x + picHeader.dy2 * y + picHeader.dx2 / 2.0 + picHeader.dy2 / 2.0;

                            picHeader.cellX = cellX * Math.Pow(2, lev - 1);
                            picHeader.cellY = cellY * Math.Pow(2, lev - 1);

                            picHeader.Save(writer.BaseStream);
                        }
                    }
                }
                picWriter.Close();

                writer.Close();
                return true;
            }
            catch (Exception ex)
            {

                _errMsg = ex.Message;
                if (sourceBitmap != null)
                {
                    sourceBitmap.Dispose(false);
                    sourceBitmap = null;
                }
                if (image != null)
                {
                    image.Dispose();
                    image = null;
                }
                return false;
            }
        }
        public string FileName
        {
            get { return _filename; }
        }

        /*
        private static int getWidth(int width)
        {
            if (width < 1250) return width;
            double w_ = width / 1250.0;

            for (int w = Math.Max((int)w_,2); w < 1000; w++)
            {
                w_ = (double)width / (double)w;
                if (w_ == Math.Round(w_)) break;
            }
            if (w_ < 1250) w_ = 1250;

            return (int)w_;
        }

        private static int getHeight(int height)
        {
            if (height < 1000) return height;
            double w_ = (double)height / 1000.0;

            for (int w = Math.Max((int)w_,2); w < 1000; w++)
            {
                w_ = (double)height / (double)w;
                if (w_ == (int)Math.Round(w_)) break;
            }
            if (w_ < 1000) w_ = 1000;

            return (int)w_;
        }
         * */

        private static int getWidth(int width)
        {
            if (width < 800) return width;
            double w_ = width / 800.0;

            for (int w = Math.Max((int)w_, 2); w < 1000; w++)
            {
                w_ = (double)width / (double)w;
                if (w_ == Math.Round(w_)) break;
            }
            if (w_ < 800) w_ = 800;

            return (int)w_;
        }

        private static int getHeight(int height)
        {
            if (height < 600) return height;
            double w_ = (double)height / 600.0;

            for (int w = Math.Max((int)w_, 2); w < 1000; w++)
            {
                w_ = (double)height / (double)w;
                if (w_ == (int)Math.Round(w_)) break;
            }
            if (w_ < 600) w_ = 600;

            return (int)w_;
        }
    }
}
