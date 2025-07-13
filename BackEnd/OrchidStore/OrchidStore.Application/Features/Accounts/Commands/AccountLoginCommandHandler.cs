using BackEnd.Utils.Const;
using Microsoft.EntityFrameworkCore;
using OrchidStore.Application.CQRS;
using OrchidStore.Application.Repositories;
using OrchidStore.Domain.WriteModels;

namespace OrchidStore.Application.Features.Accounts.Commands;
public class AccountLoginCommand : AbstractApiRequest, ICommand<AccountLoginResponse>
{
    public string? Username { get; set; }

    public string? Password { get; set; }
}

public class AccountLoginCommandHandler : ICommandHandler<AccountLoginCommand, AccountLoginResponse>
{
    private readonly ICommandRepository<Account> _accountRepository;

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="accountRepository"></param>
    public AccountLoginCommandHandler(ICommandRepository<Account> accountRepository)
    {
        _accountRepository = accountRepository;
    }

    /// <summary>
    /// Handles the login command for an account.
    /// </summary>
    /// <param name="request"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public async Task<AccountLoginResponse> Handle(AccountLoginCommand request, CancellationToken cancellationToken)
    {
        var response = new AccountLoginResponse
        {
            Success = false,
            Response = new AccountLoginEntity()
        };
        
        // Check if the request is valid
        if(request.Username == null || request.Password == null)
        {
            response.SetMessage(MessageId.I00001, "Username and password are required.");
            return response;
        }
        
        // Validate username
        var account = await _accountRepository.Find(x => x.Email == request.Username && x.IsActive, true, account => account.Role).FirstOrDefaultAsync(cancellationToken);
        if (account == null)
        {
            response.SetMessage(MessageId.I00000, "Invalid username");
            return response;
        }
        
        // Check if the password matches
        if (!BCrypt.Net.BCrypt.Verify(request.Password, account.Password))
        {
            response.SetMessage(MessageId.I00000, "Invalid password");
            return response;
        }

        response.Response.AccountId = account.AccountId;
        response.Response.Email = account.Email;
        response.Response.AccountName = account.AcountName;
        response.Response.RoleName = account.Role.RoleName;

        // True
        response.Success = true;
        response.SetMessage(MessageId.I00001);
        return response;
    }
}

public class AccountLoginResponse : AbstractApiResponse<AccountLoginEntity>
{
    public override required AccountLoginEntity Response { get; set; }
}

public class AccountLoginEntity
{
    public int AccountId { get; set; }
    
    public string Email { get; set; } = null!;
    
    public string AccountName { get; set; } = null!;
    
    public string RoleName { get; set; } = null!;
}