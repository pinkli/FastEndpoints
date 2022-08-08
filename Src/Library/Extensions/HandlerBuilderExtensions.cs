﻿using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;

namespace FastEndpoints;

internal static class HandlerBuilderExtensions
{
    public static RouteHandlerBuilder ProducesProblemFE(this RouteHandlerBuilder hb, int statusCode = 400, string contentType = "application/problem+json")
        => hb.ProducesProblemFE<ErrorResponse>(statusCode, contentType);

    public static RouteHandlerBuilder ProducesProblemFE<TResponse>(this RouteHandlerBuilder hb, int statusCode = 400, string contentType = "application/problem+json")
        => hb.Produces<TResponse>(statusCode, contentType);
}
