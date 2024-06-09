// ReSharper disable All

#pragma warning disable CS0649 // Field is never assigned to, and will always have its default value
#pragma warning disable CS8981 // The type name only contains lower-cased ascii characters. Such names may become reserved for the language.
#pragma warning disable CS0414 // Field is assigned but its value is never used
#pragma warning disable CS0169 // Field is never used

namespace hydra
{
    namespace ShaderModule
    {
        // Method attribute to mark a method as a shader
        public interface IShaderLayout
        {
        }

        [System.AttributeUsage(System.AttributeTargets.Class | AttributeTargets.Struct | AttributeTargets.Field)]
        public class uniformAttribute : System.Attribute
        {
        }

        [System.AttributeUsage(System.AttributeTargets.Class | AttributeTargets.Struct | AttributeTargets.Field)]
        public class layoutAttribute : System.Attribute
        {
            public layoutAttribute(string layout, int binding)
            {
            }

            public layoutAttribute(int binding)
            {
            }

            public layoutAttribute(int local_size_x, int local_size_y, int local_size_z)
            {
            }
        }

        [System.AttributeUsage(System.AttributeTargets.Field)]
        public class outputAttribute : System.Attribute
        {
        }

        [System.AttributeUsage(System.AttributeTargets.Field)]
        public class inputAttribute : System.Attribute
        {
        }

        [System.AttributeUsage(System.AttributeTargets.Field)]
        public class writeonlyAttribute : System.Attribute
        {
        }

        public struct sampler2D
        {
        }

        public struct image2D
        {
        }

        public struct _in_
        {
        }



    interface IGUIAttribute
    {
        Type Type { get; }
        string DisplayName { get; }
    }

    [AttributeUsage(AttributeTargets.Field)]
    class GUIFloat : Attribute, IGUIAttribute
    {
        public Type Type => typeof(float);
        public string DisplayName { get; init; }
        public float Value { get; init; }
        public float Min { get; init; }
        public float Max { get; init; }

        public GUIFloat(string displayName, float value, float min = 0.0f, float max = 1.0f)
        {
            DisplayName = displayName;
            Value = value;
            Min = min;
            Max = max;
        }

        public GUIFloat() : this("Float", 1.0f, 0.0f, 1.0f)
        {
        }

        public GUIFloat(string displayName) : this(displayName, 1.0f, 0.0f, 1.0f)
        {
        }
    }

    // A field attribute that indicates it must be exposed in the UI for the artist to tweak
    [AttributeUsage(AttributeTargets.Field)]
    class GUIInt : Attribute
    {
        public Type Type => typeof(int);
        public string DisplayName { get; init; }
        public float Min { get; init; } = 0;
        public float Max { get; init; } = 1;
        public float Value { get; init; } = 1;

        public GUIInt(string displayName, int value, int min = 0, int max = 1)
        {
            DisplayName = displayName;
            Value = value;
            Min = min;
            Max = max;
        }

        public GUIInt() : this("Int", 1, 0, 1)
        {
        }

        public GUIInt(string displayName) : this(displayName, 1, 0, 1)
        {
        }
    }
    }
}
