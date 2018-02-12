using System;
using System.Runtime.Serialization;

namespace GoSharp.Exceptions
{
    public class CoordinatesAreNullException : Exception
    {
        public CoordinatesAreNullException()
        {
        }

        public CoordinatesAreNullException(string message) : base(message)
        {
        }

        public CoordinatesAreNullException(string message, Exception innerException) : base(message, innerException)
        {
        }

        protected CoordinatesAreNullException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}