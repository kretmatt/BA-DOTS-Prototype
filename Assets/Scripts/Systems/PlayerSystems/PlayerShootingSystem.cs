using Unity.Entities;
using Unity.Transforms;
using Unity.Mathematics;
using System.Collections.Generic;
using UnityEngine;
using Unity.Jobs;
using Unity.Physics;

/// <summary>
/// System responsible for spawning projectiles if a player is trying to shoot
/// </summary>
public partial class PlayerShootingSystem : SystemBase
{
    #region Variables

    ////////////////////////////////////////////////////////////////////
    /////////////////////////      Variables      //////////////////////
    ////////////////////////////////////////////////////////////////////

    /// <summary>
    /// CommandBufferSystem for executing commands (e.g. structural changes of entities) after the Simulation is complete
    /// </summary>
    private BeginSimulationEntityCommandBufferSystem beginSimulationECB;
    
    /// <summary>
    /// Prefab of the projectile as an entity
    /// </summary>
    private Entity projectilePrefab;

    #endregion

    #region Methods

    ////////////////////////////////////////////////////////////////////
    /////////////////////////        Methods      //////////////////////
    ////////////////////////////////////////////////////////////////////

    /// <summary>
    /// Method that gets executed when the system is created. Used for retrieving the EntityCommandBufferSystem instance and for subscribing methods to events:
    /// 1. GAME_START event:
    ///    ToggleShooting - Enables the shooting controls of the player once the game starts
    /// 2. GAME_END event:
    ///    ToggleShooting - Enables the shooting controls of the player once the game ends
    /// </summary>
    protected override void OnCreate()
    {
        beginSimulationECB = World.GetOrCreateSystem<BeginSimulationEntityCommandBufferSystem>();
        EventManager.SubscribeMethodToEvent(EEventType.GAME_START, ToggleShooting);
        EventManager.SubscribeMethodToEvent(EEventType.GAME_END, ToggleShooting);
    }

    /// <summary>
    /// Method for toggling the canShoot flag of the players once the game starts
    /// </summary>
    /// <param name="message">Message from the EventManager class</param>
    void ToggleShooting(Dictionary<string, object> message)
    {
        if(message!=null && message.ContainsKey("state"))
        {
            bool canShoot = (bool)message["state"];

            JobHandle toggleShooting = Entities.ForEach((ref PlayerShootData playerShootData) => {
                playerShootData.canShoot = !canShoot;
            }).ScheduleParallel(Dependency);
            toggleShooting.Complete();
        }
        else
        {
            JobHandle toggleShooting = Entities.ForEach((ref PlayerShootData playerShootData) => {
                playerShootData.canShoot = !playerShootData.canShoot;
            }).ScheduleParallel(Dependency);
            toggleShooting.Complete();
        }
    }

    /// <summary>
    /// Method that gets executed every frame.
    /// Retrives the projectile prefab.
    /// If a player can shoot and is trying to shoot, two projectiles (from the two cannons of the spaceship) are spawned and launched in the direction the player is facing.
    /// </summary>
    protected override void OnUpdate()
    {
        if (projectilePrefab == Entity.Null)
        {
            projectilePrefab = GetSingleton<ProjectileAuthoringComponent>().projectilePrefab;
            return;
        }

        var commandBuffer = beginSimulationECB.CreateCommandBuffer().AsParallelWriter();
        var projPrefab = projectilePrefab;


        Entities.WithAll<PlayerTag>().ForEach((Entity entity,
            int nativeThreadIndex,
            in Translation position,
            in Rotation rotation,
            in BulletSpawnLocationData bulletSpawnLocationData,
            in PlayerShootData playerShootData,
            in PhysicsVelocity velocity) =>
        {
            if (!playerShootData.isShooting || !playerShootData.canShoot)
            {
                return;
            }

            var firstProjectileEntity = commandBuffer.Instantiate(nativeThreadIndex, projPrefab);
            var firstCannonPosition = new Translation { Value = position.Value + math.mul(rotation.Value, bulletSpawnLocationData.firstCannonPosition).xyz };

            commandBuffer.SetComponent(nativeThreadIndex, firstProjectileEntity, firstCannonPosition);

            var firstProjectileVelocity = new PhysicsVelocity { Linear = (45 * math.mul(rotation.Value, new float3(0, 0, 1)).xyz) + velocity.Linear };
            commandBuffer.SetComponent(nativeThreadIndex, firstProjectileEntity, firstProjectileVelocity);

            var secondProjectileEntity = commandBuffer.Instantiate(nativeThreadIndex, projPrefab);
            var secondCannonPosition = new Translation { Value = position.Value + math.mul(rotation.Value, bulletSpawnLocationData.secondCannonPosition).xyz };

            commandBuffer.SetComponent(nativeThreadIndex, secondProjectileEntity, secondCannonPosition);

            var secondProjectileVelocity = new PhysicsVelocity { Linear = (45 * math.mul(rotation.Value, new float3(0, 0, 1)).xyz) + velocity.Linear };
            commandBuffer.SetComponent(nativeThreadIndex, secondProjectileEntity, secondProjectileVelocity);

        }).ScheduleParallel();

        beginSimulationECB.AddJobHandleForProducer(Dependency);
    }

    #endregion
}
