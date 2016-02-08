using System;
using System.Linq;
using System.Collections.Generic;
using System.Collections.Specialized;
using VMware.Vim;
using DiskReporter;

/*  Requires namespace VMware.Vim
 *  Requires VMware.Vim.dll
 *  Requires VMware.VimAutomation.Logging.SoapInterceptor.dll
 */
namespace VMWareChatter {
	public class VCenterCommunicator : IDisposable {		
      private Boolean externalDebug = false;
      VimClient vSphereClient = new VimClient();
      /// <summary>
      /// Used to talk to and automate an ESXi environment
      /// </summary>
      /// <param name="hostName">The vCenter DNS hostname or IP-address it can be contacted by</param>
      /// <param name="userName">Username as part of needed credential for access</param>
      /// <param name="password">Password as part of needed credential for access</param>
      /// <param name="domain">Domain as part of needed credential for access</param>
      public VCenterCommunicator(String hostName, String userName, String password, String domain) {
         //Fetch information and collect them in a representative object:
         //ServiceContent vcon =
         vSphereClient.Connect("https://" + hostName + "/sdk");
         if (!String.IsNullOrEmpty(domain)) userName = domain + "\\" + userName;
         //UserSession vus = 
         vSphereClient.Login(userName, password);
      }
      public void Dispose() {
         try {
            vSphereClient.Logout();
            vSphereClient.Disconnect();
         }
         catch {
            //Do nothing
         }
      }

