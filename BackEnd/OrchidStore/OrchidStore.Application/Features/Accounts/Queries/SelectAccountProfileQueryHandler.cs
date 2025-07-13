using System.ComponentModel.DataAnnotations;
using BackEnd.Utils.Const;
using Microsoft.EntityFrameworkCore;
using OrchidStore.Application.CQRS;
using OrchidStore.Application.Features;
using OrchidStore.Application.Logics;
using OrchidStore.Application.Repositories;
using OrchidStore.Domain.ReadModels;
using OrchidStore.Domain.WriteModels;

namespace OrchidStore.Application.Features.Accounts.Queries;

public class SelectAccountProfileQuery : AbstractApiRequest, IQuery<SelectAccountProfileResponse>
{
  
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

public class SelectAccountProfileQueryHandler : IQueryHandler<SelectAccountProfileQuery, SelectAccountProfileResponse>
{
    private readonly IQueryRepository<AccountCollection> _accountRepository;
    private readonly IIdentityService _identityService;

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="accountRepository"></param>
    /// <param name="identityService"></param>
    public SelectAccountProfileQueryHandler(IQueryRepository<AccountCollection> accountRepository, IIdentityService identityService)
    {
        _accountRepository = accountRepository;
        _identityService = identityService;
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
        
        // Get current userId
        var currentUserId = _identityService.GetCurrentUser().UserId;
        
        var account = await _accountRepository.FindOneAsync(x => x.AccountId == int.Parse(currentUserId));
        if (account == null)
        {
            response.SetMessage(MessageId.I00000, "Account not found.");
            return response;
        }

        response.Response = new SelectAccountEntity
        {
            AccountId = account.AccountId,
            AccountName = account.AccountName,
            Email = account.Email,
        };

        response.Success = true;
        response.SetMessage(MessageId.I00001);
        return response;
    }
}
