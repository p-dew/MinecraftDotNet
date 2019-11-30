using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using MinecraftDotNet.Core.Math;

namespace MinecraftDotNet.Core
{
    public class Server
    {
        public Action<Player, Coordinates3, BlockInfo> BlockPlaced;
        
        public Server()
        {
            Clients = new List<Client>();
        }
        
        public IList<Client> Clients { get; }
    }
}