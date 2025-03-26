using System;

namespace Backend.Dtos;

public record CustomerDto(
    int Id,
    string FirstName,
    string LastName,
    string Email,
    string? PhoneNumber,
    DateTime CreatedAt,
    DateTime UpdatedAt
);