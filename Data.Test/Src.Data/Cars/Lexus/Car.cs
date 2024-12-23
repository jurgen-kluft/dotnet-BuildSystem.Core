using System;

namespace GameData
{
    public partial class Cars : IDataUnit
	{
        public CarConfiguration GetLexusConfig()
        {
            return new CarConfiguration()
            {
                Name = "Lexus",
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
