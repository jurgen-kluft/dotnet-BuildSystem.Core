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
    using f32types;

    //
    // Camera/Views

    //
    // Camera struct - can be both perspective and orthographic.
    //
    struct Camera
    {
        mat4 view;
        mat4 projection;
        mat4 view_projection;

        vec3s position;
        vec3s right;
        vec3s direction;
        vec3s up;

        float yaw;
        float pitch;

        float near_plane;
        float far_plane;

        bool perspective;
        bool update_projection;
    }
}
