using System;
using System.Text;
using System.Net.Sockets;

namespace GateFailoverService.Source.Communication
{
    class EchoServerWorker
    {
        private TcpClient client;
        private int vmRunningFlag;
        private ulong localFreeRam;
        public EchoServerWorker(TcpClient tcpClient, int vmRunningFlag, ulong localFreeRam)
        {
            this.client = tcpClient;
            this.vmRunningFlag = vmRunningFlag;
            this.localFreeRam = localFreeRam;
        }
        
        public void Process()
        {
            NetworkStream ns = null;
            try
            {
                ns = this.client.GetStream();
                byte[] data = new byte[64];
                StringBuilder builder = new StringBuilder();
                int bytes = 0;
                do
                {
                    bytes = ns.Read(data, 0, data.Length);
                    builder.Append(Encoding.Unicode.GetString(data, 0, bytes));
                }
                while (ns.DataAvailable);

                string message = builder.ToString();
                if (message.Contains("PING?"))
                {
                    message = "PING_OK";
                }else
                if (message.Contains("REPORT?"))
                {
                    if (this.vmRunningFlag == 1)
                    {
                        message = "VM_RUNNING";
                    }
                    else
                    {
                        message = "VM_NOT_RUNNING";
                    }
                    message += ",RAM="+localFreeRam.ToString();
                }

                data = Encoding.Unicode.GetBytes(message);
                ns.Write(data, 0, data.Length);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
            finally
            {
                if (ns != null)
                    ns.Close();
                if (this.client != null)
                    this.client.Close();
            }
        }

        public void SetVmRunningFlag(int value) { this.vmRunningFlag = value; }
    }
}
