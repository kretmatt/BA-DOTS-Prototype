using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Physics;
using Unity.Physics.Systems;
using Unity.Transforms;
using UnityEngine;

/// <summary>
/// System for detecting collisions between players and boids / enemies
/// </summary>
[UpdateInGroup(typeof(FixedStepSimulationSystemGroup))]
[UpdateAfter(typeof(StepPhysicsWorld))]
[UpdateBefore(typeof(ExportPhysicsWorld))]
public partial class PlayerBoidCollisionSystem : SystemBase
{
    #region Variables

    ////////////////////////////////////////////////////////////////////
    /////////////////////////      Variables      //////////////////////
    ////////////////////////////////////////////////////////////////////

    /// <summary>
    /// CommandBufferSystem for executing commands (e.g. structural changes of entities) after the Simulation is complete
    /// </summary>
    private EndSimulationEntityCommandBufferSystem endSimulationECB;

    /// <summary>
    /// PhysicsWorld in which the collisions happen
    /// </summary>
    private StepPhysicsWorld stepPhysicsWorld;

    #endregion

    #region Methods

    ////////////////////////////////////////////////////////////////////
    /////////////////////////        Methods      //////////////////////
    ////////////////////////////////////////////////////////////////////

    /// <summary>
    /// Method that gets executed when the system starts running. RegisterPhysicsRuntimeSystemReadOnly is called because we only need to react to the collisions.
    /// </summary>
    protected override void OnStartRunning()
    {
        this.RegisterPhysicsRuntimeSystemReadOnly();
    }

    /// <summary>
    /// Method that gets executed when the system is created. Retrieves the EntityCommandBufferSystem instance and the PhysicsWorld instance where the collisions occur
    /// </summary>
    protected override void OnCreate()
    {
        endSimulationECB = World.GetOrCreateSystem<EndSimulationEntityCommandBufferSystem>();
        stepPhysicsWorld = World.GetOrCreateSystem<StepPhysicsWorld>();
    }

    /// <summary>
    /// Method that gets executed every frame. Checks if there are collisions between asteroids and projectiles. If so, the asteroids are damaged.
    /// </summary>
    protected override void OnUpdate()
    {
        var collJoib = new PlayerBoidCollisionJob
        {
            players = GetComponentDataFromEntity<PlayerTag>(true),
            boids = GetComponentDataFromEntity<GeneralBoidData>(),
            ecb = endSimulationECB.CreateCommandBuffer()
        };

        Dependency = collJoib.Schedule(stepPhysicsWorld.Simulation, Dependency);
        endSimulationECB.AddJobHandleForProducer(Dependency);
    }

    #endregion
}

/// <summary>
/// Job for detecting collisions between players and boids
/// </summary>
[BurstCompile]
public struct PlayerBoidCollisionJob : ICollisionEventsJob
{
    /// <summary>
    /// All players
    /// </summary>
    [ReadOnly] public ComponentDataFromEntity<PlayerTag> players;
    
    /// <summary>
    /// All boids / enemies
    /// </summary>
    public ComponentDataFromEntity<GeneralBoidData> boids;

    /// <summary>
    /// EntityCommandBuffer for registering commands that are then executed after the simulation has ended
    /// </summary>
    public EntityCommandBuffer ecb;

    /// <summary>
    /// Method for executing the job. Checks whether the entities that are part of the collision are an enemy / boid and a player
    /// </summary>
    /// <param name="collisionEvent">Collision event that occurred in the PhysicsWorld</param>
    public void Execute(CollisionEvent collisionEvent)
    {
        Entity entityA = collisionEvent.EntityA;
        Entity entityB = collisionEvent.EntityB;

        if (players.HasComponent(entityA) && players.HasComponent(entityB)) return;

        if (players.HasComponent(entityA) && boids.HasComponent(entityB))
        {
            GeneralBoidData boid;

            boids.TryGetComponent(entityB, out boid);

            DamageData damageDealt = new DamageData { damageDealt = boid.damage };

            ecb.DestroyEntity(entityB);
            ecb.AddComponent(entityA, damageDealt);
        }
        else if (players.HasComponent(entityB) && boids.HasComponent(entityA))
        {
            GeneralBoidData boid;

            boids.TryGetComponent(entityA, out boid);

            DamageData damageDealt = new DamageData { damageDealt = boid.damage };

            ecb.DestroyEntity(entityA);
            ecb.AddComponent(entityB, damageDealt);
        }
    }
}