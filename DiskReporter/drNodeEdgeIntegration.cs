using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data;
using System.IO;

namespace DiskReporter {
   class drNodeEdgeIntegration {
      string configDirectory = Directory.GetCurrentDirectory();
      string tsmConfig = System.IO.Path.DirectorySeparatorChar + "config_TSMServers.xml";
      string vCenterConfig = System.IO.Path.DirectorySeparatorChar + "config_vCenterServer.xml";
      string logName = "DiskReporter.log";

      public async Task<object> Invoke(dynamic rPath) {
         StreamWriter log;
         string relativePath = System.IO.Path.DirectorySeparatorChar + (String)rPath;
         if (!File.Exists(logName)) {
            log = new StreamWriter(logName);
         }
         else {
            log = File.AppendText(logName);
         }
         DiskReporterMainRunFlows programFlow = new DiskReporterMainRunFlows(log);
         var result = programFlow.FetchTsmVMwareNodeData(
               configDirectory + relativePath + tsmConfig,
               configDirectory + relativePath + vCenterConfig,
            serverNameFilter: String.Empty);

         System.Collections.Specialized.OrderedDictionary vmwareNodeDictionary = result.Item1;
         System.Collections.Specialized.OrderedDictionary tsmNodeDictionary = result.Item2;

         return new { VMware = vmwareNodeDictionary, TSM = tsmNodeDictionary };
      }
   }
}