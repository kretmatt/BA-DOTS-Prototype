using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;
using Unity.Physics;
using UnityEngine;

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
/// System responsible for moving the boids and simulating the flocking behavior of birds
/// </summary>
public partial class BoidSystem : SystemBase
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
    /// EntityQuery for retrieving all other boids
    /// </summary>
    private EntityQuery otherBoidsQuery;
    
    /// <summary>
    /// CollisionFilter instance for SphereCasts to detect possible collisions with obstacles
    /// </summary>
    private CollisionFilter collFilter;
    
    /// <summary>
    /// Directions the boids can fly in to avoid obstacles
    /// </summary>
    private NativeArray<float3> avoidDirs;
    
    /// <summary>
    /// Enemy target (player position)
    /// </summary>
    private Entity target;

    #endregion

    #region Methods

    ////////////////////////////////////////////////////////////////////
    /////////////////////////        Methods      //////////////////////
    ////////////////////////////////////////////////////////////////////
    
    /// <summary>
    /// Method that gets executed when the system is created. Prepares the system by instantiating some variables
    /// and by retrieving some values from Unity DOTS.
    /// </summary>
    protected override void OnCreate()
    {
        otherBoidsQuery = GetEntityQuery(typeof(GeneralBoidData), typeof(LocalToWorld), typeof(Translation));
        endSimulationECB = World.GetOrCreateSystem<EndSimulationEntityCommandBufferSystem>();
        collFilter = new CollisionFilter
        {
            BelongsTo = (uint)(CollisionLayer.Boid),
            CollidesWith = (uint)(CollisionLayer.Obstacle | CollisionLayer.Asteroid)
        };

        var helperdirs = CollisionAvoidanceDirectionCalculator.directions;
        avoidDirs = new NativeArray<float3>(helperdirs.Length, Allocator.Persistent);

        for (int i = 0; i < helperdirs.Length; i++)
        {
            avoidDirs[i] = helperdirs[i];
        }

    }

    /// <summary>
    /// Method that gets executed when the system is destroyed.
    /// Disposes of the avoidDirs array to prevent memory leaks
    /// </summary>
    protected override void OnDestroy()
    {
        base.OnDestroy();
        avoidDirs.Dispose();
    }

    /// <summary>
    /// Method that gets executed every frame. There are 4 different jobs, that are dependent on each other:
    /// 1. Calculate the values cohesion, seperation, and alignment of the boids by iterating over the other boids.
    /// 2. Check for each boid: Is it heading for collision? What direction can the boid fly in to avoid an obstacle.
    /// 3. Combine all collected values to calculate the acceleration of each boid that is used for calculating the velocity afterwards
    /// 4. Calculate the new velocity, position, and rotation for each boid.
    /// </summary>
    protected override void OnUpdate()
    {
        if(target == Entity.Null)
        {
            var targetEntity = GetEntityQuery(typeof(PlayerTag)).GetSingletonEntity();
            target = targetEntity;
            return;
        }

        //var commandBuffer = endSimulationECB.CreateCommandBuffer().AsParallelWriter();

        var boidAmount = otherBoidsQuery.CalculateEntityCount();
        var positions = otherBoidsQuery.ToComponentDataArrayAsync<Translation>(Allocator.TempJob, out var positionsHandle);
        var ltwValues = otherBoidsQuery.ToComponentDataArrayAsync<LocalToWorld>(Allocator.TempJob, out var ltwsHandle);

        JobHandle dataBarrier = JobHandle.CombineDependencies(positionsHandle, ltwsHandle);

        Dependency = JobHandle.CombineDependencies(Dependency, dataBarrier);

        // 1. Calculate the values cohesion, seperation, and alignment of the boids by iterating over the other boids.
        Dependency = Entities.WithAll<GeneralBoidData>()
            .WithDisposeOnCompletion(positions)
            .WithDisposeOnCompletion(ltwValues)
            .WithReadOnly(positions)
            .WithReadOnly(ltwValues)
            .ForEach((ref BoidData boidData, in Translation translation, in LocalToWorld ltw, in BoidSettingsData boidSettings) =>
            {
                boidData.numFlockmates = 0;
                boidData.flockCentre = new float3(0, 0, 0);
                boidData.flockHeading = new float3(0, 0, 0);
                boidData.avoidanceHeading = new float3(0, 0, 0);
                boidData.acceleration = new float3(0, 0, 0);

                for (int i = 0; i < boidAmount; i++)
                {
                    float3 offset = positions[i].Value - translation.Value;
                    float sqrDst = offset.x * offset.x + offset.y * offset.y + offset.z * offset.z;

                    if (sqrDst < boidSettings.boidPerceptionRadius * boidSettings.boidPerceptionRadius && sqrDst != 0)
                    {
                        boidData.numFlockmates += 1;
                        boidData.flockHeading += ltwValues[i].Forward;
                        boidData.flockCentre += positions[i].Value;

                        if (sqrDst < boidSettings.boidAvoidanceRadius * boidSettings.boidAvoidanceRadius)
                        {
                            float3 avValue = offset / sqrDst;
                            boidData.avoidanceHeading -= avValue;
                        }
                    }
                }
            }).ScheduleParallel(Dependency);

        // here collision queries + find direction for avoid collisions
        var collWorld = World.GetExistingSystem<Unity.Physics.Systems.BuildPhysicsWorld>().PhysicsWorld.CollisionWorld;
        var boidCasterFilter = collFilter;
        var helperdirs = avoidDirs;

        // 2. Check for each boid: Is it heading for collision? What direction can the boid fly in to avoid an obstacle.
        Dependency = Entities.WithAll<GeneralBoidData>().WithReadOnly(collWorld).WithReadOnly(helperdirs).ForEach((ref BoidData boidData, in BoidSettingsData boidSettings, in Translation translation, in Rotation rotation) =>
        {
            var startPos = translation.Value;
            var endPos = startPos + math.forward(rotation.Value) * boidSettings.collisionAvoidanceDistance;

            bool hit = collWorld.SphereCast(startPos, boidSettings.sphereCastRadius, math.forward(rotation.Value), boidSettings.collisionAvoidanceDistance, boidCasterFilter);

            if (hit)
            {
                for (int i = 0; i < helperdirs.Length; i++)
                {
                    float3 dir = helperdirs[i];
                    bool occupiedDirection = collWorld.SphereCast(startPos, boidSettings.sphereCastRadius, dir, boidSettings.collisionAvoidanceDistance, boidCasterFilter);
                    if (!occupiedDirection)
                    {
                        float3 normAvoidDir = math.length(dir) != 0 ? math.normalize(dir) : math.forward(rotation.Value);
                        var avoidVector = Vector3.ClampMagnitude((normAvoidDir * boidSettings.maximumSpeed - boidData.velocity), boidSettings.maxSteerForce) * boidSettings.collisionAvoidanceWeight;
                        var avoidForce = new float3(avoidVector.x, avoidVector.y, avoidVector.z);
                        boidData.acceleration += avoidForce;
                        break;
                    }
                }
            }
            else
            {
                boidData.headingForCollision = false;
            }

        }).ScheduleParallel(Dependency);

        var boidTarget = EntityManager.GetComponentData<Translation>(target);//GetComponent<Translation>(GetSingletonEntity<PlayerTag>());

        // 3. Combine all collected values to calculate the acceleration of each boid that is used for calculating the velocity afterwards
        Dependency = Entities.WithAll<GeneralBoidData>()
            .ForEach((ref BoidData boidData, in BoidSettingsData boidSettings, in Translation translation) =>
            {
                float3 acceleration = boidData.acceleration;
                float3 offsetToTarget = boidTarget.Value - translation.Value;

                float3 normOffsetToTarget = math.length(offsetToTarget) != 0 ? math.normalize(offsetToTarget) : offsetToTarget;
                Vector3 targetVector = Vector3.ClampMagnitude((normOffsetToTarget * boidSettings.maximumSpeed - boidData.velocity), boidSettings.maxSteerForce) * boidSettings.targetWeight;

                acceleration = new float3(targetVector.x, targetVector.y, targetVector.z);
                if (boidData.numFlockmates != 0)
                {
                    boidData.flockCentre /= boidData.numFlockmates;

                    float3 offsetToFlockmatesCentre = boidData.flockCentre - translation.Value;

                    float3 normFlockHeading = math.length(boidData.flockHeading) != 0 ? math.normalize(boidData.flockHeading) : boidData.flockHeading;
                    var alignmentVector = Vector3.ClampMagnitude((normFlockHeading * boidSettings.maximumSpeed - boidData.velocity), boidSettings.maxSteerForce) * boidSettings.alignmentWeight;

                    float3 normOffsetToFlockmatesCentre = math.length(offsetToFlockmatesCentre) != 0 ? math.normalize(offsetToFlockmatesCentre) : offsetToFlockmatesCentre;
                    var cohesionVector = Vector3.ClampMagnitude((normOffsetToFlockmatesCentre * boidSettings.maximumSpeed - boidData.velocity), boidSettings.maxSteerForce) * boidSettings.cohesionWeight;

                    float3 normAvoidanceHeading = math.length(boidData.avoidanceHeading) != 0 ? math.normalize(boidData.avoidanceHeading) : boidData.avoidanceHeading;
                    var separationVector = Vector3.ClampMagnitude((normAvoidanceHeading * boidSettings.maximumSpeed - boidData.velocity), boidSettings.maxSteerForce) * boidSettings.seperationWeight;

                    float3 alignmentForce = new float3(alignmentVector.x, alignmentVector.y, alignmentVector.z);
                    float3 cohesionForce = new float3(cohesionVector.x, cohesionVector.y, cohesionVector.z);
                    float3 seperationForce = new float3(separationVector.x, separationVector.y, separationVector.z);

                    acceleration += alignmentForce;
                    acceleration += cohesionForce;
                    acceleration += seperationForce;
                }
                boidData.acceleration += acceleration;
            }).ScheduleParallel(Dependency);

        var deltaTime = Time.DeltaTime;

        // 4. Calculate the new velocity, position, and rotation for each boid.
        Dependency = Entities.WithAll<GeneralBoidData>()
            .ForEach((ref BoidData boidData, ref Rotation rotation, ref Translation translation, in BoidSettingsData boidSettings, in LocalToWorld ltw) =>
            {
                float3 velocity = boidData.velocity;
                velocity += boidData.acceleration * deltaTime;
                float speed = math.length(velocity);
                float3 dir = speed != 0 ? velocity / speed : 0;
                speed = math.clamp(speed, boidSettings.minimumSpeed, boidSettings.maximumSpeed);
                velocity = dir * speed;
                float3 newPos = translation.Value + velocity * deltaTime;

                quaternion newRot = quaternion.LookRotation(dir, ltw.Up);

                Translation newTranslation = new Translation { Value = newPos };
                Rotation newRotation = new Rotation { Value = newRot };

                translation = newTranslation;
                rotation = newRotation;
                boidData.velocity = velocity;
            }).ScheduleParallel(Dependency);


        endSimulationECB.AddJobHandleForProducer(Dependency);
    }

    #endregion
}

