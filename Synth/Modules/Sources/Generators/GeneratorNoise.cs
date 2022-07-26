namespace Synth.Modules.Sources.Generators;

internal class GeneratorNoise : iGenerator {
    #region Private Properties
    const float AMPLITUDE_NORMALISATION = .5f;
    Random r = new Random();
    #endregion

    #region iGenerator Members
    //                                                             Not Used             not used
    float iGenerator.GenerateSample(float Phase, float Duty, float PhaseIncrement, bool IsZeroCrossing) {
        float sample = (float)(r.NextDouble() * 2.0 - 1.0);
        return sample * AMPLITUDE_NORMALISATION;
    }

    void iGenerator.Sync() {
        // Don't have Phase Accumulator(s), so do nothing
    }
    #endregion
}
