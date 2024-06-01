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

namespace hydra
{
    using ResourceHandle = uint;

    struct float4
    {
        float x, y, z, w;
    };

    public struct mat4s
    {
        float[] m = new float[16];

        public mat4s()
        {
        }
    };

    public struct vec3s
    {
        float x, y, z;
    }

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


    //
    // Material/Shaders /////////////////////////////////////////////////////////////

    //
    // Struct used to retrieve textures and buffers.
    //
    struct ShaderResourcesDatabase
    {
        struct BufferStringMap
        {
            string key;
            BufferHandle value;
        };

        struct TextureStringMap
        {
            string key;
            TextureHandle value;
        };

        struct SamplerStringMap
        {
            string key;
            SamplerHandle value;
        };

        List<BufferStringMap> name_to_buffer;
        List<TextureStringMap> name_to_texture;
        List<SamplerStringMap> name_to_sampler;
    };

    //
    // Struct to link between a Shader Binding Name and a Resource. Used both in Pipelines and Materials.
    //
    class ShaderResourcesLookup
    {
        enum Specialization
        {
            Frame,
            Pass,
            View,
            Shader
        };

        struct NameMap
        {
            string key;
            string value;
        };

        struct SpecializationMap
        {
            string key;
            Specialization value;
        };

        List<NameMap> binding_to_resource;
        List<SpecializationMap> binding_to_specialization;
        List<NameMap> binding_to_sampler;
    };

    //
    //
    //
    class Texture
    {
        TextureHandle handle;
        string filename;
        uint pool_id;
    };

    //
    //
    //
    class ShaderEffectPass
    {
        graphics.PipelineCreation pipeline_creation;
        string name;

        PipelineHandle pipeline_handle;
        uint pool_id;
    };

    //
    //
    //
    class NameDataMap
    {
        string key;
        object value;
    };

    //
    //
    //
    class ShaderEffect
    {
        List<ShaderEffectPass> passes;

        ushort num_passes; // = 0;
        ushort num_properties; // = 0;
        uint local_constants_size; // = 0;

        string local_constants_default_data; // = nullptr;
        string properties_data; // = nullptr;

        NameDataMap name_to_property; // = nullptr;

        string name;
        string pipeline_name;
        uint pool_id; // = 0;
    };

    //
    //
    //
    class ShaderInstance
    {
        PipelineHandle pipeline;
        List<ResourceListHandle> resource_lists;
        uint num_resource_lists;
    }

    // Instances

    //
    //
    //
    class MaterialFile
    {
        struct Property
        {
            string name;
            string data;
        };

        struct Binding
        {
            string name;
            string value;
        };

        struct Header
        {
            byte num_properties;
            byte num_bindings;
            byte num_textures;
            byte num_sampler_bindings;
            string name;
            string hfx_filename;
        };

        Header header;
        Property property_array;
        Binding binding_array;
        Binding sampler_binding_array;
    }

    //
    //
    //
    class Material
    {
        // Runtime part
        ShaderInstance shader_instances;
        uint num_instances;

        // Loading/Editing part
        ShaderResourcesLookup lookups; // Per-pass resource lookup. Same count as shader instances.
        ShaderEffect effect;

        BufferHandle local_constants_buffer;
        string local_constants_data;

        string name;
        string loaded_string_buffer; // TODO: replace with global string repository!

        uint num_textures;
        uint pool_id;

        private List<Texture> textures;
    };


    // Render Pipeline //////////////////////////////////////////////////////////////


    //
    //
    //
    struct RenderStageMask
    {
        ulong value;
    };

    //
    // Encapsulate the rendering of anything that writes to one or ore Render Targets.
    //
    struct RenderStage
    {
        enum Type
        {
            Geometry,
            Post,
            PostCompute,
            Swapchain,
            Count
        };

        Type type; // = Count;

        List<TextureHandle> input_textures;
        List<TextureHandle> output_textures;

        TextureHandle depth_texture;

        float scale_x; // = 1.0f;
        float scale_y; // = 1.0f;
        ushort current_width; // = 1;
        ushort current_height; // = 1;
        byte num_input_textures; // = 0;
        byte num_output_textures; // = 0;
        RenderPassHandle render_pass;

        Material material; // = nullptr;
        RenderView render_view; // = nullptr;

        float4 clear_color;
        float clear_depth_value;
        byte clear_stencil_value;

