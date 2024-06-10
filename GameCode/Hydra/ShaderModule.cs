// ReSharper disable All

using hydra.i32types;

#pragma warning disable CS0649 // Field is never assigned to, and will always have its default value
#pragma warning disable CS8981 // The type name only contains lower-cased ascii characters. Such names may become reserved for the language.
#pragma warning disable CS0414 // Field is assigned but its value is never used
#pragma warning disable CS0169 // Field is never used

namespace hydra
{
    namespace ShaderModule
    {
        // Method attribute to mark a method as a shader
        [System.AttributeUsage(System.AttributeTargets.Class | AttributeTargets.Struct | AttributeTargets.Field)]
        public class uniformAttribute : System.Attribute
        {
        }

        [System.AttributeUsage(System.AttributeTargets.Class | AttributeTargets.Struct | AttributeTargets.Field)]
        public class shaderAttribute : System.Attribute
        {
            public string Stage { get; private set; }

            public shaderAttribute(string stage)
            {
                Stage = stage;
            }
        }

        [System.AttributeUsage(System.AttributeTargets.Class | AttributeTargets.Struct | AttributeTargets.Field)]
        public class layoutAttribute : System.Attribute
        {
            public string Layout { get; private set; }
            public string Format { get; private set; }

            public layoutAttribute()
            {
                Layout = String.Empty;
            }

            public layoutAttribute(string layout)
            {
                Layout = layout;
            }
        }

        [System.AttributeUsage(System.AttributeTargets.Class | AttributeTargets.Struct | AttributeTargets.Field)]
        public class locationAttribute : System.Attribute
        {
            public int Location { get; private set; }

            public locationAttribute()
            {
                Location = 0;
            }

            public locationAttribute(int location)
            {
                Location = location;
            }
        }

        [System.AttributeUsage(System.AttributeTargets.Class | AttributeTargets.Struct | AttributeTargets.Field)]
        public class bindingAttribute : System.Attribute
        {
            public int Binding { get; private set; }

            public bindingAttribute()
            {
                Binding = 0;
            }

            public bindingAttribute(int binding)
            {
                Binding = binding;
            }
        }

        [System.AttributeUsage(System.AttributeTargets.Class | AttributeTargets.Struct | AttributeTargets.Field)]
        public class setAttribute : System.Attribute
        {
            public int Set { get; private set; }

            public setAttribute()
            {
                Set = 0;
            }

            public setAttribute(int set)
            {
                Set = set;
            }
        }

        [System.AttributeUsage(System.AttributeTargets.Class | AttributeTargets.Struct | AttributeTargets.Field)]
        public class formatAttribute : System.Attribute
        {
            public string Format { get; private set; }

            public formatAttribute(string format)
            {
                Format = format;
            }
        }

        [System.AttributeUsage(System.AttributeTargets.Class | AttributeTargets.Struct | AttributeTargets.Field)]
        public class dispatchAttribute : System.Attribute
        {
            public int x { get; private set; }
            public int y { get; private set; }
            public int z { get; private set; }

            public dispatchAttribute(int local_size_x, int local_size_y, int local_size_z)
            {
                x = local_size_x;
                y = local_size_y;
                z = local_size_z;
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


        [AttributeUsage(AttributeTargets.Field)]
        class guiAttribute : Attribute
        {
            public string DisplayName { get; init; }

            public guiAttribute()
            {
            }

            public guiAttribute(string displayName)
            {
                DisplayName = displayName;
            }
        }

        [AttributeUsage(AttributeTargets.Field)]
        class rangeAttribute : Attribute
        {
            public object Min { get; init; }
            public object Max { get; init; }

            public rangeAttribute(object min, object max)
            {
                Min = min;
                Max = max;
            }
        }
    }
}
