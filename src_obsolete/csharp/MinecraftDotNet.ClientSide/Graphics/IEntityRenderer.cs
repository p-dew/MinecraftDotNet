using MinecraftDotNet.Core.Entities;

namespace MinecraftDotNet.ClientSide.Graphics
{
    public interface IEntityRenderer
    {
        void Render(Entity entity);
    }
}