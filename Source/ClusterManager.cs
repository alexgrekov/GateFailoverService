using System;
using System.IO;
using System.Text;
using System.Threading;
using GateFailoverService.Source.Communication;
using GateFailoverService.Source.Util;

namespace GateFailoverService.Source
{
    class ClusterManager
    {
        private int WorkingFlag;
        private Logger logger;
        private Configurator config;
        private Cluster cluster;
        private EchoServer echoServer;
        private HyperManager hyperManager;

        public ClusterManager()
        {
            WorkingFlag = 1;
        }
        public void Run()
        {
            LoadConfigurator();
            LoadLogger();
            LoadEchoServer();
            LoadCluster();
            LoadHyperManager();
            
            #region MainLoop
            while (WorkingFlag == 1)  // MAIN LOOP
            {
                System.Threading.Thread.Sleep(5000); 
                SetLocalFreeRam();

                //============================== Am I alone or with friends?
                CheckQuorum();
                //============================== if alone, I have to power off the Machine
                if (!cluster.GetQuorum())
                    SetVmRunningFlag(0);
                //============================== Am I have to start VM here?
                if (cluster.GetVmRunningFlag() == 0 && cluster.GetQuorum())
                {
                    if (NeedStartLocalVM())
                    {
                        SetVmRunningFlag(1);
                        logger.Write("Here is the best place to run the VM!");
                    }
                }
                ApplyVMState();
            }
            #endregion
        }
        public void Stop() { this.WorkingFlag = 0; }
        #region Loaders
        private void LoadConfigurator()
        {
            this.config = new Configurator();
            Thread.Sleep(500);
            #region LoadCheck
            if (!this.config.Loaded())
            {
                WorkingFlag = 0;
            }
            #endregion
        }
        private void LoadLogger()
        {
            this.logger = new Logger();
        }
        private void LoadCluster()
        {
            string ip = this.config.GetParam("localIp");
            string nodes = this.config.GetParam("nodes");
            int tcpPort = Int32.Parse(this.config.GetParam("incomeTcpPort"));
            this.cluster = new Cluster(ip, tcpPort, nodes);

            Thread.Sleep(500);
            #region LoadCheck
            if (this.cluster.GetStatus() != "OK")
            {
                logger.Write("[ERROR] Cluster load fail! Exiting...");
                WorkingFlag = 0;
            }
            #endregion
        }
        private void LoadEchoServer()
        {
            int tcpPort = Int32.Parse(this.config.GetParam("incomeTcpPort"));
            string IP = this.config.GetParam("localIp");
            ulong freeRam = UtilsWMI.GetUnusedRAM();
            logger.Write("Local IP: " + IP + " local Port: " + tcpPort.ToString());
            this.echoServer = new EchoServer(IP, tcpPort, freeRam);
            
            Thread.Sleep(500);
            #region LoadCheck
            string echoServerStatus = this.echoServer.GetStatus();
            if (!(echoServerStatus == "WaitSleepJoin" || echoServerStatus == "Running"))
            {
                logger.Write("[ERROR] echoServer load fail! Status: " + echoServerStatus + "Exiting...");
                WorkingFlag = 0;
            }
            #endregion
        }
        private void LoadHyperManager()
        {
            string vmName = config.GetParam("vmName");
            this.hyperManager = new HyperManager(vmName);

            Thread.Sleep(500);
            #region LoadCheck
            if (this.hyperManager.IsConfigured() != "OK")
            {
                logger.Write("[ERROR] Hyper-V connect fail! Exiting...");
                WorkingFlag = 0;
            }
            #endregion
        }
        #endregion
        private int GetAliveNodesCount()
        {
            int counter = 0;
            NetSender ns = new NetSender();
            //logger.Write("Internal ping nodes:");
            string[] nodes = this.cluster.GetNodes();
            string localip = this.cluster.GetIP();
            foreach (string node in nodes)
            {
                if (!node.Contains(localip))
                {
                    //logger.Write("Internal ping node " + node + ":");
                    int result = ns.PingNode(node, this.cluster.GetTcpPort());
                    if (result == 1)
                    {
                        counter++;
                        //logger.Write(" Node: " + node + " - OK");
                    }
                    else
                    {
                        //logger.Write(" Node: " + node + " - NO PING");
                    }
                }
            }
            return counter;
        }
        private bool NeedStartLocalVM()
        {
            int counterWithRunnigVM = 0;        // Counter of nodes: VM is RUNNING. if 0 then check next counter
            int counterNotRunningMoreRAM = 0;   // Counter of nodes: VM is not running, RAM > local. if 0 then run vm
            NetSender ns = new NetSender();
            string[] nodes = this.cluster.GetNodes();
            string localip = this.cluster.GetIP();
            foreach (string node in nodes)
            {
                if (!node.Contains(localip))
                {
                    ulong nodeFreeRam = UtilsWMI.GetUnusedRAM();
                    NetSender.NodeReport report = ns.GetNodeReport(node, this.cluster.GetTcpPort());
                    if (report.nodeRunningVM)
                    {
                        logger.Write("Node: " + node + " - RUNNING, FREE RAM: "+report.nodeFreeRam.ToString());
                        counterWithRunnigVM++;
                    }
                    else
                    {
                        logger.Write("Node: " + node + " - NOT RUNNING, FREE RAM: "+ report.nodeFreeRam.ToString());
                        if (report.nodeFreeRam > nodeFreeRam)
                        {
                            logger.Write("    [ More free RAM then here ]");
                            counterNotRunningMoreRAM++;
                        }
                    }
                }
            }
            if ((counterWithRunnigVM == 0) && (counterNotRunningMoreRAM == 0)) 
                return true;
            return false;
        }
        private void CheckQuorum()
        {
            int aliveNodesCount = GetAliveNodesCount()+1; // include current node;
            int totalNodesCount = cluster.GetNodesCount();
            bool quorum = aliveNodesCount > totalNodesCount / 2;
            cluster.SetQuorum(quorum);
            logger.Write((quorum ? "Quorum:" : "No Quorum:") + " nodes " + aliveNodesCount.ToString() + "/" + totalNodesCount.ToString());
        }
        private void ApplyVMState()
        {
            if (cluster.GetVmRunningFlag() == 0)
            {
                logger.Write("------- VM ["+ hyperManager.GetVmName() +"] state: stopped");
                hyperManager.StopVM();
            }
            if (cluster.GetVmRunningFlag() == 1)
            {
                logger.Write("------- VM [" + hyperManager.GetVmName() + "] state: RUNNING");
                hyperManager.StartVM();
            }
        }
        private void SetVmRunningFlag(int value)
        {
            echoServer.SetVmRunningFlag(value);
            cluster.SetVmRunningFlag(value);
        }
        private void SetLocalFreeRam()
        {
            ulong freeRam = UtilsWMI.GetUnusedRAM();
            echoServer.SetLocalFreeRam(freeRam);
            logger.Write("Free RAM: " + freeRam.ToString());
        }
    }
}
