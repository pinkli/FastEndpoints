﻿using Shared;
using TestCases.ClientStreamingTest;
using Xunit;

namespace RemoteProcedureCalls;

public class ClientStreamCommand : RPCTestBase
{
    public ClientStreamCommand(AppFixture fixture) : base(fixture) { }

    [Fact]
    public async Task Client_Stream()
    {
        var input = GetDataStream();

        var report = await remote.ExecuteClientStream<CurrentPosition, ProgressReport>(
            input, typeof(IAsyncEnumerable<CurrentPosition>), default);

        report.LastNumber.Should().Be(5);

        static async IAsyncEnumerable<CurrentPosition> GetDataStream()
        {
            var i = 0;
            while (i < 5)
            {
                i++;
                yield return new() { Number = i };
            }
        }
    }
}
