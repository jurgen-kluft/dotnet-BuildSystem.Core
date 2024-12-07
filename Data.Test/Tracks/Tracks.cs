using System;

namespace GameData
{
    public class Track  : IDataUnit
    {
        public string Name { get; set; }
        public ModelFile Model { get; set; }
        public TextureFile Road { get; set; }
    }

	public class Tracks
	{
		public string name { get { return "Tracks"; } }

		public Track[] tracks = new Track[] {
            new Track(Name = "Track1", Model = "Tracks\\Track1\\Track1.glTF", Road = "Tracks\\Track1\\Road.png"),
            new Track(Name = "Track2", Model = "Tracks\\Track2\\Track2.glTF", Road = "Tracks\\Track2\\Road.png"),
            new Track(Name = "Track3", Model = "Tracks\\Track3\\Track3.glTF", Road = "Tracks\\Track3\\Road.png"),
        };
	}
}
