# BuildSystem.Core

Game data build-system written in .NET Core that uses C# as the meta-data.
You can use most of C# to initialize/construct data etc.. However once
the C# files that define the game-data are compiled and instanciated the
following things will be done by the BuildSystem:

- It will search for IRoot and will treat it as the root of the game-data
- It recognizes the following types:
  - FileId; An Id that can be retrieved interacting with BigfileManager
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


TODO:

- Examples

