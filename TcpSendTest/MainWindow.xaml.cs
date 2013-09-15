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
using NUI.Data;
using NUI.Common;

namespace TcpSendTest
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
        const int BufferSize = 8192; // 缓存大小

        private TcpClient _client = new TcpClient();

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                _client.Connect("10.10.4.28", 8500); // 与服务器连接
                // 连接到的服务端信息
                this.Title = "Server Connected！" + _client.Client.LocalEndPoint.ToString() +
                    "-->" + _client.Client.RemoteEndPoint.ToString();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void button_send_Click(object sender, RoutedEventArgs e)
        {
            Packet packet = new Packet(textBox_msg.Text.Trim(), "0", 0);
            try
            {
                NetworkStream streamToServer = _client.GetStream();
                byte[] buffer = Serialization.Serialize(packet);// packet.ChangeObjectToByte(); // Encoding.Unicode.GetBytes(packet.ToString()); // 获得缓存
                streamToServer.Write(buffer, 0, buffer.Length); // 发往服务器
            }
            catch (System.Exception ex)
            {
                MessageBox.Show(ex.Message);
            }           
        }
    }
}
