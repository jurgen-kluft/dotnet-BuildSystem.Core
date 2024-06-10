using System.Reflection;
using System.Reflection.Metadata;
using System.Text.RegularExpressions;
using System.Runtime.InteropServices;

using GameCore;

namespace DataBuildSystem
{
    class Program
    {
        static int Main(string[] args)
        {
            var layout = new hydra.ShaderModule.SimpleDrawPBR.VertexShaderLayout();
            var scene_type = layout.scene.GetType();
            return 0;
        }
    }
}
