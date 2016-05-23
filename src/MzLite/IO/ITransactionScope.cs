using System;

namespace MzLite.IO
{
    public interface ITransactionScope : IDisposable
    {
        void Commit();
        void Rollback();
    }
}
