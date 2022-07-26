namespace Synth.Modules.Sources.Generators;

internal class GeneratorHarmonic : iGenerator {
    #region Private Properties
    // This needs to be a double as with floats, the accumulators drift with respect to each other
    private double[] _PhaseAccumulators = new double[1];
    #endregion

    #region Public Poperties
    private float[] _FourierCoefficients = new float[1];      // Default to fundamental!
    public float[] FourierCoefficients {
        get { return _FourierCoefficients; }
        set {
            _FourierCoefficients = value;

            // Need to setup as many Phase Accumulators as there are elements in the Coefficients Array
            _PhaseAccumulators = new double[_FourierCoefficients.Length];
        }
    }
    #endregion

    #region iGenerator Members
    // Uses phaseIncrement to maintain it's own Phase Accumulators                 not used
    float iGenerator.GenerateSample(float Phase, float Duty, float PhaseIncrement, bool IsZeroCrossing) {
        // This will take an array of floats, each element correspondong to the amplitude of
        // succesive harmonics, element 0 being amplitude of the fundamental

        float sample = 0f;

        for (int i = 0; i < _PhaseAccumulators.Length; i++) {
            // Calculate each harmonic
            // Shortciruit so we don't have to calculate sin if the coefficient is zero
            sample += (float)Math.Sin(_PhaseAccumulators[i] * Math.PI / 180f) * _FourierCoefficients[i];

            // Increment Phase Accumulators
            _PhaseAccumulators[i] += PhaseIncrement * (i + 1) % 360;      // [0] fundamental, [1..n] subsequent overtones
        }

        return sample;
    }

    void iGenerator.Sync() {
        for (int i = 0; i < _PhaseAccumulators.Length; i++)
            _PhaseAccumulators[i] = 0f;
    }
    #endregion
}

