using MySql.Data.MySqlClient;

namespace CodeLab.Database
{
    public class DatabaseConnection
    {
        private readonly string connectionString = "server=localhost; user=root; password=root!@#$; database=dbCodeLab";

        public MySqlConnection GetConnection()
        {
            MySqlConnection conn = new MySqlConnection(connectionString);
            conn.Open();
            return conn;
        }

    }
}
