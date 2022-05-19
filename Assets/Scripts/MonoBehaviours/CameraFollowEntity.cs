using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

/// <summary>
/// Script that makes the camera follow the player character
/// </summary>
public class CameraFollowEntity : MonoBehaviour
{
    #region Variables

      ////////////////////////////////////////////////////////////////////
     /////////////////////////      Variables      //////////////////////
    ////////////////////////////////////////////////////////////////////

    /// <summary>
    /// Player entity
    /// </summary>
    public Entity playerEntity;

    /// <summary>
    /// Default distance to the player
    /// </summary>
    public float3 defaultDistance = new float3(0f, 4f, -10f);

    /// <summary>
    /// Value that ensures that the camera doesn't snap to the new player position
    /// </summary>
    public float distanceDamp = 10f;

    /// <summary>
    /// Value that ensures that the rotation of the camera is not to snappy
    /// </summary>
    public float rotationDamp = 10f;

    /// <summary>
    /// EntityManager of Unity ECS / DOTS 
    /// </summary>
    EntityManager manager;

    #endregion

    #region Unity Messages

      ////////////////////////////////////////////////////////////////////
     /////////////////////////    Unity Messages   //////////////////////
    ////////////////////////////////////////////////////////////////////

    /// <summary>
    /// Method that gets called once in the lifetime of the script. Retrieves the EntityManager instance that is needed for retrieving the position data from the player entity
    /// </summary>
    void Awake()
    {
        manager = World.DefaultGameObjectInjectionWorld.EntityManager;
    }

    /// <summary>
    /// Method that gets called every frame after the Update method. Ensures that the camera follows the player
    /// </summary>
    void LateUpdate()
    {
        if (playerEntity == Entity.Null)
            return;

        Vector3 entityPosition = manager.GetComponentData<Translation>(playerEntity).Value;
        Quaternion entityRotation = manager.GetComponentData<Rotation>(playerEntity).Value;

        Vector3 newPosition = entityPosition + (entityRotation * defaultDistance);
        Vector3 currentPosition = Vector3.Lerp(transform.position, newPosition, distanceDamp * Time.deltaTime);
        transform.position = currentPosition;

        Quaternion newRotation = Quaternion.LookRotation(entityPosition - transform.position, entityRotation * Vector3.up);
        Quaternion currentRotation = Quaternion.Slerp(transform.rotation, newRotation, rotationDamp * Time.deltaTime);
        transform.rotation = currentRotation;
    }

    #endregion
}
