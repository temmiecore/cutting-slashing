using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Slicer 
{
    private Sliceable objectToSlice;
    private Plane plane;

    private Mesh mesh1, mesh2;

    private Vector3[] vert1, verts2;
    private int[] tri1, tri2;
    /// UVs, Normals, bones? 

    public Slicer(Sliceable objectToSlice, Plane plane) { this.objectToSlice = objectToSlice; this.plane = plane; }

    public void Slice()
    {

    }
}
