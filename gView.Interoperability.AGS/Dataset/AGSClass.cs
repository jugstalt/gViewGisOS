using System;
using System.Collections.Generic;
using System.Text;
using gView.Framework.Data;
using gView.Framework.Geometry;
using gView.Interoperability.AGS.Helper;
using gView.Interoperability.AGS.Proxy;
using gView.Framework.Web;

namespace gView.Interoperability.AGS.Dataset
{
    public class AGSClass : IWebServiceClass
    {
        private string _name;
        private AGSDataset _dataset;
        private List<IWebServiceTheme> _clonedThemes = null;
        private System.Drawing.Bitmap _legend = null;
        private GeorefBitmap _image = null;
        
        public AGSClass(AGSDataset dataset)
        {
            _dataset = dataset;
            if (_dataset != null) _name = _dataset._name;
        }

        #region IWebServiceClass Member

        public event BeforeMapRequestEventHandler BeforeMapRequest;

        public event AfterMapRequestEventHandler AfterMapRequest;

        public bool MapRequest(gView.Framework.Carto.IDisplay display)
        {
            if (_dataset == null ||
                _dataset._mapServer == null ||
                _dataset._mapDescription == null || Themes == null) return false;

            List<IWebServiceTheme> themes = Themes;

            #region Check for visible Layers
            bool visFound = false;
            foreach (IWebServiceTheme theme in themes)
            {
                if (!theme.Visible) continue;
                if (theme.MinimumScale > 1 && theme.MinimumScale > display.mapScale) continue;
                if (theme.MaximumScale > 1 && theme.MaximumScale < display.mapScale) continue;

                visFound = true;
                break;
            }
            if (!visFound)
            {
                if (_image != null)
                {
                    _image.Dispose();
                    _image = null;
                }
                return true;
            }
            #endregion

            ISpatialReference sRef = (display.SpatialReference != null) ?
                display.SpatialReference.Clone() as ISpatialReference :
                null;

            int iWidth = display.iWidth;
            int iHeight = display.iHeight;

            if (BeforeMapRequest != null)
                BeforeMapRequest(this, display, ref sRef, ref iWidth, ref iHeight);

            try
            {
                #region Extent
                _dataset._mapDescription.MapArea.Extent =
                    ArcServerHelper.EnvelopeN(display.Envelope);
                if (display.DisplayTransformation.UseTransformation)
                    _dataset._mapDescription.Rotation = display.DisplayTransformation.DisplayRotation;
                #endregion

                #region Back/Transparent Color
                RgbColor backColor = ArcServerHelper.RgbColor(display.BackgroundColor);
                SimpleFillSymbol fillSymbol = new SimpleFillSymbol();
                fillSymbol.Color = backColor;
                fillSymbol.Outline = null;
                _dataset._mapDescription.BackgroundSymbol = fillSymbol;
                _dataset._mapDescription.TransparentColor = backColor;
                #endregion

                #region Layer Visibility
                LayerDescription[] layerDescriptions = _dataset._mapDescription.LayerDescriptions;
                foreach (LayerDescription layerDescr in layerDescriptions)
                {
                    IWebServiceTheme theme = GetThemeByLayerId(layerDescr.LayerID.ToString());
                    if (theme == null)
                    {
                        continue;
                    }
                    layerDescr.Visible = theme.Visible;
                    if (layerDescr.Visible)
                    {
                        foreach (int parentLayerId in _dataset.ParentLayerIds(layerDescr.LayerID))
                        {
                            LayerDescription parent = _dataset.LayerDescriptionById(parentLayerId);
                            if (parent != null)
                                parent.Visible = true;
                        }
                    }
                }
                #endregion

                #region ImageDescription
                ImageType imgType = new ImageType();
                imgType.ImageFormat = esriImageFormat.esriImagePNG24;
                imgType.ImageReturnType = esriImageReturnType.esriImageReturnURL;

                ImageDisplay imgDisp = new ImageDisplay();
                imgDisp.ImageWidth = iWidth;
                imgDisp.ImageHeight = iHeight;
                imgDisp.ImageDPI = display.dpi;
                imgDisp.TransparentColor = backColor;

                ImageDescription imgDescr = new ImageDescription();
                imgDescr.ImageDisplay = imgDisp;
                imgDescr.ImageType = imgType;
                #endregion

                MapImage mapImg = _dataset._mapServer.ExportMapImage(_dataset._mapDescription, imgDescr);
                if (mapImg != null && !String.IsNullOrEmpty(mapImg.ImageURL))
                {
                    System.Drawing.Bitmap bm = WebFunctions.DownloadImage(mapImg.ImageURL, _dataset._proxy, System.Net.CredentialCache.DefaultNetworkCredentials);
                    if (bm != null)
                    {
                        _image = new GeorefBitmap(bm);
                        _image.Envelope = new gView.Framework.Geometry.Envelope(new gView.Framework.Geometry.Envelope(display.Envelope));
                        _image.SpatialReference = display.SpatialReference;

                        if (AfterMapRequest != null)
                            AfterMapRequest(this, display, _image);
                    }
                }
                else
                {
                    if (_image != null)
                    {
                        _image.Dispose();
                        _image = null;
                    }
                }
                return _image != null;
            }
            catch (Exception ex)
            {
                //ArcIMSClass.ErrorLog(context, "MapRequest", server, service, ex);
                return false;
            }
        }

