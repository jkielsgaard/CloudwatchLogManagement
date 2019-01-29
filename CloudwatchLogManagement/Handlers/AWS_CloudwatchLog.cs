using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Amazon;
using Amazon.CloudWatchLogs;
using Amazon.CloudWatchLogs.Model;
using System.Threading;

namespace CloudwatchLogManagement.Handlers
{
    public class AWS_CloudwatchLog
    {
        AmazonCloudWatchLogsClient awsCloudwatchClient;
        DescribeLogGroupsRequest awsLogGroup;
        DescribeLogStreamsRequest awsLogStream;
        GetLogEventsRequest awsGetLogsEvent;

        public AWS_CloudwatchLog(string awsAccessKey, string awsSecretAccessKey, RegionEndpoint awsEndpoint)
        {
            awsCloudwatchClient = new AmazonCloudWatchLogsClient(awsAccessKey, awsSecretAccessKey, awsEndpoint);
        }

        /// <summary>
        /// To check if the credentials is right
        /// </summary>
        /// <returns>Return a bool verb after trying to connect to AWS</returns>
        public bool awsConnection()
        {
            awsLogGroup = new DescribeLogGroupsRequest();
            try
            {
                awsCloudwatchClient.DescribeLogGroups(awsLogGroup);
                return true;
            }
            catch { return false; }
        }

        /// <summary>
        /// Get all the LogGroup on the RegionEndpoint
        /// </summary>
        /// <returns>return the LogGroup found</returns>
        public DescribeLogGroupsResponse GetLogGroups()
        {
            awsLogGroup = new DescribeLogGroupsRequest();
            return awsCloudwatchClient.DescribeLogGroups(awsLogGroup);
        }

        /// <summary>
        /// Get all the LogStream from the chosen LogGroup
        /// </summary>
        /// <param name="awsLogGroupName"></param>
        /// <returns>return all LogStreams from the chosen LogGroup</returns>
        public DescribeLogStreamsResponse GetLogStreams(string awsLogGroupName)
        {
            awsLogStream = new DescribeLogStreamsRequest();
            awsLogStream.LogGroupName = awsLogGroupName;
            return awsCloudwatchClient.DescribeLogStreams(awsLogStream);
        }

        /// <summary>
        /// Get and write logs to chosen file, This is not at right way to do it, but every task, thread, async/await either lock the GUI or is not working.
        /// </summary>
        /// <param name="awsLogGroupName"></param>
        /// <param name="awsLogStreamName"></param>
        /// <param name="fromdays"></param>
        /// <param name="todays"></param>
        public List<GetLogEventsResponse> GetLogsFrom(string awsLogGroupName, DescribeLogStreamsResponse awsLogStreamName, double fromdays, double todays)
        {
            List<GetLogEventsResponse> Logs = new List<GetLogEventsResponse>();

            foreach (var item in awsLogStreamName.LogStreams)
            {
                awsGetLogsEvent = new GetLogEventsRequest();
                awsGetLogsEvent.LogGroupName = awsLogGroupName;
                awsGetLogsEvent.LogStreamName = item.LogStreamName;
                awsGetLogsEvent.StartTime = DateTime.Today.AddDays(fromdays);
                awsGetLogsEvent.EndTime = DateTime.Today.AddDays(todays + 1);
                Logs.Add(awsCloudwatchClient.GetLogEvents(awsGetLogsEvent));
            }

            return Logs;
        }
    }
}
