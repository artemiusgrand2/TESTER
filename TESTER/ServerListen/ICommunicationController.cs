using System;

namespace TESTER.ServerListen
{
    public delegate void ErrorHandler<TSender, TValue>(TSender sender, TValue value);

    public interface ICommunicationController : IDisposable
    {

        event ErrorHandler<ICommunicationController, Exception> OnError;

        void Start();
        void Stop();
    }
}
