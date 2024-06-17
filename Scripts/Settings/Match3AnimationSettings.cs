using DG.Tweening;
using Infrastructure.Core;
using UnityEngine;

namespace Match3Game.Settings
{
    [CreateAssetMenu(fileName = "Match3AnimationSettings")]
    public class Match3AnimationSettings : SettingsBase
    {
        [SerializeField] public float ChipSwapTime;
        [SerializeField] public Ease ChipSwapEase;
        [SerializeField] public float ChipDestroyTime;
        [SerializeField] public Ease ChipDestroyEase;
        [SerializeField] public float ChipAppearTime;
        [SerializeField] public Ease ChipAppearEase;
    }
}