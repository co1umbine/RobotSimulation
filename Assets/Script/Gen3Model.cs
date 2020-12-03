using Microsoft.Win32;
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

        [SerializeField] bool isInControl = false;
        [SerializeField] bool isIK = false;

        HomogeneourCoordinate hC;
        List<Matrix4x4> HTMs = new List<Matrix4x4>();
        Matrix4x4 EndHTM = new Matrix4x4();
        List<float> currentAngles;

        Coroutine ikCoroutine;

        private void Awake()
        {
            hC = GetComponent<HomogeneourCoordinate>();
            currentAngles = new List<float>() { angle1 * Mathf.Deg2Rad, angle2 * Mathf.Deg2Rad, angle3 * Mathf.Deg2Rad, angle4 * Mathf.Deg2Rad, angle5 * Mathf.Deg2Rad, angle6 * Mathf.Deg2Rad };
            fk.SetHC(hC);
        }

        void Start()
        {
            SetAngles(currentAngles);
        }


        // Update is called once per frame
        void FixedUpdate()
        {

            if (!isInControl)
            {
                if (!isIK)
                {
                    if (ikCoroutine != null)
                    {
                        StopCoroutine(ikCoroutine);
                        ikCoroutine = null;
                    }

                    //print($"angles { thetas[0] * Mathf.Rad2Deg}, { thetas[1] * Mathf.Rad2Deg}, { thetas[2] * Mathf.Rad2Deg}, { thetas[3] * Mathf.Rad2Deg}, { thetas[4] * Mathf.Rad2Deg}, { thetas[5] * Mathf.Rad2Deg}");

                    currentAngles = new List<float>() { angle1 * Mathf.Deg2Rad, angle2 * Mathf.Deg2Rad, angle3 * Mathf.Deg2Rad, angle4 * Mathf.Deg2Rad, angle5 * Mathf.Deg2Rad, angle6 * Mathf.Deg2Rad };
                    SetAngles(currentAngles);

                    var readThetas = new List<float>();
                    foreach (var joint in joints)
                    {
                        readThetas.Add(joint.GetPosition());
                    }

                    HTMs = hC.GetHTMs(readThetas);
                    EndHTM = HTMs[HTMs.Count() - 1];
                    fk.Foreach(HTMs);
                }
                else
                {
                    if (ikCoroutine == null)
                    {
                        ikCoroutine = StartCoroutine(IKAuto());
                    }
                    SetAngles(currentAngles);
                }
            }

        }

        IEnumerator IKAuto()
        {
            while (true)
            {
                if (isIK)
                {
                    yield return StartCoroutine(ik.IKAuto(currentAngles, hC.LinkParams, fk));
                    yield return null;
                }
            }
        }

        public IEnumerator CulcIK(List<float> resultAngles, Vector3 targetPos, Quaternion taregetRot)
        {
            yield return StartCoroutine(ik.CulcIK(resultAngles, hC.LinkParams, fk, targetPos, taregetRot));
        }

        public FKManager GetFK()
        {
            return fk;
        }

        public void SetInControl(bool b)
        {
            isInControl = b;
        }

        public void SetAngle(List<float> angles)
        {
            SetInControl(true);
            SetAngles(angles);
        }
        public List<float> GetAngle()
        {
            var readThetas = new List<float>();
            foreach (var joint in joints)
            {
                readThetas.Add(joint.GetPosition());
            }
            return readThetas;
        }

        private void SetAngles(List<float> angles)
        {
            int i = 0;
            foreach (var joint in joints)
            {
                joint.OnUpdateJointState(angles[i]);
                i++;
            }
            angle1 = angles[0] * Mathf.Rad2Deg;
            angle2 = angles[1] * Mathf.Rad2Deg;
            angle3 = angles[2] * Mathf.Rad2Deg;
            angle4 = angles[3] * Mathf.Rad2Deg;
            angle5 = angles[4] * Mathf.Rad2Deg;
            angle6 = angles[5] * Mathf.Rad2Deg;
        }

        public Vector3 CurrentEndPosition()
        {
            return fk.GetEndPosition(EndHTM);
        }

        public Quaternion CurrentEndRotation()
        {
            return EndHTM.rotation;
        }

        private void OnDrawGizmos()
        {
            Gizmos.color = Color.red;
            Vector3 origin = new Vector4(0, 0, 0, 1);
            Vector3 x1 = new Vector4(0.1f, 0, 0, 1);
            Gizmos.DrawLine(origin + transform.position, x1 + transform.position);
            Gizmos.color = Color.blue;
            Vector3 z1 = new Vector4(0, 0, 0.1f, 1);
            Gizmos.DrawLine(origin + transform.position, z1 + transform.position);
            int i = 0;
            foreach(var htm in HTMs)
            {
                Gizmos.color = Color.red;
                origin = htm * new Vector4(0, 0, 0, 1);
                x1 = htm * new Vector4(0.1f, 0, 0, 1);
                Gizmos.DrawLine(origin + transform.position, x1 + transform.position);
                Gizmos.color = Color.blue;
                z1 = htm * new Vector4(0, 0, 0.1f, 1);
                Gizmos.DrawLine(origin + transform.position, z1 + transform.position);

                i++;
            }
        }
    }
}