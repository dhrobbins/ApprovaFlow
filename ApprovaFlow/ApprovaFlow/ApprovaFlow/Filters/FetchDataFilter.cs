using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ApprovaFlow.Workflow;

namespace ApprovaFlow.Filters
{
    /// <summary>
    /// Gather data for a workflow
    /// </summary>
    public class FetchDataFilter : FilterBase<Step>
    {
        protected override Step Process(Step input)
        {
            if(input.CanProcess)
            {
                input.Parameters["FetchDataFired"] =  true;
                input.Parameters["FilterOrder"] += "FetchDataFilter;";
            }

            return input;
        }
    }
}
