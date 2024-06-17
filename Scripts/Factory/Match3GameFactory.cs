using Core.Services;
using Infrastructure.Core;
using Infrastructure.Services;
using Match3Game.Services;
using Match3Game.Views;
using UnityEngine;

namespace Match3Game.Factory
{
    public class Match3GameFactory : GameFactory<IConfigurationService>
    {
        private ChipTypesService _chipTypesService;
        private Match3SceneDataService _sceneData;

        public override void Initialize()
        {
            base.Initialize();
            _chipTypesService = ServiceLocator.GetService<ChipTypesService>();
            _sceneData = ServiceLocator.GetService<Match3SceneDataService>();
        }

        public ChipView CreateRandomChip()
        {
            ChipView chip = CreateView<ChipView>(_sceneData.FieldRoot);
            Sprite sprite = _chipTypesService.GetRandomChipType();
            chip.SetSprite(sprite);
            
            return chip;
        }

        public ChipView CreateSameChip(Sprite sprite)
        {
            ChipView chip = CreateView<ChipView>(_sceneData.FieldRoot);
            chip.SetSprite(sprite);
            
            return chip;
        }
    }
}