using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using Newtonsoft.Json;
using ApprovaFlow.Filters;
using ApprovaFlow.Workflow;
using System.IO;

namespace TestSuite
{
    [TestFixture]
    public class JsonTests
    {
        [Test]
        public void CanSerializeFilterDefinitions()
        {
            string source = @"F:\vs10dev\ApprovaFlowSimpleWorkflowProcessor\TestSuite\TestPlugins";
            string outputSource = @"F:\vs10dev\ApprovaFlowSimpleWorkflowProcessor\TestSuite\TestData\output.json";

            var filterRegistry = new FilterRegistry<Step>();
            filterRegistry.LoadPlugInsFromShare(source);
            string json = filterRegistry.SerializeFilterDefinitions();

            Assert.AreEqual(7, filterRegistry.GetFilterCount());

            Assert.IsTrue(json.Length > 0);
            WriteFile(json, outputSource);
        }

        [Test]
        public void CanDeserializeFilterDefinitions()
        {
            string source = @"F:\vs10dev\ApprovaFlowSimpleWorkflowProcessor\TestSuite\TestData\partialoutput.json";
            string json = GetFileContent(source);

            var filterDefs = new List<FilterDefinition<Step>>();
            filterDefs = JsonConvert.DeserializeObject<List<FilterDefinition<Step>>>(json);

            Assert.AreEqual(1, filterDefs.Count);
        }

        [Test]
        public void CanDeserializePartialFilterDefinition()
        { 
            
        }

        #region Helper functions

        private void WriteFile(string content, string source)
        {   
            var fileInfo = new FileInfo(source);

            var sw = fileInfo.CreateText();
            sw.Write(content);

            sw.Close();
        }

        private string GetFileContent(string source)
        {
            var fileInfo = new FileInfo(source);
            StreamReader sr = fileInfo.OpenText();

            string content = sr.ReadToEnd();
            return content;
        }

        #endregion
    }
}
