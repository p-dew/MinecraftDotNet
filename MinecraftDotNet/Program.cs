using System;
using System.IO;
using MinecraftDotNet.ClientSide;
using MinecraftDotNet.Core;
using MinecraftDotNet.Core.Blocks;
using MinecraftDotNet.Core.Blocks.Chunks;
using MinecraftDotNet.Core.Worlds;
using MinecraftDotNet.ServerSide;
using ObjectTK.Exceptions;

namespace MinecraftDotNet
{
    class Program
    {
        private static IServer BuildServer()
        {
            var chunkRepository = new MemoryChunkRepository(new ChessChunkGenerator(c => HcBlocks.Dirt));
            var blockRepository = new ChunkBlockRepository(chunkRepository);
            var world =
                new WorldBuilder()
                    .UseBlockRepository(() => blockRepository)
                    .UseChunkRepository(() => chunkRepository)
                    .Build();
            
            return new Server(world);
        }
        
        private static void Main(string[] args)
        {
            Console.WriteLine(Directory.GetCurrentDirectory());
            Console.WriteLine("Minecraft .NET Edition | 0.0.0-indev");
        //    ProgramFactory.BasePath =
        //        "/home/vlad/Документы/Проекты/Rider/minecraftdotnet/MinecraftDotNet.ClientSide/Data/Shaders";
        //    Directory.SetCurrentDirectory("/home/vlad/Документы/Проекты/Rider/minecraftdotnet");
            try
            {
                var server = BuildServer();
                
                var client = new StandaloneClient(server);
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