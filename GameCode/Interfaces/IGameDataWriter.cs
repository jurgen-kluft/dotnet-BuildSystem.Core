// Copyright (c) J.J.Kluft. All Rights Reserved.
// Licensed under the Apache 2.0 License. See LICENSE.md in the project root for license information.

using GameCore;

namespace GameData
{

    public interface IGameDataWriter : IWriter
    {
        void Write(byte[] data, int index, int count);

        void WriteFileId(Hash160 signature);
    }
}
