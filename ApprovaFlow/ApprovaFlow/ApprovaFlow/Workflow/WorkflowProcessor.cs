using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Stateless;
using Newtonsoft.Json;
using ApprovaFlow.Filters;
using ApprovaFlow.Utils;

namespace ApprovaFlow.Workflow
{
    public class WorkflowProcessor
    {
        private StateMachine<string, string> stateMachine;
        private Step step;
        private FilterRegistry<Step>filterRegistry;
        private Pipeline<Step> pipeline;
        private Workflow workflow;

        public WorkflowProcessor(Step theStep, FilterRegistry<Step> registry, Workflow workflow)
        {
            this.step = theStep;
            this.filterRegistry = registry;
            this.workflow = workflow;

            this.pipeline = new Pipeline<Step>();
        }

        /// <summary>
        /// Set up the pipeline based on a list of pre and post process filters that
        /// execute before or after the workflow processor's ExecuteTriggerFilter       
        /// </summary>
        /// <param name="preProcessFilterNames">Filter names to execute prior to ExecuteTriggerFilter as string</param>
        /// <param name="postProcessFilterNames">Filter names to execute after to ExecuteTriggerFilter as string</param>
        /// <returns>WorkflowProcessor for fluent interface</returns>
        public WorkflowProcessor ConfigurePipeline(string preProcessFilterNames, string postProcessFilterNames)
        {   
            Enforce.That(string.IsNullOrEmpty(preProcessFilterNames) == false,
                            "WorkflowProcessor.Configure - preProcessFilterNames can not be null");

            Enforce.That(string.IsNullOrEmpty(postProcessFilterNames) == false,
                            "WorkflowProcessor.Configure - postProcessFilterNames can not be null");
            
            var actionWrapper = new ActionWrapperFilter(this.ExecuteTriggerFilter);

            this.pipeline.RegisterFromList(preProcessFilterNames, this.filterRegistry)
                            .Register(actionWrapper)
                            .RegisterFromList(postProcessFilterNames, this.filterRegistry);

            return this;
        }

        /// <summary>
        /// Set up the StateMachine for this workflow by adding States and Triggers
        /// specified in StateConfigs
        /// </summary>
        /// <returns>WorkflowProcessor for fluent interface</returns>
        public WorkflowProcessor ConfigureStateMachine()
        {            
            Enforce.That(string.IsNullOrEmpty(this.step.State) == false,
                            "WorkflowProcessor.Confgiure - step.State can not be empty");

            this.stateMachine = new StateMachine<string, string>(this.step.State);

            //  Get a distinct list of states with a trigger from state configuration
            //  "State => Trigger => TargetState
            var states = this.workflow.StateConfigs.AsQueryable()
                                    .Select(x => x.State)
                                    .Distinct()
                                    .Select(x => x)
                                    .ToList();

            //  Assing triggers to states
            states.ForEach(state =>
            {
                var triggers = this.workflow.StateConfigs.AsQueryable()
                                   .Where(config => config.State == state)
                                   .Select(config => new { Trigger = config.Trigger, TargeState = config.TargetState })
                                   .ToList();

                triggers.ForEach(trig =>
                {
                    this.stateMachine.Configure(state).Permit(trig.Trigger, trig.TargeState);
                });
            });

            return this;
        }

        /// <summary>
        /// Execute state machine trigger
        /// </summary>
        /// <returns>WorkflowProcessor for fluent interface</returns>
        public WorkflowProcessor ProcessAnswer()
        {
            this.pipeline.Execute(this.step);
            
            return this;
        }

        /// <summary>
        /// Fetch filter names from used by this workflow
        /// </summary>
        /// <returns>Names as list of strings</returns>
        public List<string> GetFilterNames()
        {
            return this.pipeline.GetNames();
        }

        /// <summary>
        /// Fetch the statemachines current state
        /// </summary>
        /// <returns>Workflow state as string</returns>
        public string GetCurrentState()
        {
            return this.stateMachine.State;
        }

        /// <summary>
        /// Fetch the errors, if any, encountered while processing the 
        /// users answer
        /// </summary>
        /// <returns>Errors as List of strings</returns>
        public List<string> GetErrorList()
        {
            return this.step.ErrorList;
        }

        /// <summary>
        /// Given the current Step, execute the StateMachine to transition
        /// to the subsequent target state
        /// </summary>
        /// <param name="input">Current step as Step</param>
        /// <returns></returns>
        private Step ExecuteTriggerFilter(Step input)
        {
            this.stateMachine.Fire(input.Answer);
            
            return input;
        }
    }
}
