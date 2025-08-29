// mRemoteNG/Credential/Repositories/OnePasswordConfig.cs
using System;
using mRemoteNG.Credential;

namespace mRemoteNG.Credential.Repositories
{
    [Serializable]
    public class OnePasswordConfig : ICredentialRepositoryConfig
    {
        public Guid Id { get; set; } = Guid.NewGuid(); // Unique ID for the repository
        public string TypeName => "OnePassword";
        public string Title { get; set; } = "1Password Vault";
        public string Source { get; set; } = "Private"; // Default 1Password vault name
        public string EncryptionKey { get; set; } = string.Empty; // Unused for 1Password

        public override bool Equals(object obj)
        {
            if (!(obj is OnePasswordConfig other)) return false;
            return Id == other.Id && Title == other.Title && Source == other.Source && EncryptionKey == other.EncryptionKey;
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
                return hash;
            }
        }
    }
}
