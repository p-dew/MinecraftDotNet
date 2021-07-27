using System.Collections.Generic;
using MinecraftDotNet.Core.Entities;

namespace MinecraftDotNet.Core.Physics
{
    public class PhysicsEngine
    {
        private IList<Entity> _entities;

        public PhysicsEngine()
        {
            _entities = new List<Entity>();
        }

        public float UpdateRate { get; } = 0.05F;

        public void Update(Entity entity)
        {
            
        }
    }
}