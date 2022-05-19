using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;
using System.Collections.Generic;
using Unity.Jobs;

/***********************************************
 * Inspired by and adapted from:
 * Title: Boids
 * Author: S., Lague
 * Date: August 26, 2019
 * Availability: https://github.com/SebLague/Boids
 
    MIT License

    Copyright (c) 2019 Sebastian Lague

    Permission is hereby granted, free of charge, to any person obtaining a copy
    of this software and associated documentation files (the "Software"), to deal
    in the Software without restriction, including without limitation the rights
    to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
    copies of the Software, and to permit persons to whom the Software is
    furnished to do so, subject to the following conditions:

    The above copyright notice and this permission notice shall be included in all
    copies or substantial portions of the Software.

    THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
    IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
    FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
    AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
    LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
    OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
    SOFTWARE.

 * ********************************************/

/// <summary>
/// System for spawning enemies / boids
/// </summary>
public partial class SpawnBoidsSystem : SystemBase
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
    /// Enemy / Boid prefab as an entity
    /// </summary>
    private Entity boidPrefab;

    #endregion

    #region Methods

    ////////////////////////////////////////////////////////////////////
    /////////////////////////        Methods      //////////////////////
    ////////////////////////////////////////////////////////////////////

    /// <summary>
    /// Method that gets executed when the system is created. Used for retrieving the EntityCommandBufferSystem instance and for subscribing methods to events:
    /// 1. GAME_START event:
    ///    EnableSpawning - Sets the amount of boids to spawn and toggle the isSpawning flag to true.
    /// 
    /// 2. GAME_END event:
    ///    DespawnBoids - Despawns all entities with the GeneralBoidData component
    /// </summary>
    protected override void OnCreate()
    {
        endSimulationECB = World.GetOrCreateSystem<EndSimulationEntityCommandBufferSystem>();
        EventManager.SubscribeMethodToEvent(EEventType.GAME_START, EnableSpawning);
        EventManager.SubscribeMethodToEvent(EEventType.GAME_END, DespawnBoids);
    }

    /// <summary>
    /// Method used for enabling spawning and setting the amount of enemies to spawn
    /// </summary>
    /// <param name="message">Message from the EventManager class. Contains the amount of enemies to spawn (Key = enemies)</param>
    void EnableSpawning(Dictionary<string, object> message)
    {
        if (message.ContainsKey("enemies"))
        {
            int newAmount = (int)message["enemies"];

            JobHandle enableAndSetHandle = Entities.ForEach((ref SpawnerData spawnTag, ref BoidSpawnerData boidSpawnerData) =>
            {
                spawnTag.isSpawning = true;
                boidSpawnerData.spawnCount = newAmount;
            }).ScheduleParallel(Dependency);

            enableAndSetHandle.Complete();
        }
    }

    /// <summary>
    /// Method used for despawning boids
    /// </summary>
    /// <param name="message">Message from the EventManager class.</param>
    void DespawnBoids(Dictionary<string, object> message)
    {
        var destroyBuffer = endSimulationECB.CreateCommandBuffer().AsParallelWriter();

        JobHandle destroyHandle = Entities.WithAll<GeneralBoidData>().ForEach((Entity entity, int nativeThreadIndex) =>
        {
            destroyBuffer.DestroyEntity(nativeThreadIndex, entity);
        }).ScheduleParallel(Dependency);

        endSimulationECB.AddJobHandleForProducer(destroyHandle);
    }

    /// <summary>
    /// Method that gets executed every frame. Retrieves the prefab for enemies / boids as an entity
    /// Calculates a random starting direction, positon, and rotation inside a sphere around the spawners.
    /// In addition, the boid data is initialized.
    /// </summary>
    protected override void OnUpdate()
    {
        if (boidPrefab == Entity.Null)
        {
            boidPrefab = GetSingleton<BoidAuthoringScript>().boidPrefab;
            return;
        }

        var commandBuffer = endSimulationECB.CreateCommandBuffer().AsParallelWriter();
        var prefab = boidPrefab;

        Entities.WithAll<SpawnerData>().ForEach((Entity entity,
            int nativeThreadIndex,
            ref SpawnerData spawnTag,
            in Translation position,
            in BoidSpawnerData boidSpawnerData,
            in BoidSettingsData boidSettings) =>
        {
            if (spawnTag.isSpawning)
            {
                spawnTag.isSpawning = false;

                var rnd = new Unity.Mathematics.Random(1);
                for (int i = 0; i < boidSpawnerData.spawnCount; i++)
                {
                    float3 unitSphere = new float3(rnd.NextFloat(0, 1), rnd.NextFloat(0, 1), rnd.NextFloat(0, 1));

                    float3 translation = position.Value + unitSphere * boidSpawnerData.spawnRadius;
                    quaternion rotation = Quaternion.LookRotation(unitSphere);

                    Translation boidTranslation = new Translation { Value = translation };
                    Rotation boidRotation = new Rotation { Value = rotation };

                    Entity boid = prefab;

                    BoidData bd = new BoidData { velocity = ((boidSettings.minimumSpeed + boidSettings.maximumSpeed) / 2) * unitSphere, acceleration = new float3(0, 0, 0) };

                    commandBuffer.SetComponent(nativeThreadIndex, boid, boidTranslation);
                    commandBuffer.SetComponent(nativeThreadIndex, boid, boidRotation);
                    commandBuffer.AddComponent(nativeThreadIndex, boid, boidSettings);
                    commandBuffer.AddComponent(nativeThreadIndex, boid, bd);
                    commandBuffer.Instantiate(nativeThreadIndex, boid);
                }
            }
        }).ScheduleParallel();

        endSimulationECB.AddJobHandleForProducer(Dependency);
    }

    #endregion



}