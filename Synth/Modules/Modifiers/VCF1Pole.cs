namespace Synth.Modules.Modifiers;

public class VCF1Pole : iModule {

    // 1 Pole LPF. Not intended for final use, but as building block
    // for multipole VCFs

    public float Value { get; internal set; }
    public float CutoffFrequency { get; set; }


    private float prevOut = 0f;
    private float prevIn = 0f;
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

    int sampleRate = 0;
    float fc => MathF.Pow(2, 4 * ModulatorAmount * (Modulator?.Value ?? 0) + 7 + 5 * CutoffFrequency);
    //float fc => 200;
    float RC => 1f / (2 * MathF.PI * fc);
    float dt => 1f / sampleRate;
    float a => dt / (RC + dt);

    public iModule? Source;
    public iModule? Modulator;

    public float ModulatorAmount { get; set; }

}
