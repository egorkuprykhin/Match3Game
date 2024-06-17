using System;
using Infrastructure.Services;
using Match3Game.Settings;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Match3Game.Views
{
    public class ChipView: Match3GameView, IBeginDragHandler, IEndDragHandler
    {
        [SerializeField] public RectTransform Transform;
        
        [SerializeField] private Image ChipImage;
        [SerializeField] private float ReleaseDragDistance;

        private GameLifecycleService _gameLifecycleService;
        
        private Vector2 _initialPosition;
        private Vector2 _releasePosition;
        
        
        public event Action<ChipView, Vector2Int> OnChipDrag;
        public event Action<ChipView> OnChipDragFailed;

        public Sprite Type => ChipImage.sprite;
        public int X { get; private set; } 
        public int Y { get; private set; }
        

        protected override void Initialize()
        {
            _gameLifecycleService = ServiceLocator.GetService<GameLifecycleService>();
        }

        public void SetCoordinates(int i, int j)
        {
            X = i;
            Y = j;
            
            gameObject.name = $"Chip {i}.{j}";
        }

        public void SetSprite(Sprite sprite)
        {
            ChipImage.sprite = sprite;
        }

        public void SetPosition(Vector2 anchoredPosition)
        {
            Transform.anchoredPosition = anchoredPosition;
        }

        public void SetSize()
        {
            Transform.sizeDelta = _settings.GetSettings<Match3GameSettings>().ChipSize;
        }

        public void OnBeginDrag(PointerEventData eventData)
        {
            if (_gameLifecycleService.FieldLocked)
                return;
            
            _initialPosition = eventData.position;
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            if (_gameLifecycleService.FieldLocked)
                return;
            
            _releasePosition = eventData.position;
            
            float distance = Vector2.Distance(_initialPosition, _releasePosition);
            
            if (distance > ReleaseDragDistance)
                OnChipDrag?.Invoke(this, GetDragDirection());
            else
                OnChipDragFailed?.Invoke(this);
        }

        private Vector2Int GetDragDirection()
        {
            Vector2Int result = Vector2Int.zero;
            Vector2 normalized = (_releasePosition - _initialPosition).normalized;
            
            if (Mathf.Abs(normalized.x) > Mathf.Abs(normalized.y))
                result.x = (int)Mathf.Sign(normalized.x);
            else
                result.y = (int)Mathf.Sign(normalized.y);

            return result;
        }
    }
}