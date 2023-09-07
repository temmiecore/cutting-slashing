using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class Sliceable : MonoBehaviour
{
    [HideInInspector] public MeshRenderer meshRenderer;
    [HideInInspector] public Mesh mesh;

    private void Start()
    {
        mesh = GetComponent<MeshFilter>().mesh;
        meshRenderer = GetComponent<MeshRenderer>();
    }
}
