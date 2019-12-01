using System;
using System.Threading;
using MinecraftDotNet.Core;
using MinecraftDotNet.Core.Mca;
using System.IO.Compression;
using System.IO;

namespace MinecraftDotNet
{
    class Program
    {
        static void Main(string[] args)
        {



            var server = new Server();
            
            var client = new Client();
            client.ConnectTo(server);
            
            Console.WriteLine("Minecraft.NET 0.0.0-indev");
            
            McaChunkProvider provider = new McaChunkProvider("/home/vlad/Рабочий стол/New World/region/");

            Console.WriteLine(provider.GetChunk(new ChunkCoords(2, 0)));
            
            client.Start();

            

        }
    }
}