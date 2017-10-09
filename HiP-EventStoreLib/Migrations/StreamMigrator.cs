using System.Collections.Generic;
using System.Threading.Tasks;
using System;
using System.Reflection;
using System.Linq;

namespace PaderbornUniversity.SILab.Hip.EventSourcing.Migrations
{
    /// <summary>
    /// Provides methods to update an Event Store stream to the latest version by applying one or multiple
    /// migrations defined in the HiP-DataStore assembly.
    /// </summary>
    public static class StreamMigrator
    {
        public const string StreamVersionMetadataKey = "StreamVersion";

        public static async Task<(int fromVersion, int toVersion)> MigrateAsync(IEventStore store, string streamName, Assembly migrationSource)
        {
            // Get current stream version from metadata
            var initialVersion = await GetStreamVersionAsync(store.Streams[streamName]) ?? 0;
            var currentVersion = initialVersion;

            // Find all applicable migrations in the current assembly
            var availableMigrations = GetAvailableMigrations(migrationSource)
                .Where(t => t.Properties.FromVersion >= currentVersion)
                .OrderBy(t => t.Properties.FromVersion)
                .ToList();

            // Check for ambiguities (i.e. are there multiple migrations for the same source version?)
            if (availableMigrations.GroupBy(t => t.Properties.FromVersion).Any(g => g.Count() > 1))
                throw new InvalidOperationException("The assembly defines multiple migrations for the same source version");

            // Repeatedly apply the migration with FromVersion == currentVersion and maximum ToVersion,
            // until no more migrations are applicable
            MigrationTypeInfo chosenMigration;

            while ((chosenMigration = availableMigrations.FirstOrDefault(t => t.Properties.FromVersion == currentVersion)) != null)
            {
                // from the group of matching migrations, choose the one with maximum ToVersion
                await ExecuteMigrationAsync(chosenMigration, store, streamName);
                currentVersion = chosenMigration.Properties.ToVersion;
            }

            return (initialVersion, currentVersion);
        }

        private static async Task<int?> GetStreamVersionAsync(IEventStream stream)
        {
            var metadata = await stream.TryGetMetadataAsync<int>(StreamVersionMetadataKey);
            return metadata.isSuccessful ? metadata.value : default(int?);
        }

        private static IEnumerable<MigrationTypeInfo> GetAvailableMigrations(Assembly migrationSource)
        {
            return migrationSource.DefinedTypes
                .Where(t => t.ImplementedInterfaces.Contains(typeof(IStreamMigration)))
                .Select(t => new MigrationTypeInfo
                {
                    Type = t,
                    Properties = t.GetCustomAttribute<StreamMigrationAttribute>()
                })
                .Where(t =>
                    t.Properties != null && // type must have the [StreamMigration]-attribute
                    t.Properties.ToVersion > t.Properties.FromVersion);
        }

        private static async Task ExecuteMigrationAsync(MigrationTypeInfo migrationType, IEventStore store, string streamName)
        {
            var stream = store.Streams[streamName];
            var migration = (IStreamMigration)Activator.CreateInstance(migrationType.Type.AsType());
            var args = new StreamMigrationArgs(stream);
            await migration.MigrateAsync(args);

            // Soft-delete the stream and recreate it by appending all new events
            await stream.DeleteAsync();
            var newStream = store.Streams[streamName];
            await newStream.AppendManyAsync(args.EventsToAppend);

            // Write new version to the stream's metadata
            await newStream.SetMetadataAsync(StreamVersionMetadataKey, migrationType.Properties.ToVersion);
        }

        class MigrationTypeInfo
        {
            public TypeInfo Type { get; set; }
            public StreamMigrationAttribute Properties { get; set; }
        }
    }
}
