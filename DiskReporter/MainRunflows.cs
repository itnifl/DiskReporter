using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Reflection;
using DiskReporter.Utilities.ExcelMagic;
using DiskReporter.Utilities;
using VMWareChatter.XmlReader;

namespace DiskReporter {
   /*public class WriteToEventLog { 
      public static void write(string Source, Exception ex) {
         EventLog m_EventLog = new EventLog("");
         m_EventLog.Source = Source;
         m_EventLog.WriteEntry("Reading text file failed " + ex.Message, EventLogEntryType.Information);
      }      
   }*/
   public class DiskReporterMainRunFlows {
      StreamWriter log;
      String ConfigDirectory { get; set; }
      private bool? _console_present;
      String logFilename;
      private ComPluginList ourCommunicationPlugins;

      public DiskReporterMainRunFlows(StreamWriter log) {
         this.log = log;
         logFilename = ((FileStream)(log.BaseStream)).Name;
         this.ourCommunicationPlugins = new ComPluginList();
      }
      public DiskReporterMainRunFlows(StreamWriter log, String configDirectory) {
         this.log = log;
         this.ConfigDirectory = configDirectory;
         if (!configDirectory[configDirectory.Length - 1].Equals("\\")) configDirectory += "\\";
         logFilename = ((FileStream)(log.BaseStream)).Name;
         this.ourCommunicationPlugins = new ComPluginList();
         //Now we check if there are any prerequisites missing to our plugins and warn to error log and console if there is:
         var result = this.ourCommunicationPlugins.VerifyAllRegisteredPlugins();
         bool pluginStatus = result.Item1;
         if(!pluginStatus) {
               foreach (string error in (string[])result.Item2) {
                  this.DisplayException(new Exception(error));
               }
         }
      }
      ~DiskReporterMainRunFlows() {         
         try {
               FileInfo f = new FileInfo(logFilename);
               if (f.Length > 204800) {
                  if (!File.Exists(logFilename + ".1")) System.IO.File.Move(logFilename, logFilename + ".1");
                  else if (!File.Exists(logFilename + ".2")) System.IO.File.Move(logFilename, logFilename + ".2");
                  else if (!File.Exists(logFilename + ".3")) System.IO.File.Move(logFilename, logFilename + ".3");
                  else if (!File.Exists(logFilename + ".4")) System.IO.File.Move(logFilename, logFilename + ".4");
                  else if (!File.Exists(logFilename + ".5")) System.IO.File.Move(logFilename, logFilename + ".5");
                  else if (!File.Exists(logFilename + ".6")) System.IO.File.Move(logFilename, logFilename + ".6");
                  else if (!File.Exists(logFilename + ".7")) System.IO.File.Move(logFilename, logFilename + ".7");
                  else if (!File.Exists(logFilename + ".8")) System.IO.File.Move(logFilename, logFilename + ".8");
                  else if (!File.Exists(logFilename + ".9")) System.IO.File.Move(logFilename, logFilename + ".9");
                  else {
                     File.Delete(logFilename + ".9");
                     System.IO.File.Move(logFilename + ".8", logFilename + ".9");
                     System.IO.File.Move(logFilename + ".7", logFilename + ".8");
                     System.IO.File.Move(logFilename + ".6", logFilename + ".7");
                     System.IO.File.Move(logFilename + ".5", logFilename + ".6");
                     System.IO.File.Move(logFilename + ".4", logFilename + ".5");
                     System.IO.File.Move(logFilename + ".3", logFilename + ".4");
                     System.IO.File.Move(logFilename + ".2", logFilename + ".3");
                     System.IO.File.Move(logFilename + ".1", logFilename + ".2");
                  }
               }
         } catch (Exception) {

         }
         log.Close();
      }
      /// <summary>
      ///  Checks if the console is present
      /// </summary>
      public bool console_present {
         get {
               if (_console_present == null) {
                  _console_present = true;
                  try { int window_height = Console.WindowHeight; }
                  catch { _console_present = false; }
               }
               return _console_present.Value;
         }
      }
      /// <summary>
      ///  Displays error messages approriately
      /// </summary>
      private void DisplayException(Exception ex) {
         log.WriteLine(DateTime.Now + " - Error: " + ex.ToString());
         if(console_present) Console.WriteLine("Error - see the logs for more information: " + ex.ToString().Substring(0, (ex.ToString().Length > 160 ? 160 : 40)));
      }
      /// <summary>
      /// Returns all nodes fetched from all plugins registered in our pluginlist
      /// </summary>
      /// <param name="ourConfigSettings">Hashtable where key is plugin name and value is configurationfile</param>
      public OrderedDictionary FetchNodeData(Hashtable ourConfigSettings) {
         throw new NotImplementedException();
         /*List<Exception> exceptionList = new List<Exception>();
         foreach (DictionaryEntry entry in ourConfigSettings) {
               Type NodeObjectType = (Type)ourCommunicationPlugins.ComPlugins.Find(x => x.PluginName.ToUpper().Equals(entry.Key.ToString())).NodeObjectType;
               Type NodesObjectType = (Type)ourCommunicationPlugins.ComPlugins.Find(x => x.PluginName.ToUpper().Equals(entry.Key.ToString())).NodesObjectType;
               //Needs to use reflection, not a good idea for performance:
               //http://stackoverflow.com/questions/4101784/calling-a-generic-method-with-a-dynamic-type
               //var test = ourPlugin.GetAllNodesData <NodesObjectType, NodeObjectType>("", "", out exceptionList);
               //MethodInfo method = typeof(NodeObjectType).GetMethod("GenericMethod");
               //MethodInfo generic = method.MakeGenericMethod(myType);
               //generic.Invoke(this, null);
         }*/
      }
      /// <summary>
      /// Returns all nodes fetched from the VMware and TSM plugins as seperate dictionaries
      /// </summary>
      /// <param name="tsmServersConfig">String representing the name an relative location of xml configuration file</param>
      /// <param name="vCenterConfig">String representing the name an relative location of xml configuration file</param>
      /// <param name="serverNameFilter">Name of server to filter out from the result</param>
      public Tuple<OrderedDictionary, OrderedDictionary> FetchTsmVMwareNodeData(string tsmServersConfig, string vCenterConfig, string serverNameFilter) {
         OrderedDictionary vmwareNodeDictionary = new OrderedDictionary();
         OrderedDictionary tsmNodeDictionary = new OrderedDictionary();
         List<Exception> vmExceptions = new List<Exception>();

         //We are not looking for anything in the vCenter if a configuration for the vCenter server is not specified:
         if (!String.IsNullOrEmpty (vCenterConfig)) {
            try {
               VmPlugin vMwarePlugin = (VmPlugin)ourCommunicationPlugins.ComPlugins.Find(x => x.PluginName.ToUpper().Equals("VMWARE"));
               VmGuests vmGuests = vMwarePlugin.GetAllNodesData<VmGuests, VmGuest>(vCenterConfig, serverNameFilter, out vmExceptions);
               if (String.IsNullOrEmpty (serverNameFilter))
                  vmwareNodeDictionary.Add("TotalCollectionStorage", vmGuests.GetTotalStorage(0));
               if (String.IsNullOrEmpty (serverNameFilter))
                  vmwareNodeDictionary.Add("TotalCollectionWindowsSystemStorage", vmGuests.GetTotalWindowsSystemStorage(0));
               if (String.IsNullOrEmpty (serverNameFilter))
                  vmwareNodeDictionary.Add("TotalCollectionLinuxRootStorage", vmGuests.GetTotalLinuxRootStorage(0));
               foreach (VmGuest vmGuest in vmGuests) {
                  String nodename = vmGuest.Name.Contains(".") ? vmGuest.Name.Split('.') [0].ToUpper() : vmGuest.Name.ToUpper();
                  if (!String.IsNullOrEmpty(nodename) && !vmwareNodeDictionary.Contains(nodename)) {
                     try {
                        if(!vmGuest.TotalStorage.HasValue) vmGuest.TotalStorage = vmGuest.GetTotalStorageSpace();
                        if(!vmGuest.TotalSystemStorage.HasValue) vmGuest.TotalSystemStorage = vmGuest.GetSystemDisk().Capacity;
            	         vmwareNodeDictionary.Add(nodename, vmGuest);
                     } catch (Exception e) {
            	         DisplayException (e);
            	         continue;
                     }
                  }
               }
            } catch (Exception e) {
               DisplayException(e);
            }
         }
         foreach (Exception e in vmExceptions) {
            DisplayException(e);
         }
         //We are not looking for anything on the TSM side if a configuration for the TSM servers are not specified:
         if (!String.IsNullOrEmpty (tsmServersConfig)) {
            //If we requested only one server and that server is already found in vmware, then look no further:
            if (String.IsNullOrEmpty(serverNameFilter) || (!String.IsNullOrEmpty(serverNameFilter) && vmwareNodeDictionary.Count == 0)) { 
               List<Exception> tsmExceptions = new List<Exception>();
               try {
                  int diskTimeSpanDays = 28;
                  TsmPlugin tSMPlugin = (TsmPlugin)ourCommunicationPlugins.ComPlugins.Find(x => x.PluginName.ToUpper().Equals("TSM"));
                  TsmNodes tsmNodes = tSMPlugin.GetAllNodesData<TsmNodes, TsmNode>(tsmServersConfig, serverNameFilter, out tsmExceptions);
                  if (String.IsNullOrEmpty(serverNameFilter)) tsmNodeDictionary.Add("TotalCollectionStorage", tsmNodes.GetTotalStorage(diskTimeSpanDays));
                  if (String.IsNullOrEmpty(serverNameFilter)) tsmNodeDictionary.Add("TotalCollectionWindowsSystemStorage", tsmNodes.GetTotalWindowsSystemStorage(diskTimeSpanDays));
                  if (String.IsNullOrEmpty(serverNameFilter)) tsmNodeDictionary.Add("TotalCollectionLinuxRootStorage", tsmNodes.GetTotalLinuxRootStorage(diskTimeSpanDays));
                  foreach (TsmNode tsmNode in tsmNodes) {
                        //Add nodes from tsm to tsmNodeDictionary only if the server name does not exist in vmwareNodeDictionary already:
                        if (!vmwareNodeDictionary.Contains(tsmNode.Name)) {
                           try {
                              if(!tsmNode.TotalStorage.HasValue) tsmNode.TotalStorage = tsmNode.GetTotalStorageSpace(new TimeSpan(diskTimeSpanDays, 0, 0, 0, 0));
                              if(!tsmNode.TotalSystemStorage.HasValue) tsmNode.TotalSystemStorage = tsmNode.GetSystemDisk(new TimeSpan(diskTimeSpanDays, 0, 0, 0, 0)).Capacity;
                              tsmNodeDictionary.Add(tsmNode.Name, tsmNode);
                           } catch (Exception e) {
                              DisplayException(e);
                              continue;
                           }
                        }
                  }
               } catch (Exception e) {
                  if (console_present) Console.WriteLine("Fetching TSM Nodes Failed: ");
                  DisplayException(e);
               }
               foreach (Exception e in tsmExceptions) {
                  DisplayException(e);
               }
            }
         }
         return Tuple.Create(vmwareNodeDictionary, tsmNodeDictionary);
      }
      /// <summary>
      /// Returns all nodes fetched from the VMware and TSM plugins as one dictionary
      /// </summary>
      /// <param name="tsmServersConfig">String representing the name an relative location of xml configuration file</param>
      /// <param name="vCenterConfig">String representing the name an relative location of xml configuration file</param>
      public OrderedDictionary FetchTsmVMwareNodeData(String tsmServersConfig, String vCenterConfig) {
         OrderedDictionary nodeDictionary = new OrderedDictionary();
         List<Exception> vmExceptions = new List<Exception>();
         //We are not looking for anything in the vCenter if a configuration for the vCenter server is not specified:
    	   if (!String.IsNullOrEmpty (vCenterConfig)) {
    		   try {
    			   foreach (VmGuest vmGuest in ourCommunicationPlugins.ComPlugins.Find(x => x.PluginName.ToUpper().Equals("VMWARE")).GetAllNodesData<VmGuests, VmGuest>(vCenterConfig, String.Empty, out vmExceptions)) {
    				   String nodename = vmGuest.Name.Contains (".") ? vmGuest.Name.Split ('.') [0].ToUpper () : vmGuest.Name.ToUpper ();
    				   if (!String.IsNullOrEmpty (nodename) && !nodeDictionary.Contains (nodename)) {
    					   try {
    						   nodeDictionary.Add (nodename, vmGuest);
    					   } catch (Exception e) {
    						   DisplayException (e);
    						   continue;
    					   }
    				   }
    			   }
    		   } catch (Exception e) {
    			   DisplayException (e);
    		   }
    	   }
    	   foreach (Exception e in vmExceptions) {
            DisplayException (e);
    	   }
         //Add nodes from tsm to nodeDictionary only if the server name does not exist in nodeDictionary already:
         List<Exception> tsmExceptions = new List<Exception>();
    	   //We are not looking for anything on the TSM side if a configuration for the TSM servers are not specified:
    	   if (!String.IsNullOrEmpty (tsmServersConfig)) {
    		   try {
    			   foreach (TsmNode tsmNode in ourCommunicationPlugins.ComPlugins.Find(x => x.PluginName.ToUpper().Equals("TSM")).GetAllNodesData<TsmNodes, TsmNode>(tsmServersConfig, String.Empty, out tsmExceptions)) {
    				   if (!nodeDictionary.Contains (tsmNode.Name)) {
    					   try {
    						   nodeDictionary.Add (tsmNode.Name, tsmNode);
    					   } catch (Exception e) {
    						   DisplayException (e);
    						   continue;
    					   }
    				   }
    			   }
    		   } catch (Exception e) {
               if (console_present) Console.WriteLine("Fetching TSM Nodes Failed: ");
               DisplayException(e);            
    		   }
    		   foreach (Exception e in tsmExceptions) {
               DisplayException (e);
    		   }
    	   }
         return nodeDictionary;
      }
      /// <summary>
      /// Creates a excel report out of data in a OrdredDictionary fed to it
      /// </summary>
      /// <param name="sheetName">Name of the sheet in Excel to be created</param>
      /// <param name="fileNameSuffix">Name to let the filename end with</param>
      /// <param name="nodeDictionary">OrderedDictionary with the data we want to create an report on</param>
      public String CreateExcelReport(string sheetName, string fileNameSuffix, OrderedDictionary nodeDictionary) {
         CreateExcelDoc excel_app = new CreateExcelDoc(sheetName);
         fileNameSuffix = String.IsNullOrEmpty(fileNameSuffix) ? String.Empty : fileNameSuffix;
         string excelDocFileName = Directory.GetCurrentDirectory () + System.IO.Path.DirectorySeparatorChar + "diskReports-" + fileNameSuffix + DateTime.Now.Day.ToString () + DateTime.Now.Month.ToString () + DateTime.Now.Year.ToString () + ".xls";

         try {
            excel_app.CreateHeaders(1, 1, "All servers and their total sizes(" + DateTime.Now + ")", "B2", "H2", 6, "GRAY", true, 24, "n");
            excel_app.AddData(2, 2, "Server Name", "C3", "C3", "");
            excel_app.AddData(2, 3, "Total Disk (MB)", "D3", "D3", "");
            excel_app.AddData(2, 4, "Total minus System Disk (MB)", "E3", "E3", "");
            excel_app.AddData(2, 5, "System Disk Detected Size (MB)", "F3", "F3", "");
            excel_app.AddData(2, 6, "System Disk Free Space (MB)", "G3", "G3", "");
            excel_app.AddData(2, 7, "Data Source", "H3", "H3", "");

            int xCounter = 0;
            foreach (DictionaryEntry dic in nodeDictionary) {
    	         Type guest = new TypeDelegator (typeof(VmGuest));
    	         Type node = new TypeDelegator (typeof(TsmNode));
    	         if (guest.Equals (dic.Value.GetType ())) {
    		         excel_app.AddData (xCounter, 2, dic.Key.ToString (), "C" + (xCounter).ToString (), "C" + (xCounter).ToString (), "#,##0");
    		         excel_app.AddData (xCounter, 3, (((VmGuest)dic.Value).GetTotalStorageSpace () / 1024 / 1024).ToString (), "D" + (xCounter).ToString(), "D" + (xCounter).ToString(), "");
    		         excel_app.AddData (xCounter, 4, ((((VmGuest)dic.Value).GetTotalStorageSpace () / 1024 / 1024) - (((VmGuest)dic.Value).GetSystemDisk().Capacity / 1024 / 1024)).ToString(), "E" + (3 + xCounter).ToString (), "E" + (3 + xCounter).ToString (), "");
    		         excel_app.AddData (xCounter, 5, (((VmGuest)dic.Value).GetSystemDisk ().Capacity / 1024 / 1024).ToString (), "F" + (xCounter).ToString(), "F" + (xCounter).ToString(), "");
    		
                  
                  excel_app.AddData (xCounter, 6, (((VmGuest)dic.Value).GetSystemDisk ().FreeSpace / 1024 / 1024).ToString (), "G" + (xCounter).ToString(), "G" + (xCounter).ToString(), "");
    		         excel_app.AddData (xCounter, 7, "VMWare", "H" + (xCounter).ToString (), "H" + (xCounter).ToString (), "");
    	         } else if (node.Equals (dic.Value.GetType ())) {
    		         excel_app.AddData (xCounter, 2, dic.Key.ToString (), "C" + (xCounter).ToString (), "C" + (xCounter).ToString (), "#,##0");
    		         excel_app.AddData (xCounter, 3, (((TsmNode)dic.Value).GetTotalStorageSpace (TimeSpan.FromDays (8)) / 1024 / 1024).ToString (), "D" + (xCounter).ToString (), "D" + (xCounter).ToString (), "");
    		         excel_app.AddData (xCounter, 4, (((TsmNode)dic.Value).GetTotalStorageSpace (TimeSpan.FromDays (8)) - ((TsmNode)dic.Value).GetSystemDisk (new TimeSpan (28, 0, 0, 0, 0)).Capacity / 1024 / 1024).ToString (), "E" + (xCounter).ToString (), "E" + (xCounter).ToString (), "");
    		         excel_app.AddData (xCounter, 5, (((TsmNode)dic.Value).GetSystemDisk (new TimeSpan (28, 0, 0, 0, 0)).Capacity / 1024 / 1024).ToString (), "F" + (xCounter).ToString (), "F" + (xCounter).ToString (), "");
    		         excel_app.AddData (xCounter, 6, (100 - ((TsmNode)dic.Value).GetSystemDisk (new TimeSpan (28, 0, 0, 0, 0)).PCT_UTIL).ToString () + "%", "G" + (xCounter).ToString (), "G" + (xCounter).ToString (), "");
    		         excel_app.AddData (xCounter, 7, "TSM", "H" + (xCounter).ToString (), "H" + (xCounter).ToString (), "");
    	         }   
    	         xCounter++;
            }
            excel_app.SaveAndClose (excelDocFileName);
         } catch (Exception e) {
            DisplayException(e);
         }
         return excelDocFileName;
      }

