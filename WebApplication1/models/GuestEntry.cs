using System.ComponentModel.DataAnnotations;

namespace WebApplication1.models;

public class GuestEntry
{
    [Key]
    public required string IpId { get; set; }
    public DateTime Date { get; set; }
}