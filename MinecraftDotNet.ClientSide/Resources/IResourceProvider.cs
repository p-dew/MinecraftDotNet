using System.Collections.Generic;
using System.Drawing;
using System.IO;
using MinecraftDotNet.Core.Resources;

namespace MinecraftDotNet.ClientSide.Resources
{
    public interface IResourceProvider
    {
        Texture GetTexture(ResourceId id);
        Sound GetSound(ResourceId id);
        // ...
    }

    public interface IResourceLoader<out TResource> where TResource : IResource
    {
        TResource Load(ResourceId id);
    }

    public class FileTextureLoader : IResourceLoader<Texture>
    {
        private readonly string _path;

        public FileTextureLoader(string path)
        {
            _path = path;
        }
        
        public Texture Load(ResourceId id)
        {
            var fileName = $"{_path}/{id}.png";
            var bitmap = new Bitmap(fileName);
            var texture = new Texture(id, bitmap);
            return texture;
        }
    }

    public class FileSoundLoader : IResourceLoader<Sound>
    {
        private readonly string _path;

        public FileSoundLoader(string path)
        {
            _path = path;
        }

        public Sound Load(ResourceId id)
        {
            var fileName = $"{_path}/{id}.png";
            var data = File.ReadAllBytes(fileName);
            return new Sound(id, data);
        }
    }

    public class ResourceProvider : IResourceProvider
    {
        private readonly IResourceLoader<Sound> _soundLoader;
        private readonly IResourceLoader<Texture> _textureLoader;

        public ResourceProvider(IResourceLoader<Texture> textureLoader, IResourceLoader<Sound> soundLoader)
        {
            _soundLoader = soundLoader;
            _textureLoader = textureLoader;
        }
        
        public Texture GetTexture(ResourceId id)
        {
            return _textureLoader.Load(id);
        }

        public Sound GetSound(ResourceId id)
        {
            return _soundLoader.Load(id);
        }
    }
}