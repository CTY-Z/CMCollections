using CM.Core;

public class FSMBase : IRefPoolItem
{
    public int idx { get; set; }
    protected string fsmName;

    public virtual string FSMName { get { return fsmName; } private set { fsmName = value ?? string.Empty; } }

}
