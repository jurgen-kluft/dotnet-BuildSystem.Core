//
//  Hydra Rendering - v0.1
//
//  Material Resource
//

// ReSharper disable All

using hydra.f32types;

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

    public interface IDescriptorType
    {
        string Layout { get; }
        string TypeName { get; }
        int Set { get; }
        int Binding { get; }
    }


    // Output Example: layout(set = 0, binding = 0, r32ui) uniform uimage2D storageImage;
    public class StorageImage : IDescriptorType
    {
        public string DescriptorTypeVulkan => "VK_DESCRIPTOR_TYPE_STORAGE_IMAGE";
        public string Layout => $"layout(set = {Set}, binding = {Binding}, {TextureFormat.GetLayoutName(LayoutTextureFormat)})";
        public string TypeName => "uimage2D";
        public TextureFormat.Enum LayoutTextureFormat => TextureFormat.Enum.R32_UINT;

        public int Set { get; init; } = 0;
        public int Binding { get; init; } = 0;
    }

    // Output Example: layout(set = 0, binding = 0) uniform sampler samplerDescriptor;
    public class Sampler : IDescriptorType
    {
        public string DescriptorTypeVulkan => "VK_DESCRIPTOR_TYPE_SAMPLER";
        public string Layout => $"layout(set = {Set}, binding = {Binding})";
        public string TypeName => "sampler";
        public int Set { get; init; } = 0;
        public int Binding { get; init; } = 0;
    }

    // Output Example: layout(set = 0, binding = 1) uniform texture2D sampledImage;
    public class Texture2D : IDescriptorType
    {
        public string DescriptorTypeVulkan => "VK_DESCRIPTOR_TYPE_SAMPLED_IMAGE";
        public string Layout => $"layout(set = {Set}, binding = {Binding})";
        public string TypeName => "texture2D";

        public int Set { get; init; } = 0;
        public int Binding { get; init; } = 0;
    }

    // Combined Image Sampler
    // VK_DESCRIPTOR_TYPE_COMBINED_IMAGE_SAMPLER
    // layout(set = 0, binding = 0) uniform sampler2D combinedImageSampler;
    public class Sampler2D : IDescriptorType
    {
        public string DescriptorTypeVulkan => "VK_DESCRIPTOR_TYPE_COMBINED_IMAGE_SAMPLER";
        public string Layout => $"layout(set = {Set}, binding = {Binding})";
        public string TypeName => "sampler2D";

        public int Set { get; init; } = 0;
        public int Binding { get; init; } = 0;
    }

    // Uniform Buffer
    // VK_DESCRIPTOR_TYPE_UNIFORM_BUFFER
    // layout(set = 0, binding = 0) uniform uniformBuffer {
    //     float a;
    //     int b;
    // } ubo;
    public class UniformBuffer : IDescriptorType
    {
        public string DescriptorTypeVulkan => "VK_DESCRIPTOR_TYPE_UNIFORM_BUFFER";
        public string Layout => $"layout(set = {Set}, binding = {Binding})";
        public string TypeName => "uniformBuffer";

        public int Set { get; init; } = 0;
        public int Binding { get; init; } = 0;
    }

    // Storage Buffer
    // VK_DESCRIPTOR_TYPE_STORAGE_BUFFER
    // layout(set = 0, binding = 0) buffer storageBuffer {
    //     float a;
    //     int b;
    // } ssbo;
    public class StorageBuffer : IDescriptorType
    {
        public string DescriptorTypeVulkan => "VK_DESCRIPTOR_TYPE_STORAGE_BUFFER";
        public string Layout => $"layout(set = {Set}, binding = {Binding})";
        public string TypeName => "storageBuffer";

        public int Set { get; init; } = 0;
        public int Binding { get; init; } = 0;
    }

    // Uniform Texel Buffer
    // VK_DESCRIPTOR_TYPE_UNIFORM_TEXEL_BUFFER
    // layout(set = 0, binding = 0) uniform textureBuffer uniformTexelBuffer;
    public class UniformTexelBuffer : IDescriptorType
    {
        public string DescriptorTypeVulkan => "VK_DESCRIPTOR_TYPE_UNIFORM_TEXEL_BUFFER";
        public string Layout => $"layout(set = {Set}, binding = {Binding})";
        public string TypeName => "textureBuffer";

        public int Set { get; init; } = 0;
        public int Binding { get; init; } = 0;
    }

    // Storage Texel Buffer
    // VK_DESCRIPTOR_TYPE_STORAGE_TEXEL_BUFFER
    // layout(set = 0, binding = 0, rgba8ui) uniform uimageBuffer storageTexelBuffer;
    public class StorageTexelBuffer : IDescriptorType
    {
        public string DescriptorTypeVulkan => "VK_DESCRIPTOR_TYPE_STORAGE_TEXEL_BUFFER";
        public string Layout => $"layout(set = {Set}, binding = {Binding}, {TextureFormat.GetLayoutName(LayoutTextureFormat)})";
        public string TypeName => "uimageBuffer";
        public TextureFormat.Enum LayoutTextureFormat => TextureFormat.Enum.R8G8B8A8_UINT;

        public int Set { get; init; } = 0;
        public int Binding { get; init; } = 0;
    }

    // Input Attachment
    // VK_DESCRIPTOR_TYPE_INPUT_ATTACHMENT
    // layout (input_attachment_index = 0, set = 0, binding = 0) uniform subpassInput inputAttachment;
    public class InputAttachment : IDescriptorType
    {
        public string DescriptorTypeVulkan => "VK_DESCRIPTOR_TYPE_INPUT_ATTACHMENT";
        public string Layout => $"layout (input_attachment_index = {InputAttachmentIndex}, set = {Set}, binding = {Binding})";
        public string TypeName => "subpassInput";
        public int InputAttachmentIndex => 0;

        public int Set { get; init; } = 0;
        public int Binding { get; init; } = 0;
    }

    // Push Constant
    // VK_DESCRIPTOR_TYPE_PUSH_CONSTANT
    // layout(push_constant, std430) uniform pc {
    //     layout(offset = 32) vec4 data;
    // };
    public class PushConstant : IDescriptorType
    {
        public string Layout => "layout(push_constant, std430)";
        public string TypeName => "pc";

        public int Set { get; init; } = 0;
        public int Binding { get; init; } = 0;
    }

    // Compute Input
    // layout (local_size_x = 256, local_size_y = 1, local_size_z = 1) in;
    public class ComputeIn : IDescriptorType
    {
        public string Layout => $"layout (local_size_x = {LocalSizeX}, local_size_y = {LocalSizeY}, local_size_z = {LocalSizeZ})";
        public string TypeName => "in";

        public int Set { get; init; } = 0;
        public int Binding { get; init; } = 0;
        public int LocalSizeX { get; init; } = 256;
        public int LocalSizeY { get; init; } = 1;
        public int LocalSizeZ { get; init; } = 1;

        public ComputeIn()
        {
        }

        public ComputeIn(int local_size_x, int local_size_y, int local_size_z)
        {
            LocalSizeX = local_size_x;
            LocalSizeY = local_size_y;
            LocalSizeZ = local_size_z;
        }
    }

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
    public interface IShaderResourceLayout
    {
    }

    public interface IResourceLayout
    {
        int Set { get; init; }
    }

    public class MyResourceLayout : IShaderResourceLayout
    {
        // All possible GLSL input types

        public vec3 position;
        public u32 set;
        public u32 type;
        public u32 count;
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
