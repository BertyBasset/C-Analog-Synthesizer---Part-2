using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Synth.Modules.Modulators;

// Simple fixed Level modulator
public class Level : iModule {

    public float Value { get; set; }

    public void Tick(float TimeIncrement) {
    }
}
