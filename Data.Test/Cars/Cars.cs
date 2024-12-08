using System;

namespace GameData
{
    public class Car
    {
        public CarConfiguration Configuration { get; set; }
        public string ModelPath { get; set; }

        public Car(CarConfiguration config, string modelPath)
        {
            Configuration = config;
            ModelPath = modelPath;
        }
    }

    public partial class Cars : IDataUnit
    {
        public EDataUnit UnitType { get; } = EDataUnit.Embed;
        public string UnitId { get; } = "Cars-56e889c7-1051-4147-9544-c37ee7bc927e";

        public Car[] m_cars;

        public Cars()
        {
            m_cars = new Car[]
            {
                new(GetBMWConfig(), "Cars\\BMW\\BMW.glTF"), // BMW
                new(GetLexusConfig(), "Cars\\Lexus\\Lexus.glTF"), // Lexus
                new(GetMercedesConfig(), "Cars\\Mercedes\\Mercedes.glTF"), // Mercedes
            };
        }
    }

    public class CarConfiguration
    {
        public string Name { get; set; }
        public float Weight { get; set; }
        public float MaxSpeed { get; set; }
        public float Acceleration { get; set; }
        public float Braking { get; set; }
        public float Cornering { get; set; }
        public float Stability { get; set; }
        public float Traction { get; set; }
    };
}
