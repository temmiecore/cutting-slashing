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

    bool isSharpShaded, isHollow;

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

        isSharpShaded = objectToSlice.isSharpShaded;
        isHollow = objectToSlice.isHollow;
    }

    public void Slice()
    {
        for (int i = 0; i < originalTris.Count; i += 3)
        {
            Vector3 vert1 = originalVert[originalTris[i]];
            Vector3 vert2 = originalVert[originalTris[i + 1]];
            Vector3 vert3 = originalVert[originalTris[i + 2]];

            Vector2 uv1 = originalUVs[originalTris[i]];
            Vector2 uv2 = originalUVs[originalTris[i+ 1]];
            Vector2 uv3 = originalUVs[originalTris[i+2]];

            Vector3 norm1 = originalNormals[originalTris[i]];
            Vector3 norm2 = originalNormals[originalTris[i + 1]];
            Vector3 norm3 = originalNormals[originalTris[i + 2]];

            bool vert1Side = plane.GetSide(vert1);
            bool vert2Side = plane.GetSide(vert2);
            bool vert3Side = plane.GetSide(vert3);

            Debug.Log(vert1Side + " " + vert2Side + " " + vert3Side);

            float distance1 = 0, distance2 = 0;

            /// Is normal calculation needed? Error doesn't seem to affect anything

            /// All vertices are on the same side:
            if (vert1Side == vert2Side && vert1Side == vert3Side)
            {
                CheckVertices(vert1Side, vert1, vert2, vert3, uv1, uv2, uv3, norm1, norm2, norm3);
            }
            /// Vertice 3 is on the other side:
            else if (vert1Side == vert2Side && vert1Side != vert3Side)
            {
                Vector3 vert4 = PlaneVectorIntersection(vert2, vert3, out distance1);
                Vector3 vert5 = PlaneVectorIntersection(vert1, vert3, out distance2);

                Vector2 uv4 = CalculateUV(uv2, uv3, distance1);
                Vector2 uv5 = CalculateUV(uv1, uv3, distance2);

                Vector3 norm4 = CalculateNormal(vert2, vert3, vert4);
                Vector3 norm5 = CalculateNormal(vert1, vert3, vert5);

                CheckVertices(vert1Side, vert5, vert1, vert4, uv5, uv1, uv4, norm5, norm1, norm4);
                CheckVertices(vert1Side, vert4, vert1, vert2, uv4, uv1, uv2, norm4, norm1, norm2);
                CheckVertices(vert3Side, vert3, vert5, vert4, uv3, uv5, uv4, norm3, norm5, norm4);
            }
            /// Vertice 2 is on the other side:
            else if (vert1Side == vert3Side && vert1Side != vert2Side)
            {
                Vector3 vert4 = PlaneVectorIntersection(vert1, vert2, out distance1);
                Vector3 vert5 = PlaneVectorIntersection(vert3, vert2, out distance2);

                Vector2 uv4 = CalculateUV(uv1, uv2, distance1);
                Vector2 uv5 = CalculateUV(uv3, uv2, distance2);

                Vector3 norm4 = CalculateNormal(vert1, vert2, vert4);
                Vector3 norm5 = CalculateNormal(vert3, vert2, vert5);

                CheckVertices(vert1Side, vert3, vert1, vert5, uv3, uv1, uv5, norm3, norm1, norm5);
                CheckVertices(vert1Side, vert5, vert1, vert4, uv5, uv1, uv4, norm5, norm1, norm4);
                CheckVertices(vert2Side, vert5, vert4, vert2, uv5, uv4, uv2, norm5, norm4, norm2);
            }
            /// Vertice 1 is on the other side:
            else if (vert2Side == vert3Side && vert2Side != vert1Side)
            {
                Vector3 vert4 = PlaneVectorIntersection(vert2, vert1, out distance1);
                Vector3 vert5 = PlaneVectorIntersection(vert3, vert1, out distance2);

                Vector2 uv4 = CalculateUV(uv2, uv1, distance1);
                Vector2 uv5 = CalculateUV(uv3, uv1, distance2);

                Vector3 norm4 = CalculateNormal(vert2, vert1, vert4);
                Vector3 norm5 = CalculateNormal(vert3, vert1, vert5);

                CheckVertices(vert2Side, vert3, vert5, vert4, uv3, uv5, uv4, norm3, norm5, norm4);
                CheckVertices(vert2Side, vert4, vert2, vert3, uv4, uv2, uv3, norm4, norm2, norm3);
                CheckVertices(vert1Side, vert5, vert1, vert4, uv5, uv1, uv4, norm5, norm1, norm4);
            }
        }

        SetUpObjects();
    }

    private void CheckVertices(bool isPositiveSide, Vector3 v1, Vector3 v2, Vector3 v3, Vector2 uv1, Vector2 uv2, Vector2 uv3, Vector3? n1, Vector3? n2, Vector3? n3)
    {
        int index1 = 0, index2 = 0, index3 = 0;

        if (isPositiveSide)
        {
            index1 = verts1.IndexOf(v1);

            if (index1 == -1 || (index1 > -1 && isSharpShaded))
            {
                verts1.Add(v1); uvs1.Add(uv1);

                if (n1 != null)
                    normals1.Add((Vector3)n1);

                index1 = verts1.Count - 1;
            }

            index2 = verts1.IndexOf(v2);

            if (index2 == -1 || (index2 > -1 && isSharpShaded))
            {
                verts1.Add(v2); uvs1.Add(uv2);

                if (n2 != null)
                    normals1.Add((Vector3)n2);

                index2 = verts1.Count - 1;
            }

            index3 = verts1.IndexOf(v3);

            if (index3 == -1 || (index3 > -1 && isSharpShaded))
            {
                verts1.Add(v3); uvs1.Add(uv3);

                if (n3 != null)
                    normals1.Add((Vector3)n3);

                index3 = verts1.Count - 1;
            }
        }
        else
        {
            index1 = verts2.IndexOf(v1);

            if (index1 == -1 || (index1 > -1 && isSharpShaded))
            {
                verts2.Add(v1); uvs2.Add(uv1);

                if (n1 != null)
                    normals2.Add((Vector3)n1);

                index1 = verts2.Count - 1;
            }

            index2 = verts2.IndexOf(v2);

            if (index2 == -1 || (index2 > -1 && isSharpShaded))
            {
                verts2.Add(v2); uvs2.Add(uv2);

                if (n2 != null)
                    normals2.Add((Vector3)n2);

                index2 = verts2.Count - 1;
            }

            index3 = verts2.IndexOf(v3);

            if (index3 == -1 || (index3 > -1 && isSharpShaded))
            {
                verts2.Add(v3); uvs2.Add(uv3);

                if (n3 != null)
                    normals2.Add((Vector3)n3);

                index3 = verts2.Count - 1;
            }
        }

        AddTriangle(isPositiveSide, index1, index2, index3);
    }

    private void AddTriangle(bool isPositiveSide, int index1, int index2, int index3)
    {
        if (isPositiveSide)
        {
            tris1.Add(index1); tris1.Add(index2); tris1.Add(index3);
        }
        else
        {
            tris2.Add(index1); tris2.Add(index2); tris2.Add(index3);
        }
    }

    /// Divide this into modules
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

        mesh1.RecalculateNormals();
        mesh2.RecalculateNormals();

        positivePart.AddComponent<Sliceable>().InitializeSliceable(objectToSlice);
        negativePart.AddComponent<Sliceable>().InitializeSliceable(objectToSlice);

        MeshCollider meshCollider1 = positivePart.AddComponent<MeshCollider>();
        MeshCollider meshCollider2 = negativePart.AddComponent<MeshCollider>();

        meshCollider1.convex = true;
        meshCollider2.convex = true;

        positivePart.AddComponent<Rigidbody>();
        negativePart.AddComponent<Rigidbody>();

        GameObject.Destroy(objectToSlice.gameObject);
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

    private Vector2 CalculateUV(Vector2 uv1, Vector2 uv2, float distance)
    { 
        return Vector2.Lerp(uv1, uv2, distance); 
    }

    private Vector3 CalculateNormal(Vector3 v1, Vector3 v2, Vector3 v3)
    {
        Vector3 side1 = v2 - v1;
        Vector3 side2 = v3 - v1;

        Vector3 normal = Vector3.Cross(side1, side2);

        return normal.normalized;
    }
}
