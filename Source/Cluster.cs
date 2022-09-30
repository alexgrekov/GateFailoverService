using System;
using System.Linq;

namespace GateFailoverService.Source
{
    class Cluster
    {
        private String localIP;
        private readonly int tcpPort;
        private String[] nodes;
        private string status;
        private int nodesCount = 0;
        private int nodesAvaliableCount = 0;
        private bool quorum = false;

        private int vmRunningFlag = 0;

        public Cluster(String localIp, int tcpPort, String clusterNodes)
        {
            this.status = "OK";
            this.localIP = localIp;
            this.tcpPort = tcpPort;
            this.nodes = clusterNodes.Split(',');
            this.nodesCount = this.nodes.Count();
            this.nodesAvaliableCount = 0;
        }

        public int GetVmRunningFlag() { return this.vmRunningFlag; }
        public void SetVmRunningFlag(int vmRunningFlag) { this.vmRunningFlag = vmRunningFlag; }

        public string ShowInfo()
        {
            string result = "----- Cluster status ------\r\n";
            result += "Status: " + this.status + "\r\n";
            result += "Nodes avaliable: " + nodesAvaliableCount + " / Total: " + this.nodesCount + "\r\n";
            foreach (String node in this.nodes)
                result += "  Node: " + node + "\r\n";
            result += "---- End cluster repoert ----" + "\r\n";
            return result;
        }

        public int GetTcpPort() { return this.tcpPort; }
        public string GetStatus() { return this.status; }
        public string GetIP() { return this.localIP; }
        public string[] GetNodes() { return this.nodes; }

        public int GetNodesCount() { return this.nodes.Count(); }
        public void SetQuorum(bool value) { this.quorum = value;  }
        public bool GetQuorum() { return this.quorum; }
    }
}
