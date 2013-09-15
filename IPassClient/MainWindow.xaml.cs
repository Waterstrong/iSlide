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
using System.IO;

using NUI.Kinect;
using NUI.Data;
using NUI.Common;

namespace IPassClient
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
        // 对状态跟踪进行填充
        readonly Brush BrushCyan = new SolidColorBrush(Color.FromArgb(255, 27, 239, 9));
        readonly Brush BrushRed = new SolidColorBrush(Color.FromArgb(255, 255, 0, 0));

        private List<Label> labels = new List<Label>(); // 创建Ellipse集合

        private SkeletonKinect _skeletonKinect = new SkeletonKinect();
        private ColorKinect _colorKinect = new ColorKinect();

        const int BufferSize = 8192; // 缓存大小

        private TcpClient _tcpClient = null;

        /// <summary>
        /// 发送数据
        /// </summary>
        /// <param name="packet"></param>
        private void SendMessage(Packet packet)
        {
            NetworkStream streamToServer = _tcpClient.GetStream();
            byte[] buffer = Serialization.Serialize(packet);// packet.ChangeObjectToByte(); 
            // Encoding.Unicode.GetBytes(packet.ToString()); // 获得缓存
            streamToServer.Write(buffer, 0, buffer.Length); // 发往服务器
        }

        /// <summary>
        /// 创建标记Ellipse
        /// </summary>
        private void CreateLabels()
        {
            labels.Clear();
            while (labels.Count() <= 6)
            {
                Label label = new Label();
                label.Height = 28;
                label.Width = 35;
                label.FontSize = 14;
                label.HorizontalContentAlignment = HorizontalAlignment.Center;
                label.FontWeight = FontWeights.Bold;
                label.Background = BrushCyan;
                label.Opacity = 0.8;
                can_main.Children.Add(label);
                MapToScreenPosition(label, new Point(-50d, -50d));
                labels.Add(label);
            }
        }
        
        const string FilePath = "config//address.conf";
        /// <summary>
        /// 读取配置信息
        /// </summary>
        /// <param name="file"></param>
        private void ReadConfig(string file)
        {
            string strRead;
            char[] seperator = { ' ', ',', ';' };
            string[] strInfo = new string[0];
            try
            {
                strRead = File.ReadAllText(file);
                strInfo = strRead.Split(seperator, StringSplitOptions.RemoveEmptyEntries);
                textBox_ip.Text = strInfo[0];
                textBox_port.Text = strInfo[1];
                comboBox_position.SelectedIndex = int.Parse(strInfo[2]);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "IPassClient", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            
        }
        /// <summary>
        /// 保存配置信息
        /// </summary>
        /// <param name="file"></param>
        private void SaveConfig(string file)
        {
            string str = textBox_ip.Text.Trim() + " " + textBox_port.Text.Trim() + " " + comboBox_position.SelectedIndex.ToString();
            try
            {
                File.WriteAllText(file, str);
            }
            catch (System.Exception ex)
            {
                MessageBox.Show(ex.Message, "IPassClient", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        /// <summary>
        /// 窗体加载
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            CreateLabels();
            ReadConfig(FilePath);
            _skeletonKinect.PropertyChanged += new System.ComponentModel.PropertyChangedEventHandler(_skeletonKinect_PropertyChanged);
            _skeletonKinect.Subject.TargetCount += new SkeletonSubject.TargetCountEventHandler(Subject_TargetCount);
            _skeletonKinect.Subject.MapPosition += new SkeletonSubject.MapPositionEventHandler(Subject_MapPosition);
            _colorKinect.Subject.DataBinding += new ColorSubject.DataBindingEventHandler(Subject_DataBinding);

            _skeletonKinect.StartKinect();
            _colorKinect.StartKinect();
        }

        /// <summary>
        /// 映射到屏幕坐标点
        /// </summary>
        /// <param name="element"></param>
        /// <param name="point"></param>
        private void MapToScreenPosition(FrameworkElement element, Point point)
        {
            Canvas.SetLeft(element, point.X - element.Width / 2);
            Canvas.SetTop(element, point.Y - element.Height / 2);
        }

        /// <summary>
        /// 映射为屏幕位置
        /// </summary>
        /// <param name="datum"></param>
        void Subject_MapPosition(List<SkeletonData> datum)
        {
            //清除位置信息
            foreach (Label label in labels)
            {
                MapToScreenPosition(label, new Point(-50d, -50d));
            }
            // 将每个人与控件绑定
            for (int i = 0; i < datum.Count(); ++i)
            {
                labels[i].Content = datum[i].GetTrackId().ToString();
                MapToScreenPosition(labels[i], datum[i].PositionToScreen());
            }
            label_tarcnt.Content = datum.Count().ToString()+"人";
        }
        
        /// <summary>
        /// 显示RGB图像
        /// </summary>
        /// <param name="data"></param>
        void Subject_DataBinding(ColorData data)
        {
            //img_color.Source = new BitmapImage(new Uri("/image/error.jpg", UriKind.Relative));
            image_color.Source = data.Image.Source;
        }
        
        /// <summary>
        /// 已经获取到人数,开始发送数据
        /// </summary>
        /// <param name="tarcnt"></param>
        void Subject_TargetCount(int tarcnt)
        {
            if (textBlock_state.Text.Trim() == "离线")
            {
                return;
            }
            // 发送数据
            try
            {
                Packet packet = new Packet(null, comboBox_position.SelectedIndex.ToString(), tarcnt);
                SendMessage(packet);
                ShowLogMessage(packet);
            }
            catch (System.Exception ex)
            {
                button_server_Click(null, null); // 更改为离线状态
                MessageBox.Show(ex.Message, "IPassClient", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        /// <summary>
        /// 连接状态改变时发生
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void _skeletonKinect_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            // 当连接断开时提示
            if (e.PropertyName == "DisconnectedReason")
            {
                MessageBox.Show(_skeletonKinect.DisconnectedReason, "IPassClient", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }
        
        /// <summary>
        /// 窗口关闭
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            _skeletonKinect.StopKinect();
            _colorKinect.StopKinect();
        }
        static int IncreaseNumber = 0;
        const string ServerConnTip = "连接服务器";
        const string ServerDisConnTip = "断开服务器";
        private void button_server_Click(object sender, RoutedEventArgs e)
        {
            if (button_server.Content.ToString() == ServerConnTip) // 需要连接服务器
            {
                try
                {
                    _tcpClient = new TcpClient();
                    _tcpClient.Connect(textBox_ip.Text.Trim(), Convert.ToInt32(textBox_port.Text.Trim()));
                    Packet packet = new Packet("TCP Connection Test Success!", comboBox_position.SelectedIndex.ToString());
                    SendMessage(packet); // 发送测试信息
                    // 连接到的服务端信息
                    string tip = _tcpClient.Client.LocalEndPoint.ToString() + "-->" + _tcpClient.Client.RemoteEndPoint.ToString();
                    this.Title = "IPassClient——Server is connected！" + tip;
                    ShowLogMessage("连接服务器成功："+tip);
                    ShowLogMessage(packet);
                    button_server.Content = ServerDisConnTip;
                    textBlock_state.Text = "已连接";
                    SaveConfig(FilePath); // 保存配置文件
                    textBox_ip.IsEnabled = false;
                    textBox_port.IsEnabled = false;
                    comboBox_position.IsEnabled = false;
                }
                catch (System.Exception ex)
                {
                    ShowLogMessage(ex.Message);
                    MessageBox.Show(ex.Message, "IPassClient", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            else // 需要断开服务器
            {
                try
                {
                    _tcpClient.Close();
                    this.Title = "IPassClient——Server is not connected";
                    ShowLogMessage("服务器连接已断开！");
                    button_server.Content = ServerConnTip;
                    textBlock_state.Text = "离线";
                    textBox_ip.IsEnabled = true;
                    textBox_port.IsEnabled = true;
                    comboBox_position.IsEnabled = true;
                }
                catch (System.Exception ex)
                {
                    ShowLogMessage(ex.Message);
                    MessageBox.Show(ex.Message,"IPassClient", MessageBoxButton.OK, MessageBoxImage.Error);
                }
                
            }
        }
        void ShowLogMessage(Packet packet)
        {
            string position = packet.Position == "0" ? "门内" : "门外";
            richTextBox_msg.AppendText("[" + (IncreaseNumber++).ToString() + "] 已发送 :  设备位置：" + position +
                        "      采集人员数量：" + packet.Tarcnt.ToString() +
                        "      附加消息：" + packet.Message + "\n");
            richTextBox_msg.ScrollToEnd();
        }
        void ShowLogMessage(string msg)
        {
            richTextBox_msg.AppendText("[" + (IncreaseNumber++).ToString() + "] " + msg + "\n");
            richTextBox_msg.ScrollToEnd();
        }

    }
}
