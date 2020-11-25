using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace RobotSimulation
{
    public class MyJoint : MonoBehaviour
    {
        private HingeJoint joint;
        [SerializeField] private MyJoint child;
        [SerializeField] private float pGain = 1;

        Quaternion defaultRotation;
        float prevState;
        //Quaternion currentRotation;

        private void Awake()
        {
            joint = GetComponent<HingeJoint>();
            defaultRotation = transform.localRotation;
        }
        private void Start()
        {
        }

        public float GetPosition()
        {
            return prevState;
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
            prevState = state;
            Quaternion rot = Quaternion.AngleAxis(state * Mathf.Rad2Deg, joint.axis);
            transform.localRotation = defaultRotation * rot;
        }

        public void KeepTorpue(float state)
        {
            var e = state - GetPosition();
            var m = joint.motor;
            m.targetVelocity = e * pGain;
            joint.motor = m;
        }

        public void UpdateJointStateHierarchical(List<float> states)
        {
            OnUpdateJointState(states[0]);
            states.Remove(states[0]);
            child?.UpdateJointStateHierarchical(states);
        }
    }
}