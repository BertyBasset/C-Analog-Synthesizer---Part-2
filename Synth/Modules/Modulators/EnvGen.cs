﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Synth.Modules.Modulators;

internal class EnvGen : iModule {
    public float Value {get; set;}

    public void Tick(float TimeIncrement) {
        throw new NotImplementedException();
    }
}
