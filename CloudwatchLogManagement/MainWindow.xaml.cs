using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Amazon;
using Amazon.CloudWatchLogs.Model;
using CloudwatchLogManagement.Handlers;
using System.Threading;
using System.Windows.Threading;

namespace CloudwatchLogManagement
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        /// <summary>
        /// When the loading the window, it will populate the regionEndpoint combobox and start the loadingstatus
        /// </summary>
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            foreach (var item in RegionEndpoint.EnumerableAllRegions)
            {
                cbox_regionEndpoint.Items.Add(item.SystemName);
            }

            Thread LoadingStatus = new Thread(() => Loading());
            LoadingStatus.Start();
        }

        /// <summary>
        /// Is being used for to stop all threads running in the background when closing
        /// </summary>
        bool RunLoading = true;

        /// <summary>
        /// all the handlers and models that are in use
        /// </summary>
        ViewController vbn = new ViewController();
        DescribeLogStreamsResponse LogStreamRespon;

        /// <summary>
        /// Aws connect button make sure that the AWS credentials is correct and is not going diedlock if the user by mistake has selected the wrong regionendpoint.
        /// </summary>
        private void btn_awsConnect_Click(object sender, RoutedEventArgs e)
        {
            if (cbox_regionEndpoint.SelectedIndex > 0)
            {
                string Accesskey = pb_AccessKey.Password;
                string SAccesskey = pb_SecretAccessKey.Password;
                RegionEndpoint REndpoint = RegionEndpoint.GetBySystemName(cbox_regionEndpoint.SelectedValue.ToString());

                if (vbn.AWS_Login(Accesskey, SAccesskey, REndpoint))
                {
                    var task_LogsGroups = Task.Run(() => {
                        return vbn.GetLogsGroups();
                    });

                    bool done = task_LogsGroups.Wait(TimeSpan.FromSeconds(5));

                    if (done && task_LogsGroups.Result.LogGroups.Count >= 1)
                    {
                        cbox_LogGroups.IsEnabled = true;
                        foreach (var item in task_LogsGroups.Result.LogGroups)
                        {
                            cbox_LogGroups.Items.Add(item.LogGroupName);
                        }
                        cbox_LogGroups.SelectedIndex = 0;
                        btn_awsConnect.IsEnabled = false;
                    }
                    else { System.Windows.MessageBox.Show("No LogGroups could be found within the Timeout (5sec)"); }
                }
                else { System.Windows.MessageBox.Show("Wrong AWS credentials"); }
            }
            else { System.Windows.MessageBox.Show("No RegionEndpoint has been chosen"); }
        }

        /// <summary>
        /// When user selecting a Loggroup is will get all logstreams from that loggroup
        /// </summary>
        private void cbox_LogGroups_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            btn_SetTimeSpan.IsEnabled = true;
            dp_datefrom.IsEnabled = true;
            dp_dateto.IsEnabled = true;
            dp_datefrom.SelectedDate = DateTime.Today;
            dp_dateto.SelectedDate = DateTime.Today;

            LogStreamRespon = vbn.GetLogStreams(cbox_LogGroups.SelectedValue.ToString());
        }

        /// <summary>
        /// Will lock the chosen timspan
        /// </summary>
        private void btn_SetTimeSpan_Click(object sender, RoutedEventArgs e)
        {
            dp_datefrom.IsEnabled = false;
            dp_dateto.IsEnabled = false;
            btn_Getlogs.IsEnabled = true;
            btn_SetTimeSpan.IsEnabled = false;
        }


        /// <summary>
        /// Will collect and write the log is this process
        /// </summary>
        string CSV_filepath = null;
        private void btn_Getlogs_Click(object sender, RoutedEventArgs e)
        {

            using (var file = new SaveFileDialog())
            {
                file.Filter = "Log files|*.Log";
                if (file.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    CSV_filepath = file.FileName;
                }
            }
            if (!string.IsNullOrEmpty(CSV_filepath))
            {
                btn_awsConnect.IsEnabled = false;
                pb_AccessKey.IsEnabled = false;
                pb_SecretAccessKey.IsEnabled = false;
                cbox_regionEndpoint.IsEnabled = false;
                cbox_LogGroups.IsEnabled = false;
                btn_Getlogs.IsEnabled = false;

                grid_loading.Visibility = Visibility.Visible;

                TimeSpan From = dp_datefrom.SelectedDate.Value.Date - DateTime.Today;
                TimeSpan To = dp_dateto.SelectedDate.Value.Date - DateTime.Today;

                vbn.CollectLogData(cbox_LogGroups.SelectedValue.ToString(), LogStreamRespon, From, To, CSV_filepath);

                AwaitWork();
            }
        }

        /// <summary>
        /// Will await to collecting is done.
        /// </summary>
        public void AwaitWork()
        {
            new Thread(() =>
            {
                while (vbn.Work == false) { }

                grid_loading.Dispatcher.BeginInvoke(new Action(() => { grid_loading.Visibility = Visibility.Hidden; }));

                btn_awsConnect.Dispatcher.BeginInvoke(new Action(() => { btn_awsConnect.IsEnabled = true; }));
                pb_AccessKey.Dispatcher.BeginInvoke(new Action(() => { pb_AccessKey.IsEnabled = true; }));
                pb_SecretAccessKey.Dispatcher.BeginInvoke(new Action(() => { pb_SecretAccessKey.IsEnabled = true; }));
                dp_datefrom.Dispatcher.BeginInvoke(new Action(() => { dp_datefrom.IsEnabled = true; }));
                dp_dateto.Dispatcher.BeginInvoke(new Action(() => { dp_dateto.IsEnabled = true; }));
                cbox_regionEndpoint.Dispatcher.BeginInvoke(new Action(() => { cbox_regionEndpoint.IsEnabled = true; }));
                cbox_LogGroups.Dispatcher.BeginInvoke(new Action(() => { cbox_LogGroups.IsEnabled = true; }));
                btn_SetTimeSpan.Dispatcher.BeginInvoke(new Action(() => { btn_SetTimeSpan.IsEnabled = true; }));
            }).Start();
        }

        private void btn_close_Click(object sender, RoutedEventArgs e)
        {
            RunLoading = false;
            System.Windows.Application.Current.Shutdown();
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            RunLoading = false;
        }

        /// <summary>
        /// Is a simple loadingstatus for the user to see that the software are working
        /// </summary>
        public void Loading()
        {
            while (RunLoading)
            {
                lb_loading.Dispatcher.BeginInvoke(new Action(() => { lb_loading.Content = "Loading |"; }));
                Thread.Sleep(500);
                lb_loading.Dispatcher.BeginInvoke(new Action(() => { lb_loading.Content = "Loading /"; }));
                Thread.Sleep(500);
                lb_loading.Dispatcher.BeginInvoke(new Action(() => { lb_loading.Content = "Loading --"; }));
                Thread.Sleep(500);
                lb_loading.Dispatcher.BeginInvoke(new Action(() => { lb_loading.Content = "Loading \\"; }));
                Thread.Sleep(500);
            }
        }

    }
}
