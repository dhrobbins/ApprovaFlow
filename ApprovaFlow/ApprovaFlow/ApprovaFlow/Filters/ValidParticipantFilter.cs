using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ApprovaFlow.Filters;
using ApprovaFlow.Workflow;

namespace ApprovaFlow.Filters
{
    /// <summary>
    /// Validate that the supplier of the answer has authorization to do so.
    /// </summary>
    public class ValidParticipantFilter : FilterBase<Step>
    {
        protected override Step Process(Step input)
        {
            if (input.CanProcess)
            {                
                input.Parameters["ValidFired"] = true;
                input.Parameters["FilterOrder"] += "ValidParticipantFilter;";

                input.CanProcess = input.IsUserValidParticipant();

                if(input.CanProcess == false)
                {
                    input.ErrorList.Add("Invalid Pariticipant - " + input.AnsweredBy);
                }
            }

            return input;
        }
    }
}
