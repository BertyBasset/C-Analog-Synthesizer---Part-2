using Synth.Utils;

namespace Synth.Modules.Sources.Generators;

internal class GeneratorSaw : iGenerator {
    #region Private Properties
    // For waves, like Saw which sound louder than waves like Sine, try and get them all to sounds about as loud
    const float AMPLITUDE_NORMALISATION = .5f;
    float _newDuty;
    #endregion

    #region iGenerator Members
    //                                                             Not Used        Used
    float iGenerator.GenerateSample(float Phase, float Duty, float PhaseIncrement, bool IsZeroCrossing) {
        float phase = Phase;
        if (Duty != 0) {
            // If we've wrapped round 360 -> 0, we're safe to do Phase Distortion
            if (IsZeroCrossing)
                _newDuty = Duty;

            phase = PhaseDistortionTransferFunction.GetPhase(phase, _newDuty, this);
        }

        var sample = 1 - phase / 180f;
        return sample * AMPLITUDE_NORMALISATION;
    }

    void iGenerator.Sync() {
        // Don't have Phase Accumulator(s), so do nothing
    }
    #endregion
}
