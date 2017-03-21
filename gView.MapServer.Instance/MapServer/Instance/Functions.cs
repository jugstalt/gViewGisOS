using System;
using System.Collections.Generic;
using System.Text;

namespace gView.MapServer.Instance
{
    internal class Functions
    {
        private static int _maxThreads = 2, _queueLength = 100;

        public static string outputPath
        {
            get
            {
                return IMS.OutputPath;
            }
        }

        public static string tileCachePath
        {
            get
            {
                return IMS.TileCachePath;
            }
        }
        public static string outputUrl
        {
            get
            {
                return IMS.OutputUrl;
            }
        }

        public static int MaxThreads
        {
            get { return _maxThreads; }
            set { _maxThreads = value; }
        }
        public static int QueueLength
        {
            get { return _queueLength; }
            set { _queueLength = value; }
        }

        public static bool log_requests
        {
            get
            {
                if (System.Configuration.ConfigurationSettings.AppSettings["log_requests"] == null) return false;
                bool result = false;
                bool.TryParse(System.Configuration.ConfigurationSettings.AppSettings["log_requests"], out result);
                return result;
            } 
        }
        public static bool log_request_details
        {
            get
            {
                if (System.Configuration.ConfigurationSettings.AppSettings["log_request_details"] == null) return false;
                bool result = false;
                bool.TryParse(System.Configuration.ConfigurationSettings.AppSettings["log_request_details"], out result);
                return result;
            }
        }
        public static bool log_errors
        {
            get
            {
                if (System.Configuration.ConfigurationSettings.AppSettings["log_errors"] == null) return false;
                bool result = false;
                bool.TryParse(System.Configuration.ConfigurationSettings.AppSettings["log_errors"], out result);
                return result;
            }
        }
        public static bool log_archive
        {
            get
            {
                if (System.Configuration.ConfigurationSettings.AppSettings["log_archive"] == null) return false;
                bool result = false;
                bool.TryParse(System.Configuration.ConfigurationSettings.AppSettings["log_archive"], out result);
                return result;
            }
        }
    }

    
}
