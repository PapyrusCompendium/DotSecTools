using Microsoft.Extensions.Configuration;

namespace DotGpt.Config {
    internal class OpenAiConfig {
        public string ModelId { get; set; }
        public string EmbeddingModelId { get; set; }
        public string ApiKey { get; set; }
        public string OrganizationId { get; set; }

        public OpenAiConfig(IConfiguration configuration) {
            configuration.GetSection(nameof(OpenAiConfig)).Bind(this);
        }
    }
}
