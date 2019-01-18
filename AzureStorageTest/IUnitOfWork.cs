using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using ASC.DataAccess;

namespace ASC.DataAccess.Base
{
    public interface IUnitOfWork : IDisposable
    {
        Queue<Task<Action>> RollbackActions { get; set; }
        string ConnectionString { get; set; }
        IRepository<T> Repository<T>() where T : BaseEntity;
        void CommitTransaction();
    }
}
