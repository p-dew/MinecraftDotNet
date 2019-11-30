using System.Threading;
using System.Threading.Tasks;
using OpenTK;

namespace MinecraftDotNet.Core
{
    public class Client
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

        public void ConnectTo(Server server)
        {
            server.Clients.Add(this);
        }
    }
}