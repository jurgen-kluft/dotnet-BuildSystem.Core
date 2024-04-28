
namespace gamelogic
{
    public interface Math
    {
        public static float NormalizeAndReturnLength(ref Float3 v) => 0;
        public static Float3 Mul(float a, Float3 v) => new Float3() { X = a * v.X, Y = a * v.Y, Z = a * v.Z };
        public static Float3 Mul(Float3 a, Float3 v) => new Float3() { X = a.X * v.X, Y = a.Y * v.Y, Z = a.Z * v.Z };

        public static Float3 Add(Float3 a, Float3 b) => new Float3() { X = a.X + b.X, Y = a.Y + b.Y, Z = a.Z + b.Z };

        public static Float3x3 Identity3x3 = new Float3x3() { M0 = new Float3() { X = 1, Y = 0, Z = 0 }, M1 = new Float3() { X = 0, Y = 1, Z = 0 }, M2 = new Float3() { X = 0, Y = 0, Z = 1 } };
        public static Float4x4 Identity4x4 = new Float4x4() { M0 = new Float4() { X = 1, Y = 0, Z = 0, W = 0 }, M1 = new Float4() { X = 0, Y = 1, Z = 0, W = 0 }, M2 = new Float4() { X = 0, Y = 0, Z = 1, W = 0 }, M3 = new Float4() { X = 0, Y = 0, Z = 0, W = 1 } };
    }

    public interface IEngineSystem {}

    public interface IEntityResource {}
    
    public struct EntityId { public long Id; }

    // An Entity System is responsible for creating, updating and destroying specific type of entities.
    public interface IEntitySystem
    {
        public void Initialize(IEntityComponentSystem ecs);

        public void UpdateCreate(float deltaTime); // Finalize the requests to create sounds, smells and visuals
        public void UpdateDynamics(float deltaTime);
        public void UpdateLogic(float deltaTime);
    }

    public interface IRenderSystem : IEntitySystem
    {
        public RenderInstanceId CreateLight(LightResourceId resourceId);
        public RenderInstanceId CreateMesh(MeshResourceId resourceId);
        public RenderInstanceId CreateVisualFx(VisualFxResourceId resourceId);

        // Other functions like: Hide/Show, SetPosition, SetRotation, SetScale, SetColor, SetIntensity, SetRange

        public void UpdateCreate(float deltaTime); // Finalize the requests to create meshes and visual effects
        public void UpdateDynamics(float deltaTime); // Update movement, positions and rotations
        public void UpdateLogic(float deltaTime); // Update any rendering logic
    }

    public interface ISoundSystem : IEntitySystem
    {
        public SoundInstanceId CreateSound(SoundResourceId resourceId, Float3 position, float volume, float range);

        // Other functions like: Play, Stop, Pause, SetVolume, SetRange, SetPosition

        public void UpdateCreate(float deltaTime); // Finalize the requests to create meshes and visual effects
        public void UpdateDynamics(float deltaTime); // Update movement, positions and rotations
        public void UpdateLogic(float deltaTime); // Update any rendering logic
    }

    public interface IPhysicsSystem : IEntitySystem
    {
        public RigidBodyId CreateStaticAabb(Float3 position, Float3 size);
        public RigidBodyId CreateDynamicAabb(Float3 position, Float3 size, float mass, float friction);
        public RigidBodyId CreateDynamicCapsule(Float3 position, float height, float width, float mass, float friction);

        // Other functions like: SetPosition, SetRotation, SetScale, SetMass, SetFriction, SetVelocity, SetAngularVelocity

        public void UpdateCreate(float deltaTime); // Finalize the requests to create rigid bodies
        public void UpdateDynamics(float deltaTime);
        public void UpdateLogic(float deltaTime);
    }

    public enum PerceptionType
    {
        Sound,
        Smell,
        Visual
    }


    public struct SoundStimulusResourceId { public long Id; }
    public struct SmellStimulusResourceId { public long Id; }
    public struct VisualStimulusResourceId { public long Id; }
    public struct StimuliInstanceId { public long Id; }

    public interface IPerceptionSystem : IEntitySystem
    {
        public StimuliInstanceId CreateSoundStimuli(SoundStimulusResourceId stimulus, Float3 position, float volume, float range, float duration);
        public StimuliInstanceId CreateSmellStimuli(SmellStimulusResourceId stimulus, Float3 position, float strength, float range, float duration);
        public StimuliInstanceId CreateVisualStimuli(VisualStimulusResourceId stimulus, Float3 position, Float3 direction, float speed, float size);

