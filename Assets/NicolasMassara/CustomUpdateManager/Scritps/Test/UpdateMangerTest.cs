using UnityEngine;

namespace NicolasMassara.CustomUpdateManager.Test
{
    public class UpdateMangerTest : ManagedBehavior, 
        IUpdatable
    {
        public UpdateGroup SelfUpdateGroup { get; } = UpdateGroup.Always;
        public TickGroup SelfTickGroup { get; } = TickGroup.EveryFrame;
        
        
        public void ExecuteUpdate(float deltaTime)
        {
            //Debug.Log($"Update from MonoBehavior class, At->{Time.realtimeSinceStartup}");
        }
        
        public void ExecuteFixedUpdate(float fixedDeltaTime)
        {
            //Debug.Log($"Fixed from MonoBehavior class, At->{Time.realtimeSinceStartup}");
        }

        
        public void ExecuteLateUpdate(float deltaTime)
        {
            //Debug.Log($"Late from MonoBehavior class, At->{Time.realtimeSinceStartup}");
        }
        
    }
}