using System;
using System.IO;

namespace DiskReporter {
   class Program {
        /// <summary>
        ///  Displays the help menu
        /// </summary>
		static void DisplayHelpMenu() {
			Console.WriteLine ("");
			Console.WriteLine ("****Disk Report System Help Screen****");
			Console.WriteLine ("");
			Console.WriteLine ("Possible command options:");
			Console.WriteLine ("    -mailReport someGuy@someDomain.com - optional");
			Console.WriteLine ("    -tsm - to only info fetch from TSM - optional");
			Console.WriteLine ("    -vmware - to only info fetch from VMware - optional");
			Console.WriteLine ("    -server - serverName to get info about in VMWare ot TSM - optional");
			Console.WriteLine ("    -excel - pipe result to XML - optional");
			Console.WriteLine ("    -help - to show this screen");
		}
        /// <summary>
        ///  Runs the console application if compiled as one
        /// </summary>
      	static void Main(string[] args) {
	        InputArguments arguments = new InputArguments(args);
	        string configDirectory = "";
	        Boolean runStatus = true;
			string logName = "DiskReporter.log";
					 
	        StreamWriter log;
			if (!File.Exists(logName)) {
				log = new StreamWriter(logName);
	        } else {
				log = File.AppendText(logName);
	        }
	    	DiskReporterMainRunFlows programFlow = new DiskReporterMainRunFlows(log);
			if(String.IsNullOrEmpty(arguments["-tsm"]) && String.IsNullOrEmpty(arguments["-vmware"])) {
				arguments.AddInputArguments (new[] {"-tsm","-vmware"});

			}
			if (!String.IsNullOrEmpty(arguments ["-help"])) {
				DisplayHelpMenu ();
			} else if (!String.IsNullOrEmpty(arguments["-mailReport"]) && !String.IsNullOrEmpty(arguments["-mailReport"])) {
				string mailReceiver = MailValidator.IsValid(arguments["-mailReport"]) ? arguments["-mailReport"] : "";
				if (!String.IsNullOrEmpty(mailReceiver)) {
					runStatus = programFlow.MailReport(!String.IsNullOrEmpty(arguments["-tsm"]) ? configDirectory + "config_TSMServers.xml" : String.Empty,
					                                   !String.IsNullOrEmpty(arguments["-vmware"]) ? configDirectory + "config_vCenterServer.xml" : String.Empty,
					                                   mailReceiver);
				} else {
					DisplayHelpMenu ();
				}
			} else {
				string serverName = !String.IsNullOrEmpty(arguments["-server"]) ? arguments["-server"] : String.Empty;
				var result = programFlow.FetchAllNodeDataAsSeparatedDicts(
					!String.IsNullOrEmpty(arguments["-tsm"]) ? configDirectory + "config_TSMServers.xml" : String.Empty,
					!String.IsNullOrEmpty(arguments["-vmware"]) ? configDirectory + "config_vCenterServer.xml" : String.Empty, 
					serverNameFilter: serverName);

				System.Collections.Specialized.OrderedDictionary vmwareNodeDictionary = result.Item1;
				System.Collections.Specialized.OrderedDictionary tsmNodeDictionary = result.Item2;

				if (vmwareNodeDictionary.Count == 0 && !String.IsNullOrEmpty(arguments["-server"])) Console.WriteLine(serverName + " was not found in VMWare.");
				else {
					if (System.Diagnostics.Debugger.IsAttached) Console.WriteLine("\nTHE FOLLOWING RESPONSE IS TEST DATA AS THE CODE WAS RUN IN DEBUG MODE!");
					Console.WriteLine("VMWare response:");
					foreach (System.Collections.DictionaryEntry entry in vmwareNodeDictionary) {
						if(entry.Value is DiskReporter.VmGuest) {
							var node = (DiskReporter.VmGuest)entry.Value;
							System.Console.WriteLine (entry.Key + ", " + node.TotalStorage + " bytes");
						} else {
							System.Console.WriteLine(entry.Key + ", " + entry.Value + " bytes");
						}
					}
					if(!String.IsNullOrEmpty(arguments ["-excel"]) && vmwareNodeDictionary.Count != 0) {
						string excelDocFileName = programFlow.CreateExcelReport("Server Report VMWare", "VMWare", vmwareNodeDictionary);
						if(!String.IsNullOrEmpty(excelDocFileName)) Console.WriteLine("Wrote results to " + excelDocFileName);
						else Console.WriteLine("Warning: looks like writing to Excel file did not succeed for VMWare nodes, please check the same folder as the executable.");
					}
				}
				if (tsmNodeDictionary.Count == 0 && !String.IsNullOrEmpty(arguments["-server"])) Console.WriteLine(serverName + " was not found in TSM.");
				else {
					if (System.Diagnostics.Debugger.IsAttached) Console.WriteLine("\nTHE FOLLOWING RESPONSE IS TEST DATA AS THE CODE WAS RUN IN DEBUG MODE!");
					Console.WriteLine("TSM response:");
					foreach (System.Collections.DictionaryEntry entry in tsmNodeDictionary) {
						if(entry.Value is DiskReporter.TsmNode) {
							var node = (DiskReporter.TsmNode)entry.Value;
							System.Console.WriteLine (entry.Key + ", " + node.TotalStorage + " MB");
						} else {
							System.Console.WriteLine(entry.Key + ", " + entry.Value + " MB");
						}
					}
					if(!String.IsNullOrEmpty(arguments ["-excel"]) && tsmNodeDictionary.Count != 0 ) {
						string excelDocFileName = programFlow.CreateExcelReport("Server Report TSM", "TSM", tsmNodeDictionary);
						if(!String.IsNullOrEmpty(excelDocFileName)) Console.WriteLine("Wrote results to " + excelDocFileName);
						else Console.WriteLine("Warning: looks like writing to Excel file for TSM nodes did not succeed, please check the same folder as the executable.");
					}
				}
			}
	     	log.Close();
	     	//Console.ReadLine();
	     	if (runStatus) {
	        	Environment.Exit(0);
	     	} else {
	        	Environment.Exit(1);
	     	}        
      	}
   	}  
}
