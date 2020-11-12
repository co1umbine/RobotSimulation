using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace RobotSimulation
{
    public class TestRotate : MonoBehaviour
    {
        [SerializeField] Transform parent;
        [SerializeField] Slider target;

        HingeJoint hinge;
        float targetEuler;
        float defaultAngle;

        // Start is called before the first frame update
        void Start()
        {
            hinge = GetComponent<HingeJoint>();
            defaultAngle = transform.localRotation.x;
        }

        // なんちゃってサーボモーター
        void Update()
        {
            // オイラー角は、マイナスにならず　　2 => 1 => 0 => 359　=> 358 　となる
            var correctX = transform.localEulerAngles.x > 180 ? transform.localEulerAngles.x - 360 : transform.localEulerAngles.x;

            Debug.Log(correctX);


            if (correctX < target.value)
            {
                transform.RotateAround(parent.position + hinge.connectedAnchor, hinge.axis, 1);
            }
            else if (correctX > target.value)
            {
                transform.RotateAround(parent.position + hinge.connectedAnchor, hinge.axis, -1);
            }
        }
    }
}