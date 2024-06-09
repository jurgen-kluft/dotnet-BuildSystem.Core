// ReSharper disable All

#pragma warning disable CS0649 // Field is never assigned to, and will always have its default value
#pragma warning disable CS8981 // The type name only contains lower-cased ascii characters. Such names may become reserved for the language.
#pragma warning disable CS0414 // Field is assigned but its value is never used
#pragma warning disable CS0169 // Field is never used

using hydra.f32types;
namespace hydra
{
    // Prototype for defining shader binding layout and being able to generate HLSL/GLSL code from it
    //    This will generate files called:
    //      - "SimpleFullScreenHfx.vsl.h"
    //      - "SimpleFullScreenHfx.psl.h"
    //      - "SimpleFullScreenHfx.csl.h"

    namespace ShaderModule
    {
        namespace SimpleFullScreenHfx
        {
            public class VertexShaderLayout : IShaderLayout
            {
                [uniform]
                [layout(layout: "std140", binding: 7)]
                public struct LocalConstants
                {
                    public float scale;
                    public float modulo;
                    public float2 pad_tail;
                }

                public LocalConstants local_constants;

                [output] public vec4 vTexCoord;
            }

            public class PixelShaderLayout : IShaderLayout
            {
                [input] public vec4 vTexCoord;
                [output] public vec4 outColor;
                [layout(binding: 0)] [uniform] public sampler2D input_texture;
            }

            public class ComputeShaderLayout : IShaderLayout
            {
                [uniform]
                [layout(layout: "std140", binding: 7)]
                public struct LocalConstants
                {
                    public float scale;
                    public float modulo;
                    public float2 pad_tail;
                }

                public LocalConstants local_constants;

                [layout(binding: 1)] [uniform] public sampler2D albedo_texture;

                [layout("rgba8", binding: 0)] [uniform] [writeonly]
                public image2D destination_texture;

                [layout(local_size_x: 32, local_size_y: 32, local_size_z: 1)]
                public _in_ _in;
            }
        }
    }
}
