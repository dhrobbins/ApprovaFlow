using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ApprovaFlow.Workflow;

namespace ApprovaFlow.Filters
{
    public class TriggerStateFilter : FilterBase<Step>
    {
        protected override Step Process(Step input)
        {
            if(input.CanProcess)
            {
                
            }
            
            return input;
        }
    }
}
