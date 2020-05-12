using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New SpawnData", menuName = "ScriptableObject/SpawnData")]
public class SpawnData : ScriptableObject
{
    [Tooltip("Leave null if the spawner is using an ObjectPool")] public GameObject PrefabToSpawn = null;

    [Tooltip("Max range prefab will spawn from this")] public float MaxSpawnDist = 10;
    [Tooltip("Min distance from this to spawn")] public float MinSpawnDistance = 2;

    [Tooltip("Max spawn angle from forward of this")] public float MaxAngle = 90;

    [Tooltip("The maximum amount of this prefab that will be spawned")] public int MaxSpawns = 10;
}
