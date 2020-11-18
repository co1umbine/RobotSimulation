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
        [SerializeField] List<JointStateReader> joints;  // 1~6
        //List<JointStateWriter> jointsW = new List<JointStateWriter>();

        //List<HingeJoint> hinge = new List<HingeJoint>();
        List<MyJoint> m_joints = new List<MyJoint>();

        [SerializeField] Transform[] fowardK;
        HomogeneourCoordinate hC;
        List<Matrix4x4> HTMs = new List<Matrix4x4>();
        Vector4 hE = new Vector4(0, 0, -0.14f, 1);
        [SerializeField] int dispJoint;

        void Start()
        {
            hC = GetComponent<HomogeneourCoordinate>();
            foreach(var j in joints)
            {
                m_joints.Add(j.GetComponent<MyJoint>());
            }
            StartCoroutine(StepUpdate());
        }

        IEnumerator StepUpdate()
        {
            var setter = new List<float>();
            foreach (var joint in m_joints)
            {
                setter.Add(0 * Mathf.PI);
            }
            m_joints[0].UpdateJointStateHierarchical(setter);

            var thetas = new List<float>();
            foreach (var joint in m_joints)
            {
                thetas.Add(joint.GetPosition());
            }
            print($"angles { thetas[0] * Mathf.Rad2Deg}, { thetas[1] * Mathf.Rad2Deg}, { thetas[2] * Mathf.Rad2Deg}, { thetas[3] * Mathf.Rad2Deg}, { thetas[4] * Mathf.Rad2Deg}, { thetas[5] * Mathf.Rad2Deg}");
            HTMs = hC.GetHTM(thetas);

            for (var i = 0; i < HTMs.Count(); i++)
            {
                fowardK[i].localPosition = HTMs[i] * new Vector4(0, 0, 0, 1);
            }
            fowardK[fowardK.Length - 1].localPosition = HTMs[HTMs.Count() - 1] * hE;

            UnityEditor.EditorApplication.isPaused = true;
            yield return null;


            setter = new List<float>();
            foreach (var joint in m_joints)
            {
                setter.Add(0.5f * Mathf.PI);
            }
            m_joints[0].UpdateJointStateHierarchical(setter);

            thetas = new List<float>();
            foreach (var joint in m_joints)
            {
                thetas.Add(joint.GetPosition());
            }
            print($"angles { thetas[0] * Mathf.Rad2Deg}, { thetas[1] * Mathf.Rad2Deg}, { thetas[2] * Mathf.Rad2Deg}, { thetas[3] * Mathf.Rad2Deg}, { thetas[4] * Mathf.Rad2Deg}, { thetas[5] * Mathf.Rad2Deg}");
            HTMs = hC.GetHTM(thetas);

            for (var i = 0; i < HTMs.Count(); i++)
            {
                fowardK[i].localPosition = HTMs[i] * new Vector4(0, 0, 0, 1);
            }
            fowardK[fowardK.Length - 1].localPosition = HTMs[HTMs.Count() - 1] * hE;
            UnityEditor.EditorApplication.isPaused = true;
            yield return null;

            
        }

        // Update is called once per frame
        void Update()
        {
            /*for(int i =0; i<joints.Count(); i++)
            {
                fowardK[i].position = joints[i].transform.position;
            }
            fowardK[fowardK.Length - 1].position = joints[joints.Count() - 1].transform.position + joints[joints.Count()-1].transform.rotation * new Vector3(hE.x, hE.z, hE.y);
            */
            /*
            foreach (var joint in jointsW)
            {
                joint.Write(0.5f * Mathf.PI);
            }
            var thetas = new List<float>();
            foreach (var joint in hinge)
            {
                thetas.Add(0.5f * Mathf.PI);
            }
            print($"angles { thetas[0] * Mathf.Rad2Deg}, { thetas[1] * Mathf.Rad2Deg}, { thetas[2] * Mathf.Rad2Deg}, { thetas[3] * Mathf.Rad2Deg}, { thetas[4] * Mathf.Rad2Deg}, { thetas[5] * Mathf.Rad2Deg}");
            HTMs = hC.GetHTM(thetas);

            for(var i = 0; i < HTMs.Count(); i++)
            {
                fowardK[i].localPosition = HTMs[i] * new Vector4(0, 0, 0, 1);
            }
            fowardK[fowardK.Length - 1].localPosition = HTMs[HTMs.Count() - 1] * hE;
            UnityEditor.EditorApplication.isPaused = true;*/
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