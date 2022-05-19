using Unity.Collections;
using System.Collections.Generic;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;
using Unity.Jobs;

/// <summary>
/// System for spawning the asteroids in asteroid belts
/// </summary>
public partial class SpawnAsteroidBeltsSystem : SystemBase
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
    /// Prefab of the light asteroid as an entity
    /// </summary>
    private Entity lightAsteroid;
    
    /// <summary>
    /// Prefab of the medium asteroid as an entity
    /// </summary>
    private Entity mediumAsteroid;
    
    /// <summary>
    /// Prefab of the heavy asteroid as an entity
    /// </summary>
    private Entity heavyAsteroid;

    #endregion

    #region Methods

    ////////////////////////////////////////////////////////////////////
    /////////////////////////        Methods      //////////////////////
    ////////////////////////////////////////////////////////////////////

    /// <summary>
    /// Method that gets executed when the system is created. Used for retrieving the EntityCommandBufferSystem instance and for subscribing methods to events:
    /// 1. GAME_START event:
    ///    EnableSpawning - Sets the amount of asteroids to spawn and toggle the isSpawning flag to true.
    /// 
    /// 2. GAME_END event:
    ///    DespawnAsteroids - Despawns all entities with the AsteroidData component
    /// </summary>
    protected override void OnCreate()
    {
        endSimulationECB = World.GetOrCreateSystem<EndSimulationEntityCommandBufferSystem>();
        EventManager.SubscribeMethodToEvent(EEventType.GAME_START, EnableSpawning);
        EventManager.SubscribeMethodToEvent(EEventType.GAME_END, DespawnAsteroids);
    }

    /// <summary>
    /// Method for enabling all asteroid belt spawners and setting the amount of asteroids to spawn
    /// </summary>
    /// <param name="message">Message from the EventManager class. Contains the amount of asteroids to spawn</param>
    void EnableSpawning(Dictionary<string, object> message)
    {
        if (message.ContainsKey("asteroids"))
        {
            int newAmount = (int)message["asteroids"];

            JobHandle enableAndSetHandle = Entities.ForEach((ref SpawnerData spawnTag, ref AsteroidBeltData asteroidBeltData) =>
            {
                spawnTag.isSpawning = true;
                asteroidBeltData.numberOfAsteroids = newAmount;
            }).ScheduleParallel(Dependency);

            enableAndSetHandle.Complete();
        }
    }

    /// <summary>
    /// Method for despawning all asteroids
    /// </summary>
    /// <param name="message">Message from the EventManager class</param>
    void DespawnAsteroids(Dictionary<string, object> message)
    {
        var destroyBuffer = endSimulationECB.CreateCommandBuffer().AsParallelWriter();

        JobHandle destroyHandle = Entities.WithAll<AsteroidData>().ForEach((Entity entity, int nativeThreadIndex) =>
        {
            destroyBuffer.DestroyEntity(nativeThreadIndex, entity);
        }).ScheduleParallel(Dependency);

        endSimulationECB.AddJobHandleForProducer(destroyHandle);
    }

    /// <summary>
    /// Method that gets executed every frame. Retrieves the asteroid prefabs as entities and spawns asteroids for the asteroid belts if their spawning process is enabled
    /// </summary>
    protected override void OnUpdate()
    {
        if (lightAsteroid == Entity.Null || mediumAsteroid == Entity.Null || heavyAsteroid == Entity.Null)
        {
            var asteroidCollectionAuthoring = GetSingleton<AsteroidCollectionAuthoring>();
            lightAsteroid = asteroidCollectionAuthoring.lightAsteroid;
            mediumAsteroid = asteroidCollectionAuthoring.mediumAsteroid;
            heavyAsteroid = asteroidCollectionAuthoring.heavyAsteroid;
            return;
        }

        var commandBuffer = endSimulationECB.CreateCommandBuffer().AsParallelWriter();
        var prefabHeavy = heavyAsteroid;
        var prefabLight = lightAsteroid;
        var prefabMedium = mediumAsteroid;


        Entities.WithAll<SpawnerData>().ForEach((Entity entity,
            int nativeThreadIndex,
            ref SpawnerData spawnTag,
            in Translation position,
            in Rotation rotation,
            in AsteroidBeltData asteroidBeltData) =>
        {
            if (spawnTag.isSpawning != false)
            {
                // Deactivate future spawning and generate Random instance 
                spawnTag.isSpawning = false;
                uint asteroidSeed = asteroidBeltData.asteroidBeltSeed;
                var rnd = new Unity.Mathematics.Random(asteroidSeed);
                
                Quaternion tempRot = rotation.Value;
                quaternion tempAsteroidRotation;
                float distanceToBeltCenter, angle, x, y, z;
                float3 asteroidPosition, asteroidOffset;
                for (int i = 0; i < asteroidBeltData.numberOfAsteroids; i++)
                {
                    // Retrieve a random angle and radius / distance value (Angle is in radians because Math
                    // takes radians values)
                    angle = rnd.NextFloat(0, (2 * math.PI));
                    distanceToBeltCenter = rnd.NextFloat(asteroidBeltData.beltInnerRadius, asteroidBeltData.beltOuterRadius);

                    // Calculate the x, y and z coordinates. X and Z are calculated with the unit circle
                    // and multiplied with the distance to the asteroid belt center. Y is the height of the asteroid
                    y = rnd.NextFloat(-(asteroidBeltData.beltHeight / 2), (asteroidBeltData.beltHeight / 2));
                    x = distanceToBeltCenter * math.cos(angle);
                    z = distanceToBeltCenter * math.sin(angle);
                    asteroidPosition = new float3(x, y, z);
                    asteroidOffset = tempRot * asteroidPosition;

                    //Set rotation
                    tempAsteroidRotation = new quaternion(1, 2, 3, 1);

                    // Select a random prefab from one of the three entities
                    int rndPrefab = rnd.NextInt(0, 3);

                    Entity tempentpref;

                    if (rndPrefab == 1)
                        tempentpref = prefabLight;
                    else if (rndPrefab == 2)
                        tempentpref = prefabMedium;
                    else
                        tempentpref = prefabHeavy;

                    // Prepare data for the command buffer
                    var asteroidTranslation = new Translation { Value = position.Value + asteroidOffset };
                    var asteroidRotation = new Rotation { Value = tempAsteroidRotation };
                    var beltObjectData = new BeltObjectData
                    {
                        orbitSpeed = asteroidBeltData.beltObjectOrbitSpeed,
                        parentPosition = position.Value,
                        parentUp = tempRot * Vector3.up,
                        rotationClockwise = asteroidBeltData.beltRotationDirection
                    };

                    // Save commands in the command buffer and instantiate the asteroid in the command buffer
                    commandBuffer.SetComponent(nativeThreadIndex, tempentpref, asteroidTranslation);
                    commandBuffer.SetComponent(nativeThreadIndex, tempentpref, asteroidRotation);
                    commandBuffer.AddComponent(nativeThreadIndex, tempentpref, beltObjectData);

                    var asteroidEntity = commandBuffer.Instantiate(nativeThreadIndex, tempentpref);
                }
            }
        }).ScheduleParallel();

        endSimulationECB.AddJobHandleForProducer(Dependency);
    }

    #endregion
}
