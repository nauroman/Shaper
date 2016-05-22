using UnityEngine;
using System.Collections;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Deployment.Internal;


namespace Flashunity.Shaper
{
    public class SelectedTriangles
    {
        public Mesh mesh;
        MeshCollider hitCollider;

        public int[] triangles = new int[0];
        public Vector3[] vertices = new Vector3[0];
        public Vector2[] uv = new Vector2[0];

        public int[] selectedMeshTriangles = new int[0];
        public int[] selectedMeshTrianglesFirstIndicesPositions = new int[0];

        public int[] selectedAndAdjesentMeshVerticesIndices = new int[0];

        public Vector3 normal;


        public bool GetHit(RaycastHit hit, out Mesh mesh, out int hitTriangleIndex)
        {
            hitCollider = hit.collider as MeshCollider;

            if (hitCollider == null)
            {
                mesh = null;
                hitTriangleIndex = 0;
                return false;
                //   throw new Exception(hit.collider.gameObject.name + " has not a MeshCollider.");
            }

            hitTriangleIndex = hit.triangleIndex;
            hitCollider.sharedMesh = hit.collider.gameObject.GetComponent<MeshFilter>().mesh;
            mesh = hitCollider.sharedMesh;

            return true;
        }

        public void UpdateCollider()
        {
            if (hitCollider != null && mesh != null)
                hitCollider.sharedMesh = mesh;//hitCollider.gameObject.GetComponent<MeshFilter>().mesh;
        }

        public void Clear()
        {
            mesh = null;
            hitCollider = null;

            triangles = new int[0];
            vertices = new Vector3[0];
            uv = new Vector2[0];

            selectedMeshTriangles = new int[0];
            selectedMeshTrianglesFirstIndicesPositions = new int[0];
            selectedAndAdjesentMeshVerticesIndices = new int[0];
        }

        public bool Selected
        {
            get
            {
                return selectedMeshTriangles.Length > 0;
            }
        }

        public void SelectTriangle(Mesh mesh, int hitTriangleIndex)
        {
            this.mesh = mesh;

            var indexBegin = hitTriangleIndex * 3;

            var meshTriangles = mesh.triangles;

            var index0 = meshTriangles [indexBegin + 0];
            var index1 = meshTriangles [indexBegin + 1];
            var index2 = meshTriangles [indexBegin + 2];

            var meshVertices = mesh.vertices;

            var v0 = meshVertices [index0];
            var v1 = meshVertices [index1];
            var v2 = meshVertices [index2];

            var meshUV = mesh.uv;

            var uv0 = meshUV [index0];
            var uv1 = meshUV [index1];
            var uv2 = meshUV [index2];

            vertices = new Vector3[]
            {
                v0,
                v1,
                v2
            };

            uv = new Vector2[]{ uv0, uv1, uv2 };

            selectedMeshTriangles = new int[]{ index0, index1, index2 };
            selectedMeshTrianglesFirstIndicesPositions = new int[]{ indexBegin };

            var meshNormals = mesh.normals;
            normal = meshNormals [index0] + meshNormals [index1] + meshNormals [index2];
            normal.Normalize();

            triangles = new int[]{ 0, 1, 2 };
        }

        public void SelectQuad(Mesh mesh, int hitTriangleIndex, float threshold = 0)
        {
            this.mesh = mesh;

            var indexBegin = hitTriangleIndex * 3;

            var meshTriangles = mesh.triangles;
            var meshNormals = mesh.normals;

            var index0 = meshTriangles [indexBegin + 0];
            var index1 = meshTriangles [indexBegin + 1];
            var index2 = meshTriangles [indexBegin + 2];

            var meshVertices = mesh.vertices;

            var v0 = meshVertices [index0];
            var v1 = meshVertices [index1];
            var v2 = meshVertices [index2];

            Vector3[] longestSideNormals;

            var longestSideVertices = GetLongestSideVertices(v0, v1, v2, meshNormals [index0], meshNormals [index1], meshNormals [index2], out longestSideNormals);//, out meshNormals [index1], out meshNormals [index2]);

            if (longestSideVertices.Length == 0)
            {
                SelectTriangle(mesh, hitTriangleIndex);
                return;
            }

            int secontTriangleFirstIndexPosition;
            var secondTriangleIndices = GetSecondTriangleIndices(meshTriangles, meshVertices, meshNormals, indexBegin, longestSideVertices, longestSideNormals, threshold, out secontTriangleFirstIndexPosition);

            if (secondTriangleIndices.Length == 0)
            {
                SelectTriangle(mesh, hitTriangleIndex);
                return;
            }

            int index3 = secondTriangleIndices [0];
            int index4 = secondTriangleIndices [1];
            int index5 = secondTriangleIndices [2];

            var meshUV = mesh.uv;

            selectedMeshTriangles = new int[]
            {
                index0,
                index1,
                index2,
                index3,
                index4,
                index5
            };
                
            selectedMeshTrianglesFirstIndicesPositions = new int[]
            {
                indexBegin,
                secontTriangleFirstIndexPosition
            };

            normal = meshNormals [index0] + meshNormals [index1] + meshNormals [index2];
            normal.Normalize();

            UpdateTrianglesAndVertices(selectedMeshTriangles, meshVertices, meshUV);
        }

