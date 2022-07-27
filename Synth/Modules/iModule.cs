using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Synth.Modules;
public interface iModule {
    // Each module needs to provide a Value - nominally between -1 and +1
    public float Value { get; }
    // When Tick occurs, each module must update its Value to reflect what i will be in TimeIncrement
    internal void Tick(float TimeIncrement);
}

