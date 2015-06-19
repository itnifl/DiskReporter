using System;
using System.Collections.Generic;

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
		bool RegisterPlugin(IComPlugin plugin);
	}
    /// <summary>
    /// Represents a plugin
    /// </summary>
	public interface IComPlugin {	
		string PluginName { get; set; }

		IComPlugin GetPlugin();
        /// <summary>
        /// Retrieves all nodes where T1 is the list(IComNodeList) of nodes(IComNode) and T2 is the node(IComNode).
        /// </summary>
		T1 GetAllNodesData<T1, T2>(String sourceConfigFileName, string nameFilter, out List<Exception> outExceptions) 
			where T1 : IComNodeList<T2>, new()
				where T2 : IComNode, new();
	}
}