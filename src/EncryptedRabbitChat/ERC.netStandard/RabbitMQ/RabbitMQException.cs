using System;
using System.Collections.Generic;
using System.Text;

namespace ERC.RabbitMQ
{
    public class RabbitMQException : Exception
    {
        public enum ExceptionType
        {
            /// <summary>Thrown if the client queue is not available</summary>
            NoClientQueue,

            /// <summary>Thrown if the server queue is not available</summary>
            NoServerQueue,

            /// <summary>Thrown if the client does not receive server handshake data</summary>
            NoServerHandshakeData,
        }

        public RabbitMQException(ExceptionType exceptionType)
        {
            Type = exceptionType;
        }

        public ExceptionType Type { get; }
    }
}
