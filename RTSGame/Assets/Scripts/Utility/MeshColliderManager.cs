using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class MeshColliderManager : MonoBehaviour
{
    public bool AddColliders;
    public bool RemoveColliders;

    private GameObject currentObj;
    private MeshCollider meshCollider;

    // Update is called once per frame
    void Update()
    {
        if (AddColliders)
        {
            AddMeshColliders(gameObject);
            AddColliders = false;
        }
        if (RemoveColliders)
        {
            RemoveMeshColliders(gameObject);
            RemoveColliders = false;
        }
    }

    void AddMeshColliders(GameObject g)
    {
        for (int i = 0; i < g.transform.childCount; i++)
        {
            currentObj = g.transform.GetChild(i).gameObject;
            meshCollider = currentObj.GetComponent<MeshCollider>();

            if (currentObj.GetComponent<ExludeMeshCollider>() == null)
            {
                if (currentObj.GetComponent<MeshFilter>() != null && meshCollider == null)
                {
                    currentObj.AddComponent<MeshCollider>();
                }
                else if (meshCollider != null && meshCollider.enabled == false)
                {
                    meshCollider.enabled = true;
                }
                currentObj.isStatic = true;
            }

            AddMeshColliders(currentObj);
        }
    }

    void RemoveMeshColliders(GameObject g)
    {
        for (int i = 0; i < g.transform.childCount; i++)
        {
            currentObj = g.transform.GetChild(i).gameObject;
            meshCollider = currentObj.GetComponent<MeshCollider>();

            if (currentObj.GetComponent<ExludeMeshCollider>() == null)
            {
                if (meshCollider != null)
                {
                    meshCollider.enabled = false;
                }

                if (meshCollider != null)
                {
                    meshCollider.enabled = false;
                }

                currentObj.isStatic = false;
            }

            RemoveMeshColliders(currentObj);
        }
    }
}
