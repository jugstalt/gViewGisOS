using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.IO.Ports;

namespace gView.Framework.GPS
{
    public class GPSDevice
    {
        private SerialPort _comport = null;
        private NmeaInterpreter _GPS = new NmeaInterpreter();
        private string _instring,_errMsg="";

        #region Delegates
        public delegate void PositionReceivedEventHandler(GPSDevice sender, string latitude, string longitude, double deciLat, double deciLon);
        public delegate void DateTimeChangedEventHandler(GPSDevice sender, System.DateTime dateTime);
        public delegate void BearingReceivedEventHandler(GPSDevice sender, double bearing);
        public delegate void SpeedReceivedEventHandler(GPSDevice sender, double speed);
        public delegate void SpeedLimitReachedEventHandler(GPSDevice sender);
        public delegate void FixObtainedEventHandler(GPSDevice sender);
        public delegate void FixLostEventHandler(GPSDevice sender);
        public delegate void SatelliteReceivedEventHandler(GPSDevice sender, 
          int pseudoRandomCode, int azimuth, int elevation,
          int signalToNoiseRatio);
        public delegate void HDOPReceivedEventHandler(GPSDevice sender, double value);
        public delegate void VDOPReceivedEventHandler(GPSDevice sender, double value);
        public delegate void PDOPReceivedEventHandler(GPSDevice sender, double value);
        public delegate void SatellitesInViewReceivedEventHandler(GPSDevice sender, int value);
        public delegate void SatellitesUsedReceivedEventHandler(GPSDevice sender, int value);
        public delegate void EllipsoidHeightReceivedEventHandler(GPSDevice sender, double value);



        #endregion
        #region Events
        public event PositionReceivedEventHandler PositionReceived;
        public event DateTimeChangedEventHandler DateTimeChanged;
        public event BearingReceivedEventHandler BearingReceived;
        public event SpeedReceivedEventHandler SpeedReceived;
        public event SpeedLimitReachedEventHandler SpeedLimitReached;
        public event FixObtainedEventHandler FixObtained;
        public event FixLostEventHandler FixLost;
        public event SatelliteReceivedEventHandler SatelliteReceived;
        public event HDOPReceivedEventHandler HDOPReceived;
        public event VDOPReceivedEventHandler VDOPReceived;
        public event PDOPReceivedEventHandler PDOPReceived;
        public event SatellitesInViewReceivedEventHandler SatellitesInViewReceived;
        public event SatellitesUsedReceivedEventHandler SatellitesUsedReceived;
        public event EllipsoidHeightReceivedEventHandler EllipsoidHeightReceived;
        #endregion

        public GPSDevice(string portname, int boudRate, Parity parity, int dataBits, StopBits stopBits)
        {
            // new SerialPort("COM1", 4800, Parity.None, 8, StopBits.One);
            try
            {
                _comport = new SerialPort(portname, boudRate, parity, dataBits, stopBits);

                _comport.DataReceived += new SerialDataReceivedEventHandler(port_DataReceived);

                _GPS.PositionReceived += new NmeaInterpreter.PositionReceivedEventHandler(GPS_PositionReceived);
                _GPS.SatellitesInViewReceived += new NmeaInterpreter.SatellitesInViewReceivedEventHandler(GPS_SatellitesInViewReceived);
                _GPS.SatellitesUsed += new NmeaInterpreter.SatellitesUsedReceivedEventHandler(GPS_SatellitesUsed);
                _GPS.SpeedReceived += new NmeaInterpreter.SpeedReceivedEventHandler(GPS_SpeedReceived);
                _GPS.BearingReceived += new NmeaInterpreter.BearingReceivedEventHandler(GPS_BearingReceived);
                _GPS.FixLost += new NmeaInterpreter.FixLostEventHandler(GPS_FixLost);
                _GPS.FixObtained += new NmeaInterpreter.FixObtainedEventHandler(GPS_FixObtained);
                _GPS.HDOPReceived += new NmeaInterpreter.HDOPReceivedEventHandler(GPS_HDOPReceived);
                _GPS.VDOPReceived += new NmeaInterpreter.VDOPReceivedEventHandler(GPS_VDOPReceived);
                _GPS.PDOPReceived += new NmeaInterpreter.PDOPReceivedEventHandler(GPS_PDOPReceived);
                _GPS.EllipsoidHeightReceived += new NmeaInterpreter.EllipsoidHeightReceivedEventHandler(GPS_EllipsoidHeightReceived);
                _GPS.SatelliteReceived += new NmeaInterpreter.SatelliteReceivedEventHandler(GPS_SatelliteReceived);
                _GPS.SpeedLimitReached += new NmeaInterpreter.SpeedLimitReachedEventHandler(GPS_SpeedLimitReached);
                _GPS.DateTimeChanged += new NmeaInterpreter.DateTimeChangedEventHandler(GPS_DateTimeChanged);
            }
            catch (Exception ex)
            {
                _errMsg = ex.Message;
            }
        }

