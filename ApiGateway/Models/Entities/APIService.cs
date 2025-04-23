using System.ComponentModel;

namespace ApiGateway.Models.Entities
{
    public class APIService
    {
        [DisplayName("API Service Name: ")]
        public string? APIName { get; set; }

        [DisplayName("API Key: ")]
        public required string APIKey { get; set; }
    }
}
