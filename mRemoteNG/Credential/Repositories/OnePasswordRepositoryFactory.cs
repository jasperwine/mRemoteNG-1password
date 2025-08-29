using mRemoteNG.Credential;

namespace mRemoteNG.Credential.Repositories
{
    public class OnePasswordRepositoryFactory : ICredentialRepositoryFactory
    {
        public string TypeName => "OnePassword";

        public ICredentialRepository Build(ICredentialRepositoryConfig config)
        {
            return new OnePasswordCredentialRepository(config);
        }

        public ICredentialRepositoryConfig GetConfigObject()
        {
            return new OnePasswordConfig();
        }
    }
}
