using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using NUI.Net;

namespace ReceiverTest
{
    class Program
    {
        static void Main(string[] args)
        {
            try
            {
                string msg = "";
                while (true)
                {
                    msg = UDPFactory.GetReceiver().ReceiveMessage();
                    if (msg.IndexOf("exit") != -1)
                    {
                        break;
                    }
                    Console.WriteLine(msg);
                }
            }
            catch (System.Exception ex)
            {
                Console.WriteLine("出错啦！" + ex.Message);
                Console.Write("按下回车键后退出");
                Console.ReadLine();
            }
        }
    }
}
