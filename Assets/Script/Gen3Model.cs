using Microsoft.Win32;
using RosSharp.RosBridgeClient;
using RosSharp.Urdf;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using UnityEngine;

namespace RobotSimulation
{
    public class Gen3Model : MonoBehaviour
    {
        [SerializeField] List<MyJoint> joints;  // 1~6
        [SerializeField, Range(-180, 180)] float angle1 = 0;
        [SerializeField, Range(-138.0828f, 138.0828f)] float angle2 = 0;
        [SerializeField, Range(-152.4068f, 152.4068f)] float angle3 = 0;
        [SerializeField, Range(-180, 180)] float angle4 = 0;
        [SerializeField, Range(-127.7696f, 127.7696f)] float angle5 = 0;
        [SerializeField, Range(-180, 180)] float angle6 = 0;
        [SerializeField] FKManager fk;
        [SerializeField] IKManager ik;

        [SerializeField] bool isIK = false;

        HomogeneourCoordinate hC;
        List<Matrix4x4> HTMs = new List<Matrix4x4>();
        Matrix4x4 EndHTM = new Matrix4x4();
        List<float> currentAngles;

        Coroutine ikCoroutine;

        void Start()
        {
            hC = GetComponent<HomogeneourCoordinate>();
            currentAngles = new List<float>() { angle1 * Mathf.Deg2Rad, angle2 * Mathf.Deg2Rad, angle3 * Mathf.Deg2Rad, angle4 * Mathf.Deg2Rad, angle5 * Mathf.Deg2Rad, angle6 * Mathf.Deg2Rad };
        }

        //IEnumerator StepUpdate()
        //{
        //    foreach (var joint in joints)
        //    {
        //        joint.OnUpdateJointState(0.0f * Mathf.PI);
        //    }

        //    var thetas = new List<float>();
        //    foreach (var joint in joints)
        //    {
        //        thetas.Add(joint.GetPosition());
        //    }
        //    print($"angles { thetas[0] * Mathf.Rad2Deg}, { thetas[1] * Mathf.Rad2Deg}, { thetas[2] * Mathf.Rad2Deg}, { thetas[3] * Mathf.Rad2Deg}, { thetas[4] * Mathf.Rad2Deg}, { thetas[5] * Mathf.Rad2Deg}");
        //    HTMs = hC.GetHTM(thetas);



        //    int time = 0;
        //    while (true)
        //    {
        //        foreach (var joint in joints)
        //        {
        //            joint.OnUpdateJointState(Mathf.Sin(time * Time.deltaTime) * Mathf.PI);
        //        }
        //        thetas = new List<float>();
        //        foreach (var joint in joints)
        //        {
        //            thetas.Add(joint.GetPosition());
        //        }
        //        print($"angles { thetas[0] * Mathf.Rad2Deg}, { thetas[1] * Mathf.Rad2Deg}, { thetas[2] * Mathf.Rad2Deg}, { thetas[3] * Mathf.Rad2Deg}, { thetas[4] * Mathf.Rad2Deg}, { thetas[5] * Mathf.Rad2Deg}");
        //        HTMs = hC.GetHTM(thetas);

        //        time++;
        //        yield return null;
        //    }
        //}

        // Update is called once per frame
        void Update()
        {

            
            if (!isIK)
            {

                //print($"angles { thetas[0] * Mathf.Rad2Deg}, { thetas[1] * Mathf.Rad2Deg}, { thetas[2] * Mathf.Rad2Deg}, { thetas[3] * Mathf.Rad2Deg}, { thetas[4] * Mathf.Rad2Deg}, { thetas[5] * Mathf.Rad2Deg}");

                currentAngles = new List<float>() { angle1 * Mathf.Deg2Rad, angle2 * Mathf.Deg2Rad, angle3 * Mathf.Deg2Rad, angle4 * Mathf.Deg2Rad, angle5 * Mathf.Deg2Rad, angle6 * Mathf.Deg2Rad };
                SetAngles(currentAngles);

                var readThetas = new List<float>();
                foreach (var joint in joints)
                {
                    readThetas.Add(joint.GetPosition());
                }

                HTMs = hC.GetHTMs(currentAngles);
                EndHTM = HTMs[HTMs.Count() - 1];
                fk.FK(HTMs);
            }
            else
            {
                if(ikCoroutine == null)
                {
                    ikCoroutine = StartCoroutine(IK());
                }
                SetAngles(currentAngles);
                angle1 = currentAngles[0] * Mathf.Rad2Deg;
                angle2 = currentAngles[1] * Mathf.Rad2Deg;
                angle3 = currentAngles[2] * Mathf.Rad2Deg;
                angle4 = currentAngles[3] * Mathf.Rad2Deg;
                angle5 = currentAngles[4] * Mathf.Rad2Deg;
                angle6 = currentAngles[5] * Mathf.Rad2Deg;
            }

        }

        IEnumerator IK()
        {
            while (true)
            {
                if (isIK)
                {
                    //var resultAngle = new List<float>(currentAngles);
                    yield return StartCoroutine(ik.IK(currentAngles, hC.LinkParams, fk));
                    //currentAngles = resultAngle;
                    //SetAngles(currentAngles);
                    //UnityEditor.EditorApplication.isPaused = true;
                    yield return null;
                }
            }
        }

        private void SetAngles(List<float> angle)
        {
            int i = 0;
            foreach (var joint in joints)
            {
                joint.OnUpdateJointState(angle[i]);
                i++;
            }
        }

        public Vector3 CurrentEndPosition()
        {
            return fk.GetEndPosition(EndHTM);
        }

        public double[,] CurrentEndRotation()
        {
            return EndHTM.RotationMatrix();
        }

        private void OnDrawGizmos()
        {
            Gizmos.color = Color.red;
            Vector3 origin = new Vector4(0, 0, 0, 1);
            Vector3 x1 = new Vector4(0.1f, 0, 0, 1);
            Gizmos.DrawLine(origin, x1);
            Gizmos.color = Color.blue;
            Vector3 z1 = new Vector4(0, 0, 0.1f, 1);
            Gizmos.DrawLine(origin, z1);
            int i = 0;
            foreach(var htm in HTMs)
            {
                //if (i == dispJoint) 
                //{
                Gizmos.color = Color.red;
                origin = htm * new Vector4(0, 0, 0, 1);
                x1 = htm * new Vector4(0.1f, 0, 0, 1);
                Gizmos.DrawLine(origin, x1);
                Gizmos.color = Color.blue;
                z1 = htm * new Vector4(0, 0, 0.1f, 1);
                Gizmos.DrawLine(origin, z1); 
                //}

                i++;
            }
        }
    }
}