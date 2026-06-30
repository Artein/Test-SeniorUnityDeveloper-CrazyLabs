using System;
using System.IO;
using System.Text;
using Game.Foundation.Persistence;
using JetBrains.Annotations;
using UnityEngine;

namespace Game.Gameplay.Economy
{
    public interface IEconomySaveRepository
    {
        EconomyLoadResult Load();
        EconomyPersistenceResult Save(PlayerEconomySnapshot snapshot, string reason);
    }

    [UsedImplicitly]
    public sealed class EconomySaveRepository : IEconomySaveRepository
    {
        private readonly IPersistentDataPathProvider _pathProvider;
        private readonly EconomySaveSerializer _serializer;
        private readonly EconomySaveSettings _settings;

        public EconomySaveRepository(
            IPersistentDataPathProvider pathProvider,
            EconomySaveSerializer serializer,
            EconomySaveSettings settings)
        {
            _pathProvider = pathProvider ?? throw new ArgumentNullException(nameof(pathProvider));
            _serializer = serializer ?? throw new ArgumentNullException(nameof(serializer));
            _settings = settings ?? throw new ArgumentNullException(nameof(settings));
        }

        public EconomyLoadResult Load()
        {
            var primaryPath = GetPrimaryPath();
            var backupPath = GetBackupPath();
            var hadLoadFailure = false;

            if (File.Exists(primaryPath))
            {
                try
                {
                    return new EconomyLoadResult(
                        LoadSnapshot(primaryPath, "primary"),
                        new EconomyPersistenceResult(
                            EconomyPersistenceStatus.LoadedPrimary,
                            "load-primary",
                            "Loaded economy save from primary file.",
                            exception: null));
                }
                catch (Exception exception)
                {
                    hadLoadFailure = true;
                    Debug.LogError("Economy save load failed from primary. " + exception.Message);
                    PreserveCorruptFile(primaryPath, "primary");
                }
            }

            if (File.Exists(backupPath))
            {
                try
                {
                    return new EconomyLoadResult(
                        LoadSnapshot(backupPath, "backup"),
                        new EconomyPersistenceResult(
                            EconomyPersistenceStatus.LoadedBackup,
                            "load-backup",
                            "Loaded economy save from backup file.",
                            exception: null));
                }
                catch (Exception exception)
                {
                    hadLoadFailure = true;
                    Debug.LogError("Economy save load failed from backup. " + exception.Message);
                    PreserveCorruptFile(backupPath, "backup");
                }
            }

            var status = hadLoadFailure ? EconomyPersistenceStatus.Failed : EconomyPersistenceStatus.LoadedDefaults;

            var message = hadLoadFailure
                ? "Economy save load failed; using default state."
                : "No economy save found; using default state.";

            return new EconomyLoadResult(
                CreateDefaultSnapshot(),
                new EconomyPersistenceResult(status, "load-defaults", message, exception: null));
        }

        public EconomyPersistenceResult Save(PlayerEconomySnapshot snapshot, string reason)
        {
            if (snapshot is null)
                throw new ArgumentNullException(nameof(snapshot));

            var operation = string.IsNullOrWhiteSpace(reason) ? "save" : reason;
            var directory = _pathProvider.PersistentDataPath;
            var primaryPath = GetPrimaryPath();
            var temporaryPath = GetTemporaryPath();
            var backupPath = GetBackupPath();

            try
            {
                Directory.CreateDirectory(directory);
                var json = _serializer.Serialize(snapshot);
                File.WriteAllText(temporaryPath, json, Encoding.UTF8);

                if (File.Exists(primaryPath))
                    File.Copy(primaryPath, backupPath, overwrite: true);

                if (File.Exists(primaryPath))
                    File.Delete(primaryPath);

                File.Move(temporaryPath, primaryPath);

                return new EconomyPersistenceResult(
                    EconomyPersistenceStatus.Saved,
                    operation,
                    "Saved economy state.",
                    exception: null);
            }
            catch (Exception exception)
            {
                Debug.LogError("Economy save write failed. " + exception.Message);
                DeleteTemporaryFile(temporaryPath);

                return new EconomyPersistenceResult(
                    EconomyPersistenceStatus.Failed,
                    operation,
                    exception.Message,
                    exception);
            }
        }

        private PlayerEconomySnapshot LoadSnapshot(string path, string sourceName)
        {
            var json = File.ReadAllText(path, Encoding.UTF8);
            return _serializer.Deserialize(json, sourceName);
        }

        private PlayerEconomySnapshot CreateDefaultSnapshot()
        {
            return new PlayerEconomySnapshot(
                revision: 0,
                Array.Empty<PlayerCurrencyBalance>(),
                Array.Empty<PlayerUpgradeLevel>());
        }

        private void PreserveCorruptFile(string path, string sourceName)
        {
            if (!File.Exists(path))
                return;

            var corruptPath = path + "." + sourceName + ".corrupt";

            try
            {
                File.Copy(path, corruptPath, overwrite: true);
            }
            catch (Exception exception)
            {
                Debug.LogWarning("Economy save corrupt-file preservation failed. " + exception.Message);
            }
        }

        private void DeleteTemporaryFile(string temporaryPath)
        {
            if (!File.Exists(temporaryPath))
                return;

            try
            {
                File.Delete(temporaryPath);
            }
            catch (Exception exception)
            {
                Debug.LogWarning("Economy save temporary-file cleanup failed. " + exception.Message);
            }
        }

        private string GetPrimaryPath()
        {
            return Path.Combine(_pathProvider.PersistentDataPath, _settings.PrimaryFileName);
        }

        private string GetTemporaryPath()
        {
            return Path.Combine(_pathProvider.PersistentDataPath, _settings.TemporaryFileName);
        }

        private string GetBackupPath()
        {
            return Path.Combine(_pathProvider.PersistentDataPath, _settings.BackupFileName);
        }
    }
}
