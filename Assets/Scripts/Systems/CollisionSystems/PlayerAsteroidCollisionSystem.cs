using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Physics;
using Unity.Physics.Systems;
using Unity.Transforms;
using UnityEngine;

/// <summary>
/// System for reacting to collisions between the player and asteroids
/// </summary>
[UpdateInGroup(typeof(FixedStepSimulationSystemGroup))]
[UpdateAfter(typeof(StepPhysicsWorld))]
[UpdateBefore(typeof(ExportPhysicsWorld))]
public partial class PlayerAsteroidCollisionSystem : SystemBase
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

    /// <summary>
    /// Helper time variable used for limiting the amount of times the player can be damaged by an asteroid in a second
    /// </summary>
    private float currTime = 0;
    
    /// <summary>
    /// Variable for limiting the amount of times the player can be damaged by an asteroid in a second
    /// </summary>
    private float damageEventsPerSecond = 5;

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
    /// Method that gets executed 5 times per second (otherwise a lot of collisions are registered, leading to performance issues). Checks if there are collisions between asteroids and players. If so, the players are damaged. 
    /// </summary>
    protected override void OnUpdate()
    {
        if (UnityEngine.Time.time >= currTime)
        {
            currTime += (1 / damageEventsPerSecond);

            var collJoib = new PlayerAsteroidCollisionJob
            {
                players = GetComponentDataFromEntity<PlayerTag>(true),
                asteroids = GetComponentDataFromEntity<AsteroidData>(),
                ecb = endSimulationECB.CreateCommandBuffer()
            };

            Dependency = collJoib.Schedule(stepPhysicsWorld.Simulation, Dependency);
            endSimulationECB.AddJobHandleForProducer(Dependency);
        }
    }

    #endregion
}

/// <summary>
/// Job for detecting collisions between asteroids and players
/// </summary>
[BurstCompile]
public struct PlayerAsteroidCollisionJob : ICollisionEventsJob
{
    /// <summary>
    /// All players
    /// </summary>
    [ReadOnly] public ComponentDataFromEntity<PlayerTag> players;
    
    /// <summary>
    /// All asteroids
    /// </summary>
    public ComponentDataFromEntity<AsteroidData> asteroids;

    /// <summary>
    /// EntityCommandBuffer for registering commands that are then executed after the simulation has ended
    /// </summary>
    public EntityCommandBuffer ecb;

    /// Method for executing the job. Checks whether the entities that are part of the collision are an asteroid and a player
    /// </summary>
    /// <param name="collisionEvent">Collision event that occurred in the PhysicsWorld</param>
    public void Execute(CollisionEvent collisionEvent)
    {
        
        Entity entityA = collisionEvent.EntityA;
        Entity entityB = collisionEvent.EntityB;

        if (players.HasComponent(entityA) && players.HasComponent(entityB)) return;

        if (players.HasComponent(entityA) && asteroids.HasComponent(entityB))
        {
            AsteroidData asteroid;

            asteroids.TryGetComponent(entityB, out asteroid);

            DamageData damageDealt = new DamageData { damageDealt = asteroid.damage };

            ecb.AddComponent(entityA, damageDealt);
        }
        else if (players.HasComponent(entityB) && asteroids.HasComponent(entityA))
        {
            AsteroidData asteroid;

            asteroids.TryGetComponent(entityA, out asteroid);

            DamageData damageDealt = new DamageData { damageDealt = asteroid.damage };

            ecb.AddComponent(entityB, damageDealt);
        }
    }
}