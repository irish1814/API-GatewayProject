using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ApiGateway.Models.Entities
{
    /// <summary>
    /// Represents the historical price of a specific cryptocurrency at a given point in time.
    /// </summary>
    [Table("PriceHistories")]
    public class PriceHistory
    {
        /// <summary>
        /// Primary key for the record.
        /// </summary>
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; init; }

        /// <summary>
        /// Gets or sets the unique identifier of the cryptocurrency (e.g., Coinlore ID).
        /// </summary>
        public int CryptoId { get; init; }

        /// <summary>
        /// Gets or sets the symbol of the cryptocurrency (e.g., BTC, ETH).
        /// </summary>
        public string? Symbol { get; init; }

        /// <summary>
        /// Gets or sets the date and time when the price was recorded.
        /// Stored in UTC.
        /// </summary>
        public DateTime Timestamp { get; init; }

        /// <summary>
        /// Gets or sets the recorded price of the cryptocurrency at the specified timestamp.
        /// </summary>
        public decimal Price { get; init; }
    }
}
