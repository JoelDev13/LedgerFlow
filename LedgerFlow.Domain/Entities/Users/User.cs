using LedgerFlow.Domain.Common;

namespace LedgerFlow.Domain.Entities.Users;

public sealed class User : AggregateRoot<Guid>
{
    public string Email { get; private set; } = default!;
    public string PasswordHash { get; private set; } = default!;
    public string FirstName { get; private set; } = default!;
    public string LastName { get; private set; } = default!;
    public DateTime CreatedAt { get; private set; }

    private User()
    {
    }

    private User(Guid id, string email, string passwordHash, string firstName, string lastName)
        : base(id)
    {
        Email = email;
        PasswordHash = passwordHash;
        FirstName = firstName;
        LastName = lastName;
        CreatedAt = DateTime.UtcNow;
    }

    public static Result<User> Create(
        string email,
        string passwordHash,
        string firstName,
        string lastName)
    {
        if (string.IsNullOrWhiteSpace(email))
        {
            return Result.Failure<User>(UserErrors.EmailRequired);
        }

        if (!email.Contains('@'))
        {
            return Result.Failure<User>(UserErrors.InvalidEmail);
        }

        if (string.IsNullOrWhiteSpace(passwordHash))
        {
            return Result.Failure<User>(UserErrors.PasswordRequired);
        }

        if (string.IsNullOrWhiteSpace(firstName))
        {
            return Result.Failure<User>(UserErrors.FirstNameRequired);
        }

        if (string.IsNullOrWhiteSpace(lastName))
        {
            return Result.Failure<User>(UserErrors.LastNameRequired);
        }

        return Result.Success(new User(
            Guid.NewGuid(),
            email.Trim().ToLowerInvariant(),
            passwordHash,
            firstName.Trim(),
            lastName.Trim()));
    }
}
