using System;
using System.Data.SqlClient;
using System.Data;


namespace Clr.Shared
{
    public class SqlHelper : IDisposable
    {
        private SqlConnection _connection;

        public SqlHelper(string ConnectionString) : this(new SqlConnection(ConnectionString)) { }

        public SqlHelper(SqlConnection Connection)
        {
            _connection = Connection;
        }

        public void Dispose()
        {
            if (_connection.State == ConnectionState.Open)
            {
                try
                {
                    _connection.Close();
                }
                catch
                {

                }
            }
        }

        public string CheckConnection()
        {
            try
            {
                _connection.Open();
                _connection.Close();
                return "";
            }
            catch (Exception ex)
            {
                return ex.Message;
            }
        }

        public int ExecuteNonQuery(string queryString, CommandType commandType = CommandType.Text)
        {
            using (var command = new SqlCommand(queryString, _connection))
            {
                command.CommandType = commandType;
                command.Connection.Open();
                var i = command.ExecuteNonQuery();
                command.Connection.Close();
                return i;
            }
        }

        public SqlDataReader ExecuteReader(string queryString, CommandType commandType = CommandType.Text)
        {
            using (var command = new SqlCommand(queryString, _connection))
            {
                command.CommandType = commandType;
                command.Connection.Open();
                var i = command.ExecuteReader();
                command.Connection.Close();
                return i;

            }
        }

        public DataTable GetDataTable(string queryString, CommandType commandType = CommandType.Text)
        {
            var dataTable = new DataTable();

            using (var command = new SqlCommand(queryString, _connection))
            {
                command.CommandType = commandType;
                command.Connection.Open();

                using (var dataAdapter = new SqlDataAdapter(command))
                {
                    dataAdapter.Fill(dataTable);
                }
                command.Connection.Close();
                return dataTable;
            }
        }


    }
}
