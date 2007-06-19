using System;
namespace System.Core.Logging
{
    public interface ILogger
    {
        void Debug(string message, Exception exception);
        void Debug(string message);
        void Error(string message, Exception exception);
        void Error(string message);
        void Fatal(string message, Exception exception);
        void Fatal(string message);
        LogSink GetSink(string name);
        void Info(string message);
        void Info(string message, Exception exception);
        void Warning(string message, Exception exception);
        void Warning(string message);
    }
}
