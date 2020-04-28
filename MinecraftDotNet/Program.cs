using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Console;
using Microsoft.Extensions.Options;
using MinecraftDotNet.ClientSide;
using MinecraftDotNet.ClientSide.Graphics;
using MinecraftDotNet.ClientSide.Graphics.Core;
using MinecraftDotNet.Core;
using MinecraftDotNet.Core.Blocks;
using MinecraftDotNet.Core.Blocks.Chunks;
using MinecraftDotNet.Core.Blocks.Chunks.Regions.Mca;
using MinecraftDotNet.Core.Worlds;
using MinecraftDotNet.ServerSide;
using ObjectTK.Exceptions;

namespace MinecraftDotNet
{
    class Program
    {
        private static IServerApplication BuildServerApp(ILoggerFactory loggerFactory)
        {
            var chunkRepository = new RegionChunkRepository(
                new McaChunkPacker(), 
                new ChessChunkGenerator(c => HcBlocks.Dirt),
                new McaRegionRepository(
                    "./saves/test_save/DIM1/region/", () => new DictRegionBuilder()));
            var blockRepository = new ChunkBlockRepository(chunkRepository);
            var world =
                new WorldBuilder()
                    .UseBlockRepository(() => blockRepository)
                    .UseChunkRepository(() => chunkRepository)
                    .Build();
            
            return new ServerApplication(world);
        }

        private static IClientApplication BuildClientApp(ILoggerFactory loggerFactory)
        {
            return new ClientApplication(
                new ResourceRepository(), 
                loggerFactory.CreateLogger<ClientApplication>(),
                new RenderersProvider(
                    new SingleBlockChunkRenderer(loggerFactory.CreateLogger<SingleBlockChunkRenderer>())),
                () => BuildServerApp(loggerFactory));
        }

        private static ILoggerFactory CreateLoggerFactory()
        {
            var loggerFactory = new LoggerFactory();
            loggerFactory.AddProvider(
                new ConsoleLoggerProvider(
                    new OptionsMonitor<ConsoleLoggerOptions>(
                        new OptionsFactory<ConsoleLoggerOptions>(
                            new List<IConfigureOptions<ConsoleLoggerOptions>>() { },
                            new List<IPostConfigureOptions<ConsoleLoggerOptions>>() { }),
                        new List<IOptionsChangeTokenSource<ConsoleLoggerOptions>>() { },
                        new OptionsCache<ConsoleLoggerOptions>())));
            return loggerFactory;
        }
        
        private static void Main(string[] args)
        {
            var loggerFactory = CreateLoggerFactory();

            Console.WriteLine("Minecraft .NET Edition | 0.0.0-indev");
            
            if (args.Contains("--server"))
            {
                var server = BuildServerApp(loggerFactory);
                server.Run();
            }
            else
            {
                var client = BuildClientApp(loggerFactory);
                client.Run();
            }
            
            Console.WriteLine("Minecraft .NET Edition | Process stopped");
        }
    }
}