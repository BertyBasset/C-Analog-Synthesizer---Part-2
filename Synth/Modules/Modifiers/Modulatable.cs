/*

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Synth.Modules.Modulators;
// Maintains array of modulators, modulator levels, and modulator scaling
public class Modulators {
    public List<Modulator> modulators = new List<Modulator>();

    public void AddModulator(Modulator Modulator) {
        modulators.Add(Modulator);
    }

    public void RemoveModulatorAt(int index) {
        modulators.RemoveAt(index);
    }
    public void RemoveAllModulators() {
        modulators.Clear();
    }

    public void RemoveModulator(Modulator Modulator) {
        modulators.Remove(Modulator);
    }

    public float Value {
        get {
            // Calculate current modulation value for each modulator
            float value = 0;
            foreach (var m in modulators)
                value += m.Value;

            return value;
        }
    }
}

public class Modulator : iModule {
    public string Name { get; set; } = "";
    public iModule? ModulatorSource { get; set; }
    public float ModulatorAmount { get; set; }      // Nominally -1 to +1 or 0 to 1
    public float ModScaling { get; set; } = 1f;     // Default scaling to 1
                                                    //    Maybe have Offset/Bias property, though not sure it it's to be applied before or after scaling

    // iModule members
    public float Value { get; internal set; }

    public virtual void Tick(float TimeIncrement) {
        // Update Value
        if (ModulatorSource == null)
            Value = 0f;
        else
            Value = ModulatorSource.Value * ModulatorAmount * ModScaling;
    }
}

public class ModulatableModulator : Modulator {
    public iModule? ModulatorModulatorSource { get; set; }
    public float ModulatorModulatorAmount { get; set; }
    public float ModulatorModulatorAmountScaling { get; set; } = 1f;

    public override void Tick(float TimeIncrement) {
        // Update Value
        float value = 0f;
        if (ModulatorModulatorSource != null) {
            value = ModulatorModulatorSource.Value * ModulatorModulatorAmount * ModulatorModulatorAmountScaling;
        }

        base.Tick(TimeIncrement);

        base.Value = base.Value * value;
    }
} 


*/