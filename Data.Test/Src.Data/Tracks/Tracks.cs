using System;

namespace GameData
{
    public class Track : IDataUnit
    {
        public string Signature { get; set; }

        public ModelCompiler Model;
        public TextureCompiler Road;
    }

    public class Tracks
    {
        public Track[] tracks = new Track[]
        {
            new Track { Signature = "7f8cfa5c-4b10-41c2-aa33-ee4d68d1b8b1", Model = new ModelCompiler("Tracks\\Track1\\Track1.glTF"), Road = new TextureCompiler("Tracks\\Track1\\Road.png") },
            new Track { Signature = "73f6f11a-7e12-4e2b-b950-288e5e5e5264", Model = new ModelCompiler("Tracks\\Track2\\Track2.glTF"), Road = new TextureCompiler("Tracks\\Track2\\Road.png") },
            new Track { Signature = "1476ef93-dec6-41c3-9892-25f963a5d387", Model = new ModelCompiler("Tracks\\Track3\\Track3.glTF"), Road = new TextureCompiler("Tracks\\Track3\\Road.png") },
        };
    }
}
