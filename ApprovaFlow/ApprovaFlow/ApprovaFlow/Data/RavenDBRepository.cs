using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Raven.Client.Document;
using Raven.Client.Authorization;
using ApprovaFlow.Utils;


namespace ApprovaFlow.Data
{
    public class RavenDBRepository : IRepository
    {
        private DocumentStore documentStore;
        private DocumentSession session;

        public string UserId { get; set; }
        public string Operation { get; set; }

        public RavenDBRepository(DocumentStore docStore)
        {
            this.documentStore = docStore;
            this.session = (DocumentSession)this.documentStore.OpenSession();

            this.UserId = string.Empty;
            this.Operation = string.Empty;
        }

        public RavenDBRepository(DocumentStore docStore, string userId, string operation)
        {
            this.documentStore = docStore;
            this.session = (DocumentSession)this.documentStore.OpenSession();

            this.UserId = userId;
            this.Operation = operation;
        }
        
        public T Get<T>(string Id)
        {
            return this.session.Load<T>(Id);
        }

        public void Delete<T>(T deleteItem)
        {
            this.session.Delete<T>(deleteItem);            
        }

        public void Save<T>(T saveItem)
        {
            this.session.Store(saveItem);
            this.session.SaveChanges();
        }

        public List<T> GetAll<T>()
        {
            var items = this.session.Query<T>().ToList();
            return items;
        }

        public List<T> GetAllWhere<T>(string predicateString)
        {
            var predicate = new PredicateConstructor<T>();
            return GetAllWhere(predicate.CompileToExpression(predicateString));
        }

        public List<T> GetAllWhere<T>(System.Linq.Expressions.Expression<Func<T, bool>> predicate)
        {
            var items = this.session.Query<T>()
                                    .Customize(x => x.WaitForNonStaleResults())
                                    .Where<T>(predicate)
                                    .ToList();
            return items;
        }

        public RavenDBRepository SecureForOperation(string userId, string operation)
        {
            this.UserId = userId;
            this.Operation = operation;

            return SecureForOperation();
        }

        public RavenDBRepository SecureForOperation()
        {
            this.session = (DocumentSession)this.documentStore.OpenSession();
            this.session.SecureFor(this.UserId, this.Operation);
            this.session.SaveChanges();
            return this;
        }
    }
}
