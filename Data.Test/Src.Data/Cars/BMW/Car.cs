using System;

namespace GameData
{
    public partial class Cars : IDataUnit
	{
        public CarConfiguration GetBMWConfig()
        {
            return new CarConfiguration
            {
                Name = "BMW",
                Weight = 1500,
                MaxSpeed = 200,
                Acceleration = 10,
                Braking = 5,
                Cornering = 5,
                Stability = 5,
                Traction = 5
            };
        }
    }
}