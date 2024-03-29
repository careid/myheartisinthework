﻿using System;
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
        public bool online;
        public Thread t;
        public static bool shouldExit = false;

        public Client(bool online)
        {
            this.online = online;
        }

        public string Connect()
        {
            CQ = new CommandQueue();
            if (this.online)
            {
                // Networking shit
                try
                {
                    TC = new System.Net.Sockets.TcpClient();
                    //TC.Connect("169.254.19.75", 3000);
                    IAsyncResult result = TC.BeginConnect("169.254.67.42", 3000, null, null);

                    if (!result.AsyncWaitHandle.WaitOne(8000, true)) // 4 sec timout
                    {
                        TC.Close();
                        throw new Exception();
                    }

                    SW = new StreamWriter(TC.GetStream());
                    //request dwarf count from server
                    //SW.WriteLine("add dwarf");
                    //SW.Flush();
                    StreamReader SR = new StreamReader(TC.GetStream());
                    string name = SR.ReadLine();
                    t = new Thread(new ParameterizedThreadStart(runListener));
                    t.Start(CQ);
                    return name;
                }
                catch (Exception e)
                {
                    this.online = false;
                }
                return "error";
            }
            else
            {
                return "0";
            }
        }

        static void runListener(object Obj)
        {
            CommandQueue CQ = (CommandQueue)Obj;
            StreamReader SR = new StreamReader(TC.GetStream());
            while (!shouldExit)
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
            if (online)
            {
                SW.WriteLine(msg);
                SW.Flush();
            }
            else
            {
                CQ.Write(msg);
            }
        }
    }


}
