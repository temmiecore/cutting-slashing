using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class Sliceable : MonoBehaviour
{
    private MeshFilter meshFilter;
    [HideInInspector] public Mesh mesh;

    private void Start()
    {
        meshFilter = GetComponent<MeshFilter>();
        mesh = meshFilter.mesh;
    }
}
