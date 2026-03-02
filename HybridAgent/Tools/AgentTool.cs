namespace HybridAgent.Tools;

public abstract class AgentTool<TArgs> : IAgentTool
    where TArgs : class
{
    public abstract string Name { get; }

    public abstract object GetSchema();

    public Type ArgumentsType => typeof(TArgs);

    public async Task<string> ExecuteUntypedAsync(object args)
    {
        if (args is not TArgs typedArgs)
            throw new InvalidOperationException("Invalid argument type.");

        return await ExecuteAsync(typedArgs);
    }

    protected abstract Task<string> ExecuteAsync(TArgs args);
}