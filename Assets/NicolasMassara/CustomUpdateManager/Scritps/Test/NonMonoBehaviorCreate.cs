using System;
using UnityEngine;

namespace NicolasMassara.CustomUpdateManager.Test
{
    public class NonMonoBehaviorCreate : MonoBehaviour
    {
        ClassExample _example;
        
        private void Awake()
        {
            _example = new ClassExample();
        }
    }
    
    public class ClassExample : ManagedComponent, 
        IUpdatable, IFixedUpdatable, ILateUpdatable
    {
        public UpdateGroup SelfUpdateGroup { get; } = UpdateGroup.Always;
        public TickGroup SelfTickGroup { get; } = TickGroup.EveryFrame;

        public ClassExample()
        {
            base.InitializeInManager();
        }

        public void ExecuteUpdate(float deltaTime)
        {
            //Debug.Log($"Update from non MonoBehavior class, At->{Time.realtimeSinceStartup}");
        }
        
        public void ExecuteFixedUpdate(float fixedDeltaTime)
        {
            //Debug.Log($"Fixed from non MonoBehavior class, At->{Time.realtimeSinceStartup}");
        }

        
        public void ExecuteLateUpdate(float deltaTime)
        {
            //Debug.Log($"Late from non MonoBehavior class, At->{Time.realtimeSinceStartup}");
        }
    }
}