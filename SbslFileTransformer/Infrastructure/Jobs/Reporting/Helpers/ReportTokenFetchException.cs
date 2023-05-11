using System;
using System.Runtime.Serialization;

namespace SbslFileTransformer.Infrastructure.Jobs.Reporting.Helpers
{
    [Serializable]
    internal class ReportTokenFetchException : Exception
    {
        public ReportTokenFetchException()
        {
        }

        public ReportTokenFetchException(string message) : base(message)
        {
        }

        public ReportTokenFetchException(string message, Exception innerException) : base(message, innerException)
        {
        }

        protected ReportTokenFetchException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}
