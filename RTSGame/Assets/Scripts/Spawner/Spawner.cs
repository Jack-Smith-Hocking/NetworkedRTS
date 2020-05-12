using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Spawner : MonoBehaviour
{
    [System.Serializable]
    public class SpawningData
    {
        [Tooltip("ScriptableObject that holds data for spawning")] public SpawnData SpawnData = null;

        [HideInInspector] public int CurrentSpawns = 0;
    }

    [Tooltip("Whether or not will be spawning NavAgents")] public bool SpawningOnNavMesh = false;

    [Tooltip("List of possible SpawnDatas to be used (randomly) each time spawning is available")] public List<SpawningData> SpawnDatas = new List<SpawningData>(10);

    private List<SpawningData> availableSpawns = null;

    [Header("Spawn Info")]
    [Tooltip("The time before each spawn")] public float TimeBetweenSpawns = 2;

    private float currentSpawnTime = 0;

    // Start is called before the first frame update
    void Start()
    {
        availableSpawns = new List<SpawningData>(SpawnDatas.Count);
        availableSpawns.AddRange(SpawnDatas);

        currentSpawnTime = Time.time + TimeBetweenSpawns;
    }

    // Update is called once per frame
    void Update()
    {
        // Spawn a GameObject after the delay
        if (Time.time >= currentSpawnTime)
        {
            currentSpawnTime = Time.time + TimeBetweenSpawns;

            Spawn();
        }
    }

    /// <summary>
    /// Trims a list of SpawnDatas to only contain ones that can still spawn
    /// </summary>
    /// <param name="spawnList">List to trim</param>
    /// <returns>A random SpawnData from the available list</returns>
    SpawningData GetSpawningData(ref List<SpawningData> spawnList)
    {
        if (spawnList.Count == 0)
        {
            return null;
        }

        int spawnIndex = Random.Range(0, spawnList.Count);
        SpawningData spawningData = spawnList[spawnIndex];

        if (spawningData.CurrentSpawns >= spawningData.SpawnData.MaxSpawns)
        {
            spawnList.Remove(spawningData);

            return GetSpawningData(ref spawnList);
        }

        return spawningData;
    }

    /// <summary>
    /// Spawns a random SpawnData from the list into the world
    /// </summary>
    void Spawn()
    {
        // Gets a random SpawningData from the availableSpawns list
        SpawningData spawningData = GetSpawningData(ref availableSpawns);

        if (spawningData == null)
        {
            return;
        }

        float dist = 0, angle = 0;
        Vector3 randPoint = Vector3.zero;

        // Get position
        if (spawningData.SpawnData != null)
        {
            dist = Random.Range(-spawningData.SpawnData.MaxSpawnDist, spawningData.SpawnData.MaxSpawnDist);

            if (dist < 0)
            {
                dist = Mathf.Clamp(dist, -spawningData.SpawnData.MinSpawnDistance, -spawningData.SpawnData.MaxSpawnDist);
            }
            else
            {
                dist = Mathf.Clamp(dist, spawningData.SpawnData.MinSpawnDistance, spawningData.SpawnData.MaxSpawnDist);
            }

            if (!SpawningOnNavMesh)
            {
                angle = Random.Range(-90, 90);

                randPoint = Vector3.forward;
                randPoint = Quaternion.AngleAxis(angle, Vector3.up) * randPoint;
                randPoint = randPoint.normalized * dist;

                randPoint += transform.position;
            }

            if (SpawningOnNavMesh)
            {
                // Get a random point on a NavMesh
                randPoint = StaticHelper.RandomNavSphere(transform.position, Mathf.Abs(dist), -1);
            }
        }
        else
        {
            return;
        }

        GameObject spawnedObject = Instantiate(spawningData.SpawnData.PrefabToSpawn, randPoint, Quaternion.identity, transform);

        if (spawnedObject == null)
        {
            return;
        }

        spawningData.CurrentSpawns++;
    }
}
