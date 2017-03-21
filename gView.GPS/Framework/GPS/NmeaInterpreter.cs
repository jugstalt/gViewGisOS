using System;
using System.Globalization;

namespace gView.Framework.GPS
{
    internal class NmeaInterpreter
    {
        // Represents the EN-US culture, used for numers in NMEA sentences
        public static CultureInfo NmeaCultureInfo = new CultureInfo("en-US");
        // Used to convert knots into miles per hour
        public static double MPHPerKnot = double.Parse("1.150779",
          NmeaCultureInfo);
        #region Delegates
        internal delegate void PositionReceivedEventHandler(string latitude,
          string longitude);
        internal delegate void DateTimeChangedEventHandler(System.DateTime dateTime);
        internal delegate void BearingReceivedEventHandler(double bearing);
        internal delegate void SpeedReceivedEventHandler(double speed);
        internal delegate void SpeedLimitReachedEventHandler();
        internal delegate void FixObtainedEventHandler();
        internal delegate void FixLostEventHandler();
        internal delegate void SatelliteReceivedEventHandler(
          int pseudoRandomCode, int azimuth, int elevation,
          int signalToNoiseRatio);
        internal delegate void HDOPReceivedEventHandler(double value);
        internal delegate void VDOPReceivedEventHandler(double value);
        internal delegate void PDOPReceivedEventHandler(double value);
        internal delegate void SatellitesInViewReceivedEventHandler(int value);
        internal delegate void SatellitesUsedReceivedEventHandler(int value);
        internal delegate void EllipsoidHeightReceivedEventHandler(double value);



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
        public event SatellitesUsedReceivedEventHandler SatellitesUsed;
        public event EllipsoidHeightReceivedEventHandler EllipsoidHeightReceived;
        #endregion

