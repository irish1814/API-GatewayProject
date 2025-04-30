using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel;

namespace ApiGateway.Models.Entities
{
    /// <summary>
    /// Represents a user entity for the API Gateway system.
    /// Contains identity information, authentication credentials, and a unique API key for access control.
    /// </summary>
    [Table("Users")]
    public class User
    {
        /// <summary>
        /// Gets or initializes the unique wallet identifier assigned to the user for managing cryptocurrency operations.
        /// This serves as the primary key.
        /// </summary>
        [Key]
        [Column("WalletId")]
        [DisplayName("Crypto Wallet ID")]
        public Guid WalletId { get; init; } = Guid.NewGuid();

        /// <summary>
        /// Gets or sets the display name of the user.
        /// This is an optional field.
        /// </summary>
        [Column("Name")]
        [DisplayName("User Name")]
        public string? Name { get; set; }

        /// <summary>
        /// Gets or sets the email address of the user.
        /// This is a required field and should be unique.
        /// </summary>
        [Required]
        [Column("Email")]
        [DisplayName("User Email")]
        public required string Email { get; set; }

        /// <summary>
        /// Gets or sets the password used for authenticating the user.
        /// This field is required.
        /// </summary>
        [Required]
        [Column("Password")]
        public required string Password { get; set; }

        /// <summary>
        /// Gets or sets the API key assigned to the user.
        /// This key is used to authorize requests to protected endpoints.
        /// </summary>
        [Required]
        [Column("ApiKey")]
        [DisplayName("API Key")]
        public Guid ApiKey { get; set; }
    }
}
