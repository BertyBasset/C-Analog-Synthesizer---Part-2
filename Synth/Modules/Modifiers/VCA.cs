namespace Synth.Modules.Modifiers;

public class VCA : iModule {
    #region Public Properties
    public iModule? Source { get; set; }
    public iModule? Modulator { get; set; }
    #endregion

    #region iModule Members
    public float Value { get; internal set; }

    public void Tick(float TimeIncrement) {
        if (Source == null)
            Value = 0f;
        else {
            if (Modulator == null)
                Value = Source.Value;
            else
                Value = Source.Value * Modulator.Value;
        }
    }
    #endregion
}