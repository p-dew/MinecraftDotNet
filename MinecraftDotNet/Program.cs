using System;
using System.Threading;
using MinecraftDotNet.Core;

namespace MinecraftDotNet
{
    class Program
    {
        static void Main(string[] args)
        {
            var server = new Server();
            
            var client = new Client();
            client.ConnectTo(server);
            
            client.Start();
        }
    }
}