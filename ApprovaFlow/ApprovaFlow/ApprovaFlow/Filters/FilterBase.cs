using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ApprovaFlow.Filters
{
    public abstract class FilterBase<T> : IFilter<T>
    {
        private IFilter<T> next;
        
        protected abstract T Process(T input);

        public T Execute(T input)
        {
            T val = Process(input);

            if (this.next != null)
            {
                val = this.next.Execute(val);
            }

            return val;
        }

        public void Register(IFilter<T> filter)
        {
            if (this.next == null)
            {
                this.next = filter;
            }
            else
            {
                this.next.Register(filter);
            }
        }
    }
}
