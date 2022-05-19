using Unity.Entities;
using UnityEngine;

/// <summary>
/// System responsible for removing projectiles if they do not collide with an asteroid after a given amount of time
/// </summary>
public partial class TimeToLiveSystem : SystemBase
{
    #region Variables

    ////////////////////////////////////////////////////////////////////
    /////////////////////////      Variables      //////////////////////
    ////////////////////////////////////////////////////////////////////

    /// <summary>
    /// CommandBufferSystem for executing commands (e.g. structural changes of entities) after the Simulation is complete
    /// </summary>
    private BeginSimulationEntityCommandBufferSystem beginSimulationECB;

    #endregion

    #region Methods

    ////////////////////////////////////////////////////////////////////
    /////////////////////////        Methods      //////////////////////
    ////////////////////////////////////////////////////////////////////

    /// <summary>
    /// Method that gets executed when the system is created. Retrieves the needed EntityCommandBufferSystem instance.
    /// </summary>
    protected override void OnCreate()
    {
        beginSimulationECB = World.GetOrCreateSystem<BeginSimulationEntityCommandBufferSystem>();
    }

    /// <summary>
    /// Method that gets executed every frame. It looks at the current duration values of the projectiles. If the value is greater than the maximum possible value, the projectile gets removed.
    /// </summary>
    protected override void OnUpdate()
    {
        var commandBuffer = beginSimulationECB.CreateCommandBuffer().AsParallelWriter();

        var deltaTime = Time.DeltaTime;

        Entities.ForEach((Entity entity, int nativeThreadIndex, ref TimeToLiveData timeToLiveData) =>
        {
            timeToLiveData.currentDuration += deltaTime;
            if (timeToLiveData.currentDuration > timeToLiveData.maxDuration)
                commandBuffer.DestroyEntity(nativeThreadIndex, entity);
        }).ScheduleParallel();

        beginSimulationECB.AddJobHandleForProducer(Dependency);
    }

    #endregion
}
