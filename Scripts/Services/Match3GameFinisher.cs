using Core.Services;
using Infrastructure.Services;

namespace Match3Game.Services
{
    public class Match3GameFinisher : IGameFinisherService
    {
        private Match3GameService _match3GameService;
        
        public void Initialize()
        {
            _match3GameService = ServiceLocator.GetService<Match3GameService>();
        }

        public void FinishGame()
        {
            _match3GameService.ClearField();
        }
    }
}