using CSharpFunctionalExtensions;

namespace DirectoryService.Application.Abstractions;

public interface ICommandHandler<TResponse, in TCommand>
{
    Task<Result<TResponse>> Handle(TCommand request, CancellationToken cancellationToken = default);
}

public interface ICommandHandler<in TCommand>
{
    Task<Result> Handle(TCommand request, CancellationToken cancellationToken = default);
}