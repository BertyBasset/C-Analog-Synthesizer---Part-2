namespace Synth.Modules.Sources {
    internal class GeneratorSine : iGenerator {
        //                                                             Not Used

        // To prevent clicks, only apply changed Duty on a zero crossing
        float _newDuty;
        //   
        float iGenerator.GenerateSample(float Phase, float Duty, float PhaseIncrement, bool IsZeroCrossing) {

            // Phase Distortion
            // For sine, instead of varying SQ duty cycle, we can do phase distortion a la Casio CZ100 !
            float phase = Phase;
            if (Duty != 0) {
                // If we've wrapped round 360 -> 0, we're safe to do Phase Distortion
                if (IsZeroCrossing)
                    _newDuty = Duty;
                    
                phase = PhaseDistortionTransferFunction.GetPhase(phase, _newDuty, this);

            }

            var sample = (float)Math.Sin(phase * Math.PI / 180f);

            return sample;
        }

        void iGenerator.Sync() {

            // Don't have Phase Accumulator(s), so do nothing
        }
    }
}
