using System.ComponentModel.DataAnnotations;
using BackEnd.Utils.Const;
using OrchidStore.Application.CQRS;
using OrchidStore.Application.Repositories;
using OrchidStore.Domain.WriteModels;

namespace OrchidStore.Application.Features.Accounts.Queries;

public class SelectAccountProfileQuery : AbstractApiRequest, IQuery<SelectAccountProfileResponse>
{
    [Required(ErrorMessage = "Account ID is required.")]
    public int AccountId { get; set; }
}

public class SelectAccountProfileQueryHandler : IQueryHandler<SelectAccountProfileQuery, SelectAccountProfileResponse>
{
    private readonly IQueryRepository<Account> _accountRepository;

    public SelectAccountProfileQueryHandler(IQueryRepository<Account> accountRepository)
    {
        _accountRepository = accountRepository;
    }

    /// <summary>
    /// Handles the select account profile query.
    /// </summary>
    /// <param name="request"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public async Task<SelectAccountProfileResponse> Handle(SelectAccountProfileQuery request, CancellationToken cancellationToken)
    {
        var response = new SelectAccountProfileResponse { Success = false };
        var account = await _accountRepository.FindOneAsync(x => x.AccountId == request.AccountId);
        if (account == null)
        {
            response.SetMessage(MessageId.I00000, "Account not found.");
            return response;
        }

        response.Response = new SelectAccountEntity
        {
            AccountId = account.AccountId,
            AccountName = account.AcountName,
            Email = account.Email,
        };

        response.Success = true;
        response.SetMessage(MessageId.I00001);
        return response;
    }
}

public class SelectAccountProfileResponse : AbstractApiResponse<SelectAccountEntity>
{
    public override SelectAccountEntity Response { get; set; }
}

public class SelectAccountEntity
{
    public int AccountId { get; set; }
    public string AccountName { get; set; } = null!;
    public string Email { get; set; } = null!;
}