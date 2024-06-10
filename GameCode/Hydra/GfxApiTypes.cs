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

    // i32types types
    namespace i32types
    {
        //
        //
        public struct rect2d()
        {
            public i16 x = 0;
            public i16 y = 0;
            public u16 width = 0;
            public u16 height = 0;
        };

        //
        //
        public struct int2
        {
            public int x, y;
        };

        public struct uint2
        {
            public uint x, y;
        };

        public struct int3
        {
            public int x, y, z;
        };

        public struct uint3
        {
            public uint x, y, z;
            public uint2 xy { get; }
        };


        //
        // Color class that embeds color in a uint32.
        //
        public struct color
        {
            public uint abgr;

            public const uint red = 0xff0000ff;
            public const uint green = 0xff00ff00;
            public const uint blue = 0xffff0000;
            public const uint black = 0xff000000;
            public const uint white = 0xffffffff;
            public const uint transparent = 0x00000000;
        };

    }

    // 32 bit floating point types
    namespace f32types
    {
        public struct float2
        {
            public float x, y;
        };

        public struct vec2
        {
            public float x, y;
        };

        public struct float3
        {
            public float x, y, z;
        }

        public struct vec3
        {
            public float3 v;

            public vec3(float x, float y, float z)
            {
                v.x = x;
                v.y = y;
                v.z = z;
            }

            public float x { get => v.x; set => v.x = value; }
            public float y { get => v.y; set => v.y = value; }
            public float z { get => v.z; set => v.z = value; }

            // operators
            public static vec3 operator +(vec3 a, vec3 b)
            {
                return new vec3(a.x + b.x, a.y + b.y, a.z + b.z);
            }

            public static vec3 operator -(vec3 a, vec3 b)
            {
                return new vec3(a.x - b.x, a.y - b.y, a.z - b.z);
            }

            public static vec3 operator *(vec3 a, float b)
            {
                return new vec3(a.x + b, a.y + b, a.z + b);
            }
        };

        public struct float4
        {
            public float x, y, z, w;
        }

        public struct vec4
        {
            public float x, y, z, w;

            public vec4(float x, float y, float z, float w)
            {
                this.x = x;
                this.y = y;
                this.z = z;
                this.w = w;
            }

            public vec4(vec3 xyz, float w)
            {
                this.x = xyz.x;
                this.y = xyz.y;
                this.z = xyz.z;
                this.w = w;
            }

            public vec4(vec3 xyz)
            {
                this.x = xyz.x;
                this.y = xyz.y;
                this.z = xyz.z;
                this.w = 0.0f;
            }

            public vec4(vec2 xy, float z, float w)
            {
                this.x = xy.x;
                this.y = xy.y;
                this.z = z;
                this.w = w;
            }

            public vec2 xy { get; }
            public vec3 xyz { get; }
        };

        public struct mat4
        {
            public vec4 m0, m1, m2, m3;
        };

        public struct vec3s
        {
            public float x, y, z;
        }

        //
        //
        public struct box3d
        {
            public vec3s min;
            public vec3s max;
        }

        //
        //
        struct ray3d
        {
 public            vec3s origin;
 public            vec3s direction;
        };

        //
        //
        public struct rect2d()
        {
        public     f32 x = 0.0f;
        public     f32 y = 0.0f;
        public     f32 width = 0.0f;
        public     f32 height = 0.0f;
        };

        public struct range
        {
            public f32 min;
            public f32 max;
        };
    }

    //
    //
    public struct Viewport
    {
        public i32types.rect2d rect;
        public f32types.range depth;
    };
}

namespace hydra
{
    using ResourceHandle = uint;

    public class buffer_t
    {
        public byte[] data;
    }

    public struct BufferHandle
    {
        public ResourceHandle handle;
    };

    public struct TextureHandle
    {
        public ResourceHandle handle;
    };

    public struct ShaderHandle
    {
        public ResourceHandle handle;
    };

    public struct SamplerHandle
    {
        public ResourceHandle handle;
    };

    public struct ResourceListLayoutHandle
    {
        public ResourceHandle handle;
    };

    public struct ResourceListHandle
    {
        public ResourceHandle handle;
    };

    public struct PipelineHandle
    {
        public ResourceHandle handle;
    };

    public struct RenderPassHandle
    {
        public ResourceHandle handle;
    };
}
