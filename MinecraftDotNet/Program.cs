using System;
using MinecraftDotNet.ClientSide;
using ObjectTK.Exceptions;

namespace MinecraftDotNet
{
    class Program
    {
        private static void Main(string[] args)
        {
            Console.WriteLine("Minecraft .NET Edition | 0.0.0-indev");

            try
            {
                var client = new StandaloneClient();
                client.Run();
            }
            catch (ShaderCompileException e)
            {
                Console.WriteLine(e.InfoLog);
                throw;
            }
        }
    }
}