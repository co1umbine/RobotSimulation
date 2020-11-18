using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace RobotSimulation
{
    public class MyJoint : MonoBehaviour
    {
        private HingeJoint joint;
        [SerializeField] private MyJoint child;

        Quaternion defaultRotation;

        private void Start()
        {
            defaultRotation = transform.localRotation;
            joint = GetComponent<HingeJoint>();
        }

        public float GetPosition()
        {
            return Quaternion.Angle(defaultRotation, transform.localRotation) * Mathf.Deg2Rad;
        }
        public float GetVelocity()
        {
            return joint.velocity * Mathf.Deg2Rad;
        }
        public float GetEffort()
        {
            return joint.motor.force;
        }

        public void OnUpdateJointState(float state)
        {
            Quaternion rot = Quaternion.AngleAxis(state * Mathf.Rad2Deg, joint.axis);
            transform.rotation = transform.rotation * rot;
        }

        public void UpdateJointStateHierarchical(List<float> states)
        {
            OnUpdateJointState(states[0]);
            states.Remove(states[0]);
            child?.UpdateJointStateHierarchical(states);
        }
    }
}