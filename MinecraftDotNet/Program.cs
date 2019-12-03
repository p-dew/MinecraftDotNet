using System;
using System.Threading;
using MinecraftDotNet.Core;
using System.IO.Compression;
using System.IO;
using MinecraftDotNet.Core.Blocks.Chunks;

namespace MinecraftDotNet
{
    class Program
    {
        private static void Main(string[] args)
        {
            var server = new Server();
            
            var client = new Client();
            client.ConnectTo(server);
            
            Console.WriteLine("Minecraft .NET Edition | 0.0.0-indev");
            
            client.Start();
        }
    }
}