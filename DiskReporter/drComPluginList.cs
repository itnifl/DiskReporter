using System;
using System.Collections.Generic;
using DiskReporter.PluginContracts;

namespace DiskReporter {
	/// <summary>
	///  This class keeps track of all our plugins that we use for fetching node data with
	/// </summary>
	public class ComPluginList : IComPluginList {
		public List<IComPlugin> ComPlugins { get; set; }

        /* Want to implement auto loading of existing plugins here later, 
        *  via both loading of dlls in a folder or instantiating of correct classes.
        *  After the plugins are loaded, I want them to be fully encapsulated and automatically 
        *  ready for use without extensive outside coding.
        */
		public ComPluginList() {
			ComPlugins = new List<IComPlugin>();
            LoadAllPlugins();
		}

		public IEnumerator<IComPlugin> GetEnumerator() {
			ComPlugins.Sort(delegate(IComPlugin p1, IComPlugin p2) {
				return p1.PluginName.CompareTo(p2.PluginName);
			});
			foreach (IComPlugin plugin in ComPlugins) {
				yield return plugin;
			}
		}
        /// <summary>
        /// Register plugin into list of plugins
        /// </summary>
        /// <param name="plugin">The plugin that implements IComPlugin we want to register</param>
		public bool RegisterPlugin(IComPlugin plugin) {
			try {
				ComPlugins.Add(plugin);
			} catch {
				return false;
			}
			return true;
		}
        /// <summary>
        /// Loads all plugins that are defined in the method below
        /// </summary>
        private void LoadAllPlugins() {
            this.RegisterPlugin(new VmMethods("VMware"));
            this.RegisterPlugin(new TsmMethods("TSM"));
        }
	}
}

