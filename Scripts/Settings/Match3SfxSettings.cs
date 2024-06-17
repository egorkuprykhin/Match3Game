using Core.Sfx;
using Infrastructure.Core;
using UnityEngine;

namespace Match3Game.Settings
{
    [CreateAssetMenu(fileName = "Match3SfxSettings")]
    public class Match3SfxSettings : SettingsBase
    {
        public SfxType ChipSwap;
        public SfxType ChipSwapFailed;
        public SfxType ChipMatch;
        public SfxType ChipAppear;
    }
}