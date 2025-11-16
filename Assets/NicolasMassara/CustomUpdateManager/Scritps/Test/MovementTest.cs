using System;
using UnityEngine;

namespace NicolasMassara.CustomUpdateManager.Test
{
    public class MovementTest : ManagedBehavior, IUpdatable
    {
        [SerializeField] private SpeedController speedController;
        [SerializeField] private UpdateGroup updateGroup;
        [SerializeField] private TickGroup tickGroup;
        public UpdateGroup SelfUpdateGroup => updateGroup;
        public TickGroup SelfTickGroup => tickGroup;
        
        public void ExecuteUpdate(float deltaTime)
        {
            transform.Rotate(0f, 0f, speedController.RotationSpeed * deltaTime);
        }
    }
}