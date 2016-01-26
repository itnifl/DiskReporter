using NUnit.Framework;
using System;
using System.Text;
using System.IO;
using System.Reflection;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DiskReporter {
   [TestFixture()]
   public class TestNodeEdgeIntegration {
      [Test()]
      public void TestCase() {
         //drNodeEdgeIntegration ourEdgeIntegration = new drNodeEdgeIntegration();
         /* Not sure about this approach:
          * var resultTask = ourEdgeIntegration.FetchVMwareAndTSMServerData(String.Empty);
          * if (resultTask.Status != TaskStatus.RanToCompletion) resultTask.Start();
          * Assert.IsNotNull(resultTask, "Expected resultTask to be instantiated");
          * while (resultTask.Status != TaskStatus.RanToCompletion) {
          *    Wait for task to be done..
          * }
          * Assert.AreEqual(resultTask.Status, TaskStatus.RanToCompletion, "Expected TaskStatus to be RanToCompletion, but was: " + resultTask.Status);
          */
         //Object dataReturns = Task<Object>.Run(() => ourEdgeIntegration.FetchVMwareAndTSMServerData(String.Empty).Result);
         //Assert.IsNotNull(dataReturns, "Expected dataReturns to be instantiated");
         //Need to also verify the contents of the object.
      }
   }
}