using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ApprovaFlow.Filters
{
    public interface IFilterChain<T>
    {
        void Execute(T input);
        IFilterChain<T> Register(IFilter<T> filter);
        IFilterChain<T> RegisterFromList(string filterNames, FilterRegistry<T> filterRegistry);
    }
}
