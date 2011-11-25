using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ApprovaFlow.Workflow;

namespace ApprovaFlow.Filters
{
    /// <summary>
    /// Accept a delegate so that the pipeline can execute a method from
    /// an object.
    /// </summary>
    public class ActionWrapperFilter : FilterBase<Step>
    {
        private Func<Step, Step> doAction;

        public ActionWrapperFilter(Func<Step, Step> action)
        {
            this.doAction = action;
        }
        
        protected override Step Process(Step input)
        {
            if(input.CanProcess)
            {
                this.doAction.Invoke(input);
            }

            return input;
        }
    }
}
