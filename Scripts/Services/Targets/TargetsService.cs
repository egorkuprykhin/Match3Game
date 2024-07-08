using System;
using System.Collections.Generic;
using System.Linq;
using Core.Services;
using Infrastructure.Common;
using Match3Game.Services;
using Match3Game.Settings;
using UnityEngine;

namespace Infrastructure.Services
{
    public class TargetsService : ITargetsService
    {
        public List<TargetData> Targets { get; } = new List<TargetData>();

        private Configuration _configuration;
        private ChipTypesService _chipTypesService;
        private TargetsSettings _targetsSettings;
        private IConfigurationService _configurationService;

        public event Action TargetsCollected;
        public event Action<Sprite> TargetUpdated;

        public bool AllTargetsCollected => Targets.All(target => target.Collected);
        public float DelayBeforeWin => _targetsSettings.DelayBeforeWin;

        public void Initialize()
        {
            _chipTypesService = ServiceLocator.GetService<ChipTypesService>();
            _configurationService = ServiceLocator.GetService<IConfigurationService>();
            _targetsSettings = _configurationService.Configuration.GetSettings<TargetsSettings>();
        }

        public void CreateTargets()
        {
            Targets.Clear();
            _chipTypesService.InitUniqueTypesSequence();

            for (int i = 0; i < _targetsSettings.TargetsTypesCount; i++)
            {
                TargetData targetData = new TargetData
                {
                    Type = _chipTypesService.GetRandomUniqueChipType(),
                    Count = _targetsSettings.InitialTargetsCount
                };
                
                Targets.Add(targetData);
            }
        }

        public void CollectTarget(Sprite type)
        {
            TargetData targetData = Targets.FirstOrDefault(target => target.Type == type);
            if (targetData != null)
            {
                targetData.CollectTarget(1);
                TargetUpdated?.Invoke(type);
            }

            if (AllTargetsCollected) 
                TargetsCollected?.Invoke();
        }
    }
}