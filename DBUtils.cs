using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
            string host = "alfigroup.eu";
            int port = 3306;
            string database = "videria"; 
            string username = "videria";
            string password = "I05961p8ATEl72gS";

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