        Vector3[] GetLongestSideVertices(Vector3 v0, Vector3 v1, Vector3 v2, Vector3 n0, Vector3 n1, Vector3 n2, out Vector3[] normals)
        {
            var l01 = (v0 - v1).sqrMagnitude;
            var l12 = (v1 - v2).sqrMagnitude;
            var l20 = (v2 - v0).sqrMagnitude;

            if (l01 == l12 && l01 == l20)
            {
                normals = new Vector3[0];
                return new Vector3[0];
            }

            if (l01 >= l12 && l01 >= l20)
            {
                normals = new Vector3[]{ n0, n1 };
                return new Vector3[]{ v0, v1 };
            }
            
            if (l12 >= l01 && l12 >= l20)
            {
                normals = new Vector3[]{ n1, n2 };
                return new Vector3[]{ v1, v2 };
            }

            if (l20 >= l01 && l20 >= l12)
            {
                normals = new Vector3[]{ n2, n0 };
                return new Vector3[]{ v2, v0 };
            }
            normals = new Vector3[]{ n0, n1 };
            return new Vector3[]{ v0, v1 };
        }

        int[] GetSecondTriangleIndices(int[] triangles, Vector3[] vertices, Vector3[] normals, int skipTriangleIndexBegin, Vector3[] longestSideVertices, Vector3[] longestSideNormals, float threshold, out int secondTriangleIndexBeginPosition)
        {
            for (int i = 0; i < triangles.Length; i += 3)
            {
                if (i == skipTriangleIndexBegin)
                    continue;

                var index0 = triangles [i];
                var index1 = triangles [i + 1];
                var index2 = triangles [i + 2];

                Vector3[] v = new Vector3[]
                {
                    vertices [index0],
                    vertices [index1],
                    vertices [index2]
                };

                Vector3[] n = new Vector3[]
                {
                    normals [index0],
                    normals [index1],
                    normals [index2]
                };

                if (TriangleHasVertex(v, n, longestSideVertices [0], longestSideNormals [0], threshold) && TriangleHasVertex(v, n, longestSideVertices [1], longestSideNormals [1], threshold))
                {
                    secondTriangleIndexBeginPosition = i;
                    return new int[]{ index0, index1, index2 };
                }
            }

            secondTriangleIndexBeginPosition = 0;

            return new int[0];
        }

        bool TriangleHasVertex(Vector3[] vertices, Vector3[] normals, Vector3 vertex, Vector3 normal, float threshold)
        {
            for (int i = 0; i < vertices.Length; i++)
                if (vertices [i] == vertex)
                {
                    if (AreSimilarVectors(normals [i], normal, threshold))
                        return true;
                }

            return false;
        }


