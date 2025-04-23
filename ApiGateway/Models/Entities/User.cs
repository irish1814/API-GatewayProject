using System.ComponentModel;

namespace ApiGateway.Models.Entities
{
    public class User
    {
        [DisplayName("User ID: ")]
        public Guid Id { get; set; }

        [DisplayName("User Name: ")]
        public string? Name { get; set; }

        [DisplayName("User Email: ")]
        public required string Email { get; set; }

        public required string Password { get; set; }
    }
}
