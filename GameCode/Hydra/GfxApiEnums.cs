//
//  Hydra Graphics - v0.01
//
//  Quote: "Rendering in general is a matter of creating, modifying and combining resources"
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
    using s8 = sbyte;
    using ResourceHandle = uint;

    // Enums ////////////////////////////////////////////////////////////////////////

    namespace Blend
    {
        public enum Enum : byte
        {
            Zero,
            One,
            SrcColor,
            InvSrcColor,
            SrcAlpha,
            InvSrcAlpha,
            DestAlpha,
            InvDestAlpha,
            DestColor,
            InvDestColor,
            SrcAlphasat,
            Src1Color,
            InvSrc1Color,
            Src1Alpha,
            InvSrc1Alpha,
            Count
        };
    }

    namespace BlendOperation
    {
        public enum Enum : byte
        {
            Add,
            Subtract,
            RevSubtract,
            Min,
            Max,
            Count
        };

        public enum Mask : byte
        {
            Add_mask = 1 << 0,
            Subtract_mask = 1 << 1,
            RevSubtract_mask = 1 << 2,
            Min_mask = 1 << 3,
            Max_mask = 1 << 4,
            Count_mask = 1 << 5
        };
    }

    namespace ColorWriteEnabled
    {
        public enum Enum : byte
        {
            Red,
            Green,
            Blue,
            Alpha,
            All,
            Count
        };

        public enum Mask : byte
        {
            Red_mask = 1 << 0,
            Green_mask = 1 << 1,
            Blue_mask = 1 << 2,
            Alpha_mask = 1 << 3,
            All_mask = Red_mask | Green_mask | Blue_mask | Alpha_mask
        };
    }

    namespace ComparisonFunction
    {
        public enum Enum : ushort
        {
            Never,
            Less,
            Equal,
            LessEqual,
            Greater,
            NotEqual,
            GreaterEqual,
            Always,
            Count
        };

        public enum Mask : ushort
        {
            Never_mask = 1 << 0,
            Less_mask = 1 << 1,
            Equal_mask = 1 << 2,
            LessEqual_mask = 1 << 3,
            Greater_mask = 1 << 4,
            NotEqual_mask = 1 << 5,
            GreaterEqual_mask = 1 << 6,
            Always_mask = 1 << 7,
            Count_mask = 1 << 8
        };
    }

    namespace CullMode
    {
        public enum Enum : byte
        {
            None,
            Front,
            Back,
            Count
        };

        public enum Mask : byte
        {
            None_mask = 1 << 0,
            Front_mask = 1 << 1,
            Back_mask = 1 << 2,
            Count_mask = 1 << 3
        };
    }

    namespace DepthWriteMask
    {
        public enum Enum : byte
        {
            Zero,
            All,
            Count
        };

        public enum Mask : byte
        {
            Zero_mask = 1 << 0,
            All_mask = 1 << 1,
            Count_mask = 1 << 2
        };
    }

    namespace FillMode
    {
        public enum Enum : byte
        {
            Wireframe,
            Solid,
            Point,
            Count
        };

        public enum Mask : byte
        {
            Wireframe_mask = 1 << 0,
            Solid_mask = 1 << 1,
            Point_mask = 1 << 2,
            Count_mask = 1 << 3
        };
    }

    namespace FrontClockwise
    {
        public enum Enum : byte
        {
            True,
            False,
            Count
        };

        public enum Mask : byte
        {
            True_mask = 1 << 0,
            False_mask = 1 << 1,
            Count_mask = 1 << 2
        };
    }

    namespace StencilOperation
    {
        public enum Enum : ushort
        {
            Keep,
            Zero,
            Replace,
            IncrSat,
            DecrSat,
            Invert,
            Incr,
            Decr,
            Count
        };

        public enum Mask : ushort
        {
            Keep_mask = 1 << 0,
            Zero_mask = 1 << 1,
            Replace_mask = 1 << 2,
            IncrSat_mask = 1 << 3,
            DecrSat_mask = 1 << 4,
            Invert_mask = 1 << 5,
            Incr_mask = 1 << 6,
            Decr_mask = 1 << 7,
            Count_mask = 1 << 8
        };
    }

    public static partial class TextureFormat
    {
        public enum Enum : ushort
        {
            UNKNOWN,
            R32G32B32A32_TYPELESS,
            R32G32B32A32_FLOAT,
            R32G32B32A32_UINT,
            R32G32B32A32_SINT,
            R32G32B32_TYPELESS,
            R32G32B32_FLOAT,
            R32G32B32_UINT,
            R32G32B32_SINT,
            R16G16B16A16_TYPELESS,
            R16G16B16A16_FLOAT,
            R16G16B16A16_UNORM,
            R16G16B16A16_UINT,
            R16G16B16A16_SNORM,
            R16G16B16A16_SINT,
            R32G32_TYPELESS,
            R32G32_FLOAT,
            R32G32_UINT,
            R32G32_SINT,
            R10G10B10A2_TYPELESS,
            R10G10B10A2_UNORM,
            R10G10B10A2_UINT,
            R11G11B10_FLOAT,
            R8G8B8A8_TYPELESS,
            R8G8B8A8_UNORM,
            R8G8B8A8_UNORM_SRGB,
            R8G8B8A8_UINT,
            R8G8B8A8_SNORM,
            R8G8B8A8_SINT,
            R16G16_TYPELESS,
            R16G16_FLOAT,
            R16G16_UNORM,
            R16G16_UINT,
            R16G16_SNORM,
            R16G16_SINT,
            R32_TYPELESS,
            R32_FLOAT,
            R32_UINT,
            R32_SINT,
            R8G8_TYPELESS,
            R8G8_UNORM,
            R8G8_UINT,
            R8G8_SNORM,
            R8G8_SINT,
            R16_TYPELESS,
            R16_FLOAT,
            R16_UNORM,
            R16_UINT,
            R16_SNORM,
            R16_SINT,
            R8_TYPELESS,
            R8_UNORM,
            R8_UINT,
            R8_SNORM,
            R8_SINT,
            R9G9B9E5_SHAREDEXP,
            D32_FLOAT_S8X24_UINT,
            D24_UNORM_S8_UINT,
            D32_FLOAT,
            D24_UNORM_X8_UINT,
            D16_UNORM,
            S8_UINT,
            BC1_TYPELESS,
            BC1_UNORM,
            BC1_UNORM_SRGB,
            BC2_TYPELESS,
            BC2_UNORM,
            BC2_UNORM_SRGB,
            BC3_TYPELESS,
            BC3_UNORM,
            BC3_UNORM_SRGB,
            BC4_TYPELESS,
            BC4_UNORM,
            BC4_SNORM,
            BC5_TYPELESS,
            BC5_UNORM,
            BC5_SNORM,
            B5G6R5_UNORM,
            B5G5R5A1_UNORM,
            B8G8R8A8_UNORM,
            B8G8R8X8_UNORM,
            R10G10B10_XR_BIAS_A2_UNORM,
            B8G8R8A8_TYPELESS,
            B8G8R8A8_UNORM_SRGB,
            B8G8R8X8_TYPELESS,
            B8G8R8X8_UNORM_SRGB,
            BC6H_TYPELESS,
            BC6H_UF16,
            BC6H_SF16,
            BC7_TYPELESS,
            BC7_UNORM,
            BC7_UNORM_SRGB,
            FORCE_UINT,
            Count
        };
    }

    namespace TopologyType
    {
        public enum Enum : byte
        {
            Unknown,
            Point,
            Line,
            Triangle,
            Patch,
            Count
        };

        public enum Mask : byte
        {
            Unknown_mask = 1 << 0,
            Point_mask = 1 << 1,
            Line_mask = 1 << 2,
            Triangle_mask = 1 << 3,
            Patch_mask = 1 << 4,
            Count_mask = 1 << 5
        };
    }

    namespace BufferType
    {
        public enum Enum : byte
        {
            Vertex,
            Index,
            Constant,
            Indirect,
            Count
        };

        public enum Mask : byte
        {
            Vertex_mask = 1 << 0,
            Index_mask = 1 << 1,
            Constant_mask = 1 << 2,
            Indirect_mask = 1 << 3,
            Count_mask = 1 << 4
        };
    }

    namespace ResourceUsageType
    {
        public enum Enum : byte
        {
            Immutable,
            Dynamic,
            Stream,
            Count
        };

        public enum Mask : byte
        {
            Immutable_mask = 1 << 0,
            Dynamic_mask = 1 << 1,
            Stream_mask = 1 << 2,
            Count_mask = 1 << 3
        };
    }

    namespace IndexType
    {
        public enum Enum : byte
        {
            Uint16,
            Uint32,
            Count
        };

        public enum Mask : byte
        {
            Uint16_mask = 1 << 0,
            Uint32_mask = 1 << 1,
            Count_mask = 1 << 2
        };
    }

    namespace TextureType
    {
        public enum Enum : byte
        {
            Texture1D,
            Texture2D,
            Texture3D,
            Texture_1D_Array,
            Texture_2D_Array,
            Texture_Cube_Array,
            Count
        };

        public enum Mask : byte
        {
            Texture1D_mask = 1 << 0,
            Texture2D_mask = 1 << 1,
            Texture3D_mask = 1 << 2,
            Texture_1D_Array_mask = 1 << 3,
            Texture_2D_Array_mask = 1 << 4,
            Texture_Cube_Array_mask = 1 << 5,
            Count_mask = 1 << 6
        };
    }

    namespace ShaderStage
    {
        public enum Enum : byte
        {
            Vertex,
            Fragment,
            Geometry,
            Compute,
            Hull,
            Domain,
            Count
        };

        public enum Mask : byte
        {
            Vertex_mask = 1 << 0,
            Fragment_mask = 1 << 1,
            Geometry_mask = 1 << 2,
            Compute_mask = 1 << 3,
            Hull_mask = 1 << 4,
            Domain_mask = 1 << 5,
            Count_mask = 1 << 6
        };
    }

    namespace TextureFilter
    {
        public enum Enum : byte
        {
            Nearest,
            Linear,
            Count
        };

        public enum Mask : byte
        {
            Nearest_mask = 1 << 0,
            Linear_mask = 1 << 1,
            Count_mask = 1 << 2
        };
    }

    namespace TextureMipFilter
    {
        public enum Enum : byte
        {
            Nearest,
            Linear,
            Count
        };

        public enum Mask : byte
        {
            Nearest_mask = 1 << 0,
            Linear_mask = 1 << 1,
            Count_mask = 1 << 2
        };
    }

    namespace TextureAddressMode
    {
        public enum Enum : byte
        {
            Repeat,
            Mirrored_Repeat,
            Clamp_Edge,
            Clamp_Border,
            Count
        };

        public enum Mask : byte
        {
            Repeat_mask = 1 << 0,
            Mirrored_Repeat_mask = 1 << 1,
            Clamp_Edge_mask = 1 << 2,
            Clamp_Border_mask = 1 << 3,
            Count_mask = 1 << 4
        };
    }

    namespace VertexComponentFormat
    {
        public enum Enum : byte
        {
            Float,
            Float2,
            Float3,
            Float4,
            Mat4,
            Byte,
            Byte4N,
            UByte,
            UByte4N,
            Short2,
            Short2N,
            Short4,
            Short4N,
            Count
        };
    }

    namespace VertexInputRate
    {
        public enum Enum : byte
        {
            PerVertex,
            PerInstance,
            Count
        };

        public enum Mask : byte
        {
            PerVertex_mask = 1 << 0,
            PerInstance_mask = 1 << 1,
            Count_mask = 1 << 2
        };
    }

    namespace LogicOperation
    {
        public enum Enum : byte
        {
            Clear,
            Set,
            Copy,
            CopyInverted,
            Noop,
            Invert,
            And,
            Nand,
            Or,
            Nor,
            Xor,
            Equiv,
            AndReverse,
            AndInverted,
            OrReverse,
            OrInverted,
            Count
        };

        public enum Mask : uint
        {
            Clear_mask = 1 << 0,
            Set_mask = 1 << 1,
            Copy_mask = 1 << 2,
            CopyInverted_mask = 1 << 3,
            Noop_mask = 1 << 4,
            Invert_mask = 1 << 5,
            And_mask = 1 << 6,
            Nand_mask = 1 << 7,
            Or_mask = 1 << 8,
            Nor_mask = 1 << 9,
            Xor_mask = 1 << 10,
            Equiv_mask = 1 << 11,
            AndReverse_mask = 1 << 12,
            AndInverted_mask = 1 << 13,
            OrReverse_mask = 1 << 14,
            OrInverted_mask = 1 << 15,
            Count_mask = 1 << 16
        };
    }

    namespace QueueType
    {
        public enum Enum : byte
        {
            Graphics,
            Compute,
            CopyTransfer,
            Count
        };

        public enum Mask : byte
        {
            Graphics_mask = 1 << 0,
            Compute_mask = 1 << 1,
            CopyTransfer_mask = 1 << 2,
            Count_mask = 1 << 3
        };
    }

    namespace CommandType
    {
        public enum Enum : byte
        {
            BindPipeline,
            BindResourceTable,
            BindVertexBuffer,
            BindIndexBuffer,
            BindResourceSet,
            Draw,
            DrawIndexed,
            DrawInstanced,
            DrawIndexedInstanced,
            Dispatch,
            CopyResource,
            SetScissor,
            SetViewport,
            Clear,
            ClearDepth,
            ClearStencil,
            BeginPass,
            EndPass,
            Count
        };
    }

    namespace ResourceType
    {
        public enum Enum : byte
        {
            Sampler,
            Texture,
            TextureRW,
            Constants,
            Buffer,
            BufferRW,
            Count
        };

        public enum Mask : byte
        {
            Sampler_mask = 1 << 0,
            Texture_mask = 1 << 1,
            TextureRW_mask = 1 << 2,
            Constants_mask = 1 << 3,
            Buffer_mask = 1 << 4,
            BufferRW_mask = 1 << 5,
            Count_mask = 1 << 6
        };
    }

    // Manually typed enums
    public enum DeviceExtensions
    {
        DeviceExtensions_DebugCallback = 1 << 0,
    };

    // TODO: Error enum?

    // Consts ///////////////////////////////////////////////////////////////////////

    namespace Constraints
    {
        public enum Enum : byte
        {
            k_max_image_outputs = 8, // Maximum number of images/render_targets/fbo attachments usable.
            k_max_resource_layouts = 8, // Maximum number of layouts in the pipeline.
            k_max_shader_stages = 5,
        }
    }

    //
    //
    public struct ViewportState
    {
        public u32 num_viewports; // = 0;
        public u32 num_scissors; // = 0;
        public Viewport viewport; // = nullptr;
        public f32types.rect2d scissors; // = nullptr;
    };

    //
    //
    public struct StencilOperationState
    {
        public StencilOperation.Enum fail; // = StencilOperation::Keep;
        public StencilOperation.Enum pass; // = StencilOperation::Keep;
        public StencilOperation.Enum depth_fail; // = StencilOperation::Keep;
        public ComparisonFunction.Enum compare; // = ComparisonFunction::Always;
        public u32 compare_mask; // = 0xff;
        public u32 write_mask; // = 0xff;
        public u32 reference; // = 0xff;
    };

    //
    //
    public struct DepthStencilCreation
    {
        public StencilOperationState front;
        public StencilOperationState back;
        public ComparisonFunction.Enum depth_comparison; // = ComparisonFunction::Always;

        public bool depth_enable;
        public bool depth_write_enable;
        public bool stencil_enable;


        // Default constructor
        public DepthStencilCreation()
        {
            depth_enable = false;
            depth_write_enable = false;
            stencil_enable = false;
            depth_comparison = ComparisonFunction.Enum.Less;
        }
    };

    public struct BlendState()
    {
        public Blend.Enum source_color; // = Blend::One;
        public Blend.Enum destination_color; // = Blend::One;
        public BlendOperation.Enum color_operation; // = BlendOperation::Add;

        public Blend.Enum source_alpha; // = Blend::One;
        public Blend.Enum destination_alpha; // = Blend::One;
        public BlendOperation.Enum alpha_operation; // = BlendOperation::Add;

        public ColorWriteEnabled.Mask color_write_mask; // = ColorWriteEnabled::All_mask;

        public bool blend_enabled = false;
        public bool separate_blend = false;
    };

    public struct BlendStateCreation
    {
        public List<BlendState> blend_states;
    };

    //
    //
    public struct RasterizationCreation
    {
        public CullMode.Enum cull_mode; // = CullMode::None;
        public FrontClockwise.Enum front; // = FrontClockwise::False;
        public FillMode.Enum fill; // = FillMode::Solid;
    };

    //
    //
    public class DeviceCreation
    {
        public object window; // void* = nullptr; // Pointer to API-specific window: SDL_Window, GLFWWindow
        public u16 width; // = 1;
        public u16 height; // = 1;
        public bool debug; // = false;
    };

    //
    //
    public class BufferCreation
    {
        public BufferType.Enum type; // = BufferType::Vertex;
        public ResourceUsageType.Enum usage; // = ResourceUsageType::Immutable;
        public u32 size; // = 0;
        public buffer_t initial_data; // void* = nullptr;
        public string name; // = nullptr;
    };

    //
    //
    public class TextureCreation
    {
        public buffer_t initial_data; // void* = nullptr;
        public u16 width; // = 1;
        public u16 height; // = 1;
        public u16 depth; // = 1;
        public byte mipmaps; // = 1;
        public byte render_target; // = 0;

        public TextureFormat.Enum format; // = TextureFormat::UNKNOWN;
        public TextureType.Enum type; // = TextureType::Texture2D;

        public string name; // = nullptr;
    };

    //
    //
    public class SamplerCreation
    {
        public TextureFilter.Enum min_filter; // = TextureFilter::Nearest;
        public TextureFilter.Enum mag_filter; // = TextureFilter::Nearest;
        public TextureMipFilter.Enum mip_filter; // = TextureMipFilter::Nearest;

        public TextureAddressMode.Enum address_mode_u; // = TextureAddressMode::Repeat;
        public TextureAddressMode.Enum address_mode_v; // = TextureAddressMode::Repeat;
        public TextureAddressMode.Enum address_mode_w; // = TextureAddressMode::Repeat;

        public string name; // = nullptr;
    };

    namespace RenderPassType
    {
        public enum Enum : byte
        {
            Geometry,
            Swapchain,
            Compute
        };
    }

    //
    //
    public struct BufferDescription
    {
        public BufferType.Enum type; // = BufferType::Vertex;
        public ResourceUsageType.Enum usage; // = ResourceUsageType::Immutable;
        public u32 size; // = 0;
        public string name; //= nullptr;
    };

    //
    //
    public struct TextureDescription
    {
        public u16 width; // = 1;
        public u16 height; // = 1;
        public u16 depth; // = 1;
        public byte mipmaps; // = 1;
        public byte render_target; // = 0;
        public TextureFormat.Enum format; // = TextureFormat::UNKNOWN;
        public TextureType.Enum type; // = TextureType::Texture2D;
    };

    //
    //
    public struct SamplerDescription
    {
        public TextureFilter.Enum min_filter; // = TextureFilter::Nearest;
        public TextureFilter.Enum mag_filter; // = TextureFilter::Nearest;
        public TextureMipFilter.Enum mip_filter; // = TextureMipFilter::Nearest;

        public TextureAddressMode.Enum address_mode_u; // = TextureAddressMode::Repeat;
        public TextureAddressMode.Enum address_mode_v; // = TextureAddressMode::Repeat;
        public TextureAddressMode.Enum address_mode_w; // = TextureAddressMode::Repeat;
    };
}
