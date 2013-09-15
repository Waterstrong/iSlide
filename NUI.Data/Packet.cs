//************************************
// @FileName: Packet.cs
// @Brief   : 数据包，可序列化
// @Author  : Waterstrong
// @DateTime: 2013-9-12 16:45:57
//************************************
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NUI.Data
{
    [Serializable()]
    public class Packet
    {
        public Packet(string msg = null, string pos = "0", int cnt = 0)
        {
            _message = msg;
            _position = pos;
            _tarcnt = cnt;
        }
        protected string _message; // 附加消息
        public string Message
        {
            get { return _message; }
            set { _message = value; }
        }
        protected string _position; // 设备位置
        public string Position
        {
            get { return _position; }
            set { _position = value; }
        }
        protected int _tarcnt; // 人数
        public int Tarcnt
        {
            get { return _tarcnt; }
            set { _tarcnt = value; }
        }
    }
}
