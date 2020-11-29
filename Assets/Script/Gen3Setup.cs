using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace RobotSimulation
{
    public class Gen3Setup : MonoBehaviour
    {
        [SerializeField] private float stiffness = 100000;
        [SerializeField] private float damping = 10000;
        [SerializeField] private float forceLimit = 10000;
        [SerializeField] private float speed = 5f;
        [SerializeField] private float torque = 100f;
        [SerializeField] private float acceleration = 5f;

        private void Awake()
        {
            var robot = GetComponent<Gen3Model>();
            var grip = GetComponent<GripController>();

            var armList = new List<MyJoint>();
            foreach (ArticulationBody joint in GetComponentsInChildren<ArticulationBody>())
            {
                if (joint.jointType != ArticulationJointType.FixedJoint)
                {
                    if (armList.Count() < 6)
                    {
                        armList.Add(joint.gameObject.AddComponent<MyJoint>());
                        var drive = joint.xDrive;
                        drive.stiffness = stiffness;
                        drive.damping = damping;
                        drive.forceLimit = forceLimit;
                    }
                }


            }
        }

        void Start()
        {
        }

        // Update is called once per frame
        void Update()
        {

        }
    }
}