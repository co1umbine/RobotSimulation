using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace RobotSimulation
{
    public class FKManager : MonoBehaviour
    {
        [SerializeField] Transform[] fowardK;
        [SerializeField] Vector4 hE = new Vector4(0, 0, -0.14f, 1);

        public void FK(List<Matrix4x4> HTMs)
        {
            for (var i = 0; i < HTMs.Count(); i++)
            {
                fowardK[i].localPosition = HTMs[i] * new Vector4(0, 0, 0, 1);
            }
            fowardK[fowardK.Length - 1].localPosition = HTMs[HTMs.Count() - 1] * hE;
        }
    }
}