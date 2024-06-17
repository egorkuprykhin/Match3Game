using Infrastructure.Core;
using UnityEngine;

namespace Match3Game.Settings
{
    [CreateAssetMenu(fileName = "TargetsSettings")]
    public class TargetsSettings : SettingsBase
    {
        [SerializeField] public int TargetsTypesCount;
        [SerializeField] public int InitialTargetsCount;
        [SerializeField] public int ProgressiveTargetsCount;
    }
}