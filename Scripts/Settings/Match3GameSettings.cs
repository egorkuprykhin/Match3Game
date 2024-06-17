using System.Collections.Generic;
using System.Linq;
using Infrastructure.Core;
using Match3Game.Data;
using UnityEngine;

namespace Match3Game.Settings
{
    [CreateAssetMenu(fileName = "Match3GameSettings")]
    public class Match3GameSettings : SettingsBase
    {
        [SerializeField] public Vector2Int FieldSize;  
        [SerializeField] public List<FieldOverride> FieldOverrides; 
        [SerializeField] public Vector2Int CellSize;
        [SerializeField, Range(0,100)] public int TwoStarsTimeLeftPercent;
        [SerializeField, Range(0,100)] public int ThreeStarsTimeLeftPercent;
        [SerializeField] public List<Sprite> Chips;

        public bool FieldIncludeCell(int i, int j)
        {
            FieldOverride @override = FieldOverrides.FirstOrDefault(@override => @override.I == i && @override.J == j);
            return @override == null || @override.Included;
        }
    }
}