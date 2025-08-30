// mRemoteNG/Credential/Repositories/OnePasswordConfig.cs
using System;
using System.ComponentModel;
using mRemoteNG.Credential;

namespace mRemoteNG.Credential.Repositories
{
    [Serializable]
    public class OnePasswordConfig : ICredentialRepositoryConfig
    {
        public event PropertyChangedEventHandler PropertyChanged;

        public Guid Id { get; set; } = Guid.NewGuid();
        public string TypeName => "OnePassword";
        public string Title { get; set; } = "1Password Vault";
        public string Source { get; set; } = "Private";
        public string EncryptionKey { get; set; } = string.Empty;
        public string Key { get; set; } = string.Empty; // Added for ICredentialRepositoryConfig

        public override bool Equals(object? obj)
        {
            if (obj is not OnePasswordConfig other) return false;
            return Id == other.Id && Title == other.Title && Source == other.Source && EncryptionKey == other.EncryptionKey && Key == other.Key;
        }

        public override int GetHashCode()
        {
            unchecked
            {
                int hash = 17;
                hash = hash * 23 + Id.GetHashCode();
                hash = hash * 23 + (Title?.GetHashCode() ?? 0);
                hash = hash * 23 + (Source?.GetHashCode() ?? 0);
                hash = hash * 23 + (EncryptionKey?.GetHashCode() ?? 0);
                hash = hash * 23 + (Key?.GetHashCode() ?? 0);
                return hash;
            }
        }

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}