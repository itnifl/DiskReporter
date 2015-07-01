using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data;
using System.IO;
using System.Collections.Specialized;

namespace DiskReporter {
   class drNodeEdgeIntegration {
      string configDirectory = Directory.GetCurrentDirectory();
      string tsmConfig = System.IO.Path.DirectorySeparatorChar + "config_TSMServers.xml";
      string vCenterConfig = System.IO.Path.DirectorySeparatorChar + "config_vCenterServer.xml";
      string logName = "DiskReporter.log";
      string objectLogRelativePath = "";
      Boolean newLogFileCreated = false;

      ~drNodeEdgeIntegration() {
         if (newLogFileCreated) File.Delete(objectLogRelativePath + logName);
      }
      /// <summary>
      /// Returns all nodes and theyr data as one dictionary fetched from the VMware and TSM plugins
      /// </summary>
      /// <returns>{ ServerCollection = serverCollection, TotalStorage = totalStorageSum }</returns>
      /// <param name="rPath">String representing the path that the XML configuration is in under</param>
      public async Task<object> FetchVMwareAndTSMServerData(dynamic rPath) {
         StreamWriter log;
         List<Object> serverCollection = new List<Object>();
         Dictionary<string, long> totalStorageSum = new Dictionary<string, long>();         
         string relativePath = System.IO.Path.DirectorySeparatorChar + (String)rPath;
         objectLogRelativePath = configDirectory + relativePath + System.IO.Path.DirectorySeparatorChar;

         if (!File.Exists(objectLogRelativePath + logName)) {
            log = new StreamWriter(objectLogRelativePath + logName);
         }
         else {
            //Create new file, and if so delete it afterwards:
            try {
               log = File.AppendText(objectLogRelativePath + logName);
            } catch {
               newLogFileCreated = true;
               logName = logName.Split('.')[0] + new Random().Next(10,10000).ToString() + '.' + logName.Split('.')[1];
               log = new StreamWriter(objectLogRelativePath + logName);
            }
         }
         OrderedDictionary vmwareNodeDictionary = new OrderedDictionary();
         OrderedDictionary tsmNodeDictionary = new OrderedDictionary();
         try {
            DiskReporterMainRunFlows programFlow = new DiskReporterMainRunFlows(log);
            var result = programFlow.FetchTsmVMwareNodeData(
                  objectLogRelativePath + tsmConfig,
                  objectLogRelativePath + vCenterConfig,
               serverNameFilter: String.Empty);
            vmwareNodeDictionary = result.Item1;
            tsmNodeDictionary = result.Item2;
         } catch (Exception ex) {
            log.WriteLine(DateTime.Now + " - Error: " + ex.ToString());
         }

         try {
            foreach (var key in vmwareNodeDictionary.Keys) {
               if (!key.ToString().Equals("TotalCollectionStorage") && !key.ToString().Equals("TotalCollectionWindowsSystemStorage") && !key.ToString().Equals("TotalCollectionLinuxRootStorage")) serverCollection.Add(vmwareNodeDictionary[key]);
               else if (!String.IsNullOrEmpty(key.ToString()) && !String.IsNullOrEmpty(vmwareNodeDictionary[key].ToString())) {
                  totalStorageSum.Add(key.ToString(), Int64.Parse(vmwareNodeDictionary[key].ToString()));
               }
            }
         } catch (Exception ex) {
            log.WriteLine(DateTime.Now + " - Error: " + ex.ToString());
         }
         try {
            foreach (var key in tsmNodeDictionary.Keys) {
               if (!key.ToString().Equals("TotalCollectionStorage") && !key.ToString().Equals("TotalCollectionWindowsSystemStorage") && !key.ToString().Equals("TotalCollectionLinuxRootStorage")) {
                  serverCollection.Add(tsmNodeDictionary[key]);
               }
               else if (key.ToString().Equals("TotalCollectionStorage")) {
                  totalStorageSum = HandleListStorageData("TotalCollectionStorage", totalStorageSum, tsmNodeDictionary);
               }
               else if (key.ToString().Equals("TotalCollectionWindowsSystemStorage")) {
                  totalStorageSum = HandleListStorageData("TotalCollectionWindowsSystemStorage", totalStorageSum, tsmNodeDictionary);
               }
               else if (key.ToString().Equals("TotalCollectionLinuxRootStorage")) {
                  totalStorageSum = HandleListStorageData("TotalCollectionLinuxRootStorage", totalStorageSum, tsmNodeDictionary);
               }
            }
         } catch (Exception ex) {
            log.WriteLine(DateTime.Now + " - Error: " + ex.ToString());
         }
         return new { ServerCollection = serverCollection, TotalStorage = totalStorageSum };
      }
      private Dictionary<string, long> HandleListStorageData(string key, Dictionary<string, long> pairList, System.Collections.Specialized.OrderedDictionary itemDictionary) {
         var totalStorage = pairList.FirstOrDefault(x => x.Key == key);
         if (!totalStorage.Equals(default(KeyValuePair<string, long>)) && totalStorage.Key.Equals(key)) {
            pairList.Remove(key);
            pairList.Add(key, totalStorage.Value + Int64.Parse(itemDictionary[key].ToString()));
         }
         else pairList.Add(key, Int64.Parse(itemDictionary[key].ToString()));
         return pairList;
      }
   }
}