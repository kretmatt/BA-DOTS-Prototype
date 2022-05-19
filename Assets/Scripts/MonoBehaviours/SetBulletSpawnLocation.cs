using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

/// <summary>
/// Script for converting the positions of the cannons to component data
/// </summary>
public class SetBulletSpawnLocation : MonoBehaviour, IConvertGameObjectToEntity
{
    #region Variables

      ////////////////////////////////////////////////////////////////////
     /////////////////////////      Variables      //////////////////////
    ////////////////////////////////////////////////////////////////////

    /// <summary>
    /// First cannon of the ship. Determines one of the positions where projectiles are shot from
    /// </summary>
    public GameObject firstCannon;

    /// <summary>
    /// Second cannon of the ship. Determines one of the positions where projectiles are shot from
    /// </summary>
    public GameObject secondCannon;

    #endregion

    #region Methods


      ////////////////////////////////////////////////////////////////////
     /////////////////////////        Methods      //////////////////////
    ////////////////////////////////////////////////////////////////////

    /// <summary>
    /// Method for converting the game objects for the shooting positions to component data
    /// </summary>
    /// <param name="entity">Player entity</param>
    /// <param name="dstManager">Entity Manager</param>
    /// <param name="conversionSystem">Conversion system</param>
    public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
    {
        if (firstCannon == null || secondCannon == null)
            return;

        BulletSpawnLocationData bulletSpawnLocationData = new BulletSpawnLocationData();

        Vector3 firstCannonPosition = firstCannon.transform.position;
        Vector3 secondCannonPosition = secondCannon.transform.position;
        bulletSpawnLocationData.firstCannonPosition = new float3(firstCannonPosition.x, firstCannonPosition.y, firstCannonPosition.z);
        bulletSpawnLocationData.secondCannonPosition = new float3(secondCannonPosition.x, secondCannonPosition.y, secondCannonPosition.z);

        dstManager.AddComponentData(entity, bulletSpawnLocationData);
    }

    #endregion
}
