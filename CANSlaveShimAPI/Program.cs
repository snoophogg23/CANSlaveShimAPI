using Microsoft.AspNetCore.DataProtection.KeyManagement;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Concurrent;
using System.Text.Json.Serialization;
using System.Xml.Linq;

namespace CANSlaveShimAPI
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateSlimBuilder(args);

            builder.Services.AddSingleton<CANSlaveRepo>();
            builder.Services.ConfigureHttpJsonOptions(options =>
            {
                options.SerializerOptions.TypeInfoResolverChain.Insert(0, StringJsonSerializerContext.Default);
                options.SerializerOptions.TypeInfoResolverChain.Insert(1, EnumerableStringJsonSerializerContext.Default);
            });

            var app = builder.Build();

            var canSlaveApi = app.MapGroup("/canslave/api");
            canSlaveApi.MapGet("/", ([FromServices] CANSlaveRepo repo)
                => repo.GetActiveAsync());
            canSlaveApi.MapGet("{network}/{node}/{index}/{subindex}", async (int network, int node, int index, int subindex, [FromServices] CANSlaveRepo repo)
            => 
            {
                var result = await repo.GetAsync(network, node, index, subindex);
                if (result != null)
                    return Results.Ok(result);
                return Results.Empty;
            });
            canSlaveApi.MapPost("{network}/{node}/{index}/{subindex}", async (int network, int node, int index, int subindex, [FromForm] string value, [FromServices] CANSlaveRepo repo)
            =>
            {
                await repo.PutAsync(network, node, index, subindex, value);
                return Results.Accepted();
            }).DisableAntiforgery();
            app.Run();
        }
    }

    [JsonSerializable(typeof(string))]
    internal partial class StringJsonSerializerContext : JsonSerializerContext
    {

    }
    [JsonSerializable(typeof(IEnumerable<string>))]
    internal partial class EnumerableStringJsonSerializerContext : JsonSerializerContext
    {

    }

    public class CANSlaveRepo
    {
        ConcurrentDictionary<(int index, int subIndex), string> _values 
            = new ConcurrentDictionary<(int index, int subIndex), string>();

        public Task<IEnumerable<string>> GetActiveAsync()
            => Task.FromResult(_values.Select(v => $"{v.Key.index} {v.Key.subIndex} {v.Value}"));

        public Task<string?> GetAsync(int network, int node, int index, int subindex)
        {
            if (_values.TryGetValue((index, subindex), out string? value))
                return Task.FromResult((string?)value);
            return Task.FromResult((string?)null);
        }

        public Task PutAsync(int network, int node, int index, int subindex, string value)
        {
            _values.AddOrUpdate((index, subindex), value, (key, oldValue) => value);
            return Task.CompletedTask;
        }
    }
}
