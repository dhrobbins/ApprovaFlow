using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Linq.Expressions;

namespace ApprovaFlow.Data
{
    public interface IRepository
    {
        T Get<T>(string Id);
        void Delete<T>(T deleteItem);
        void Save<T>(T saveItem);
        List<T> GetAllWhere<T>(string predicateString);
        List<T> GetAllWhere<T>(Expression<Func<T, bool>> predicate);
    }
}
