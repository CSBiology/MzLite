using System;
using MzLite.Model;

namespace MzLite.IO
{
    public interface IMzLiteIO : IDisposable
    {
        MzLiteModel Model { get; }
        void SaveModel();        
        ITransactionScope BeginTransaction();
    }
}
