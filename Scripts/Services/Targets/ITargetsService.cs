using System;
using System.Collections.Generic;
using Infrastructure.Services;
using UnityEngine;

namespace Match3Game.Services
{
    public interface ITargetsService : IInitializableService
    {
        public void CreateTargets();
        
        public void CollectTarget(Sprite type);
        
        public bool Enabled { get; }

        public event Action<Sprite> TargetUpdated;
        
        public event Action TargetsCollected;

        public List<TargetData> Targets { get; }
        
        public bool AllTargetsCollected { get; }

        public float DelayBeforeWin { get; }
    }
}