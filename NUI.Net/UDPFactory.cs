using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NUI.Net
{
    public static class UDPFactory
    {
        private static ISender _sender = null;
        private static IReceiver _receiver = null;
        public static ISender GetSender()
        {
            if (_sender == null)
            {
                try
                {
                     _sender = new UDPSender();
                }
                catch (System.Exception ex)
                {
                    throw new Exception(ex.Message);
                }
            }
            return _sender;
        }
        public static IReceiver GetReceiver()
        {
            if(_receiver == null)
            {
                try
                {
                    _receiver = new UDPReceiver();
                }
                catch (System.Exception ex)
                {
                    throw new Exception(ex.Message);
                }
            }
            return _receiver;
        }
    }
}
