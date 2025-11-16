using System;
using System.Collections.Generic;
using UnityEngine;

namespace NicolasMassara.CustomUpdateManager
{
    
    public class UpdateManager : MonoBehaviour
    {

        // ----------------------- Singleton ---------------------------
        public static UpdateManager Instance =>  _instance != null ? _instance : (_instance = CreateInstance());
        protected static UpdateManager _instance;
        
        private static UpdateManager CreateInstance()
        {
            var gameObject = new GameObject(nameof(UpdateManager))
            {
                hideFlags = HideFlags.DontSave,
            };
            //Debug.Log($"Singleton Created: {typeof(T)}");
            DontDestroyOnLoad(gameObject);
            return gameObject.AddComponent<UpdateManager>();
        }
        
        // -------------------------------------------------------------
        
        // --------------------- Updatable Components -----------------------
        
        #region Updatable Component Controller

        #region Tools

        private static float GetTickByGroup(TickGroup group, float frameTime, int targetFrameRate)
        {
            float targetFPS = targetFrameRate > 0 ? targetFrameRate : (1f / frameTime);
            float baseFrameTime = 1f / targetFPS;
            float adaptiveFrameTime = Mathf.Lerp(baseFrameTime, frameTime, 0.2f);
            
            var tickValue = group switch
            {
                TickGroup.EveryFrame => adaptiveFrameTime,
                TickGroup.HalfTarget => adaptiveFrameTime * 2,
                TickGroup.QuarterTarget => adaptiveFrameTime * 4,
                TickGroup.EightTarget => adaptiveFrameTime * 8,
                TickGroup.SixteenthTarget => adaptiveFrameTime * 16,
                TickGroup.ThirtySecondTarget => adaptiveFrameTime * 32,
                TickGroup.SixtyFourthTarget => adaptiveFrameTime * 64,
                TickGroup.EverySecond => 1f,
                _ => throw new ArgumentOutOfRangeException(nameof(group), group, null)
            };

            return tickValue;
        }

        #endregion
        
        private abstract class UpdateController<T> where T : IBaseUpdatable
        {
            private bool _isUpdating;
            
            private readonly List<T> _running = new List<T>();
            private readonly List<T> _toAdd = new List<T>();
            private readonly List<T> _toRemove = new List<T>();
            private readonly Dictionary<T, float> _accumulatedDeltaTimeDic = new Dictionary<T, float>();

            protected int TargetFrameRate { get; private set; }
            protected int MaxTicksPerFrame { get; set; } = 5;

            public bool IsPaused;
            public int RunningCount => _running.Count;

            protected UpdateController(int targetFrameRate)
            {
                TargetFrameRate = targetFrameRate;
            }
            
            public void UpdateComponents()
            {
                ApplyPending();
                
                _isUpdating = true;

                if (!IsPaused || RunningCount > 0)
                {
                    for (int i = 0; i < _running.Count; i++)
                    {
                        UpdateElement(_running[i]);
                    }
                }
                
                _isUpdating = false;
                
                ApplyPending();
            }

            protected abstract void UpdateElement(T element);


            public void SetTargetFrameRate(int targetFrameRate)
            {
                TargetFrameRate = targetFrameRate;
            }

            #region Add/Remove

            private void AddToRunningList(T element)
            {
                _accumulatedDeltaTimeDic.TryAdd(element, 0);

                if (_running.Contains(element) == false)
                {
                    _running.Add(element);
                }
            }

            private void RemoveFromRunningList(T element)
            {
                if (_running.Contains(element))
                {
                    _running.Remove(element);
                }

                if (_accumulatedDeltaTimeDic.ContainsKey(element))
                {
                    _accumulatedDeltaTimeDic.Remove(element);
                }
            }

            public void Add(T updatable)
            {
                if (_isUpdating)
                {
                    if (!_toAdd.Contains(updatable))
                    {
                        _toAdd.Add(updatable);
                    }
                }
                else if (!_running.Contains(updatable))
                {
                    AddToRunningList(updatable);
                }
            }
            
