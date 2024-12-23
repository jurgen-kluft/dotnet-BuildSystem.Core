using System;

namespace GameData
{
    public class Track : IDataUnit
    {
        public string Signature { get; set; }

        public ModelDataFile Model;
        public TextureFile Road;
    }

    public class Tracks
    {
        public Track[] tracks = new Track[]
        {
            new Track { Signature = "7f8cfa5c-4b10-41c2-aa33-ee4d68d1b8b1", Model = new ModelDataFile("Tracks\\Track1\\Track1.glTF"), Road = new TextureFile("Tracks\\Track1\\Road.png") },
            new Track { Signature = "73f6f11a-7e12-4e2b-b950-288e5e5e5264", Model = new ModelDataFile("Tracks\\Track2\\Track2.glTF"), Road = new TextureFile("Tracks\\Track2\\Road.png") },
            new Track { Signature = "1476ef93-dec6-41c3-9892-25f963a5d387", Model = new ModelDataFile("Tracks\\Track3\\Track3.glTF"), Road = new TextureFile("Tracks\\Track3\\Road.png") },
        };
    }
}
