using System;
using RabbitMQ.Client;

namespace ERC.RabbitMQ
{
    public class RabbitConnection : ConnectionFactory, IDisposable
    {
        protected bool IsDisposed = false;

        protected IConnection Connection { get; private set; }
        protected IModel Model { get; private set; }

        public RabbitConnection(string hostName, string virtualHost, string userName, string password)
        {
            HostName = hostName;
            UserName = userName;
            Password = password;
            VirtualHost = virtualHost;
        }

        public virtual void Connect()
        {
            Connection = CreateConnection();
            Model = Connection.CreateModel();
        }

        #region IDisposable

        public virtual void Dispose()
        {
            if (IsDisposed)
                return;

            Model.Dispose();
            Connection.Dispose();

            IsDisposed = true;
        }

        #endregion 
    }
}
