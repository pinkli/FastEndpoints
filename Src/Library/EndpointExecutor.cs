﻿using FastEndpoints.Validation;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;

namespace FastEndpoints;

internal record EndpointDefinitionCacheEntry(
    Func<object> CreateInstance,
    IValidator? Validator,
    object? PreProcessors,
    object? PostProcessors);

internal record ServiceBoundPropCacheEntry(
    Type PropType,
    Action<object, object> PropSetter);

[HideFromDocs]
public static class EndpointExecutor
{
    //key: endpoint route with verb prefixed - {verb}:{path/of/route}
    internal static Dictionary<string, EndpointDefinitionCacheEntry> CachedEndpointDefinitions { get; } = new();

    //key: TEndpoint
    internal static Dictionary<Type, ServiceBoundPropCacheEntry[]> CachedServiceBoundProps { get; } = new();

    //note: this handler is called by .net for each http request
    public static Task HandleAsync(HttpContext ctx, CancellationToken cancellation)
    {
        var ep = (RouteEndpoint?)ctx.GetEndpoint();
        var routePath = ep?.RoutePattern.RawText;
        var epDef = CachedEndpointDefinitions[$"{ctx.Request.Method}:{routePath}"];
        var endpointInstance = (BaseEndpoint)epDef.CreateInstance();

        var respCacheAttrib = ep?.Metadata.OfType<ResponseCacheAttribute>().FirstOrDefault();
        if (respCacheAttrib is not null)
            ResponseCacheExecutor.Execute(ctx, respCacheAttrib);

        ResolveServices(endpointInstance, ctx);

#pragma warning disable CS8601,CS8604
        return endpointInstance.ExecAsync(ctx, epDef.Validator, epDef.PreProcessors, epDef.PostProcessors, cancellation);
#pragma warning restore CS8601,CS8604
    }

    private static void ResolveServices(object endpointInstance, HttpContext ctx)
    {
#pragma warning disable CS8604
        if (CachedServiceBoundProps.TryGetValue(endpointInstance.GetType(), out var props))
        {
            for (int i = 0; i < props.Length; i++)
            {
                var prop = props[i];
                var serviceInstance = ctx.RequestServices.GetService(prop.PropType);
                prop.PropSetter(endpointInstance, serviceInstance);
            }
        }
#pragma warning restore CS8604
    }
}

