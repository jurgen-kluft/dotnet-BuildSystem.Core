using System;
using System.Collections.Generic;
using GameCore;

namespace GameData
{
    public sealed class CurveFile : FileIdPtr
    {
        public CurveFile(string path)
        {
            var compiler = new CurveCompiler(path);
            Compiler = compiler;
            Provider = compiler;
        }
    }

    public sealed class TextureFile : FileIdPtr
    {
        public TextureFile(string path)
        {
            var compiler = new TextureCompiler(path);
            Compiler = compiler;
            Provider = compiler;
        }
    }

    public sealed class ModelFile : FileIdPtr
    {
        public ModelFile(string path)
        {
            var compiler = new ModelCompiler(path);
            Compiler = compiler;
            Provider = compiler;
        }
    }

    public sealed class AudioFile : FileIdPtr
    {
        public AudioFile(string path)
        {
            var compiler = new AudioCompiler(path);
            Compiler = compiler;
            Provider = compiler;
        }
    }

    public sealed class FontFile : FileIdPtr
    {
        public FontFile(string path)
        {
            var compiler = new FontCompiler(path);
            Compiler = compiler;
            Provider = compiler;
        }
    }

    public sealed class LocalizationFile : FileIdPtr
    {
        public LocalizationFile(string path)
        {
            var compiler = new LocalizationCompiler(path);
            Compiler = compiler;
            Provider = compiler;
        }
    }
}
