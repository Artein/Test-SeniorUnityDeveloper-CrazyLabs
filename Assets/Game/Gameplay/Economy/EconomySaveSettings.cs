namespace Game.Gameplay.Economy
{
    public sealed class EconomySaveSettings
    {
        public int CurrentSchemaVersion => 1;
        public string PrimaryFileName => "economy-save.json";
        public string TemporaryFileName => "economy-save.tmp";
        public string BackupFileName => "economy-save.backup.json";
    }
}
