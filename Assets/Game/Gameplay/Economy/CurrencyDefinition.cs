using System;

#if UNITY_EDITOR
using UnityEditor;
#endif

using UnityEngine;

namespace Game.Gameplay.Economy
{
    [CreateAssetMenu(
        fileName = nameof(CurrencyDefinition),
        menuName = "Game/Gameplay/Economy/Currency Definition")]
    public sealed partial class CurrencyDefinition : ScriptableObject
    {
        [SerializeField] private string _saveId;
        [SerializeField] private Sprite _icon;

        public string SaveId => _saveId;
        public Sprite Icon => _icon;

        public static class Serialization
        {
            public const string SaveId = nameof(_saveId);
            public const string Icon = nameof(_icon);
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            ReconcileSaveIdWithAssetGuidForEditor();
        }

        internal void ReconcileSaveIdWithAssetGuidForEditor()
        {
            var status = GetSaveIdStatusForEditor();

            if (status.State == CurrencyDefinitionSaveIdState.Missing)
            {
                _saveId = status.ExpectedSaveId;
                EditorUtility.SetDirty(this);
                return;
            }

            if (status.State != CurrencyDefinitionSaveIdState.Mismatched)
                return;

            Debug.LogError(
                $"Currency Definition '{name}' save id does not match its asset GUID. " +
                $"Asset Path: '{status.AssetPath}'. Asset GUID: '{status.AssetGuid}'. " +
                $"Current Save ID: '{status.CurrentSaveId}'. Expected Save ID: '{status.ExpectedSaveId}'. " +
                "This usually means the asset was duplicated, edited manually, or its .meta was regenerated. " +
                "Use the inspector fix button if this asset should use the current asset identity.",
                this);
        }

        internal CurrencyDefinitionSaveIdStatus GetSaveIdStatusForEditor()
        {
            var assetPath = AssetDatabase.GetAssetPath(this);

            if (string.IsNullOrWhiteSpace(assetPath))
                return new CurrencyDefinitionSaveIdStatus(CurrencyDefinitionSaveIdState.NonAsset, string.Empty, string.Empty, _saveId, string.Empty);

            var assetGuid = AssetDatabase.AssetPathToGUID(assetPath);

            if (string.IsNullOrWhiteSpace(assetGuid))
                return new CurrencyDefinitionSaveIdStatus(CurrencyDefinitionSaveIdState.NonAsset, assetPath, string.Empty, _saveId, string.Empty);

            var expectedSaveId = GetExpectedSaveId(assetGuid);

            if (string.IsNullOrWhiteSpace(_saveId))
                return new CurrencyDefinitionSaveIdStatus(CurrencyDefinitionSaveIdState.Missing, assetPath, assetGuid, _saveId, expectedSaveId);

            if (!string.Equals(_saveId, expectedSaveId, StringComparison.Ordinal))
                return new CurrencyDefinitionSaveIdStatus(CurrencyDefinitionSaveIdState.Mismatched, assetPath, assetGuid, _saveId, expectedSaveId);

            return new CurrencyDefinitionSaveIdStatus(CurrencyDefinitionSaveIdState.Valid, assetPath, assetGuid, _saveId, expectedSaveId);
        }

        internal bool TrySetSaveIdFromAssetGuidForEditor(string undoName)
        {
            var status = GetSaveIdStatusForEditor();

            if (status.State == CurrencyDefinitionSaveIdState.NonAsset || string.IsNullOrWhiteSpace(status.ExpectedSaveId))
                return false;

            if (string.Equals(_saveId, status.ExpectedSaveId, StringComparison.Ordinal))
                return false;

            Undo.RecordObject(this, undoName);
            _saveId = status.ExpectedSaveId;
            EditorUtility.SetDirty(this);
            return true;
        }

        private string GetExpectedSaveId(string assetGuid)
        {
            return "currency-" + assetGuid;
        }
#endif // UNITY_EDITOR
    }

#if UNITY_EDITOR
    internal enum CurrencyDefinitionSaveIdState
    {
        NonAsset = 0,
        Missing = 1,
        Mismatched = 2,
        Valid = 3
    }

    internal readonly struct CurrencyDefinitionSaveIdStatus
    {
        public CurrencyDefinitionSaveIdStatus(
            CurrencyDefinitionSaveIdState state,
            string assetPath,
            string assetGuid,
            string currentSaveId,
            string expectedSaveId)
        {
            State = state;
            AssetPath = assetPath ?? string.Empty;
            AssetGuid = assetGuid ?? string.Empty;
            CurrentSaveId = currentSaveId ?? string.Empty;
            ExpectedSaveId = expectedSaveId ?? string.Empty;
        }

        public CurrencyDefinitionSaveIdState State { get; }
        public string AssetPath { get; }
        public string AssetGuid { get; }
        public string CurrentSaveId { get; }
        public string ExpectedSaveId { get; }
    }
#endif // UNITY_EDITOR
}
