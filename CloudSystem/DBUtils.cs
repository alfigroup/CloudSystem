using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace CloudSystem
{
    class DBUtils
    {
        public static MySqlDataReader ExecuteCommand(MySqlConnection conn, String query) {
            MySqlCommand cmd = new MySqlCommand(query, conn);
            return cmd.ExecuteReader();
        }

        public static MySqlConnection GetDBConnection()
        {
            string text = File.ReadAllText("./mysql.login");
            string host = text.Split(':')[0];
            int port = 3306;
            string database = text.Split(':')[1]; 
            string username = text.Split(':')[2];
            string password = text.Split(':')[3];

            return GetDBConnection(host, port, database, username, password);
        }

        public static MySqlConnection GetDBConnection(string host, int port, string database, string username, string password)
        {
            // Connection String.
            String connString = "Server=" + host + ";Database=" + database  + ";port=" + port + ";User Id=" + username + ";password=" + password;

            MySqlConnection conn = new MySqlConnection(connString);

            return conn;
        }

    }
}
