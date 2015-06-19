using System;
using System.IO;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using Kraggs.TSM;
using Kraggs.TSM.Utils;
using VMWareChatter.XmlReader;
using DiskReporter.PluginContracts;

namespace DiskReporter {
    class TsmNodes : IComNodeList<TsmNode> {
        public List<TsmNode> Nodes { get; set; }
        public TsmNodes() {
            Nodes = new List<TsmNode>();
        }
        public void AddNode(TsmNode node) {         
            Nodes.Add(node);     
        }
        public IEnumerator<TsmNode> GetEnumerator() {
            Nodes.Sort(delegate(TsmNode p1, TsmNode p2) {
                return p1.Name.CompareTo(p2.Name);
            });
            foreach (TsmNode tsmNode in Nodes) {
                yield return tsmNode;
            }
         }
        /// <summary>
        ///  Gets the total storage space of all disks registered on all nodes.
        /// </summary>
        /// <param name="daySpan">The time span in days that the disk must exist within/taken backup of</param>
        public long? GetTotalStorage(int daySpan) {
            long? totalStorage = 0;
            Nodes.ForEach(x => totalStorage += x.GetTotalStorageSpace(new TimeSpan(daySpan, 0, 0, 0, 0)));
            return totalStorage;
        }
        /// <summary>
        ///  Gets the total windows system storage space of all disks registered on all nodes.
        /// </summary>
        /// <param name="daySpan">The time span in days that the disk must exist within/taken backup of</param>
        public long? GetTotalWindowsSystemStorage(int daySpan) {
            long? totalStorage = 0;
            IEnumerable<GeneralDisk> systemDisks = from x in Nodes where x.GetSystemDisk(new TimeSpan(daySpan, 0, 0, 0, 0)).DiskPath.ToLower().Contains("c$") || (!x.GetSystemDisk(new TimeSpan(daySpan, 0, 0, 0, 0)).DiskPath.ToLower().Contains("c$") && x.GetSystemDisk(new TimeSpan(daySpan, 0, 0, 0, 0)).DiskPath.ToLower().Contains("m$")) select x.GetSystemDisk(new TimeSpan(daySpan, 0, 0, 0, 0));
            foreach (GeneralDisk disk in systemDisks) {
                totalStorage += disk.Capacity;
            }
            return totalStorage;
        }
        /// <summary>
        ///  Gets the total Linux system storage space of all disks registered on all nodes.
        /// </summary>
        /// <param name="daySpan">The time span in days that the disk must exist within/taken backup of</param>
        public long? GetTotalLinuxRootStorage(int daySpan) {
            long? totalStorage = 0;
            IEnumerable<GeneralDisk> systemDisks = from x in Nodes where x.GetSystemDisk(new TimeSpan(daySpan, 0, 0, 0, 0)).DiskPath.ToLower().Contains("/") select x.GetSystemDisk(new TimeSpan(14, 0, 0, 0, 0));
            foreach (GeneralDisk disk in systemDisks) {
                totalStorage += disk.Capacity;
            }
            return totalStorage;
        }
    }
    class TsmNode : IComNode {
        public string Name { get; set; }
        public List<GeneralDisk> Disks { get; set; }
        public long? TotalStorage { get; set; }
        public long? TotalSystemStorage { get; set; }
	    public TimeSpan? LastUsedTimeSpan { get;  set; } //This is the last TimeSpan used when fetching data information. The usage of this is possibly ambiguous.

