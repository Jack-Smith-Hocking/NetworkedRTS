using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

/// <summary>
/// A class that will add NavMeshObstacle compnents too all of the child components that have a MeshCollider
/// NOTE - Probably could have just done gameObject.GetComponentsInChildren<MeshCollider>() and added a NavMeshObstacle to all of those
/// </summary>
[ExecuteInEditMode]
public class NavObstacleManager : MonoBehaviour
{
    public bool AddObstacles;
    public bool RemoveObstacles;

    private GameObject currentObj;
    private NavMeshObstacle navObstacle;

    // Update is called once per frame
    void Update()
    {
        if (AddObstacles)
        {
            AddNavObstacles(gameObject);
            AddObstacles = false;
        }
        if (RemoveObstacles)
        {
            RemoveNavObstacles(gameObject);
            RemoveObstacles = false;
        }
    }

    void AddNavObstacles(GameObject g)
    {
        for (int i = 0; i < g.transform.childCount; i++)
        {
            currentObj = g.transform.GetChild(i).gameObject;
            navObstacle = currentObj.GetComponent<NavMeshObstacle>();

            if (currentObj.isStatic == true)
            {
                if (currentObj.GetComponent<MeshCollider>() != null && navObstacle == null)
                {
                    navObstacle = currentObj.AddComponent<NavMeshObstacle>();
                    navObstacle.carving = true;
                }
                else if (navObstacle != null && navObstacle.enabled == false)
                {
                    navObstacle.enabled = true;
                }
            }

            AddNavObstacles(currentObj);
        }
    }

    void RemoveNavObstacles(GameObject g)
    {
        for (int i = 0; i < g.transform.childCount; i++)
        {
            currentObj = g.transform.GetChild(i).gameObject;
            navObstacle = currentObj.GetComponent<NavMeshObstacle>();

            if (currentObj.isStatic == true)
            {
                if (navObstacle != null)
                {
                    navObstacle.enabled = false;
                    navObstacle = null;
                }
            }

            RemoveNavObstacles(currentObj);
        }
    }
}
