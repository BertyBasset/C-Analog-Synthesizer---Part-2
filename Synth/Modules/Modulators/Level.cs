namespace Synth.Modules.Modulators;

// Simple fixed Level modulator
public class Level : iModule {
    #region iModule Members
    public float Value { get; set; }

    public void Tick(float TimeIncrement) {
    }
    #endregion
}
