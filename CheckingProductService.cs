using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;

namespace MinimalApi;
public class CheckingProductService : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken cancellationToken)
    {
        Console.WriteLine("checking products");
        await Task.Delay(15 * 1000);
        Console.WriteLine("done checking products");
    }
}