using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace RobotSimulation
{
    public class DepthTexture : MonoBehaviour
    {
        [SerializeField]
        private Shader _shader;
        private Material _material;

        void Start()
        {
            // ���Ƃ��΃��C�g��Shadow Type��No Shadows�̂Ƃ��Ȃǂ�
            // ���ꂪ�ݒ肳��Ă��Ȃ��ƃf�v�X�e�N�X�`������������Ȃ�
            GetComponent<Camera>().depthTextureMode |= DepthTextureMode.Depth;

            _material = new Material(_shader);
        }

        private void OnRenderImage(RenderTexture source, RenderTexture dest)
        {
            Graphics.Blit(source, dest, _material);
        }
    }
}