using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using DiskReporter.PluginContracts;
using DiskReporter.Utilities;
using VMWareChatter;
using System.Security;

namespace DiskReporter {
   public class VmGuests : IComNodeList<VmGuest> {
      public List<VmGuest> Nodes { get; set; }
      public IEnumerator<VmGuest> GetEnumerator() {
         if(this.Nodes != null && this.Nodes.Count() > 0) {
            Nodes.Sort(delegate (VmGuest p1, VmGuest p2) {
               return p1.Name.CompareTo(p2.Name);
            });
            foreach (VmGuest guest in Nodes) {
               yield return guest;
            }
         }        
      }
      public VmGuests(List<VirtualMachineWrapper> wrapperList) {
         this.Nodes = new List<VmGuest>();
         wrapperList.ForEach((VirtualMachineWrapper vm) => {
            this.Nodes.Add(new VmGuest(vm));
         });         
      }
      public VmGuests() {
         this.Nodes = new List<VmGuest>();
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

      public VmGuest(VMWareChatter.VirtualMachineWrapper vm) {
         this.Name = vm.HostName;
         foreach (GuestDiskInfoWrapper disk in vm.Disk) {
            if (this.Disks == null) {
               this.Disks = new List<GeneralDisk>();
            }
            this.Disks.Add(new GeneralDisk(disk));
         }         
         this.IP = vm.IpAddress;
         this.PowerStatus = vm.PowerState.ToString();
         this.ToolsStatus = vm.ToolsRunningStatus;
      }
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
   class VmPlugin : IComPlugin {
      [Required(ErrorMessage = "The plugin needs to be named", AllowEmptyStrings = false)]
      public string PluginName { get; set;}
      /// <summary>
      ///  Keeps track of what T1 should be like in GetAllNodesData
      /// </summary>
      [Required(ErrorMessage = "Type of object to list nodes required")]
      public Type NodesObjectType { get; set;}
      /// <summary>
      ///  Keeps track of what T2 should be like in GetAllNodesData
      /// </summary>
      [Required(ErrorMessage = "Type of node object is required")]
      public Type NodeObjectType { get; set;}
     /*  Requires VMware.Vim.dll
      *  Requires VMware.VimAutomation.Logging.SoapInterceptor.dll
      *  These should be checked if are available either in local folder or in GAC
      */

      public VmPlugin(string pluginName) {
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
         vCenterCommunicator vCom = null;
         XmlReaderLocal vmConfigReader = new XmlReaderLocal(sourceConfigFileName);
         List<Hashtable> hashtableList = vmConfigReader.ReadAllServers();
         List<Exception> loopExceptions = new List<Exception>();
         VmGuests ourGuests = null;
         T1 returnGuests = new T1();

         foreach (Hashtable htable in hashtableList) {
            string host = (string)htable["VCENTER"];
            string domain = (string)htable["DOMAIN"];
            string username = (string)htable["USER"];

            SecureString securePass = new SecureString();
            foreach (char c in (string)htable["PASSWORD"]) {
               securePass.AppendChar(c);
            }

            vCom = new vCenterCommunicator(host, username, securePass, domain);

            try {
					ourGuests = new VmGuests(vCom.GetVirtualMachines(nameFilter));
            } catch (Exception e) {
					loopExceptions.Add(e);
	        	}
				returnGuests.Nodes = new List<T2>();
				foreach(var ourGuest in ourGuests) {
               foreach (GeneralDisk disk in ourGuest.Disks) {
                  disk.PCT_UTIL = Convert.ToInt64((double)(1 - ((double)disk.FreeSpace / (double)disk.Capacity)) * 100);
               }
               returnGuests.AddNode(new T2() {
                  Name = ourGuest.Name,
                  Disks = ourGuest.Disks,
                  TotalStorage = ourGuest.TotalStorage,
                  TotalSystemStorage = ourGuest.TotalSystemStorage,
                  PowerStatus = ourGuest.PowerStatus,
                  IP = ourGuest.IP,
                  State = ourGuest.State,
                  ToolsStatus = ourGuest.ToolsStatus,
                  ToolsVersionStatus = ourGuest.ToolsVersionStatus,
                  OSFamily = ourGuest.OSFamily
               });
				}
	     	}
         outExceptions = loopExceptions;
         return returnGuests;
      }
      public bool CheckPrerequisites(out List<Exception> outExceptions) {
         bool allOK = true;
         outExceptions = new List<Exception>();
         string currentDirectory = System.IO.Directory.GetCurrentDirectory();

         if (!System.Reflection.Assembly.LoadFrom("VMware.Vim.dll").GlobalAssemblyCache) { // not in gac
            if (!System.IO.File.Exists(currentDirectory + System.IO.Path.DirectorySeparatorChar + "VMware.Vim.dll")) {
               allOK = false;
               outExceptions.Add(new Exception("Could load or find VMware.Vim.dll"));
            }
         }
         if (!System.Reflection.Assembly.LoadFrom("VMware.VimAutomation.Logging.SoapInterceptor.dll").GlobalAssemblyCache) { // not in gac
            if (!System.IO.File.Exists(currentDirectory + System.IO.Path.DirectorySeparatorChar + "VMware.VimAutomation.Logging.SoapInterceptor.dll")) {
               allOK = false;
               outExceptions.Add(new Exception("Could load or find VMware.VimAutomation.Logging.SoapInterceptor.dll"));
            }
         }
         return allOK;
      }
   }
}