using System.ComponentModel.DataAnnotations;
using BackEnd.Utils.Const;
using Microsoft.EntityFrameworkCore;
using OrchidStore.Application.CQRS;
using OrchidStore.Application.Features;
using OrchidStore.Application.Repositories;
using OrchidStore.Domain.ReadModels;
using OrchidStore.Domain.WriteModels;

namespace OrchidStore.Application.Features.Accounts.Commands;

public class AccountUpdateCommand : AbstractApiRequest, ICommand<CommandResponse>
{
    [Required(ErrorMessage = "Account ID is required.")]
    public int AccountId { get; set; }

    public string? AccountName { get; set; }

    public string? Email { get; set; }
}

/// <summary>
/// Command handler for updating an account.
/// </summary>
public class AccountUpdateCommandHandler : ICommandHandler<AccountUpdateCommand, CommandResponse>
{
    private readonly ICommandRepository<Account> _accountRepository;

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="accountRepository"></param>
    public AccountUpdateCommandHandler(ICommandRepository<Account> accountRepository)
    {
        _accountRepository = accountRepository;
    }

    /// <summary>
    /// Handles the account update command.
    /// </summary>
    /// <param name="request"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public async Task<CommandResponse> Handle(AccountUpdateCommand request, CancellationToken cancellationToken)
    {
        var response = new CommandResponse { Success = false };
        // Find existing account
        var existingAccount = await _accountRepository.Find(x => x.AccountId == request.AccountId)
            .FirstOrDefaultAsync(cancellationToken);
        if (existingAccount == null)
        {
            response.SetMessage(MessageId.I00000, "Account not found.");
            return response;
        }

        // Validate email if provided and different from current
        if (!string.IsNullOrEmpty(request.Email) && request.Email != existingAccount.Email)
        {
            var emailExists = await _accountRepository
                .Find(x => x.Email == request.Email && x.AccountId != request.AccountId)
                .FirstOrDefaultAsync(cancellationToken);
            if (emailExists != null)
            {
                response.SetMessage(MessageId.I00000, "Email already exists.");
                return response;
            }

            existingAccount.Email = request.Email;
        }

        // Update fields if provided
        if (!string.IsNullOrEmpty(request.AccountName))
        {
            existingAccount.AcountName = request.AccountName;
        }

        // Save changes
        _accountRepository.Update(existingAccount);
        await _accountRepository.SaveChangesAsync(existingAccount.Email);

        // Session save changes
        _accountRepository.Store(AccountCollection.FromWriteModel(existingAccount), existingAccount.Email, true);
        await _accountRepository.SessionSavechanges();

        // True
        response.Success = true;
        response.SetMessage(MessageId.I00001);
        return response;
    }
}