using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.IO;

namespace CloudSystem
{
    class Program
    {
        // Just the local system object to return or access over
        private static System system;

        static void Main(string[] args)
        {
            system = new System();
            new Thread(new ThreadStart(new Program().InputHandler)).Start(); //Start Console Handler
        }

        // returs the System object
        public static System getSystem()
        {
            return system;
        }

        // Handles console Input
        public void InputHandler()
        {
            while (true)
            {   
                // Reads Line
                string val = Console.ReadLine();
                // If the first Parameter starts with a dot it is a CloudCommand
                if(!val.StartsWith("."))
                {
                    Program.getSystem().servers[Program.getSystem().consoleServer].Input(val);
                }
                // else the command will be passed through to the programm
                else
                {
                    Program.getSystem().RunCommand(val);
                }
            }
        }
    }

    // NEW CLASS SocketHandler
    // Socket interface to send commands to
    public class SocketHandler
    {
        public void Handler()
        {
            // Connection Data
            int port = 6536;
            string ip = "127.0.0.1";
            // New TCP TcpListener / Socket / open Port / Server
            TcpListener server = new TcpListener(IPAddress.Parse(ip), port);

            // start TcpListener
            server.Start();
            Console.WriteLine("[Socket] Server has started on " + ip + ":"+ port +".");

            while (true)
            {
                ClientWorking cw = new ClientWorking(server.AcceptTcpClient());
                new Thread(new ThreadStart(cw.HandleClient)).Start();
            }
        }
    }

    // NEW CLASS: Server
    // The class for a server process of the cloud
    public class Server
    {
        // Process Variables and logFile Writer
        private Process process;
        private int id;
        private StreamWriter logFile;
        private string path;

        // SUGGESTION by Checker8763 (Block comment!)
        /*
         *  // Example #1: Write an array of strings to a file.
         *   string[] lines = { "First line", "Second line", "Third line" };
         *   System.IO.File.WriteAllLines(@"C:\Users\Public\TestFolder\WriteLines.txt", lines);
         *   
         *  // Example #2: Write one string to a text file.
         *   string text = "A class is the most powerful data type in C#. Like a structure, " +
         *      "a class defines the data and behavior of the data type. ";
         * 
         */

        public Server(Process process, int id, string path)
        {
            this.process = process;
            this.id = id;
            this.path = path;
            if (!File.Exists(path + "/server.log"))
            {
                var fs = new FileStream(path + "/server.log", FileMode.Create);
                fs.Dispose();
            }
            //logFile = new StreamWriter(path + "/server.log", true);
        }

        public void Start()
        {
            process.Start();
            process.BeginOutputReadLine();
            process.BeginErrorReadLine();

            process.OutputDataReceived += new DataReceivedEventHandler(OutputHandler);
            process.ErrorDataReceived += new DataReceivedEventHandler(OutputHandler);
            process.Exited += new EventHandler(ExitHandler);
            process.WaitForExit();
        }

        public void Stop()
        {
            if(!process.HasExited)
            {
                process.Kill();
            }
        }

        public void Input(string line)
        {
            process.StandardInput.WriteLine(line);
        }

        public void OutputHandler(object sendingProcess, DataReceivedEventArgs outLine)
        {
            var line = outLine.Data;
            if (Program.getSystem().consoleServer == id)
            {
                Console.WriteLine("[Server] [" + id + "] " + line);
            }
            //logFile.WriteLineAsync(line);
        }

        public void ExitHandler(object sendingProcess, EventArgs outLine)
        {
            Console.WriteLine("[Server] [" + id + "] Server exited");
            Program.getSystem().StopServer(id);
        }
    }
}
