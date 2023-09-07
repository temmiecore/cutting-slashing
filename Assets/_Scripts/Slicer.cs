using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Slicer
{
    private Sliceable objectToSlice;
    private Plane plane;
    private Material originalMaterial;

    private Mesh mesh1, mesh2; /// 1 - positive side, 2 - negative side

    private List<Vector3> originalVert;
    private List<Vector3> verts1, verts2;

    private List<Vector2> originalUVs;
    private List<Vector2> uvs1, uvs2;

    private List<Vector3> originalNormals;
    private List<Vector3> normals1, normals2;

    private List<int> originalTris;
    private List<int> tris1, tris2;
    /// UVs, Normals, bones? 

    public Slicer(Sliceable objectToSlice, Plane plane)
    {
        this.objectToSlice = objectToSlice;
        this.plane = plane;
        originalMaterial = objectToSlice.meshRenderer.material;

        originalVert = objectToSlice.mesh.vertices.ToList();
        originalTris = objectToSlice.mesh.triangles.ToList();
        originalUVs = objectToSlice.mesh.uv.ToList();
        originalNormals = objectToSlice.mesh.normals.ToList();

        mesh1 = new Mesh(); mesh2 = new Mesh();
        verts1 = new List<Vector3>(); verts2 = new List<Vector3>();
        tris1 = new List<int>(); tris2 = new List<int>();
        uvs1 = new List<Vector2>(); uvs2 = new List<Vector2>();
        normals1 = new List<Vector3>(); normals2 = new List<Vector3>();
    }

    public void Slice()
    {
        for (int i = 0; i < originalTris.Count; i += 3)
        {
            Vector3 vert1 = originalVert[originalTris[i]];
            Vector3 vert2 = originalVert[originalTris[i + 1]];
            Vector3 vert3 = originalVert[originalTris[i + 2]];

            bool vert1Side = plane.GetSide(vert1);
            bool vert2Side = plane.GetSide(vert2);
            bool vert3Side = plane.GetSide(vert3);

            float distance1 = 0, distance2 = 0;

            /// All vertices are on the same side:
            if (vert1Side == vert2Side && vert1Side == vert3Side)
            {
                AddTriangle(vert1Side, vert1, vert2, vert3);
            }
            /// Vertice 3 is on the other side:
            else if (vert1Side == vert2Side && vert1Side != vert3Side)
            {
                Vector3 vert4 = PlaneVectorIntersection(vert1, vert3, out distance1);
                Vector3 vert5 = PlaneVectorIntersection(vert2, vert3, out distance2);

                AddNewVertice(vert4, vert1, vert3, distance1);
                AddNewVertice(vert5, vert2, vert3, distance2);

                AddTriangle(vert1Side, vert1, vert5, vert4);
                AddTriangle(vert1Side, vert1, vert2, vert5);
                AddTriangle(vert3Side, vert4, vert5, vert3);
            }
            /// Vertice 2 is on the other side:
            else if (vert1Side == vert3Side && vert1Side != vert2Side)
            {
                Vector3 vert4 = PlaneVectorIntersection(vert1, vert2, out distance1);
                Vector3 vert5 = PlaneVectorIntersection(vert3, vert2, out distance2);

                AddNewVertice(vert4, vert1, vert2, distance1);
                AddNewVertice(vert5, vert3, vert2, distance2);

                AddTriangle(vert1Side, vert1, vert4, vert3);
                AddTriangle(vert1Side, vert3, vert4, vert5);
                AddTriangle(vert2Side, vert4, vert2, vert5);
            }
            /// Vertice 1 is on the other side:
            else if (vert2Side == vert3Side && vert2Side != vert1Side)
            {
                Vector3 vert4 = PlaneVectorIntersection(vert1, vert2, out distance1);
                Vector3 vert5 = PlaneVectorIntersection(vert1, vert3, out distance2);

                AddNewVertice(vert4, vert1, vert2, distance1);
                AddNewVertice(vert5, vert1, vert3, distance2);

                AddTriangle(vert2Side, vert5, vert4, vert3);
                AddTriangle(vert2Side, vert4, vert2, vert3);
                AddTriangle(vert1Side, vert1, vert4, vert5);
            }
        }

        SetUpObjects();
    }

    private void SetUpObjects()
    {
        Vector3[] vertices1 = verts1.ToArray();
        int[] triangles1 = tris1.ToArray();
        mesh1.vertices = vertices1; mesh1.triangles = triangles1; mesh1.normals = normals1.ToArray(); mesh1.uv = uvs1.ToArray();

        Vector3[] vertices2 = verts2.ToArray();
        int[] triangles2 = tris2.ToArray();
        mesh2.vertices = vertices2; mesh2.triangles = triangles2; mesh2.normals = normals2.ToArray(); mesh2.uv = uvs2.ToArray();

        GameObject positivePart = new GameObject(objectToSlice.name + " positive part");
        positivePart.transform.position = objectToSlice.transform.position;
        positivePart.transform.rotation = objectToSlice.transform.rotation;
        positivePart.transform.localScale = objectToSlice.transform.localScale;

        MeshFilter meshFilter1 = positivePart.AddComponent<MeshFilter>();
        meshFilter1.mesh = mesh1;

        positivePart.AddComponent<MeshRenderer>().material = originalMaterial;

        GameObject negativePart = new GameObject(objectToSlice.name + " negative part");
        negativePart.transform.position = objectToSlice.transform.position;
        negativePart.transform.rotation = objectToSlice.transform.rotation;
        negativePart.transform.localScale = objectToSlice.transform.localScale;

        MeshFilter meshFilter2 = negativePart.AddComponent<MeshFilter>();
        meshFilter2.mesh = mesh2;

        negativePart.AddComponent<MeshRenderer>().material = originalMaterial;

        GameObject.Destroy(objectToSlice.gameObject);
    }

    private void AddNewVertice(Vector3 vertice, Vector3 start, Vector3 end, float distance) /// Start and end of a vector on which new vertice lies
    {
        if (originalVert.IndexOf(vertice) > -1)
            return;

        originalVert.Add(vertice);

        originalNormals.Add(CalculateNormal(vertice, start, end));

        Vector2 uv = InterpolateUV(originalUVs[originalVert.IndexOf(start)], originalUVs[originalVert.IndexOf(end)], distance);
        originalUVs.Add(uv);
    }

    private void AddTriangle(bool isPositiveSide, Vector3 v1, Vector3 v2, Vector3 v3)
    {
        if (isPositiveSide)
        {
            int index1 = verts1.IndexOf(v1);
            int index2 = verts1.IndexOf(v2);
            int index3 = verts1.IndexOf(v3);

            /// Is there a need for additional side check?
            if (index1 == -1)
                index1 = AddVectorInfo(isPositiveSide, v1, originalNormals[originalVert.IndexOf(v1)], originalUVs[originalVert.IndexOf(v1)]);
            if (index2 == -1)
                index2 = AddVectorInfo(isPositiveSide, v2, originalNormals[originalVert.IndexOf(v2)], originalUVs[originalVert.IndexOf(v2)]);
            if (index3 == -1)
                index3 = AddVectorInfo(isPositiveSide, v3, originalNormals[originalVert.IndexOf(v3)], originalUVs[originalVert.IndexOf(v3)]);

            Debug.Log(originalVert.IndexOf(v1) + " " + originalVert.IndexOf(v2) + " " + originalVert.IndexOf(v3));

            tris1.Add(index1); tris1.Add(index2); tris1.Add(index3);
        }
        else
        {
            int index1 = verts2.IndexOf(v1);
            int index2 = verts2.IndexOf(v2);
            int index3 = verts2.IndexOf(v3);

            if (index1 == -1)
                index1 = AddVectorInfo(isPositiveSide, v1, originalNormals[originalVert.IndexOf(v1)], originalUVs[originalVert.IndexOf(v1)]);
            if (index2 == -1)
                index2 = AddVectorInfo(isPositiveSide, v2, originalNormals[originalVert.IndexOf(v2)], originalUVs[originalVert.IndexOf(v2)]);
            if (index3 == -1)
                index3 = AddVectorInfo(isPositiveSide, v3, originalNormals[originalVert.IndexOf(v3)], originalUVs[originalVert.IndexOf(v3)]);

            tris2.Add(index1); tris2.Add(index2); tris2.Add(index3);
        }
    }

    private int AddVectorInfo(bool isPositiveSide, Vector3 vertice, Vector3 normal, Vector3 uv)
    {
        if (isPositiveSide)
        {
            verts1.Add(vertice);
            normals1.Add(normal);
            uvs1.Add(uv);

            return verts1.IndexOf(vertice);
        }
        else
        {
            verts2.Add(vertice);
            normals2.Add(normal);
            uvs2.Add(uv);

            return verts2.IndexOf(vertice);
        }
    }

    private Vector3 PlaneVectorIntersection(Vector3 start, Vector3 end, out float distance)
    {
        Vector3 direction = end - start;
        Ray ray = new Ray(start, direction);

        float enter = 0;
        plane.Raycast(ray, out enter);

        distance = enter;

        return ray.GetPoint(enter);
    }

    private Vector2 InterpolateUV(Vector2 uv1, Vector2 uv2, float distance)
    { return Vector2.Lerp(uv1, uv2, distance); }

    private Vector3 CalculateNormal(Vector3 v1, Vector3 v2, Vector3 v3)
    {
        Vector3 side1 = v2 - v1;
        Vector3 side2 = v3 - v1;

        Vector3 normal = Vector3.Cross(side1, side2);

        return normal;
    }
}
