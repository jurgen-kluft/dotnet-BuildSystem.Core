# BuildSystem.Core

Game data build-system written in .NET Core that uses C# as meta-data.
You can use most of C# to initialize/construct data etc.. However once
the C# files that define the game-data are compiled and instanciated the
following things will be done by the BuildSystem:

- It will search for ``IDataRoot`` and will treat it as the root DataUnit
- It recognizes the following types:
  - ``data_unit_t<T>``; A reference to a ``IDataUnit`` that exists as a specific block/chunk in the GameData file.
  - ``data_t<T>``; An object that can be used to interact with BigfileManager to read data
  - ``bool``; Multiple booleans are combined and basically become `bits`
  - ``s8``/``s16``/``s32``/``s64``;
  - ``u8``/``u16``/``u32``/``u64``;
  - ``f32``/``f64``; single and double float precision
  - ``f32x2``/``f32x3``/``f32x4``;
  - ``FRect``/``IRect``; floating point and integer rectangle (left, right, top, bottom)
  - ``FSize``/``Size``; floating point and integer size (width, height) compound
  - ``FMat22``/``FMat33``/``FMat44``;
  - ``color_t``; 32-bit RGBA color
  - ``lstring_t``; Localized string
  - ``array_t<T>``; C style array of any type listed here
 - Anything derived from ``IDataFile``:
   - ```MeshCompiler("objects/rock.ply")```
   - ```TextureCompiler("textures/logo.TGA", ETexFormat.BC5_UNORM_BLOCK)```
   - ```MaterialCompiler("materials/stone.mat")```
   - ```ShaderCompiler("shaders/shadow.vs", EShaderFormat.VS_SPIRV)```
   - ```ShaderCompiler("shaders/shadow.ps", EShaderFormat.PS_SPIRV)```

In your game runtime you can use the `C++` library `cgamedata` to use it.

## WIP

Currently working on exporting C++ header files containing classes/structs that are direct
mirrors of their C# counterpart and map directly to the written data.
Working with the data in this way is a lot more convenient and easier to understand, but it 
does require that you 'recompile' your game executable (when the layout changes).
When only the data changes you do not need to recompile your application.