        public bool LegendRequest(gView.Framework.Carto.IDisplay display)
        {
            return false;
        }

        public GeorefBitmap Image
        {
            get { return _image; }
        }

        public System.Drawing.Bitmap Legend
        {
            get { return _legend; }
        }

        public gView.Framework.Geometry.IEnvelope Envelope
        {
            get
            {
                if (_dataset == null) return null;
                if (_dataset.State != DatasetState.opened) _dataset.Open();
                return _dataset.Envelope;
            }
        }

        public List<IWebServiceTheme> Themes
        {
            get
            {
                if (_clonedThemes != null) return _clonedThemes;
                if (_dataset != null)
                {
                    if (_dataset.State != DatasetState.opened) _dataset.Open();
                    return _dataset._themes;
                }
                return new List<IWebServiceTheme>();
            }
        }

        public ISpatialReference SpatialReference
        {
            get
            {
                if (_dataset != null)
                    return _dataset.SpatialReference;

                return null;
            }
            set
            {
                if (_dataset != null)
                    _dataset.SpatialReference = value;
            }
        }

        #endregion

        #region IClass Member

        public string Name
        {
            get { return _name; }
        }

        public string Aliasname
        {
            get { return _name; }
        }

        public IDataset Dataset
        {
            get { return _dataset; }
        }

        #endregion

        #region IClone Member

        public object Clone()
        {
            AGSClass clone = new AGSClass(_dataset);
            clone._clonedThemes = new List<IWebServiceTheme>();

            foreach (IWebServiceTheme theme in Themes)
            {
                if (theme == null || theme.Class == null) continue;
                clone._clonedThemes.Add(LayerFactory.Create(theme.Class, theme as ILayer, clone) as IWebServiceTheme);
            }
            clone.BeforeMapRequest = BeforeMapRequest;
            clone.AfterMapRequest = AfterMapRequest;
            return clone;
        }

        #endregion

        #region Helper
        internal IWebServiceTheme GetThemeByLayerId(string layerId)
        {
            if (Themes == null)
                return null;

            foreach (IWebServiceTheme theme in Themes)
            {
                if (theme == null)
                    continue;

                if (theme.Class is IWebFeatureClass && ((IWebFeatureClass)theme.Class).ID == layerId)
                    return theme;
                else if(theme.Class is IWebRasterClass && ((IWebRasterClass)theme.Class).ID == layerId)
                    return theme;
            }
            return null;
        }
        #endregion
    }
}
