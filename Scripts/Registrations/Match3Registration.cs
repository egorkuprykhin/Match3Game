using Core.Game;
using Core.Services;
using Infrastructure.Core;
using Infrastructure.Services;
using Match3Game.Services;
using Match3Game.Settings;
using UnityEngine;

namespace Match3Game.Registrations
{
    public class Match3Registration : RegistrationBase
    {
        [SerializeField] private Match3ConfigurationService Match3ConfigurationService;
        
        protected override void RegisterServices(IServicesRegistrar registrar)
        {
            registrar.Register<IConfigurationService, Match3ConfigurationService>(Match3ConfigurationService);
            
            registrar.Register<IGameService, Match3GameService>();
            
            registrar.Register<FieldLinesCache>();
            registrar.Register<ChipTypesService>();
        }
    }
}