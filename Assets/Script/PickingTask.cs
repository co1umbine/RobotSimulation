using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace RobotSimulation
{
    public class PickingTask : MonoBehaviour
    {
        [SerializeField] List<Transform> taskObjects;
        [SerializeField] Collider goalArea;
        [SerializeField] float threshold;

        [SerializeField] float p;
        [SerializeField] float pg = 0.01f;

        [SerializeField] float moveLoopMax = 10000;
        [SerializeField] Transform midPoint;
        Gen3Model robot;
        GripController grip;

        private void Awake()
        {
            robot = GetComponent<Gen3Model>();
            grip = GetComponent<GripController>();
        }

        private void Start()
        {
            StartCoroutine(TaskSequence());
        }

        IEnumerator TaskSequence()
        {
            foreach(var task in taskObjects)
            {
                robot.SetInControl(true);
                grip.SetInControl(true);

                var currentAngle = robot.GetAngle();
                yield return StartCoroutine(robot.CulcIK(currentAngle, task.position + new Vector3(0, 0.1f, 0), Quaternion.Euler(-90, 0, 0)));

                int moveloop = 0;
                print("moving");
                while(!IsCloseEnough(robot.GetAngle(), currentAngle) && moveloop < moveLoopMax)
                {
                    robot.SetAngle(AnglesLerp(robot.GetAngle(), currentAngle));
                    moveloop++;
                    yield return null;
                }


                print("culc");
                yield return StartCoroutine(robot.CulcIK(currentAngle, task.position + new Vector3(0, 0.01f, 0), Quaternion.Euler(-90, 0, 0)));

                moveloop = 0;
                print("moving");
                while (!IsCloseEnough(robot.GetAngle(), currentAngle) && moveloop < moveLoopMax)
                {
                    robot.SetAngle(AnglesLerp(robot.GetAngle(), currentAngle));
                    moveloop++;
                    yield return null;
                }

                moveloop = 0;
                print("moving");
                float gripWidth = 0;
                while(gripWidth < 0.3f)
                {
                    gripWidth += pg;
                    grip.SetWidth(gripWidth);
                    moveloop++;
                    yield return null;
                }


                print("culc");
                yield return StartCoroutine(robot.CulcIK(currentAngle, task.position + new Vector3(0, 0.01f, 0), Quaternion.Euler(-90, 0, 0)));

                moveloop = 0;
                print("moving");
                while (!IsCloseEnough(robot.GetAngle(), currentAngle) && moveloop < moveLoopMax)
                {
                    robot.SetAngle(AnglesLerp(robot.GetAngle(), currentAngle));
                    moveloop++;
                    yield return null;
                }


                print("culc");
                yield return StartCoroutine(robot.CulcIK(currentAngle, midPoint.position + new Vector3(0, 0.01f, 0), Quaternion.Euler(-90, 0, 0)));

                moveloop = 0;
                print("moving");
                while (!IsCloseEnough(robot.GetAngle(), currentAngle) && moveloop < moveLoopMax)
                {
                    robot.SetAngle(AnglesLerp(robot.GetAngle(), currentAngle));
                    moveloop++;
                    yield return null;
                }

                print("culc");
                yield return StartCoroutine(robot.CulcIK(currentAngle, goalArea.transform.position, Quaternion.Euler(-90, 0, 0)));

                moveloop = 0;
                print("moving");
                while (!IsCloseEnough(robot.GetAngle(), currentAngle) && moveloop < moveLoopMax)
                {
                    robot.SetAngle(AnglesLerp(robot.GetAngle(), currentAngle));
                    moveloop++;
                    yield return null;
                }

                moveloop = 0;
                print("moving");
                while (gripWidth >= 0)
                {
                    gripWidth -= pg;
                    grip.SetWidth(gripWidth);
                    moveloop++;
                    yield return null;
                }


                print("culc");
                yield return StartCoroutine(robot.CulcIK(currentAngle, midPoint.position + new Vector3(0, 0.01f, 0), Quaternion.Euler(-90, 0, 0)));

                moveloop = 0;
                print("moving");
                while (!IsCloseEnough(robot.GetAngle(), currentAngle) && moveloop < moveLoopMax)
                {
                    robot.SetAngle(AnglesLerp(robot.GetAngle(), currentAngle));
                    moveloop++;
                    yield return null;
                }
                yield return null;
            }

            robot.SetInControl(false);
            grip.SetInControl(false);

        }

        private bool IsCloseEnough(List<float> objects, List<float> targets)
        {
            for(int i =0; i <objects.Count(); ++i)
            {
                if(Mathf.Abs(objects[i] - targets[i]) > threshold)
                {
                    return false;
                }
            }
            return true;
        }
        private List<float> AnglesLerp(List<float> current, List<float> target)
        {
            var result = new List<float>();
            for(int i = 0; i < current.Count(); ++i)
            {
                result.Add(Mathf.Lerp(current[i], target[i], p * Time.deltaTime));
            }
            return result;
        }
    }
}