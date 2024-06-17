using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using Infrastructure.Extensions;
using Infrastructure.Services;
using Match3Game.Data;
using Match3Game.Factory;
using Match3Game.Services;
using Match3Game.Settings;
using Match3Game.Views;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Core.Services
{
    public class Match3GameService : IInitializableService
    {
        private ChipView[,] _chips;
        private HashSet<ChipView> _matchedChips = new HashSet<ChipView>();
        private Dictionary<Sprite, HashSet<ChipView>> _fieldChips = new Dictionary<Sprite, HashSet<ChipView>>();
        private CancellationTokenSource _cancellationTokenSource;

        private IConfigurationService _configurationService;
        private SfxService _sfxService;
        private GameLifecycleService _gameLifecycleService;
        private Match3GameFactory _gameFactory;
        private ScoresService _scoresService;
        private Match3SceneDataService _sceneData;
        private FieldLinesCache _fieldLinesCache;
        // private TargetsService _targetsService;
        
        private Match3GameSettings _gameSettings;
        private Match3AnimationSettings _animationSettings;
        private Match3SfxSettings _sfxSettings;

        public void Initialize()
        {
            _configurationService = ServiceLocator.GetService<IConfigurationService>();
            _sfxService = ServiceLocator.GetService<SfxService>();
            _gameLifecycleService = ServiceLocator.GetService<GameLifecycleService>();
            _gameFactory = ServiceLocator.GetService<Match3GameFactory>();
            _scoresService = ServiceLocator.GetService<ScoresService>();
            _fieldLinesCache = ServiceLocator.GetService<FieldLinesCache>();
            _sceneData = ServiceLocator.GetService<Match3SceneDataService>();
            // _targetsService = ServiceLocator.GetService<TargetsService>();
            
            _gameSettings = _configurationService.Configuration.GetSettings<Match3GameSettings>();
            _animationSettings = _configurationService.Configuration.GetSettings<Match3AnimationSettings>();
            _sfxSettings = _configurationService.Configuration.GetSettings<Match3SfxSettings>();
        }

        public void CreateField()
        {
            RoundField();
            CreateFieldInternal();
            _fieldLinesCache.CacheActiveLines();
        }

        public void ClearInitialMatches()
        {
            CollectMatches();

            int matchedChips = _matchedChips.Count;
            if (matchedChips > 0)
            {
                ReplaceMatchedChips();
                ClearInitialMatches();
            }
        }

        private void ReplaceMatchedChips()
        {
            foreach (ChipView chip in _matchedChips)
            {
                Vector2Int coordinates = new Vector2Int(chip.X, chip.Y);
                
                RemoveChipInstant(chip);
                CreateChip(coordinates.x, coordinates.y);
            }
            
            _matchedChips.Clear();
        }

        public void ClearField()
        {
            ClearFieldInternal();
        }

        private ChipView CreateChip(int i, int j, Sprite sprite = null)
        {
            Vector2Int halfScreenFieldSize = new Vector2Int(
                _gameSettings.FieldSize.x * _gameSettings.CellSize.x, 
                _gameSettings.FieldSize.y * _gameSettings.CellSize.y) / 2;

            int posX = -halfScreenFieldSize.x + _gameSettings.CellSize.x * i;
            int posY = -halfScreenFieldSize.y + _gameSettings.CellSize.y * j;

            ChipView chip = sprite 
                ? _gameFactory.CreateSameChip(sprite) 
                : _gameFactory.CreateRandomChip();
            
            chip.SetPosition(new Vector2(posX, posY));
            chip.SetCoordinates(i, j);
            chip.SetSize();
            
            _chips[i, j] = chip;
            
            if (!_fieldChips.ContainsKey(chip.Type))
                _fieldChips.Add(chip.Type, new HashSet<ChipView>());

            _fieldChips[chip.Type].Add(chip);

            chip.OnChipDrag += OnChipDrag;
            chip.OnChipDragFailed += OnChipDragFailed;

            return chip;
        }

        private void CollectMatches()
        {
            List<FieldLine> fieldLines = _fieldLinesCache.GetFieldLines();
            
            foreach (FieldLine fieldLine in fieldLines)
            {
                if (fieldLine.Cells.Count < 3)
                    continue;

                int combinationSize = 1;
                FieldCell firstCell = fieldLine.Cells[0];
                Sprite combinationSprite = _chips[firstCell.I, firstCell.J].Type;

                for (var i = 1; i < fieldLine.Cells.Count; i++)
                {
                    FieldCell fieldCell = fieldLine.Cells[i];
                    if (_chips[fieldCell.I, fieldCell.J].Type == combinationSprite)
                    {
                        combinationSize++;
                    }
                    else
                    {
                        if (combinationSize >= 3)
                        {
                            while (combinationSize > 0)
                            {
                                FieldCell cell = fieldLine.Cells[i - combinationSize];
                                _matchedChips.Add(_chips[cell.I, cell.J]);
                                combinationSize--;
                            }
                        }

                        combinationSprite = _chips[fieldCell.I, fieldCell.J].Type;
                        combinationSize = 1;
                    }
                }

                if (combinationSize >= 3)
                {
                    while (combinationSize > 0)
                    {
                        FieldCell cell = fieldLine.Cells[fieldLine.Cells.Count - combinationSize];
                        _matchedChips.Add(_chips[cell.I, cell.J]);
                        combinationSize--;
                    }
                }
            }
        }

        private bool HasAtLeastOneFullMatch()
        {
            return _fieldChips.Values.Any(chips => chips.Count >= 3);
        }

        private Sprite GetAlmostFullSprite()
        {
            var collection = _fieldChips.Where(pair => pair.Value.Count >= 2).ToList();
            return collection[Random.Range(0, collection.Count)].Key;
        }

        private void OnChipDrag(ChipView chip, Vector2Int direction)
        {
            if (CanDragChipInDirection(chip, direction, out Vector2Int targetChipCoords))
            {
                ChipView targetChip = _chips[targetChipCoords.x, targetChipCoords.y];
                
                _gameLifecycleService.LockField();

                CreateSwapSequence(chip, targetChip)
                    .AppendCallback(() =>
                        OnChipDragComplete(chip, targetChip).Forget());
            }
        }

        private async UniTaskVoid OnChipDragComplete(ChipView chip, ChipView targetChip)
        {
            SwapChips(chip, targetChip);
            _sfxService.PlaySfx(_sfxSettings.ChipSwap);

            _cancellationTokenSource = new CancellationTokenSource();
            try
            {
                await WaitForFieldStabilization(_cancellationTokenSource.Token);
            }
            finally
            {
                _cancellationTokenSource.Dispose();
                _cancellationTokenSource = null;
                
                _gameLifecycleService.UnlockField();
            }
        }

        private void OnChipDragFailed(ChipView chip)
        {
            _sfxService.PlaySfx(_sfxSettings.ChipSwapFailed);
        }

        private async UniTask WaitForFieldStabilization(CancellationToken cancellationToken)
        {
            int matchedChips = await TryMatchChips(cancellationToken);

            if (matchedChips > 0)
            {
                _scoresService.AddScores(matchedChips);
                
                cancellationToken.ThrowIfCancellationRequested();
                await CreateNewChips(cancellationToken);
                cancellationToken.ThrowIfCancellationRequested();
                await WaitForFieldStabilization(cancellationToken);
            }
        }

        private async UniTask CreateNewChips(CancellationToken cancellationToken)
        {
            _sfxService.PlaySfx(_sfxSettings.ChipAppear);
            
            for (int i = 0; i < _gameSettings.FieldSize.x; i++)
            {
                for (int j = 0; j < _gameSettings.FieldSize.y; j++)
                {
                    if (!_chips[i, j] && ChipExistInCell(i, j))
                    {
                        ChipView newChip;
                        
                        if (HasAtLeastOneFullMatch())
                            newChip = CreateChip(i, j);
                        else
                        {
                            Sprite almostFullSprite = GetAlmostFullSprite();
                            newChip = CreateChip(i, j, almostFullSprite);
                        }
                        
                        newChip.Transform.localScale = Vector3.zero;

                        newChip.Transform
                            .DOScale(Vector3.one, _animationSettings.ChipAppearTime)
                            .SetEase(_animationSettings.ChipAppearEase);
                    }
                }
            }

            await UniTask.WaitForSeconds(_animationSettings.ChipAppearTime, cancellationToken: cancellationToken);
            cancellationToken.ThrowIfCancellationRequested();
            await UniTask.NextFrame();
        }

        private async UniTask<int> TryMatchChips(CancellationToken cancellationToken)
        {
            CollectMatches();
            
            int matchedChipsCount = _matchedChips.Count;
            
            foreach (ChipView chip in _matchedChips)
            {
                // _targetsService.CollectTarget(chip.Type);
                RemoveChipAnimated(chip);
            }
            
            if (matchedChipsCount > 0)
                _sfxService.PlaySfx(_sfxSettings.ChipMatch);

            await UniTask.WaitForSeconds(_animationSettings.ChipDestroyTime, cancellationToken: cancellationToken);
            cancellationToken.ThrowIfCancellationRequested();
            await UniTask.NextFrame();
            
            _matchedChips.Clear();
            
            return matchedChipsCount;
        }

        private void RemoveChipAnimated(ChipView chip)
        {
            chip.OnChipDrag -= OnChipDrag;
            chip.OnChipDragFailed -= OnChipDragFailed;
            
            _fieldChips[chip.Type].Remove(chip);
            
            Vector2Int coords = new Vector2Int(chip.X, chip.Y);
            
            chip.Transform
                .DOScale(Vector3.zero, _animationSettings.ChipDestroyTime)
                .SetEase(_animationSettings.ChipDestroyEase)
                .OnComplete(() => Object.Destroy(chip.gameObject));
            
            _chips[coords.x, coords.y] = null;
        }

        private void RemoveChipInstant(ChipView chip)
        {
            chip.OnChipDrag -= OnChipDrag;
            chip.OnChipDragFailed -= OnChipDragFailed;
            
            _fieldChips[chip.Type].Remove(chip);
            
            Vector2Int coords = new Vector2Int(chip.X, chip.Y);

            Object.Destroy(chip.gameObject);

            _chips[coords.x, coords.y] = null;
        }

        private bool CanDragChipInDirection(ChipView chip, Vector2Int direction, out Vector2Int targetChipCoords)
        {
            targetChipCoords = new Vector2Int(chip.X + direction.x, chip.Y + direction.y);
            
            if (targetChipCoords.x < 0 || targetChipCoords.x >= _chips.GetLength(0))
                return false;
            
            if (targetChipCoords.y < 0 || targetChipCoords.y >= _chips.GetLength(1))
                return false;

            if (!ChipExistInCell(targetChipCoords.x, targetChipCoords.y))
                return false;

            return true;
        }

        private Sequence CreateSwapSequence(ChipView chip, ChipView targetChip)
        {
            Sequence sequence = DOTween.Sequence();
            
            sequence.Join(
                chip.Transform
                    .AnimateAnchoredPosition(
                        targetChip.Transform.anchoredPosition,
                        _animationSettings.ChipSwapTime,
                        _animationSettings.ChipSwapEase));

            sequence.Join(
                targetChip.Transform
                    .AnimateAnchoredPosition(
                        chip.Transform.anchoredPosition,
                        _animationSettings.ChipSwapTime,
                        _animationSettings.ChipSwapEase));
            
            return sequence;
        }

        private void SwapChips(ChipView chip1, ChipView chip2)
        {
            _chips[chip1.X, chip1.Y] = chip2;
            _chips[chip2.X, chip2.Y] = chip1;
            
            Vector2Int chip1Coords = new Vector2Int(chip1.X, chip1.Y);
            
            chip1.SetCoordinates(chip2.X, chip2.Y);
            chip2.SetCoordinates(chip1Coords.x, chip1Coords.y);
        }

        private void CreateFieldInternal()
        {
            _chips = new ChipView[_gameSettings.FieldSize.x, _gameSettings.FieldSize.y];
            
            int fieldSize = _gameSettings.FieldSize.x * _gameSettings.FieldSize.y - _gameSettings.FieldOverrides.Count;
            int currentFieldSize = 0;
            
            for (int i = 0; i < _gameSettings.FieldSize.x; i++)
            for (int j = 0; j < _gameSettings.FieldSize.y; j++)
            {
                if (ChipExistInCell(i, j))
                {
                    if (currentFieldSize >= fieldSize - 2)
                    {
                        if (HasAtLeastOneFullMatch())
                            CreateChip(i, j);
                        else
                        {
                            Sprite sprite = GetAlmostFullSprite();
                            CreateChip(i, j, sprite);
                        }
                    }
                    else
                        CreateChip(i, j);
                    
                    currentFieldSize++;
                }
            }
        }

        private void ClearFieldInternal()
        {
            _cancellationTokenSource?.Cancel();
            
            if (_chips != null)
                foreach (ChipView chip in _chips)
                    if (chip)
                    {
                        chip.Transform.DOKill();
                        Object.Destroy(chip.gameObject);
                    }

            _chips = null;
            _fieldChips.Clear();
            _matchedChips.Clear();
        }

        private bool ChipExistInCell(int i, int j)
        {
            return _gameSettings.FieldIncludeCell(i, j);
        }
        
        private void RoundField()
        {
            Vector2Int halfCellSize = _gameSettings.CellSize / 2;
            Vector2 containerPosition = new Vector2(halfCellSize.x, halfCellSize.y);
            
            _sceneData.FieldRoundRoot.anchoredPosition = containerPosition;
        }
    }
}