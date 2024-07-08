using Core.Sfx;
using Infrastructure.Core;
using UnityEngine;

namespace Match3Game.Settings
{
    [CreateAssetMenu(fileName = "TargetsSettings")]
    public class TargetsSettings : SettingsBase
    {
        [SerializeField] public bool UseTargets;
        [SerializeField] public int TargetsTypesCount;
        [SerializeField] public int InitialTargetsCount;
        [SerializeField] public float DelayBeforeWin;
        [SerializeField] public SfxType TargetCollectedSound;
        [SerializeField] public int ProgressiveTargetsCount;
    }
}