using System;
using System.Management.Automation.Runspaces;

namespace GateFailoverService.Source.Util
{
    class HyperManager
    {
        private string vmName = null;
        public HyperManager(String vmName)
        {
            this.vmName = vmName;
        }
        public string GetVmName() { return this.vmName; }
        public string IsConfigured() { return (vmName!=null) ? "OK" : "FAIL"; }
        public void StartVM()
        {
            string strScriptBody = "$vm = Get-VM -Name " + this.vmName + " ; if ($vm.state -eq \"Off\"){ Start-VM $vm }";
            this.RunPsScript(strScriptBody);
        }
        public void StopVM()
        {
            string strScriptBody = "$vm = Get-VM -Name " + this.vmName + "; if ($vm.state -eq \"Running\"){ Stop-VM $vm -Force };";
            this.RunPsScript(strScriptBody);
        }
        private void RunPsScript(string strScriptBody)
        {
            Runspace runspace = RunspaceFactory.CreateRunspace();
            runspace.Open();
            Pipeline pipeLine = runspace.CreatePipeline();
            pipeLine.Commands.AddScript(strScriptBody);
            pipeLine.Commands.Add("Out-String");
            pipeLine.Invoke();
            runspace.Close();
        }
    }
}
