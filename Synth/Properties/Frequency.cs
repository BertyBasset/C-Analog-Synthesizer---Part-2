using Synth.IO;
using Synth.Modules;
using Synth.Modules.Modulators;

namespace Synth.Properties;
public class Frequency {
    // Keyboard and Frequency classes should be dealing with logarithmic digital
    // equivalent of CV, however we've already gone down frequency route
    // However, consider changing, esp if we go down C++ pico route

    #region Private Properties etc.
    private const float DEFAULT_FREQUENCY = 110f;
    #endregion

    #region Public Properties
    // Keyboard Control
    // If false, Note property has no effect on frequecny
    private bool _Kbd = true;
    public bool Kbd {
        get { return _Kbd; }
        set { 
            _Kbd = value;
            //setFrequency();
        }
    }


    // Frequency is now a derived property, driven by the following
    public float PreModFrequency { get; internal set; }        // 20 to 10,000  - This is pre modulation

    private int _Octave = 0;
    public int Octave {                                        // -3 to +3 octaves
        get { return _Octave; }
        set {
            _Octave = Utils.Misc.Constrain(value, -3, 3);
            //setFrequency();
        }
    }

    private float _Tune = 0;
    public float Tune {                                        // -1 to +1 octave
        get { return _Tune; }
        set {
            _Tune = Utils.Misc.Constrain(value, -1f, 1f);
            //setFrequency();
        }
    }

    private float _FineTune = 0;
    public float FineTune {                                    // -1 to +1 octave, but normally will be +/- seimitone  +/- 12th root of 2
        get { return _FineTune; }
        set {
            _FineTune = Utils.Misc.Constrain(value, -1f, 1f);
            //setFrequency();
        }
    }


    private float _PitchWheel = 0;
    internal int PitchWheel {
        set {
            float semitone = (value - 8192f) / 4096 / 12;
            _PitchWheel = semitone * MathF.Pow(2, 1 / 12);
            //setFrequency();
        }
    }

    public Keyboard? Keyboard { get; set; }


    public iModule? Modulator { get; set; }

    private float _ModulationAmount;
    public float ModulationAmount {                            // 0 to 10000
        get { return _ModulationAmount; }
        set {
            _ModulationAmount = Utils.Misc.Constrain(value, 0f, 10000f);
        }   
    }


    public LFO? ModulatorLFO;

    private float _LFOAmount;
    public float LFOAmount {                            // 0 to 100
        get { return _LFOAmount; }
        set {
            _LFOAmount = Utils.Misc.Constrain(value /1000f, 0f, 100f);
        }
    }




    // We've got a modulator to modulate the modulation amount!!  
    public iModule? ModulationAmountModulator { get; set; }
    private float _ModulationAmountModulatorAmount;
    public float ModulationAmountModulatorAmount {                            // 0 to 10000
        get { return _ModulationAmountModulatorAmount; }
        set {
            _ModulationAmountModulatorAmount = Utils.Misc.Constrain(value, 0f, 10000f);
        }
    }
    #endregion

    #region Public Methods
    //  Modulation Frequency scaling is 1.0 per octave
    public float GetFrequency() {
        // NB     / 2 because of stereo interleaving
        // This is final frequency used for driving Phase Accumulator

        // Whenever one of the frequency controlling properties change, we update Pre Mod Frequency
        if (Keyboard == null)
            PreModFrequency = DEFAULT_FREQUENCY;                                  // Base Frequency
        else
            PreModFrequency = Keyboard.Value;

        PreModFrequency = PreModFrequency * MathF.Pow(2, _Octave);    // Adjust Octave
        // Both Tune and FineTune are 1 per octave, however they have separate values as they'll normally have separate UI controls
        PreModFrequency = PreModFrequency * MathF.Pow(2, _Tune);      // Tune within octave
        PreModFrequency = PreModFrequency * MathF.Pow(2, _FineTune);  // Tune within semitone
        var f = PreModFrequency / 2f;

        // <<-- ** Apply modulation here
        if (Modulator != null) {
            float modAmountMod = 0;
            if (ModulationAmountModulator != null)
                modAmountMod = ModulationAmountModulator.Value * 1000f * _ModulationAmountModulatorAmount;      // <<< Not sure what value to give a good effect


            f = f + (_ModulationAmount + modAmountMod) * Modulator.Value;           // Vibrato here << 
       }

        if (ModulatorLFO != null)
            //f = f + _LFOAmount * ModulatorLFO.Value;
            f *= 1 + (ModulatorLFO.Value * _LFOAmount / 10);


        return f;
    }
    #endregion

}

