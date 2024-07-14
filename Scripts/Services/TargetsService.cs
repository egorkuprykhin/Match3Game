using System;
using System.Collections.Generic;
using System.Linq;
using Infrastructure.Common;
using Infrastructure.Data;
using Infrastructure.Settings;
using Match3Game.Services;
using UnityEngine;

namespace Infrastructure.Services
{
    public class TargetsService : ITargetsService
    {
        public List<TargetData> Targets { get; } = new List<TargetData>();

        private Configuration _configuration;
        private ChipTypesService _chipTypesService;
        private TargetsSettings _targetsSettings;
        private ConfigurationService _configurationService;
        private SfxService _soundService;

        public bool Enabled => _targetsSettings.UseTargets;
        
        public event Action TargetsCollected;
        public event Action<Sprite> TargetUpdated;

        public bool AllTargetsCollected => Targets.All(target => target.Collected);
        public float DelayBeforeWin => _targetsSettings.DelayBeforeWin;


        public void Initialize()
        {
            _chipTypesService = ServiceLocator.GetService<ChipTypesService>();
            _configurationService = ServiceLocator.GetService<ConfigurationService>();
            _targetsSettings = _configurationService.GetSettings<TargetsSettings>();
            _soundService = ServiceLocator.GetService<SfxService>();
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
                
                if (!targetData.Collected)
                    _soundService.PlaySfx(_targetsSettings.TargetCollectedSound);
            }

            if (AllTargetsCollected) 
                TargetsCollected?.Invoke();
        }
    }
}