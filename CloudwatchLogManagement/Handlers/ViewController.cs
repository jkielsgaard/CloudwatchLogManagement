using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CloudwatchLogManagement.Handlers;
using Amazon;
using Amazon.CloudWatchLogs.Model;
using System.Threading;

namespace CloudwatchLogManagement.Handlers
{
    /// <summary>
    /// ViewBackend is handling all work behind MainWindow.view to avoid deadlocks on the GUI
    /// </summary>

    public class ViewController
    {
        AWS_CloudwatchLog awslog;

        /// <summary>
        /// Check the aws credentials
        /// </summary>
        /// <param name="AccessKey">User AccessKey</param>
        /// <param name="SAccessKey">User Secret AccessKey</param>
        /// <param name="REndpoint">RegionEndpoint on AWS</param>
        /// <returns>return bool after the check, true if succeed</returns>
        public bool AWS_Login(string AccessKey, string SAccessKey, RegionEndpoint REndpoint)
        {
            awslog = new AWS_CloudwatchLog(AccessKey, SAccessKey, REndpoint);

            if (awslog.awsConnection())
            {
                return true;
            }
            else { return false; }
        }

        /// <summary>
        /// Get and return DescribeLogGroupsResponse
        /// </summary>
        /// <returns>return DescribeLogGroupsResponse</returns>
        public DescribeLogGroupsResponse GetLogsGroups()
        {
            return awslog.GetLogGroups();
        }

        /// <summary>
        /// Get and return DescribeLogStreamsResponse
        /// </summary>
        /// <param name="awsLogStream">string parameter on witch logstream to search on</param>
        /// <returns>return DescribeLogStreamsResponse</returns>
        public DescribeLogStreamsResponse GetLogStreams(string awsLogStream)
        {
            return awslog.GetLogStreams(awsLogStream);
        }

        /// <summary>
        /// Starting the log collection process
        /// </summary>
        /// <param name="LogGroup">AWS LogGroup </param>
        /// <param name="LogStreamRespon">DescribeLogStreamsResponse logsteam from loggroup</param>
        /// <param name="From">TimeSpan date from</param>
        /// <param name="To">TimeSpan date to</param>
        /// <param name="CSV_filepath">Path to where the log file shall be saved</param>
        public void CollectLogData(string LogGroup, DescribeLogStreamsResponse LogStreamRespon, TimeSpan From, TimeSpan To, string CSV_filepath)
        {
            new Thread(() => CollectLogDataThread(LogGroup, LogStreamRespon, From, To, CSV_filepath)).Start();
        }

        /// <summary>
        /// The collecting process in a thread. when done is will set the Done bool to true
        /// </summary>
        /// <param name="LogGroup">AWS LogGroup </param>
        /// <param name="LogStreamRespon">DescribeLogStreamsResponse logsteam from loggroup</param>
        /// <param name="From">TimeSpan date from</param>
        /// <param name="To">TimeSpan date to</param>
        /// <param name="CSV_filepath">Path to where the log file shall be saved</param>
        private void CollectLogDataThread(string LogGroup, DescribeLogStreamsResponse LogStreamRespon, TimeSpan From, TimeSpan To, string CSV_filepath)
        {
            Logfile l = new Logfile(CSV_filepath);
            List<GetLogEventsResponse> Logs = awslog.GetLogsFrom(LogGroup, LogStreamRespon, From.TotalDays, To.TotalDays);
            l.write(Logs);

            Done = true;
        }

        bool Done = false;
        public bool Work { get { return Done; } }

    }
}
