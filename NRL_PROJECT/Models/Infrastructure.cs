using Microsoft.AspNetCore.Mvc.ModelBinding;
using System.Globalization;

namespace NRL_PROJECT.Infrastructure;

/// <summary>
/// Model binder that accepts both '.' and ',' as decimal separators for double/double? values.
/// Useful when users enter numbers with local separators.
/// </summary>
public sealed class InvariantDoubleModelBinder : IModelBinder
{
    public Task BindModelAsync(ModelBindingContext ctx)
    {
        var vr = ctx.ValueProvider.GetValue(ctx.ModelName);
        if (vr == ValueProviderResult.None) return Task.CompletedTask;

        var value = vr.FirstValue;
        if (string.IsNullOrWhiteSpace(value)) return Task.CompletedTask;

        // Accept both invariant (.) and current culture (which may use ,)
        if (double.TryParse(value, NumberStyles.Float | NumberStyles.AllowThousands, CultureInfo.InvariantCulture, out var d) ||
            double.TryParse(value, NumberStyles.Float | NumberStyles.AllowThousands, CultureInfo.CurrentCulture, out d))
        {
            ctx.Result = ModelBindingResult.Success(d);
            return Task.CompletedTask;
        }

        ctx.ModelState.TryAddModelError(ctx.ModelName, $"The value '{value}' is not valid for {ctx.ModelName}.");
        return Task.CompletedTask;
    }
}

/// <summary>
/// Provider for <see cref="InvariantDoubleModelBinder"/>. Returns the binder for double and double? model types.
/// Register this provider early in ModelBinderProviders so it takes precedence where needed.
/// </summary>
public sealed class InvariantDoubleModelBinderProvider : IModelBinderProvider
{
    public IModelBinder? GetBinder(ModelBinderProviderContext context)
    {
        if (context.Metadata.ModelType == typeof(double) || context.Metadata.ModelType == typeof(double?))
            return new InvariantDoubleModelBinder();
        return null;
    }
}
