using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Synth.Modules;
public interface iModule {

    // All a modulator needs to do is to provide a value between -1 and +1
    public float Value { get; }
    internal void Tick(float TimeIncrement);

}