      /// <summary>
      /// Creates and mails a report to a mail address
      /// </summary>
      /// <param name="tsmServersConfig">TSM configuration file</param>
      /// <param name="vCenterConfig">vCenter Configuration file</param>
      /// <param name="mailReceiver">The mail address to send to</param>
      public bool MailReport(String tsmServersConfig, String vCenterConfig, String mailReceiver) {
         OrderedDictionary nodeDictionary = FetchTsmVMwareNodeData(tsmServersConfig, vCenterConfig);
         XmlReaderLocal mailSettingReader = new XmlReaderLocal(Directory.GetCurrentDirectory() + System.IO.Path.DirectorySeparatorChar + "config_mailsettings.xml");
         List<Hashtable> hashtableList = mailSettingReader.ReadAllMailSenders();
         string excelDocFileName = CreateExcelReport("Server Report", null, nodeDictionary);

         foreach (Hashtable htable in hashtableList) {
            string smtpServer = (String)htable["SMTPSERVER"];
            string fromAddress = (String)htable["FROMADDRESS"];

            try {
    	         //Need to read correct sender and address from config file:
    	         ndrMailMessage mailSender = new ndrMailMessage (smtpServer, fromAddress, mailReceiver);
    	         mailSender.setRegularSubject("Disk Reports on " + DateTime.Now + ".xls");
    	         mailSender.addAttachment(excelDocFileName);
    	         mailSender.sendMessage();
            } catch (Exception e) {
                     DisplayException(e);
            }
         }
         return (nodeDictionary.Count > 0 ? true : false);
      }      
   }
}