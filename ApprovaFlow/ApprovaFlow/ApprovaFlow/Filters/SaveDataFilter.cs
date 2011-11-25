using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ApprovaFlow.Workflow;

namespace ApprovaFlow.Filters
{
    /// <summary>
    /// Save the workflow data
    /// </summary>
    public class SaveDataFilter : FilterBase<Step>
    {
        protected override Step Process(Step input)
        {
            if(input.CanProcess)
            {
                input.Parameters["SaveDataFired"] = true;
                input.Parameters["FilterOrder"] += "SaveDataFilter;";
            }

            return input;
        }
    }
}
