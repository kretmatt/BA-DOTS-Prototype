using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using Unity.Mathematics;
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
/// Component for the movement data of a boid
/// </summary>
public struct BoidData : IComponentData
{
    /// <summary>
    /// Current velocity of a boid
    /// </summary>
    public float3 velocity;
    
    /// <summary>
    /// Average heading of the flock of a given boid
    /// </summary>
    public float3 flockHeading;
    
    /// <summary>
    /// Centre of mass of the flock of a given boid
    /// </summary>
    public float3 flockCentre;
    
    /// <summary>
    /// Direction to avoid other nearby boids
    /// </summary>
    public float3 avoidanceHeading;
    
    /// <summary>
    /// Acceleration of the boid
    /// </summary>
    public float3 acceleration;
    
    /// <summary>
    /// Number of flockmates of a given boid
    /// </summary>
    public float numFlockmates;
    
    /// <summary>
    /// Flag that determines whether the boid is heading for collision
    /// </summary>
    public bool headingForCollision;
}
