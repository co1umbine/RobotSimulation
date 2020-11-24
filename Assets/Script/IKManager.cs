using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System;

namespace RobotSimulation
{
    public class IKManager : MonoBehaviour
    {
        [SerializeField] private Transform targetObject;
        [SerializeField] private double w_n = 1e-3;
        [SerializeField] private double w_e_pos = 1;
        [SerializeField] private double w_e_rot = 1;

        private double[,] W_e = new double[6, 6] { 
            { 1, 0, 0, 0, 0, 0 },
            { 0, 1, 0, 0, 0, 0 },
            { 0, 0, 1, 0, 0, 0 },
            { 0, 0, 0, 1, 0, 0 },
            { 0, 0, 0, 0, 1, 0 },
            { 0, 0, 0, 0, 0, 1 } };
        [SerializeField] private double threshold = 1e-12;

        public IEnumerator IK(List<float> angles, List<LinkParam> linkParams, FKManager fk)
        {
            int culcCount = 0;
            double[,] q = angles.ToVertical();

            double[,] prev_e = new double[6, 1];

            while (true/*culcCount < 100000*/)
            {

                var HTMs = HomogeneourCoordinate.GetHTMs(linkParams, angles);

                var J = fk.GetJacobian(HTMs).Matrix;
                var J_T = J.Transpose();

                var EndHTM = HTMs[HTMs.Count() - 1];
                var e_q = Error(fk.GetEndPosition(EndHTM), EndHTM.RotationMatrix());

                for (int i = 0; i < 3; i++)
                {
                    W_e[i, i] = w_e_pos;
                }
                for (int i = 3; i < 6; i++)
                {
                    W_e[i, i] = w_e_rot;
                }

                var Jt_W_J_plus_Wn__inv = (J_T.Times(W_e).Times(J).Plus(W_n(e_q, W_e))).inverseMatrix();


                var delta_q = Jt_W_J_plus_Wn__inv.Times(J_T).Times(W_e).Times(e_q);

                q = q.Plus(delta_q);

                for (int i = 0; i < q.GetLength(0); i++)
                {
                    angles[i] = (float)q[i, 0];
                }

                if (IsSmallEnough(delta_q, threshold))
                    yield break;

                if (IsSmallEnough(e_q.Minus(prev_e), threshold))
                    yield break;


                prev_e = e_q;

                culcCount++;

                if (culcCount % 100 == 0)
                {
                    yield return null;
                }
            }
            print("loop num: "+culcCount);
            yield break;
        }

        private double[,] Error(Vector3 currentPosition, double[,] currentRotation)
        {
            Vector3 p_error = targetObject.position - currentPosition;
            Vector3 r_error = G(targetObject.localToWorldMatrix.RotationMatrix().Times(currentRotation.Transpose()));
            return new double[6, 1] { { p_error.x }, { p_error.y }, { p_error.z }, { r_error.x }, { r_error.y }, { r_error.z } };
        }

        private Vector3 G(double[,] A)
        {
            Vector3 l = new Vector3((float)(A[2, 1] - A[1, 2]), (float)(A[0, 2] - A[2, 0]), (float)(A[1, 0] - A[0, 1] ));
            var numerator = Mathf.Atan2(l.magnitude, (float)(A[0, 0] + A[1, 1] + A[2, 2] - 1));
            var coefficient = numerator / l.magnitude;
            return coefficient * l;
        }

        private double[,] W_n(double[,] e, double[,] W_e)
        {
            var I = new double[,] {
                { 1, 0, 0, 0, 0, 0 },
                { 0, 1, 0, 0, 0, 0 },
                { 0, 0, 1, 0, 0, 0 },
                { 0, 0, 0, 1, 0, 0 },
                { 0, 0, 0, 0, 1, 0 },
                { 0, 0, 0, 0, 0, 1 }
            };

            var et_W_e = e.Transpose().Times(W_e).Times(e)[0, 0];

            return et_W_e.ScalarTimes(I).Plus(w_n.ScalarTimes(I));
        }

        private bool IsSmallEnough(double[,] V, double threshold)
        {
            double sqrSum = 0;
            foreach(var v in V)
            {
                sqrSum += Math.Pow(v, 2);
            }
            if (sqrSum > Math.Pow(threshold, 2))
                return false;
            return true;
        }
    }
}