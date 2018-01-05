using ConsoleProgram.Protocol.Handler;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleProgram.Protocol
{
    public static class ProtocolHandler
    {
        public static bool GetServerInfo()
        {
            if (Protocol17.PingServer("mc.strictlyvanilla.org", 25565))
            {
                return true;
            }

            return false;
        }
    }
}
