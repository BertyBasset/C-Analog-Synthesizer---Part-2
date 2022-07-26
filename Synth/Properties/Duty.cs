using Synth.Modules;

namespace Synth.Properties;
public class Duty {

    #region Public Properties
    public float Value { get; set; }

    public iModule? Modulator{ get; set; }
    
    private float _ModulationAmount;
    public float ModulationAmount {         
        get { return _ModulationAmount; }
        set {
            _ModulationAmount = Utils.Misc.Constrain(value, -1f, 1f);           // not sure what this should be at the moment
        }
    }
    #endregion

    #region Public Methods
    public float GetDuty() {
        float value = Value;
        if(Modulator != null)    
            value = value + Modulator.Value * _ModulationAmount;

        return Utils.Misc.Constrain(value, -0.98f, 0.98f);
    }
    #endregion
}
