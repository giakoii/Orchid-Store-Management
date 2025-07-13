using System.ComponentModel.DataAnnotations;
using BackEnd.Utils.Const;
using Microsoft.EntityFrameworkCore;
using OrchidStore.Application.CQRS;
using OrchidStore.Application.Repositories;
using OrchidStore.Domain.ReadModels;
using OrchidStore.Domain.WriteModels;

namespace OrchidStore.Application.Features.Accounts.Commands;

public class AccountRegisterCommand : AbstractApiRequest, ICommand<CommandResponse>
{
    [Required(ErrorMessage = "Email is required.")]
    [EmailAddress(ErrorMessage = "Invalid email format.")]
    public string Email { get; set; }
    
    [Required(ErrorMessage = "Password is required.")]
    public string Password { get; set; }
    
    [Required(ErrorMessage = "Account name is required.")]
    public string AccountName { get; set; }
}

public class AccountRegisterCommandHandler : ICommandHandler<AccountRegisterCommand, CommandResponse>
{
    private readonly ICommandRepository<Account> _accountRepository;
    private readonly ICommandRepository<Role> _roleRepository;

    public AccountRegisterCommandHandler(ICommandRepository<Account> accountRepository, ICommandRepository<Role> roleRepository)
    {
        _accountRepository = accountRepository;
        _roleRepository = roleRepository;
    }

    /// <summary>
    /// Handles the account registration command.
    /// </summary>
    /// <param name="request"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public async Task<CommandResponse> Handle(AccountRegisterCommand request, CancellationToken cancellationToken)
    {
        var response = new CommandResponse { Success = false };
        
        // Validate email
        var emailExist = await _accountRepository.Find(x => x.Email == request.Email).FirstOrDefaultAsync();
        if (emailExist != null)
        {
            response.SetMessage(MessageId.I00000, "Email already exists.");
            return response;
        }
        
        // Validate role
        var role = await _roleRepository.Find(x => x.RoleName == ConstantEnum.UserRole.Customer.ToString()).FirstOrDefaultAsync();
        if (role == null)
        {
            response.SetMessage(MessageId.E99999);
            return response;
        }
        // Begin transaction
        await _accountRepository.ExecuteInTransactionAsync(async () =>
        {
            // Insert new account
            var newAccount = new Account
            {
                Email = request.Email,
                Password = BCrypt.Net.BCrypt.HashPassword(request.Password, workFactor: 12),
                AcountName = request.AccountName,
                RoleId = role.RoleId,
            };

            // Context Save changes
            await _accountRepository.AddAsync(newAccount);
            await _accountRepository.SaveChangesAsync(request.Email);
            
            // Session save changes
            _accountRepository.Store(AccountCollection.FromWriteModel(newAccount, true), request.Email);
            await _accountRepository.SessionSavechanges();
        
            response.Success = true;
            response.SetMessage(MessageId.I00001);
            return true;
        });
        return response;
    }
}