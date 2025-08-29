using System;
using mRemoteNG.Credential;

namespace mRemoteNG.Credential.Repositories
{
    [Serializable]
    public class OnePasswordConfig : ICredentialRepositoryConfig
    {
        public string TypeName => "OnePassword";
        public string Title { get; set; } = "1Password Vault";
        public string Source { get; set; } = "Private"; // Default vault name; configurable in UI
        public string EncryptionKey { get; set; } = string.Empty; // Unused for 1Password

        public override bool Equals(object obj)
        {
            if (!(obj is OnePasswordConfig other)) return false;
            return Title == other.Title && Source == other.Source && EncryptionKey == other.EncryptionKey;
        }

        public override int GetHashCode()
        {
            unchecked
            {
                int hash = 17;
                hash = hash * 23 + (Title?.GetHashCode() ?? 0);
                hash = hash * 23 + (Source?.GetHashCode() ?? 0);
                hash = hash * 23 + (EncryptionKey?.GetHashCode() ?? 0);
                return hash;
            }
        }
    }
}
