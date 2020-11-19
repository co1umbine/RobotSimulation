using RosSharp;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace RobotSimulation
{
    public class BasicJacobian
    {
        double[,] matrix = new double[6, 6];

        public double[,] Matrix { get { return matrix; } }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="rotationMats">0~R_1 --- 0~R_6  6個</param>
        /// <param name="pVecs">1~p_1+1 --- 6~p_E  6個</param>
        public BasicJacobian(List<Matrix3x3> rotationMats, List<Vector3> pVecs)
        {
            Vector3 prev_p_Ei = Vector3.zero;
            for(int i = matrix.GetLength(1)-1; i>=0; i--)
            {
                Vector3 z_i = rotationMats[i] * new Vector3(0, 0, 1);
                Vector3 p_Ei = prev_p_Ei + rotationMats[i] * pVecs[i];

                Vector3 zxp = Vector3.Cross(z_i, p_Ei);

                matrix[0, i] = zxp.x;
                matrix[1, i] = zxp.y;
                matrix[2, i] = zxp.z;
                matrix[3, i] = z_i.x;
                matrix[4, i] = z_i.y;
                matrix[5, i] = z_i.z;

                prev_p_Ei = p_Ei;
            }
        }
    }
}