using UnityEngine;
using System.Collections;
using System;
using System.Security.Principal;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Collections.Specialized;

namespace Flashunity.Shaper
{

    public class BTest : MonoBehaviour
    {
        public Toggle moveTrianglesToggle;
        public Toggle extrudeTrianglesToggle;
        public Toggle splitToggle;

        public GameObject selected;

        private Mesh selectedMesh;

        public float selectRate = 0.1f;
        public float selectThreshold = 0.3f;
        public float moveRate = 0.01f;
        private float nextSelectTime;

        private SelectedTriangles selectedTriangles = new SelectedTriangles();

        //        private int[] selectedTriangles = { };

        EditMode editMode
        {
            get
            {
                if (moveTrianglesToggle.isOn)
                    return EditMode.MoveTriangles;
                if (extrudeTrianglesToggle.isOn)
                    return EditMode.ExtrudeTriangles;
                if (splitToggle.isOn)
                    return EditMode.SplitTriangles;
                return EditMode.None;
            }
        }

        void UpdateSelection()
        {
            if (editMode != EditMode.None)
            {
                SelectTriangles(false);
            } else
                HideSelectionShape();
        }

        void SelectTriangles(bool immideately)
        {
            if (immideately || Time.time > nextSelectTime)
            {                    
                nextSelectTime = Time.time + selectRate;

                RaycastHit hit;
                if (GetRaycastHit(out hit))
                {
                    if (selectedTriangles == null)
                        selectedTriangles = new SelectedTriangles();

                    Mesh mesh;
                    int triangleIndex;

                    selectedTriangles.GetHit(hit, out mesh, out triangleIndex);

                    selectedTriangles.SelectPlane(mesh, triangleIndex, selectThreshold);

                    ShowSelectedMesh(hit.collider.transform, selectedTriangles.triangles, selectedTriangles.vertices, hit.normal);
                } else
                    HideSelectionShape();
            }
        }

        void MoveTriangles()
        {
            var moveTriangles = new Flashunity.Shaper.MoveTriangles();

            moveTriangles.Move(selectedTriangles.mesh, SelectedTriangles.GetSelectedAndAdjesentMeshVerticesIndices(selectedTriangles.mesh, selectedTriangles.vertices), selectedTriangles.normal * moveRate);

            SelectTriangles(true);
        }

        void ExtrudeTriangles()
        {
            var extrudeTriangles = new ExtrudeTriangles();

            var edgesVerticesPairs = selectedTriangles.GetEdgesVerticesPairs();

            //         Debug.Log("edgesIndeces: " + edgesVerticesPairs.Length);

            var indices = extrudeTriangles.Extrude(selectedTriangles.mesh, selectedTriangles.selectedMeshTriangles, selectedTriangles.selectedMeshTrianglesFirstIndices, edgesVerticesPairs, selectedTriangles.normal);

            extrudeTrianglesToggle.isOn = false;
            moveTrianglesToggle.isOn = true;

            var moveTriangles = new Flashunity.Shaper.MoveTriangles();

            moveTriangles.Move(selectedTriangles.mesh, indices, selectedTriangles.normal * (moveRate == 0f ? 0.1f : moveRate));

            selectedTriangles.Clear();

            SelectTriangles(true);
        }

        void HideSelectionShape()
        {
            selected.SetActive(false);
        }


        void ShowSelectedMesh(Transform parent, int[] triangles, Vector3[] vertices, Vector3 normal)
        {
            selected.SetActive(true);

            if (selectedMesh == null)
                selectedMesh = selected.GetComponent<MeshFilter>().mesh;

            selected.transform.parent = parent;

//            selected.transform.localPosition = new Vector3(0f, 0f, 0f);
            selected.transform.localPosition = normal * 0.01f * Time.deltaTime;

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

            //        selectedMesh.RecalculateNormals();

            selected.transform.parent = null;

            //    selected.transform.position += normal * 0.01f * Time.deltaTime;

//            selectedMesh.normals = normals;

        }


        bool GetRaycastHit(out RaycastHit hit)
        {
            //      Vector3 rayOrigin = Camera.main.ViewportToWorldPoint(new Vector3(.5f, .5f, 0));
//        if (Physics.Raycast(rayOrigin, Camera.main.transform.forward, out hit, 100))
            
            return Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out hit);
        }

        void Update()
        {
            UpdateSelection();

            if (Input.GetButton("Fire1"))
            {
                if (selectedTriangles != null && selectedTriangles.triangles.Length > 0)
                {
                    switch (editMode)
                    {
                        case EditMode.MoveTriangles:
                            MoveTriangles();
                            break;
                        case EditMode.ExtrudeTriangles:
                            ExtrudeTriangles();
                            break;
                    }
                }
                
            } else
            {                
                //           UpdateSelection();
            }
        }
    }


    enum EditMode
    {
        None,
        MoveTriangles,
        ExtrudeTriangles,
        SplitTriangles
    }

}