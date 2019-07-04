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
        public int consoleServer = 1;

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
                    if (data.GetBoolean(6))
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
                        Console.WriteLine("[Cloud] Command on Server " + id);
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
                if (inputList[0].Equals(".console"))
                {
                    int id = int.Parse(inputList[1]);
                    if(servers.ContainsKey(id))
                    {
                        consoleServer = id;
                    }
                }
            }
            else if(inputList.Length == 1)
            {
                if (inputList[0].Equals(".list"))
                {
                    foreach(Server server in servers.Values)
                    {
                        Console.WriteLine("[Cloud] Server " + server.getID());
                    }
                } else
                {
                    Console.WriteLine("[Cloud] .cmd (ServerID) (Command)");
                    Console.WriteLine("[Cloud] .start (ServerID)");
                    Console.WriteLine("[Cloud] .stop (ServerID)");
                    Console.WriteLine("[Cloud] .console (ServerID)");
                    Console.WriteLine("[Cloud] .list");
                }
            }
            else
            {
                Console.WriteLine("[Cloud] Please use .help for a list of commands!");
            }
        }

        public void StartServer(int id)
        {
            if(servers.ContainsKey(id))
            {
                Console.WriteLine("[Server] Server already started");
                return;
            }
            try
            {
                MySqlDataReader server_data = DBUtils.ExecuteCommand(conn, "SELECT * FROM server WHERE id = '" + id + "'");
                if (server_data.Read())
                {
                    Console.WriteLine("[Server] Starting Server " + server_data.GetInt32(0));
                    String filename = server_data.GetString(3);
                    String path = server_data.GetString(4);
                    int game = server_data.GetInt32(5);
                    int ram = server_data.GetInt32(7);
                    double cpu = server_data.GetDouble(8);
                    String exec_user = server_data.GetString(9);
                    int port = server_data.GetInt32(10);
                    server_data.Close();

                    Console.WriteLine("[Server] Select game data");

                    MySqlDataReader game_data = DBUtils.ExecuteCommand(conn, "SELECT * FROM games WHERE id = '" + game + "'");
                    if(game_data.Read())
                    {
                        String startCommand = game_data.GetString(2);
                        String stopCommand = game_data.GetString(3);
                        game_data.Close();

                        startCommand = startCommand.Replace("%ram%", "" + ram);
                        startCommand = startCommand.Replace("%cpu%", "" + cpu);
                        startCommand = startCommand.Replace("%filename%", "" + filename);
                        startCommand = startCommand.Replace("%port%", "" + port);

                        var process = new Process
                        {
                            StartInfo = new ProcessStartInfo
                            {
                                FileName = "su",
                                Arguments = "-s /bin/bash " + exec_user + " -c " + '"' + startCommand + '"',
                                UseShellExecute = false, //Test
                                RedirectStandardInput = true,
                                RedirectStandardOutput = true,
                                RedirectStandardError = true,
                                CreateNoWindow = true,
                                WorkingDirectory = path,

                            }
                        };

                        DBUtils.ExecuteCommand(conn, "UPDATE server SET started = '1' WHERE id = '" + id + "'").Close();

                        // ProcessManager.ThrottleProcess(process.Id, 1);

                        Server server = new Server(process, id, path, stopCommand);
                        new Thread(() => server.Start()).Start();

                        servers.Add(id, server);
                    }
                    else
                    {
                        game_data.Close();
                        Console.WriteLine("[Server] Starting Server " + id + " failed!");
                    }
                }
                else
                {
                    server_data.Close();
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
                Console.WriteLine("[Server] Stopping server " + id);
                if(consoleServer == id)
                {
                    consoleServer = -1;
                }
                new Thread(() => servers[id].Stop()).Start();
                servers.Remove(id);

                DBUtils.ExecuteCommand(conn, "UPDATE server SET started = '0' WHERE id = '" + id + "'").Close();
            }
            else
            {
                Console.WriteLine("[Server] Server not running!" );
            }
        }
    }
}
