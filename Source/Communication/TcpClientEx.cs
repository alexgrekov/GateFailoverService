using System;
using System.Net;
using System.Net.Sockets;

namespace GateFailoverService.Source.Communication
{
    public class TcpClientEx : TcpClient
    {
        public void Connect(String address, int port, int timeout)
        {
            IPAddress ip = IPAddress.Parse(address);
            var result = base.Client.BeginConnect(ip, port, null, null);
            while (!result.AsyncWaitHandle.WaitOne(timeout, true))
            {
                base.Client.Dispose();
                throw new Exception("Timeout error!");
            }
        }
    }
}
