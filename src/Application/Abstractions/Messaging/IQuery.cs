using MediatR;
using Domain.Common;

namespace Application;

public interface IQuery<TResponse> : IRequest<Result<TResponse>>;
