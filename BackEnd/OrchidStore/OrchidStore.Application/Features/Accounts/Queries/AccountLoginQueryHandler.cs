// using OrchidStore.Application.CQRS;
// using OrchidStore.Application.Repositories;
//
// namespace OrchidStore.Application.Features.Accounts.Queries;
//
// public class LoginCommand(string Username, string Password) : AbstractApiRequest, IQuery<AccountLoginResponse>;
//
//
// public class AccountLoginQueryHandler : IQueryHandler<LoginCommand, AccountLoginResponse>
// {
//     private readonly IQueryRepository<Account> _accountRepository;
//
//     public AccountLoginQueryHandler(ICommandRepository<Account> accountRepository)
//     {
//         _accountRepository = accountRepository;
//     }
//
//     public Task<AccountLoginResponse> Handle(LoginCommand request, CancellationToken cancellationToken)
//     {
//         // Implementation of the login logic goes here
//         throw new NotImplementedException();
//     }
// }
//
// public class AccountLoginResponse : AbstractApiResponse<AccountLoginEntity>
// {
//     public override AccountLoginEntity Response { get; set; }
// }
//
// public class AccountLoginEntity
// {
// }