using NUnit.Framework;
using System;
using System.Text;
using System.IO;
using System.Reflection;
using System.Collections.Generic;

namespace DiskReporter {
    [TestFixture()]
    public class TestTsmNodesPlugin {
        [Test()]
        public void TestCase() {
            string configDirectory = Directory.GetCurrentDirectory();
            string tsmConfig = System.IO.Path.VolumeSeparatorChar + "config_TSMServers.xml";
            List<Exception> exceptionList = new List<Exception>();
            StringBuilder sBuilder = new StringBuilder();

            TsmPlugin ourTsmPlugin = new TsmPlugin("TSM") { NodeObjectType = new TypeDelegator (typeof(TsmNode)), NodesObjectType = new TypeDelegator (typeof(TsmNodes)) };
            if (!System.Diagnostics.Debugger.IsAttached) {
                Boolean testResult = ourTsmPlugin.CheckPrerequisites(out exceptionList);
                exceptionList.ForEach(x => sBuilder.Append(x.ToString()));
                Assert.AreEqual(0, exceptionList.Count, sBuilder.ToString());
                Assert.AreEqual(true, testResult, sBuilder.ToString());
                exceptionList.Clear();
            }
            TsmNodes ourTsmNodes = ourTsmPlugin.GetAllNodesData<TsmNodes, TsmNode>(configDirectory + tsmConfig, String.Empty, out exceptionList);
            Assert.IsNotNull(ourTsmNodes, "Expected ourTsmNodes to be instantiated");
            Assert.Greater(ourTsmNodes.Nodes.Count, 0, "Expected ourTsmNodes to be instantiated with more then 0 nodes");
            sBuilder.Clear();
            exceptionList.ForEach(x => sBuilder.Append(x.ToString()));
            Assert.AreEqual(0, exceptionList.Count, "Expected the exceptionList to have 0 exceptions: " + sBuilder.ToString());
        }
    }
}