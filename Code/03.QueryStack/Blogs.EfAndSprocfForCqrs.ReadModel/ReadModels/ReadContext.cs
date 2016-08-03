using Dibware.Helpers.Validation;
using System;
using System.Data;
using System.Data.SqlClient;

namespace Blogs.EfAndSprocfForCqrs.ReadModel.ReadModels
{

    public class ReadContext : IDisposable
    {
        private bool _disposed;

        private readonly SqlConnection _connection;

        public ReadContext(string connectionString)
        {
            Guard.ArgumentIsNotNullOrEmpty(connectionString, "connectionString");

            _connection = new SqlConnection(connectionString);
        }

        internal SqlConnection Connection
        {
            get { return _connection; }
        }

        public void Dispose()
        {
            Dispose(true);
        }

        ~ReadContext()
        {
            Dispose(false);
            GC.SuppressFinalize(this);
        }

        private void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    CloseAndDisposeConnection();
                }

                _disposed = true;
            }
        }

        private void CloseAndDisposeConnection()
        {
            if (_connection != null)
            {
                if (_connection.State != ConnectionState.Closed) _connection.Close();

                if (_connection != null)
                {
                    _connection.Dispose();
                }
            }
        }
    }
}