//************************************
// @FileName: RemoteClient.cs
// @Brief   : 远程客户端访问控制
// @Author  : Waterstrong
// @DateTime: 2012-11-7 9:27:22
//************************************
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.Windows;
using System.Windows.Controls;

using NUI.Data;
using NUI.Common;

namespace IPassServer
{
    public class RemoteClient
    {

        static int IncreaseNumber = 1;

        private TcpClient _client;
        private NetworkStream _streamToClient;
        const int BufferSize = 8192; // 缓存大小
        private byte[] _buffer;
        // private RequestHandler handler;
       private MainWindow _win;
        private RichTextBox _rtb_msg;
        private TextBox _txb_total;

        private bool _isRecieve = true; // 是否继续接收信息
        public bool IsRecieve
        {
            get { return _isRecieve; }
            set { _isRecieve = value; }
        }
        public RemoteClient()
        {

        }
        public RemoteClient(TcpClient client, MainWindow win)
        {
            _rtb_msg = win.richTextBox_message;
            _txb_total = win.textBox_restnum;
            _win = win;

            //_total_num = Convert.ToInt32(win.textBox_restnum.Text.Trim());
            _client = client;
            _streamToClient = client.GetStream();
            _buffer = new byte[BufferSize];
            AsyncCallback callBack = new AsyncCallback(ReceivePacket);
            _streamToClient.BeginRead(_buffer, 0, BufferSize, callBack, null);
        }

        void NewAction()
        {

        }

        /// <summary>
        /// 接收包
        /// </summary>
        /// <param name="ar"></param>
        /// <returns></returns>
        public void ReceivePacket(IAsyncResult ar)
        {
            if (!_isRecieve)
            {
                DisposeClient();
                return;
            }
            int bytesRead = 0;
            try
            {
                lock (_streamToClient)
                {
                    bytesRead = _streamToClient.EndRead(ar);
                }
                if (bytesRead == 0)
                {
                    throw new Exception(_client.Client.RemoteEndPoint.ToString() + " 已经断开连接。");
                }

                Packet packet = (Packet)Serialization.Deserialize(_buffer);
                // string msg = Encoding.Unicode.GetString(_buffer, 0, bytesRead);
                Array.Clear(_buffer, 0, _buffer.Length); // 清空缓存

                //_win.rtb_msg.Dispatcher.Invoke(NewAction());
                _rtb_msg.Dispatcher.Invoke(new Action(() =>
                {
                    string position = packet.Position=="0" ? "门内" : "门外";
                    _rtb_msg.AppendText("【" + (IncreaseNumber++).ToString() + "】 Received :  设备位置：" + position +
                        "      采集人员数量：" + packet.Tarcnt.ToString() +
                        "      附加消息：" + packet.Message + "\n");
                    //_richTextBox.LineDown();
                    //_richTextBox.ScrollToVerticalOffset(100);
                    _rtb_msg.ScrollToEnd();
                }));

                // 实现判定人数
                if (packet.Position == "0")
                {
                    _win.Pos0_num = packet.Tarcnt;
                    if (_win.Pos1_num > 0) // 进去
                    {
                        _win.Total_num += _win.Pos0_num;
                        _win.Pos1_num = 0;
                        _win.Pos0_num = 0;
                    }
                }
                else if (packet.Position == "1")
                {
                    _win.Pos1_num = packet.Tarcnt;
                    if (_win.Pos0_num > 0) //出去
                    {
                        _win.Total_num -= _win.Pos1_num;
                        _win.Pos0_num = 0;
                        _win.Pos1_num = 0;
                    }
                }
                _txb_total.Dispatcher.Invoke(new Action(() =>
                {
                    _txb_total.Text = _win.Total_num.ToString();
                }));
                
                // 再次调用BeginRead，完成时调用自身
                lock (_streamToClient)
                {
                    AsyncCallback callBack = new AsyncCallback(ReceivePacket);
                    _streamToClient.BeginRead(_buffer, 0, BufferSize, callBack, null);
                }
            }
            catch (System.Exception ex)
            {
                DisposeClient();
                _rtb_msg.Dispatcher.Invoke(new Action(() =>
                {
                    _rtb_msg.AppendText("【" + (IncreaseNumber++).ToString() + "】 " + "RemoteClient : " + ex.Message + "\n");
                }));
            }
        }

        private void DisposeClient()
        {
            if (_streamToClient != null)
            {
                _streamToClient.Dispose();
            }
            _client.Close();
        }

    }
}
