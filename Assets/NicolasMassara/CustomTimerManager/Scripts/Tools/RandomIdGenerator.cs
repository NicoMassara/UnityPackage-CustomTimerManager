using System;
using System.Collections.Generic;

namespace NicolasMassara.CustomTimerManager.Tools
{
    public class TimerGeneratedId
    {
        public ulong Id { get; private set; }
        public bool IsActive => Id > 0;
        private event Action<TimerGeneratedId> _onRelease;

        public TimerGeneratedId(ulong id, Action<TimerGeneratedId> onRelease)
        {
            Id = id;
            _onRelease = onRelease;
        }

        public void Release()
        {
            _onRelease?.Invoke(this);
        }

        public void Reset()
        {
            Id = 0;
        }
    }

    public class RandomIdGenerator
    {
        private const ulong NullId = 0; // Default ID used as a null
        
        private readonly HashSet<ulong> _inUseId = new HashSet<ulong>(); // In Used ID List 
        private readonly System.Random _random = new System.Random();
        
        /// <summary>
        /// Generates a random GeneratedId
        /// That contains an ulong used as the ID
        /// </summary>
        /// <returns></returns>
        public TimerGeneratedId Generate()
        {
            ulong value;
            int attempts = 0;

            do
            {
                value = NextUlong();
                attempts++;

                if (attempts > 100)
                {
                    break;
                }

            } while (_inUseId.Contains(value));

            _inUseId.Add(value);
            
            var generatedId = new TimerGeneratedId(value,Release);
            
            return generatedId;
        }
        
        private void Release(TimerGeneratedId idData)
        {
            _inUseId.Remove(idData.Id);
            idData.Reset();
        }
        
        private ulong NextUlong()
        {
            ulong value;

            do
            {
                byte[] bytes = new byte[8];
                _random.NextBytes(bytes);
                value = BitConverter.ToUInt64(bytes, 0);

            } while (value == NullId);

            return value;
        }
    }
}