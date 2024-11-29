using System.Text.RegularExpressions;
using WebApplication1.models;
using WebApplication1.models.message;

namespace WebApplication1.Services;

public static class MessageValidator
{
    internal static async ValueTask<object?> ValidateEmptyId(
        EndpointFilterInvocationContext context,
        EndpointFilterDelegate next
    )
    {
        var messageDto = context.GetArgument<MessageDto>(0);
        if (!string.IsNullOrEmpty(messageDto.Id))
        {
            return Results.BadRequest(new Error("incorrect type"));
        }

        return await next(context);
    }

    internal static async ValueTask<object?> ValidateEmail(
        EndpointFilterInvocationContext context,
        EndpointFilterDelegate next)
    {
        var messageDto = context.GetArgument<MessageDto>(0);

        if (!Regex.IsMatch(messageDto.Email, @"[A-Z0-9._%+-]+@[A-Z0-9.-]+\.[A-Z]{2,4}", RegexOptions.IgnoreCase))
        {
            return Results.BadRequest(new Error("incorrect format"));
        }

        return await next(context);
    }
}