        bool clear_rt;
        bool clear_depth;
        bool clear_stencil;
        bool resize_output;

        byte pass_index; // = 0;
        uint pool_id; // = 0xffffffff;
        ulong geometry_stage_mask; // Used to send render objects to the proper stage. Not used by compute or postprocess stages.

        List<IRenderManager> render_managers;
    };

    //
    // A full frame of rendering using RenderStages.
    //
    struct RenderPipeline
    {
        struct StageMap
        {
            string key;
            RenderStage value;
        };

        struct TextureMap
        {
            string key;
            TextureHandle value;
        };

        StageMap name_to_stage; // = nullptr;
        TextureMap name_to_texture; // = nullptr;

        ShaderResourcesDatabase resource_database;
        ShaderResourcesLookup resource_lookup;
    };

    //
    //
    //
    struct PipelineMap
    {
        string key;
        RenderPipeline value;
    }; // struct PipelineMap

    //
    //
    //
    struct RenderViewMap
    {
        string key;
        RenderView value;
    };

    // Geometry/Math/Utils //////////////////////////////////////////////////////////

    //
    // Color class that embeds color in a uint32.
    //
    struct ColorUint
    {
        uint abgr;

        const uint red = 0xff0000ff;
        const uint green = 0xff00ff00;
        const uint blue = 0xffff0000;
        const uint black = 0xff000000;
        const uint white = 0xffffffff;
        const uint transparent = 0x00000000;
    };

    //
    //
    struct Box
    {
        vec3s min;
        vec3s max;
    }

    //
    //
    struct Ray
    {
        vec3s origin;
        vec3s direction;
    };


    //
    // Mesh/models/scene ////////////////////////////////////////////////////////////

    //
    //
    struct SubMesh
    {
        uint start_index;
        uint end_index;

        List<BufferHandle> vertex_buffers;
        List<uint> vertex_buffer_offsets;
        BufferHandle index_buffer;

        Box bounding_box;
        Material material;
    };

    //
    //
    struct Mesh
    {
        List<SubMesh> sub_meshes;
    };

    //
    //
    struct RenderNode
    {
        Mesh mesh;
        uint node_id;
        uint parent_id;
    };

    //
    //
    struct RenderScene
    {
        IRenderManager render_manager;
        RenderStageMask stage_mask; // Used to bind the scene to one or more stages.
        BufferHandle node_transforms_buffer; // Shared buffers

        List<RenderNode> nodes;
        List<BufferHandle> buffers; // All vertex and index buffers are here. Accessors will reference handles coming from here.
        List<mat4s> node_transforms;
    };

    //
    // Camera/Views

    //
    // Camera struct - can be both perspective and orthographic.
    //
    struct Camera
    {
        mat4s view;
        mat4s projection;
        mat4s view_projection;

        vec3s position;
        vec3s right;
        vec3s direction;
        vec3s up;

        float yaw;
        float pitch;

        float near_plane;
        float far_plane;

        bool perspective;
        bool update_projection;
    }

    //
    // Render view is a 'contextualized' camera - a way of using the camera in the render pipeline.
    //
    struct RenderView
    {
        Camera camera;
        List<RenderScene> visible_render_scenes;
    }

    //
    // Renderers

    struct RenderContext
    {
        RenderView render_view;
        graphics.Device device;
        graphics.CommandBuffer commands;

        RenderScene render_scene_array;
        ushort start;
        ushort count;
        ushort stage_index;
    }

    //
    //
    //
    interface IRenderManager
    {
        void render(RenderContext render_context);
    }

    //
    //
    //
    struct SceneRenderer : IRenderManager
    {
        public void render(RenderContext render_context)
        {
        }

        public Material material;
    }

    //
    //
    //
    struct LineRenderer : IRenderManager
    {
        public void render(RenderContext render_context)
        {
        }

        BufferHandle lines_vb;
        BufferHandle lines_vb_2d;
        BufferHandle lines_cb;
        Material line_material;

        uint current_line_index;
        uint current_line_index_2d;
    }

    //
    //
    //
    struct LightingManager : IRenderManager
    {
        public void render(RenderContext render_context)
        {
        }

        BufferHandle lighting_cb;
        vec3s directional_light;
        vec3s point_light_position;
        float point_light_intensity;
        bool use_point_light;
    }
}
