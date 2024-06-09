// ReSharper disable All

#pragma warning disable CS0649 // Field is never assigned to, and will always have its default value
#pragma warning disable CS8981 // The type name only contains lower-cased ascii characters. Such names may become reserved for the language.
#pragma warning disable CS0414 // Field is assigned but its value is never used
#pragma warning disable CS0169 // Field is never used

using hydra;
using hydra.ShaderModule;
using hydra.f32types;
using hydra.i32types;

namespace GameData
{
    // ====================================================================================================================================
    // ====================================================================================================================================
    // Definitions, Structures, and Enums
    // ====================================================================================================================================

    class ShaderEffect
    {
        public List<IShaderBinding> Layout { get; set; }
        public List<ShaderEffectPass> Passes { get; set; }
    }

    enum EBindingType
    {
        CBuffer,
        Texture2D,
        RwTexture2D,
    }

    interface IShaderBinding
    {
    }

    class Texture2DShaderBinding : IShaderBinding
    {
    }

    class RwTexture2DShaderBinding : IShaderBinding
    {
    }

    class ShaderEffectPass
    {
        public string Name { get; init; }
        public int3 ComputeDispatch { get; init; }
        public hydra.ShaderStage.Enum Stage { get; init; }
        public List<IShaderBinding> Resources { get; init; }
        public FileId Compute { get; init; }
        public FileId Vertex { get; init; }
        public FileId Fragment { get; init; }
    }


    // ====================================================================================================================================
    // ====================================================================================================================================
    // Shader Effect Declarations
    // ====================================================================================================================================


    class SimpleFullscreen : ShaderEffect
    {
        public struct LocalConstants
        {
            public LocalConstants(float _scale, float _modulo)
            {
                scale = _scale;
                modulo = _modulo;
            }

            [GUIFloat("scale", 16.0f, 0.0f, 1.0f)] public float scale = 16.0f;
            [GUIFloat("modulo")] public float modulo = 1.0f;
            public float pad_tail1, pad_tail2;
        }

        public struct LocalCompute : IShaderBinding
        {
            public LocalConstants local_constants;
            public Texture2DShaderBinding albedo_texture;
            public RwTexture2DShaderBinding destination_texture;
        }

        public struct Local : IShaderBinding
        {
            public Texture2DShaderBinding input_texture;
        }

        public SimpleFullscreen()
        {
            // For the developer
            var LocalCompute = new LocalCompute();
            var Local = new Local();

            Layout = new() { LocalCompute, Local };

            var computeShader = new FileId(new ShaderCompiler("SimpleFullScreenHfx.cs.glsl"));
            var vertexShader = new FileId(new ShaderCompiler("SimpleFullScreenHfx.vs.glsl"));
            var fragmentShader = new FileId(new ShaderCompiler("SimpleFullScreenHfx.ps.glsl"));

            // pipeline = computeTest

            var FillTexture = new ShaderEffectPass
            {
                Name = "FillTexture",
                ComputeDispatch = new int3 { x = 32, y = 32, z = 1 },
                Stage = hydra.ShaderStage.Enum.Compute,
                Resources = new List<IShaderBinding> { LocalCompute },
                Compute = computeShader
            };

            // pass FillTexture
            // {
            //     dispatch = 32, 32, 1
            //     stage = compute0
            //     resources = LocalCompute
            //     compute = ComputeTest
            // }

            var ToScreen = new ShaderEffectPass
            {
                Name = "ToScreen",
                Stage = hydra.ShaderStage.Enum.Fragment,
                Resources = new List<IShaderBinding> { Local },
                Vertex = vertexShader,
                Fragment = fragmentShader
            };

            // pass ToScreen
            // {
            //     stage = final
            //     resources = Local
            //     vertex = ToScreen
            //     fragment = ToScreen
            // }

            Passes = new() { FillTexture, ToScreen };
        }
    }
}
