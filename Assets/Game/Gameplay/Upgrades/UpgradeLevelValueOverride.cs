using System;
using UnityEngine;

namespace Game.Gameplay.Upgrades
{
    [Serializable]
    public struct UpgradeLevelValueOverride
    {
        [SerializeField] private int _level;
        [SerializeField] private float _value;

        public int Level => _level;
        public float Value => _value;

        public UpgradeLevelValueOverride(int level, float value)
        {
            _level = level;
            _value = value;
        }
    }
}
