using System;

namespace GameData
{
    public class Track : IDataUnit
    {
        public ModelCompiler Model;
        public TextureCompiler Road;
    }

    public class Tracks
    {
        public Track[] tracks = new Track[]
        {
            new Track { Model = new ModelCompiler("Tracks\\Track1\\Track1.glTF"), Road = new TextureCompiler("Tracks\\Track1\\Road.png") },
            new Track { Model = new ModelCompiler("Tracks\\Track2\\Track2.glTF"), Road = new TextureCompiler("Tracks\\Track2\\Road.png") },
            new Track { Model = new ModelCompiler("Tracks\\Track3\\Track3.glTF"), Road = new TextureCompiler("Tracks\\Track3\\Road.png") },
        };
    }
}
