using CMFramework.Core;

public class FSMBase : IPoolItem
{
    public int idx { get; set; }
    protected string fsmName;

    public virtual string FSMName { get { return fsmName; } private set { fsmName = value ?? string.Empty; } }

}