        public void SelectPlane(Mesh mesh, int hitTriangleIndex, float threshold = 0)
        {
            this.mesh = mesh;

            var indexBegin = hitTriangleIndex * 3;
            var meshTriangles = mesh.triangles;
            var meshNormals = mesh.normals;

            var index0 = meshTriangles [indexBegin + 0];
            var index1 = meshTriangles [indexBegin + 1];
            var index2 = meshTriangles [indexBegin + 2];

            var meshVertices = mesh.vertices;

            normal = meshNormals [index0] + meshNormals [index1] + meshNormals [index2];//) / 3;
            normal.Normalize();

            var v0 = meshVertices [index0];
            var v1 = meshVertices [index1];
            var v2 = meshVertices [index2];

            var meshUV = mesh.uv;

            Vector3 center = (v0 + v1 + v2) / 3;

            List<Triangle> sortedTriangles = GetSortedListTriangles(meshTriangles, meshVertices, meshNormals, center, normal, threshold);

            var t = new List<int>(){ index0, index1, index2 };

            var tMeshTrianglesFirstIndices = new List<int>() { indexBegin };

            List<Vector3> v = new List<Vector3>()
            {
                v0,
                v1,
                v2
            };

            for (int i = 1; i < sortedTriangles.Count; i++)
            {
                var tr = sortedTriangles [i];

                index0 = tr.indices [0];
                index1 = tr.indices [1];
                index2 = tr.indices [2];

                v0 = meshVertices [index0];
                v1 = meshVertices [index1];
                v2 = meshVertices [index2];

                if (IsTriangleFromTheSamePlane(v, normal, new Vector3[]
                {
                    v0,
                    v1,
                    v2
                }, new Vector3[]
                {
                    meshNormals [index0],
                    meshNormals [index1],
                    meshNormals [index2]
                }, threshold))
                {
                    t.Add(index0);
                    t.Add(index1);
                    t.Add(index2);

                    AddOnlyIfUnique(v, v0);
                    AddOnlyIfUnique(v, v1);
                    AddOnlyIfUnique(v, v2);

                    tMeshTrianglesFirstIndices.Add(tr.meshTriangleFirstIndexPosition);
                }
            }

            selectedMeshTrianglesFirstIndicesPositions = tMeshTrianglesFirstIndices.ToArray();

            UpdateTrianglesAndVertices(t.ToArray(), meshVertices, meshUV);
        }

        void UpdateTrianglesAndVertices(int[] t, Vector3[] v, Vector2[] meshUV)
        {
            List<int> lt = new List<int>();
            List<Vector3> lv = new List<Vector3>();
            List<Vector2> luv = new List<Vector2>();

            selectedMeshTriangles = new int[t.Length];

            for (int i = 0; i < t.Length; i++)
            {
                var ti = t [i];
                selectedMeshTriangles [i] = ti;

                var l = lv.Count;
                int index = AddOnlyIfUnique(lv, v [ti]);

                if (l != lv.Count)
                    luv.Add(meshUV [ti]);
                
                lt.Add(index);
            }
                
            triangles = lt.ToArray();
            vertices = lv.ToArray();
            uv = luv.ToArray();
        }

        List<Triangle> GetSortedListTriangles(int[] meshTriangles, Vector3[] meshVertices, Vector3[] meshNormals, Vector3 center, Vector3 normal, float threshold)
        {
            List<Triangle> t = new List<Triangle>();

            for (int i = 0; i < meshTriangles.Length; i += 3)
            {
                var index0 = meshTriangles [i];
                var index1 = meshTriangles [i + 1];
                var index2 = meshTriangles [i + 2];

                if (Triangle.IsOnPlane(new Vector3[]
                {
                    meshNormals [index0],
                    meshNormals [index1],
                    meshNormals [index2]
                }, normal, threshold))
                {
                    t.Add(new Triangle(i, new int[]{ index0, index1, index2 }, center, new Vector3[]
                    {
                        meshVertices [index0],
                        meshVertices [index1],
                        meshVertices [index2]
                    }));
                }
            }

            t.Sort();

            return t;
        }

        int AddOnlyIfUnique(List<Vector3> v, Vector3 vector)
        {
            for (int i = 0; i < v.Count; i++)
            {
                if (v [i] == vector)
                {
                    return i;
                }
            }

            v.Add(vector);
            return v.Count - 1;
        }

        bool IsTriangleFromTheSamePlane(List<Vector3> v, Vector3 n, Vector3[] vertices, Vector3[] normals, float threshold)
        {
            for (int i = 0; i < 3; i++)
            {
                if (!AreSimilarVectors(normals [i], n, threshold))
                    return false;

                var vertex = vertices [i];

                for (int j = 0; j < v.Count; j++)
                {
                    if (v [j] == vertex)
                        return true;
                }
            }

            return false;        
        }


        public static bool AreSimilarVectors(Vector3 v0, Vector3 v1, float threshold)
        {
            if (threshold == 0)
            {
                return v0 == v1;
            }
            return Mathf.Abs(v0.x - v1.x) <= threshold && Mathf.Abs(v0.y - v1.y) <= threshold && Mathf.Abs(v0.z - v1.z) <= threshold;
        }

