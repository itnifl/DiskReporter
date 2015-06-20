using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace DiskReporter.PluginContracts {
    /// <summary>
    /// Represents a list of nodes returned by the plugin
    /// </summary>
	public interface IComNodeList<T> where T : IComNode, new() {	
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
	public interface IComNode {	
		string Name { get; set; }
		List<GeneralDisk> Disks { get; set; }
		long? TotalStorage { get; set; }
		long? TotalSystemStorage { get; set; }
		  
		void AddDisk(GeneralDisk disk);
		long? GetTotalStorageSpace();
		GeneralDisk GetSystemDisk();
	}
    /// <summary>
    /// Represents a list of plugins
    /// </summary>
	public interface IComPluginList {
		List<IComPlugin> ComPlugins { get; set; }

        IEnumerator<IComPlugin> GetEnumerator();
        Tuple<bool, string[]> VerifyAllRegisteredPlugins();
		bool RegisterPlugin(IComPlugin plugin);
        bool RemovePlugin(string pluginName);
        string[] GetAllPluginNames();
        void LoadAllPlugins();
	}
    /// <summary>
    /// Represents a plugin
    /// </summary>
	public interface IComPlugin {	
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

		IComPlugin GetPlugin();
        /// <summary>
        /// Retrieves all nodes where T1 is the list(IComNodeList) of nodes(IComNode) and T2 is the node(IComNode).
        /// </summary>
		T1 GetAllNodesData<T1, T2>(String sourceConfigFileName, string nameFilter, out List<Exception> outExceptions) 
			where T1 : IComNodeList<T2>, new()
				where T2 : IComNode, new();
        bool CheckPrerequisites(out List<Exception> outExceptions);
	}
}