using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace Flashunity.Shaper
{
    public class ExtrudeTriangles
    {
        public int[] Extrude(Mesh mesh, int[] selectedMeshTriangles, int[] selectedMeshTrianglesFirstIndices, Vector3[] edgesVerticesPairs, Vector3 direction)
        {
            if (selectedMeshTriangles.Length == 0)
                return new int[0];

            DisctonnectNotSelectedButWithMutualVerticesTriangles(mesh, selectedMeshTriangles, selectedMeshTrianglesFirstIndices);

//            return new int[0];

            var vertices = new List<Vector3>();
            var triangles = new List<int>();
            var adjasentIndices = new List<int>();
            var normals = new List<Vector3>();

            int index = 0;

            for (int i = 0; i < edgesVerticesPairs.Length; i += 2)
            {
                var v0 = edgesVerticesPairs [i];
                var v1 = edgesVerticesPairs [i + 1];
                var v2 = new Vector3(v1.x, v1.y, v1.z);
                var v3 = new Vector3(v0.x, v0.y, v0.z);

                vertices.Add(v0);
                vertices.Add(v1);
                vertices.Add(v2);
                vertices.Add(v3);


                var i0 = index;
                var i1 = index + 1;
                var i2 = index + 2;
                var i3 = index + 3;

                triangles.Add(i2);
                triangles.Add(i1);
                triangles.Add(i0);

                triangles.Add(i0);
                triangles.Add(i3);
                triangles.Add(i2);

                var n = GetNormal(v1 - v0, direction);
//                var n1 = GetNormal(v0, v3, v2);

                normals.Add(n);
                normals.Add(n);
                normals.Add(n);
                normals.Add(n);

                adjasentIndices.Add(i0);
                adjasentIndices.Add(i1);

                index += 4;
            }

            var meshVertices = new List<Vector3>(mesh.vertices);
            var meshTriangles = new List<int>(mesh.triangles);
            var meshNormals = new List<Vector3>(mesh.normals);

            var meshVerticesCount = meshVertices.Count;

            meshVertices.AddRange(vertices);
            meshNormals.AddRange(normals);

            for (int i = 0; i < triangles.Count; i++)
            {
                meshTriangles.Add(triangles [i] + meshVerticesCount);
            }

            for (int i = 0; i < adjasentIndices.Count; i++)
            {
                adjasentIndices [i] = adjasentIndices [i] + meshVerticesCount;
            }

            UpdateMesh(mesh, meshVertices.ToArray(), meshTriangles.ToArray(), meshNormals.ToArray());

            return GetSelectedIndices(selectedMeshTriangles, adjasentIndices.ToArray());
        }

        Vector3 GetNormal(Vector3 side1, Vector3 side2)
        {
            var v = Vector3.Cross(side1, side2);

            v.Normalize();

//            var perpLength = v.magnitude;
            //          v /= perpLength;

            return v;
        }



        Vector3 GetNormal(Vector3 v0, Vector3 v1, Vector3 v2)
        {
            var side1 = v1 - v0;
            var side2 = v2 - v0;

            var v = Vector3.Cross(side1, side2);

            var perpLength = v.magnitude;
            v /= perpLength;

            return v;
        }

        int[] GetSelectedIndices(int[] selectedMeshTriangles, int[] adjasentIndices)
        {
            var indices = new List<int>();

            for (int i = 0; i < selectedMeshTriangles.Length; i++)
            {
                var index = selectedMeshTriangles [i];

                if (!indices.Contains(index))
                    indices.Add(index);
            }            

            for (int i = 0; i < adjasentIndices.Length; i++)
            {
                var index = adjasentIndices [i];

                if (!indices.Contains(index))
                    indices.Add(index);
            }            

            return indices.ToArray();
        }

        void UpdateMesh(Mesh mesh, Vector3[] vertices, int[] triangles, Vector3[] normals)
        {
            mesh.Clear();
            mesh.vertices = vertices;
            mesh.triangles = triangles;
            mesh.normals = normals;

//            mesh.RecalculateNormals();
        }

        void DisctonnectNotSelectedButWithMutualVerticesTriangles(Mesh mesh, int[] selectedMeshTriangles, int[] selectedMeshTrianglesFirstIndices)
        {
            var meshTriangles = mesh.triangles;

            var nonSelectedMutualIndices = new List<int>();

            for (int i = 0; i < meshTriangles.Length; i += 3)
            {
                // the same mesh triangle from selected triangles
                if (IsFirstTriangleIndexInTriangles(i, selectedMeshTrianglesFirstIndices))
                    continue;

                var nonSelectedIndex0 = meshTriangles [i];
                var nonSelectedIndex1 = meshTriangles [i + 1];
                var nonSelectedIndex2 = meshTriangles [i + 2];
                            
                for (int j = 0; j < selectedMeshTriangles.Length; j += 3)
                {
                    var indices = GetMutualIndices(i, new int[]
                    {
                        nonSelectedIndex0,
                        nonSelectedIndex1,
                        nonSelectedIndex2
                    }, new int[]
                    {
                        selectedMeshTriangles [j],
                        selectedMeshTriangles [j + 1],
                        selectedMeshTriangles [j + 2]
                    });

                    if (indices.Length > 0)
                    {
                        AddUniqueIndicesOnly(nonSelectedMutualIndices, indices);
//                        nonSelectedMutualIndices.AddRange(indices);
                    }
                }
            }

            UpdateNonSelectedMutualIndices(mesh, nonSelectedMutualIndices.ToArray());
        }


        void AddUniqueIndicesOnly(List<int> list, int[] indices)
        {
            for (int i = 0; i < indices.Length; i++)
            {            
                var index = indices [i];

                if (!list.Contains(index))
                    list.Add(index);
            }
        }

        void UpdateNonSelectedMutualIndices(Mesh mesh, int[] nonSelectedMutualIndices)
        {
            var vertices = new List<Vector3>(mesh.vertices);
            var triangles = mesh.triangles;
            var normals = new List<Vector3>(mesh.normals);
            var uvs = new List<Vector2>(mesh.uv);

            var newVerticesIndices = new Dictionary<int, int>();
            //var colors32 = new List<Color32>(mesh.colors32);

            for (int i = 0; i < nonSelectedMutualIndices.Length; i++)
            {
                var indexPos = nonSelectedMutualIndices [i];

                var index = triangles [indexPos];

                if (newVerticesIndices.ContainsKey(index))
                {
                    triangles [indexPos] = newVerticesIndices [index];
                } else
                {
                    /*
                if (index >= mesh.vertices.Length)
                {
                    Debug.Log("i: " + i);
                    Debug.Log("indexPos: " + indexPos);
                    Debug.Log("index: " + index);
                    Debug.Log("mesh.vertices.length: " + mesh.vertices.Length);
                    continue;
                }
                */
//                    Debug.DebugBreak();

                    var v = mesh.vertices [index];
                    var n = mesh.normals [index];
                    var uv = mesh.uv [index];
                    //var color32 = mesh.colors32 [index];

                    vertices.Add(new Vector3(v.x, v.y, v.z));
                    normals.Add(new Vector3(n.x, n.y, n.z));
                    uvs.Add(new Vector2(uv.x, uv.y));
                    //colors32.Add(new Color(color32.r, color32.g, color32.b, color32.a));

                    var newVertexIndex = vertices.Count - 1;
                    triangles [indexPos] = newVertexIndex;

                    newVerticesIndices [index] = newVertexIndex;

                    //triangles [indexPos] = 0;//vertices.Count - 1;
                }
            }

            mesh.Clear();


            mesh.vertices = vertices.ToArray();
            mesh.normals = normals.ToArray();
            mesh.uv = uvs.ToArray();

            //mesh.colors32 = colors32.ToArray();
            mesh.triangles = triangles;
        }


        int[] GetMutualIndices(int meshNonSelectedTriangleFirstIndex, int[] meshNonSelectedTriangle, int[] meshSelectedTriangle)
        {
            var indices = new List<int>();

            for (int i = 0; i < 3; i++)
            {
                var nonSelectedIndex = meshNonSelectedTriangle [i];

                for (int j = 0; j < 3; j++)
                {
                    if (nonSelectedIndex == meshSelectedTriangle [j])
                        indices.Add(meshNonSelectedTriangleFirstIndex + i);
                }
            }

            return indices.ToArray();
        }

        bool IsFirstTriangleIndexInTriangles(int index, int[] selectedMeshTrianglesFirstIndices)
        {
            for (int i = 0; i < selectedMeshTrianglesFirstIndices.Length; i++)
            {
                if (selectedMeshTrianglesFirstIndices [i] == index)
                    return true;
            }

            return false;
        }

    }
}