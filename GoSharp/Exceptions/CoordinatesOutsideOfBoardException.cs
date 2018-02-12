using System;
using System.Runtime.Serialization;

namespace GoSharp.Exceptions
{
    public class CoordinatesOutsideOfBoardException : Exception
    {
        public CoordinatesOutsideOfBoardException()
        {
        }

        public CoordinatesOutsideOfBoardException(string message) : base(message)
        {
        }

        public CoordinatesOutsideOfBoardException(string message, Exception innerException) : base(
            message, innerException)
        {
        }

        protected CoordinatesOutsideOfBoardException(SerializationInfo info, StreamingContext context) : base(
            info, context)
        {
        }
    }
}