using Synth.Modules.Modulators;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Synth.Modules.Modifiers;

public class Mixer : iModule {
    public float[] Level = new float[10];

    public float Value { get; internal set; }

    public void Tick(float TimeIncrement) {
        Value = 0;
        for (int i = 0; i < Sources.Count; i++)
            Value += Sources[i].Value * Level[i];
    }

    public List<iModule> Sources = new List<iModule>();
}

