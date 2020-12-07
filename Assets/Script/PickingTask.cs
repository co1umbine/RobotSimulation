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

        [SerializeField] float maxMoveLoop = 10000;
        [SerializeField] Transform midPoint;

        [SerializeField] float aveSpeed = 10;
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
            FKManager fk = robot.GetFK();
            List<LinkParam> linkParams = robot.GetLinkParams();
            int moveloop;
            foreach (var task in taskObjects)
            {
                robot.SetInControl(true);

                var taskAbove = task.position + new Vector3(0, 0.1f, 0);
                targetAngle = robot.GetAngles();

                //[TODO] 躍度最小

                yield return StartCoroutine(robot.CulcIK(targetAngle, taskAbove, Quaternion.Euler(-90, -90, 0)));

                FKManager.Foreach(linkParams, targetAngle, fk);  // debug用に目標位置を赤玉で表示

                yield return StartCoroutine(TargetMove(taskAbove, targetAngle));



                yield return StartCoroutine(robot.CulcIK(targetAngle, task.position + new Vector3(0, 0.01f, 0), Quaternion.Euler(-90, -90, 0)));

                FKManager.Foreach(linkParams, targetAngle, fk);

                yield return StartCoroutine(TargetMove(task.position + new Vector3(0, 0.01f, 0), targetAngle));


                moveloop = 0;
                print("gripping");
                float gripWidth = 0;
                while(gripWidth < 0.28f)
                {
                    gripWidth += pg;
                    grip.SetWidth(gripWidth);
                    moveloop++;
                    yield return null;
                }


                yield return StartCoroutine(robot.CulcIK(targetAngle, taskAbove, Quaternion.Euler(-90, -90, 0)));

                FKManager.Foreach(linkParams, targetAngle, fk);

                yield return StartCoroutine(TargetMove(taskAbove, targetAngle));



                yield return StartCoroutine(robot.CulcIK(targetAngle, midPoint.position, Quaternion.Euler(-90, 180, 0)));

                FKManager.Foreach(linkParams, targetAngle, fk);

                yield return StartCoroutine(TargetMove(midPoint.position, targetAngle));

                

                yield return StartCoroutine(robot.CulcIK(targetAngle, goalArea.transform.position, Quaternion.Euler(-90, 90, 0)));

                FKManager.Foreach(linkParams, targetAngle, fk);

                yield return StartCoroutine(TargetMove(goalArea.position, targetAngle));

                

                moveloop = 0;
                print("gripping");
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

                yield return StartCoroutine(TargetMove(midPoint.position, targetAngle));
            }

            targetAngle = targetAngle.Select(a => 0f).ToList();

            FKManager.Foreach(linkParams, targetAngle, fk);

            yield return StartCoroutine(AngleMove(targetAngle));

            robot.SetInControl(false);
        }

        IEnumerator TargetMove(Vector3 targetPos, List<float> targetAngle)
        {
            var startAngle = robot.GetAngles();
            var moveTime = GetMagnitude(robot.GetAngles(), targetAngle) / aveSpeed;
            int moveloop = 0;

            var currentTime = 0.0f;


            print("moving");

            while (!IsCloseEnough(robot.CurrentEndPosition(), targetPos) && moveloop < maxMoveLoop && currentTime < moveTime)
            {
                robot.SetAngle(AnglesDelta(robot.GetAngles(), startAngle, targetAngle, currentTime, moveTime));
                moveloop++;
                currentTime += Time.deltaTime;
                yield return null;
            }
        }

        IEnumerator AngleMove(List<float> targetAngle)
        {
            int moveloop = 0;
            var moveTime = GetMagnitude(robot.GetAngles(), targetAngle) / aveSpeed;

            var currentTime = 0.0f;

            print("moving");

            var startAngle = robot.GetAngles();
            while (robot.GetAngles().Where(a => a != 0).Any() && moveloop < maxMoveLoop)
            {
                robot.SetAngle(AnglesDelta(robot.GetAngles(), startAngle, targetAngle, currentTime, moveTime));
                moveloop++;
                currentTime += Time.deltaTime;
                yield return null;
            }
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

        private float GetMagnitude(List<float> lhs, List<float> rhs)
        {
            return lhs
                .Zip(rhs, (la, ra) => (la, ra))
                .Aggregate(
                    0.0f,
                    (r, lra) => r + Mathf.Pow(lra.la - lra.ra, 2),
                    r => Mathf.Sqrt(r)
                );
        }

        List<float> speed;
        private List<float> AnglesDelta(List<float> current, List<float> start, List<float> target, float currentTime, float targetTime)
        {
            var fordebug = new List<float>();
            var result = new List<float>();
            var t_d_tf = currentTime / targetTime;

            for (int i = 0; i < start.Count(); ++i)
            {
                var xf_m_x0 = target[i] - start[i];

                var delta = xf_m_x0 * (10 * Mathf.Pow(t_d_tf, 3) - 15 * Mathf.Pow(t_d_tf, 4) + 6 * Mathf.Pow(t_d_tf, 5));

                result.Add(start[i] + delta);
            }
            return result;
        }
    }
}