
namespace GameData
{
    // An IResource serves the purpose of being able to separate structures in their own game data file section.

    // You could want to centralize the full 'array' of

    // An example of different resource categories that can be strongly regarded as such are:
    // - Rendering
    //     - Texture Resource (Such a resource still has a FileId to the actual texture content that needs to be loaded from a Bigfile)
    //     - Shader Resource
    //     - Buffers (Vertex, Index, )
    //     - VFX (ParticleSystem)
    // - Audio
    //     - Music
    //     - Sound Effect

    // Reasoning:
    // Q: Why would we want to use a specific IResource interface for this, why not just use something like a FileId?
    // A: Quick answer is that a FileId would unreasonably increase the amount of supposed resources, but many files are not actual
    //    resources, they are just content or the content of a resource. For example a texture file is not a resource, it is the data
    //    content for a texture resource. The texture resource is the actual resource that is used in the game.
    //
    // For an application, like a game, resources do have a counter-part which you could say is an instance of a resource. A single
    // resource could be referenced by multiple other resources. So how do we manage this during runtime, how do we know
    // if a resource already has been instantiated? This is where IResource comes in. The IResource has a uniquely packed
    // identifier that unpacked contains the following information:
    // - The Resource Section index
    // - The Resource Object index
    //
    // So in the runtime we can have a ResourceLibraryManager that manages all the Resource Sections and their Resource Objects and
    // this can be queried with IResource to get the actual Resource Object. This way we can obtain a reference to the actual
    // resource object, and we can check if the resource object has already been instantiated. If it has we can return the reference
    // to the already instantiated resource object.
    // When the game/application starts it can scan the directory for resource libraries and load their TOC into memory. When those
    // are in memory you can then create the ResourceLibraryManager and create an Array<ResourceSection> and for each ResourceSection
    // create an Array<ResourceObject>. Now whenever we encounter a IResource we can easily get the ResourceObject from the Resource
    // Library Manager.
    //
    // We could take this one step further, we can introduce a 'type' into the IResource, so now we can have a ResourceLibrary
    // per 'type'. So for example a ResourceLibrary that will only contain TextureResources, and another ResourceLibrary that will
    // only contain ShaderResources. This way we can easily manage the resources, and we can easily query the ResourceLibraryManager.
    //
    // How do we pack this info into 64 bits, the following layout could work:
    // - 16 bits for the ResourceSection index
    // - 16 bits for the ResourceType index
    // - 32 bits for the ResourceObject index

    public interface IResource : IStruct
    {
        ushort ResourceTypeId { get; }
        ushort ResourceSectionId { get; set; }
        uint ResourceObjectId { get; set; }
    }
}
