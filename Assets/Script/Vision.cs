using OpenCVForUnity.CoreModule;
using OpenCVForUnity.ImgprocModule;
using OpenCVForUnity.UnityUtils;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace RobotSimulation
{
    public class Vision : MonoBehaviour
    {
        [SerializeField] private RenderTexture renderTex;
        [SerializeField] private MeshRenderer output;
        [SerializeField] private Texture2D outputTex;

        [SerializeField, Range(0, 255)] private int hueMin = 0;
        [SerializeField, Range(0, 255)] private int hueMax = 255;
        [SerializeField, Range(0, 255)] private int satMin = 0;
        [SerializeField, Range(0, 255)] private int satMax = 255;
        [SerializeField, Range(0, 255)] private int valMin = 0;
        [SerializeField, Range(0, 255)] private int valMax = 255;

        Texture2D image;
        Texture2D texture;

        Mat bgrMat;
        Mat maskMat;
        Mat hsvMat;
        Mat resultMat;

        // Start is called before the first frame update
        void Start()
        {
            Utils.setDebugMode(true, false);
            image = new Texture2D(renderTex.width, renderTex.height, TextureFormat.ARGB32, false, false);

        }

        // Update is called once per frame
        void Update()
        {
            //tex�Ɠ����傫����Mat�i�����Ȃ�8bit�����^�A3�`�����l���j
            bgrMat = new Mat(renderTex.height, renderTex.width, CvType.CV_8UC3);
            maskMat = new Mat(renderTex.height, renderTex.width, CvType.CV_8U);
            hsvMat = new Mat(renderTex.height, renderTex.width, CvType.CV_8UC3);
            resultMat = new Mat(renderTex.height, renderTex.width, CvType.CV_8UC3);


            //RenderTexture��Texture2D��
            RenderTexture.active = renderTex;
            image.ReadPixels(new UnityEngine.Rect(0, 0, renderTex.width, renderTex.height), 0, 0);
            image.Apply();
            RenderTexture.active = null;

            //Mat�ɕϊ�
            Utils.texture2DToMat(image, bgrMat);

            //hsv�ɕϊ�
            Imgproc.cvtColor(bgrMat, hsvMat, Imgproc.COLOR_RGB2HSV);

            //��l��
            Core.inRange(hsvMat, new Scalar(hueMin, satMin, valMin), new Scalar(hueMax, satMax, valMax), maskMat);

            //�}�X�N...�H
            Core.bitwise_and(bgrMat, bgrMat, resultMat, maskMat);

            texture = new Texture2D(resultMat.cols(), resultMat.rows(), TextureFormat.RGBA32, false);

            //Texture2D�ɕϊ�
            Utils.matToTexture2D(resultMat, texture);

            output.material.mainTexture = texture;
        }
    }
}