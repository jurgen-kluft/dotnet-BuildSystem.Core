using System;

namespace GameData.Cars
{
    public partial class Cars : IDataUnit
    {
        public string name { get { return "Cars"; } }

        public Car[] cars = new Car[] {
            new Car(BMWConfig, "Cars\\BMW\\BMW.glTF"),
            new Car(LexusConfig, "Cars\\Lexus\\Lexus.glTF"),
            new Car(MercedesConfig, "Cars\\Mercedes\\Mercedes.glTF"),
        };
    };

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
