using Core.Services;
using Infrastructure.Common;
using Infrastructure.Core;
using Infrastructure.Services;
using Services.Core;
using UnityEngine;

namespace Match3Game.Settings
{
    public class Match3ConfigurationService : MonoService, IInitializableService, IConfigurationService
    {
        [SerializeField] private Configuration GameConfiguration;

        public ConfigurationProxy Configuration { get; private set; }
        
        public void Initialize()
        {
            Configuration = new ConfigurationProxy(GameConfiguration);
        }
    }
}