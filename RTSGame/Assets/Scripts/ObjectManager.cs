using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Essentially a dictionary of string, GameObject pairs that can be used to identify positions from ScriptableObjects
/// </summary>
public class ObjectManager : MonoBehaviour
{
    [System.Serializable]
    public class KeyPair
    {
        [Tooltip("Name of the position, used for finding it by ScriptableObjects")] public string Key = default;
        [Tooltip("The position to add")] public GameObject Value = default;
    }

    public List<KeyPair> ObjectList = new List<KeyPair>();

    public Dictionary<string, GameObject> ObjectDictionary = null;

    public void Awake()
    {
        ObjectDictionary = new Dictionary<string, GameObject>(2 * ObjectList.Count);

        Helper.LoopListForEach(ObjectList, (KeyPair pair) => { ObjectDictionary.Add(pair.Key, pair.Value); });
    }

    public GameObject GetPosition(string objectName)
    {
        if (objectName.Length > 0)
        {
            if (ObjectDictionary != null && ObjectDictionary.ContainsKey(objectName))
            {
                return ObjectDictionary[objectName];
            }
        }

        return null;
    }
}
