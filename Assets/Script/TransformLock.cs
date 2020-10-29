using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace RobotSimulation
{
    public class TransformLock : MonoBehaviour
    {
        Vector3 keepPosition;
        Quaternion keepRotation;
        Vector3 keepScale;
        void Start() 
        { 
        
            keepPosition = transform.position;
            keepRotation = transform.rotation;
            keepScale = transform.localScale;
        }

        // Update is called once per frame
        void Update()
        {
            transform.position = keepPosition;
            transform.rotation = keepRotation;
            transform.localScale = keepScale;
        }
    }
}
