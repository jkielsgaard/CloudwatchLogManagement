using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Amazon;
using Amazon.CloudWatchLogs.Model;
using System.IO;

namespace CloudwatchLogManagement.Handlers
{
    public class Logfile
    {
        string _filepath = "";

        /// <summary>
        /// 
        /// </summary>
        /// <param name="filepath">The path to where the log file shall be saved</param>
        public Logfile(string filepath)
        {
            _filepath = filepath;
        }

        /// <summary>
        /// Log file streamwriter
        /// </summary>
        /// <param name="awsLogData">The data from AWS GetLogEventsResponse</param>
        public void write(List<GetLogEventsResponse> awsLogData)
        {
            List<string> log = new List<string>();

            foreach (var logs in awsLogData)
            {
                foreach (var item in logs.Events)
                {
                    using (StreamWriter file = new StreamWriter(_filepath, true))
                    {
                        file.WriteLine("[" + item.Timestamp + "], " + item.Message);
                    }
                }
            }
        }

    }
}
