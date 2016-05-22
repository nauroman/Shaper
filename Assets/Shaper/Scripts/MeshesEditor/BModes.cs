using UnityEngine;
using System.Collections;
using UnityEngine.UI;

namespace Flashunity.Shaper
{

    public class BModes : MonoBehaviour
    {
        public Toggle selectTriangleToggle;
        public Toggle selectQuadToggle;
        public Toggle selectPlaneToggle;

        public Toggle moveToggle;
        public Toggle extrudeToggle;

        public Toggle sculptMoveToggle;
        public Toggle sculptExtrudeToggle;

        internal EditorMode editorMode;

        internal bool extruded;

        void Awake()
        {
            if (selectMode != SelectMode.None)
            {
                editorMode = EditorMode.Select;
            }
        }

        public SelectMode selectMode
        {
            get
            {
                if (selectTriangleToggle.isOn)
                    return SelectMode.Triangles;
                if (selectQuadToggle.isOn)
                    return SelectMode.Quad;
                if (selectPlaneToggle.isOn)
                    return SelectMode.Plane;
                return SelectMode.None;
            }
        }

        public EditMode editMode
        {
            get
            {
                if (moveToggle.isOn)
                    return EditMode.Move;
                if (extrudeToggle.isOn)
                    return EditMode.Extrude;
                if (sculptMoveToggle.isOn)
                    return EditMode.SculptMove;
                if (sculptExtrudeToggle.isOn)
                    return EditMode.SculptExtrude;
                return EditMode.None;
            }
        }
    }

    public enum EditorMode
    {
        None,
        Select,
        Edit
    }

    public enum SelectMode
    {
        None,
        Triangles,
        Quad,
        Plane
    }

    public  enum EditMode
    {
        None,
        Move,
        Extrude,
        SculptMove,
        SculptExtrude
    }
}