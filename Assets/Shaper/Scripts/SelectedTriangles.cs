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

        public int[] triangles = new int[0];
        public Vector3[] vertices = new Vector3[0];

        public int[] selectedMeshTriangles = new int[0];
        public int[] selectedMeshTrianglesFirstIndices = new int[0];
        //        public Vector3[] selectedMeshVertices;

        public Vector3 normal;


        public void GetHit(RaycastHit hit, out Mesh mesh, out int hitTriangleIndex)
        {
            var hitCollider = hit.collider as MeshCollider;

            if (hitCollider == null)
            {
                throw new Exception(hit.collider.gameObject.name + " has not a MeshCollider.");
            }

            hitTriangleIndex = hit.triangleIndex;
/*
            mesh = hitCollider.sharedMesh;
            if (mesh == null)
            {
            */
            hitCollider.sharedMesh = hit.collider.gameObject.GetComponent<MeshFilter>().mesh;
            mesh = hitCollider.sharedMesh;
//            }
        }

        public void Clear()
        {
            mesh = null;

            triangles = new int[0];
            vertices = new Vector3[0];

            selectedMeshTriangles = new int[0];
            selectedMeshTrianglesFirstIndices = new int[0];
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

            vertices = new Vector3[]
            {
                v0,
                v1,
                v2
            };

            selectedMeshTriangles = new int[]{ index0, index1, index2 };
            selectedMeshTrianglesFirstIndices = new int[]{ indexBegin };
            /*
            selectedMeshVertices = new Vector3[]
            {
                v0,
                v1,
                v2
            };
*/
            var meshNormals = mesh.vertices;
            normal = meshNormals [index0] + meshNormals [index1] + meshNormals [index2];
            normal.Normalize();

            triangles = new int[]{ 0, 1, 2 };
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

            Vector3 center = (v0 + v1 + v2) / 3;

            List<Triangle> sortedTriangles = GetSortedListTriangles(meshTriangles, meshVertices, meshNormals, center, normal, threshold);

            var t = new List<int>(){ index0, index1, index2 };

            var tMeshTrianglesFirstIndices = new List<int>() { indexBegin };

            List<Vector3> v = new List<Vector3>()
            {
                meshVertices [index0],
                meshVertices [index1],
                meshVertices [index2]
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

                    // doesn't work!!! it's sorted already :(
                    tMeshTrianglesFirstIndices.Add(tr.meshTriangleFirstIndex);
                }
            }

            selectedMeshTrianglesFirstIndices = tMeshTrianglesFirstIndices.ToArray();

//            Debug.Log("t.Count: " + t.Count);
            UpdateTrianglesAndVertices(t, meshVertices);

            //      Debug.Log("triangles.Length : " + triangles.Length);
            //    Debug.Log("vertices.Length : " + vertices.Length);

        }

        void UpdateTrianglesAndVertices(List<int> t, Vector3[] v)
        {
            List<int> lt = new List<int>();
            List<Vector3> lv = new List<Vector3>();

            selectedMeshTriangles = new int[t.Count];

            for (int i = 0; i < t.Count; i++)
            {
                selectedMeshTriangles [i] = t [i];

                int index = AddOnlyIfUnique(lv, v [t [i]]);
                lt.Add(index);
            }

            for (int i = 0; i < t.Count; i++)
            {
            }

            triangles = lt.ToArray();
            vertices = lv.ToArray();
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
                    return i;
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

        public static int[] GetSelectedAndAdjesentMeshVerticesIndices(Mesh mesh, Vector3[] selectedVertices)
        {
            List<int> indices = new List<int>();

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

            return indices.ToArray();
        }

        public Vector3[] GetEdgesVerticesPairs()
        {
            List<Vector3> edgesTriangles = new List<Vector3>();

            var edgesIndices = GetEdgesIndices();

            for (int i = 0; i < edgesIndices.Length; i++)
            {
                edgesTriangles.Add(vertices [edgesIndices [i]]);
            }

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

        /*
        public static int[] GetEdgesIndices(Mesh mesh, int[] selectedMeshTriangles, Vector3 normal)
        {
            List<int> edgesIndices = new List<int>();
            
            for (int i = 0; i < selectedMeshTriangles.Length; i += 3)
            {
                int[] edgeIndices;

                if (IsEdgeTriangle(mesh, selectedMeshTriangles, new int[]
                {
                    selectedMeshTriangles [i],
                    selectedMeshTriangles [i + 1],
                    selectedMeshTriangles [i + 2]
                }, i, out edgeIndices))
                {
                    edgesIndices.Add(edgeIndices [0]);
                    edgesIndices.Add(edgeIndices [1]);
                }
            }

            return edgesIndices.ToArray();
        }

        static bool IsEdgeTriangle(Mesh mesh, int[] selectedMeshTriangles, int[] triangle, int triangleIndex, out int[] edgeIndices)
        {
            edgeIndices = new int[2];

            for (int i=0; i<selectedMeshTriangles.Length; i+=3)
            {
                if (i == triangleIndex)
                    continue;

                var index0 = selectedMeshTriangles [i];
                var index1 = selectedMeshTriangles [i+1];
                var index2 = selectedMeshTriangles [i+2];

                if (selectedMeshTriangles[i])
            }


            return true;
        }



        static bool TriangleHasVertices(Mesh mesh, Vector3 vertex)
        {
            return true;
        }

        static bool TriangleHasVertex(Mesh mesh, Vector3 vertex)
        {

        }
        */

        /*
        public static int[] GetSelectedAndAdjesentMeshVerticesIndices(Mesh mesh, int[] selectedMeshTriangles)
        {
            List<int> indices = new List<int>();

            var meshTriangles = mesh.triangles;

            for (int i = 0; i < meshTriangles.Length; i += 3)
            {
                if (!IsTriangleInTriangles(selectedMeshTriangles, meshTriangles [i]))
                {
                    
                }
            }

            return indices.ToArray();
        }

        static bool IsTriangleInTriangles(int[] triangles, int triangleBeginIndex)
        {
            for (int i = 0; i < triangles.Length; i += 3)
            {
                if (triangles [i] == triangleBeginIndex)
                    return true;
            }

            return false;
        }

        static bool AreTrianglesHaveCommonVector(Mesh mesh, int[] triangle0, int[] triangle1)
        {
            
        }
*/
            

        /*
        public static int[] GetSelectedMeshVerticesIndices(int[] selectedMeshTriangles)
        {
            List<int> indices = new List<int>();

            for (int i = 0; i < selectedMeshTriangles.Length; i++)
            {
                int index = selectedMeshTriangles [i];

                if (!indices.Contains(index))
                    indices.Add(index);
            }

            return indices.ToArray();
        }
        */




    }

    class Triangle : IComparable<Triangle>
    {
        public float distance;

        public int[] indices;

        public int meshTriangleFirstIndex;

        public static bool IsOnPlane(Vector3[] normals, Vector3 normal, float threshold)
        {
            for (int i = 0; i < 3; i++)
            {
                if (!SelectedTriangles.AreSimilarVectors(normals [i], normal, threshold))
                    return false;
            }

            return true;
        }

        public Triangle(int meshTriangleFirstIndex, int[] indices, Vector3 center, Vector3[] vertices)
        {
            this.meshTriangleFirstIndex = meshTriangleFirstIndex;
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
