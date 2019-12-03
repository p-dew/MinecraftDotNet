using System.Threading;
using System.Threading.Tasks;
using MinecraftDotNet.Core;
using OpenTK;

namespace MinecraftDotNet.ClientSide
{
    public class Client : IClient
    {
        public Client()
        {
            
        }
        
        public void Start(CancellationToken cancellationToken = new CancellationToken())
        {
            var renderTask = Task.Run(() =>
            {
                var window = (GameWindow) new MinecraftGameWindow("MinecraftDotNet", 1024, 720);
                window.Run();
            }, cancellationToken);
            
            renderTask.Wait(cancellationToken);
        }

        public void ConnectTo(IServer server)
        {
            // server.Clients.Add(this);
        }
    }
}