        // Processes information from the GPS receiver
        public bool Parse(string sentence)
        {
            // Discard the sentence if its checksum does not match our
            // calculated checksum
            if (!IsValid(sentence)) return false;
            // Look at the first word to decide where to go next
            switch (GetWords(sentence)[0])
            {
                case "$GPRMC":
                    // A "Recommended Minimum" sentence was found!
                    return ParseGPRMC(sentence);
                case "$GPGSV":
                    // A "Satellites in View" sentence was recieved
                    return ParseGPGSV(sentence);
                case "$GPGSA":
                    return ParseGPGSA(sentence);
                case "$GPGGA":
                    return ParseGPGGA(sentence);
                default:
                    // Indicate that the sentence was not recognized
                    return false;
            }
        }
        // Divides a sentence into individual words
        public string[] GetWords(string sentence)
        {
            //strip off the final * + checksum
            sentence = sentence.Substring(0, sentence.IndexOf("*"));
            //now split it up
            return sentence.Split(',');
        }
        // Interprets a $GPRMC message
        public bool ParseGPRMC(string sentence)
        {
            // Divide the sentence into words
            string[] Words = GetWords(sentence);
            // Do we have enough values to describe our location?
            if (Words[3] != "" & Words[4] != "" &
              Words[5] != "" & Words[6] != "")
            {
                // Yes. Extract latitude and longitude
                // Append hours
                string Latitude = Words[3].Substring(0, 2) + "\u00b0";
                // Append minutes
                Latitude = Latitude + Words[3].Substring(2) + "\"";
                // Append hours
                Latitude = Latitude + Words[4]; // Append the hemisphere
                string Longitude = Words[5].Substring(0, 3) + "\u00b0";
                // Append minutes
                Longitude = Longitude + Words[5].Substring(3) + "\"";
                // Append the hemisphere
                Longitude = Longitude + Words[6];
                // Notify the calling application of the change
                if (PositionReceived != null)
                    PositionReceived(Latitude, Longitude);
            }
            // Do we have enough values to parse satellite-derived time?
            if (Words[1] != "")
            {
                // Yes. Extract hours, minutes, seconds and milliseconds
                int UtcHours = Convert.ToInt32(Words[1].Substring(0, 2));
                int UtcMinutes = Convert.ToInt32(Words[1].Substring(2, 2));
                int UtcSeconds = Convert.ToInt32(Words[1].Substring(4, 2));
                int UtcMilliseconds = 0;
                // Extract milliseconds if it is available
                if (Words[1].Length > 7)
                {
                    UtcMilliseconds = Convert.ToInt32(Words[1].Substring(7));
                }
                // Now build a DateTime object with all values
                System.DateTime Today = System.DateTime.Now.ToUniversalTime();
                System.DateTime SatelliteTime = new System.DateTime(Today.Year,
                  Today.Month, Today.Day, UtcHours, UtcMinutes, UtcSeconds,
                  UtcMilliseconds);
                // Notify of the new time, adjusted to the local time zone
                if (DateTimeChanged != null)
                    DateTimeChanged(SatelliteTime.ToLocalTime());
            }
            // Do we have enough information to extract the current speed?
            if (Words[7] != "")
            {
                // Yes.  Parse the speed and convert it to MPH
                double Speed = double.Parse(Words[7], NmeaCultureInfo) *
                  MPHPerKnot;
                // Notify of the new speed
                if (SpeedReceived != null)
                    SpeedReceived(Speed);
                // Are we over the highway speed limit?
                if (Speed > 55)
                    if (SpeedLimitReached != null)
                        SpeedLimitReached();
            }
            // Do we have enough information to extract bearing?
            if (Words[8] != "")
            {
                // Indicate that the sentence was recognized
                double Bearing = double.Parse(Words[8], NmeaCultureInfo);
                if (BearingReceived != null)
                    BearingReceived(Bearing);
            }
            // Does the device currently have a satellite fix?
            if (Words[2] != "")
            {
                switch (Words[2])
                {
                    case "A":
                        if (FixObtained != null)
                            FixObtained();
                        break;
                    case "V":
                        if (FixLost != null)
                            FixLost();
                        break;
                }
            }
            // Indicate that the sentence was recognized
            return true;
        }
        // Interprets a "Satellites in View" NMEA sentence
        public bool ParseGPGSV(string sentence)
        {
            int PseudoRandomCode = 0;
            int Azimuth = 0;
            int Elevation = 0;
            int SignalToNoiseRatio = 0;
            // Divide the sentence into words
            string[] Words = GetWords(sentence);
            // Each sentence contains four blocks of satellite information. 
            // Read each block and report each satellite's information
            int Count = 0;
            for (Count = 1; Count <= 4; Count++)
            {
                // Do we have enough values to parse satellitesIinView?
                if (Words[3] != "")
                {
                    if (SatellitesInViewReceived != null)
                        SatellitesInViewReceived(int.Parse(Words[3]));

                }

                // Does the sentence have enough words to analyze?
                if ((Words.Length - 1) >= (Count * 4 + 3))
                {
                    // Yes.  Proceed with analyzing the block. 
                    // Does it contain any information?
                    if (Words[Count * 4] != "" & Words[Count * 4 + 1] != ""
                      & Words[Count * 4 + 2] != "" & Words[Count * 4 + 3] != "")
                    {
                        // Yes. Extract satellite information and report it
                        PseudoRandomCode = System.Convert.ToInt32(Words[Count * 4]);
                        Elevation = Convert.ToInt32(Words[Count * 4 + 1]);
                        Azimuth = Convert.ToInt32(Words[Count * 4 + 2]);
                        SignalToNoiseRatio = Convert.ToInt32(Words[Count * 4 + 3]);
                        // Notify of this satellite's information
                        if (SatelliteReceived != null)
                            SatelliteReceived(PseudoRandomCode, Azimuth,
                            Elevation, SignalToNoiseRatio);
                    }
                }
            }
            // Indicate that the sentence was recognized
            return true;
        }
        // Interprets a "Fixed Satellites and DOP" NMEA sentence
        public bool ParseGPGSA(string sentence)
        {
            // Divide the sentence into words
            string[] Words = GetWords(sentence);
         
            // Update the DOP values
            if (Words.Length>15 && Words[15] != "")
            {
                if (PDOPReceived != null)
                    PDOPReceived(double.Parse(Words[15], NmeaCultureInfo));
            }
            if (Words.Length>16 && Words[16] != "")
            {
                if (HDOPReceived != null)
                    HDOPReceived(double.Parse(Words[16], NmeaCultureInfo));
            }
            if (Words.Length>17 && Words[17] != "")
            {
                if (VDOPReceived != null)
                    VDOPReceived(double.Parse(Words[17], NmeaCultureInfo));
            }
            return true;
        }
        // Returns True if a sentence's checksum matches the

