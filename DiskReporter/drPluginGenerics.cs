using System;

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
        }
        public GeneralDisk() {
            this.LAST_BACKUP_END = DateTime.Today;
        }
        public double PCT_UTIL { get; set; }
        public DateTime LAST_BACKUP_END { get; set; }
        public string DiskPath { get; set; }
        public long? Capacity { get; set; } //bytes
        public long? FreeSpace { get; set; } //bytes
    }
}

