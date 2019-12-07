using System;
using System.IO;
using MinecraftDotNet.ClientSide;
using ObjectTK.Exceptions;
using ObjectTK;
using ObjectTK.Shaders;

namespace MinecraftDotNet
{
    class Program
    {
        private static void Main(string[] args)
        {
            Console.WriteLine("Minecraft .NET Edition | 0.0.0-indev");
//            ProgramFactory.BasePath =
//                "/home/vlad/Документы/Проекты/Rider/minecraftdotnet/MinecraftDotNet.ClientSide/Data/Shaders";
//            Directory.SetCurrentDirectory("/home/vlad/Документы/Проекты/Rider/minecraftdotnet");
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
            Console.WriteLine("Minecraft .NET Edition | 0.0.0-indev");
        }
    }
}