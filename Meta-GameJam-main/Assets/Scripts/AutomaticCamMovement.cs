using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class AutomaticCam : MonoBehaviour

{
    public Transform target; // Assign your player here in the Inspector

    public Vector3 offset = new Vector3(0f, 4.99f, -2.94f);
    private Quaternion fixedRotation = Quaternion.Euler(41.657f, 0f, 0f);

    void LateUpdate()
    {
        if (target == null) return;

        // Hard lock the camera to the player + offset
        transform.position = target.position + offset;

        // Lock the camera rotation
        transform.rotation = fixedRotation;
    }
}