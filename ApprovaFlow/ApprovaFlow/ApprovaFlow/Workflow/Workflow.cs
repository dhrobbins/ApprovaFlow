using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ApprovaFlow.Workflow
{
    public class Workflow
    {
        public string WorkflowId { get; set; }
        public string WorkflowType { get; set; }
        public List<State> States { get; set; }
        public List<Trigger> Triggers { get; set; }
        public List<StateConfig> StateConfigs { get; set; }

        public Workflow() { }
    }
}
