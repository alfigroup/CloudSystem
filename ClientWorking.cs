using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace CloudSystem
{
    class ClientWorking
    {
        private Stream ClientStream;
        private TcpClient Client;

        public ClientWorking(TcpClient Client)
        {
            this.Client = Client;
            ClientStream = Client.GetStream();
        }
        
        public void HandleClient()
        {
            StreamWriter sw = new StreamWriter(ClientStream);
            StreamReader sr = new StreamReader(sw.BaseStream);

            string data;
            try
            {
                while ((data = sr.ReadLine()) != "exit" && data != null)
                {
                    Console.WriteLine("[Socket] " + data);
                    Program.getSystem().RunCommand(data);
                    //sw.WriteLine();
                    //sw.Flush();
                }
            }
            finally
            {
                sw.Close();
            }
        }
    }
}
