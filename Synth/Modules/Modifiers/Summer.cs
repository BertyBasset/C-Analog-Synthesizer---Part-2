using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Synth.Modules.Modifiers;

public class Summer :iModule {

    // This is an enumarable but with a CollectionChanged event so we can resize Levels list when new Sources are added
    public List<iModule> Sources = new List<iModule>();

    public float Value { get; internal set; }

    public void Tick(float TimeIncrement) {
        Value = 0;
        for (int i = 0; i < Sources.Count; i++)
            Value += Sources[i].Value;
    }
}