	    public TsmNode() {
            this.Name = "Noname";
            this.Disks = new List<GeneralDisk>();
	    }
        public TsmNode(string name) {
            this.Name = name;
            this.Disks = new List<GeneralDisk>();
        }      
        /// <summary>
        ///  Adds a disk to a definition of a TSM node
        /// </summary>
        /// <param name="disk">Disk represented as GeneralDisk</param>
        public void AddDisk(GeneralDisk disk) {
            Disks.Add(disk);
        }
        /// <summary>
        ///  Gets the total storage space of all disks registered on the node that have been taken backup of within a TimeSpan of x days .
        /// </summary>
        /// <param name="timeSpanLimit">The time span that defines the disk as a part of the total</param>
        public long? GetTotalStorageSpace(TimeSpan timeSpanLimit) {
            long? totalSpace = 0;
            foreach(GeneralDisk disk in Disks) {
                if ((DateTime.Now - disk.LAST_BACKUP_END).Days <= timeSpanLimit.Days) totalSpace += disk.Capacity;
            }
            this.LastUsedTimeSpan = timeSpanLimit;
            this.TotalStorage = totalSpace;
            return totalSpace;
        }
        /// <summary>
        ///  Gets the total storage space of all disks registered on the node regardsless of when they were taken backup of.
        /// </summary>
        public long? GetTotalStorageSpace() {
            long? totalSpace = 0;
            foreach (GeneralDisk disk in Disks) {
               totalSpace += disk.Capacity;
            }
            return totalSpace;
        }
        /// <summary>
        ///  Gets the system disk within a timespan limit of 14 days for when it has been taken backup of.
        /// </summary>
        /// <param name="timeSpanLimit">The time span that the disk must exist within</param>
        public GeneralDisk GetSystemDisk() {
            TimeSpan timeSpanLimit = new TimeSpan(14, 0, 0, 0);
            GeneralDisk diskC = Disks.Find(x => x.DiskPath.ToLower().Contains("c$") && (DateTime.Now - x.LAST_BACKUP_END).Days <= timeSpanLimit.Days);
            GeneralDisk diskM = Disks.Find(x => x.DiskPath.ToLower().Contains("m$") && (DateTime.Now - x.LAST_BACKUP_END).Days <= timeSpanLimit.Days);
            GeneralDisk diskLinuxRoot = Disks.Find(x => x.DiskPath.Equals("/"));
            GeneralDisk nullDisk = new GeneralDisk("None Found", 0, 0, DateTime.MinValue);
            GeneralDisk returnDisk = diskC != null && diskC.Capacity.HasValue ? diskC : (diskM != null && diskM.Capacity.HasValue ? diskM : (diskLinuxRoot != null && diskLinuxRoot.Capacity.HasValue ? diskLinuxRoot : nullDisk));
            this.LastUsedTimeSpan = timeSpanLimit;
            this.TotalSystemStorage = returnDisk.Capacity;
            return returnDisk;
        }
        /// <summary>
        ///  Gets the system disk within a timespan limit of x days for when it has been taken backup of.
        /// </summary>
        /// <param name="timeSpanLimit">The time span that the disk must exist within</param>
        public GeneralDisk GetSystemDisk(TimeSpan timeSpanLimit) {
            GeneralDisk diskC = Disks.Find(x => x.DiskPath.ToLower().Contains("c$") && (DateTime.Now - x.LAST_BACKUP_END).Days <= timeSpanLimit.Days);
            GeneralDisk diskM = Disks.Find(x => x.DiskPath.ToLower().Contains("m$") && (DateTime.Now - x.LAST_BACKUP_END).Days <= timeSpanLimit.Days);
            GeneralDisk diskLinuxRoot = Disks.Find(x => x.DiskPath.Equals("/"));
            GeneralDisk nullDisk = new GeneralDisk("None Found", 0, 0, DateTime.MinValue);
            GeneralDisk returnDisk = diskC != null && diskC.Capacity.HasValue ? diskC : (diskM != null && diskM.Capacity.HasValue ? diskM : (diskLinuxRoot != null && diskLinuxRoot.Capacity.HasValue ? diskLinuxRoot : nullDisk));
            this.LastUsedTimeSpan = timeSpanLimit;
            this.TotalSystemStorage = returnDisk.Capacity;
            return returnDisk;
         }
    }
    class TsmProp {
        public TsmProp(string node_name, string filespace_name, long? capacity, double pct_util, DateTime last_backup_end) {
            this.NODE_NAME = node_name;
            this.FILESPACE_NAME = filespace_name;
            this.CAPACITY = capacity;
            this.PCT_UTIL = pct_util;
            this.LAST_BACKUP_END = last_backup_end;
        }
        public string NODE_NAME { get; set; }
        public string FILESPACE_NAME { get; set; }
        public long? CAPACITY { get; set; } //capacity in MB
        public double PCT_UTIL { get; set; }
        public DateTime LAST_BACKUP_END { get; set; }
    }
    public class TsmMethods : IComPlugin {
        public string PluginName { get; set; }
        /*  Requires TSM Admin Client
         *    for full functionality,
         *      works for testing in debug mode.
         */