      /// <summary>
      ///  Fetches all vmware guests, optionally filtered.
      /// </summary>
      /// <param name="hostName">The vCenter DNS hostname or IP-address it can be contacted by</param>
      /// <param name="userName">Username as part of needed credential for access</param>
      /// <param name="password">Password as part of needed credential for access</param>
      /// <param name="domain">Domain as part of needed credential for access</param>
      /// <param name="guestNameFilter">Name of guest that you want to fetch information about</param>
      public VmGuests GetVMServerInfo(String guestNameFilter) {
			VmGuests guests = new VmGuests();

			//For debugging we don't want to talk to any vCenter, we will create our own data to work with:
			if (System.Diagnostics.Debugger.IsAttached || externalDebug) { 
				guests.Nodes = new List<VmGuest> {
					new VmGuest("HUGO_S_APP01") { Name = "HUGO_S_APP01", 
						PowerStatus = "PoweredOn", 
						IP = "192.168.1.80",  
						Disks = new List<GeneralDisk>(){
							new GeneralDisk(){ 
								DiskPath = "C:\\", 
								Capacity = 48318382080,
								FreeSpace = 11811160064
							},								
							new GeneralDisk(){
								DiskPath = "E:\\", 
								Capacity = 48318382080,
								FreeSpace = 11811160064
							}
						}, 
						State = "running", 
						ToolsStatus = "toolsOk",
						ToolsVersionStatus = "guestToolsUnmanaged",
						OSFamily = "windowsGuest"
					},
					new VmGuest("HUGO_N_APP02") { Name = "HUGO_N_APP02",
						PowerStatus = "PoweredOff", 
						IP = "192.168.1.81",  
						Disks = new List<GeneralDisk>(){
							new GeneralDisk(){ 
								DiskPath = "/", 
								Capacity = 48318382080,
								FreeSpace = 21811160064
							},								
							new GeneralDisk(){
								DiskPath = "/mnt/disk1", 
								Capacity = 88318382080,
								FreeSpace = 51818161014
							}
						}, 
						State = "running", 
						ToolsStatus = "NotInstalled",
						ToolsVersionStatus = "guestToolsIsNotInstalled",
						OSFamily = "linuxGuest" },
					new VmGuest("HUGO_W_DB01") { Name = "HUGO_W_DB01",
						PowerStatus = "PoweredOn", 
						IP = "192.168.1.82",  
						Disks = new List<GeneralDisk>(){
							new GeneralDisk(){ 
								DiskPath = "/", 
								Capacity = 28318382080,
								FreeSpace = 19119160064
							},								
							new GeneralDisk(){
								DiskPath = "/usr", 
								Capacity = 8318382080,
								FreeSpace = 181816101
							}
						}, 
						State = "running", 
						ToolsStatus = "toolsOk",
						ToolsVersionStatus = "guestToolsCurrent",
						OSFamily = "linuxGuest" },
					new VmGuest("HUGO_E_WEB01") { Name = "HUGO_E_WEB01",
						PowerStatus = "PoweredOn", 
						IP = "192.168.1.83",  
						Disks = new List<GeneralDisk>(){
							new GeneralDisk(){ 
								DiskPath = "C:\\", 
								Capacity = 88318382080,
								FreeSpace = 21212160064
							},								
							new GeneralDisk(){
								DiskPath = "F:\\", 
								Capacity = 41318382080,
								FreeSpace = 11811160064
							}
						}, 
						State = "running", 
						ToolsStatus = "toolsOk",
						ToolsVersionStatus = "guestToolsUnmanaged",
						OSFamily = "windowsGuest"
					},
					new VmGuest("HUGO_E_WEB02") { Name = "HUGO_E_WEB02",
						PowerStatus = "PoweredOn", 
						IP = "192.168.1.84",  
						Disks = new List<GeneralDisk>(){
							new GeneralDisk(){ 
								DiskPath = "C:\\", 
								Capacity = 88318382080,
								FreeSpace = 51212160064
							},								
							new GeneralDisk(){
								DiskPath = "F:\\", 
								Capacity = 41318382080,
								FreeSpace = 1981160064
							}
						}, 
						State = "running", 
						ToolsStatus = "toolsOk",
						ToolsVersionStatus = "guestToolsUnmanaged",
						OSFamily = "windowsGuest"
					}
				};
			} else {
            //Fetch information and collect them in a representative object:
				var filter = new NameValueCollection();
				filter.Add("name", guestNameFilter);
				IList<EntityViewBase> vms = vSphereClient.FindEntityViews(typeof(VirtualMachine), null, filter, null);
				foreach (VMware.Vim.EntityViewBase tmp in vms) {            
					VMware.Vim.VirtualMachine vm = (VirtualMachine)tmp;
					VmGuest currentGuest = new VmGuest((vm.Guest.HostName != null ? (String)vm.Guest.HostName : ""));
					currentGuest.PowerStatus = (String)((vm.Guest.GuestState.Equals("running") ? "PoweredOn" : "PoweredOff"));
					currentGuest.IP = (!String.IsNullOrEmpty(vm.Guest.IpAddress) ? vm.Guest.IpAddress : "0.0.0.0");
					currentGuest.Disks = (vm.Guest.Disk != null ? ConvertGuestDiskInfo(vm.Guest.Disk.ToList()) : new List<GeneralDisk>());
					currentGuest.State =  (!String.IsNullOrEmpty(vm.Guest.GuestState) ? vm.Guest.GuestState : "");
					currentGuest.ToolsStatus = (!String.IsNullOrEmpty(vm.Guest.ToolsRunningStatus) ? vm.Guest.ToolsRunningStatus : "");
					currentGuest.ToolsVersionStatus = (!String.IsNullOrEmpty(vm.Guest.ToolsVersionStatus2) ? vm.Guest.ToolsVersionStatus2 : "");
					currentGuest.OSFamily = (!String.IsNullOrEmpty(vm.Guest.GuestFamily) ? vm.Guest.GuestFamily : "");
					guests.AddNode(currentGuest);
				}
            vSphereClient.Disconnect();
			}
         if (!String.IsNullOrEmpty(guestNameFilter)) {
            var newGuests = guests.Nodes.Where(x => x.Name.Equals(guestNameFilter)).ToList();
            VmGuests filteredGuests = new VmGuests();

            foreach(var newGuest in newGuests) {
               filteredGuests.AddNode(newGuest);
            }
            return filteredGuests; 
         }
			return guests;
		}
      /// <summary>
      ///  Converts GuestDiskInfo to GeneralDisk that is general to any communication plugin that is used in this system.
      /// </summary>
      /// <param name="ourFromList">List of GuestDiskInfo to convert</param>
		private List<GeneralDisk> ConvertGuestDiskInfo(List<GuestDiskInfo> ourFromList) {
			List<GeneralDisk> ourToList = new List<GeneralDisk>();
			foreach(GuestDiskInfo disk in ourFromList) {
				ourToList.Add(new GeneralDisk() { DiskPath = disk.DiskPath, Capacity = disk.Capacity, FreeSpace = disk.FreeSpace });
			}
			return ourToList;
		}
      public int GetVMNumMksConnections(String guestNameFilter) {
         var filter = new NameValueCollection();
         filter.Add("name", guestNameFilter);
         VirtualMachine vm = null;
         try {
            vm = (VirtualMachine)vSphereClient.FindEntityView(typeof(VirtualMachine), null, filter, null);
         }
         catch {
            //Do nothing
         }
         return vm != null ? vm.Summary.Runtime.NumMksConnections : 0;
      }
      /// <summary>
      /// Sets a new powerstate
      /// </summary>
      /// <param name="guestNameFilter">The guest to power on or off</param>
      /// <param name="powerState">True to power on, false to power off</param>
      public void SetVMPowerState(String guestNameFilter, bool powerState) {
         var filter = new NameValueCollection();
         filter.Add("name", guestNameFilter);
         VirtualMachine vm = null;
         try {
            vm = (VirtualMachine)vSphereClient.FindEntityView(typeof(VirtualMachine), null, filter, null);
         }
         catch {
            //Do nothing
         }
         try {
            if (powerState) {
               HostSystem host = GetHostSystems().FirstOrDefault();
               if (host != null) {
                  vm.PowerOnVM_Task(host.MoRef);
               }
            }
            else {
               vm.PowerOffVM();
            }
         }
         catch (Exception e) {
            //Do nothing
            //This code will no work unless we have write access to the vSphere API
         }
      }
      /// <summary>
      ///  Gets information to start a VMRC session to virtual machines
      /// </summary>
      /// <param name="guestNameFilter">Name of guest that you want to fetch information about</param>
      public OrderedDictionary GetVMRCLogonInfo(String guestNameFilter, string userName) {
         OrderedDictionary LogonInfoDictionary = new OrderedDictionary();
         var filter = new NameValueCollection();
         filter.Add("name", guestNameFilter);
         IList<EntityViewBase> vms = vSphereClient.FindEntityViews(typeof(VirtualMachine), null, filter, null);

         foreach (VMware.Vim.EntityViewBase tmp in vms) {
            VirtualMachine vm = (VirtualMachine)tmp;
            string vmkid = vm.Summary.Vm.Value;
            string vmName = vm.Name;
            int numConnections = vm.Summary.Runtime.NumMksConnections;
            string cloneTicket = GetCloneTicket();
            SessionManagerLocalTicket localTicket = null;
            try {
               localTicket = GetLocalTicket(userName);
            }
            catch (Exception e) {
               throw (e);
            }
            LogonInfoDictionary.Add(vmName, new VmrcLogonInfo() { VmkID = vmkid, LocalTicket = localTicket, CloneTicket = cloneTicket, ConsoleConnections = numConnections });
         }
         return LogonInfoDictionary;
      }
      private ServiceInstance GetServiceInctance() {
         ManagedObjectReference _svcRef = new ManagedObjectReference() { Type = "ServiceInstance", Value = "ServiceInstance" };
         ServiceInstance _service = new ServiceInstance(vSphereClient, _svcRef);
         return _service;
      }
      private LicenseManager GetLicenseManager() {
         LicenseManager licenseManager = null;
         try {
            ServiceContent _sic = GetServiceInctance().RetrieveServiceContent();
            licenseManager = (LicenseManager)vSphereClient.GetView(_sic.LicenseManager, null);
         }
         catch (Exception e) {
            throw (e);
         }
         return licenseManager;
      }
      private SessionManager GetSessionManager() {
         SessionManager sessionManager = null;
         try {
            ServiceContent _sic = GetServiceInctance().RetrieveServiceContent();
            sessionManager = (SessionManager)vSphereClient.GetView(_sic.SessionManager, null);
         }
         catch (Exception e) {
            throw (e);
         }
         return sessionManager;
      }
      /// <summary>
      /// Get a list of all hosts
      /// </summary>
      /// <returns>List of all ESXi hosts</returns>
      public List<HostSystem> GetHostSystems() {
         List<HostSystem> hostSystems = new List<HostSystem>();
         try {
            Datacenter dataCenter = (Datacenter)vSphereClient.FindEntityView(typeof(Datacenter), null, null, null);
            Folder folder = (Folder)vSphereClient.GetView(dataCenter.HostFolder, null);
            foreach (ManagedObjectReference mObjR in folder.ChildEntity.Where(x => x.Type == "ComputeResource")) {
               ComputeResource computeResource = (ComputeResource)vSphereClient.GetView(mObjR, null);
               foreach (ManagedObjectReference hostRef in computeResource.Host) {
                  hostSystems.Add((HostSystem)vSphereClient.GetView(hostRef, null));
               }
            }
         }
         catch (Exception e) {
            throw (e);
         }
         return hostSystems;
      }
      /// <summary>
      /// Check if a license feature is present. 
      /// </summary>
      /// <param name="featureName">Name of the feature</param>
      /// <returns>True or false</returns>
      public bool CheckLicenseFeature(string featureName) {
         List<KeyValue> keyValueList = GetLicenseManager().Licenses[0].Properties.Where(x => x.Key == "feature").Select(x => x.Value).ToList().Cast<KeyValue>().ToList().Where(keyValue => keyValue.Value.ToString().ToLower() == featureName.ToLower()).ToList();
         if (keyValueList != null && keyValueList.Count > 0) {
            return true;
         }
         return false; ;
      }
      /// <summary>
      /// Update the license where the client is connected
      /// </summary>
      /// <param name="license">License as string</param>
      /// <returns></returns>
      public bool UpdateLicense(string license) {
         KeyValue DummyKey = new KeyValue() { Key = "DummyKey", Value = "DummyValue" };
         KeyValue[] DummyArray = new KeyValue[1] { DummyKey };
         LicenseManagerLicenseInfo lInfo = GetLicenseManager().UpdateLicense(license, DummyArray);
         if (lInfo == null) return false;
         return lInfo.LicenseKey.ToUpper() == license.ToUpper();
      }
      /// <summary>
      /// Gets a clone ticket to use for one time authentication
      /// </summary>
      /// <returns></returns>
      public string GetCloneTicket() {
         return GetSessionManager().AcquireCloneTicket();
      }
      /// <summary>
      /// Get a local ticket. It will be created on the host and won't be available to us.
      /// You can find it under /var/run/vmware-hostd-ticket/ for a few seconds, it contains the password.
      /// </summary>
      /// <param name="userName">Username to create a one time ticket for</param>
      /// <returns>One time ticket object</returns>
      public SessionManagerLocalTicket GetLocalTicket(string userName) {
         return GetSessionManager().AcquireLocalTicket(userName);
      }
      /// <summary>
      /// Disconnect all sessions that are running where the clint is connected.
      /// </summary>
      public void DisconnectAllSessions() {
         SessionManager sessionManager = GetSessionManager();
         string[] sessionId = sessionManager.SessionList.Where(x => x.Key != sessionManager.CurrentSession.Key).Select(key => key.Key).ToArray();
         try {
            sessionManager.TerminateSession(sessionId);
         }
         catch (Exception e) {
            if (e.Message.ToLower() != "a specified parameter was not correct") {
               throw (e);
            }
         }
      }
   }
   public class VmrcLogonInfo {
      public string VmkID { get; set; }
      public SessionManagerLocalTicket LocalTicket { get; set; }
      public string CloneTicket { get; set; }
      public int ConsoleConnections { get; set; }
   }
}