using System;
using System.Threading;
using MinecraftDotNet.Core;

namespace MinecraftDotNet
{
    class Program
    {
        static void Main(string[] args)
        {
            var game = new MinecraftGame();
            game.Start();
        }
    }
}