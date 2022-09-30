using System.ServiceProcess;
using System.Threading;
using GateFailoverService.Source;

namespace GateFailoverService
{
    public partial class GateFailoverService : ServiceBase
    {
        private ClusterManager cluster;
        public GateFailoverService()
        {
            InitializeComponent(); 
            this.CanStop = true;
            this.CanPauseAndContinue = false;
            this.AutoLog = true;
        }

        protected override void OnStart(string[] args)
        {
            cluster = new ClusterManager();
            Thread.Sleep(100);
            Thread clusterThread = new Thread(new ThreadStart(cluster.Run));
            clusterThread.Start();
        }

        protected override void OnStop()
        {
            cluster.Stop();
            Thread.Sleep(1000);
        }
    }
}
