using UnityEngine;
using System.Collections;

namespace Flashunity.Shaper
{
    public class BHighlight : MonoBehaviour
    {
        [SerializeField]
        GameObject selected;

        private Mesh selectedMesh;

        void Awake()
        {
            selectedMesh = selected.GetComponent<MeshFilter>().mesh;
            HideMesh();
        }

        public void ShowMesh(Transform parent, int[] triangles, Vector3[] vertices, Vector3 normal)
        {
            selected.SetActive(true);

            selected.transform.parent = parent;

//            selected.transform.localPosition = new Vector3(0f, 0f, 0f);
            selected.transform.localPosition = normal * 0.001f;// * Time.deltaTime;

            selected.transform.localRotation = Quaternion.identity;
            selected.transform.localScale = new Vector3(1.01f, 1.01f, 1.01f);

            selectedMesh.Clear();
            selectedMesh.vertices = vertices;

            Vector3[] normals = new Vector3[vertices.Length];
            for (int i = 0; i < vertices.Length; i++)
            {
                normals [i] = normal;
            }


            selectedMesh.normals = normals;

            selectedMesh.triangles = triangles;

//            selectedMesh.RecalculateNormals();

            selected.transform.parent = null;
        }

        public void HideMesh()
        {
            selected.SetActive(false);
        }
    }
}
