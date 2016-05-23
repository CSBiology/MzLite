using System;
using MzLite.Model;

namespace MzLite.IO
{
    public interface IMzLiteIO : IDisposable
    {
        MzLiteModel GetModel();
        void SaveModel();        
        ITransactionScope BeginTransaction();
    }
}
