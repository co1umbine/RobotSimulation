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
        // TODO limit
        public void OnUpdateJointState(float state)
        {
            Quaternion rot = Quaternion.AngleAxis(state * Mathf.Rad2Deg, joint.axis);
            transform.localRotation = defaultRotation * rot;
        }

        public void UpdateJointStateHierarchical(List<float> states)
        {
            OnUpdateJointState(states[0]);
            states.Remove(states[0]);
            child?.UpdateJointStateHierarchical(states);
        }
    }
}