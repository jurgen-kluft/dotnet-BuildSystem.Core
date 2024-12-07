using System;
using System.Collections.Generic;
using GameCore;

namespace GameData
{
    public sealed class CurveFile : IFile
    {
        public CurveFile(string path)
        {
            Path = path;
        }

        public string Path { get; }
    }

    public sealed class TextureFile : IFile
    {
        public TextureFile(string path)
        {
            Path = path;
        }
        public string Path { get; }
    }

    public sealed class ModelFile : IFile
    {
        public ModelFile(string path)
        {
            Path = path;
        }
        public string Path { get; }
    }

    public sealed class AudioFile : IFile
    {
        public AudioFile(string path)
        {
            Path = path;
        }
        public string Path { get; }
    }

    public sealed class FontFile : IFile
    {
        public FontFile(string path)
        {
            Path = path;
        }
        public string Path { get; }
    }

    public sealed class LocalizationFile : IFile
    {
        public LocalizationFile(string path)
        {
            Path = path;
        }
        public string Path { get; }
    }
}
