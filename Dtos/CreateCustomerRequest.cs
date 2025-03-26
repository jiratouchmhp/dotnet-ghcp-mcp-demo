namespace Backend.Dtos;

public record CreateCustomerRequest(
    string FirstName,
    string LastName,
    string Email,
    string? PhoneNumber
);