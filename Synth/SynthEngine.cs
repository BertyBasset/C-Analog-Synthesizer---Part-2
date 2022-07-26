using NAudio.Wave;
using Synth.Modules;
using Synth.Modules.Modulators;
using Synth.IO;

namespace Synth;

// Reworking of Synth Engine using Phase Accumulators for shaping oscillator waveform
// Old technique of using Time Accumulator was leading to waveform discontinuities
// when changin frequecny.

public class SynthEngine : WaveProvider32 {
    #region Private Properties
    //private DirectSoundOut? asioOut;
    private WaveOut? asioOut;

    // These config settings are injected into constructor by client application
    int _SampleRate;
    int _Channels;
    #endregion

    #region 'Real Time' Graph
    // These allow any client app to display a 'real time' graph.
    // There will be a discontinuity at index=DisplayGraphCounter, so a helper method
    // is provided to return an array that wraps round if necessary going backwards
    // from index=DisplayGraphCounter
    private int _DisplayGraphCounter;
    private float[] _DisplayGraph = new float[1000];

    public float[] GetGraphData(int arraySize = 512) {
        // Array size maxes out at _DisplayGraph - 10 to keep away from the discontinuity which might have moved slightly
        // We've still got a bit of a glitch, but we can live wit it

        if (arraySize > _DisplayGraph.Length - 10)
            arraySize = _DisplayGraph.Length - 10;

        float[] rv = new float[arraySize];

        int src = _DisplayGraphCounter;

        for (int i = arraySize - 1; i >= 0; i--) {
            rv[i] = _DisplayGraph[src];
            src--;
            if (src < 0)
                src = _DisplayGraph.Length - 1;
        }

        return rv;
    }
    #endregion

    #region Stop/Start
    public void Start() {
        // Maybe this needs to be in config


        SetWaveFormat(_SampleRate, _Channels);                   // 16kHz stereo

        asioOut = new();

        asioOut.DesiredLatency = 100;
        asioOut.NumberOfBuffers = 3;

        asioOut.Init(this);
        asioOut.Play();
        Started = true;
    }

    public void Stop() {
        if (asioOut != null) {
            asioOut.Stop();
            asioOut.Dispose();
            asioOut = null;
            Started = false;
        }
    }
    #endregion

    #region Public Synth Properties
    public bool Started;            // Is synth running or not
    public float Volume { get; set; } = .25f;
    public List<iModule> Modules = new();


    public ModWheel ModWheel { get; set; } = new();


    // Lemons - Review this
    private int _PitchWheel;
    public int PitchWheel {
        get => _PitchWheel;
        set {
            _PitchWheel = value;
            //foreach (var o in Oscillators)
            //  o.Frequency.PitchWheel = _PitchWheel;
        }
    }




    



    #endregion

    #region Constructor
    public SynthEngine(Config config, float volume = 0.25f) {
        _SampleRate = config.SampleRate;
        _Channels = config.Channels;
        Volume = volume;
    }
    #endregion

    #region Sound Generation loop
    // Looks like this is a callback function which gets called when NAudio needs more wave data
    public override int Read(float[] buffer, int offset, int sampleCount) {
        float timeIncrement = 1f / (float)_SampleRate;
        for (int n = 0; n < sampleCount; n++) {

            foreach (var m in Modules)
                m.Tick(timeIncrement);

            // Aggregate outputs from all AudioOut modules
            float wave = 0;
            foreach (var m in Modules.Where(m => m.GetType() == typeof(AudioOut)))
                wave += m.Value;


            // Housekeeping - set final sample value with overall Volume
            float currentSample = (Volume * wave);
            buffer[n + offset] = currentSample;

            populateGraphArray(n, buffer[n + offset]);
        }
        return sampleCount;
    }
    #endregion

    #region Private Methods
    private void populateGraphArray(int n, float value) {
        // Populate array for displaying waveform
        if (n % 2 == 0) {
            //_DisplayGraph[_DisplayGraphCounter] = currentSample;
            _DisplayGraph[_DisplayGraphCounter] = value;
            _DisplayGraphCounter++;
            if(_DisplayGraphCounter >= _DisplayGraph.Length - 1)
                _DisplayGraphCounter =0;
        }
    }
    #endregion
}
