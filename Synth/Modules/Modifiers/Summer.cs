namespace Synth.Modules.Modifiers;

public class Summer :iModule {
    #region Public Properties
    public List<iModule> Sources { get; set; } = new();
    #endregion

    #region iModule Members
    public float Value { get; internal set; }

    public void Tick(float TimeIncrement) {
        Value = 0;
        for (int i = 0; i < Sources.Count; i++)
            Value += Sources[i].Value;
    }
    #endregion
}