        public TsmMethods(string pluginName) {
            this.PluginName = pluginName;
        }
        public IComPlugin GetPlugin() {
            return this;
        }
        /// <summary>
        ///  Retrieves all TSM nodes and their data. Use T1 as TsmNodes and T2 as TsmNode
        /// </summary>
        /// <param name="sourceConfigFileName">XML configuration file that describes needed information for fetching TSM node data</param>
        /// <param name="nameFilter">The name of the node we want to filter out from the results</param>
        /// <param name="outExceptions">List of exceptions that we can examine after the code completes</param>
        public T1 GetAllNodesData<T1, T2>(String sourceConfigFileName, string nameFilter, out List<Exception> outExceptions) 
            where T1 : IComNodeList<T2>, new() 
                where T2 : IComNode, new() 
        {
            XmlReaderLocal tsmServersConfigReader = new XmlReaderLocal(sourceConfigFileName);
            List<Hashtable> hashtableList = tsmServersConfigReader.ReadAllServers();
            Exception cachedTSMException = new Exception("unassigned");
            List<Exception> loopExceptions = new List<Exception>();
            T1 allNodes = new T1();

            String tmpfile = "";
            int? result = 0;
            String tsmServerAddress;
            //old query that also gets vmware backups, which we don't want: 
            //String dbQUERY = "SELECT node_name,filespace_name,capacity,pct_util,backup_end from filespaces where filespace_name like '\\%' and node_name in (select node_name from nodes where lastacc_time>(current_timestamp-30 days))";
            String dbQUERY = "SELECT node_name,filespace_name,capacity,pct_util,backup_end from filespaces where (filespace_type='NTFS' or filespace_type like 'EXT%') and node_name in (select node_name from nodes where lastacc_time>(current_timestamp-30 days))" + (!String.IsNullOrEmpty(nameFilter) ? " and node_name='" + nameFilter + "'": "");

            foreach (Hashtable htable in hashtableList) {
            	tsmServerAddress = (String)htable["ADDRESS"];
            	String userName = (String)htable["USER"];
            	String password = (String)htable["PASSWORD"];
            	if (String.IsNullOrEmpty(tsmServerAddress) || String.IsNullOrEmpty(userName) || String.IsNullOrEmpty(password)) continue;                    

            	Boolean currentException = false;
            	tmpfile = Path.GetTempFileName();
            	if(!System.Diagnostics.Debugger.IsAttached) {
            		try {
            			AdminClient dsmadmc;
            			try {
            				dsmadmc = new AdminClient(tsmServerAddress, userName, password);
            			} catch (Exception e) {
            				cachedTSMException = e;
            				loopExceptions.Add(cachedTSMException);
            				currentException = true;
            				continue;
            			}
            			result = dsmadmc.ExecuteCommandToFile(dbQUERY, tmpfile);
            		} catch (TSMException exc) {
            			String exceptionText = exc.ToString();
            			String tsmMessageText = "";
            			foreach (var m in exc.TSMMessages) tsmMessageText += " " + m.ToString() + "\n";
            			cachedTSMException = new Exception("Error from TSM AdminClient when working with TSMServer " + tsmServerAddress + ": " + exceptionText + " - " + tsmMessageText + ". DBQuery was: " + dbQUERY);
            			loopExceptions.Add(cachedTSMException);
            			currentException = true;
            			continue;
            		}

            	}
            	if (!currentException) {
            			using (var reader = System.Diagnostics.Debugger.IsAttached ? new StreamReader(GenerateStreamFromString("HUGO_S_APP03," +
                                                                                                                                "\\\\HUGO_S_APP03\\c$," +
                                                                                                                                "2502," +
                                                                                                                                "72," +
                                                                                                                                "2015-06-18 06:50:02.000301" +
                                                                                                                                System.Environment.NewLine +
                                                                                                                                "HUGO_W_DB02," +
                                                                                                                                "/," +
                                                                                                                                "16502," +
                                                                                                                                "80," +
                                                                                                                                "2015-06-18 02:50:02.000001" +
                                                                                                                                System.Environment.NewLine

            				)) : new StreamReader(File.OpenRead(tmpfile))) {
            	      if (!reader.EndOfStream) {
            	         do {
                            String line = reader.ReadLine();
                            if (line.Split(',').Length != 5) continue;
                            string nodeName = Convert.ToString(line.Split(',')[0].ToUpper());
                            string fileSpace = Convert.ToString(line.Split(',')[1]);     
                            string cap1 = line.Split(',')[2];
                            double cap2 = double.Parse(cap1) * 1048576; //We want the capacity in bytes - same as what we get in VMware
                            long? capacity = long.Parse(Math.Round(cap2).ToString());
            	            double util = double.Parse(line.Split(',')[3]);
            	            DateTime backupEnd = DateTime.MinValue;
            	            if (!DateTime.TryParseExact(line.Split(',')[4], "yyyy-MM-dd HH:mm:ss.ffffff", System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.None, out backupEnd)) {
            	                backupEnd = DateTime.MinValue;
            	            }
            	            
            	            TsmProp tsmProperties = new TsmProp(nodeName, fileSpace, capacity, util, backupEnd);
                            T2 ourNode = allNodes != null ? allNodes.Nodes.Find(x => x.Name.Equals(tsmProperties.NODE_NAME)) : new T2();
                            if (ourNode != null && !ourNode.Name.Equals("Noname")) {                            
                                int index = allNodes.Nodes.IndexOf(ourNode);
                                ourNode.AddDisk(new GeneralDisk(tsmProperties.FILESPACE_NAME, tsmProperties.CAPACITY, tsmProperties.PCT_UTIL, tsmProperties.LAST_BACKUP_END));
                                allNodes.Nodes[index] = ourNode;
            	            } else {
                                ourNode = new T2();
                                ourNode.Name = tsmProperties.NODE_NAME;
                                ourNode.AddDisk(new GeneralDisk(tsmProperties.FILESPACE_NAME, tsmProperties.CAPACITY, tsmProperties.PCT_UTIL, tsmProperties.LAST_BACKUP_END));
                                allNodes.AddNode(ourNode);
            	            }
            	         } while (!reader.EndOfStream);
            	      }
            	   }
            	}
            	if (!String.IsNullOrEmpty(tmpfile) && File.Exists(tmpfile)) File.Delete(tmpfile);
            }
            outExceptions = loopExceptions;
            if (allNodes != null && allNodes.Nodes != null && allNodes.Nodes.Count > 0) return allNodes;
            if (loopExceptions.Count > 0 || !cachedTSMException.Message.ToString().Equals("unassigned", StringComparison.InvariantCultureIgnoreCase)) {
            	throw cachedTSMException;
            }
            return allNodes;
        }
        private static Stream GenerateStreamFromString(string s) {
            Stream stream = new MemoryStream();
            StreamWriter writer = new StreamWriter(stream);
            writer.Write(s);
            writer.Flush();
            stream.Position = 0;
            return stream;
        }
    }
}
