using GameCore;
using GameData;

namespace MetaTest
{
    public struct BufferHandle : IStruct
    {
        public Int32 Value;

        public void StructWrite(IBinaryWriter writer)
        {
            writer.Write(Value);
        }
    }

    public struct TextureHandle : IStruct
    {
        public Int32 Value;

        public void StructWrite(IBinaryWriter writer)
        {
            writer.Write(Value);
        }
    }

    public struct SamplerHandle : IStruct
    {
        public Int32 Value;

        public void StructWrite(IBinaryWriter writer)
        {
            writer.Write(Value);
        }
    }

    public class BufferStringMap
    {
        public string key;
        public BufferHandle value;
    }

    public class TextureStringMap
    {
        public string key;
        public TextureHandle value;
    }

    public class SamplerStringMap
    {
        public string key;
        public SamplerHandle value;
    }

    public struct ShaderResourcesDatabase
    {
        public BufferStringMap name_to_buffer;
        public TextureStringMap name_to_texture;
        public SamplerStringMap name_to_sampler;
    }
}
