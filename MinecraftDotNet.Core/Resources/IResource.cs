using System;

namespace MinecraftDotNet.Core.Resources
{
    public interface IResource : IDisposable, IEquatable<IResource>
    {
        ResourceId Id { get; }

        bool IEquatable<IResource>.Equals(IResource other)
        {
            return object.Equals(other.Id, this.Id);
        }
    }
}