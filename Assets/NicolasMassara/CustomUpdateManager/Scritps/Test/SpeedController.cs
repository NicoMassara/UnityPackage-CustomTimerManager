using System;
using UnityEngine;

namespace NicolasMassara.CustomUpdateManager.Test
{
#if UNITY_EDITOR
    public class SpeedController : MonoBehaviour
    {
        [Header("Speed")]
        [Range(1,500)]
        [SerializeField] private float rotationSpeed;

        [Header("Frame Rate")]
        [Range(1, 240)] 
        [SerializeField] private int frameRate = 60;
        
        [Header("Update Group")]
        [SerializeField] private UpdateGroup updateGroup;
        [Range(0, 1)]
        [SerializeField] 
        private float timeScale = 1;

        private bool _hasStarted;
        private int _lastFrameRate;
        private UpdateGroup _lastUpdateGroup;
        private float _lastTimeScale;
        public float RotationSpeed => rotationSpeed;

        public void OnValidate()
        {
            if(_hasStarted == false) return;

            if (_lastFrameRate != frameRate)
            {
                UpdateManager.Instance.SetTargetFrameRate(frameRate);
                _lastFrameRate = frameRate;
            }

            if (_lastUpdateGroup != updateGroup)
            {
                CustomTime.SetChannelTimeScale(updateGroup, timeScale);
                _lastUpdateGroup = updateGroup;
            }

            if (!Mathf.Approximately(_lastTimeScale, timeScale))
            {
                CustomTime.SetChannelTimeScale(updateGroup, timeScale);
                _lastTimeScale = timeScale;
            }
        }
        
        private void Start()
        {
            _hasStarted = true;
        }
    }
    
#endif
}