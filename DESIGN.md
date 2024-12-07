# Independence

We would like to be able to convert any piece of data with minimal effort. 
For example, take glTF, we want to convert it to a very specific format that we can use directly in our engine.
So in C# we do not need the data structures of the glTF, we just need to convert it to that format. Also we need to be able to handle the file input/output dependency in a more elegant way.

It would be very helpfull if we had a 'process' that could kick-off a process and know which files are written/created/read by that process.
This could be used for example on any process, like a shader compiler where when compiling a shader would detect all the header files that are read by the shader and then recompile the shader when any of the header files are changed.

# TODO
  
- More examples in Data.Test
- FMat33, FMat44
- C# to C++ code (scripting for game-code or other necessary parts)

- TextureCompiler supporting many output formats and a couple of input formats
- MeshCompiler, converting OBJ/PLY/STL to Binary Format (TRI)
