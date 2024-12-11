// Copyright (c) J.J.Kluft. All Rights Reserved.
// Licensed under the Apache 2.0 License. See LICENSE.md in the project root for license information.

using GameCore;

namespace GameData
{

    public interface IGameDataWriter : IBinaryWriter
    {
        void Write(sbyte v);
        void Write(byte v);
        void Write(short v);
        void Write(ushort v);
        void Write(int v);
        void Write(uint v);
        void Write(long v);
        void Write(ulong v);
        void Write(float v);
        void Write(double v);
        void Write(byte[] data, int index, int count);
        void Write(string v);

        void WriteFileId(Hash160 signature);

    }

}
