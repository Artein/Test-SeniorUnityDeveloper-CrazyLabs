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
        private readonly string _backupPath;
        private readonly string _directory;
        private readonly string _primaryPath;
        private readonly EconomySaveSerializer _serializer;
        private readonly EconomySaveSettings _settings;
        private readonly string _temporaryPath;

        public EconomySaveRepository(
            IPersistentDataPathProvider pathProvider,
            EconomySaveSerializer serializer,
            EconomySaveSettings settings)
        {
            if (pathProvider is null)
                throw new ArgumentNullException(nameof(pathProvider));

            _serializer = serializer ?? throw new ArgumentNullException(nameof(serializer));
            _settings = settings ?? throw new ArgumentNullException(nameof(settings));

            _directory = pathProvider.PersistentDataPath;
            _primaryPath = Path.Combine(_directory, _settings.PrimaryFileName);
            _temporaryPath = Path.Combine(_directory, _settings.TemporaryFileName);
            _backupPath = Path.Combine(_directory, _settings.BackupFileName);
        }

        public EconomyLoadResult Load()
        {
            var primaryPath = _primaryPath;
            var backupPath = _backupPath;
            var hadLoadFailure = false;

            if (File.Exists(primaryPath))
            {
                try
                {
                    var snapshot = LoadSnapshot(primaryPath, "primary");
#if UNITY_EDITOR || DEVELOPMENT_BUILD
                    Debug.Log("[EconomyDiagnostics] Economy save loaded primary: "
                              + $"Path='{primaryPath}', "
                              + $"Revision={snapshot.Revision}, "
                              + $"CurrencyBalances={snapshot.CurrencyBalances.Count}, "
                              + $"UpgradeLevels={snapshot.UpgradeLevels.Count}");
#endif

                    return new EconomyLoadResult(
                        snapshot,
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
                    var snapshot = LoadSnapshot(backupPath, "backup");
#if UNITY_EDITOR || DEVELOPMENT_BUILD
                    Debug.Log("[EconomyDiagnostics] Economy save loaded backup: "
                              + $"Path='{backupPath}', "
                              + $"Revision={snapshot.Revision}, "
                              + $"CurrencyBalances={snapshot.CurrencyBalances.Count}, "
                              + $"UpgradeLevels={snapshot.UpgradeLevels.Count}");
#endif

                    return new EconomyLoadResult(
                        snapshot,
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

#if UNITY_EDITOR || DEVELOPMENT_BUILD
            Debug.Log("[EconomyDiagnostics] Economy save loaded defaults: "
                      + $"PrimaryPath='{primaryPath}', "
                      + $"BackupPath='{backupPath}', "
                      + $"HadLoadFailure={hadLoadFailure}");
#endif

            return new EconomyLoadResult(
                CreateDefaultSnapshot(),
                new EconomyPersistenceResult(status, "load-defaults", message, exception: null));
        }

        public EconomyPersistenceResult Save(PlayerEconomySnapshot snapshot, string reason)
        {
            if (snapshot is null)
                throw new ArgumentNullException(nameof(snapshot));

            var operation = string.IsNullOrWhiteSpace(reason) ? "save" : reason;
            var directory = _directory;
            var primaryPath = _primaryPath;
            var temporaryPath = _temporaryPath;
            var backupPath = _backupPath;

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

#if UNITY_EDITOR || DEVELOPMENT_BUILD
                Debug.Log("[EconomyDiagnostics] Economy save wrote primary: "
                          + $"Operation='{operation}', "
                          + $"Path='{primaryPath}', "
                          + $"BackupPath='{backupPath}', "
                          + $"Revision={snapshot.Revision}, "
                          + $"CurrencyBalances={snapshot.CurrencyBalances.Count}, "
                          + $"UpgradeLevels={snapshot.UpgradeLevels.Count}, "
                          + $"JsonBytes={Encoding.UTF8.GetByteCount(json)}");
#endif

                return new EconomyPersistenceResult(
                    EconomyPersistenceStatus.Saved,
                    operation,
                    "Saved economy state.",
                    exception: null);
            }
            catch (Exception exception)
            {
                Debug.LogError("Economy save write failed. "
                               + exception.Message
                               + $" Path='{primaryPath}'");
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
    }
}
