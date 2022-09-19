# BuildSystem.Core

Game data build-system written in .NET Core that uses C# as the meta-data.
You can use most of C# to initialize/construct data etc.. However once
the C# files that define the game-data are compiled and instanciated the
following things will be done by the BuildSystem:

- It will search for IRoot and will treat it as the root of the game-data
- It recognizes the following types:
  - ``FileId``; An Id that can be retrieved interacting with BigfileManager
  - FileIdList; An array of FileId's
  - FRect/IRect; floating point and integer rectangle (left, right, top, bottom) compound
  - FSize/Size; floating point and integer size (width, height) compound
  - FVector2/FVector3/FVector4;
  - sbyte/short/int/int64
  - sbyte/ushort/uint/uint64
  - fx16/fx32
  - float/double; single and double float precision
  - Color(RGBA); 32-bit RGBA color
  - LString; Localized string
 - Anything derived from IAtom (standard types, see above)
 - Anything derived from ICompound
 - Anything derived from IDataCompiler
  - ```AssetCompiler("objects/rock.asset", ESerialize.Reference / ESerialize.Embed)```
  - ```TextureCompiler("textures/logo.TGA", ETexFormat.BC5_UNORM_BLOCK)```
  - ```MaterialCompiler("materials/stone.mat")```
  - ```ShaderCompiler("shaders/shadow.vs", EShaderFormat.VS_SPIRV)```
  - ```ShaderCompiler("shaders/shadow.ps", EShaderFormat.PS_SPIRV)```

In your game runtime you can use the C++ files ``GameData.h`` to load and use it.


TODO

- Rethink the whole setup and steps to compile C# and compile the data.

- FilePath could be hashed and for both the 'source' and 'cooked' we could just substite a 64-bit hash
  and use that in the dependency file.
  For the final resolve to replace files with a fileid_t we can again use the hashes.

- Refactor 'Dependency' mechanism, could be a lot simpler and able to handle multi-threading so that
  we can launch DataCompilers on a job system to improve compilation performance.
  
  DataAssemblyManager::FileRegistrar should cache HashOf(Filename)->FileId and load it at each run so
  as to keep FileIds 'consistent', in there we should also store the HashOf(TimeStamp/Content). 
  When starting we can thus identify any 'changed'/'removed' dependency.

- Examples
- FMatrix3x3, FMatrix4x4
- C# to C++ code (scripting for game-code or other necessary parts)

