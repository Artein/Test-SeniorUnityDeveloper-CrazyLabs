namespace Game.Gameplay.Upgrades
{
    public readonly struct UpgradeValidationError
    {
        public UpgradeValidationErrorCode Code { get; }
        public object Source { get; }
        public string Message { get; }

        public UpgradeValidationError(UpgradeValidationErrorCode code, object source, string message)
        {
            Code = code;
            Source = source;
            Message = message;
        }
    }
}
