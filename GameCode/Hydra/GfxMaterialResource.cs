//
//  Hydra Rendering - v0.1
//
//  Material Resource
//

// ReSharper disable All

#pragma warning disable CS0649 // Field is never assigned to, and will always have its default value
#pragma warning disable CS8981 // The type name only contains lower-cased ascii characters. Such names may become reserved for the language.
#pragma warning disable CS0414 // Field is assigned but its value is never used
#pragma warning disable CS0169 // Field is never used

// Types
namespace hydra
{
    using u64 = ulong;
    using u32 = uint;
    using u16 = ushort;
    using u8 = byte;
    using s64 = long;
    using s32 = int;
    using s16 = short;
    using i16 = short;
    using s8 = sbyte;
    using f32 = float;
    using f64 = double;

    public enum PropertyType
    {
        Float,
        Int,
        Range,
        Color,
        Vector,
        Texture1D,
        Texture2D,
        Texture3D,
        TextureVolume,
        Unknown
    };

    //
    //
    public class ComputeDispatch
    {
        public u16 x; // = 0;
        public u16 y; // = 0;
        public u16 z; // = 0;
    };

    //
    //
    public class RenderStateBlueprint
    {
        public RasterizationCreation rasterization;
        public DepthStencilCreation depth_stencil;
        public BlendStateCreation blending;
    }

    //
    //
    public class ShaderCodeBlueprint
    {
        public buffer_t code;
        public ShaderStage.Enum stage;
    }

    //
    //
    public interface IResourceLayoutBlueprint
    {

    }

    public class MyResourceLayout : IResourceLayoutBlueprint
    {

    }

    //
    //
    public class ShaderPassBlueprint
    {
        public string debug_name;
        public string stage_name;

        public ComputeDispatch compute_dispatch;
        public u8 is_spirv;

        public List<ShaderCodeBlueprint> shaders;

        RenderStateBlueprint render_state;

        List<VertexStream> vertex_streams;
        List<VertexAttribute> vertex_attributes;

        //List<ResourceLayoutBlueprint> resource_layouts;
    };

    //
    //
    public class ShaderEffectBlueprint
    {
        public static readonly u32 sVersion = 0;

        public string binary_header_magic;
        public string debug_name;
        public List<ShaderPassBlueprint> passes;
    }
}
