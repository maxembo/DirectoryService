using CSharpFunctionalExtensions;
using DirectoryService.Contracts.Abstractions;

namespace DirectoryService.Application.Abstractions;

public interface ICommandHandler<TResponse, in TCommand>
    where TCommand : ICommand
{
    Task<Result<TResponse>> Handle(TCommand request, CancellationToken cancellationToken = default);
}

public interface ICommandHandler<in TCommand>
    where TCommand : ICommand
{
    Task<UnitResult<string>> Handle(TCommand request, CancellationToken cancellationToken = default);
}