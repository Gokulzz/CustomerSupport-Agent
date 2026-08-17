using System.ComponentModel.DataAnnotations;

namespace CustomerSupport.Application.SupportTickets;

public sealed record CreateSupportTicketRequest(
    string Subject,
    string Description);
