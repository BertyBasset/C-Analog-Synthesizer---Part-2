namespace Synth.Modules.Modifiers;

public class VCF1Pole : iModule {
    // 1 Pole LPF. Not intended for final use, but as building block
    // for multipole VCFs

    #region Private Props
    private float prevOut = 0f;
    private float prevIn = 0f;
    #endregion

    #region Constructor
    public float CutoffFrequency { get; set; }
    #endregion

    #region Public Properties
    public iModule? Source { get; set; }

    public iModule? Modulator { get; set; }
    public float ModulatorAmount { get; set; }
    public iModule? Modulator2 { get; set; }
    public float ModulatorAmount2 { get; set; }
    #endregion

    #region iModule Members
    public float Value { get; internal set; }
    
    public void Tick(float TimeIncrement) {
        if (sampleRate == 0)
            sampleRate = (int)(1 / TimeIncrement);

        if (Source == null)
            Value = 0f;
        else {
            //Lowpass
            Value = a * Source.Value + (1 - a) * prevOut;

            //Highpass
            //Value = RC / (RC + dt) * (prevOut + Source.Value - prevIn);

            prevOut = Value;
            prevIn = Source.Value;
        }
    }
    #endregion

    #region Private Methods
    int sampleRate = 0;
    // Simualate Resistor Capacitor network for 1 poles simulation
    // float fc => MathF.Pow(2, 4 * ModulatorAmount * (Modulator?.Value ?? 0) + 7 + 5 * CutoffFrequency);
    float fc => MathF.Pow(2, 4 * TotalModulation() + 7 + 5 * CutoffFrequency);
    //float fc => 200;
    float RC => 1f / (2 * MathF.PI * fc);
    float dt => 1f / sampleRate;
    float a => dt / (RC + dt);

    // Get total mod from 2 modulators
    private float TotalModulation() {
        float mod = 0;
        mod += (Modulator?.Value ?? 0) * ModulatorAmount;
        mod += (Modulator2?.Value ?? 0) * ModulatorAmount2;
        return mod;
    }
    #endregion
}