        public void RegisterReceiver(EntityId entity, PerceptionType types);        // Stimuli are send to the receiver through the IEventSystem

        public void UpdateCreate(float deltaTime); // Finalize the requests to create sounds, smells and visuals
        public void UpdateDynamics(float deltaTime); // Update the stimuli, fading etc..
        public void UpdateLogic(float deltaTime); // Update the perception logic
    }


    // The IEventSystem is responsible for distributing events to game entities.
    public struct PropertyId { public long Id; }
    public struct EventId { public long Id; }

    public struct EventType
    {
    }

    public interface IEventSystem : IEngineSystem
    {
        public PropertyId RegisterEntityIdProperty(string name);
        public PropertyId RegisterFloatProperty(string name);
        public PropertyId RegisterIntProperty(string name);

        // Standard properties
        public static PropertyId FromProperty;

        public EventId BeginEventWriting();

        public void WriteEventEntityId(PropertyId propertyId, EntityId entityId);
        public void WriteEventPropertyFloat(PropertyId propertyId, float value);
        public void WriteEventPropertyInt(PropertyId propertyId, int value);

        public void EndEventWriting();
        public void BeginEventReading(EventId eventId);
        public EntityId ReadEventPropertyAsEntityId();
        public float ReadEventPropertyAsFloat();
        public int ReadEventPropertyAsInt();
        public void EndEventReading();

        public void SendEvent(EntityId to, EventId eventId, EventType eventType);
        public void RegisterEventReceiver(EntityId receiver, EventType e);
    }

    public interface IEntityComponent {}





    public struct Double3 { public double X, Y, Z; }
    public struct Double4 { public double X, Y, Z, W; }
    public struct Float3 { public float X, Y, Z; }
    public struct Float4 { public float X, Y, Z, W; }
    public struct Int3 { public int X, Y, Z; }
    public struct Int4 { public int X, Y, Z, W; }
    public struct Float3x3 { public Float3 M0, M1, M2; }
    public struct Float4x4 { public Float4 M0, M1, M2, M3; }

    public struct MeshResourceId { public long Id; }
    public struct LightResourceId { public long Id; }
    public struct VisualFxResourceId { public long Id; }
    public struct RenderInstanceId { public long Id; }

    public struct SoundResourceId { public long Id; }
    public struct SoundInstanceId { public long Id; }

    public struct RigidBodyId { public long Id; }

    public struct ExplosiveResourceId { public long Id; }



    public struct BulletId { public long Id; }

    public class BulletSystem : IEntitySystem
    {
        // A bullet is a fast moving projectile that can hit a target, we need to check for overlaps
        // with other entities and the world geometry.
        // So this requires 'continues collision detection', in one frame a bullet becomes a OOBB which
        // we query against the collision world.
        // The goal is to be able to process many bullets at the same time, so we need to be able to
        // process many bullets in parallel.

        public BulletId SpawnBullet(Float3 position, Float3 direction, float speed, float friction, float mass, float damage)
        {
            // Schedule: Create a bullet with initial position, direction, speed, friction, mass and damage
            return new BulletId() { Id = 0 };
        }

        public void Initialize(IEntityComponentSystem ecs)
        {
        }

        public void UpdateCreate(float deltaTime)
        {

        }

        public void UpdateDynamics(float deltaTime)
        {
        }

        public void UpdateLogic(float deltaTime)
        {
        }
    }


    public struct AabbComponent : IEntityComponent
    {
        public Float3 Position;
        public Float3 Size;
    }

    public class TriggerVolumeResource : IEntityResource
    {
        public Float3 Size;
        public EventType OnEnterEventType;
        public EventType OnLeaveEventType;
        public PropertyId TriggerOnEnterPropertyId;
        public PropertyId TriggerOnLeavePropertyId;
    }

    public class TriggerPropertiesComponent : IEntityComponent
    {
        public TriggerVolumeResource Resource;
    }

    public interface IEntity {}

    // A trigger volume is a 3D volume that can detect when an entity enters/touches/leaves it.
    public class TriggerVolume : IEntity
    {
        // Entity components
        public AabbComponent Aabb;
        public TriggerPropertiesComponent Properties;

        // Notes:
        // You want to execute some logic when an entity enters/leaves the trigger volume.

        // Example:
        // A door opens when a player enters a trigger volume and it closes when the player leaves.
        // How do we associate the trigger volume with the door and with the logic to open/close it?

        // Reasoning:
        // We could also have this trigger volume send an event to the event system when an entity enters/leaves it, but
        // we also include the target entity (the door).

