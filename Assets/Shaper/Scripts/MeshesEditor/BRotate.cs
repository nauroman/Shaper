using UnityEngine;
using System.Collections;

public class BRotate : MonoBehaviour
{
    public float speed = 5.0f;

    void Update()
    {
        transform.Rotate(Vector3.up, 5.0f * Time.deltaTime);
    }
}
