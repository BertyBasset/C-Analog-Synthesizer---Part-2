namespace Synth.Modules.Modulators;

public class ModWheel : iModule {

    #region iModule Members
    private float _value;
    public float Value {
        get { return _value; }
        set { _value = value / 127f; }
    }

    public void Tick(float TimeIncrement) {
        // Does not need to do anything
    }
    #endregion
}
