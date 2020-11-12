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
        public List<float> deltaThetasRad = new List<float>();

        // Start is called before the first frame update
        void Start()
        {
            foreach(var l in LinkParams)
            {
                deltaThetasRad.Add(0);
            }
            GetHTM();
        }

        public List<Matrix4x4> GetHTM()
        {
            var returns = new List<Matrix4x4>();
            HTM = Matrix4x4.identity;
            int i = 0;
            foreach (var param in LinkParams)
            {
                var aMat = Matrix4x4.Translate(new Vector3(param.a, 0, 0));
                var alphaMat = Matrix4x4.Rotate(Quaternion.Euler(param.alphaPI * Mathf.PI * Mathf.Rad2Deg, 0, 0));
                var dMat = Matrix4x4.Translate(new Vector3(0, 0, param.d));
                var thetaMat = Matrix4x4.Rotate(Quaternion.Euler(0, 0, (param.thetaPI * Mathf.PI + deltaThetasRad[i]) * Mathf.Rad2Deg));
                if (i == 1) { print("i:1 theta+pi/2 "+ ((param.thetaPI * Mathf.PI + deltaThetasRad[i]) * Mathf.Rad2Deg) + " theta2 " + (deltaThetasRad[i] * Mathf.Rad2Deg)); }
                print($"i: {i}\na\n{aMat.ToString()}\nalpha\n{alphaMat.ToString()}\nd\n{dMat.ToString()}\ntheta\n{thetaMat.ToString()}");

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
            //if(thetas.Count() == LinkParams.Count())
            //{
                deltaThetasRad = thetas;
            //}
            return GetHTM();
        }
    }
}