using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace RobotSimulation
{
    public class PickingTask : MonoBehaviour
    {
        [SerializeField] List<Transform> taskObjects;
        [SerializeField] Transform goalArea;
        [SerializeField] float threshold;

        [SerializeField] float p;
        [SerializeField] float pg = 0.01f;

        [SerializeField] float moveLoopMax = 10000;
        [SerializeField] Transform midPoint;

        [SerializeField] float maxSpeed = 10;
        [SerializeField] AnimationCurve curve;
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
            List<float> targetAngle = new List<float>();
            List<float> startAngle;
            int moveloop;
            foreach (var task in taskObjects)
            {
                robot.SetInControl(true);

                var taskAbove = task.position + new Vector3(0, 0.1f, 0);
                targetAngle = robot.GetAngle();
                yield return StartCoroutine(robot.CulcIK(targetAngle, taskAbove, Quaternion.Euler(-90, -90, 0)));

                moveloop = 0;
                print("moving");
                startAngle = robot.GetAngle();
                while(!IsCloseEnough(robot.CurrentEndPosition(), taskAbove) && moveloop < moveLoopMax)
                {
                    robot.SetAngle(AnglesCurve(robot.GetAngle(), startAngle, targetAngle));
                    moveloop++;
                    yield return null;
                }


                print("culc");
                yield return StartCoroutine(robot.CulcIK(targetAngle, task.position + new Vector3(0, 0.01f, 0), Quaternion.Euler(-90, -90, 0)));

                moveloop = 0;
                print("moving");
                startAngle = robot.GetAngle();
                while (!IsCloseEnough(robot.CurrentEndPosition(), task.position + new Vector3(0, 0.01f, 0)) && moveloop < moveLoopMax)
                {
                    robot.SetAngle(AnglesCurve(robot.GetAngle(), startAngle, targetAngle));
                    moveloop++;
                    yield return null;
                }

                moveloop = 0;
                print("moving");
                float gripWidth = 0;
                while(gripWidth < 0.25f)
                {
                    gripWidth += pg;
                    grip.SetWidth(gripWidth);
                    moveloop++;
                    yield return null;
                }


                print("culc");
                yield return StartCoroutine(robot.CulcIK(targetAngle, taskAbove, Quaternion.Euler(-90, -90, 0)));

                moveloop = 0;
                print("moving");
                startAngle = robot.GetAngle();
                while (!IsCloseEnough(robot.CurrentEndPosition(), taskAbove) && moveloop < moveLoopMax)
                {
                    robot.SetAngle(AnglesCurve(robot.GetAngle(), startAngle, targetAngle));
                    moveloop++;
                    yield return null;
                }


                print("culc");
                yield return StartCoroutine(robot.CulcIK(targetAngle, midPoint.position, Quaternion.Euler(-90, 180, 0)));

                moveloop = 0;
                print("moving");
                startAngle = robot.GetAngle();
                while (!IsCloseEnough(robot.CurrentEndPosition(), midPoint.position) && moveloop < moveLoopMax)
                {
                    robot.SetAngle(AnglesCurve(robot.GetAngle(), startAngle, targetAngle));
                    moveloop++;
                    yield return null;
                }

                print("culc");
                yield return StartCoroutine(robot.CulcIK(targetAngle, goalArea.transform.position, Quaternion.Euler(-90, 90, 0)));

                moveloop = 0;
                print("moving");
                startAngle = robot.GetAngle();
                while (!IsCloseEnough(robot.CurrentEndPosition(), goalArea.position) && moveloop < moveLoopMax)
                {
                    robot.SetAngle(AnglesCurve(robot.GetAngle(), startAngle, targetAngle));
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
                yield return StartCoroutine(robot.CulcIK(targetAngle, midPoint.position, Quaternion.Euler(-90, 0, 0)));

                moveloop = 0;
                print("moving");
                startAngle = robot.GetAngle();
                while (!IsCloseEnough(robot.CurrentEndPosition(), midPoint.position) && moveloop < moveLoopMax)
                {
                    robot.SetAngle(AnglesCurve(robot.GetAngle(), startAngle, targetAngle));
                    moveloop++;
                    yield return null;
                }
                yield return null;
            }

            moveloop = 0;
            targetAngle = targetAngle.Select(a => 0f).ToList();
            print("moving");
            startAngle = robot.GetAngle();
            while (robot.GetAngle().Where(a => a!=0).Any() && moveloop < moveLoopMax)
            {
                robot.SetAngle(AnglesCurve(robot.GetAngle(), startAngle, targetAngle));
                moveloop++;
                yield return null;
            }

            robot.SetInControl(false);

        }

        private bool IsCloseEnough(Vector3 lhs, Vector3 rhs)
        {
            for(int i =0; i < 3; ++i)
            {
                if(Mathf.Abs(lhs[i] - rhs[i]) > threshold)
                {
                    return false;
                }
            }
            return true;
        }
        private List<float> AnglesCurve(List<float> current, List<float> start, List<float> target)
        {
            var result = new List<float>();
            for(int i = 0; i < start.Count(); ++i)
            {
                var d = target[i] - start[i];
                var progressRate = curve.Evaluate((current[i] - start[i]) / d);

                result.Add(current[i] + d * maxSpeed * progressRate * Time.deltaTime);
            }
            return result;
        }
    }
}