        // Then we just need the DoorSystem to register itself to the IEventSystem and listen for the 'door open/close' event.
        // The DoorSystem::Update can then process the events and open/close a door by playing a door open/close animation.
        // The DoorSystem can have many different doors, and each door can have a different animation, sound, etc.

        public void OnEnter(EntityId entity, IEventSystem eventSystem)
        {
            var eventId = eventSystem.BeginEventWriting();
            eventSystem.WriteEventEntityId(IEventSystem.FromProperty, entity);
            eventSystem.WriteEventPropertyInt(Properties.Resource.TriggerOnEnterPropertyId, 1);
            eventSystem.EndEventWriting();
            eventSystem.SendEvent(entity, eventId, new EventType());
        }

        public void OnLeave(EntityId entity, IEventSystem eventSystem)
        {
            var eventId = eventSystem.BeginEventWriting();
            eventSystem.WriteEventEntityId(IEventSystem.FromProperty, entity);
            eventSystem.WriteEventPropertyInt(Properties.Resource.TriggerOnLeavePropertyId, 1);
            eventSystem.EndEventWriting();
            eventSystem.SendEvent(entity, eventId, new EventType());
        }
    }

    // A missile has a position, direction, velocity and can move. It also
    // has a visual representation (static mesh) and a collision volume.

    public class ParticleEffectResource : IEntityResource
    {
        public float Duration; // Duration of the particle effect
        public VisualFxResourceId FxId; // The visual effect to spawn
    }

    public class SoundResource : IEntityResource
    {
        public SoundResourceId SoundId; // The sound to play
        public float Volume; // Volume of the sound
        public float Range; // Range of the sound
        public float Duration; // Duration of the sound
    }
    public class ExplosiveResource : IEntityResource
    {
        public MeshResourceId MeshResourceId; // The mesh to render the explosive (landmine, C4, etc.
        public SoundResourceId SoundResourceId; // Sound Effect to Play upon impact
        public VisualFxResourceId VisualFxResourceId; // Visual Effect to Spawn upon impact
        public Float3 Size; // Size of the explosive
    }

    public class MotionComponent : IEntityComponent
    {
        public Float3 Direction;
        public float Velocity;
    }

    public class PropulsionComponent : IEntityComponent
    {
        // The force is applied in the traversal direction
        // A couple of parameters that describe the force attenuation curve over time
        public float Force; // e.g. 3000 N, thrust force of the missile
        public float Duration; // e.g. 600 s (fuel only lasts for 10 minutes)
    }

    public class GravityComponent : IEntityComponent
    {
        public float Gravity; // Gravity, e.g. 9.81 m/s^2
    }

    public class TransformComponent : IEntityComponent
    {
        public Float3x3 Transform;
        public Float3 Position;
    }

    public class MissilePropertiesComponent : IEntityComponent
    {
        public MissileResource Resource;
    }

    // The MissileResource is a resource that contains the missile's properties and can be used to spawn missiles.
    public class MissileResource : IEntityResource
    {
        public float Mass; // Mass of the missile
        public float Drag; // Drag coefficient of the missile in air
        public ExplosiveResourceId ExplosiveResource; // The explosive to spawn upon impact
    }

    public class MissileEntity : IEntity
    {
        public MotionComponent Motion;
        public TransformComponent Transform;
        public MissilePropertiesComponent Properties;
    }

    public struct EntityComponentId { public long Id; }
    public struct EntityFlagId { public long Id; }
    public struct EntityResourceId { public long Id; }

    public interface IEntityResourceSystem : IEngineSystem
    {
        public T GetResource<T>(EntityResourceId resourceId);
    }

    // An Entity Component System is the back-end/storage/owner, for entities, components and systems.
    public interface IEntityComponentSystem : IEngineSystem
    {
        public EntityId CreateEntity();
        public void DestroyEntity(EntityId entityId);

        public T AddComponent<T>(EntityId entityId);
        public bool HasComponent<T>(EntityId entityId);
        public T GetComponent<T>(EntityId entityId);
        public void RemoveComponent<T>(EntityId entity);
        
        public T AddFlag<T>(EntityId entity);
        public bool HasFlag<T>(EntityId entity);
        public bool GetFlag<T>(EntityId entity);
        public void RemoveFlag<T>(EntityId entity);

        public void AddSystem(IEntitySystem system);
        public void RemoveSystem(IEntitySystem system);

        public void UpdateCreate(float deltaTime);
        public void UpdateDynamics(float deltaTime);
        public void UpdateLogic(float deltaTime);
    }

