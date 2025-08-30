// mRemoteNG/Credential/Repositories/OnePasswordCredentialRepository.cs
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
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
        public List<ICredentialRecord> CredentialRecords { get; } = new();

        public event EventHandler<CollectionUpdatedEventArgs<ICredentialRecord>>? CredentialsUpdated;
        public event EventHandler? RepositoryConfigUpdated;

        public OnePasswordCredentialRepository(ICredentialRepositoryConfig config)
        {
            Config = config ?? throw new ArgumentNullException(nameof(config));
        }

        public void Load()
        {
            CredentialRecords.Clear();

            try
            {
                // List all Login items in the specified vault
                string listArgs = $"item list --vault \"{Config.Source}\" --categories Login --format json";
                string listOutput = RunOpCommand(listArgs);
                var items = JsonConvert.DeserializeObject<List<ItemSummary>>(listOutput);

                foreach (var item in items)
                {
                    // Fetch full item details
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
                            Id = Guid.NewGuid(), // 1Password IDs are strings; generate GUID
                            Title = item.Title,
                            Username = username,
                            Password = password,
                            Domain = domain ?? string.Empty
                        };
                        CredentialRecords.Add(record);
                    }
                }

                // Notify UI of updated credentials
                CredentialsUpdated?.Invoke(this, new CollectionUpdatedEventArgs<ICredentialRecord>(ActionType.Added, CredentialRecords));
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.WriteLine($"1Password Load Error: {ex.Message}");
                throw;
            }
        }

        public void Save()
        {
            throw new NotImplementedException("1Password repository is read-only.");
        }

        private string RunOpCommand(string arguments)
        {
            using var process = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = "op", // Ensure 'op' is in PATH or specify full path
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