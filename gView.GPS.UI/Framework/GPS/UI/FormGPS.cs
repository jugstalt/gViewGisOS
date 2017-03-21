using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.IO.Ports;
using System.Windows.Forms;
using gView.GPS.UI.Properties;

namespace gView.Framework.GPS.UI
{
    public partial class FormGPS : Form
    {
        private static GPSDevice _device = null;
        private Dictionary<int, Satellite> _satellites;

        public FormGPS()
        {
            InitializeComponent();
            InitialiseControlValues();

            _satellites = new Dictionary<int, Satellite>();

            if (_device != null)
            {
                _device.PositionReceived -= new GPSDevice.PositionReceivedEventHandler(device_PositionReceived);
                _device.PDOPReceived -= new GPSDevice.PDOPReceivedEventHandler(device_PDOPReceived);
                _device.VDOPReceived -= new GPSDevice.VDOPReceivedEventHandler(device_VDOPReceived);
                _device.HDOPReceived -= new GPSDevice.HDOPReceivedEventHandler(device_HDOPReceived);
                _device.SatelliteReceived -= new GPSDevice.SatelliteReceivedEventHandler(device_SatelliteReceived);
                _device.DateTimeChanged -= new GPSDevice.DateTimeChangedEventHandler(device_DateTimeChanged);


                _device.PositionReceived += new GPSDevice.PositionReceivedEventHandler(device_PositionReceived);
                _device.PDOPReceived += new GPSDevice.PDOPReceivedEventHandler(device_PDOPReceived);
                _device.VDOPReceived += new GPSDevice.VDOPReceivedEventHandler(device_VDOPReceived);
                _device.HDOPReceived += new GPSDevice.HDOPReceivedEventHandler(device_HDOPReceived);
                _device.SatelliteReceived += new GPSDevice.SatelliteReceivedEventHandler(device_SatelliteReceived);
                _device.DateTimeChanged += new GPSDevice.DateTimeChangedEventHandler(device_DateTimeChanged);
            }
        }

        private void InitialiseControlValues()
        {
            cmbParity.Items.Clear(); cmbParity.Items.AddRange(Enum.GetNames(typeof(Parity)));
            cmbStopBits.Items.Clear(); cmbStopBits.Items.AddRange(Enum.GetNames(typeof(StopBits)));

            
            cmbParity.Text = Settings.Default.Parity.ToString();
            cmbStopBits.Text = Settings.Default.StopBits.ToString();
            cmbDataBits.Text = Settings.Default.DataBits.ToString();
            cmbBaudRate.Text = Settings.Default.BaudRate.ToString();

            cmbPortName.Items.Clear();
            foreach (string s in SerialPort.GetPortNames())
                cmbPortName.Items.Add(s);

            if (cmbPortName.Items.Count > 0) cmbPortName.SelectedIndex = 0;
            else
            {
                MessageBox.Show(this, "There are no COM Ports detected on this computer.\nPlease install a COM Port and restart this app.", "No COM Ports Installed", MessageBoxButtons.OK, MessageBoxIcon.Error);
                this.Close();
            }
        }

        public GPSDevice Device
        {
            get { return _device; }
            set
            {
                _device = value;

                if (_device != null && _device.SerialPort != null)
                {
                    SerialPort comport = _device.SerialPort;
                    
                    btnStart.Text = (comport.IsOpen ? "Stop" : "Start");

                    cmbParity.Text = comport.Parity.ToString();
                    cmbStopBits.Text = comport.StopBits.ToString();
                    cmbDataBits.Text = comport.DataBits.ToString();
                    cmbBaudRate.Text = comport.BaudRate.ToString();
                    cmbPortName.Text = comport.PortName;
                }
            }
        }

        private void btnStart_Click(object sender, EventArgs e)
        {
            if (_device != null)
            {
                _device.Dispose();
                _device = null;
            }
            if (btnStart.Text == "Start")
            {
                _device = new GPSDevice(
                    cmbPortName.Text,
                    int.Parse(cmbBaudRate.Text),
                    (Parity)Enum.Parse(typeof(Parity), cmbParity.Text),
                    int.Parse(cmbDataBits.Text),
                    (StopBits)Enum.Parse(typeof(StopBits), cmbStopBits.Text));

                _device.PositionReceived -= new GPSDevice.PositionReceivedEventHandler(device_PositionReceived);
                _device.PDOPReceived -= new GPSDevice.PDOPReceivedEventHandler(device_PDOPReceived);
                _device.VDOPReceived -= new GPSDevice.VDOPReceivedEventHandler(device_VDOPReceived);
                _device.HDOPReceived -= new GPSDevice.HDOPReceivedEventHandler(device_HDOPReceived);
                _device.SatelliteReceived -= new GPSDevice.SatelliteReceivedEventHandler(device_SatelliteReceived);
                _device.DateTimeChanged -= new GPSDevice.DateTimeChangedEventHandler(device_DateTimeChanged);

                _device.PositionReceived+=new GPSDevice.PositionReceivedEventHandler(device_PositionReceived);
                _device.PDOPReceived+=new GPSDevice.PDOPReceivedEventHandler(device_PDOPReceived);
                _device.VDOPReceived+=new GPSDevice.VDOPReceivedEventHandler(device_VDOPReceived);
                _device.HDOPReceived += new GPSDevice.HDOPReceivedEventHandler(device_HDOPReceived);
                _device.SatelliteReceived += new GPSDevice.SatelliteReceivedEventHandler(device_SatelliteReceived);
                _device.DateTimeChanged += new GPSDevice.DateTimeChangedEventHandler(device_DateTimeChanged);
                
                if (_device.Start())
                {
                    btnStart.Text = "Stop";
                }
                else
                {
                    MessageBox.Show("Can't connect to port...\n"+_device.ErrorMessage);
                }
            }
            else
            {
                btnStart.Text = "Start";
            }
        }

