using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using CloudwatchLogManagement.Handlers;
using Amazon.CloudWatchLogs.Model;
using Amazon;
using System.Collections.Generic;
using System.IO;

namespace CloudwatchLogManagement_UnitTests
{
    [TestClass]
    public class awsUnitTests
    {
        AWS_CloudwatchLog awsC;

        [TestInitialize]
        public void TestInit()
        {
            /////////////////////////////// aws accesskeys credentials
            string awsAccessKey = "";
            string awsSecretAccessKey = "";
            RegionEndpoint awsEndPoint = RegionEndpoint.EUWest1;
            ///////////////////////////////

            awsC = new AWS_CloudwatchLog(awsAccessKey, awsSecretAccessKey, awsEndPoint);
        }

        [TestMethod]
        public void awsConnection()
        {
            AWS_CloudwatchLog awsC_bad = new AWS_CloudwatchLog("", "", RegionEndpoint.EUWest1);

            Assert.IsFalse(awsC_bad.awsConnection()); // Is checking if wrong credentials is returning a false
            Assert.IsTrue(awsC.awsConnection());
        }

        [TestMethod]
        public void awsGetLogGroup() //Pull all LogGroup from a aws account
        {
            bool LogGroupExist = false;

            DescribeLogGroupsResponse respon = awsC.GetLogGroups();

            foreach (var item in respon.LogGroups)
            {
                if (item.LogGroupName.Contains("ecs")) // /ecs/blade-*** is from the ML logs
                {
                    LogGroupExist = true;
                    break;
                }
            }
            Assert.IsTrue(LogGroupExist);
        }

        [TestMethod]
        public void awsGetLogStream() //Pull all LogStream from a LogGroup on a aws account
        {
            string awsLogGroup = "";

            DescribeLogGroupsResponse LogGroupRespon = awsC.GetLogGroups();

            foreach (var item in LogGroupRespon.LogGroups)
            {
                if (item.LogGroupName.Contains("ecs")) // /ecs/blade-*** is from the ML logs
                {
                    awsLogGroup = item.LogGroupName;
                    break;
                }
            }

            DescribeLogStreamsResponse LogStreamRespon = awsC.GetLogStreams(awsLogGroup);

            Assert.IsNotNull(LogStreamRespon);
        }

        [TestMethod] // May fail if there are no logs within the timespan that are set and may take some time to be done, if there is 
        public void awsGetLogsFrom() //Pull all Logs from a Logstream from a timespan on a aws account
        {
            string awsLogGroup = "";
            int awsLogFrom = -20;
            int awsLogTo = -15;

            DescribeLogGroupsResponse LogGroupRespon = awsC.GetLogGroups();

            foreach (var item in LogGroupRespon.LogGroups)
            {
                if (item.LogGroupName.Contains("/ecs/blade-seg")) // /ecs/blade-seg is from the ML logs
                {
                    awsLogGroup = item.LogGroupName;
                    break;
                }
            }

            DescribeLogStreamsResponse LogStreamRespon = awsC.GetLogStreams(awsLogGroup);

            List<GetLogEventsResponse> Logs = awsC.GetLogsFrom(awsLogGroup, LogStreamRespon, awsLogFrom, awsLogTo);

            bool containdata = false;

            foreach (var log in Logs)
            {
                foreach (var item in log.Events)
                {
                    if (!string.IsNullOrEmpty(item.Message))
                    {
                        containdata = true;
                        break;
                    }
                }
            }

            Assert.IsTrue(containdata);
        }
    }
}
