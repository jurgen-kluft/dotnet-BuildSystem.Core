layout(std140, binding = 7) uniform LocalConstants {

  float scale;
  float modulo;
  float pad_tail[2];
}
local_constants;

out vec4 vTexCoord;
