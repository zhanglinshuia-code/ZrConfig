using ZrConfig.Core;

using var cancellation = new CancellationTokenSource();
Console.CancelKeyPress += (_, e) =>
{
    e.Cancel = true;
    cancellation.Cancel();
};
return ToolCommandRunner.Run(args, cancellationToken: cancellation.Token);
