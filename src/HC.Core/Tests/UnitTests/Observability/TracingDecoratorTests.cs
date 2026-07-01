using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using HC.Core.Infrastructure.Observability;
using MediatR;

namespace HC.Core.UnitTests.Observability;

public sealed class TracingDecoratorTests : IDisposable
{
    private readonly ActivitySource _source = new("HC.LIS.Test");
    private readonly List<Activity> _started = [];
    private readonly List<Activity> _stopped = [];
    private readonly ActivityListener _listener;

    public TracingDecoratorTests()
    {
        _listener = new ActivityListener
        {
            ShouldListenTo = s => s.Name == "HC.LIS.Test",
            Sample = (ref ActivityCreationOptions<ActivityContext> _) =>
                ActivitySamplingResult.AllDataAndRecorded,
            ActivityStarted = _started.Add,
            ActivityStopped = _stopped.Add
        };
        ActivitySource.AddActivityListener(_listener);
    }

    public void Dispose()
    {
        _listener.Dispose();
        _source.Dispose();
    }

    [Fact]
    public async Task VoidDecoratorStartsActivityNamedAfterRequest()
    {
        var sut = new TracingCommandHandlerDecorator<FakeCommand>(new FakeCommandHandler(), _source);

        await sut.Handle(new FakeCommand(), CancellationToken.None).ConfigureAwait(true);

        _started.Should().ContainSingle(a => a.OperationName == nameof(FakeCommand));
    }

    [Fact]
    public async Task VoidDecoratorSetsOkStatusOnSuccess()
    {
        var sut = new TracingCommandHandlerDecorator<FakeCommand>(new FakeCommandHandler(), _source);

        await sut.Handle(new FakeCommand(), CancellationToken.None).ConfigureAwait(true);

        _stopped.Should().ContainSingle(a => a.Status == ActivityStatusCode.Ok);
    }

    [Fact]
    public async Task VoidDecoratorSetsErrorStatusAndRethrowsOnException()
    {
        var sut = new TracingCommandHandlerDecorator<FakeCommand>(new ThrowingCommandHandler(), _source);

        Func<Task> act = () => sut.Handle(new FakeCommand(), CancellationToken.None);

        await act.Should().ThrowAsync<InvalidOperationException>().ConfigureAwait(true);
        _stopped.Should().ContainSingle(a => a.Status == ActivityStatusCode.Error);
    }

    [Fact]
    public async Task ResultDecoratorStartsActivityAndReturnsResult()
    {
        var sut = new TracingCommandHandlerWithResultDecorator<FakeQuery, string>(new FakeQueryHandler(), _source);

        var result = await sut.Handle(new FakeQuery(), CancellationToken.None).ConfigureAwait(true);

        result.Should().Be("ok");
        _started.Should().ContainSingle(a => a.OperationName == nameof(FakeQuery));
    }

    private sealed record FakeCommand : IRequest;

    private sealed record FakeQuery : IRequest<string>;

    private sealed class FakeCommandHandler : IRequestHandler<FakeCommand>
    {
        public Task Handle(FakeCommand request, CancellationToken cancellationToken) => Task.CompletedTask;
    }

    private sealed class ThrowingCommandHandler : IRequestHandler<FakeCommand>
    {
        public Task Handle(FakeCommand request, CancellationToken cancellationToken) =>
            throw new InvalidOperationException("boom");
    }

    private sealed class FakeQueryHandler : IRequestHandler<FakeQuery, string>
    {
        public Task<string> Handle(FakeQuery request, CancellationToken cancellationToken) =>
            Task.FromResult("ok");
    }
}
