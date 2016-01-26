using NUnit.Framework;
using System;
using System.Text;
using System.IO;
using System.Reflection;
using System.Collections.Generic;

namespace DiskReporter {
    [TestFixture()]
    public class TestVMwareNodesPlugin {
        [Test()]
        public void TestCase() {
            string configDirectory = Directory.GetCurrentDirectory();
            string tsmConfig = System.IO.Path.DirectorySeparatorChar + "config_vCenterServer.xml";
            List<Exception> exceptionList = new List<Exception>();
            StringBuilder sBuilder = new StringBuilder();

            VmPlugin ourPlugin = new VmPlugin("VMware") { NodeObjectType = new TypeDelegator (typeof(VmGuest)), NodesObjectType = new TypeDelegator (typeof(VmGuests)) };

            Boolean testResult = ourPlugin.CheckPrerequisites(out exceptionList);
            exceptionList.ForEach(x => sBuilder.Append(x.ToString()));
            Assert.AreEqual(0, exceptionList.Count, sBuilder.ToString());
            Assert.AreEqual(true, testResult, sBuilder.ToString());
            exceptionList.Clear();
            VmGuests ourNodes = ourPlugin.GetAllNodesData<VmGuests, VmGuest>(Path.Combine(configDirectory, tsmConfig), String.Empty, out exceptionList);
            Assert.IsNotNull(ourPlugin, "Expected ourNodes to be instantiated");
            Assert.Greater(ourNodes.Nodes.Count, 0, "Expected ourNodes to be instantiated with more then 0 nodes");
            sBuilder.Clear();
            exceptionList.ForEach(x => sBuilder.Append(x.ToString()));
            Assert.AreEqual(0, exceptionList.Count, "Expected the exceptionList to have 0 exceptions: " + sBuilder.ToString());
        }
    }
}

