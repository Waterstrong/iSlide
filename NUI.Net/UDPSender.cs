using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.Net.Sockets;
using System.Net;
using System.IO;

namespace NUI.Net
{
    public class UDPSender : ISender
    {
        private IPAddress _ip = IPAddress.Parse("127.0.0.1");
        private int _port = 8500;

        public UDPSender()
        {
            string fileName = "config\\address.conf";
            string strRead;
            string[] strInfo = new string[0];
            char[] seperator = {':', '\n', '\r' };
            try
            {
                strRead = File.ReadAllText(fileName);
                strInfo = strRead.Split(seperator, StringSplitOptions.RemoveEmptyEntries);
                _ip = IPAddress.Parse(strInfo[1]);
                _port = int.Parse(strInfo[3]);
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public void SendMessage(string message)
        {
            try
            {
                UdpClient udpClient = new UdpClient();
                udpClient.Connect(_ip, _port);
                Byte[] sendBytes = Encoding.ASCII.GetBytes(message);
                udpClient.Send(sendBytes, sendBytes.Length);
                udpClient.Close();
            }
            catch (System.Exception ex)
            {
                throw new Exception(ex.Message);
            }
                
        }

    }
}
