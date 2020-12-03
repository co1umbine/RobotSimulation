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
        [SerializeField] float accel = 1;
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
            FKManager fk = robot.GetFK();
            List<LinkParam> linkParams = robot.GetLinkParams();
            int moveloop;
            foreach (var task in taskObjects)
            {
                robot.SetInControl(true);

                var taskAbove = task.position + new Vector3(0, 0.1f, 0);
                targetAngle = robot.GetAngle();
                yield return StartCoroutine(robot.CulcIK(targetAngle, taskAbove, Quaternion.Euler(-90, -90, 0)));

                FKManager.Foreach(linkParams, targetAngle, fk);
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

                FKManager.Foreach(linkParams, targetAngle, fk);
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
                while(gripWidth < 0.28f)
                {
                    gripWidth += pg;
                    grip.SetWidth(gripWidth);
                    moveloop++;
                    yield return null;
                }


                print("culc");
                yield return StartCoroutine(robot.CulcIK(targetAngle, taskAbove, Quaternion.Euler(-90, -90, 0)));

                FKManager.Foreach(linkParams, targetAngle, fk);
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

                FKManager.Foreach(linkParams, targetAngle, fk);
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

                FKManager.Foreach(linkParams, targetAngle, fk);
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

                FKManager.Foreach(linkParams, targetAngle, fk);
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

            targetAngle = targetAngle.Select(a => 0f).ToList();

            FKManager.Foreach(linkParams, targetAngle, fk);
            moveloop = 0;
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
            if((lhs - rhs).magnitude > threshold)
            {
                //print($"error {(lhs - rhs).magnitude}, betw {lhs}, {rhs}");
                return false;
            }
            return true;
        }

        List<float> speed;
        private List<float> AnglesCurve(List<float> current, List<float> start, List<float> target)
        {
            speed = speed ?? new List<float>() { 0, 0, 0, 0, 0, 0};
            var fordebug = new List<float>();
            var result = new List<float>();

            for(int i = 0; i < start.Count(); ++i)
            {
                var dist = target[i] - start[i];
                var prog = current[i] - start[i];
                var diff = target[i] - current[i];


                var sign = Mathf.Sign(diff * prog);

                if (sign > 0 && Mathf.Abs(diff) > Mathf.Abs(dist))
                {
                    start[i] = current[i];
                    dist = target[i] - start[i];
                    prog = current[i] - start[i];
                    diff = target[i] - current[i];
                    speed[i] = 0;
                }


                var accelRate = curve.Evaluate(Mathf.Clamp(1 - Mathf.Abs(diff / dist), 0, 1));

                fordebug.Add(diff / dist);

                speed[i] = Mathf.Clamp(speed[i] + accel * accelRate, 0, maxSpeed);
                result.Add(current[i] + Mathf.Sign(diff) * Mathf.Abs(dist) * speed[i] * Time.deltaTime);

                if (i == 1)
                {
                    print($"i: {i}, c {current[i]}, s {start[i]}, t {target[i]}, dist {dist}, speed {speed[i]}");
                }

                // // result.add(mathf.lerp(current[i], target[i], p * time.deltatime));
            }
            // Debug.Log($"progress rate is {fordebug[0]}, {fordebug[1]}, {fordebug[2]}, {fordebug[3]}, {fordebug[4]}, {fordebug[5]}");
            return result;
        }
    }
}