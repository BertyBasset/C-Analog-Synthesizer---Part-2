using Synth.Modules.Modulators;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.ObjectModel;

namespace Synth.Modules.Modifiers;

public class Mixer : iModule {


    // This is an enumarable but with a CollectionChanged event so we can resize Levels list when new Sources are added
    public ObservableCollection<iModule> Sources = new ObservableCollection<iModule>();
    public Mixer() {
        Sources.CollectionChanged += (o, e) => SourcesChanged();
    }

    private void SourcesChanged() {
        // Resize Levels list to match number of items in Source
        if (Sources.Count > Levels.Count) {
            // Make bigger
            for (int i = Levels.Count; i < Sources.Count; i++)
                Levels.Insert(i, 0f);
        }

        if (Sources.Count < Levels.Count) {
            throw new InvalidOperationException("Removal of modules is not allowed");

        }

    }


    public List<float> Levels = new List<float>();

    public float Value { get; internal set; }

    public void Tick(float TimeIncrement) {
        Value = 0;
        for (int i = 0; i < Sources.Count; i++)
            Value += Sources[i].Value * Levels[i];
    }




}

