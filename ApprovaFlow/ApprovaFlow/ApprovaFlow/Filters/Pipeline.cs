using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ApprovaFlow.Filters
{
    public class Pipeline<T> : IFilterChain<T>
    {
        private IFilter<T> root;
        private int count;
        private List<string> filters;

        public Pipeline()
        {
            this.count = 0;
            this.filters = new List<string>();
        }
        
        public void Execute(T input)
        {
            this.root.Execute(input);
        }

        public IFilterChain<T> Register(IFilter<T> filter)
        {
            if (this.root == null)
            {
                root = filter;
            }
            else
            {
                root.Register(filter);
            }

            this.count++;
            this.filters.Add(filter.GetType().ToString());

            return this;
        }

        public IFilterChain<T> RegisterFromList(string filterNames, FilterRegistry<T> filterRegistry)
        {
            var filters = filterRegistry.GetFilters(filterNames).ToList();
            filters.ForEach(filter => this.Register(filter));

            return this;
        }

        public int GetCount()
        {
            return this.count;
        }

        public List<string> GetNames()
        {
            return this.filters;
        }
    }
}
