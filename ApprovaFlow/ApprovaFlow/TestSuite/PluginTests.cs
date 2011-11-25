using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ApprovaFlow.Workflow;
using ApprovaFlow.Filters;
using NUnit.Framework;
using Newtonsoft.Json;
using System.IO;


namespace TestSuite
{
    [TestFixture]
    public class PluginTests
    {
        /// <summary>
        /// Should be able to read assembly from share and register any
        /// object with type FilterBase<T>"/>
        /// </summary>
        [Test]
        [Category("FilterRegistry")]
        public void CanRegisterPlugins()
        {
            string source = @"F:\vs10dev\ApprovaFlowSimpleWorkflowProcessor\Plugins\bin\Debug\Plugins.dll";

            var filterRegistry = new FilterRegistry<Step>();
            filterRegistry.LoadPlugIn(source);

            //  We should have 4 standard filters and one from the plugin
            Assert.AreEqual(5, filterRegistry.GetFilterCount());
            Console.WriteLine(filterRegistry.GetFilterNames());
        }

        /// <summary>
        /// Should be able to limit filter registry by entries in a manifest
        /// NOTE:  Rename file "Rename for only from manifest.json" to manifest.json
        /// </summary>
        [Test]
        [Category("FilterRegistry")]
        public void CanRegistorOnlyPluginsFromManifest()
        {
            string source = @"F:\vs10dev\ApprovaFlowSimpleWorkflowProcessor\TestSuite\TestPlugins";

            var filterRegistry = new FilterRegistry<Step>();
            filterRegistry.LoadPlugInsFromShare(source);

            Console.WriteLine(filterRegistry.GetFilterNames());
            Assert.AreEqual(6, filterRegistry.GetFilterCount());
        }


        /// <summary>
        /// Should be able to retrieve filter from registry and execute the filter
        /// </summary>
        [Test]
        [Category("FilterRegistry")]
        public void CanFetchAndExecuteFilter()
        {
            string source = @"F:\vs10dev\ApprovaFlowSimpleWorkflowProcessor\Plugins\bin\Debug\Plugins.dll";

            var filterRegistry = new FilterRegistry<Step>();
            filterRegistry.LoadPlugIn(source);

            var filters = filterRegistry.GetFilters("Plugins.CaptainUnfitForCommandFilter")
                                        .ToList();
            
            Assert.AreEqual(1, filters.Count);

            var parameters = new Dictionary<string, object>();
            parameters.Add("KirkInfected", true);

            var step = new Step("12w", "231a", "CaptainApproval", "FirstOfficeReview",
                                "Deny", DateTime.Now, "Kirk", "Kirk",
                                parameters);
            step.CanProcess = true;

            step = filters[0].Execute(step);

            Assert.AreEqual(false, step.CanProcess);
            Assert.AreEqual(1, step.ErrorList.Count);
            Assert.IsTrue((bool)step.Parameters["MedicalOverride"]);
            Console.WriteLine(step.ErrorList[0]);
        }

        /// <summary>
        /// Should be able to load plugins from two separate assemblies
        /// </summary>
        [Test]
        [Category("FilterRegistry")]
        public void CanLoadMultiplePluginAssemblies()
        {
            string source = @"F:\vs10dev\ApprovaFlowSimpleWorkflowProcessor\TestSuite\TestPlugins";

            var filterRegistry = new FilterRegistry<Step>();
            filterRegistry.LoadPlugInsFromShare(source);

            Console.WriteLine(filterRegistry.GetFilterNames());
            Assert.AreEqual(7, filterRegistry.GetFilterCount());
        }

        #region Helpers

        /// <summary>
        /// Rename the manifest.json file for special testing conditions
        /// </summary>
        /// <param name="currentName">Old name as string</param>
        /// <param name="newName">New name as string</param>
        private void RenameFileForManifestOnlyTest(string currentName, string newName)
        {
            var fileInfo = new FileInfo(currentName);

            if (fileInfo.Exists)
            {
                fileInfo.MoveTo(newName);
            }
        }

        #endregion
    }
}
