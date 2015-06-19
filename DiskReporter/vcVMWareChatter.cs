using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using VMware.Vim;
using DiskReporter;

namespace VMWareChatter {
	public class VCenterCommunicator {
		VimClient vcli = new VimClient();
		ServiceContent vcon;
		UserSession vus;
	
        /// <summary>
        ///  Fetches all vmware guests, optionally filtered.
        /// </summary>
        /// <param name="hostName">The vCenter DNS hostname or IP-address it can be contacted by</param>
        /// <param name="userName">Username as part of needed credential for access</param>
        /// <param name="password">Password as part of needed credential for access</param>
        /// <param name="domain">Domain as part of needed credential for access</param>
        /// <param name="guestNameFilter">Name of guest that you want to fetch information about</param>
		public VmGuests GetVMServerInfo(String hostName, String userName, String password, String domain, String guestNameFilter) {
			VmGuests guests = new VmGuests();

			//For debugging we don't want to talk to any vCenter, we will create our own data to work with:
			if (System.Diagnostics.Debugger.IsAttached) { 
				guests.Nodes = new List<VmGuest> {
					new VmGuest("HUGO_N_APP01") { Name = "HUGO_S_APP01", 
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
				vcon = vcli.Connect("https://" + hostName + "/sdk");
				if (!String.IsNullOrEmpty(domain)) userName = domain + "\\" + userName;
				UserSession vus = vcli.Login(userName, password);
				var filter = new NameValueCollection();
				filter.Add("name", guestNameFilter);
				IList<EntityViewBase> vms = vcli.FindEntityViews(typeof(VirtualMachine), null, filter, null);
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
				vcli.Disconnect();
			}
			if(!String.IsNullOrEmpty(guestNameFilter)) return (VmGuests)guests.Nodes.Where(x => x.Name.Equals(guestNameFilter));
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
	}
}