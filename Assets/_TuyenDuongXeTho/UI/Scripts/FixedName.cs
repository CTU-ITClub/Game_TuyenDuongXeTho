using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FixedName : MonoBehaviour
{
    private Camera cam;

    void Start()
    {
        cam = Camera.main;
    }

    void LateUpdate()
    {
        if (cam == null) return;

        // Luôn quay mặt về camera
        transform.forward = cam.transform.forward;
    }
}

