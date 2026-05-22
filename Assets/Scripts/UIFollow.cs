using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIFollow : MonoBehaviour
{
    [Tooltip("该UI是不是世界空间UI")]
    public bool isWorldUI;
    
    public Camera observationCamera;
    public Transform target;
    public Vector3 offset;
    

    // Update is called once per frame
    void Update()
    {
        if (target == null)
        {
            return;
        }

        if (observationCamera == null)
        {
            observationCamera = Camera.main;
        }

        if (!isWorldUI)
        {
            transform.position = observationCamera.WorldToScreenPoint(target.position + offset);
        }
        else
        {
            transform.position = target.position + offset;
        }

    }
}
