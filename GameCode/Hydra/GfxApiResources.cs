//
//  Hydra Rendering - v0.1
//
//  High level rendering implementation based on Hydra Graphics library.
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

    // Example IShaderVertexFormat
    // From this could we not automatically generate the "layout(location = x) in type name" lines for the shader code?
    // We also need to generate a VertexAttribute array so that the C++ side knows how to tell the Gpu what the format is.
    public class MyShaderVertexFormat
    {
        public f32types.vec3 inPosition;
        public f32types.vec3 inNormal;
    }

    //
    //
    public struct VertexAttribute
    {
        public u32 offset; // = 0;
        public u16 location; // = 0;
        public u8 binding; // = 0; What is the maximum value according to specifications ?
        public VertexComponentFormat.Enum format; // = VertexComponentFormat::Count;
    };

    //
    //
    public struct VertexStream
    {
        public u16 stride; // = 0;
        public u8 binding; // = 0; What is the maximum value according to specifications ?
        public VertexInputRate.Enum input_rate; // = VertexInputRate::Count;
    };

    //
    //
    public struct VertexInputCreation
    {
        public List<VertexStream> vertex_streams;
        public List<VertexAttribute> vertex_attributes;
    };

}