            public void Remove(T updatable)
            {
                if (_isUpdating)
                {
                    if (!_toRemove.Contains(updatable))
                    {
                        _toRemove.Add(updatable);
                    }
                }
                else
                { 
                    RemoveFromRunningList(updatable);
                }
            }
            
            private void ApplyPending()
            {
                if (_toAdd.Count > 0)
                {
                    foreach (var a in _toAdd)
                    {
                        if (!_running.Contains(a))
                        {
                            AddToRunningList(a);
                        }
                    }
                    _toAdd.Clear();
                }

                if (_toRemove.Count > 0)
                {
                    foreach (var r in _toRemove)
                    {
                        RemoveFromRunningList(r);
                    }
                    
                    _toRemove.Clear();
                }
            }

            #endregion
            
            #region Delta Time

            protected void SetAccumulatedDeltaTime(T updatable, float deltaTime)
            {
                _accumulatedDeltaTimeDic[updatable] = deltaTime;
            }

            protected float GetAccumulatedDeltaTime(T updatable)
            {
                return _accumulatedDeltaTimeDic.ContainsKey(updatable) ? 
                    _accumulatedDeltaTimeDic[updatable] : 0f;
            }

            #endregion
        }

        private class UpdatableComponent : UpdateController<IUpdatable>
        {
            public UpdatableComponent(int targetFrameRate) : 
                base(targetFrameRate)
            {
            }

            protected override void UpdateElement(IUpdatable element)
            {
                var chanel = CustomTime.GetChannel(element.SelfUpdateGroup);
                
                if (chanel.IsPaused)
                    return;

                float unscaledDeltaTime = chanel.UnscaledDeltaTime;
                float interval = GetTickByGroup(element.SelfTickGroup, unscaledDeltaTime, TargetFrameRate);
                float accumulatedTime = GetAccumulatedDeltaTime(element);
                accumulatedTime += chanel.DeltaTime;
                
                // Tick cap
                int ticksExecuted = 0;
                

                while (accumulatedTime >= interval && ticksExecuted < MaxTicksPerFrame)
                {
                    element.ExecuteUpdate(interval);
                    accumulatedTime -= interval;
                    ticksExecuted++;
                }
                
                if (accumulatedTime > interval)
                {
                    accumulatedTime = interval;
                }

                SetAccumulatedDeltaTime(element, accumulatedTime);
            }
        }
        
        private class FixedUpdatableComponent : UpdateController<IFixedUpdatable>
        {
            public FixedUpdatableComponent(int targetFrameRate) :
                base(targetFrameRate)
            {
            }

            protected override void UpdateElement(IFixedUpdatable element)
            {
                var chanel = CustomTime.GetChannel(element.SelfUpdateGroup);
                
                if (chanel.IsPaused)
                    return;

                float unscaledFixedDeltaTime = chanel.UnscaledFixedDeltaTime;
                float interval = GetTickByGroup(element.SelfTickGroup, unscaledFixedDeltaTime, TargetFrameRate);
                float accumulatedTime = GetAccumulatedDeltaTime(element);
                accumulatedTime += chanel.FixedDeltaTime;

                // Tick cap
                int ticksExecuted = 0;

                while (accumulatedTime >= interval && ticksExecuted < MaxTicksPerFrame)
                {
                    element.ExecuteFixedUpdate(interval);
                    accumulatedTime -= interval;
                    ticksExecuted++;
                }
                
                if (accumulatedTime > interval)
                {
                    accumulatedTime = interval;
                }

                SetAccumulatedDeltaTime(element, accumulatedTime);
            }
        }
        
        private class LateUpdatableComponent : UpdateController<ILateUpdatable>
        {
            public LateUpdatableComponent(int targetFrameRate) : 
                base(targetFrameRate)
            {
            }

