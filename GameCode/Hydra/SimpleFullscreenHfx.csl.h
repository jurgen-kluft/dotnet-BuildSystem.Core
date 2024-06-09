layout(std140, binding = 7) uniform LocalConstants {

  float scale;
  float modulo;
  float2 pad_tail;
}
local_constants;

layout(binding = 1) uniform sampler2D albedo_texture;
layout(rgba8, binding = 0) writeonly uniform image2D destination_texture;
layout(local_size_x = 32, local_size_y = 32, local_size_z = 1) in;