    public class MissileEntitySystem : IEntitySystem
    {
        private IEntityComponentSystem _ecs; // The EntityComponentSystem
        private IEntityResourceSystem _ers; // The EntityResourceSystem
        private List<EntityId> _missileLaunchers = new List<EntityId>();
        private List<EntityId> _missiles = new List<EntityId>();

        public MissileEntitySystem(IEntityComponentSystem ecs, IEntityResourceSystem ers)
        {
            _ecs = ecs;
            _ers = ers;

            _ecs.AddSystem(this);
        }

        private void SpawnMissile(EntityResourceId missileResourceId, Float3 position, Float3 direction, float speed)
        {
            var missile = _ecs.CreateEntity();

            // Note: The ComponentIds can be cached for performance
            var motion = _ecs.AddComponent<MotionComponent>(missile);
            var transform = _ecs.AddComponent<TransformComponent>(missile);
            var propulsion = _ecs.AddComponent<PropulsionComponent>(missile);
            //var gravity = _ecs.AddComponent<GravityComponent>(missile);
            var properties = _ecs.AddComponent<MissilePropertiesComponent>(missile);

            var missileResource = _ers.GetResource<MissileResource>(missileResourceId);

            motion.Direction = direction;
            motion.Velocity = speed;

            transform.Transform = Math.Identity3x3;
            // need to set the position on the transform

            properties.Resource = missileResource;

            propulsion.Force = 3000;
            propulsion.Duration = 600;

            _missiles.Add(missile);
        }

        private void MoveMissile(EntityId missile, float deltaTime)
        {
            var g = new Float3() { X = 0, Y = -9.81f, Z = 0 };

            // Get the necessary components of the missile
            var motion = _ecs.GetComponent<MotionComponent>(missile);
            var transform = _ecs.GetComponent<TransformComponent>(missile);
            var propulsion = _ecs.GetComponent<PropulsionComponent>(missile);
            var gravity = _ecs.GetComponent<GravityComponent>(missile); //
            var properties = _ecs.GetComponent<MissilePropertiesComponent>(missile);

            // The missile moves in a direction with a velocity
            var position = Math.Add(transform.Position, Math.Mul(deltaTime, Math.Mul(motion.Velocity, motion.Direction)));

            // Calculate the direction since it is influenced by gravity
            motion.Direction = Math.Mul(motion.Velocity, motion.Direction);
            motion.Direction.Y += g.Y * deltaTime;
            motion.Velocity = Math.NormalizeAndReturnLength(ref motion.Direction);

            // Update transform
            transform.Position = position;
        }

        private void UpdateLauncher(EntityId missileLauncher)
        {
            // Check if the missile launcher should fire a missile
            // If so, spawn a missile
        }

        public EntityId SpawnMissileLauncher(EntityResourceId resourceId, Float3 position, Float3 direction)
        {
            // Create a missile launcher entity
            return new EntityId();
        }

        public void Initialize(IEntityComponentSystem ecs)
        {
        }

        public void UpdateCreate(float deltaTime)
        {
            // Handle any create requests
        }

        public void UpdateDynamics(float deltaTime)
        {
            // Update the missile launchers
            foreach (var missileLauncher in _missileLaunchers)
            {
                UpdateLauncher(missileLauncher);
            }

            // Update the missiles
            foreach (var missile in _missiles)
            {
                MoveMissile(missile, deltaTime);
            }


            // Missiles have moved, but any attached entities (Visual FX, Sound) are not updated.
            // We need to update the RenderSystem and SoundSystem to update the visual and sound effects.
        }

        public void UpdateLogic(float deltaTime)
        {

        }
    }

    public class MissileLauncherResource : IEntityResource
    {
        public MeshResourceId MeshResourceId; // The mesh to render the missile launcher
        public SoundResourceId SoundResourceId; // Sound Effect to Play upon firing
        public VisualFxResourceId VisualFxResourceId; // Visual Effect to Spawn upon firing
        public Float3 Size; // Size of the missile launcher
    }
    
    public class MissileLauncherComponent : IEntityComponent
    {
        public RenderInstanceId _meshInstanceId;
        public SoundInstanceId _soundInstanceId;
        public RenderInstanceId _visualFxInstanceId;
        public MissileResource _missileResource; // The missile resource to use for spawning missiles
        public MissileLauncherResource _resource;
    }

    // A Missile Launcher has a position, orientation and can fire missiles
    public class MissileLauncherEntity : IEntity
    {
        public TransformComponent _transform;
        public MissileLauncherComponent _properties;
    }
}
