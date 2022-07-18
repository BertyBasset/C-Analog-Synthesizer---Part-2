using Synth.Modules;
using Synth.Properties;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Synth.IO;


// Keyboard and Frequecny classes should be dealing with logarithmic digital
// equivalent of CV, however we've already gone down frequency route

public class Keyboard : iModule {
    public event EventHandler TriggerOn;
    public event EventHandler TriggerOff;



    // Used to implement Glide
    float _previousNoteFreq = 0;  // Previous Note       
    float _newNoteFreq;           // New Note 

    public Keyboard() {
        // init
        Value = Note.GetByDesc("C3").Frequency;     // Set default
        _previousNoteFreq = Value;
        _newNoteFreq = Value;
    }


    // This is primarilly for internal usage by synth modules
    public bool Gate { get; set; }

    // These two are write only as the 'real' keyboard manager will set them
    // They will then raise events that suscribers to the Keyboard can use
    // However, consider changing, esp if we go down C++ pico route
    public bool KeyUp { 
        set {
            Gate = false;
            TriggerOff?.Invoke(this, new EventArgs());
        }
    }
    public bool KeyDown {
        set {
            Gate = true;
            TriggerOn?.Invoke(this, new EventArgs());
        }
    }



    public float Glide { get;  set; } = 0;

    public float Value { get; internal set; } = 0;

    public void Tick(float interval) {
        // Shortcut, go straight to new note if glide is off
        if (Glide == 0) {
            Value = _newNoteFreq;
            return;
        }

        if (_previousNoteFreq != _newNoteFreq) {
            if (_previousNoteFreq < _newNoteFreq) {
                _previousNoteFreq += 1f/Glide;
                if (_previousNoteFreq > _newNoteFreq)       // Deals with overshoot !
                    _newNoteFreq = _previousNoteFreq;
            } else {
                _previousNoteFreq -= 1f/Glide;
                if (_previousNoteFreq < _newNoteFreq)
                    _newNoteFreq = _previousNoteFreq;       // Deals with undershoot !

            }
            Value = _previousNoteFreq;
        } else
            _previousNoteFreq = _newNoteFreq;
    }

    private Note _Note = new Note();
    public Note Note {
        get { return _Note; }
        set  {
            _Note = value;
            _newNoteFreq = _Note.Frequency;
        }
    }

}
