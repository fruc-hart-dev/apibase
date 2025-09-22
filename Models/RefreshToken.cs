using System;

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ApiBase.Models;

public class RefreshToken
{
    [Key]
    public string Token { get; set; } = string.Empty;
    
    [Required]
    public string JwtId { get; set; } = string.Empty;
    
    [Required]
    public DateTime CreationDate { get; set; }
    
    [Required]
    public DateTime ExpiryDate { get; set; }
    
    public bool Used { get; set; }
    public bool Invalidated { get; set; }
    
    [Required]
    public Guid UserId { get; set; }
    
    [ForeignKey(nameof(UserId))]
    public virtual User User { get; set; } = null!;
}
