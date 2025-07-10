using MediatR;

namespace OrchidStore.Application.CQRS;

public interface IQuery<out TResponse> : IRequest<TResponse> 
    where TResponse : notnull
{
    
}