        public bool Start()
        {
            if (_comport == null) return false;
            try
            {
                Stop();
                _comport.Open();
               
                return true;
            }
            catch(Exception ex)
            {
                _errMsg = ex.Message;
                return false;
            }
        }
        public void Stop()
        {
            if (_comport != null)
            {
                if (_comport.IsOpen)
                {
                    _comport.Close();
                }
            }
        }

        public void Dispose()
        {
            if (_comport != null)
            {
                Stop();
                _comport.Dispose();
                _comport = null;
            }
        }

        public string ErrorMessage { get { return _errMsg; } }

        /*
        private void _debug_ParseGPSLog()
        {
            StreamReader sr = new StreamReader(@"C:\gps.log");
            string line;
            while ((line = sr.ReadLine()) != null)
            {
                string[] gpsString = line.Split();
                foreach (string item in gpsString) _GPS.Parse(item);
            }
            sr.Close();
        }
        */
        #region serialport
        private void port_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            string inbuff;
            inbuff = _comport.ReadExisting();
            if (inbuff != null)
            {
                if (inbuff.StartsWith("$"))
                {
                    _instring = inbuff;
                }
                else
                {
                    _instring += inbuff;
                }
                /*
                if (_instring != null)
                {
                    StreamWriter sw = new StreamWriter(@"C:\gps.log", true);
                    sw.WriteLine(_instring);
                    sw.Close();
                }
                 * */
                string [] gpsString = _instring.Split();
                foreach (string item in gpsString)
                {
                    try
                    {
                        _GPS.Parse(item);
                    }
                    catch { }
                }
            }
        }
        public SerialPort SerialPort
        {
            get { return _comport; }
        }
        #endregion

        #region Members
        int _SatInView = 0, _SatInUse = 0;
        double _speed = 0.0, _bearing = 0.0, _hdop = 0.0, _vdop = 0.0, _pdop = 0.0, _ellipsoidHeight = 0.0;
        double _lon = 0.0, _lat = 0.0;

        public int SatInView { get{ return _SatInView; } }
        public int SatInUse { get { return _SatInUse; } }
        public double Speed { get { return _speed; } }
        public double Bearing { get { return _bearing; } }
        public double HDOP { get { return _hdop; } }
        public double VDOP { get { return _vdop; } }
        public double PDOP { get { return _pdop; } }
        public double EllipsoidHeight { get { return _ellipsoidHeight; } }
        public double Longitude { get { return _lon; } }
        public double Latitude { get { return _lat; } }
        #endregion 

        #region GPS data

        private void GPS_PositionReceived(string Lat, string Lon)
        {
            NMEACalc calc = new NMEACalc();
            calc.ParseNMEA(Lat, Lon);

            _lat = calc.deciLat;
            _lon = calc.deciLon;

            if (this.PositionReceived != null) this.PositionReceived(this, Lat, Lon, calc.deciLat, calc.deciLon);
        }

        private void GPS_SatellitesInViewReceived(int SatInView)
        {
            _SatInView = SatInView;

            if (this.SatellitesInViewReceived != null) this.SatellitesInViewReceived(this, SatInView);
        }

        private void GPS_SatellitesUsed(int SatInUse)
        {
            _SatInUse = SatInUse;

            if (this.SatellitesUsedReceived != null) this.SatellitesUsedReceived(this, SatInUse);
        }

        private void GPS_SpeedReceived(double Speed)
        {
            _speed = Speed;

            if (this.SpeedReceived != null) this.SpeedReceived(this, Speed);
        }

        private void GPS_SpeedLimitReached()
        {
            if (this.SpeedLimitReached != null) this.SpeedLimitReached(this);
        }

        private void GPS_BearingReceived(double Bearing)
        {
            _bearing = Bearing;

            if (this.BearingReceived != null) this.BearingReceived(this, Bearing);
        }

        void GPS_FixLost()
        {
            if (this.FixLost != null) this.FixLost(this);
        }

        void GPS_FixObtained()
        {
            if (this.FixObtained != null) this.FixObtained(this);
        }

        void GPS_HDOPReceived(double value)
        {
            _hdop = value;

            if (this.HDOPReceived != null) this.HDOPReceived(this, value);
        }

        void GPS_VDOPReceived(double value)
        {
            _vdop = value;

            if (this.VDOPReceived != null) this.VDOPReceived(this, value);
        }

        void GPS_PDOPReceived(double value)
        {
            _pdop = value;

            if (this.PDOPReceived != null) this.PDOPReceived(this, value);
        }

        void GPS_EllipsoidHeightReceived(double value)
        {
            _ellipsoidHeight = value;

            if (this.EllipsoidHeightReceived != null) this.EllipsoidHeightReceived(this, value);
        }

        private void GPS_SatelliteReceived(int pseudoRandomCode, int azimuth, int elevation, int signalToNoiseRatio)
        {
            if (this.SatelliteReceived != null)
                this.SatelliteReceived(this, pseudoRandomCode, azimuth, elevation, signalToNoiseRatio);
        }

        void GPS_DateTimeChanged(DateTime dateTime)
        {
            if(this.DateTimeChanged!=null) DateTimeChanged(this,dateTime);
        }

        #endregion
    }
}
