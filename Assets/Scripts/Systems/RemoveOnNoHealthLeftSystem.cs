using System;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Physics.Systems;
using UnityEngine;

/// <summary>
/// System for removing elements (mostly asteroids) when they have no health left. Also ends the game if the player has no health
/// </summary>
[UpdateInGroup(typeof(FixedStepSimulationSystemGroup))]
[UpdateAfter(typeof(StepPhysicsWorld))]
[UpdateBefore(typeof(ExportPhysicsWorld))]
public partial class RemoveOnNoHealthLeftSystem : SystemBase
{
    #region Variables

    ////////////////////////////////////////////////////////////////////
    /////////////////////////      Variables      //////////////////////
    ////////////////////////////////////////////////////////////////////

    /// <summary>
    /// CommandBufferSystem for executing commands (e.g. structural changes of entities) after the Simulation is complete
    /// </summary>
    private EndSimulationEntityCommandBufferSystem endSimulationECB;

    private bool helperflag = false;

    #endregion

    #region Structs

    ////////////////////////////////////////////////////////////////////
    /////////////////////////        Structs      //////////////////////
    ////////////////////////////////////////////////////////////////////  

    /// <summary>
    /// Struct for registering score increase events on asteroid removal
    /// </summary>
    public struct ScoreIncreaseComponent : IComponentData
    {
        /// <summary>
        /// Number of points to increase the current score with
        /// </summary>
        public int scoreIncrease;
    }

    /// <summary>
    /// Struct for registering game end events on player death
    /// </summary>
    public struct GameOverComponent : IComponentData
    {
        /// <summary>
        /// Dummy value, because at least one variable is needed
        /// </summary>
        public int Value;
    }

    #endregion

    #region Methods

    ////////////////////////////////////////////////////////////////////
    /////////////////////////        Methods      //////////////////////
    ////////////////////////////////////////////////////////////////////

    /// <summary>
    /// Method that gets executed once the system starts running.
    /// </summary>
    protected override void OnStartRunning()
    {
        this.RegisterPhysicsRuntimeSystemReadOnly();
    }

    /// <summary>
    /// Method that gets executed when the system gets created. Used for retrieving the needed EntityCommandBufferSystem instance
    /// </summary>
    protected override void OnCreate()
    {
        endSimulationECB = World.GetOrCreateSystem<EndSimulationEntityCommandBufferSystem>();
    }

    /// <summary>
    /// Method that gets executed every frame. 
    /// Checks whether there are elements that need to be removed (because no health is left). 
    /// If an asteroid has no health left, a score increase event gets registered by putting it into a collection
    /// If a player has no health left, a game over event gets registered by putting it into a collection.
    /// In the next frame, the collections are iterated over and the events get triggered.
    /// </summary>
    protected override void OnUpdate()
    {
        if (helperflag)
            helperflag = !helperflag;

        var removeBuffer = endSimulationECB.CreateCommandBuffer().AsParallelWriter();

        // Remove asteroids with no health create a score increase entity
        Dependency = Entities.WithNone<PlayerTag>().WithAll<AsteroidData>().ForEach((Entity entity, int nativeThreadIndex, in HealthData healthData) =>
        {
            if (healthData.currentHealth <= 0)
            {
                if (entity != Entity.Null)
                    removeBuffer.DestroyEntity(nativeThreadIndex, entity);
                Entity increaseScoreEntity = removeBuffer.CreateEntity(nativeThreadIndex);
                removeBuffer.AddComponent(nativeThreadIndex, increaseScoreEntity, new ScoreIncreaseComponent { scoreIncrease = healthData.maxHealth });
            }
        }).Schedule(Dependency);

        // Create a game over entity if a player has no health left
        Dependency = Entities.WithAll<PlayerTag>().ForEach((Entity entity, int nativeThreadIndex, ref HealthData healthData, ref PlayerMoveData playerMoveData, ref PlayerShootData playerShootData) =>
        {
            if (healthData.currentHealth <= 0 && playerMoveData.moveable)
            {
                healthData.currentHealth = healthData.maxHealth;
                Entity gameOverEntity = removeBuffer.CreateEntity(nativeThreadIndex);
                removeBuffer.AddComponent(nativeThreadIndex, gameOverEntity, new GameOverComponent { Value = 1 });
            }

        }).Schedule(Dependency);

        endSimulationECB.AddJobHandleForProducer(Dependency);

        var removeEventsBuffer = endSimulationECB.CreateCommandBuffer().AsParallelWriter();
        var scoreEvents = new NativeList<ScoreIncreaseComponent>(Allocator.TempJob);
        var gameOverEvents = new NativeList<GameOverComponent>(Allocator.TempJob);

        //Capture score increase events
        Entities.WithAll<ScoreIncreaseComponent>().ForEach((Entity entity, int nativeThreadIndex, in ScoreIncreaseComponent scoreIncrease) =>
        {
            if (entity != Entity.Null)
            {
                scoreEvents.Add(scoreIncrease);

                removeEventsBuffer.DestroyEntity(nativeThreadIndex, entity);
            }

        }).Run();

        // Capture game over events
        Entities.WithAll<GameOverComponent>().ForEach((Entity entity, int nativeThreadIndex, in GameOverComponent gameOver) =>
        {
            gameOverEvents.Add(gameOver);
            removeEventsBuffer.DestroyEntity(nativeThreadIndex, entity);
        }).Run();

        // Iterate over the registered events

        foreach (var si in scoreEvents)
        {
            IncreaseScore(si.scoreIncrease);
        }

        if (gameOverEvents.Length > 0)
        {
            GameOver();
        }
       

        endSimulationECB.AddJobHandleForProducer(Dependency);
        scoreEvents.Dispose();
        gameOverEvents.Dispose();
    }

    /// <summary>
    /// Trigger a score increase event
    /// </summary>
    /// <param name="scoreIncrease">Points to be added to the score</param>
    void IncreaseScore(int scoreIncrease)
    {
        EventManager.TriggerEvent(EEventType.SCORE_INCREASE, new System.Collections.Generic.Dictionary<string, object>() { { "amount", scoreIncrease } });
    }

    /// <summary>
    /// Trigger a game over event
    /// </summary>
    void GameOver()
    {
        EventManager.TriggerEvent(EEventType.GAME_END, new System.Collections.Generic.Dictionary<string, object>() { { "state", true} });
    }

    #endregion
}
