using System;
using System.Runtime.Serialization;

namespace GoSharp.Exceptions
{
    public class SaveDataCorruptedException : Exception
    {
        public SaveDataCorruptedException()
        {
        }

        public SaveDataCorruptedException(string message) : base(message)
        {
        }

        public SaveDataCorruptedException(string message, Exception innerException) : base(message, innerException)
        {
        }

        protected SaveDataCorruptedException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}