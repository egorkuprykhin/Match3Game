using System.Collections.Generic;
using Core.Services;
using Match3Game.Data;
using Match3Game.Settings;

namespace Infrastructure.Services
{
    public class FieldLinesCache : IInitializableService
    {
        private IConfigurationService _configurationService;
        private Match3GameSettings _gameSettings;

        private readonly List<FieldLine> _fieldLines = new List<FieldLine>();

        public void Initialize()
        {
            _configurationService = ServiceLocator.GetService<IConfigurationService>();
            _gameSettings = _configurationService.Configuration.GetSettings<Match3GameSettings>();
        }

        public List<FieldLine> GetFieldLines() => _fieldLines;

        public void CacheActiveLines()
        {
            List<FieldCell> lineCells = new List<FieldCell>();
            
            for (int i = 0; i < _gameSettings.FieldSize.x; i++)
            {
                for (int j = 0; j < _gameSettings.FieldSize.y; j++)
                {
                    if (ChipExistInCell(i, j))
                        lineCells.Add(new FieldCell(i, j));
                    else
                        TryAddLineCells(ref lineCells);
                }
                
                TryAddLineCells(ref lineCells);
            }
            
            for (int i = 0; i < _gameSettings.FieldSize.y; i++)
            {
                for (int j = 0; j < _gameSettings.FieldSize.x; j++)
                {
                    if (ChipExistInCell(j, i))
                        lineCells.Add(new FieldCell(j, i));
                    else
                        TryAddLineCells(ref lineCells);
                }
                
                TryAddLineCells(ref lineCells);
            }

            void TryAddLineCells(ref List<FieldCell> cells)
            {
                if (cells.Count > 0)
                {
                    _fieldLines.Add(new FieldLine(cells));
                    cells.Clear();
                }
            }
        }

        private bool ChipExistInCell(int i, int j)
        {
            return _gameSettings.FieldIncludeCell(i, j);
        }
    }
}