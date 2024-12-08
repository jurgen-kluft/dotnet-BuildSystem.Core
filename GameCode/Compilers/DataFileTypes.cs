using System;
using System.Collections.Generic;
using GameCore;

namespace GameData
{
    public sealed class CurveFile : FileIdPtr
    {
        public CurveFile(string path) : base(new CurveCompiler(path), typeof(Curve))
        {
        }
    }

    public sealed class TextureFile : FileIdPtr
    {
        public TextureFile(string path) : base(new TextureCompiler(path), typeof(Texture))
        {
        }
    }

    public sealed class ModelFile : FileIdPtr
    {
        public ModelFile(string path) : base(new ModelCompiler(path), typeof(Model))
        {
        }
    }

    public sealed class AudioFile : FileIdPtr
    {
        public AudioFile(string path) : base(new AudioCompiler(path), typeof(Audio))
        {
        }
    }

    public sealed class FontFile : FileIdPtr
    {
        public FontFile(string path) : base(new FontCompiler(path), typeof(Font))
        {
        }
    }

    public sealed class LocalizationFile : FileIdPtr
    {
        public LocalizationFile(string path) : base(new LocalizationCompiler(path), typeof(Localization))
        {
        }
    }
}
