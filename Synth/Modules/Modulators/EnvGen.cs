using Synth.IO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Synth.Modules.Modulators;

public class EnvGen : iModule {
    public event EventHandler? StageChanged;

    private Stage _currentStage = Stage.Release;
    public Stage CurrentStage {
        get { return _currentStage; }
        set {
            _currentStage = value;
            StageChanged?.Invoke(this, new EventArgs());
        }
    }

    public enum Stage {
        Attack,
        Decay,
        Sustain,
        Release
    }


    #region ADSR properties
    private float _Attack = .05f;
    public float Attack {
        get { return _Attack; }
        set {
            _Attack = Utils.Misc.Constrain<float>(value, 0.05f, 10f);     // 0.05 to 10 seconds  (not 0, as we will get glitches esp on sine waves)
        }
    }

    private float _Decay = .05f;
    public float Decay {
        get { return _Decay; }
        set {
            _Decay = Utils.Misc.Constrain<float>(value, 0.05f, 10f);     // 0.05 to 10 seconds  (not 0, as we will get glitches esp on sine waves)
        }
    }

    private float _Sustain = 1f;
    public float Sustain {
        get { return _Sustain; }
        set {
            _Sustain = Utils.Misc.Constrain<float>(value, 0f, 1f);     // Sustain is 0 to nominal x1 out

        }
    }

    private float _Release = .05f;
    public float Release {
        get { return _Release; }
        set {
            _Release = Utils.Misc.Constrain<float>(value, 0.05f, 10f);     // 0.05 to 10 seconds  (not 0, as we will get glitches esp on sine waves)
        }
    }
    #endregion



    public float Value {get; set;}



    // This is the *-*-* MEATY *-*-* bit
    public void Tick(float TimeIncrement) {
        // Implement ADSR according to Trigger/Gate state
        float inc = 0f;

        switch (CurrentStage) {
            case Stage.Attack:
                inc = TimeIncrement/ _Attack;
                Value += inc;
                if (Value > 1f) {                       // >1, go to Decay
                    Value = 1f;
                    CurrentStage = Stage.Decay;
                }
                break;
            case Stage.Decay:                           // < Sustain level, go to Sustain
                inc = -TimeIncrement/ Decay;
                Value += inc;
                if (Value < _Sustain) {
                    Value = _Sustain;
                    CurrentStage = Stage.Sustain;
                }
                break;
            case Stage.Sustain:                         // It's Sustain!! Nice...
                Value = _Sustain;
                break;
            case Stage.Release:                         // Release down to 0
                inc = -TimeIncrement/ Release;
                Value += inc;
                if (Value < 0)
                    Value = 0f;
                break;
            default: break;
        }
    }

    #region Trigger Event handling
    private Keyboard? _Keyboard;
    public Keyboard Keyboard {
        set {
            _Keyboard = value;

            _Keyboard.TriggerOn += _Keyboard_TriggerOn;
            _Keyboard.TriggerOff += _Keyboard_TriggerOff;
        }
    }

    private void _Keyboard_TriggerOff(object? sender, EventArgs e) {
        CurrentStage = Stage.Release;
    }

    private void _Keyboard_TriggerOn(object? sender, EventArgs e) {
        CurrentStage = Stage.Attack;
    }
    #endregion
}
