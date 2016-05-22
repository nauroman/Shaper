using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace Flashunity.Shaper
{
    public class MoveTriangles
    {
        public void Move(Mesh mesh, int[] selectedMeshVerticesIndices, Vector3 move)
        {
            if (selectedMeshVerticesIndices.Length == 0)
                return;
            
            var vertices = mesh.vertices;
            var v = new Vector3[vertices.Length];

            for (int i = 0; i < vertices.Length; i++)
            {
                v [i] = vertices [i];
            }

            for (int i = 0; i < selectedMeshVerticesIndices.Length; i++)
            {
                v [selectedMeshVerticesIndices [i]] += move;
            }

            mesh.vertices = v;
        }

    }
}