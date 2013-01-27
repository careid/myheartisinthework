using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.IO;
using System.Threading;

namespace HeartGame
{
    public class CommandQueue
    {
        Queue<string> commands;
        public CommandQueue()
        {
            commands = new Queue<string>();
        }

        public string Read()
        {
            lock (this)
            {
                if (commands.Count > 0)
                {
                    return commands.Dequeue();
                }
                else
                {
                    return "";
                }
            }
        }
        public void Write(string inp)
        {
            lock (this)
            {
                commands.Enqueue(inp);
            }
        }
    }

    public class Client
    {
        static System.Net.Sockets.TcpClient TC;
        protected StreamWriter SW;
        protected CommandQueue CQ;

        public Client()
        {
            
        }

        public string Connect()
        {
            // Networking shit
            TC = new System.Net.Sockets.TcpClient();
            TC.Connect("172.24.8.194", 3000);

            SW = new StreamWriter(TC.GetStream());
            //request dwarf count from server
            //SW.WriteLine("add dwarf");
            //SW.Flush();
            StreamReader SR = new StreamReader(TC.GetStream());
            string name = SR.ReadLine();
            CQ = new CommandQueue();
            Thread t = new Thread(new ParameterizedThreadStart(runListener));
            t.Start(CQ);
            return name;
        }

        static void runListener(object Obj)
        {
            CommandQueue CQ = (CommandQueue)Obj;
            StreamReader SR = new StreamReader(TC.GetStream());
            while (true)
            {
                string line = SR.ReadLine();
                CQ.Write(line);
            }
        }

        public string Read()
        {
            return CQ.Read();
        }

        public void Write(string msg)
        {
            CQ.Write(msg);
        }
    }


}
