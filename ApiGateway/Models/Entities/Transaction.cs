using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ApiGateway.Models.Entities
{
    /// <summary>
    /// Represents a cryptocurrency transaction (buy or sell) performed by a user.
    /// Stores transaction details including type, price at time of transaction, and timestamp.
    /// </summary>
    [Table("Transactions")]
    public class Transaction
    {
        /// <summary>
        /// Gets or sets the unique identifier for the transaction.
        /// </summary>
        [Key]
        public Guid Id { get; init; } = Guid.NewGuid();

        /// <summary>
        /// Gets or sets the wallet ID of the user who performed the transaction.
        /// </summary>
        public Guid WalletId { get; init; }

        /// <summary>
        /// Gets or sets the type of transaction. Valid values are "buy" or "sell".
        /// </summary>
        [Required]
        public string Type { get; init; } = "buy";

        /// <summary>
        /// Gets or sets the ID of the cryptocurrency involved in the transaction.
        /// </summary>
        public int CryptoId { get; init; }

        /// <summary>
        /// Gets or sets the price of the cryptocurrency at the moment the transaction was executed.
        /// </summary>
        public decimal PriceAtTransaction { get; init; }

        /// <summary>
        /// Gets or sets the date and time when the transaction occurred.
        /// Stored in UTC.
        /// </summary>
        public DateTime DateTime { get; init; } = DateTime.UtcNow;
    }
}
