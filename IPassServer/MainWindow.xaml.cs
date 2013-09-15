using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace IPassServer
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private int _pos0_num = 0;
        public int Pos0_num
        {
            get { return _pos0_num; }
            set { _pos0_num = value; }
        }
        private int _pos1_num = 0;
        public int Pos1_num
        {
            get { return _pos1_num; }
            set { _pos1_num = value; }
        }
        private int _total_num = 0;
        public int Total_num
        {
            get { return _total_num; }
            set { _total_num = value; }
        }

        private Thread _listenThread = null; // 线程对象
        private TcpListener _listener = null; // TCP监听

        private RemoteClient _remoteClient = null;
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            
        }

        /// <summary>
        /// 接受消息
        /// </summary>
        private void ReceiveMsg()
        {
            while (_listener != null)
            {
                // 获取一个连接，中断方法
                TcpClient client = _listener.AcceptTcpClient();
                // 打印连接到的客户端信息
                richTextBox_message.Dispatcher.Invoke(new Action(() =>
                {
                    richTextBox_message.AppendText("【Client Connected！ " + client.Client.LocalEndPoint.ToString() + "  Accept  " + client.Client.RemoteEndPoint.ToString() + "】\n");
                }));
                _remoteClient = new RemoteClient(client, this);
                _remoteClient.IsRecieve = true;
            }
        }
        /// <summary>
        /// 开启服务
        /// </summary>
        private void StartServer()
        {
            IPAddress[] ipList = Dns.GetHostAddresses(Dns.GetHostName());
            foreach (IPAddress ip in ipList)
            {
                if (ip.AddressFamily == AddressFamily.InterNetwork)
                {
                    richTextBox_message.AppendText("[" + ip.ToString() + "] 服务器开始运行……\n");
                    _listener = new TcpListener(ip, 8500);
                    _listener.Start();           // 开始侦听
                    richTextBox_message.AppendText("服务器正在监听……\n");
                    break;
                }
            }
            //开一个线程
            _listenThread = new Thread(new ThreadStart(ReceiveMsg));
            _listenThread.Start();
        }

        /// <summary>
        /// 停止服务
        /// </summary>
        private void StopServer()
        {
            if (_listener != null)
            {
                _listenThread.Abort();
                _listener.Stop();
                if (_remoteClient != null)
                {
                    _remoteClient.IsRecieve = false;
                }
                _listener = null;
                richTextBox_message.AppendText("服务器已停止运行！\n");
            }
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            StopServer();
        }

        const string ServerStartTip = "开启服务";
        const string ServerStopTip = "停止服务";
        private void button_startstop_Click(object sender, RoutedEventArgs e)
        {
            if (button_startstop.Content.ToString() == ServerStartTip)
            {
                StartServer();
                button_startstop.Content = ServerStopTip;
            }
            else
            {
                StopServer();
                button_startstop.Content = ServerStartTip;
            }
     
        }

        private void button_clear_Click(object sender, RoutedEventArgs e)
        {
            
        }

    }
}
