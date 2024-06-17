using Core.Services;
using Infrastructure.Services;

namespace Match3Game.Services
{
    public class Match3GameStarter : IGameStarterService
    {
        private Match3GameService _match3GameService;
        
        public void Initialize()
        {
            _match3GameService = ServiceLocator.GetService<Match3GameService>();
        }

        public void StartGame()
        {
            _match3GameService.CreateField();
            _match3GameService.ClearInitialMatches();
        }
    }
}