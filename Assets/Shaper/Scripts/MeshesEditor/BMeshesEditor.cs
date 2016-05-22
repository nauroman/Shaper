using UnityEngine;
using System.Collections;
using System;
using System.Security.Principal;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Collections.Specialized;

namespace Flashunity.Shaper
{

    public class BMeshesEditor : MonoBehaviour
    {
        BModes modes;

        BHighlight highlight;


        [Range(0.0f, 1.0f)]
        public float selectDelay = 0.1f;

        [Range(0.0f, 1.0f)]
        public float selectNormalThreshold = 0.3f;

        public float moveRate = 0.002f;
        float nextSelectTime;

        public float sculptRate = 0.03f;

        SelectedTriangles selected;

        Vector3 initialMousePosition = new Vector3();

        void Awake()
        {
            selected = new SelectedTriangles();
            modes = GetComponent<BModes>();
            highlight = GetComponent<BHighlight>();
        }


        void SelectTriangle(bool immideately)
        {
            if (immideately || Time.time > nextSelectTime)
            {                    
                nextSelectTime = Time.time + selectDelay;

                RaycastHit hit;
                if (GetRaycastHit(out hit))
                {
                    Mesh mesh;
                    int triangleIndex;

                    if (selected.GetHit(hit, out mesh, out triangleIndex))
                    {                        
                        selected.SelectTriangle(mesh, triangleIndex);
                        highlight.ShowMesh(hit.collider.transform, selected.triangles, selected.vertices, hit.normal);
                    } else
                    {
                        selected.Clear();
                        highlight.HideMesh();
                    }
                } else
                    highlight.HideMesh();
            }
        }

        void SelectQuad(bool immideately)
        {
            if (immideately || Time.time > nextSelectTime)
            {                    
                nextSelectTime = Time.time + selectDelay;

                RaycastHit hit;
                if (GetRaycastHit(out hit))
                {
                    Mesh mesh;
                    int triangleIndex;

                    if (selected.GetHit(hit, out mesh, out triangleIndex))
                    {
                        selected.SelectQuad(mesh, triangleIndex, selectNormalThreshold);
                        highlight.ShowMesh(hit.collider.transform, selected.triangles, selected.vertices, hit.normal);
                    }
                } else
                    highlight.HideMesh();
            }
        }

        void SelectPlane(bool immideately)
        {
            if (immideately || Time.time > nextSelectTime)
            {                    
                nextSelectTime = Time.time + selectDelay;

                RaycastHit hit;
                if (GetRaycastHit(out hit))
                {
                    Mesh mesh;
                    int triangleIndex;

                    if (selected.GetHit(hit, out mesh, out triangleIndex))
                    {
                        selected.SelectPlane(mesh, triangleIndex, selectNormalThreshold);
                        highlight.ShowMesh(hit.collider.transform, selected.triangles, selected.vertices, hit.normal);
                    } else
                    {
                        selected.Clear();
                        highlight.HideMesh();
                    }
                } else
                    highlight.HideMesh();
            }
        }


        bool GetRaycastHit(out RaycastHit hit)
        {
            //      Vector3 rayOrigin = Camera.main.ViewportToWorldPoint(new Vector3(.5f, .5f, 0));
//        if (Physics.Raycast(rayOrigin, Camera.main.transform.forward, out hit, 100))
            
            return Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out hit);
        }

        void ResetInitialMousePosition()
        {
            initialMousePosition = Input.mousePosition;
        }

        float GetMoveDistance()
        {
            return (Input.mousePosition.x - initialMousePosition.x) + (Input.mousePosition.y - initialMousePosition.y);
        }

        void MoveSelected()
        {
            var moveTriangles = new Flashunity.Shaper.MoveTriangles();

            moveTriangles.Move(selected.mesh, selected.selectedAndAdjesentMeshVerticesIndices, selected.normal * moveRate * GetMoveDistance());
            ResetInitialMousePosition();
        }



        void ExtrudeSelected()
        {
            var extrudeTriangles = new ExtrudeTriangles();

            Vector2[] edgesUVPairs;
            var edgesVerticesPairs = selected.GetEdgesVerticesPairs(out edgesUVPairs);

            var indices = extrudeTriangles.Extrude(selected.mesh, selected.selectedMeshTriangles, selected.selectedMeshTrianglesFirstIndicesPositions, edgesVerticesPairs, edgesUVPairs, selected.normal);

            selected.selectedAndAdjesentMeshVerticesIndices = indices;
        }

