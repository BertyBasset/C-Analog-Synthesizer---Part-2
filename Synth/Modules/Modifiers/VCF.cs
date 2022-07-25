using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Synth.Modules.Modifiers;

public class VCF : iModule {
    public float Value => poles[poles.Count - 1].Value;

    //List<VCF1Pole> poles = new() { new VCF1Pole(), new VCF1Pole(), new VCF1Pole(), new VCF1Pole() };
    List<VCF1Pole> poles;
    public VCF(int numPoles = 8)
    {
        poles = new();




        for (int i = 0; i < numPoles; i++)
            poles.Add(new VCF1Pole());

        for (int i = 1; i < poles.Count; i++)
            poles[i].Source = poles[i - 1];

        poles[0].QSource = poles[numPoles - 1];
    }

    public void Tick(float TimeIncrement) {
        foreach (var pole in poles)
            pole.Tick(TimeIncrement);
    }

    public iModule? Source {
        get => poles[0].Source;
        set => poles[0].Source = value;
    }

    public float CutoffFrequency {
        get => poles[0].CutoffFrequency;
        set {
            foreach(var pole in poles)
                pole.CutoffFrequency = value;
        }
    }

    public iModule? Modulator {
        get => poles[0].Modulator;
        set {
            foreach (var pole in poles)
                pole.Modulator = value;
        }
    }

    public float QAmount
    {
        get => poles[0].QAmount;
        set => poles[0].QAmount = value;
    }

    public float ModulatorAmount {
        get => poles[0].ModulatorAmount;
        set {
            foreach (var pole in poles)
                pole.ModulatorAmount = value;
        }
    }

    public iModule? Modulator2 {
        get => poles[0].Modulator2;
        set {
            foreach (var pole in poles)
                pole.Modulator2 = value;
        }
    }

    public float ModulatorAmount2 {
        get => poles[0].ModulatorAmount2;
        set {
            foreach (var pole in poles)
                pole.ModulatorAmount2 = value * 5f;         // Magic number to sound good !!
        }
    }
}
