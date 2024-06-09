//
//  Hydra Rendering - v0.1
//
//  High level rendering implementation based on Hydra Graphics library.
//

// ReSharper disable All

#pragma warning disable CS0649 // Field is never assigned to, and will always have its default value
#pragma warning disable CS8981 // The type name only contains lower-cased ascii characters. Such names may become reserved for the language.
#pragma warning disable CS0414 // Field is assigned but its value is never used
#pragma warning disable CS0169 // Field is never used


// Types
namespace hydra
{
    //
    // Camera/Views

    //
    // Camera struct - can be both perspective and orthographic.
    //
    struct Camera
    {
        f32types.mat4s view;
        f32types.mat4s projection;
        f32types.mat4s view_projection;

        f32types.vec3s position;
        f32types.vec3s right;
        f32types.vec3s direction;
        f32types.vec3s up;

        float yaw;
        float pitch;

        float near_plane;
        float far_plane;

        bool perspective;
        bool update_projection;
    }
}