        void SculptMoveSelected()
        {
            var moveTriangles = new Flashunity.Shaper.MoveTriangles();
            moveTriangles.Move(selected.mesh, selected.selectedAndAdjesentMeshVerticesIndices, selected.normal * sculptRate);
        }

        void SculptExtrudeSelected()
        {
            var extrudeTriangles = new ExtrudeTriangles();

            Vector2[] edgesUVPairs;
            var edgesVerticesPairs = selected.GetEdgesVerticesPairs(out edgesUVPairs);

            var indices = extrudeTriangles.Extrude(selected.mesh, selected.selectedMeshTriangles, selected.selectedMeshTrianglesFirstIndicesPositions, edgesVerticesPairs, edgesUVPairs, selected.normal);

            selected.selectedAndAdjesentMeshVerticesIndices = indices;
        }

        void UpdateSelection()
        {
            switch (modes.selectMode)
            {
                case SelectMode.None:
                    selected.Clear();
                    highlight.HideMesh();
                    break;

                case SelectMode.Triangles:
                    SelectTriangle(false);
                    break;

                case SelectMode.Quad:
                    SelectQuad(false);
                    break;

                case SelectMode.Plane:
                    SelectPlane(false);
                    break;
            }
        }

        void Update()
        {
            UpdateModes();
        }

        void UpdateModes()
        {
            if (Input.GetButton("Fire1"))
            {
                switch (modes.editorMode)
                {
                    case EditorMode.None:
                        if (selected.Selected && modes.editMode != EditMode.None)
                        {
                            modes.editorMode = EditorMode.Edit;
                            selected.UpdateSelectedAndAdjesentMeshVerticesIndices();//selectedTriangles.mesh, selectedTriangles.vertices);
                            ResetInitialMousePosition();
                        }
                        break;

                    case EditorMode.Select:
                        if (selected.Selected && modes.editMode != EditMode.None)
                        {
                            modes.editorMode = EditorMode.Edit;
                            selected.UpdateSelectedAndAdjesentMeshVerticesIndices();//selectedTriangles.mesh, selectedTriangles.vertices);
                            ResetInitialMousePosition();
                        }
                        break;

                    case EditorMode.Edit:
                        if (selected.Selected)
                        {
                            switch (modes.editMode)
                            {
                                case EditMode.Move:

                                    MoveSelected();
                                    break;

                                case EditMode.Extrude:
                                    if (modes.extruded)
                                    {
                                        MoveSelected();
                                    } else
                                    {
                                        ExtrudeSelected();
                                        modes.extruded = true;
                                        MoveSelected();
                                    }
                                    break;

                                case EditMode.SculptMove:

                                    UpdateSelection();
                                    selected.UpdateCollider();
                                    selected.UpdateSelectedAndAdjesentMeshVerticesIndices();
                                    SculptMoveSelected();


                                    break;

                                case EditMode.SculptExtrude:

                                    UpdateSelection();
                                    selected.UpdateCollider();
                                    selected.UpdateSelectedAndAdjesentMeshVerticesIndices();
                                    SculptExtrudeSelected();
                                    SculptMoveSelected();

                                    break;


                            }

                        }

                        break;

                }
            } else
            {
                modes.extruded = false;


                switch (modes.editorMode)
                {
                    case EditorMode.None:
                        if (modes.selectMode != SelectMode.None)
                        {
                            modes.editorMode = EditorMode.Select;
                        }
                        break;

                    case EditorMode.Select:
                        if (modes.selectMode == SelectMode.None)
                        {
                            modes.editorMode = EditorMode.None;
                        }

                        break;

                    case EditorMode.Edit:
                        
                        selected.UpdateCollider();

                        if (modes.selectMode != SelectMode.None)
                        {
                            modes.editorMode = EditorMode.Select;
                        } else
                        {
                            modes.editorMode = EditorMode.None;
                        }
                        break;
                }

                UpdateSelection();

            }
        }
    }



}