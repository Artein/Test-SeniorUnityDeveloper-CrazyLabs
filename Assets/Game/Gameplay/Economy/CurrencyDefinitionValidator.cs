using System;
using System.Collections.Generic;

namespace Game.Gameplay.Economy
{
    public enum CurrencyValidationErrorCode
    {
        NullCurrencyDefinition = 0,
        MissingSaveId = 1,
        DuplicateSaveId = 2,
        MismatchedSaveId = 3
    }

    public readonly struct CurrencyValidationError
    {
        public CurrencyValidationErrorCode Code { get; }
        public CurrencyDefinition Definition { get; }
        public string Message { get; }

        public CurrencyValidationError(
            CurrencyValidationErrorCode code,
            CurrencyDefinition definition,
            string message)
        {
            Code = code;
            Definition = definition;
            Message = message ?? string.Empty;
        }

        public override string ToString()
        {
            return Message;
        }
    }

    public sealed class CurrencyDefinitionValidator
    {
        public IReadOnlyList<CurrencyValidationError> Validate(CurrencyDefinition definition)
        {
            var errors = new List<CurrencyValidationError>();
            ValidateSingle(definition, errors);
            return errors;
        }

        public IReadOnlyList<CurrencyValidationError> ValidateAll(IEnumerable<CurrencyDefinition> definitions)
        {
            if (definitions is null)
                throw new ArgumentNullException(nameof(definitions));

            var errors = new List<CurrencyValidationError>();
            var acceptedSaveIds = new Dictionary<string, CurrencyDefinition>(StringComparer.Ordinal);

            foreach (var definition in definitions)
            {
                var beforeCount = errors.Count;
                ValidateSingle(definition, errors);

                if (errors.Count > beforeCount || definition == null)
                    continue;

                if (!acceptedSaveIds.TryGetValue(definition.SaveId, out var acceptedDefinition))
                {
                    acceptedSaveIds.Add(definition.SaveId, definition);
                    continue;
                }

                if (ReferenceEquals(acceptedDefinition, definition))
                    continue;

                errors.Add(new CurrencyValidationError(
                    CurrencyValidationErrorCode.DuplicateSaveId,
                    definition,
                    $"Currency definition contains duplicate save id '{definition.SaveId}'."));
            }

            return errors;
        }

        private void ValidateSingle(CurrencyDefinition definition, List<CurrencyValidationError> errors)
        {
            if (definition == null)
            {
                errors.Add(new CurrencyValidationError(
                    CurrencyValidationErrorCode.NullCurrencyDefinition,
                    definition,
                    "Currency definition cannot be null."));
                return;
            }

            if (!string.IsNullOrWhiteSpace(definition.SaveId))
            {
#if UNITY_EDITOR
                ValidateAssetBackedSaveId(definition, errors);
#endif // UNITY_EDITOR
                return;
            }

            errors.Add(new CurrencyValidationError(
                CurrencyValidationErrorCode.MissingSaveId,
                definition,
                $"Currency definition '{definition.name}' requires a stable save id."));
        }

#if UNITY_EDITOR
        private void ValidateAssetBackedSaveId(CurrencyDefinition definition, List<CurrencyValidationError> errors)
        {
            var status = definition.GetSaveIdStatusForEditor();

            if (status.State != CurrencyDefinitionSaveIdState.Mismatched)
                return;

            errors.Add(new CurrencyValidationError(
                CurrencyValidationErrorCode.MismatchedSaveId,
                definition,
                $"Currency definition '{definition.name}' save id '{status.CurrentSaveId}' does not match its asset GUID. Expected save id '{status.ExpectedSaveId}'."));
        }
#endif // UNITY_EDITOR
    }
}