        public void UpdateSelectedAndAdjesentMeshVerticesIndices()//Mesh mesh, Vector3[] selectedVertices)
        {
            List<int> indices = new List<int>();

            var selectedVertices = vertices;

            var meshVertices = mesh.vertices;

            for (int i = 0; i < meshVertices.Length; i++)
            {
                var v = meshVertices [i];

                for (int j = 0; j < selectedVertices.Length; j++)
                {
                    if (v == selectedVertices [j])
                    {
                        indices.Add(i);
                        break;
                    }
                }
            }

            selectedAndAdjesentMeshVerticesIndices = indices.ToArray();
        }

        public Vector3[] GetEdgesVerticesPairs(out Vector2[] edgesUVPairs)
        {
            List<Vector3> edgesTriangles = new List<Vector3>();
            List<Vector2> edgesUV = new List<Vector2>();

            var edgesIndices = GetEdgesIndices();

            for (int i = 0; i < edgesIndices.Length; i++)
            {
                var index = edgesIndices [i];

                edgesTriangles.Add(vertices [index]);
                edgesUV.Add(uv [index]);
            }

            edgesUVPairs = edgesUV.ToArray();

            return edgesTriangles.ToArray();
        }


        int[] GetEdgesIndices()
        {
            List<int> edgesIndices = new List<int>();

            for (int i = 0; i < triangles.Length; i += 3)
            {
                List<int> edgeIndices;

                IsEdgeTriangle(i, out edgeIndices);

                for (int j = 0; j < edgeIndices.Count; j++)
                {
                    edgesIndices.Add(edgeIndices [j]);
                }
            }

            return edgesIndices.ToArray();
        }

        void IsEdgeTriangle(int triangleIndex, out List<int> edgeIndices)
        {
            edgeIndices = new List<int>();

            var index0 = triangles [triangleIndex];
            var index1 = triangles [triangleIndex + 1];
            var index2 = triangles [triangleIndex + 2];

            bool b01 = false;
            bool b12 = false;
            bool b20 = false;

            for (int i = 0; i < triangles.Length; i += 3)
            {
                if (i == triangleIndex)
                    continue;

                var t0 = triangles [i];
                var t1 = triangles [i + 1];
                var t2 = triangles [i + 2];
                 
                if (TriangeHasIndex(t0, t1, t2, index0) && TriangeHasIndex(t0, t1, t2, index1))
                {
                    b01 = true;
                    if (b01 && b12 && b20)
                        return;
                }

                if (TriangeHasIndex(t0, t1, t2, index1) && TriangeHasIndex(t0, t1, t2, index2))
                {
                    b12 = true;
                    if (b01 && b12 && b20)
                        return;
                }

                if (TriangeHasIndex(t0, t1, t2, index2) && TriangeHasIndex(t0, t1, t2, index0))
                {
                    b20 = true;
                    if (b01 && b12 && b20)
                        return;
                }
            }

            if (!b01)
            {
                edgeIndices.Add(index0);
                edgeIndices.Add(index1);
            }

            if (!b12)
            {
                edgeIndices.Add(index1);
                edgeIndices.Add(index2);
            }

            if (!b20)
            {
                edgeIndices.Add(index2);
                edgeIndices.Add(index0);
            }

        }

        bool TriangeHasIndex(int t0, int t1, int t2, int index)
        {
            return t0 == index || t1 == index || t2 == index;
        }



    }

    class Triangle : IComparable<Triangle>
    {
        public float distance;

        public int[] indices;

        public int meshTriangleFirstIndexPosition;

        public static bool IsOnPlane(Vector3[] normals, Vector3 normal, float threshold)
        {
            for (int i = 0; i < 3; i++)
            {
                if (!SelectedTriangles.AreSimilarVectors(normals [i], normal, threshold))
                    return false;
            }

            return true;
        }

        public Triangle(int meshTriangleFirstIndexPosition, int[] indices, Vector3 center, Vector3[] vertices)
        {
            this.meshTriangleFirstIndexPosition = meshTriangleFirstIndexPosition;
            this.indices = indices;

            var tc = new Vector3();

            for (int i = 0; i < 3; i++)
            {
                tc += vertices [i];
            }

            tc = tc / 3;

            distance = Vector3.Distance(center, tc);
        }

        public int CompareTo(Triangle other)
        {
            if (this.distance < other.distance)
                return -1;
            else if (this.distance > other.distance)
                return 1;
            else
                return 0;
        }
    }


}
