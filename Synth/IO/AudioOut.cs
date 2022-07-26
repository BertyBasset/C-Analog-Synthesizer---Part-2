using Synth.Modules;

namespace Synth.IO;

public class AudioOut : iModule {
    #region Public Properties
    public iModule? Source { get; set; }
    #endregion

    #region iModule Members
    public float Value { get; internal set; } = 0;

    public void Tick(float TimeIncrement) {
        Value = Source == null ? 0 : Source.Value;
    }
    #endregion
}
