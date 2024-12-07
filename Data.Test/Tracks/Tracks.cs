using System;

namespace GameData
{
    public class Track  : IDataUnit
    {
        public EDataUnit UnitType { get; } = EDataUnit.External;
        public string UnitID { get; init; }
        public string Name { get; set; }
        public ModelFile Model { get; set; }
        public TextureFile Road { get; set; }
    }

	public class Tracks
	{
		public string name { get { return "Tracks"; } }

		public Track[] tracks = new Track[] {
            new Track {UnitID = "Track1-56e889c7", Name = "Track1", Model = new ModelFile("Tracks\\Track1\\Track1.glTF"), Road = new TextureFile("Tracks\\Track1\\Road.png")},
            new Track{UnitID = "Track2-56e889c7", Name = "Track2", Model = new ModelFile("Tracks\\Track2\\Track2.glTF"), Road = new TextureFile("Tracks\\Track2\\Road.png")},
            new Track{UnitID = "Track3-56e889c7", Name = "Track3", Model = new ModelFile("Tracks\\Track3\\Track3.glTF"), Road = new TextureFile("Tracks\\Track3\\Road.png")},
        };
	}
}
