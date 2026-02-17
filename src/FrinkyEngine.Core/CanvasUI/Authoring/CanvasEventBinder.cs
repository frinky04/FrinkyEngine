using System.Linq.Expressions;
using System.Reflection;
using FrinkyEngine.Core.Rendering;

namespace FrinkyEngine.Core.CanvasUI.Authoring;

internal static class CanvasEventBinder
{
    private static readonly HashSet<string> WarnedMissingHandlers = new(StringComparer.OrdinalIgnoreCase);
    private static readonly Dictionary<(Type ContextType, string MethodName), MethodInfo?[]> HandlerCache = new();

    public static bool TryBind(Panel panel, EventInfo eventInfo, string methodName)
    {
        var handlerType = eventInfo.EventHandlerType;
        if (handlerType == null)
            return false;

        var invoke = handlerType.GetMethod("Invoke");
        if (invoke == null || invoke.ReturnType != typeof(void))
            return false;

        var parameters = invoke.GetParameters();
        if (parameters.Length > 1)
            return false;

        var panelConst = Expression.Constant(panel);
        var methodConst = Expression.Constant(methodName);

        ParameterExpression[] lambdaParams;
        Expression argExpr;
        if (parameters.Length == 0)
        {
            lambdaParams = Array.Empty<ParameterExpression>();
            argExpr = Expression.Constant(null, typeof(object));
        }
        else
        {
            var p = Expression.Parameter(parameters[0].ParameterType, "arg0");
            lambdaParams = new[] { p };
            argExpr = Expression.Convert(p, typeof(object));
        }

        var dispatch = Expression.Call(
            typeof(CanvasEventBinder),
            nameof(DispatchEvent),
            Type.EmptyTypes,
            panelConst,
            methodConst,
            argExpr);

        var lambda = Expression.Lambda(handlerType, dispatch, lambdaParams);
        var del = lambda.Compile();
        eventInfo.AddEventHandler(panel, del);
        return true;
    }

    private static void DispatchEvent(Panel panel, string methodName, object? arg)
    {
        var context = panel.BindingContext;
        if (context == null)
            return;

        try
        {
            var method = ResolveHandler(context.GetType(), methodName, arg?.GetType());
            if (method == null)
            {
                WarnMissingHandler(context.GetType(), panel, methodName);
                return;
            }

            var parameters = method.GetParameters();
            if (parameters.Length == 0)
                method.Invoke(context, null);
            else
                method.Invoke(context, new[] { arg });
        }
        catch (Exception ex)
        {
            FrinkyLog.Warning($"CanvasUI event handler '{methodName}' failed: {ex.Message}");
        }
    }

    private static MethodInfo? ResolveHandler(Type contextType, string methodName, Type? argType)
    {
        // Cache stores [0] = parameterless overload, [1] = single-arg overload (or null)
        var key = (contextType, methodName);
        if (!HandlerCache.TryGetValue(key, out var cached))
        {
            var methods = contextType
                .GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
                .Where(m => string.Equals(m.Name, methodName, StringComparison.OrdinalIgnoreCase)
                            && !m.IsGenericMethod
                            && m.ReturnType == typeof(void))
                .ToList();

            cached = new MethodInfo?[2];
            cached[0] = methods.FirstOrDefault(m => m.GetParameters().Length == 0);
            cached[1] = methods.FirstOrDefault(m => m.GetParameters().Length == 1);
            HandlerCache[key] = cached;
        }

        if (argType != null && cached[1] != null)
        {
            var ps = cached[1]!.GetParameters();
            if (ps[0].ParameterType.IsAssignableFrom(argType))
                return cached[1];
        }

        return cached[0];
    }

    private static void WarnMissingHandler(Type contextType, Panel panel, string methodName)
    {
        string key = $"{contextType.FullName}|{panel.GetType().FullName}|{methodName}";
        if (!WarnedMissingHandlers.Add(key))
            return;
        FrinkyLog.Warning($"CanvasUI markup event handler not found: {contextType.Name}.{methodName} (panel {panel.GetType().Name})");
    }
}
