using System;

namespace GameData.Cars
{
    public partial class Cars : IDataUnit
	{
        public CarConfiguration MercedesConfig = new CarConfiguration() {
            Name = "Mercedes",
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
