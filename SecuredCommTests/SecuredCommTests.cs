using System;
using Xunit;
using SecuredCommunication;

namespace SecuredCommTests
{
    public class SecuredCommTests
    {
        [Fact]
        public void Sanity_VerifyCanBeCreated()
        {
            var secretsMock = (ISecretsManagement)new SecretsManagementMock();
            var sec = new SecuredComm(secretsMock,
                                      new Uri("amqp://user:pass@host:10000/vhost"), "verficationKey", "signKey", false);

        }
    }
}
