using Microsoft.VisualBasic;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using UnityEngine;

namespace RobotSimulation
{
    [Serializable]
    public struct LinkParam
    {
        public float a;
        public float alphaPI;
        public float d;
        public float thetaPI;
    }
    public class HomogeneourCoordinate : MonoBehaviour
    {
        public List<LinkParam> LinkParams;
        public Matrix4x4 HTM;
        private List<float> deltaThetasDeg = new List<float>();

        // Start is called before the first frame update
        void Start()
        {
            foreach(var l in LinkParams)
            {
                deltaThetasDeg.Add(0);
            }
            GetHTM();
        }

        public List<Matrix4x4> GetHTM()
        {
            var returns = new List<Matrix4x4>();
            HTM = Matrix4x4.identity;
            int i = 0;
            foreach (var p in LinkParams)
            {
                var aMat = Matrix4x4.Translate(new Vector3(p.a, 0, 0));
                var alphaMat = Matrix4x4.Rotate(Quaternion.Euler(p.alphaPI * Mathf.PI * Mathf.Rad2Deg, 0, 0));
                var dMat = Matrix4x4.Translate(new Vector3(0, 0, p.d));
                var thetaMat = Matrix4x4.Rotate(Quaternion.Euler(0, 0, (p.thetaPI * Mathf.PI + deltaThetasDeg[i]) * Mathf.Rad2Deg));
                HTM = HTM * aMat * alphaMat * dMat * thetaMat;
                returns.Add(HTM);
                i++;
            }
            print(returns[returns.Count()-1].ToString());
            return returns;
        }
        public List<Matrix4x4> GetHTM(List<LinkParam> linkParams)
        {
            LinkParams = linkParams;
            return GetHTM();
        }

        public List<Matrix4x4> GetHTM(List<float> thetas)
        {
            if(thetas.Count() == LinkParams.Count())
            {
                deltaThetasDeg = thetas;
            }
            return GetHTM();
        }
    }
}