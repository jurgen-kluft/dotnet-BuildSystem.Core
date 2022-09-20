# Design

Game data with a FileId should not have to wait for any data compilation and so we should
focus on breaking this dependency.

- Requirements:
  - A FileId should be handed out immediately
  - Files are cooked from source folder `S:` to target folder `T:`
  - Handle tens of million of files (10 to 100 million)
  - File dependency tracking (bi-directional)
  - Out-Of-Core databases (due to large memory usage)
  - Compilers are preferred to be wrapped as a Mananaged .DLL to eliminate start-up time
    - e.g. Batch compiling shaders

Are we going to allow Compilers create new Compilers? For example a `.FBX` Compiler,
since such a file contains links to textures. Hmmm, we better have some intermediate format.
So for such formats we need a tool that converts the data to a "C#" file that embeds the
data and/or has Compilers to the Materials/Textures/TriData that it refers to.

```c++
struct assethdr_t
{
  u32 m_fileId_array_count;  
  // Followed by "m_fileId_array_count" * sizeof(u32) bytes
};
```

- Assumptions:
  1. All cooked files have a binary header `assethdr_t`
  2. The first 65536 FileId's are reserved and are used as `Tool Bundle` Ids
  3. Compilers always resolve to a main output file with 0 or more additional output files
     - Texture to Texture, e.g. `.PNG` to `.BC7`
     - Mesh to Mesh, e.g. `.FBX` to `.MDL` or `.PLY` to `.MDL`
     - ZoneCompiler, e.g. many input to many output, but there is one main `.zone` file
       that depends on all the many output files.
  4. A ToolBundle is a compiler together with all its .DLL's and configuration files.
     A ToolBundle has a unique FileId.
     A ToolBundle is hashed to know if it has changed.

Types:

- Hash(FilePath) -> 32 bytes (256-bit hash)
- FileId         -> 4 bytes
- Generation     -> 4 bytes
- File Size      -> 4 bytes
- Mod-Time       -> 8 bytes

Databases:

- FileIdAndStateDB (fixed key, fixed value)
  - Hash(FilePath) -> FileId, Generation, Size/Mod-Time
- FilePathDB (fixed key, dynamic value)
  - FileId -> FilePath
- DependingDB (fixed key, dynamic value)
  - FileId -> UsedBy:Array(FileId)
  - e.g. A texture used by many materials
- DependencyDB (fixed key, dynamic value)
  - FileId -> InOut:Array(FileId)
  - e.g. A ZoneCompiler

# Example

TextureCompiler(`S:textures/hello.png`, ETextureFormat.BC7);
TextureCompiler(`S:textures/hello.png`, ETextureFormat.SRGB);
TextureCompiler(`S:textures/albedo.png`, ETextureFormat.BC7);
TextureCompiler(`S:textures/roughness.png`, ETextureFormat.BC7);
TextureCompiler(`S:textures/metalness.png`, ETextureFormat.BC7);

*Name of the material is the hash that is generated from a string that consists of the texture names, texture formats and parameters*
*Should we include the parameters in the Hash?*
MaterialCompiler(
  Albedo = TextureCompiler(`S:textures/albedo.png`, ETextureFormat.BC7),
  Roughness = TextureCompiler(`S:textures/roughness.png`, ETextureFormat.BC7),
  Metalness = TextureCompiler(`S:textures/metalness.png`, ETextureFormat.BC7),
  {parameters}
)

-- FileIdAndStateDB
Hash(`S:textures/hello.png`), FileId(5000), 0, 60Kb, date+time
Hash(`D:textures/hello.png.bc7`), FileId(5001), 0, 60Kb, date+time
Hash(`D:textures/hello.png.srgb`), FileId(5002), 0, 50Kb, date+time
Hash(`D:textures/albedo.png.bc7`), FileId(5003), 0, 60Kb, date+time
Hash(`D:textures/roughness.png.bc7`), FileId(5004), 0, 60Kb, date+time
Hash(`D:textures/metalness.png.bc7`), FileId(5005), 0, 60Kb, date+time
Hash(`D:materials/30d116e7729c40e3854f426944c1992b.mtl`), FileId(5006), 0, 1Kb, date+time

-- FilePathDB
FileId(5000) -> `S:textures/hello.png`
FileId(5001) -> `D:textures/hello.png.bc7`
FileId(5002) -> `D:textures/hello.png.srgb`
FileId(5003) -> `D:materials/30d116e7729c40e3854f426944c1992b.mtl`

-- DependingDB
FileId(5001) -> [](FileId(5000))
FileId(5002) -> [](FileId(5000))
FileId(5006) -> [](FileId(5003),FileId(5004),FileId(5005))

-- DependencyDB
FileId(5000) -> [](FileId(5001), FileId(5002))
FileId(5003) -> [](FileId(5006))
FileId(5004) -> [](FileId(5006))
FileId(5005) -> [](FileId(5006))

-- Finding deleted/modified `D:` assets
So the game data should have a root object that has a `assethdr_t` that lists all its
dependencies. With this we could scan and collect all required assets and find any *missing*
assets/FileId's. This uses the FileIdAndStateDB.

-- Finding deleted/modified `S:` assets
When iterating over all the Compilers we can determine if any of the source/destination files
are missing/out-of-date.

-- Writing all Compilers in a database?
When compiling the game data units and using reflection to find all Compilers, could we then not
write all Compilers and their necessary variables to a database (including a Generation value)?

With this database we can iterate over all Compilers in this database and kick of the Compilers to
do the actual asset cooking.

The generation value of each Compiler is used to be able to 'ignore' old/stale entries that are not
valid anymore.

# Dependency Analysis

When does something in terms of a dependency chain need to be checked, like which object
really depends on another object?

Different compilers can use the same source file.

We know that the tools that are part of many compilers are a hard dependency, so when they change the assets that
use those compilers (might) need to be recooked (only if there is a serious bug in that compiler).

For assets, we have an `.FBX` file which outputs a `.MDL`, this model is using materials which in turn use textures.
If one of the textures changes do I need to recompile the model, in this scenario that doesn't seem to be the case.

But we could have a LevelCompiler that reads an `.FBX` file and outputs multiple `.zone_00_00` and `.pvs` files. 
When one of those zone files is deleted we need to rebuild. So each zone file is added to the `FileIdAndStateDB` 
database as well as the `DependingDB` database.

But this could result in `stale` entries so we need to find out how we can `compact` the database.

For audio, if audio files are cooked and the cooking is configured by a configuration file then when the configuration
changes to audio files need to be recooked.

So we have hard dependencies that can be described as:

- Tool Bundle: includes .exe and .config files


When building the big


# TODO

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