/// <summary>
/// Enums with the collision layers represented as bitmasks
/// </summary>
public enum CollisionLayer
{
    Obstacle = 1<<0,
    Player = 1<<1,
    Projectile = 1<<2,
    Boid = 1<<3,
    Asteroid = 1<<4
}

/// <summary>
/// Helper class that provides several directions a boid can steer towards to avoid collisions
/// </summary>
public static class CollisionAvoidanceDirectionCalculator
{
    #region Variables

    ////////////////////////////////////////////////////////////////////
    /////////////////////////      Variables      //////////////////////
    ////////////////////////////////////////////////////////////////////

    /// <summary>
    /// Amount of directions
    /// </summary>
    const int numViewDirections = 100;

    /// <summary>
    /// Directions a boid can steer towards to avoid obstacles
    /// </summary>
    public static readonly Vector3[] directions;

    #endregion

    #region Constructors

    ////////////////////////////////////////////////////////////////////
    /////////////////////////     Constructors    //////////////////////
    ////////////////////////////////////////////////////////////////////

    /// <summary>
    /// Constructor of CollisionAvoidanceDirectionCalculator
    /// </summary>
    static CollisionAvoidanceDirectionCalculator()
    {
        directions = new Vector3[numViewDirections];

        float goldenRatio = (1 + Mathf.Sqrt(5)) / 2;
        float angleIncrement = Mathf.PI * 2 * goldenRatio;

        for (int i = 0; i < numViewDirections; i++)
        {
            float t = (float)i / numViewDirections;
            float inclination = Mathf.Acos(1 - 2 * t);
            float azimuth = angleIncrement * i;

            float x = Mathf.Sin(inclination) * Mathf.Cos(azimuth);
            float y = Mathf.Sin(inclination) * Mathf.Sin(azimuth);
            float z = Mathf.Cos(inclination);
            directions[i] = new Vector3(x, y, z);
        }
    }

    #endregion
}