using ConsoleProgram.Protocol;
using ConsoleProgram.Protocol.Handler;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleProgram
{
    class Program
    {
        static void Main(string[] args)
        {

            Console.WriteLine("Getting server info");
            if (!ProtocolHandler.GetServerInfo())
            {
                Console.WriteLine("Cannot ping server");
            }
          
        }
    }
}
