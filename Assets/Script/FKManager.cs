using RosSharp;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace RobotSimulation
{
    public class FKManager : MonoBehaviour
    {
        [SerializeField] Transform[] fowardK;
        Vector4 hE;

        [SerializeField] List<Vector4> pVecs;


        private void Start()
        {
            hE = pVecs[pVecs.Count() - 1];
        }

        public Vector4 GetEndVector()
        {
            return hE;
        }


        public static void Foreach(List<LinkParam> linkParams, List<float> angles, FKManager fk)
        {
            fk.Foreach(HomogeneourCoordinate.GetHTMs(linkParams, angles));
        }
        public void Foreach(List<Matrix4x4> HTMs)
        {
            for (var i = 0; i < HTMs.Count(); i++)
            {
                fowardK[i].localPosition = HTMs[i] * new Vector4(0, 0, 0, 1);
            }
            fowardK[fowardK.Length - 1].localPosition = HTMs[HTMs.Count() - 1] * hE;
        }

        public static Vector3 GetEndPosition(List<LinkParam> linkParams, List<float> angles, FKManager fk)
        {
            return HomogeneourCoordinate.GetHTMs(linkParams, angles)[~1] * fk.hE;
        }
        public Vector3 GetEndPosition(Matrix4x4 HTM)
        {
            return HTM * hE;
        }

        public BasicJacobian GetJacobian(List<Matrix4x4> HTMs)
        {
            return new BasicJacobian(HTMs.Select(m4x4 => m4x4.To3x3()).ToList(), pVecs.Select(v4 => (Vector3)v4).ToList());
        }
    }
}