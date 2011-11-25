using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Janga.Validation;
using ApprovaFlow.Utils;

namespace ApprovaFlow.Workflow
{
    /// <summary>
    /// Step presents the State of a Workflowinstance and accepts and answer from 
    /// a user that will be used to trigger a transition to a different state.
    /// </summary>
    public class Step
    {
        #region Properties

        public string WorkflowInstanceId { get; set; }
        public string WorkflowId { get; set; }
        public string StepId { get; set; }
        public string State { get; set; }
        public string PreviousState { get; set; }
        public string Answer { get; set; }
        public DateTime Created { get; set; }
        public string AnsweredBy { get; set; }
        public string Participants { get; set; }

        public List<string> ErrorList;
        public bool CanProcess { get; set; }
        public IDictionary<string, object> Parameters { get; set; }

        #endregion

        #region Constructors

        public Step()
            : this(string.Empty, string.Empty, string.Empty, string.Empty,
                   string.Empty, new DateTime(), string.Empty, string.Empty,
                    new Dictionary<string,object>())
        { }
        
        public Step(string workflowInstanceId, string stepId, string state, string previousState,
                        string answer, DateTime created, string answeredBy, string participants,
                        Dictionary<string, object> parameters)
        {
            this.WorkflowInstanceId = workflowInstanceId;
            this.StepId = stepId;
            this.State = state;
            this.PreviousState = previousState;
            this.Answer = answer;
            this.Created = created;
            this.AnsweredBy = answeredBy;
            this.Participants = participants;

            this.ErrorList = new List<string>();
            this.Parameters = parameters;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Generate the step id from a guid.  Used when a step is first created
        /// </summary>
        /// <returns>New StepId as string</returns>
        public string CreateStepId()
        {
            Enforce.That(string.IsNullOrEmpty(this.StepId),
                            "Step.CreateStepId - StepId has already been defined - " +
                            this.StepId);

            this.StepId = Guid.NewGuid().ToString();

            return this.StepId;
        }

        /// <summary>
        /// Determine if a step has all the information needed to trigger
        /// the next state in a state machine
        /// </summary>
        /// <returns>True when valid</returns>
        public bool IsValidForWorkflowTransition()
        {
            return this.Enforce<Step>("Step", true)
                        .When("AnsweredBy", Janga.Validation.Compare.NotEqual, string.Empty)
                        .When("Answer", Janga.Validation.Compare.NotEqual, string.Empty)
                        .When("State", Janga.Validation.Compare.NotEqual, string.Empty)
                        .When("WorkflowInstanceId", Janga.Validation.Compare.NotEqual, string.Empty)                        
                        .IsValid;
        }

        /// <summary>
        /// Determine if the user is authorized to provide an answer
        /// </summary>
        /// <returns></returns>
        public bool IsUserValidParticipant()
        {
            return this.Enforce<Step>("Step", true)
                        .When("Participants", Janga.Validation.Compare.Contains, this.AnsweredBy)
                        .IsValid;
        }

        #endregion
    }
}
