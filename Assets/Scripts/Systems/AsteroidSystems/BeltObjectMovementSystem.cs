using Unity.Entities;
using Unity.Transforms;
using Unity.Mathematics;
using UnityEngine;
using Unity.Physics;

/// <summary>
/// System for moving the asteroids of an asteroid belt
/// </summary>
public partial class BeltObjectMovementSystem : SystemBase
{
    /// <summary>
    /// Method that gets executed every frame.
    /// Depending on the rotationClockwise flag, each asteroid moves clockwise or counter-clockwise on the asteroid belt.
    /// </summary>
    protected override void OnUpdate()
    {
        float deltaTime = Time.DeltaTime;
        Entities.WithAll<AsteroidData>().ForEach((ref Translation position,
            in BeltObjectData beltObjectData) =>
        {
            if (beltObjectData.rotationClockwise)
            {
                position.Value = math.mul(quaternion.AxisAngle(beltObjectData.parentUp, beltObjectData.orbitSpeed * deltaTime * 0.15f), position.Value - beltObjectData.parentPosition) + new float3(beltObjectData.parentPosition.x, beltObjectData.parentPosition.y, beltObjectData.parentPosition.z);
            }
            else
            {
                position.Value = math.mul(quaternion.AxisAngle(-beltObjectData.parentUp, beltObjectData.orbitSpeed * deltaTime * 0.15f), position.Value - beltObjectData.parentPosition) + new float3(beltObjectData.parentPosition.x, beltObjectData.parentPosition.y, beltObjectData.parentPosition.z);
            }

        }).ScheduleParallel();
    }
}
