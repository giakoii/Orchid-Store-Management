using OrchidStore.Application.CQRS;
using OrchidStore.Application.Repositories;
using OrchidStore.Domain.WriteModels;

namespace OrchidStore.Application.Features.Accounts.Commands;

public class LoginCommand(string Username, string Password) : AbstractApiRequest, ICommand<AccountLoginResponse>;

public class AccountLoginCommandHandler : ICommandHandler<LoginCommand, AccountLoginResponse>
{
    private readonly ICommandRepository<Account> _accountRepository;

    public AccountLoginCommandHandler(ICommandRepository<Account> accountRepository)
    {
        _accountRepository = accountRepository;
    }

    public Task<AccountLoginResponse> Handle(LoginCommand request, CancellationToken cancellationToken)
    {
        return null;
    }
}

public class AccountLoginResponse : AbstractApiResponse<AccountLoginEntity>
{
    public override AccountLoginEntity Response { get; set; }
}

public class AccountLoginEntity
{
}