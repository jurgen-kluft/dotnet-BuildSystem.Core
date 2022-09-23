# Design

Game data with a FileId should not have to wait for any data compilation and so we should
focus on breaking this dependency.

## Requirements

  - A FileId or HashId should be handed out immediately
  - Compilers always resolve to a main output file with 0 or more additional output files
  - Files are cooked from source folder `S:` to target folder `T:`
  - Handle a maximum of a hundred of million files (max 100 million files)
  - File dependency tracking
  - Compilers are preferred to be wrapped as a Mananaged .DLL to eliminate start-up time
    - e.g. Batch compiling shaders
  - A BuildMachine that is required to deal with the maximum number of files better have
    enough CPU memory (>= 128 GB)

Are we going to allow Compilers create new Compilers? For example a `.FBX` Compiler,
since such a file contains links to textures. Answer is NO, we better have some intermediate format.
So for such formats we need a tool that converts the data to a "C#" file that embeds the
data and/or has Compilers to the Materials/Textures/TriData that it refers to.

## Assumptions

  1. Compilers always resolve to a main output file with 0 or more additional output files
     - Texture to Texture, e.g. `.PNG` to `.BC7`
     - Mesh to Mesh, e.g. `.FBX` to `.MDL` or `.PLY` to `.MDL`
     - ZoneCompiler, e.g. many input to many output, but there is one main `.zone` file
       that depends on all the many output files.
  2. A ToolBundle is a compiler together with all its .DLL's and configuration files.
     A ToolBundle has a unique Id.
     A ToolBundle is hashed to know if it has changed.
  3. Our BuildSystem is the only application that executes the Compilers. In a multi-threaded
     environment care is taken to have multiple threads update files in source and target. 
     But since we are NOT reading/writing many files at once we can manage this through a
     custom interface.

## Types

- Hash() -> 24 bytes (192-bit hash)
- FileId -> 4 bytes

## Example

TextureCompiler(`textures/hello.png`, ETextureFormat.BC7);
TextureCompiler(`textures/hello.png`, ETextureFormat.SRGB);
TextureCompiler(`textures/albedo.png`, ETextureFormat.BC7);
TextureCompiler(`textures/roughness.png`, ETextureFormat.BC7);
TextureCompiler(`textures/metalness.png`, ETextureFormat.BC7);

MaterialCompiler(
  Albedo = TextureCompiler(`textures/albedo.png`, ETextureFormat.BC7),
  Roughness = TextureCompiler(`textures/roughness.png`, ETextureFormat.BC7),
  Metalness = TextureCompiler(`textures/metalness.png`, ETextureFormat.BC7),
  {parameters}
)

### A specialized (in-memory) Key/Value Store (maximum 100 Million entries)

- (can be written in C/C++ and through managed C# used in the BuildSystem)
- (using the RedBlack Tree algorithm to insert/find/remove)
- Hash -> Index
- Node16B { Parent4B, Child4B[2], EntryIndex4B }
- Entry32B { Hash24B, NodeIndex4B, Value4B }
- Virtual Memory Array of 'rbnode_t'
- Virtual Memory Array of 'entry_t'
- We do not need a freelist, we can always do a swap-remove!

### Our Pivot Point is the *Compiler*

So we have one or more binary files that contain a 'list' of Compilers that need to be or have been
executed. 

### Compilers

So a Compiler needs to be able to write out a representation of itself to a structured log as well
as being able to read itself from the structured log.

```c++
Compiler ("prototype of standard header")
{
    u32           binary_size;
    u32           compiler_index;
    u32           number_of_filenames;
    const char**  filenames;

    ... custom variables/data
};
```

### Finding deleted/modified `S:` and `T:` assets

When iterating over all the Compilers we can determine if any of the source/destination files
are missing/out-of-date.

### Writing all Compilers in a structured log (and reading)

When compiling the game data units and using reflection to find all Compilers, could we then not
write all Compilers and their necessary variables to a structured log. This structured log in terms 
of file size can become huge (4, 8, 16GB or more) but it never has to be fully loaded into memory.

With this log we can iterate over all Compilers and kick of the actual asset cooking. When we do
this cooking pass we can have the compilers write themselves (again) to a structured log, this time
with up-to-date information of the source and target files.

## Dependency Analysis

When does something in terms of a dependency chain need to be checked, like which object
really depends on another object?

Different compilers can use the same source file.

We know that the tools that are part of many compilers are a hard dependency, so when they change the assets that
use those compilers (might) need to be recooked (only if there is a serious bug in that compiler).

For assets, we have an `.FBX` file which outputs a `.MDL`, this model is using materials which in turn use textures.
If one of the textures changes do I need to recompile the model, in this scenario that doesn't seem to be the case.

But we could have a LevelCompiler that reads an `.FBX` file and outputs multiple `.zone_00_00` and `.pvs` files. 
When one of those zone files is deleted we need to rebuild. 

For audio, if audio files are cooked and the cooking is configured by a configuration file then when the configuration
changes to audio files need to be recooked.

So we have hard dependencies that can be described as:

- Tool Bundle: includes .exe and .config files

# Data Units

We would like to separate the project into compilation Units using the standard C# way using .csproj files and the .sln file.
We can reference other Units like

```c#
   public class AllCharacters
   {
       Hero = new DataUnit("characters/hero/");
       Enemies = new DataUnit[] {
          new DataUnit("characters/enemies/wolf/"),
          new DataUnit("characters/enemies/lion/"),
       };
   }
```

# TODO
  
- Examples
- FMat33, FMat44
- C# to C++ code (scripting for game-code or other necessary parts)

# FileId

FileId has changed to be a simple Index, it now has an extra 'Bigfile Index' indirection.

```c++
struct FileId
{
  u32 unit_index;
  u32 file_index;
};
```

# Bigfile Structure

## Bigfile bft/bfn/bfh

```c++

struct BigfileTOC
{
  u32 number_of_units;   // How many TOC units
  u32 offset_to_unit[];  // Offset to each TOC
  TOCEntry TOC[];        // Array of TOC Entry 
};

// Same structure will apply to the Bigfile Filename and Hash files
struct BigfileBFN
{
  u32 number_of_units;      // How many BFN units
  u32 offset_to_unit[];     // Offset to each unit
  u32 filenameOffsets[];    // Array of Offsets to filename
};

```