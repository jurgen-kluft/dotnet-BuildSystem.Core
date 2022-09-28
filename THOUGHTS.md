# BuildSystem.Core

- glTF to C# convertor
- Texture Converter
- Need to be able to embed an external file in game data (might come from a DataCompiler(texture / audio ?))
- A compiler that can generate a bigfile (e.g. bigfile)
  - Option; Reference/Embed
  - Can have external File-Id's ? (shared textures / music / skybox / characters)

## Objects (C# to C++/C)

- Components
  - CompRender
  - CompCollision
  - CompInteract
  - CompTakeDamage
- Settings
  - Common
  - Platform
- References
  - Mesh
  - Collision
- Event Management
  - OnFirstNotice
  - OnHit
  - OnDestroyed
  - OnInteraction
- Perception
  - Point-of-interest[]
  - Body-can-lean
  - Body-can-sit

## World

- Grid
  - Cell[]
    - Object[]
- Cell Resource/Instance
  - Multi-Instance
  - Group(s) selector
    - Scriptable
      - Distance (LOD)
      - Weather
      - Story
      - Entities
      - Audio / Music
      - Season
  - Stream IN/OUT
    - Scriptable

## Character

A character consists of many elements.

- Skeleton
- SkinnedMesh
- Collision
- AnimationGraph
- Locomotion
- Controller
- Event-Management
- Perception
- Interaction

