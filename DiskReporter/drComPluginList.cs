using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using DiskReporter.PluginContracts;

namespace DiskReporter {
	/// <summary>
	///  This class keeps track of all our plugins that we use for fetching node data with
	/// </summary>
	public class ComPluginList : IComPluginList {
        /// <summary>
        ///  Our list of plugins
        /// </summary>
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
        /// Instantiates all plugins that are defined in the method to usable objects
        /// </summary>
        public void LoadAllPlugins() {
            this.RegisterPlugin(new VmPlugin("VMware") { NodeObjectType = new TypeDelegator(typeof(VmGuest)), NodesObjectType = new TypeDelegator (typeof(VmGuests)) });
            this.RegisterPlugin(new TsmPlugin("TSM") { NodeObjectType = new TypeDelegator (typeof(TsmNode)), NodesObjectType = new TypeDelegator (typeof(TsmNodes)) });
        }
        /// <summary>
        /// Remove plugin from list of plugins by name
        /// </summary>
        /// <param name="pluginName">The plugin name of the plugin that we want to remove</param>
        public bool RemovePlugin(string pluginName) {
            var item = ComPlugins.SingleOrDefault(x => x.PluginName == pluginName);
            if (item != null) {
                ComPlugins.Remove(item);
                return true;
            }
            return false;
        }
        /// <summary>
        /// Remove plugin from list of reference to plugin
        /// </summary>
        /// <param name="plugin">The plugin that implements IComPlugin we want to remove</param>
        public void RemovePlugin(IComPlugin plugin) {
            ComPlugins.Remove(plugin);
        }
        /// <summary>
        /// Returns all names of all plugins as a string array 
        /// </summary>
        public string[] GetAllPluginNames() {
            return ComPlugins.Select(x => x.PluginName).ToArray();
        }
        /// <summary>
        /// Returns all names of all plugins as a string array 
        /// </summary>
        public Tuple<bool, string[]> VerifyAllRegisteredPlugins() {
            List<string> failures = new List<string>();
            bool allOK = false;
            foreach(IComPlugin plugin in ComPlugins) {
                if(!plugin.CheckPrerequisites()) {
                    failures.Add("Warning: " + plugin.PluginName + " failed its prerequisites check.");
                }
            }
            if(failures.Count == 0) {
                failures.Add("All plugins passed");
                allOK = true;
            }
            return Tuple.Create(allOK, failures.ToArray());
        }
    }
}

