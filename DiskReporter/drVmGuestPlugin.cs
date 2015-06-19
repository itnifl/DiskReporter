﻿using System;
using System.Net;
using System.IO;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using VMWareChatter.XmlReader;
using VMWareChatter;
using DiskReporter.PluginContracts;

namespace DiskReporter {
    public class VmGuests : IComNodeList<VmGuest> {
        public List<VmGuest> Nodes { get; set; }
        public IEnumerator<VmGuest> GetEnumerator() {
            Nodes.Sort(delegate(VmGuest p1, VmGuest p2) {
                return p1.Name.CompareTo(p2.Name);
             });
            foreach (VmGuest guest in Nodes) { 
                yield return guest; 
            } 
        }

        public void AddNode(VmGuest node) {         
            Nodes.Add(node);         
        }
        /// <summary>
        ///  Gets the total storage space of all disks registered on all nodes.
        /// </summary>
        /// <param name="daySpan">This parameter is implemented to satisfy the interface and is not used here</param>
        public long? GetTotalStorage(int daySpan) {
            long? totalStorage = 0;
            Nodes.ForEach(x => totalStorage += x.GetTotalStorageSpace());
            return totalStorage;
        }
        /// <summary>
        ///  Gets the total windows system storage space of all disks registered on all nodes.
        /// </summary>
        /// <param name="daySpan">This parameter is implemented to satisfy the interface and is not used here</param>
        public long? GetTotalWindowsSystemStorage(int daySpan) {
            long? totalStorage = 0;
            IEnumerable<GeneralDisk> systemDisks = from x in Nodes where x.GetSystemDisk().DiskPath.ToLower().Contains("c:") || (!x.GetSystemDisk().DiskPath.ToLower().Contains("c:") && x.GetSystemDisk().DiskPath.ToLower().Contains("m:")) select x.GetSystemDisk();
            foreach (GeneralDisk disk in systemDisks) {
                totalStorage += disk.Capacity;
            }
            return totalStorage;
        }
        /// <summary>
        ///  Gets the total linux system storage space of all disks registered on all nodes.
        /// </summary>
        /// <param name="daySpan">This parameter is implemented to satisfy the interface and is not used here</param>
        public long? GetTotalLinuxRootStorage(int daySpan) {
            long? totalStorage = 0;
            IEnumerable<GeneralDisk> systemDisks = from x in Nodes where x.GetSystemDisk().DiskPath.ToLower().Contains("/") select x.GetSystemDisk();
            foreach (GeneralDisk disk in systemDisks) {
                totalStorage += disk.Capacity;
            }
           return totalStorage;
        }
    }
    public class VmGuest : IComNode {
        public string Name { get; set; }
        public string PowerStatus { get; set; }
        public string IP { get; set; }
        public List<GeneralDisk> Disks { get; set; }
        public string State { get; set; }
        public string ToolsStatus { get; set; }
        public string ToolsVersionStatus { get; set; }
        public string OSFamily { get; set; }
        public long? TotalStorage { get; set; }
        public long? TotalSystemStorage { get; set; }

        public VmGuest() {
           this.Name = "Noname";
        }
        public VmGuest(string name) {
           this.Name = name;
        }
        /// <summary>
        ///  Adds a disk to the dfinition of a virtual VMware guest
        /// </summary>
        /// <param name="disk">Disk represented as GeneralDisk</param>
        public void AddDisk(GeneralDisk disk) {
           Disks.Add(disk);
        }
        /// <summary>
        ///  Retrieves the total sum of storage of the virtual guest
        /// </summary>
        public long? GetTotalStorageSpace() {
            long? totalSpace = 0;
            foreach (GeneralDisk disk in Disks) {
                totalSpace += disk.Capacity;
            }
            this.TotalStorage = totalSpace;
            return totalSpace;
        }
        /// <summary>
        ///  Retrieves the system disk of the virtual machine
        /// </summary>
        public GeneralDisk GetSystemDisk() {
            GeneralDisk diskC = Disks.Find(x => x.DiskPath.ToLower().Contains("c:"));
            GeneralDisk diskM = Disks.Find(x => x.DiskPath.ToLower().Contains("m:"));
            GeneralDisk diskLinuxRoot = Disks.Find(x => x.DiskPath.Equals("/"));
            GeneralDisk nullDisk = new GeneralDisk() {DiskPath = "None Found", Capacity= 0, FreeSpace = 0};
            GeneralDisk returnDisk = diskC != null && diskC.Capacity.HasValue ? diskC : (diskM != null && diskM.Capacity.HasValue ? diskM : (diskLinuxRoot != null && diskLinuxRoot.Capacity.HasValue ? diskLinuxRoot : nullDisk));
            this.TotalSystemStorage = returnDisk.Capacity;
            return returnDisk;
        }
   }
    class VmMethods : IComPlugin {
        public string PluginName { get; set;}

		public VmMethods(string pluginName) {
			this.PluginName = pluginName;
		}
        /// <summary>
        ///  Retrieves a reference to the object itself
        /// </summary>
		public IComPlugin GetPlugin() {
			return this;
		}
        /// <summary>
        ///  Retrieves all VMware guests and their data. Use T1 as VmGuests and T2 as VmGuest.
        /// </summary>
        /// <param name="sourceConfigFileName">XML configuration file that describes needed information for fetching VMWare guest data</param>
        /// <param name="nameFilter">The name of the virtual guest we want to filter out from the results</param>
        /// <param name="outExceptions">List of exceptions that we can examine after the code completes</param>
		public T1 GetAllNodesData<T1, T2>(string sourceConfigFileName, string nameFilter, out List<Exception> outExceptions) where T1 : IComNodeList<T2>, new() 
			where T2 : IComNode, new()
		{
			/*
			 * Since ourGuests will represent all our nodes, T2 gets to be obsolete in this implementation of the method.
			 * This should to be fixed to have consistent code.
			 * */
			VCenterCommunicator vCom = new VCenterCommunicator ();
			XmlReaderLocal vmConfigReader = new XmlReaderLocal(sourceConfigFileName);
	     	List<Hashtable> hashtableList = vmConfigReader.ReadAllServers();
			List<Exception> loopExceptions = new List<Exception>();
			VmGuests ourGuests = new VmGuests();
			T1 returnGuests = new T1();

	     	foreach (Hashtable htable in hashtableList) {
	        	string host = (string)htable["VCENTER"];
	        	string domain = (string)htable["DOMAIN"];
	       	    string username = (string)htable["USER"];
	        	string password = (string)htable["PASSWORD"];

	        	try {
					ourGuests = vCom.GetVMServerInfo(host, username, password, domain, nameFilter);
	        	} catch (Exception e) {
					loopExceptions.Add(e);
	        	}
				returnGuests.Nodes = new List<T2>();
				foreach(var ourGuest in ourGuests) {
					returnGuests.AddNode(new T2() {
						Name = ourGuest.Name,
						Disks = ourGuest.Disks,
						TotalStorage = ourGuest.TotalStorage,
						TotalSystemStorage = ourGuest.TotalSystemStorage
					});
				}
	     	}
			outExceptions = loopExceptions;
			return returnGuests;
	    }
    }
}
