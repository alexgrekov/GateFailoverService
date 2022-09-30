using System;
using System.Text;
using System.Net.Sockets;

namespace GateFailoverService.Source.Communication
{
    class NetSender
    {
        public int PingNode(string ip, int port)
        {
            TcpClientEx client = null;
            int result = 0;
            try
            {
                client = new TcpClientEx();
                client.Connect(ip, port, 1000);
                NetworkStream stream = client.GetStream();

                string message = "PING?";
                byte[] data = Encoding.Unicode.GetBytes(message);
                stream.Write(data, 0, data.Length);

                data = new byte[64];
                StringBuilder builder = new StringBuilder();
                int bytes = 0;
                do
                {
                    bytes = stream.Read(data, 0, data.Length);
                    builder.Append(Encoding.Unicode.GetString(data, 0, bytes));
                }
                while (stream.DataAvailable);

                message = builder.ToString();
                if (message.Contains("PING_OK"))
                    result = 1;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
            finally
            {
                client.Close();
            }
            return result;
        }
        public NodeReport GetNodeReport(string ip, int port)
        {
            TcpClientEx client = null;
            NodeReport result = new NodeReport();
            try
            {
                client = new TcpClientEx();
                client.Connect(ip, port, 1000);
                NetworkStream stream = client.GetStream();

                string message = "REPORT?";
                byte[] data = Encoding.Unicode.GetBytes(message);
                stream.Write(data, 0, data.Length);

                result.nodeAvalible = true;
                
                data = new byte[64];
                StringBuilder builder = new StringBuilder();
                int bytes = 0;
                do
                {
                    bytes = stream.Read(data, 0, data.Length);
                    builder.Append(Encoding.Unicode.GetString(data, 0, bytes));
                }
                while (stream.DataAvailable);

                message = builder.ToString();
                result.Parse(message);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
            finally
            {
                client.Close();
            }
            return result;
        }
        public class NodeReport
        {
            public bool nodeAvalible = false;
            public ulong nodeFreeRam = 0;
            public bool nodeRunningVM = false;
            public void Parse(string message)
            {
                string report = message;
                Console.WriteLine(report);
                if (report.Contains("VM_RUNNING"))
                {
                    this.nodeRunningVM = true;
                    report = report.Replace("VM_RUNNING,RAM=", "");
                }
                else
                {
                    report = report.Replace("VM_NOT_RUNNING,RAM=", "");
                }
                this.nodeFreeRam = UInt64.Parse(report);
            }
        }
    }
}
