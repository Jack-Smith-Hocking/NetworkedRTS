using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MeshColliderManager : MonoBehaviour
{

    [ContextMenu("RemoveMeshColliders")]
    public void RemoveMeshColliders()
    {
        List<MeshCollider> colliders = new List<MeshCollider>(Helper.GetComponents<MeshCollider>(gameObject));

        Helper.LoopList_ForEach<MeshCollider>(colliders, (MeshCollider mesh) =>
        {
            DestroyImmediate(mesh);
        });
    }

    [ContextMenu("AddMeshColliders")]
    public void AddMeshColliders()
    {
        // TO-DO
        // Implement adding MeshColliders
    }
}
