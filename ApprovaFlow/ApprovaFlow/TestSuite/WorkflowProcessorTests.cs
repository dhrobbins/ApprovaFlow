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
    public class WorkflowProcessorTests
    {
        [Test]
        [Category("WorkflowProcessor")]
        public void CanConfigureProcessorWithPipelineAndRegistry()
        {
            string preFilterNames = "FetchDataFilter;ValidParticipantFilter;";
            string postFilterNames = "SaveDataFilter";
            
            var parameters = new Dictionary<string, object>();
            parameters.Add("FilterOrder", string.Empty);
            parameters.Add("FetchDataFired", false);
            parameters.Add("SaveDataFired", false);
            parameters.Add("ValidFired", false);

            var step = new Step("13", "12", "Manager Approve", "Request Promotion",
                                    "Approve", DateTime.Now, "Spock", "Spock;Kirk",
                                    parameters);
            step.CanProcess = true;

            var filterRegistry = new FilterRegistry<Step>();
            var processor = new WorkflowProcessor(step, filterRegistry, new Workflow());
            processor.ConfigurePipeline(preFilterNames, postFilterNames);

            var filterNames = processor.GetFilterNames();            
            filterNames.ForEach(x => Console.WriteLine(x));

            Assert.AreEqual(4, filterNames.Count);
        }

        [Test]
        [Category("WorkflowProcessor")]
        public void CanConfigureProcessorStateMachine()
        {
            string source = @"F:\vs10dev\ApprovaFlowSimpleWorkflowProcessor\TestSuite\TestData\RequestPromotion.json";
            var workflow = DeserializeWorkflow(source);

            var step = new Step("13", "12", "Manager Approve", "Request Promotion",
                                    "Approve", DateTime.Now, "Spock", "Spock;Kirk",
                                    new Dictionary<string, object>());            
            var filterRegistry = new FilterRegistry<Step>();
            
            var processor = new WorkflowProcessor(step, filterRegistry, workflow);
            processor.ConfigureStateMachine();

            Assert.AreEqual("Manager Approve", processor.GetCurrentState());
        }

        [Test]
        [Category("WorkflowProcessor")]
        public void CanTriggerChangeInState()
        {
            string source = @"F:\vs10dev\ApprovaFlowSimpleWorkflowProcessor\TestSuite\TestData\RequestPromotion.json";
            string preFilterNames = "FetchDataFilter;ValidParticipantFilter;";
            string postFilterNames = "SaveDataFilter";            
            
            var workflow = DeserializeWorkflow(source);

            var parameters = new Dictionary<string, object>();
            parameters.Add("FilterOrder", string.Empty);
            parameters.Add("FetchDataFired", false);
            parameters.Add("SaveDataFired", false);
            parameters.Add("ValidFired", false);

            var step = new Step("13", "12", "ManagerReview", "RequestPromotionForm",
                                    "Approve", DateTime.Now, "Spock", "Spock;Kirk",
                                    parameters);
            step.CanProcess = true;

            var filterRegistry = new FilterRegistry<Step>();

            var processor = new WorkflowProcessor(step, filterRegistry, workflow);
            string newState = processor.ConfigurePipeline(preFilterNames, postFilterNames)
                        .ConfigureStateMachine()
                        .ProcessAnswer()
                        .GetCurrentState();

            Assert.AreEqual("VicePresidentApprove", newState);                        
        }

        [Test]
        [Category("WorkflowProcessor")]
        public void CanReportErrorsWithProcessor()
        {
            string source = @"F:\vs10dev\ApprovaFlowSimpleWorkflowProcessor\TestSuite\TestData\RequestPromotion.json";
            string preFilterNames = "FetchDataFilter;ValidParticipantFilter;";
            string postFilterNames = "SaveDataFilter";

            var workflow = DeserializeWorkflow(source);

            var parameters = new Dictionary<string, object>();
            parameters.Add("FilterOrder", string.Empty);
            parameters.Add("FetchDataFired", false);
            parameters.Add("SaveDataFired", false);
            parameters.Add("ValidFired", false);

            var step = new Step("13", "12", "ManagerReview", "RequestPromotionForm",
                                    "Approve", DateTime.Now, "Data", "Spock;Kirk",
                                    parameters);
            step.CanProcess = true;

            var filterRegistry = new FilterRegistry<Step>();

            var processor = new WorkflowProcessor(step, filterRegistry, workflow);
            string newState = processor.ConfigurePipeline(preFilterNames, postFilterNames)
                        .ConfigureStateMachine()
                        .ProcessAnswer()
                        .GetCurrentState();

            Assert.AreEqual("ManagerReview", newState);
            Assert.AreEqual(1, processor.GetErrorList().Count);
        }        

        #region Helpers

        private Workflow DeserializeWorkflow(string source)
        {
            var fileInfo = new FileInfo(source);

            if (fileInfo.Exists == false)
            {
                throw new ApplicationException("RequestPromotion.Configure - File not found");
            }

            StreamReader sr = fileInfo.OpenText();
            string json = sr.ReadToEnd();
            sr.Close();
            
            var workflow = JsonConvert.DeserializeObject<Workflow>(json);
            return workflow;
        }

        #endregion
    }
}
