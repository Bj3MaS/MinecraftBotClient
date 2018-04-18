using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleProgram.Protocol.Handler
{
    class Protocol17
    {
        TcpClient _client;
        private bool _encrypted = false;
      

        public static void WriteConsole()
        {
            Console.WriteLine("Test");
            Console.ReadLine();
        }

        public Protocol17( TcpClient tcp)
        {
            this._client = tcp;
        }
        
        public static bool PingServer(string host, int port)
        {

            string version = "";
            TcpClient tcp = new TcpClient("mc.strictlyvanilla.org", 25565);
            tcp.ReceiveBufferSize = 1024 * 1024;

            byte[] packet_id = GetVarInt(0);
            byte[] protocol_version = GetVarInt(340);
            byte[] server_adress_val = Encoding.UTF8.GetBytes("localhost");
            byte[] server_adress_len = GetVarInt(server_adress_val.Length);
            byte[] server_port = BitConverter.GetBytes((ushort)25565); Array.Reverse(server_port);
            byte[] next_state = GetVarInt(1);
            byte[] packet = concatBytes(packet_id, protocol_version, server_adress_len, server_adress_val, server_port, next_state);
            byte[] tosend = concatBytes(GetVarInt(packet.Length), packet);

            tcp.Client.Send(tosend, SocketFlags.None);

            byte[] status_request = GetVarInt(0);
            byte[] request_packet = concatBytes(GetVarInt(status_request.Length), status_request);

            tcp.Client.Send(request_packet, SocketFlags.None);

            Protocol17 TcpTemp = new Protocol17(tcp);

            int packetLength = TcpTemp.readNextVarIntRAW();

            List<byte> packetData = new List<byte>(TcpTemp.readDataRAW(packetLength));
            if (readNextVarInt(packetData) == 0x00)
            {
                string result = readNextString(packetData);
                Console.WriteLine(result);
                return true;
                var test = 0;
            }

            return false;
            
        }

        private static byte[] GetVarInt(int paramInt)
        {
            List<byte> bytes = new List<byte>();
            while ((paramInt & -128) != 0)
            {
                bytes.Add((byte)(paramInt & 127 | 128));
                paramInt = (int)(((uint)paramInt) >> 7);
            }
            bytes.Add((byte)paramInt);
            return bytes.ToArray();
        }

        private static byte[] concatBytes(params byte[][] bytes)
        {
            List<byte> result = new List<byte>(); 
            foreach (byte[] array in bytes)
                result.AddRange(array);
            return result.ToArray();
        }

        private int readNextVarIntRAW()
        {
            int i = 0;
            int j = 0;
            int k = 0;
            byte[] tmp = new byte[1];
            while (true)
            {
                Receive(tmp, 0, 1, SocketFlags.None);
                k = tmp[0];
                i |= (k & 0x7F) << j++ * 7;
                if (j > 5) throw new OverflowException("VarInt too big");
                if ((k & 0x80) != 128) break;
            }
            return i;
        }

        private void Receive(byte[] buffer, int start, int offset, SocketFlags f)
        {
            int read = 0;
            while (read < offset)
            {
                if (_encrypted)
                {
                    //read += s.Read(buffer, start + read, offset - read);
                }
                else read += _client.Client.Receive(buffer, start + read, offset - read, f);
                //Console.WriteLine(read);
            }
        }

        private byte[] readDataRAW(int offset)
        {
            if (offset > 0)
            {
                try
                {
                    byte[] cache = new byte[offset];
                    Receive(cache, 0, offset, SocketFlags.None);
                    return cache;
                }
                catch (OutOfMemoryException) { }
            }
            return new byte[] { };
        }

        private static int readNextVarInt(List<byte> cache)
        {
            int i = 0;
            int j = 0;
            int k = 0;
            while (true)
            {
                k = readNextByte(cache);
                i |= (k & 0x7F) << j++ * 7;
                if (j > 5) throw new OverflowException("VarInt too big");
                if ((k & 0x80) != 128) break;
            }
            return i;
        }

        private static byte readNextByte(List<byte> cache)
        {
            byte result = cache[0];
            cache.RemoveAt(0);
            return result;
        }

        private static byte[] readData(int offset, List<byte> cache)
        {
            byte[] result = cache.Take(offset).ToArray();
            cache.RemoveRange(0, offset);
            return result;
        }

        private static string readNextString(List<byte> cache)
        {
            int length = readNextVarInt(cache);
            if (length > 0)
            {
                return Encoding.UTF8.GetString(readData(length, cache));
            }
            else return "";
        }
    }
}
