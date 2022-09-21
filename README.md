# BuildSystem.Core

Game data build-system written in .NET Core that uses C# as the meta-data.
You can use most of C# to initialize/construct data etc.. However once
the C# files that define the game-data are compiled and instanciated the
following things will be done by the BuildSystem:

- It will search for ``IDataUnit`` and will treat it as the root of the game-data
- It recognizes the following types:
  - ``FileId``; An Id that can be retrieved interacting with BigfileManager
  - ``FileIdList``; An array of ``FileId``
  - ``FRect``/``IRect``; floating point and integer rectangle (left, right, top, bottom) compound
  - ``FSize``/``Size``; floating point and integer size (width, height) compound
  - ``FVec2``/``FVec3``/``FVec4``;
  - ``FMat22``/``FMat33``/``FMat44``;
  - ``s8``/``s16``/``s32``/``s64``
  - ``u8``/``u16``/``u32``/``u64``
  - ``fx16``/``fx32``
  - ``f32``/``f64``; single and double float precision
  - ``Color``; 32-bit RGBA color
  - ``LString``; Localized string
 - Anything derived from ``IAtom`` (system types byte/short/..., see above)
 - Anything derived from ``ICompound`` (mapped to a struct or class in C++)
 - Anything derived from ``IDataCompiler``
  - ```AssetCompiler("objects/rock.asset", ESerialize.Reference / ESerialize.Embed)```
  - ```TextureCompiler("textures/logo.TGA", ETexFormat.BC5_UNORM_BLOCK)```
  - ```MaterialCompiler("materials/stone.mat")```
  - ```ShaderCompiler("shaders/shadow.vs", EShaderFormat.VS_SPIRV)```
  - ```ShaderCompiler("shaders/shadow.ps", EShaderFormat.PS_SPIRV)```

In your game runtime you can use the ``C++`` files ``GameData.h`` to load and use it.

