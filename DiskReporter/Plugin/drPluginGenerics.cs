using System;
using VMWareChatter;
namespace DiskReporter {
    /// <summary>
    ///  This is the class we use to represent a disk for all plugins
    /// </summary>
    public class GeneralDisk {
        public GeneralDisk(string diskpath, long? capacity, double pct_util = 0, DateTime last_backup_end = new DateTime()) {
            this.DiskPath = diskpath;
            this.Capacity = capacity;
            this.PCT_UTIL = pct_util;
            this.LAST_BACKUP_END = last_backup_end;
            this.FreeSpace = (long)(capacity * (pct_util / 100));
        }
         public GeneralDisk(GuestDiskInfoWrapper dw) {
            this.DiskPath = dw.DiskPath;
            this.Capacity = dw.Capacity;
            this.PCT_UTIL = Convert.ToInt64((double)(1 - ((double)dw.FreeSpace / (double)dw.Capacity)) * 100);
            this.FreeSpace = dw.FreeSpace;
         }
        public GeneralDisk() {
            this.LAST_BACKUP_END = DateTime.Today; //This is the default, marks the disks and its data as current. Maybe confusing for VMware servers where there is no backup informatiin.
        }
        public double PCT_UTIL { get; set; }
        public DateTime LAST_BACKUP_END { get; set; }
        public string DiskPath { get; set; }
        public long? Capacity { get; set; } //bytes
        public long? FreeSpace { get; set; } //bytes
    }
}

