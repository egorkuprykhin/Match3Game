using Core.Services;
using Infrastructure.ButtonActions;
using Infrastructure.Services;

namespace Match3Game.ButtonActions
{
    public class ClearMatch3FieldButtonAction : ButtonAction
    {
        private Match3GameService _gameService;
        
        public override void Action()
        {
            _gameService.ClearField();
        }

        protected override void Initialize()
        {
            _gameService = ServiceLocator.GetService<Match3GameService>();
        }
    }
}