            protected override void UpdateElement(ILateUpdatable element)
            {
                var chanel = CustomTime.GetChannel(element.SelfUpdateGroup);
                
                if (chanel.IsPaused)
                    return;

                float unscaledDeltaTime = chanel.UnscaledDeltaTime;
                float interval = GetTickByGroup(element.SelfTickGroup, unscaledDeltaTime, TargetFrameRate);
                float accumulatedTime = GetAccumulatedDeltaTime(element);
                accumulatedTime += chanel.DeltaTime;

                // Tick cap
                int ticksExecuted = 0;
                

                while (accumulatedTime >= interval && ticksExecuted < MaxTicksPerFrame)
                {
                    element.ExecuteLateUpdate(interval);
                    accumulatedTime -= interval;
                    ticksExecuted++;
                }
                
                if (accumulatedTime > interval)
                {
                    accumulatedTime = interval;
                }

                SetAccumulatedDeltaTime(element, accumulatedTime);
            }
        }

        #endregion
        
        private UpdatableComponent _updatableComponent;
        private FixedUpdatableComponent _fixedUpdatableComponent;
        private LateUpdatableComponent _lateUpdatableComponent;
        
        // -------------------------------------------------------------
        
        public const int TargetFrameRate = 60;
        
        public bool IsUpdatePaused { get; set; }
        public bool IsFixedUpdatePaused { get; set; }
        
        private void Awake()
        {
            Application.targetFrameRate = TargetFrameRate;
            
            _updatableComponent = new UpdatableComponent(TargetFrameRate);
            _fixedUpdatableComponent = new FixedUpdatableComponent(TargetFrameRate);
            _lateUpdatableComponent = new LateUpdatableComponent(TargetFrameRate);
        }

        public void SetTargetFrameRate(int targetFrameRate)
        {
            Application.targetFrameRate = targetFrameRate;
            _updatableComponent.SetTargetFrameRate(targetFrameRate);
            _fixedUpdatableComponent.SetTargetFrameRate(targetFrameRate);
            _lateUpdatableComponent.SetTargetFrameRate(targetFrameRate);
        }

        #region Update Actions

        private void Update()
        {
            CustomTime.UpdateAll(IsUpdatePaused ? 0 : Time.unscaledDeltaTime);
            _updatableComponent.IsPaused = IsUpdatePaused;
            if(IsUpdatePaused) return;
            
            _updatableComponent.UpdateComponents();
        }

        private void FixedUpdate()
        {
            CustomTime.FixedUpdateAll(IsFixedUpdatePaused ? 0 : Time.fixedUnscaledDeltaTime);
            _fixedUpdatableComponent.IsPaused  = IsFixedUpdatePaused;
            if(IsFixedUpdatePaused) return;
            
            _fixedUpdatableComponent.UpdateComponents();
        }
        
        
        private void LateUpdate()
        {
            _lateUpdatableComponent.IsPaused  = IsUpdatePaused;
            if(IsUpdatePaused) return;
            
            _lateUpdatableComponent.UpdateComponents();
        }

        #endregion

        #region Register/Unregister
        
        public void Register(IManagedObject element)
        {
            if(element == null) return;
            
            if (element is IUpdatable updatable)
            {
                _updatableComponent.Add(updatable);
            }
            if (element is IFixedUpdatable fixedUpdatable)
            {
                _fixedUpdatableComponent.Add(fixedUpdatable);
            }
            if (element is ILateUpdatable lateUpdatable)
            {
                _lateUpdatableComponent.Add(lateUpdatable);
            }
        }

        public void Unregister(IManagedObject element)
        {
            if(element == null) return;
            
            if (element is IUpdatable updatable)
            {
                _updatableComponent.Remove(updatable);
            }
            if (element is IFixedUpdatable fixedUpdatable)
            {
                _fixedUpdatableComponent.Remove(fixedUpdatable);
            }
            if (element is ILateUpdatable lateUpdatable)
            {
                _lateUpdatableComponent.Remove(lateUpdatable);
            }  
        }
        
        #endregion
    }
}