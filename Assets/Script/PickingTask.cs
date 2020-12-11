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

        [SerializeField] float pg = 0.01f;

        [SerializeField] float maxMoveLoop = 10000;
        [SerializeField] List<Transform> midPoints;

        [SerializeField] float aveSpeed = 10;
        Gen3Model robot;
        GripController grip;

        FKManager fk;
        List<LinkParam> linkParams;
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
            fk = robot.GetFK();
            linkParams = robot.GetLinkParams();

            var taskCount = 0;

            int moveloop;
            foreach (var task in taskObjects)
            {
                robot.SetInControl(true);

                var midAngleList = new List<List<float>>();

                var taskAbove = task.position + new Vector3(0, 0.1f, 0);
                targetAngle = robot.GetAngles();

                if(taskCount == 0)
                {
                    yield return StartCoroutine(robot.CulcIK(targetAngle, taskAbove, Quaternion.Euler(-90, -90, 0)));

                    yield return StartCoroutine(TargetMove(taskAbove, targetAngle));
                }
                else
                {
                    midAngleList = new List<List<float>>();
                    foreach (var p in midPoints)
                    {
                        yield return StartCoroutine(robot.CulcIK(targetAngle, p.position, Quaternion.Euler(-90, 180, 0)));
                        midAngleList.Add(new List<float>(targetAngle));
                    }

                    midAngleList.Reverse();

                    yield return StartCoroutine(robot.CulcIK(targetAngle, taskAbove, Quaternion.Euler(-90, -90, 0)));
                    midAngleList.Add(new List<float>(targetAngle));

                    yield return StartCoroutine(TargetMove(taskAbove, midAngleList));
                }



                yield return StartCoroutine(robot.CulcIK(targetAngle, task.position + new Vector3(0, 0.01f, 0), Quaternion.Euler(-90, -90, 0)));

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

                yield return StartCoroutine(TargetMove(taskAbove, targetAngle));


                midAngleList = new List<List<float>>();
                foreach (var p in midPoints)
                {
                    yield return StartCoroutine(robot.CulcIK(targetAngle, p.position, Quaternion.Euler(-90, 180, 0)));
                    midAngleList.Add(new List<float>(targetAngle));
                }
                yield return StartCoroutine(robot.CulcIK(targetAngle, goalArea.transform.position, Quaternion.Euler(-90, 90, 0)));
                midAngleList.Add(new List<float>(targetAngle));



                yield return StartCoroutine(TargetMove(goalArea.position, midAngleList));




                //FKManager.Foreach(linkParams, targetAngle, fk);

                //yield return StartCoroutine(TargetMove(goalArea.position, targetAngle));

                

                moveloop = 0;
                print("gripping");
                while (gripWidth >= 0)
                {
                    gripWidth -= pg;
                    grip.SetWidth(gripWidth);
                    moveloop++;
                    yield return null;
                }
                ++taskCount;
            }

            targetAngle = targetAngle.Select(a => 0f).ToList();


            yield return StartCoroutine(AngleMove(targetAngle));

            robot.SetInControl(false);
        }

        IEnumerator TargetMove(Vector3 targetPos, List<float> targetAngle)
        {
            var startAngle = robot.GetAngles();
            var moveTime = GetMagnitude(robot.GetAngles(), targetAngle) / aveSpeed;

            var currentTime = 0.0f;
            int moveloop = 0;

            FKManager.Foreach(linkParams, targetAngle, fk);

            print("moving");

            while (!IsCloseEnough(robot.CurrentEndPosition(), targetPos) && moveloop < maxMoveLoop && currentTime < moveTime)
            {
                robot.SetAngle(AnglesDelta(robot.GetAngles(), startAngle, targetAngle, currentTime, moveTime));
                moveloop++;
                currentTime += Time.deltaTime;
                yield return null;
            }
        }
        IEnumerator TargetMove(Vector3 targetPos, List<List<float>> midsTargetAngles)
        {
            var startAngle = robot.GetAngles();

            var deltas = new List<float>();
            deltas.Add(GetMagnitude(startAngle, midsTargetAngles[0]));

            for (var i = 1; i < midsTargetAngles.Count(); ++i)
            {
                deltas.Add(GetMagnitude(midsTargetAngles[i - 1], midsTargetAngles[i]));
            }

            var allDelta = deltas.Sum();
            var phasies = deltas.Select(d => d / allDelta).ToList();
            var moveTime = allDelta / aveSpeed;

            var currentTime = 0.0f;
            int moveloop = 0;


            print("moving");

            int index = 0;

            FKManager.Foreach(linkParams, midsTargetAngles[index], fk);

            float offset = 0;
            float progress = 0;
            while (!IsCloseEnough(robot.CurrentEndPosition(), targetPos) && moveloop < maxMoveLoop && currentTime < moveTime)
            {
                if (progress >= 1)
                {
                    offset += phasies[index];
                    ++index;
                    startAngle = robot.GetAngles();

                    if (index >= midsTargetAngles.Count()) break;
                    FKManager.Foreach(linkParams, midsTargetAngles[index], fk);
                }
                robot.SetAngle(AnglesDeltaByPhase(robot.GetAngles(), startAngle, midsTargetAngles[index], offset, phasies[index], out progress, currentTime, moveTime));


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

            FKManager.Foreach(linkParams, targetAngle, fk);
            print("moving");

            var startAngle = robot.GetAngles();
            while (robot.GetAngles().Where(a => a != 0).Any() && moveloop < maxMoveLoop && currentTime < moveTime)
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

        private List<float> AnglesDelta(List<float> current, List<float> start, List<float> target, float currentTime, float targetTime)
        {
            var result = new List<float>();
            var t_d_tf = currentTime / targetTime;
            var rate = 10 * Mathf.Pow(t_d_tf, 3) - 15 * Mathf.Pow(t_d_tf, 4) + 6 * Mathf.Pow(t_d_tf, 5);

            for (int i = 0; i < start.Count(); ++i)
            {
                var xf_m_x0 = target[i] - start[i];

                var delta = xf_m_x0 * rate;

                result.Add(start[i] + delta);
            }
            return result;
        }

        private List<float> AnglesDeltaByPhase(List<float> current, List<float> start, List<float> midsTarget, float offset, float phase, out float progress, float currentTime, float targetTime)
        {
            var result = new List<float>();
            var t_d_tf = currentTime / targetTime;

            var rate = 10 * Mathf.Pow(t_d_tf, 3) - 15 * Mathf.Pow(t_d_tf, 4) + 6 * Mathf.Pow(t_d_tf, 5);
            progress = (rate - offset) / phase;

            for (int i = 0; i < start.Count(); ++i)
            {
                var xf_m_x0 = midsTarget[i] - start[i];
                var delta = xf_m_x0 * progress;

                result.Add(start[i] + delta);
            }
            return result;
        }
    }
}