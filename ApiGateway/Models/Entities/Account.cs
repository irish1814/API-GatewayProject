using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ApiGateway.Models.Entities
{
    /// <summary>
    /// Represents a user's cryptocurrency account.
    /// </summary>
    [Table("Accounts")]
    public class Account
    {
        /// <summary>
        /// The unique Wallet ID associated with the user. This serves as the primary key.
        /// </summary>
        [Key]
        [Column("WalletId")]
        public Guid WalletId { get; init; }

        /// <summary>
        /// The balance of Bitcoin (BTC) for the user.
        /// </summary>
        [Column("Bitcoin")]
        public decimal Bitcoin { get; set; }

        /// <summary>
        /// The balance of Ethereum (ETH) for the user.
        /// </summary>
        [Column("Ethereum")]
        public decimal Ethereum { get; set; }

        /// <summary>
        /// The balance of Solana (SOL) for the user.
        /// </summary>
        [Column("Solana")]
        public decimal Solana { get; set; }

        /// <summary>
        /// The balance of Ripple (XRP) for the user.
        /// </summary>
        [Column("Ripple")]
        public decimal Ripple { get; set; }

        /// <summary>
        /// The balance of Litecoin (LTC) cryptocurrency for the user.
        /// </summary>
        [Column("Litecoin")]
        public decimal Litecoin { get; set; }

        /// <summary>
        /// The balance of Cardano (ADA) cryptocurrency for the user.
        /// </summary>
        [Column("Litecoin")]
        public decimal Cardano { get; set; }

        /// <summary>
        /// The USD equivalent balance for the user.
        /// </summary>
        [Column("Balance")]
        public decimal Balance { get; set; }
    }
}
