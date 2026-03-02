namespace HybridAgent.Tools;

public abstract class Tool<TArgs> : ITool
    where TArgs : class
{
    public abstract string Name { get; }

    public abstract object GetSchema();

    public Type ArgumentsType => typeof(TArgs);

    public async Task<IToolResult> ExecuteFromJsonAsync(object args)
    {
        if (args is not TArgs typedArgs)
            throw new InvalidOperationException("Invalid argument type.");

        return await ExecuteAsync(typedArgs);
    }

    protected abstract Task<IToolResult> ExecuteAsync(TArgs args);
}