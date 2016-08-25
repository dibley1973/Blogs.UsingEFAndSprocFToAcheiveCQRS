using Dibware.Helpers.Validation;
using System;
using System.Data;
using System.Data.SqlClient;

namespace Blogs.EfAndSprocfForCqrs.ReadModel.Context
{

    public class ReadContext : IDisposable
    {
        private bool _disposed;

        private SqlConnection _connection;

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
            GC.SuppressFinalize(this);
        }

        ~ReadContext()
        {
            Dispose(false);
        }

        private void Dispose(bool disposing)
        {
            if (_disposed) return;

            if (disposing) CloseAndDisposeConnection();

            _disposed = true;
        }

        private void CloseAndDisposeConnection()
        {
            if (_connection == null) return;

            if (_connection.State != ConnectionState.Closed) _connection.Close();

            if (_connection != null)
            {
                _connection.Dispose();
                _connection = null;
            }
        }
    }
}