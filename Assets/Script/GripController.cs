using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

namespace RobotSimulation
{
    public class GripController : MonoBehaviour
    {
        [SerializeField] MyJoint leftInnerKnuckle;
        [SerializeField] MyJoint leftInnerFinger;
        [SerializeField] MyJoint leftOuterFinger;
        [SerializeField] MyJoint rightInnerKnuckle;
        [SerializeField] MyJoint rightInnerFinger;
        [SerializeField] MyJoint rightOuterFinger;
        [SerializeField, Range(0,1)] float width;
        [SerializeField, Range(0, 40)] float leftInnerAngle = 0;
        [SerializeField, Range(0, 40)] float leftInnerFingerAngle = 0;
        [SerializeField, Range(0, 36)] float leftOuterAngle = 0;
        [SerializeField, Range(0, 40)] float rightInnerAngle = 0;
        [SerializeField, Range(-40, 0)] float rightInnerFingerAngle = 0;
        [SerializeField, Range(0, 36)] float rightOuterAngle = 0;

        [SerializeField] bool isInControl = false;

        private void Start()
        {
            
        }

        private void FixedUpdate()
        {
            //SetAngles(leftInnerFinger, leftInnerFingerAngle * Mathf.Deg2Rad);
            //SetAngles(rightInnerFinger, rightInnerFingerAngle * Mathf.Deg2Rad);
            if (!isInControl)
            { 
                SetWidth(width);
            }
        }

        public void SetInControl(bool b)
        {
            isInControl = b;
        }

        public void SetWidth(float width)
        {
            leftInnerAngle = KeepTorpue(leftInnerKnuckle, Mathf.Lerp(0, 40, width) * Mathf.Deg2Rad);
            leftInnerFingerAngle = KeepTorpue(leftInnerFinger, Mathf.Lerp(0, 40, width) * Mathf.Deg2Rad);
            leftOuterAngle = KeepTorpue(leftOuterFinger, Mathf.Lerp(0, 36, width) * Mathf.Deg2Rad);
            rightInnerAngle = KeepTorpue(rightInnerKnuckle, Mathf.Lerp(0, 40, width) * Mathf.Deg2Rad);
            rightInnerFingerAngle = KeepTorpue(rightInnerFinger, Mathf.Lerp(0, -40, width) * Mathf.Deg2Rad);
            rightOuterAngle = KeepTorpue(rightOuterFinger, Mathf.Lerp(0, 36, width) * Mathf.Deg2Rad);
        }

        private void SetAngles(MyJoint joint, float angle)
        {
            joint.OnUpdateJointState(angle);
        }
        private float KeepTorpue(MyJoint joint, float angle)
        {
            joint.OnUpdateJointState(angle);
            return angle * Mathf.Rad2Deg;
        }
    }
}