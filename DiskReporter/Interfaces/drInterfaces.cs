using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Collections.Specialized;

namespace DiskReporter.PluginContracts {
   /// <summary>
   /// Represents a list of nodes returned by the plugin
   /// </summary>
	public interface IReporterNodeList<T> where T : IReporterNode, new() {	
		List<T> Nodes { get; set; }

		void AddNode(T node);
		IEnumerator<T> GetEnumerator();
		long? GetTotalStorage(int daySpan);
		long? GetTotalWindowsSystemStorage(int daySpan);
		long? GetTotalLinuxRootStorage(int daySpan);
	}
   /// <summary>
   /// Represents a node in a list of nodes returned by a plugin
   /// </summary>
	public interface IReporterNode {	
      string Name { get; set; }
      List<GeneralDisk> Disks { get; set; }
      long? TotalStorage { get; set; }
      long? TotalSystemStorage { get; set; }
      string PowerStatus { get; set; }
      string IP { get; set; }
      string State { get; set; }
      string ToolsStatus { get; set; }
      string ToolsVersionStatus { get; set; }
      string OSFamily { get; set; }
		  
		void AddDisk(GeneralDisk disk);
		long? GetTotalStorageSpace();
		GeneralDisk GetSystemDisk();
	}
   /// <summary>
   /// Represents a list of plugins
   /// </summary>
	public interface IReporterPluginList {
      List<IReporterPlugin> ComPlugins { get; set; }

      IEnumerator<IReporterPlugin> GetEnumerator();
      Tuple<bool, string[]> VerifyAllRegisteredPlugins();
      bool RegisterPlugin(IReporterPlugin plugin);
      bool RemovePlugin(string pluginName);
      string[] GetAllPluginNames();
      void LoadAllPlugins(bool force);
	}
   /// <summary>
   /// Represents a plugin
   /// </summary>
   public interface IReporterPlugin {
      [Required(ErrorMessage = "The plugin needs to be named", AllowEmptyStrings = false)]
      string PluginName { get; set; }
      /// <summary>
      ///  Keeps track of what T1 should be like in GetAllNodesData
      /// </summary>
      [Required(ErrorMessage = "Type of object to list nodes required")]
      Type NodesObjectType { get; set;}
      /// <summary>
      ///  Keeps track of what T2 should be like in GetAllNodesData
      /// </summary>
      [Required(ErrorMessage = "Type of node object is required")]
      Type NodeObjectType { get; set;}
      /// <summary>
      /// Get all console commands this plugin offers
      /// </summary>
      /// <returns>Lst of commands that implement IReporterCommand</returns>
      IReporterCommands<T> GetCommands<T>() where T : IReporterCommand, new(); 
      /// <summary>
      /// Retrieves the plugin
      /// </summary>
      /// <returns>A plugin that implements IReporterPlugin</returns>
      IReporterPlugin GetPlugin();
      /// <summary>
      /// Retrieves all nodes where T1 is the list(IReporterNodeList) of nodes(IReporterNode) and T2 is the node(IReporterNode).
      /// </summary>
      T1 GetAllNodesData<T1, T2>(String sourceConfigFileName, string nameFilter, out List<Exception> outExceptions) 
         where T1 : IReporterNodeList<T2>, new()
	         where T2 : IReporterNode, new();
      bool CheckPrerequisites(out List<Exception> outExceptions);
   }
   public interface IReporterCommand {
      string Command { get; set; }
      string Description { get; set; }
      Func<String, OrderedDictionary> ExecuteCommand { get; set; }
   }
   public interface IReporterCommands<T> where T : IReporterCommand {
      List<T> Commands { get; set; }
   }
}