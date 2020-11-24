using RosSharp;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace RobotSimulation
{
    /// <summary>
    /// 出典: https://qiita.com/sekky0816/items/8c73a7ec32fd9b040127
    /// </summary>
    public static class MatrixUtil
    {
        public static double[,] Transpose(this double[,] A)
        {

            double[,] AT = new double[A.GetLength(1), A.GetLength(0)];

            for (int i = 0; i < A.GetLength(1); i++)
            {
                for (int j = 0; j < A.GetLength(0); j++)
                {
                    AT[i, j] = A[j, i];
                }
            }

            return AT;
        }

        public static double[,] Times(this double[,] A, double[,] B)
        {

            double[,] product = new double[A.GetLength(0), B.GetLength(1)];

            for (int i = 0; i < A.GetLength(0); i++)
            {
                for (int j = 0; j < B.GetLength(1); j++)
                {
                    for (int k = 0; k < A.GetLength(1); k++)
                    {
                        product[i, j] += A[i, k] * B[k, j];
                    }
                }
            }

            return product;
        }
        public static double[,] ScalarTimes(this double s, double[,] A)
        {

            double[,] product = new double[A.GetLength(0), A.GetLength(1)];

            for (int i = 0; i < A.GetLength(0); i++)
            {
                for (int j = 0; j < A.GetLength(1); j++)
                {
                    product[i, j] = A[i, j] * s;
                }
            }
            return product;
        }
        public static double[,] Plus(this double[,] A, double[,] B)
        {
            int n = A.GetLength(0);
            int m = A.GetLength(1);

            var result = new double[n, m];

            for (int i = 0; i < n; i++)
            {
                for (int j = 0; j < m; j++)
                {
                    result[i, j] = A[i, j] + B[i, j];
                }
            }
            return result;
        }

        public static double[,] Minus(this double[,] A, double[,] B)
        {
            int n = A.GetLength(0);
            int m = A.GetLength(1);

            var result = new double[n, m];

            for (int i = 0; i < n; i++)
            {
                for (int j = 0; j < m; j++)
                {
                    result[i, j] = A[i, j] - B[i, j];
                }
            }
            return result;
        }

        public static double[,] inverseMatrix(this double[,] A)
        {

            int n = A.GetLength(0);
            int m = A.GetLength(1);

            double[,] invA = new double[n, m];

            if (n == m)
            {

                int max;
                double tmp;

                for (int j = 0; j < n; j++)
                {
                    for (int i = 0; i < n; i++)
                    {
                        invA[j, i] = (i == j) ? 1 : 0;
                    }
                }

                for (int k = 0; k < n; k++)
                {
                    max = k;
                    for (int j = k + 1; j < n; j++)
                    {
                        if (Math.Abs(A[j, k]) > Math.Abs(A[max, k]))
                        {
                            max = j;
                        }
                    }

                    if (max != k)
                    {
                        for (int i = 0; i < n; i++)
                        {
                            // 入力行列側
                            tmp = A[max, i];
                            A[max, i] = A[k, i];
                            A[k, i] = tmp;
                            // 単位行列側
                            tmp = invA[max, i];
                            invA[max, i] = invA[k, i];
                            invA[k, i] = tmp;
                        }
                    }

                    tmp = A[k, k];

                    for (int i = 0; i < n; i++)
                    {
                        A[k, i] /= tmp;
                        invA[k, i] /= tmp;
                    }

                    for (int j = 0; j < n; j++)
                    {
                        if (j != k)
                        {
                            tmp = A[j, k] / A[k, k];
                            for (int i = 0; i < n; i++)
                            {
                                A[j, i] = A[j, i] - A[k, i] * tmp;
                                invA[j, i] = invA[j, i] - invA[k, i] * tmp;
                            }
                        }
                    }

                }


                //逆行列が計算できなかった時の措置
                for (int j = 0; j < n; j++)
                {
                    for (int i = 0; i < n; i++)
                    {
                        if (double.IsNaN(invA[j, i]))
                        {
                            Console.WriteLine("Error : Unable to compute inverse matrix");
                            invA[j, i] = 0;//ここでは，とりあえずゼロに置き換えることにする
                        }
                    }
                }


                return invA;

            }
            else
            {
                Console.WriteLine("Error : It is not a square matrix");
                return invA;
            }

        }

        public static double[,] RotationMatrix(this Matrix4x4 matrix)
        {
            return new double[3, 3] { { matrix[0, 0], matrix[0, 1], matrix[0, 2] }, { matrix[1, 0], matrix[1, 1], matrix[1, 2] }, { matrix[2, 0], matrix[2, 1], matrix[2, 2] } };
        }

        public static double[,] To3x1D(this Vector3 vector3)
        {
            return new double[,] { { vector3.x }, { vector3.y }, { vector3.z } };
        }

        public static Matrix3x3 To3x3(this Matrix4x4 m4x4)
        {
            return new Matrix3x3(new float[] { m4x4.m00, m4x4.m01, m4x4.m02, m4x4.m10, m4x4.m11, m4x4.m12, m4x4.m20, m4x4.m21, m4x4.m22 });
        }

        public static double[,] ToVertical(this IEnumerable<float> floats)
        {
            var result = new double[floats.Count(), 1];
            int i = 0;
            foreach(var f in floats)
            {
                result[i,0] = f;
                i++;
            }
            return result;
        }
    }
}