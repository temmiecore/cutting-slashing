using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class Sliceable : MonoBehaviour
{
    [HideInInspector] public MeshRenderer meshRenderer;
    [HideInInspector] public Mesh mesh;

    [Tooltip("Check if you want new vertices to be created for each triangle, hence making the model have sharp shading. No check - smooth shading.")]
    public bool isSharpShaded;

    [Tooltip("Check if is hollow inside.")]
    public bool isHollow;

    private void Start()
    {
        mesh = GetComponent<MeshFilter>().mesh;
        meshRenderer = GetComponent<MeshRenderer>();
    }

    public void InitializeSliceable(Sliceable parent)
    {
        isSharpShaded = parent.isSharpShaded;
        isHollow = parent.isHollow;
        /// ...
    }
}
