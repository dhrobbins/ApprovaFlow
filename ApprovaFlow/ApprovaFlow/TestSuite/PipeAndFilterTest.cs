using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using ApprovaFlow.Workflow;
using ApprovaFlow.Filters;

namespace TestSuite
{
    [TestFixture]
    public class PipeAndFilterTest
    {
        /// <summary>
        /// Should register 4 default filters with FilterRegistry
        /// </summary>
        [Test]
        [Category("FilterRegistry")]
        public void CanRegisterFilters()
        {
            var filterRegistry = new FilterRegistry<Step>();

            Assert.AreEqual(4, filterRegistry.GetFilterCount());
            Console.WriteLine(filterRegistry.GetFilterNames());
        }        

        /// <summary>
        /// Should be able to register filters with the Pipeline
        /// </summary>
        [Test]
        [Category("Pipeline")]
        public void CanRegisterFiltersWithPipeline()
        {
            var pipeline = new Pipeline<Step>();
            pipeline.Register(new ValidParticipantFilter())
                      .Register(new SaveDataFilter());

            Assert.AreEqual(2, pipeline.GetCount());
            Console.WriteLine(pipeline.GetNames());
        }

        /// <summary>
        /// Should be able to query the FilterRegistry and register
        /// filters with the Pipeline.
        /// </summary>
        [Test]
        [Category("Pipeline")]
        public void CanRegisterFiltersWithRegistry()
        {
            string filterNames = "ValidParticipantFilter;SaveDataFilter;TriggerStateFilter";
            
            var filterRegistry = new FilterRegistry<Step>();
            var pipeline = new Pipeline<Step>();
            pipeline.RegisterFromList(filterNames, filterRegistry);

            Assert.AreEqual(3, pipeline.GetCount());
            Console.WriteLine(pipeline.GetNames());
        }

        /// <summary>
        /// Should be able to provision a pipeline with a FunctionRegistry
        /// and execute all filters in pipeline
        /// </summary>
        [Test]
        [Category("Pipeline")]
        public void CanExecutePipeline()
        {
            var parameters = new Dictionary<string, object>();
            parameters.Add("FilterOrder", string.Empty);

            var step = new Step("13", "12", "Manager Approve", "Request Promotion",
                                    "Approve", DateTime.Now, "Spock", "Spock;Kirk",
                                    parameters);
            step.CanProcess = true;      

            string filterNames = "ValidParticipantFilter;SaveDataFilter;FetchDataFilter;";
            string reverseOrderFilterNames = "FetchDataFilter;SaveDataFilter;ValidParticipantFilter;";

            var filterRegistry = new FilterRegistry<Step>();
            var pipeline = new Pipeline<Step>();
            pipeline.RegisterFromList(filterNames, filterRegistry)
                        .Execute(step);

            Assert.IsTrue((bool)step.Parameters["ValidFired"]);
            Assert.IsTrue((bool)step.Parameters["SaveDataFired"]);
            Assert.IsTrue((bool)step.Parameters["FetchDataFired"]);

            Assert.AreEqual(filterNames, step.Parameters["FilterOrder"].ToString());

            //  Change up order
            step.Parameters.Clear();
            step.Parameters.Add("FilterOrder", string.Empty);

            var revPipeline = new Pipeline<Step>();
            revPipeline.RegisterFromList(reverseOrderFilterNames, filterRegistry)
                                .Execute(step);

            Assert.IsTrue((bool)step.Parameters["ValidFired"]);
            Assert.IsTrue((bool)step.Parameters["SaveDataFired"]);
            Assert.IsTrue((bool)step.Parameters["FetchDataFired"]);

            Assert.AreEqual(reverseOrderFilterNames,
                            step.Parameters["FilterOrder"].ToString());
        }

        
        /// <summary>
        /// Should be able to register a filter accepts a Func<Step,Step> and
        /// execute that filter from a pipeline
        /// </summary>
        [Test]
        [Category("Pipeline")]
        public void CanRegisterActionAsFilter()
        {
            var parameters = new Dictionary<string, object>();
            var actionWrapper = new ActionWrapperFilter(this.ActionFunction);
            var step = new Step("13", "12", "Manager Approve", "Request Promotion",
                                    "Approve", DateTime.Now, "Spock", "Spock;Kirk",
                                    parameters);
            step.CanProcess = true;

            var pipeline = new Pipeline<Step>();
            pipeline.Register(actionWrapper)
                    .Execute(step);

            Assert.AreEqual("ActionFunction fired", step.Parameters["ActionResults"].ToString());
        }

        #region Helpers

        private Step ActionFunction(Step input)
        {
            input.Parameters.Add("ActionResults", "ActionFunction fired");

            return input;
        }

        #endregion
    }
}
