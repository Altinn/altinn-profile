using System;
using System.Net.Http;
using System.Threading.Tasks;
using Altinn.Profile.Integrations.OfficialAddressRegister;
using Moq;
using Xunit;

namespace Altinn.Profile.Tests.Profile.Integrations.OficialAddressRegister
{
    public class OfficialAddressHttpClientTests
    {
        private readonly Mock<HttpMessageHandler> _messageHandler = new();

        [Fact]
        public async Task ThrowsWhenMissingEndpointURl()
        {
            var httpClient = new HttpClient(_messageHandler.Object);
            var client = new OfficialAddressHttpClient(httpClient);

            await Assert.ThrowsAsync<ArgumentException>(async () => await client.GetAddressChangesAsync(null));
        }

        [Fact]
        public async Task ThrowsWhenEmptyEndpointURl()
        {
            var httpClient = new HttpClient(_messageHandler.Object);
            var client = new OfficialAddressHttpClient(httpClient);

            await Assert.ThrowsAsync<ArgumentException>(async () => await client.GetAddressChangesAsync(string.Empty));
        }

        [Fact]
        public async Task ThrowsWhenInvalidEndpointURl()
        {
            var httpClient = new HttpClient(_messageHandler.Object);
            var client = new OfficialAddressHttpClient(httpClient);

            await Assert.ThrowsAsync<ArgumentException>(async () => await client.GetAddressChangesAsync("notAnUrl"));
        }
    }
}
