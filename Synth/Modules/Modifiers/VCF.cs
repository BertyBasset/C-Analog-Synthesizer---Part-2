using Synth.IO;

namespace Synth.Modules.Modifiers;

public class VCF : iModule {
    #region Private Properties
    List<VCF1Pole> poles { get; set; }
    #endregion

    #region Constructor
    public VCF(int numPoles = 8) {
        poles = new();

        for (int i = 0; i < numPoles; i++)
            poles.Add(new VCF1Pole());

        for (int i = 1; i < poles.Count; i++)
            poles[i].Source = poles[i - 1];
    }
    #endregion

    #region Public Properties
    public Keyboard? Keyboard { get; set; }     // Used for keyboard tracking

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
    #endregion

    #region iModule Members
    public float Value => poles[poles.Count - 1].Value;

    public void Tick(float TimeIncrement) {
        foreach (var pole in poles)
            pole.Tick(TimeIncrement);
    }
    #endregion
}
