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
    public class WorkflowScenarioTests
    {
        
        /// <summary>
        /// Should be able to run workflow and allow Kirk to excuse a red shirt
        /// from duty
        /// </summary>
        [Test]
        [Category("RedShirtPromotionWorkflow")]
        public void CanPromoteRedShirtOffLandingParty()
        {
            string source = @"F:\vs10dev\ApprovaFlowSimpleWorkflowProcessor\TestSuite\TestData\RedShirtPromotion.json";
            string preFilterNames = "FetchDataFilter;ValidParticipantFilter;";
            string postFilterNames = "SaveDataFilter";

            var workflow = DeserializeWorkflow(source);

            var parameters = new Dictionary<string, object>();
            parameters.Add("FilterOrder", string.Empty);
            parameters.Add("FetchDataFired", false);
            parameters.Add("SaveDataFired", false);
            parameters.Add("ValidFired", false);

            var step = new Step("13", "12", "RequestPromotionForm", "",
                                    "Complete", DateTime.Now, "RedShirtGuy", "Data;RedShirtGuy",
                                    parameters);
            step.CanProcess = true;

            var filterRegistry = new FilterRegistry<Step>();

            var processor = new WorkflowProcessor(step, filterRegistry, workflow);
            string newState = processor.ConfigurePipeline(preFilterNames, postFilterNames)
                        .ConfigureStateMachine()
                        .ProcessAnswer()
                        .GetCurrentState();

            Assert.AreEqual("FirstOfficerReview", newState);

            step.Answer = "Approve";
            step.AnsweredBy = "Spock";
            step.Participants = "Spock;Kirk";
            step.State = newState;

            processor = new WorkflowProcessor(step, filterRegistry, workflow);
            newState = processor.ConfigurePipeline(preFilterNames, postFilterNames)
                        .ConfigureStateMachine()
                        .ProcessAnswer()
                        .GetCurrentState();

            Assert.AreEqual("CaptainApproval", newState);

            step.Answer = "Approve";
            step.AnsweredBy = "Kirk";
            step.Participants = "Kirk";
            step.State = newState;

            processor = new WorkflowProcessor(step, filterRegistry, workflow);
            newState = processor.ConfigurePipeline(preFilterNames, postFilterNames)
                        .ConfigureStateMachine()
                        .ProcessAnswer()
                        .GetCurrentState();

            Assert.AreEqual("PromotedOffLandingParty", newState);
        }

        /// <summary>
        /// Should prevent McCoy from advancing a worflow when he is not listed
        /// as a participant
        /// </summary>
        [Test]
        [Category("RedShirtPromotionWorkflow")]
        public void CanPreventMcCoyFromOverridingKirk()
        {
            string source = @"F:\vs10dev\ApprovaFlowSimpleWorkflowProcessor\TestSuite\TestData\RedShirtPromotion.json";
            string preFilterNames = "FetchDataFilter;ValidParticipantFilter;";
            string postFilterNames = "SaveDataFilter";

            var workflow = DeserializeWorkflow(source);

            var parameters = new Dictionary<string, object>();
            parameters.Add("FilterOrder", string.Empty);
            parameters.Add("FetchDataFired", false);
            parameters.Add("SaveDataFired", false);
            parameters.Add("ValidFired", false);

            var step = new Step("13", "12", "RequestPromotionForm", "",
                                    "Complete", DateTime.Now, "RedShirtGuy", "Data;RedShirtGuy",
                                    parameters);
            step.CanProcess = true;

            var filterRegistry = new FilterRegistry<Step>();

            var processor = new WorkflowProcessor(step, filterRegistry, workflow);
            string newState = processor.ConfigurePipeline(preFilterNames, postFilterNames)
                        .ConfigureStateMachine()
                        .ProcessAnswer()
                        .GetCurrentState();

            Assert.AreEqual("FirstOfficerReview", newState);

            step.Answer = "Approve";
            step.AnsweredBy = "Spock";
            step.Participants = "Spock;Kirk";
            step.State = newState;

            processor = new WorkflowProcessor(step, filterRegistry, workflow);
            newState = processor.ConfigurePipeline(preFilterNames, postFilterNames)
                        .ConfigureStateMachine()
                        .ProcessAnswer()
                        .GetCurrentState();

            Assert.AreEqual("CaptainApproval", newState);

            //  Workflow will not process the trigger because
            //  the ValidParticipant filter will set setp.CanProcess to false
            step.Answer = "Approve";
            step.AnsweredBy = "McCoy";
            step.Participants = "Kirk";
            step.State = newState;

            processor = new WorkflowProcessor(step, filterRegistry, workflow);
            newState = processor.ConfigurePipeline(preFilterNames, postFilterNames)
                        .ConfigureStateMachine()
                        .ProcessAnswer()
                        .GetCurrentState();

            Assert.AreEqual("CaptainApproval", newState);
            Assert.IsFalse(step.CanProcess);
            Assert.AreEqual(1, processor.GetErrorList().Count);
            Console.WriteLine(processor.GetErrorList()[0]);
        }

        /// <summary>
        /// Should enable McCoy to issue an override to Kirk's orders.
        /// NOTE:  You must rename the "rename_this_to_manifest_for_workflowscenario_test.json"
        /// to "manifest.json" to run this test.
        /// </summary>
        [Test]
        [Category("RedShirtPromotionWorkflow")]
        public void CanAllowMcCoyToIssueUnfitForDuty()
        {
            string source = @"F:\vs10dev\ApprovaFlowSimpleWorkflowProcessor\TestSuite\TestData\RedShirtPromotion.json";
            string pluginSource = @"F:\vs10dev\ApprovaFlowSimpleWorkflowProcessor\TestSuite\TestPlugins";
            string preFilterNames = "FetchDataFilter;MorePlugins.TransporterDiagnosisFilter;ValidParticipantFilter;";
            string postFilterNames = "MorePlugins.TransporterRepairFilter;Plugins.CaptainUnfitForCommandFilter;SaveDataFilter;";

            var workflow = DeserializeWorkflow(source);

            var parameters = new Dictionary<string, object>();
            parameters.Add("FilterOrder", string.Empty);
            parameters.Add("FetchDataFired", false);
            parameters.Add("SaveDataFired", false);
            parameters.Add("ValidFired", false);

            //  Mission to beam down issue and the red shirt wants off
            var step = new Step("13", "12", "RequestPromotionForm", "",
                                    "Complete", DateTime.Now, "RedShirtGuy", "Data;RedShirtGuy",
                                    parameters);
            step.CanProcess = true;

            var filterRegistry = new FilterRegistry<Step>();
            filterRegistry.LoadPlugInsFromShare(pluginSource);

            var processor = new WorkflowProcessor(step, filterRegistry, workflow);
            string newState = processor.ConfigurePipeline(preFilterNames, postFilterNames)
                        .ConfigureStateMachine()
                        .ProcessAnswer()
                        .GetCurrentState();

            //  Spock says he'll evaluate request
            Assert.AreEqual("FirstOfficerReview", newState);

            step.Answer = "Approve";
            step.AnsweredBy = "Spock";
            step.Participants = "Spock;Kirk";
            step.State = newState;

            processor = new WorkflowProcessor(step, filterRegistry, workflow);
            newState = processor.ConfigurePipeline(preFilterNames, postFilterNames)
                        .ConfigureStateMachine()
                        .ProcessAnswer()
                        .GetCurrentState();

            Assert.AreEqual("CaptainApproval", newState);

            //  Captain Kirt denies request, but McCoy issues unfit for command
            parameters.Add("KirkInfected", true);

            step.Answer = "Deny";
            step.AnsweredBy = "Kirk";
            step.Participants = "Kirk";
            step.State = newState;

            processor = new WorkflowProcessor(step, filterRegistry, workflow);
            newState = processor.ConfigurePipeline(preFilterNames, postFilterNames)
                        .ConfigureStateMachine()
                        .ProcessAnswer()
                        .GetCurrentState();

            //  Medical override issued and email to Starfleet generated
            bool medicalOverride = (bool)parameters["MedicalOverride"];
            bool emailSent = (bool)parameters["StarfleetEmail"];

            Assert.IsTrue(medicalOverride);
            Assert.IsTrue(emailSent);
        }

        /// <summary>
        /// Should be able to issue special orders based on previous conditions in
        /// the workflow.
        /// NOTE:  You must rename the "rename_this_to_manifest_for_workflowscenario_test.json"
        /// to "manifest.json" to run this test.
        /// </summary>
        [Test]
        [Category("RedShirtPromotionWorkflow")]
        public void CanIssueSpecialOrdersWithPlugin()
        {
            string source = @"F:\vs10dev\ApprovaFlowSimpleWorkflowProcessor\TestSuite\TestData\RedShirtPromotion.json";
            string pluginSource = @"F:\vs10dev\ApprovaFlowSimpleWorkflowProcessor\TestSuite\TestPlugins";
            string preFilterNames = "FetchDataFilter;MorePlugins.TransporterDiagnosisFilter;ValidParticipantFilter;";
            string postFilterNames = "MorePlugins.TransporterRepairFilter;SaveDataFilter;";

            var workflow = DeserializeWorkflow(source);

            var parameters = new Dictionary<string, object>();
            parameters.Add("FilterOrder", string.Empty);
            parameters.Add("FetchDataFired", false);
            parameters.Add("SaveDataFired", false);
            parameters.Add("ValidFired", false);            

            var step = new Step("13", "12", "RequestPromotionForm", "",
                                    "Complete", DateTime.Now, "RedShirtGuy", "Data;RedShirtGuy",
                                    parameters);
            step.CanProcess = true;

            var filterRegistry = new FilterRegistry<Step>();
            filterRegistry.LoadPlugInsFromShare(pluginSource);

            var processor = new WorkflowProcessor(step, filterRegistry, workflow);
            string newState = processor.ConfigurePipeline(preFilterNames, postFilterNames)
                        .ConfigureStateMachine()
                        .ProcessAnswer()
                        .GetCurrentState();

            Assert.AreEqual("FirstOfficerReview", newState);

            step.Answer = "Approve";
            step.AnsweredBy = "Spock";
            step.Participants = "Spock;Kirk";
            step.State = newState;

            processor = new WorkflowProcessor(step, filterRegistry, workflow);
            newState = processor.ConfigurePipeline(preFilterNames, postFilterNames)
                        .ConfigureStateMachine()
                        .ProcessAnswer()
                        .GetCurrentState();

            Assert.AreEqual("CaptainApproval", newState);

            //  Captain Kirt denies request, special order issued
            step.Answer = "Deny";
            step.AnsweredBy = "Kirk";
            step.Participants = "Kirk";
            step.State = newState;

            processor = new WorkflowProcessor(step, filterRegistry, workflow);
            newState = processor.ConfigurePipeline(preFilterNames, postFilterNames)
                        .ConfigureStateMachine()
                        .ProcessAnswer()
                        .GetCurrentState();

            Assert.AreEqual("Report to Shuttle Bay", step.Parameters["SpecialOrders"].ToString());
        }


        #region Helpers

        /// <summary>
        /// Fecth json and create workflow
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
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
