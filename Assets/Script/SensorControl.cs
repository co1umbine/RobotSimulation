using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace RobotSimulation
{
    public class SensorControl : MonoBehaviour
    {
        [SerializeField] Camera colorSensor;
        [SerializeField] Camera depthSensor;

        // Start is called before the first frame update
        void Start()
        {
            depthSensor.depthTextureMode |= DepthTextureMode.Depth;
        }

        // Update is called once per frame
        void Update()
        {

        }
    }
}