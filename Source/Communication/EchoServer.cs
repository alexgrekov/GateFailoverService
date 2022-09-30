using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;


namespace GateFailoverService.Source.Communication
{
    class EchoServer
    {
        private Thread echoServerThread;

        private string localIp;
        private int localPort;
        private int vmRunningFlag = 0;
        private ulong localFreeRam = 0;

        public EchoServer(string IP, int tcpPort, ulong localFreeRam)
        {
            this.localIp = IP;
            this.localPort = tcpPort;
            this.localFreeRam = localFreeRam;
            this.echoServerThread = new Thread(new ThreadStart(this.Process));
            this.echoServerThread.Start();
        }
        
        public void Process()
        {
            TcpListener listener = null;
            try
            {
                listener = new TcpListener(IPAddress.Parse(this.localIp), this.localPort);
                listener.Start();
                while (true)
                {
                    TcpClient client = listener.AcceptTcpClient();
                    EchoServerWorker worker = new EchoServerWorker(client, this.vmRunningFlag, this.localFreeRam);
                    Thread clientThread = new Thread(new ThreadStart(worker.Process));
                    clientThread.Start();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
            finally
            {
                if (listener != null)
                    listener.Stop();
            }
        }
        public string GetStatus() { return this.echoServerThread.ThreadState.ToString(); }
        public void SetVmRunningFlag(int value) { this.vmRunningFlag = value; }
        public void SetLocalFreeRam(ulong value) { this.localFreeRam = value; }
    }
}
