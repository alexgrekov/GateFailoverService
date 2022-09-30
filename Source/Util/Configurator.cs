using System;
using System.IO;

namespace GateFailoverService.Source.Util
{
    class Configurator
    {
        private bool isLoaded;
        String[] config;
        public Configurator(String ConfPath = "C:\\GFS\\GateFailoverService.conf")
        {
            isLoaded = File.Exists(ConfPath);
            if (isLoaded)
                this.config = File.ReadAllLines(ConfPath);
        }
        
        public string GetParam(String paramName)
        {
            foreach (String param in config)
            {
                string paramFullName = "[" + paramName + "]=";
                if (param.Contains(paramFullName))
                {
                    return param.Replace(paramFullName, "");
                }
            }
            return "Parameter not found";
        }
        public bool Loaded() { return this.isLoaded; }
    }
}
