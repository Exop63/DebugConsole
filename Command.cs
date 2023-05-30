public class Command
{
    public string Name { get; set; }
    public string Description { get; set; }

    private DebugConsole _debugConsole = null;
    internal DebugConsole DebugConsole => _debugConsole;
    public string[] Args { get; set; }
    public Command(DebugConsole debugConsole)
    {
        _debugConsole = debugConsole;
    }
    public Command(string name, string description)
    {
        Name = name;
        Description = description;
    }

    public virtual void InvokeCmnd()
    {
        Debug.Log(Name + $" Command Arg: {string.Join(',', Args)}");
    }
}