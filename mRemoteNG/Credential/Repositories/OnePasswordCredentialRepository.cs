// mRemoteNG/Credential/Repositories/OnePasswordCredentialRepository.cs
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Security;
using Newtonsoft.Json;
using mRemoteNG.Credential;
using mRemoteNG.Tools.CustomCollections;

namespace mRemoteNG.Credential.Repositories
{
    public class OnePasswordCredentialRepository : ICredentialRepository
    {
        public ICredentialRepositoryConfig Config { get; }
        public string TypeName => "OnePassword";
        public bool IsReadOnly => true;
        public IList<ICredentialRecord> CredentialRecords { get; } = new List<ICredentialRecord>();
        public bool IsLoaded { get; private set; } = false;

        public event EventHandler<CollectionUpdatedEventArgs<ICredentialRecord>>? CredentialsUpdated;
        public event EventHandler? RepositoryConfigUpdated;

        public OnePasswordCredentialRepository(ICredentialRepositoryConfig config)
        {
            Config = config ?? throw new ArgumentNullException(nameof(config));
        }

        public void Load()
        {
            LoadCredentials(null);
        }

        public void LoadCredentials(SecureString key)
        {
            CredentialRecords.Clear();

            try
            {
                string listArgs = $"item list --vault \"{Config.Source}\" --categories Login --format json";
                string listOutput = RunOpCommand(listArgs);
                var items = JsonConvert.DeserializeObject<List<ItemSummary>>(listOutput);

                foreach (var item in items)
                {
                    string getArgs = $"item get \"{item.Id}\" --format json";
                    string fullOutput = RunOpCommand(getArgs);
                    var fullItem = JsonConvert.DeserializeObject<FullItem>(fullOutput);

                    var username = fullItem.Fields.FirstOrDefault(f => string.Equals(f.Label, "username", StringComparison.OrdinalIgnoreCase))?.Value;
                    var password = fullItem.Fields.FirstOrDefault(f => string.Equals(f.Label, "password", StringComparison.OrdinalIgnoreCase))?.Value;
                    var domain = fullItem.Fields.FirstOrDefault(f => string.Equals(f.Label, "domain", StringComparison.OrdinalIgnoreCase))?.Value;

                    if (!string.IsNullOrEmpty(username) && !string.IsNullOrEmpty(password))
                    {
                        var record = new CredentialRecord
                        {
                            Id = Guid.NewGuid(),
                            Title = item.Title,
                            Username = username,
                            Password = password,
                            Domain = domain ?? string.Empty
                        };
                        CredentialRecords.Add(record);
                    }
                }

                IsLoaded = true;
                CredentialsUpdated?.Invoke(this, new CollectionUpdatedEventArgs<ICredentialRecord>(ActionType.Added, CredentialRecords));
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.WriteLine($"1Password Load Error: {ex.Message}");
                File.AppendAllText("1password.log", $"Error: {ex.Message}\n");
                throw;
            }
        }

        public void SaveCredentials(SecureString key)
        {
            throw new NotImplementedException("1Password repository is read-only.");
        }

        public void UnloadCredentials()
        {
            CredentialRecords.Clear();
            IsLoaded = false;
            CredentialsUpdated?.Invoke(this, new CollectionUpdatedEventArgs<ICredentialRecord>(ActionType.Removed, new List<ICredentialRecord>()));
        }

        private string RunOpCommand(string arguments)
        {
            using var process = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = "op",
                    Arguments = arguments,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                }
            };

            process.Start();
            string output = process.StandardOutput.ReadToEnd();
            string error = process.StandardError.ReadToEnd();
            process.WaitForExit();

            if (process.ExitCode != 0)
                throw new Exception($"1Password CLI error: {error}");

            return output;
        }

        private class ItemSummary
        {
            public string Id { get; set; } = string.Empty;
            public string Title { get; set; } = string.Empty;
        }

        private class FullItem
        {
            public List<Field> Fields { get; set; } = new();
        }

        private class Field
        {
            public string Label { get; set; } = string.Empty;
            public string Value { get; set; } = string.Empty;
        }
    }
}