using MinecraftDotNet.Core.Math;

namespace MinecraftDotNet.Core.Entities
{
    public abstract class Entity
    {
        protected Entity()
        {
            Position = new Coords3(0, 0, 0);
        }

        public Coords3 Position { get; set; }
    }
}