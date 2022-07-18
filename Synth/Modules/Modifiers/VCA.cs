using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Synth.Modules.Modifiers;

public class VCA : iModule {

    public float Value { get; internal set; }

    public void Tick(float TimeIncrement) {
        if (Source == null)
            Value = 0f;
        else {
            if (Modulator == null)
                Value = Source.Value;
            else
                Value = Source.Value * Modulator.Value;
        }
    }

    public iModule? Source;
    public iModule? Modulator;

}
