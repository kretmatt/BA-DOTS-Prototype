using Unity.Entities;
using UnityEngine;


/// <summary>
/// CameraAuthoringScript is responsible for assigning the player entity to a component of the camera
/// </summary>
[AddComponentMenu("Custom Authoring/Camera Authoring")]
public class CameraAuthoringScript : MonoBehaviour, IConvertGameObjectToEntity
{
    #region Variables

    ////////////////////////////////////////////////////////////////////
    /////////////////////////      Variables      //////////////////////
    ////////////////////////////////////////////////////////////////////

    /// <summary>
    /// Game object of the camera
    /// </summary>
    public GameObject cameraObject;

    #endregion

    #region Methods

      ////////////////////////////////////////////////////////////////////
     /////////////////////////        Methods      //////////////////////
    ////////////////////////////////////////////////////////////////////


    /// <summary>
    /// Converts the player game object to an entity and adds it to the camera
    /// </summary>
    /// <param name="entity">Player entity</param>
    /// <param name="dstManager">EntityManager instance</param>
    /// <param name="conversionSystem">Conversion system for game objects</param>
    public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
    {
        CameraFollowEntity cameraFollowEntity = cameraObject.GetComponent<CameraFollowEntity>();

        if (cameraFollowEntity == null)
            cameraFollowEntity = cameraObject.AddComponent<CameraFollowEntity>();

        cameraFollowEntity.playerEntity = entity;
    }

    #endregion
}
