using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Outliner : MonoBehaviour
{
    [Tooltip("The base object to outline, all the children will also be outlined if appropriate")] public GameObject ObjectToOutline = null;
    [Tooltip("The material to be used for outlining")] public Material OutlineMaterial = null;
    [Space]
    [Tooltip("The colour of the outline")] public Color OutlineColour = Color.red;
    [Tooltip("The thickness of the outline")] public float OutlineThickness = 0.1f;

    Renderer rend = null;
    List<Material> mats = null;

    // Start is called before the first frame update
    void Start()
    {
        // Get a 'new instance' of the material
        if (OutlineMaterial)
        {
            OutlineMaterial = new Material(OutlineMaterial);
            OutlineMaterial.SetFloat("_OutlineThickness", 0);
        }

        // Add the materials to all the renderers
        if (ObjectToOutline)
        {
            AddOutline(ObjectToOutline);
        }
    }

    /// <summary>
    /// Recursively add the material to the outlineObject and its children
    /// </summary>
    /// <param name="outlineObject">The object to start from in the chain</param>
    void AddOutline(GameObject outlineObject)
    {
        rend = outlineObject.GetComponent<Renderer>();
        if (rend)
        {
            // Get all the materials on the renderer and add the OutlineMaterial to the end
            mats = new List<Material>(rend.materials);
            mats.Add(OutlineMaterial);

            rend.materials = mats.ToArray();
        }

        for (int i = 0; i < outlineObject.transform.childCount; i++)
        {
            AddOutline(outlineObject.transform.GetChild(i).gameObject);
        }
    }
    
    /// <summary>
    /// Sets the thickness to the desired thickness
    /// NOTE - It would be better to just add the material to there renderers here
    /// </summary>
    public void OutlineOn()
    {
        if (OutlineMaterial)
        {
            OutlineMaterial.SetFloat("_OutlineThickness", OutlineThickness);
            OutlineMaterial.SetColor("_OutlineColour", OutlineColour);
        }
    }

    /// <summary>
    /// Sets the thickness to zero
    /// NOTE - Would be better to just remove the material from the renderer
    /// </summary>
    public void OutlineOff()
    {
        if (OutlineMaterial)
        {
            OutlineMaterial.SetFloat("_OutlineThickness", 0);
        }
    }
}