        //  Interprets a $GPGGA message
        public bool ParseGPGGA(string sentence)
        {
            // Divide the sentence into words
            string[] Words = GetWords(sentence);
            // Satellites Used
            if (Words.Length>7 && Words[7] != "")
            {
                if (SatellitesUsed != null)
                    SatellitesUsed(int.Parse(Words[7]));
            }
            if (Words.Length>8 && Words[8] != "")
            {
                if (HDOPReceived != null)
                    HDOPReceived(double.Parse(Words[8], NmeaCultureInfo));
            }

            if (Words.Length>9 && Words[9] != "")
            {
                if (EllipsoidHeightReceived != null)
                    EllipsoidHeightReceived(double.Parse(Words[9]));
            }
            //

            return true;
        }
        // Returns True if a sentence's checksum matches the

        // calculated checksum
        public bool IsValid(string sentence)
        {
            // Compare the characters after the asterisk to the calculation
            return sentence.Substring(sentence.IndexOf("*") + 1) ==
              GetChecksum(sentence);
        }
        // Calculates the checksum for a sentence
        public string GetChecksum(string sentence)
        {
            // Loop through all chars to get a checksum
            int Checksum = 0;
            foreach (char Character in sentence)
            {
                if (Character == '$')
                {
                    // Ignore the dollar sign
                }
                else if (Character == '*')
                {
                    // Stop processing before the asterisk
                    break;
                }
                else
                {
                    // Is this the first value for the checksum?
                    if (Checksum == 0)
                    {
                        // Yes. Set the checksum to the value
                        Checksum = Convert.ToByte(Character);
                    }
                    else
                    {
                        // No. XOR the checksum with this character's value
                        Checksum = Checksum ^ Convert.ToByte(Character);
                    }
                }
            }
            // Return the checksum formatted as a two-character hexadecimal
            return Checksum.ToString("X2");
        }
    }

    internal class NMEACalc
    {
        public double deciLat;
        public double deciLon;

        public void ParseNMEA(string Nlat, string Nlon)
        {
            //grab the bit up to the °
            deciLat = Convert.ToDouble(Nlat.Substring(0, Nlat.IndexOf("\u00b0")));
            deciLon = Convert.ToDouble(Nlon.Substring(0, Nlon.IndexOf("\u00b0")));

            //remove that bit from the string now we've used it and the ° symbol
            Nlat = Nlat.Substring(Nlat.IndexOf("\u00b0") + 1);
            Nlon = Nlon.Substring(Nlon.IndexOf("\u00b0") + 1);

            //grab the bit up to the " - divide by 60 to convert to degrees and add it to our double value
            deciLat += (Convert.ToDouble(Nlat.Substring(0, Nlat.IndexOf("\"")).Replace(".", ","))) / 60;
            deciLon += (Convert.ToDouble(Nlon.Substring(0, Nlat.IndexOf("\"")).Replace(".", ","))) / 60;

            //ok remove that now and just leave the compass direction
            Nlat = Nlat.Substring(Nlat.IndexOf("\"") + 1);
            Nlon = Nlon.Substring(Nlon.IndexOf("\"") + 1);

            // check for negative directions
            if (Nlat == "S") deciLat = 0 - deciLat;
            if (Nlon == "W") deciLon = 0 - deciLon;
        }
    }
}
