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
        private static System system;

        static void Main(string[] args)
        {
            system = new System();
            new Thread(new ThreadStart(new Program().InputHandler)).Start(); //Start Console Handler
        }

        public static System getSystem()
        {
            return system;
        }

        public void InputHandler()
        {
            while (true)
            {
                string val = Console.ReadLine();
                Program.getSystem().RunCommand(val);
            }
        }
    }
    
    public class SocketHandler
    {
        public void Handler()
        {
            TcpListener server = new TcpListener(IPAddress.Parse("127.0.0.1"), 6536);

            server.Start();
            Console.WriteLine("[Socket] Server has started on 127.0.0.1:6536.");

            while (true)
            {
                ClientWorking cw = new ClientWorking(server.AcceptTcpClient());
                new Thread(new ThreadStart(cw.HandleClient)).Start();
            }
        }
    }

    public class Server
    {
        private Process process;
        private int id;
        private StreamWriter logFile;
        private string path;

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
            Console.WriteLine("[Server] " + line);
            //logFile.WriteLineAsync(line);
        }

        public void ExitHandler(object sendingProcess, EventArgs outLine)
        {
            Console.WriteLine("[Server] Server exited");
            Program.getSystem().StopServer(id);
        }
    }
}