        private delegate void device_DateTimeChangedCallback(GPSDevice sender, DateTime dateTime);
        void device_DateTimeChanged(GPSDevice sender, DateTime dateTime)
        {
            if (this.InvokeRequired)
            {
                device_DateTimeChangedCallback d = new device_DateTimeChangedCallback(device_DateTimeChanged);
                this.Invoke(d, new object[] {sender,dateTime});
            }
            else
            {
                txtTime.Text = dateTime.ToLongTimeString();
            }
        }

        private delegate void device_SatelliteReceivedCallback(GPSDevice sender, int pseudoRandomCode, int azimuth, int elevation, int signalToNoiseRatio);
        void device_SatelliteReceived(GPSDevice sender, int pseudoRandomCode, int azimuth, int elevation, int signalToNoiseRatio)
        {
            if (this.InvokeRequired)
            {
                device_SatelliteReceivedCallback d = new device_SatelliteReceivedCallback(device_SatelliteReceived);
                this.Invoke(d, new object[] { sender, pseudoRandomCode, azimuth, elevation, signalToNoiseRatio });
            }
            else
            {
                if (!_satellites.ContainsKey(pseudoRandomCode))
                {
                    _satellites.Add(pseudoRandomCode, new Satellite(pseudoRandomCode, azimuth, elevation, signalToNoiseRatio));
                }
                else
                {
                    Satellite sat = _satellites[pseudoRandomCode];
                    sat.pseudoRandomCode = pseudoRandomCode;
                    sat.azimuth = azimuth;
                    sat.elevation = elevation;
                    sat.signalToNoiseRatio = signalToNoiseRatio;
                }
                panelSatellites.Refresh();
            }
        }

        private delegate void device_Callback(GPSDevice sender, double value);
        void device_HDOPReceived(GPSDevice sender, double value)
        {
            if (this.InvokeRequired)
            {
                device_Callback d = new device_Callback(device_HDOPReceived);
                this.Invoke(d, new object[] { sender, value });
            }
            else
            {
                txtHDOP.Text = value.ToString();
            }
        }

        void device_VDOPReceived(GPSDevice sender, double value)
        {
            if (this.InvokeRequired)
            {
                device_Callback d = new device_Callback(device_VDOPReceived);
                this.Invoke(d, new object[] { sender, value });
            }
            else
            {
                txtVDOP.Text = value.ToString();
            }
        }

        void device_PDOPReceived(GPSDevice sender, double value)
        {
            if (this.InvokeRequired)
            {
                device_Callback d = new device_Callback(device_PDOPReceived);
                this.Invoke(d, new object[] { sender, value });
            }
            else
            {
                txtPDOP.Text = value.ToString();
            }
        }

        private delegate void device_PositionReceivedCallback(GPSDevice sender, string Lon, string Lat, double deciLon, double deciLat);
        private void device_PositionReceived(GPSDevice sender, string Lon, string Lat, double deciLon, double deciLat)
        {
            if (this.InvokeRequired)
            {
                device_PositionReceivedCallback d = new device_PositionReceivedCallback(device_PositionReceived);
                this.Invoke(d, new object[] { sender, Lon, Lat, deciLon, deciLat });
            }
            else
            {
                txtLon.Text = Lon;
                txtLat.Text = Lat;

                txtDLon.Text = deciLon.ToString();
                txtDLat.Text = deciLat.ToString();
            }
        }

        private void panelSatellites_Paint(object sender, PaintEventArgs e)
        {
            int r = Math.Min(panelSatellites.Width, panelSatellites.Height)/2;
            int cx = 0, cy = 0;
            if (r == panelSatellites.Height / 2)
            {
                e.Graphics.TranslateTransform(cx = r + panelSatellites.Width / 2 - r, cy = r);
            }
            else
            {
                e.Graphics.TranslateTransform(cx = r, cy = r + panelSatellites.Height / 2 - r);
            }
            using (SolidBrush brush = new SolidBrush(Color.White))
            {
                e.Graphics.FillEllipse(brush, -r, -r, 2 * r, 2 * r);
                using (Pen pen = new Pen(Color.Gray))
                {
                    for (int R = r; R > 0; R -= r/3)
                    {
                        e.Graphics.DrawEllipse(pen, -R, -R, 2 * R, 2 * R);
                    }
                    e.Graphics.DrawLine(pen, 0, r, 0, -r);
                    e.Graphics.DrawLine(pen, r, 0, -r, 0);
                }

                brush.Color = Color.Red;
                double azi = 0.0;
                foreach (Satellite sat in _satellites.Values)
                {
                    azi = -sat.azimuth;
                    e.Graphics.ResetTransform();

                    e.Graphics.TranslateTransform(cx, cy);
                    e.Graphics.RotateTransform((float)azi);
                    
                    int ele = (int)(sat.elevation * ((double)r / 90.0));
                    e.Graphics.FillEllipse(brush, -3, ele, 6, 6);
                }
            }
            e.Graphics.ResetTransform();
        }
    }

    internal class Satellite
    {
        public int pseudoRandomCode;
        public int azimuth;
        public int elevation;
        public int signalToNoiseRatio;

        public Satellite(int pseudocode, int azi, int ele, int signal)
        {
            pseudoRandomCode = pseudocode;
            azimuth = azi;
            elevation = ele;
            signalToNoiseRatio = signal;
        }
    }
}