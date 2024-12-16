using System;

namespace GameData
{
    public class Car
    {
        public CarConfiguration Configuration;
        public ModelCompiler ModelPath;

        public Car(CarConfiguration config, string modelPath)
        {
            Configuration = config;
            ModelPath = new ModelCompiler(modelPath);
        }
    }

    public partial class Cars : IDataUnit
    {
        public string Signature => "918ca960-1ceb-45f8-96df-c29690bb8619";

        public Car[] cars;

        public Cars()
        {
            cars = new Car[]
            {
                new(GetBMWConfig(), "Cars\\BMW\\BMW.glTF"), // BMW
                new(GetLexusConfig(), "Cars\\Lexus\\Lexus.glTF"), // Lexus
                new(GetMercedesConfig(), "Cars\\Mercedes\\Mercedes.glTF"), // Mercedes
            };
        }
    }

    public class CarConfiguration
    {
        public string Name;
        public float Weight;
        public float MaxSpeed;
        public float Acceleration;
        public float Braking;
        public float Cornering;
        public float Stability;
        public float Traction;
    };
}
