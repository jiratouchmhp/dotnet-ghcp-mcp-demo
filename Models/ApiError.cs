namespace Backend.Models;

public record ApiError(string Message, string? Details = null);