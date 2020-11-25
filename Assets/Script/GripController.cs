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
        [SerializeField, Range(-40, 0)] float leftInnerAngle = 0;
        [SerializeField, Range(0, 40)] float leftInnerFingerAngle = 0;
        [SerializeField, Range(-36, 0)] float leftOuterAngle = 0;
        [SerializeField, Range(0, 40)] float rightInnerAngle = 0;
        [SerializeField, Range(-40, 0)] float rightInnerFingerAngle = 0;
        [SerializeField, Range(0, 36)] float rightOuterAngle = 0;

        private void Start()
        {
            
        }

        private void Update()
        {
            //SetAngles(leftInnerFinger, leftInnerFingerAngle * Mathf.Deg2Rad);
            //SetAngles(rightInnerFinger, rightInnerFingerAngle * Mathf.Deg2Rad);
            SetWidth(width);
        }

        private void SetWidth(float width)
        {
            KeepTorpue(leftInnerKnuckle, Mathf.Lerp(0, -40, width));
            KeepTorpue(leftInnerFinger, Mathf.Lerp(0, 40, width));
            KeepTorpue(leftOuterFinger, Mathf.Lerp(0, -36, width));
            KeepTorpue(rightInnerKnuckle, Mathf.Lerp(0, 40, width));
            KeepTorpue(rightInnerFinger, Mathf.Lerp(0, -40, width));
            KeepTorpue(rightOuterFinger, Mathf.Lerp(0, 36, width));
        }

        private void SetAngles(MyJoint joint, float angle)
        {
            joint.OnUpdateJointState(angle);
        }
        private void KeepTorpue(MyJoint joint, float angle)
        {
            joint.OnUpdateJointState(angle * Mathf.Deg2Rad);
        }
    }
}