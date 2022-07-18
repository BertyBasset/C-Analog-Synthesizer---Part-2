using Synth.Modules;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Synth.IO;

public class AudioOut : iModule {
    public float Value { get; internal set; } = 0;

    public void Tick(float TimeIncrement) {
        Value = Source == null ? 0 : Source.Value;
    }

    public iModule? Source;
}
