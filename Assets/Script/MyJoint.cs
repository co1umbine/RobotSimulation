using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace RobotSimulation
{
    public class MyJoint : MonoBehaviour
    {
        private Rigidbody rb;
        private HingeJoint joint;
        private ArticulationBody articulation;
        [SerializeField] private float pGain = 1;

        Quaternion defaultRotation;
        float prevState;
        //Quaternion currentRotation;

        private void Awake()
        {
            joint = GetComponent<HingeJoint>();
            articulation = GetComponent<ArticulationBody>();

            defaultRotation = transform.localRotation;
        }
        private void Start()
        {
        }

        public float GetPosition()
        {
            if (joint)
                return joint.angle * Mathf.Deg2Rad;
            else
                return articulation.jointPosition[0];  // 一軸回転座標系なので x:= 回転ラジアン


            //return prevState;
            var rotationV = transform.localRotation * Vector3.forward;
            var defaultRotationV = defaultRotation * Vector3.forward;
            return Vector3.SignedAngle(defaultRotationV, rotationV, transform.InverseTransformDirection(joint.axis)) * Mathf.Deg2Rad;
        }
        public float GetVelocity()
        {
            return joint.velocity * Mathf.Deg2Rad;
        }
        public float GetEffort()
        {
            return joint.motor.force;
        }
        // TODO limit
        public void OnUpdateJointState(float state)
        {
            var drive = articulation.xDrive;
            drive.target = state * Mathf.Rad2Deg;
            articulation.xDrive = drive;
            return;

            joint.useSpring = false;
            prevState = state;
            Quaternion rot = Quaternion.AngleAxis(state * Mathf.Rad2Deg, joint.axis);
            transform.localRotation = defaultRotation * rot;
        }

        public void KeepTorpue(float state)
        {

        }
    }
}