using System;
using Unity.Cinemachine;
//using Cinemachine;
using UnityEngine;

namespace Camera
{
    public class CameraController : MonoBehaviour
    {

        [SerializeField] private GameObject _curtain;
        [SerializeField] private CinemachineFollow _vcam;
        [SerializeField] private CinemachineVolumeSettings _volumeSettings;
        private UnityEngine.Camera _camera;

        private void Awake()
        {
            _camera = GetComponent<UnityEngine.Camera>();
        }

        public void SwitchVirtualCamera( bool isActive ) => _vcam.enabled = isActive;
        
        public void SwitchVolumeWeight(float weight) => _volumeSettings.Weight = weight;

        public void SwitchCurtain( bool isActive ) => _curtain.SetActive(isActive);

        public void SetOrthographicSize( float size ) => _camera.orthographicSize = size;
    }
}