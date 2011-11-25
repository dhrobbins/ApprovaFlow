using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ApprovaFlow.Filters;
using System.Linq.Expressions;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System.Runtime.Serialization;

namespace ApprovaFlow.Filters
{
    public class FilterDefinition<T>
    {   
        public string Name { get; set; }        
        public string FilterCategory { get; set; }
        public Type FilterType { get; set; }
        public string TypeFullName { get; set; }
        
        [JsonIgnore()]
        public Func<FilterBase<T>> Filter{get; set;}

        public FilterDefinition() { }

        [OnError]
        internal void OnError(StreamingContext context, ErrorContext errorContext)
        {
            errorContext.Handled = true;
        }

    }
}
