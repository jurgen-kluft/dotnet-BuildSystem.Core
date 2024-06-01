//
//  Hydra Graphics - v0.047

// ReSharper disable All

#pragma warning disable CS0649 // Field is never assigned to, and will always have its default value
#pragma warning disable CS8981 // The type name only contains lower-cased ascii characters. Such names may become reserved for the language.
#pragma warning disable CS0414 // Field is assigned but its value is never used
#pragma warning disable CS0169 // Field is never used

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

    // Attribute that specifies that a static class should be treated as a namespace.
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    public sealed class Namespace : Attribute
    {
    }

    namespace graphics
    {
        // Resources ////////////////////////////////////////////////////////////////////

        struct BufferHandle
        {
            ResourceHandle handle;
        };

        struct TextureHandle
        {
            ResourceHandle handle;
        };

        struct ShaderHandle
        {
            ResourceHandle handle;
        };

        struct SamplerHandle
        {
            ResourceHandle handle;
        };

        struct ResourceListLayoutHandle
        {
            ResourceHandle handle;
        };

        struct ResourceListHandle
        {
            ResourceHandle handle;
        };

        struct PipelineHandle
        {
            ResourceHandle handle;
        };

        struct RenderPassHandle
        {
            ResourceHandle handle;
        };

        // Enums ////////////////////////////////////////////////////////////////////////


        // !!! WARNING !!!
        // THIS CODE IS GENERATED WITH HYDRA DATA FORMAT CODE GENERATOR.

        /////////////////////////////////////////////////////////////////////////////////

        namespace Blend
        {
            enum Enum
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
            enum Enum
            {
                Add,
                Subtract,
                RevSubtract,
                Min,
                Max,
                Count
            };

            enum Mask
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
            enum Enum
            {
                Red,
                Green,
                Blue,
                Alpha,
                All,
                Count
            };

            enum Mask
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
            enum Enum
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

            enum Mask
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
            enum Enum
            {
                None,
                Front,
                Back,
                Count
            };

            enum Mask
            {
                None_mask = 1 << 0,
                Front_mask = 1 << 1,
                Back_mask = 1 << 2,
                Count_mask = 1 << 3
            };
        }

        namespace DepthWriteMask
        {
            enum Enum
            {
                Zero,
                All,
                Count
            };

            enum Mask
            {
                Zero_mask = 1 << 0,
                All_mask = 1 << 1,
                Count_mask = 1 << 2
            };
        }

        namespace FillMode
        {
            enum Enum
            {
                Wireframe,
                Solid,
                Point,
                Count
            };

            enum Mask
            {
                Wireframe_mask = 1 << 0,
                Solid_mask = 1 << 1,
                Point_mask = 1 << 2,
                Count_mask = 1 << 3
            };
        }

        namespace FrontClockwise
        {
            enum Enum
            {
                True,
                False,
                Count
            };

            enum Mask
            {
                True_mask = 1 << 0,
                False_mask = 1 << 1,
                Count_mask = 1 << 2
            };
        }

        namespace StencilOperation
        {
            enum Enum
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

            enum Mask
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

        static partial class TextureFormat
        {
            public enum Enum
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
            enum Enum
            {
                Unknown,
                Point,
                Line,
                Triangle,
                Patch,
                Count
            };

            enum Mask
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
            enum Enum
            {
                Vertex,
                Index,
                Constant,
                Indirect,
                Count
            };

            enum Mask
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
            enum Enum
            {
                Immutable,
                Dynamic,
                Stream,
                Count
            };

            enum Mask
            {
                Immutable_mask = 1 << 0,
                Dynamic_mask = 1 << 1,
                Stream_mask = 1 << 2,
                Count_mask = 1 << 3
            };
        }

        namespace IndexType
        {
            enum Enum
            {
                Uint16,
                Uint32,
                Count
            };

            enum Mask
            {
                Uint16_mask = 1 << 0,
                Uint32_mask = 1 << 1,
                Count_mask = 1 << 2
            };
        }

        namespace TextureType
        {
            enum Enum
            {
                Texture1D,
                Texture2D,
                Texture3D,
                Texture_1D_Array,
                Texture_2D_Array,
                Texture_Cube_Array,
                Count
            };

            enum Mask
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
            enum Enum
            {
                Vertex,
                Fragment,
                Geometry,
                Compute,
                Hull,
                Domain,
                Count
            };

            enum Mask
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
            enum Enum
            {
                Nearest,
                Linear,
                Count
            };

            enum Mask
            {
                Nearest_mask = 1 << 0,
                Linear_mask = 1 << 1,
                Count_mask = 1 << 2
            };
        }

        namespace TextureMipFilter
        {
            enum Enum
            {
                Nearest,
                Linear,
                Count
            };

            enum Mask
            {
                Nearest_mask = 1 << 0,
                Linear_mask = 1 << 1,
                Count_mask = 1 << 2
            };
        }

        namespace TextureAddressMode
        {
            enum Enum
            {
                Repeat,
                Mirrored_Repeat,
                Clamp_Edge,
                Clamp_Border,
                Count
            };

            enum Mask
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
            enum Enum
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
            enum Enum
            {
                PerVertex,
                PerInstance,
                Count
            };

            enum Mask
            {
                PerVertex_mask = 1 << 0,
                PerInstance_mask = 1 << 1,
                Count_mask = 1 << 2
            };
        }

        namespace LogicOperation
        {
            enum Enum
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

            enum Mask
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
            enum Enum
            {
                Graphics,
                Compute,
                CopyTransfer,
                Count
            };

            enum Mask
            {
                Graphics_mask = 1 << 0,
                Compute_mask = 1 << 1,
                CopyTransfer_mask = 1 << 2,
                Count_mask = 1 << 3
            };
        }

        namespace CommandType
        {
            enum Enum
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
            enum Enum
            {
                Sampler,
                Texture,
                TextureRW,
                Constants,
                Buffer,
                BufferRW,
                Count
            };

            enum Mask
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
        enum DeviceExtensions
        {
            DeviceExtensions_DebugCallback = 1 << 0,
        };

        // TODO: Error enum?

        // Consts ///////////////////////////////////////////////////////////////////////

        namespace Constraints
        {
            enum Enum : int
            {
                k_max_image_outputs = 8, // Maximum number of images/render_targets/fbo attachments usable.
                k_max_resource_layouts = 8, // Maximum number of layouts in the pipeline.
                k_max_shader_stages = 5,
            }
        }

        // Resource creation structs ////////////////////////////////////////////////////

        //
        //
        class Rect2D
        {
            float x; // = 0.0f;
            float y; // = 0.0f;
            float width; // = 0.0f;
            float height; // = 0.0f;
        };

        class IReadOnlyRect2D : Rect2D
        {
        }

        //
        //
        class Viewport
        {
            Rect2D rect;
            float min_depth; // = 0.0f;
            float max_depth; // = 0.0f;
        };

        class IReadOnlyViewport : Viewport
        {
        }

        //
        //
        struct ViewportState
        {
            u32 num_viewports; // = 0;
            u32 num_scissors; // = 0;
            Viewport viewport; // = nullptr;
            Rect2D scissors; // = nullptr;
        };

        //
        //
        struct StencilOperationState
        {
            StencilOperation.Enum fail; // = StencilOperation::Keep;
            StencilOperation.Enum pass; // = StencilOperation::Keep;
            StencilOperation.Enum depth_fail; // = StencilOperation::Keep;
            ComparisonFunction.Enum compare; // = ComparisonFunction::Always;
            u32 compare_mask; // = 0xff;
            u32 write_mask; // = 0xff;
            u32 reference; // = 0xff;
        };

        //
        //
        struct DepthStencilCreation
        {
            StencilOperationState front;
            StencilOperationState back;
            ComparisonFunction.Enum depth_comparison; // = ComparisonFunction::Always;

            bool depth_enable;
            bool depth_write_enable;
            bool stencil_enable;


            // Default constructor
            public DepthStencilCreation()
            {
                depth_enable = false;
                depth_write_enable = false;
                stencil_enable = false;
                depth_comparison = ComparisonFunction.Enum.Less;
            }
        };

        struct BlendState()
        {
            Blend.Enum source_color; // = Blend::One;
            Blend.Enum destination_color; // = Blend::One;
            BlendOperation.Enum color_operation; // = BlendOperation::Add;

            Blend.Enum source_alpha; // = Blend::One;
            Blend.Enum destination_alpha; // = Blend::One;
            BlendOperation.Enum alpha_operation; // = BlendOperation::Add;

            ColorWriteEnabled.Mask color_write_mask; // = ColorWriteEnabled::All_mask;

            bool blend_enabled = false;
            bool separate_blend = false;
        };

        struct BlendStateCreation()
        {
            BlendState[] blend_states; // = new BlendState[(int)Constraints.Enum.k_max_image_outputs];
            u32 active_states; // = 0;
        };

        //
        //
        struct RasterizationCreation
        {
            CullMode.Enum cull_mode; // = CullMode::None;
            FrontClockwise.Enum front; // = FrontClockwise::False;
            FillMode.Enum fill; // = FillMode::Solid;
        };

        //
        //
        class DeviceCreation
        {
            object window; // void* = nullptr; // Pointer to API-specific window: SDL_Window, GLFWWindow
            u16 width; // = 1;
            u16 height; // = 1;
            bool debug; // = false;
        };

        class IReadOnlyDeviceCreation : DeviceCreation
        {
        }

        //
        //
        class BufferCreation
        {
            BufferType.Enum type; // = BufferType::Vertex;
            ResourceUsageType.Enum usage; // = ResourceUsageType::Immutable;
            u32 size; // = 0;
            object initial_data; // void* = nullptr;
            string name; // = nullptr;
        };

        class IReadOnlyBufferCreation : BufferCreation
        {
        }

        //
        //
        class TextureCreation
        {
            object initial_data; // void* = nullptr;
            u16 width; // = 1;
            u16 height; // = 1;
            u16 depth; // = 1;
            byte mipmaps; // = 1;
            byte render_target; // = 0;

            TextureFormat.Enum format; // = TextureFormat::UNKNOWN;
            TextureType.Enum type; // = TextureType::Texture2D;

            string name; // = nullptr;
        };

        class IReadOnlyTextureCreation : TextureCreation
        {
        }

        //
        //
        class SamplerCreation
        {
            TextureFilter.Enum min_filter; // = TextureFilter::Nearest;
            TextureFilter.Enum mag_filter; // = TextureFilter::Nearest;
            TextureMipFilter.Enum mip_filter; // = TextureMipFilter::Nearest;

            TextureAddressMode.Enum address_mode_u; // = TextureAddressMode::Repeat;
            TextureAddressMode.Enum address_mode_v; // = TextureAddressMode::Repeat;
            TextureAddressMode.Enum address_mode_w; // = TextureAddressMode::Repeat;

            string name; // = nullptr;
        };

        class IReadOnlySamplerCreation : SamplerCreation
        {
        }

        //
        //
        class ShaderCreation
        {
            struct Stage
            {
                string code; // = nullptr;
                u32 code_size; // = 0;
                ShaderStage.Enum type; // = ShaderStage::Compute;
            };

            Stage[] stages; // new Stage[k_max_shader_stages];

            string name; //= nullptr;

            u32 stages_count; // = 0;
        };

        class IReadOnlyShaderCreation : ShaderCreation
        {
        }

        //
        //
        class ResourceListLayoutCreation
        {
            //
            // A single resource binding. It can be relative to one or more resources of the same type.
            //
            struct Binding
            {
                ResourceType.Enum type; // = ResourceType::Buffer;
                u16 start; // = 0;
                u16 count; // = 0;
                string name;
            };

            List<Binding> bindings;
        };

        class IReadOnlyResourceListLayoutCreation : ResourceListLayoutCreation
        {
        }

        //
        //
        class ResourceListCreation
        {
            ResourceListLayoutHandle layout;

            // List of resources
            struct Resource
            {
                ResourceHandle handle;
            };

            List<Resource> resources;
        };

        class IReadOnlyResourceListCreation : ResourceListCreation
        {
        }

        //
        //
        struct VertexAttribute
        {
            u16 location; // = 0;
            u16 binding; // = 0;
            u32 offset; // = 0;
            VertexComponentFormat.Enum format; // = VertexComponentFormat::Count;
        };

        //
        //
        struct VertexStream
        {
            u16 binding; // = 0;
            u16 stride; // = 0;
            VertexInputRate.Enum input_rate; // = VertexInputRate::Count;
        };

        //
        //
        struct VertexInputCreation
        {
            u32 num_vertex_streams; // = 0;
            u32 num_vertex_attributes; // = 0;

            List<VertexStream> vertex_streams;
            List<VertexAttribute> vertex_attributes;
        };

        //
        //
        struct ShaderBinding
        {
            string binding_name; // = nullptr;
            string resource_name; // = nullptr;
        };

        //
        //
        class RenderPassCreation
        {
            u16 num_render_targets; // = 0;
            byte is_swapchain; // = false;
            byte is_compute_post; // = false;                // This is set to determine if the post-process pass is compute or not.

            List<TextureHandle> output_textures;
            TextureHandle depth_stencil_texture;
        };

        class IReadOnlyRenderPassCreation : RenderPassCreation
        {
        }

        //
        //
        class PipelineCreation
        {
            RasterizationCreation rasterization;
            DepthStencilCreation depth_stencil;
            BlendStateCreation blend_state;
            VertexInputCreation vertex_input;
            ShaderCreation shaders;

            RenderPassHandle render_pass;
            ResourceListLayoutHandle[] resource_list_layout; // new ResourceListLayoutHandle[k_max_resource_layouts];
            ViewportState viewport; // = nullptr;

            u32 num_active_layouts; // = 0;
        };

        class IReadOnlyPipelineCreation : PipelineCreation
        {
        }

        // API-agnostic structs /////////////////////////////////////////////////////////

        //
        // Helper methods for texture formats
        //
        [Namespace]
        static partial class TextureFormat
        {
            // D32_FLOAT_S8X24_UINT, D24_UNORM_S8_UINT, D32_FLOAT, D24_UNORM_X8_UINT, D16_UNORM, S8_UINT
            static bool is_depth_stencil(Enum value)
            {
                return value == Enum.D32_FLOAT_S8X24_UINT || value == Enum.D24_UNORM_S8_UINT;
            }

            static bool is_depth_only(Enum value)
            {
                return value >= Enum.D32_FLOAT && value < Enum.S8_UINT;
            }

            static bool is_stencil_only(Enum value)
            {
                return value == Enum.S8_UINT;
            }

            static bool has_depth(Enum value)
            {
                return value >= Enum.D32_FLOAT && value < Enum.S8_UINT;
            }

            static bool has_stencil(Enum value)
            {
                return value == Enum.D32_FLOAT_S8X24_UINT || value == Enum.D24_UNORM_S8_UINT || value == Enum.S8_UINT;
            }
        }

        struct ResourceData
        {
            object data; // void* = nullptr;
        };

        //
        //
        struct ResourceBinding
        {
            u16 type; // = 0;    // ResourceType
            u16 start; // = 0;
            u16 count; // = 0;
            u16 set; // = 0;

            string name; //= nullptr;
        };

        // API-agnostic descriptions ////////////////////////////////////////////////////

        //
        //
        struct ShaderStateDescription
        {
            object native_handle; // void* = nullptr;
            string name; //= nullptr;
        };

        //
        //
        struct BufferDescription
        {
            object native_handle; // void* = nullptr;

            BufferType.Enum type; // = BufferType::Vertex;
            ResourceUsageType.Enum usage; // = ResourceUsageType::Immutable;
            u32 size; // = 0;
            string name; //= nullptr;
        };

        //
        //
        struct TextureDescription
        {
            object native_handle; // void* = nullptr;

            u16 width; // = 1;
            u16 height; // = 1;
            u16 depth; // = 1;
            byte mipmaps; // = 1;
            byte render_target; // = 0;

            TextureFormat.Enum format; // = TextureFormat::UNKNOWN;
            TextureType.Enum type; // = TextureType::Texture2D;
        };

        //
        //
        struct SamplerDescription
        {
            TextureFilter.Enum min_filter; // = TextureFilter::Nearest;
            TextureFilter.Enum mag_filter; // = TextureFilter::Nearest;
            TextureMipFilter.Enum mip_filter; // = TextureMipFilter::Nearest;

            TextureAddressMode.Enum address_mode_u; // = TextureAddressMode::Repeat;
            TextureAddressMode.Enum address_mode_v; // = TextureAddressMode::Repeat;
            TextureAddressMode.Enum address_mode_w; // = TextureAddressMode::Repeat;
        };

        //static const u32 k_max_resources_per_list = 32;

        //
        //
        struct ResourceListLayoutDescription
        {
            ResourceBinding[] bindings; // new ResourceBinding[k_max_resources_per_list];
            u32 num_active_bindings; // = 0;
        };

        //
        //
        struct ResourceListDescription
        {
            ResourceData[] resources; // new ResourceData[k_max_resources_per_list];
            u32 num_active_resources; // = 0;
        };

        //
        //
        struct PipelineDescription
        {
            ShaderHandle shader;
        };


        // API-agnostic resource modifications //////////////////////////////////////////

        class MapBufferParameters
        {
            BufferHandle buffer;
            u32 offset; // = 0;
            u32 size; // = 0;
        };

        class IReadOnlyMapBufferParameters : MapBufferParameters
        {
        }


        // API-gnostic resources ////////////////////////////////////////////////////////

        // #ifdef HYDRA_OPENGL
        // struct ShaderStateGL;
        // struct TextureGL;
        // struct BufferGL;
        // struct PipelineGL;
        // struct SamplerGL;
        // struct ResourceListLayoutGL;
        // struct ResourceListGL;
        // struct RenderPassGL;
        // struct DeviceStateGL;

        // #define ShaderState ShaderStateGL
        // #define TextureAPIGnostic TextureGL
        // #define BufferAPIGnostic BufferGL
        // #define PipelineAPIGnostic PipelineGL
        // #define SamplerAPIGnostic SamplerGL
        // #define ResourceListLayoutAPIGnostic ResourceListLayoutGL
        // #define ResourceListAPIGnostic ResourceListGL
        // #define RenderPassAPIGnostic RenderPassGL

        // #endif // HYDRA_OPENGL

        struct ShaderStateRHI
        {
        }

        struct IReadOnlyShaderStateRHI
        {
        }

        struct TextureRHI
        {
        }

        struct IReadOnlyTextureRHI
        {
        }

        struct BufferRHI
        {
        }

        struct IReadOnlyBufferRHI
        {
        }

        struct PipelineRHI
        {
        }

        struct IReadOnlyPipelineRHI
        {
        }

        struct SamplerRHI
        {
        }

        struct IReadOnlySamplerRHI
        {
        }

        struct ResourceListLayoutRHI
        {
        }

        struct IReadOnlyResourceListLayoutRHI
        {
        }

        struct ResourceListRHI
        {
        }

        struct IReadOnlyResourceListRHI
        {
        }

        struct RenderPassRHI
        {
        }

        struct IReadOnlyRenderPassRHI
        {
        }

        struct DeviceStateRHI
        {
        }

        struct IReadOnlyDeviceStateRHI
        {
        }


        // Main structs /////////////////////////////////////////////////////////////////

        // Forward-declarations /////////////////////////////////////////////////////////

        struct ResourcePool
        {
            delegate void init(u32 pool_size, u32 resource_size);

            delegate void terminate();

            delegate u32 obtain_resource();

            delegate void release_resource(u32 handle);

            delegate object access_resource(u32 handle);

            List<byte> memory; // = nullptr;
            List<u32> free_indices; // = nullptr;

            u32 free_indices_head; // = 0;
            u32 size; // = 16;
            u32 resource_size; // = 4;
        };

        class Device
        {
            // Init/Terminate methods
            delegate void init(IReadOnlyDeviceCreation creation);

            delegate void terminate();

            // Creation/Destruction of resources ////////////////////////////////////////
            delegate BufferHandle create_buffer(IReadOnlyBufferCreation creation);

            delegate TextureHandle create_texture(IReadOnlyTextureCreation creation);

            delegate PipelineHandle create_pipeline(IReadOnlyPipelineCreation creation);

            delegate SamplerHandle create_sampler(IReadOnlySamplerCreation creation);

            delegate ResourceListLayoutHandle create_resource_list_layout(IReadOnlyResourceListLayoutCreation creation);

            delegate ResourceListHandle create_resource_list(IReadOnlyResourceListCreation creation);

            delegate RenderPassHandle create_render_pass(IReadOnlyRenderPassCreation creation);

            delegate void destroy_buffer(BufferHandle buffer);

            delegate void destroy_texture(TextureHandle texture);

            delegate void destroy_pipeline(PipelineHandle pipeline);

            delegate void destroy_sampler(SamplerHandle sampler);

            delegate void destroy_resource_list_layout(ResourceListLayoutHandle resource_layout);

            delegate void destroy_resource_list(ResourceListHandle resource_list);

            delegate void destroy_render_pass(RenderPassHandle render_pass);

            // Query Description ////////////////////////////////////////////////////////
            delegate void query_buffer(BufferHandle buffer, BufferDescription out_description);

            delegate void query_texture(TextureHandle texture, TextureDescription out_description);

            delegate void query_pipeline(PipelineHandle pipeline, PipelineDescription out_description);

            delegate void query_sampler(SamplerHandle sampler, SamplerDescription out_description);

            delegate void query_resource_list_layout(ResourceListLayoutHandle resource_layout, ResourceListLayoutDescription out_description);

            delegate void query_resource_list(ResourceListHandle resource_list, ResourceListDescription out_description);

            // Map/Unmap ////////////////////////////////////////////////////////////////
            delegate List<byte> map_buffer(IReadOnlyMapBufferParameters parameters);

            delegate void unmap_buffer(IReadOnlyMapBufferParameters parameters);

            // Command Buffers //////////////////////////////////////////////////////////
            delegate CommandBuffer get_command_buffer(QueueType.Enum type, u32 size, bool baked); // Request a command buffer with a certain size. If baked reset will affect only the read offset.

            delegate void free_command_buffer(CommandBuffer command_buffer);

            delegate void queue_command_buffer(CommandBuffer command_buffer); // Queue command buffer that will not be executed until present is called.

            // Rendering ////////////////////////////////////////////////////////////////
            delegate void present();

            delegate void resize(u16 width, u16 height);

            delegate void resize_output_textures(RenderPassHandle render_pass, u16 width, u16 height);

            delegate BufferHandle get_fullscreen_vertex_buffer(); // Returns a vertex buffer usable for fullscreen shaders that uses no vertices.

            delegate RenderPassHandle get_swapchain_pass(); // Returns what is considered the final pass that writes to the swapchain.

            delegate TextureHandle get_dummy_texture();

            delegate BufferHandle get_dummy_constant_buffer();

            // Internals ////////////////////////////////////////////////////////////////
            delegate void backend_init(IReadOnlyDeviceCreation creation);

            delegate void backend_terminate();

            ResourcePool buffers;
            ResourcePool textures;
            ResourcePool pipelines;
            ResourcePool samplers;
            ResourcePool resource_list_layouts;
            ResourcePool resource_lists;
            ResourcePool render_passes;
            ResourcePool command_buffers;
            ResourcePool shaders;

            // Primitive resources
            BufferHandle fullscreen_vertex_buffer;

            RenderPassHandle swapchain_pass;

            // Dummy resources
            TextureHandle dummy_texture;
            BufferHandle dummy_constant_buffer;

            List<CommandBuffer> queued_command_buffers;
            u32 num_allocated_command_buffers; // = 0;
            u32 num_queued_command_buffers; // = 0;

            u16 swapchain_width; // = 1;
            u16 swapchain_height; // = 1;

            delegate ShaderStateRHI access_shader(ShaderHandle shader);

            delegate IReadOnlyShaderStateRHI access_const_shader(ShaderHandle shader);

            delegate TextureRHI access_texture(TextureHandle texture);

            delegate IReadOnlyTextureRHI access_const_texture(TextureHandle texture);

            delegate BufferRHI access_buffer(BufferHandle buffer);

            delegate IReadOnlyBufferRHI access_const_buffer(BufferHandle buffer);

            delegate PipelineRHI access_pipeline(PipelineHandle pipeline);

            delegate IReadOnlyPipelineRHI access_const_pipeline(PipelineHandle pipeline);

            delegate SamplerRHI access_sampler(SamplerHandle sampler);

            delegate IReadOnlySamplerRHI access_const_sampler(SamplerHandle sampler);

            delegate ResourceListLayoutRHI access_resource_list_layout(ResourceListLayoutHandle resource_layout);

            delegate IReadOnlyResourceListLayoutRHI access_const_resource_list_layout(ResourceListLayoutHandle resource_layout);

            delegate ResourceListRHI access_resource_list(ResourceListHandle resource_list);

            delegate IReadOnlyResourceListRHI access_const_resource_list(ResourceListHandle resource_list);

            delegate RenderPassRHI access_render_pass(RenderPassHandle render_pass);

            delegate IReadOnlyRenderPassRHI access_const_render_pass(RenderPassHandle render_pass);

            // API (OpenGL) Specific resource methods. Ideally they should not exist, but needed.

            // Shaders are considered resources in the OpenGL device!
            delegate ShaderHandle create_shader(IReadOnlyShaderCreation creation);

            delegate void destroy_shader(ShaderHandle shader);

            delegate void query_shader(ShaderHandle shader, ShaderStateDescription out_description);

            // Specialized methods
            delegate void link_texture_sampler(TextureHandle texture, SamplerHandle sampler);

            DeviceStateRHI device_state; // = nullptr;
        };

        class IReadOnlyDevice : Device
        {
        }


        // Commands list ////////////////////////////////////////////////////////////////

        namespace commands
        {
            class Command()
            {
                u16 type = 0;
                u16 size = 0;
            };

            class BindPipeline : Command
            {
                PipelineHandle handle;

                static u16 Type()
                {
                    return (u16)CommandType.Enum.BindPipeline;
                }
            };

            class BeginPass : Command
            {
                RenderPassHandle handle;

                static u16 Type()
                {
                    return (u16)CommandType.Enum.BeginPass;
                }
            };

            class EndPass : Command
            {
                static u16 Type()
                {
                    return (u16)CommandType.Enum.EndPass;
                }
            };

            class BindResourceList : Command
            {
                ResourceListHandle[] handles; //[k_max_resource_layouts];
                u32[] offsets; // [k_max_resource_layouts];

                u32 num_lists;
                u32 num_offsets;

                static u16 Type()
                {
                    return (u16)CommandType.Enum.BindResourceSet;
                }
            };

            class BindVertexBuffer : Command
            {
                BufferHandle buffer;
                u32 binding;
                u32 byte_offset;

                static u16 Type()
                {
                    return (u16)CommandType.Enum.BindVertexBuffer;
                }
            };

            class BindIndexBuffer : Command
            {
                BufferHandle buffer;

                static u16 Type()
                {
                    return (u16)CommandType.Enum.BindIndexBuffer;
                }
            };


            class Draw : Command
            {
                TopologyType.Enum topology;
                u32 first_vertex;
                u32 vertex_count;
                u32 instance_count;

                static u16 Type()
                {
                    return (u16)CommandType.Enum.Draw;
                }
            };

            class DrawIndexed : Command
            {
                TopologyType.Enum topology;
                u32 index_count;
                u32 instance_count;
                u32 first_index;
                s32 vertex_offset;
                u32 first_instance;

                static u16 Type()
                {
                    return (u16)CommandType.Enum.DrawIndexed;
                }
            };

            class DrawInstanced : Command
            {
                static u16 Type()
                {
                    return (u16)CommandType.Enum.DrawInstanced;
                }
            };

            class DrawIndexedInstanced : Command
            {
                static u16 Type()
                {
                    return (u16)CommandType.Enum.DrawIndexedInstanced;
                }
            };

            class Dispatch : Command
            {
                u16 group_x;
                u16 group_y;
                u16 group_z;

                static u16 Type()
                {
                    return (u16)CommandType.Enum.Dispatch;
                }
            };

            class CopyResource : Command
            {
                static u16 Type()
                {
                    return (u16)CommandType.Enum.CopyResource;
                }
            };

            class SetViewport : Command
            {
                Viewport viewport;

                static u16 Type()
                {
                    return (u16)CommandType.Enum.SetViewport;
                }
            };

            class SetScissor : Command
            {
                Rect2D rect;

                static u16 Type()
                {
                    return (u16)CommandType.Enum.SetScissor;
                }
            };

            class Clear : Command
            {
                float4 clear_color;

                static u16 Type()
                {
                    return (u16)CommandType.Enum.Clear;
                }
            };

            class ClearDepth : Command
            {
                float value;

                static u16 Type()
                {
                    return (u16)CommandType.Enum.ClearDepth;
                }
            };

            class ClearStencil : Command
            {
                byte value;

                static u16 Type()
                {
                    return (u16)CommandType.Enum.ClearStencil;
                }
            };

            struct SubmitHeader
            {
                u32 sentinel;
                u32 data_size;
            };
        }

        // CommandBuffer ////////////////////////////////////////////////////////////////

        //
        //
        struct SubmitCommand
        {
            u64 key;
            object data;
        };

        //
        //
        struct CommandKey
        {
        };

        //
        //
        struct CommandBuffer
        {
            delegate void init(QueueType.Enum type, u32 buffer_size, u32 submit_size, bool baked);

            delegate void terminate();

            //
            // Commands interface
            //

            delegate void begin_pass(RenderPassHandle handle);

            delegate void end_pass();

            delegate void begin_submit(u64 sort_key);

            delegate void end_submit();

            delegate void bind_pipeline(PipelineHandle handle);

            delegate void bind_vertex_buffer(BufferHandle handle, u32 binding, u32 offset);

            delegate void bind_index_buffer(BufferHandle handle);

            delegate void bind_resource_list(ResourceListHandle handles, u32 num_lists, u32[] offsets, u32 num_offsets);

            delegate void set_viewport(IReadOnlyViewport viewport);

            delegate void set_scissor(IReadOnlyRect2D rect);

            delegate void clear(float red, float green, float blue, float alpha);

            delegate void clear_depth(float value);

            delegate void clear_stencil(byte value);

            delegate void draw(TopologyType.Enum topology, u32 start, u32 count, u32 instance_count = 0);

            delegate void drawIndexed(TopologyType.Enum topology, u32 index_count, u32 instance_count, u32 first_index, s32 vertex_offset, u32 first_instance);

            delegate void dispatch(u32 group_x, u32 group_y, u32 group_z);

            //
            // Internal interface, public T FunctionName<T>(T data)
            //
            delegate T write_command<T>();

            // Get command and proceed the reading
            delegate T read_command<T>();

            // Retrieve current command type
            delegate CommandType.Enum get_command_type();

            bool has_commands()
            {
                return write_offset > 0;
            }

            bool end_of_stream()
            {
                return read_offset >= write_offset;
            }

            delegate void reset();

            ResourceHandle handle;
            u32 swapchain_frame_issued;
            u32 frames_in_flight;

            SubmitCommand current_submit_command; // Cached submit command
            commands.SubmitHeader current_submit_header; // = nullptr;
            SubmitCommand submit_commands; // = nullptr;
            u32 num_submits; // = 0;
            u32 max_submits; // = 0;

            QueueType.Enum type; // = QueueType.Enum.Graphics;

            object data; // = nullptr;
            u32 read_offset; // = 0;
            u32 write_offset; // = 0;
            u32 buffer_size; // = 0;
            bool baked; // = false; // If baked reset will affect only the read of the commands.
        };
    }
}
