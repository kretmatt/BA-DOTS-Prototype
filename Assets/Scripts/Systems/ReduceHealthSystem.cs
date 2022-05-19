using Unity.Collections;
using Unity.Entities;
using UnityEngine;

/// <summary>
/// System for reducing the health of asteroids / players when they take damage
/// </summary>
public partial class ReduceHealthSystem : SystemBase
{
    #region Variables

    ////////////////////////////////////////////////////////////////////
    /////////////////////////      Variables      //////////////////////
    ////////////////////////////////////////////////////////////////////

    /// <summary>
    /// CommandBufferSystem for executing commands (e.g. structural changes of entities) after the Simulation is complete
    /// </summary>
    private EndSimulationEntityCommandBufferSystem endSimulationECB;

    #endregion

    #region Structs

    ////////////////////////////////////////////////////////////////////
    /////////////////////////        Structs      //////////////////////
    ////////////////////////////////////////////////////////////////////  

    /// <summary>
    /// Struct for registering health change events 
    /// </summary>
    public struct CurrentHealthComponent : IComponentData
    {
        public float percentage;
    }

    #endregion

    #region Methods

    ////////////////////////////////////////////////////////////////////
    /////////////////////////        Methods      //////////////////////
    ////////////////////////////////////////////////////////////////////

    /// <summary>
    /// Method that gets executed when the system gets created. Used for retrieving the needed EntityCommandBufferSystem instance
    /// </summary>
    protected override void OnCreate()
    {
        base.OnCreate();
        endSimulationECB = World.GetOrCreateSystem<EndSimulationEntityCommandBufferSystem>();
    }

    /// <summary>
    /// Method that gets executed every frame.
    /// Checks whether any entity has taken damage.
    /// Reduces the health of an entity if it has taken damage.
    /// If a players' health is reduced, the UI gets notified through the EventManager class.
    /// </summary>
    protected override void OnUpdate()
    {
        var commandBuffer = endSimulationECB.CreateCommandBuffer();

        // Reduce the health of asteroids / players and register changes in the player health
        Dependency = Entities.ForEach((Entity entity, int nativeThreadIndex, ref HealthData healthData, in DamageData damageData) =>
        {
            healthData.currentHealth -= damageData.damageDealt;
            commandBuffer.RemoveComponent<DamageData>(entity);
            if (HasComponent<PlayerTag>(entity))
            {
                Entity currentHealthData = commandBuffer.CreateEntity();
                commandBuffer.AddComponent(currentHealthData, new CurrentHealthComponent { percentage = (float)healthData.currentHealth / healthData.maxHealth });
            }
        }).Schedule(Dependency);

        endSimulationECB.AddJobHandleForProducer(Dependency);

        var eventBuffer = endSimulationECB.CreateCommandBuffer();
        var currentHealthValues = new NativeList<float>(Allocator.TempJob);

        // Save all health change events to a list
        Entities.WithAll<CurrentHealthComponent>()
            .ForEach((Entity entity, in CurrentHealthComponent currentHealth) =>
            {
                currentHealthValues.Add(currentHealth.percentage);
                eventBuffer.DestroyEntity(entity);
            }).Run();

        endSimulationECB.AddJobHandleForProducer(Dependency);

        // Iterate over the list of health change events and notify the UI
        foreach (float percentage in currentHealthValues)
        {
            SendCurrentHealthToHealthBar(percentage);
        }

        currentHealthValues.Dispose();
    }

    /// <summary>
    /// Method for triggering a HEALTH_DISPLAY event to update the healthbar
    /// </summary>
    /// <param name="percentage">Current health of the player in percentage</param>
    void SendCurrentHealthToHealthBar(float percentage)
    {
        EventManager.TriggerEvent(EEventType.HEALTH_DISPLAY, new System.Collections.Generic.Dictionary<string, object>() { { "percentage", percentage } });
    }

    #endregion
}
