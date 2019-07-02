using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace CloudSystem
{
    class System
    {

        public MySqlConnection conn;
        public Dictionary<int, Server> servers = new Dictionary<int, Server>();

        public System()
        {
            conn = DBUtils.GetDBConnection();
            try
            {
                conn.Open();
                Console.WriteLine("[MySQL] Verbindung erfolgreich!");
            }
            catch (Exception e)
            {
                Console.WriteLine("[MySQL] Verbindung fehlgeschlagen!");
                Console.WriteLine(e.Message);
            }
            try
            {
                Thread Socket = new Thread(new ThreadStart(new SocketHandler().Handler)); //Start Console Handler
                Socket.Start();

                List<int> startServer = new List<int>();
                MySqlDataReader data = DBUtils.ExecuteCommand(conn, "SELECT * FROM `server`");
                while (data.Read())
                {
                    int id = data.GetInt32(0);
                    if (data.GetBoolean(7))
                    {
                        startServer.Add(id);
                        //new Thread(() => StartServer(id)).Start();
                    }
                }
                data.Close();
                foreach(int id in startServer)
                {
                    StartServer(id);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
        }

        public void RunCommand(String input)
        {
            String[] inputList = input.Split(' ');
            if (inputList.Length > 1)
            {
                if (inputList[0].Equals(".cmd"))
                {
                    int id = int.Parse(inputList[1]);
                    string command = "";

                    if (inputList.Length > 2) { 
                        for (int i = 2; i < inputList.Length; i++)
                        {
                            if (i == 2)
                            {
                                command = inputList[i];
                            }
                            else
                            {
                                command = command + " " + inputList[i];
                            }
                        }
                    }
                    if (servers.ContainsKey(id))
                    {
                        Console.WriteLine("[Server] Command on Server " + id);
                        servers[id].Input(command);
                    }
                }
                if (inputList[0].Equals(".start"))
                {
                    int id = int.Parse(inputList[1]);
                    StartServer(id);
                }
                if (inputList[0].Equals(".stop"))
                {
                    int id = int.Parse(inputList[1]);
                    StopServer(id);
                }
            }
            else if(inputList.Length == 1)
            {
                Console.WriteLine("[Server] .cmd (ServerID) (Command)");
                Console.WriteLine("[Server] .start (ServerID)");
                Console.WriteLine("[Server] .stop (ServerID)");
            }
            else
            {
                Console.WriteLine("[Server] Please use .help for a list of commands!");
            }
        }

        public void StartServer(int id)
        {
            try
            {
                MySqlDataReader data = DBUtils.ExecuteCommand(conn, "SELECT * FROM server WHERE id = '" + id + "'");
                if (data.Read())
                {
                    Console.WriteLine("[Server] Starting Server " + data.GetInt32(0));
                    String filename = data.GetString(3);
                    String arguments = data.GetString(4);
                    String path = data.GetString(5);
                    data.Close();

                    var process = new Process
                    {
                        StartInfo = new ProcessStartInfo
                        {
                            FileName = filename,
                            Arguments = arguments,
                            UseShellExecute = false,
                            RedirectStandardInput = true,
                            RedirectStandardOutput = true,
                            RedirectStandardError = true,
                            CreateNoWindow = true,
                            WorkingDirectory = path,

                        }
                    };

                    DBUtils.ExecuteCommand(conn, "UPDATE server SET started = '1' WHERE id = '" + id + "'").Close();

                    // ProcessManager.ThrottleProcess(process.Id, 1);

                    Server server = new Server(process, id, path);
                    new Thread(() => server.Start()).Start();

                    servers.Add(id, server);
                }
                else
                {
                    Console.WriteLine("[Server] Starting Server " + id + " failed ! (Server not found)");
                }
            } catch (Exception e)
            {
                Console.WriteLine("[Server] Starting Server " + id + " failed!");
                Console.WriteLine(e.Message);
                StopServer(id);
            }
        }

        public void StopServer(int id)
        {
            if (servers.ContainsKey(id))
            {
                servers[id].Stop();
                servers.Remove(id);

                DBUtils.ExecuteCommand(conn, "UPDATE server SET started = '0' WHERE id = '" + id + "'").Close();
            }
        }
    }
}
