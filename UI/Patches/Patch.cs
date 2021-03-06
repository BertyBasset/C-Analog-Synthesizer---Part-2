using Synth.Modules.Sources;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UI {
    internal class Patch {
        private const string PATCH_PATH = "Patches\\patches.json";

        public bool IsInit { get; set; }
        public string PatchName { get; set; } = "";
        public List<Control> Controls { get; set; } = new List<Control>();  

        public class Control {
            public string ControlName { get; set; } = "";

            // Depending on control type, one of the following 3 populated/
            // We'll use reflection to dynamically set/get properties of controls

            public int? Value { get; set; }
            public int? SelectedIndex { get; set; }
            public bool? Checked { get; set; }
        }

        public string? WaveTableName { get; set; }
        public float[] FourierCoefficients { get; set; } = new float[0];
        public string? WaveTableName1 { get; set; }
        public float[] FourierCoefficients1 { get; set; } = new float[0];
        public string? WaveTableName2 { get; set; }
        public float[] FourierCoefficients2 { get; set; } = new float[0];

        public override string ToString() {
            return PatchName;
        }

        internal static List<Patch> GetPatchList(bool AddBlank = false) {
            var patches = Utils.Json<Patch>.GetListFromFile(PATCH_PATH);
            if (AddBlank)
                patches.Insert(0, new Patch());

            patches = patches.OrderBy(p => p.PatchName).ToList();
            return patches;
        }

        // Need to make sure when saving patches that Name is unique
        internal static Patch? GetPatchByName(string PatchName) {
            var patches = GetPatchList();

            //  This might return NULL, so client will need to check
            return patches.Where(p => p.PatchName.Trim().ToLower() == PatchName.Trim().ToLower()).First();
        }

        internal static Patch GetInitPatch() {
            var patches = GetPatchList();
            return patches.Where(p => p.IsInit).ToList().First();            // Should only be 1, so this should be ok
        }

        internal static void RecallPatch(Form Form, Patch patch, Oscillator o1, Oscillator o2, Oscillator o3) {
            foreach (var c in patch.Controls) {
                var control = Form.Controls.Find(c.ControlName, true)[0];

                switch (control.GetType().Name) {
                    case "TrackBar":
                        Debug.Assert(c.Value != null);
                        ((TrackBar)control).Value = (int)c.Value;
                        break;
                    case "ComboBox":
                        Debug.Assert(c.SelectedIndex != null);
                        ((ComboBox)control).SelectedIndex = (int)c.SelectedIndex;
                        break;
                    case "CheckBox":
                        Debug.Assert(c.Checked != null);
                        ((CheckBox)control).Checked = (bool)c.Checked;
                        break;
                    default:
                        break;

                }

            }

            o1.WaveTableFileName = patch.WaveTableName ?? "";
            o2.WaveTableFileName = patch.WaveTableName1 ?? "";
            o3.WaveTableFileName = patch.WaveTableName2 ?? "";
            o1.FourierCoefficients = patch.FourierCoefficients;
            o2.FourierCoefficients = patch.FourierCoefficients1;
            o3.FourierCoefficients = patch.FourierCoefficients2;
        }

        // Need to pass Fourier arrays etc in as well. 
        // Return true if sucesfull saved
        internal static Patch? SaveNewPatch(Form Form, Oscillator o1, Oscillator o2, Oscillator o3) {
            // Get Name
            var patchName = TextInputBox.Show("Save Patch", "Patch Name:");
            if (patchName.Trim() == "")
                return null;

            // Check Name does not exist
            var patches = GetPatchList();

            if (patches.Where(p => p.PatchName.Trim().ToLower() == patchName.Trim().ToLower()).Count() > 0) {
                MessageBox.Show($"A Patch with name '{patchName}' already exists.", "Save Patch", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                return null;
            }

            // Do away with Init Patch,
            //just save the three types of controls - can add more types
            
            Patch newPatch = new ();
            newPatch.IsInit = false;
            newPatch.PatchName = patchName;

            foreach (var c in GetAllControls(Form)) {
                if (c.Name.ToLower() == "ddlpatches" || c.Name.ToLower() == "ddlnote")
                    continue;

                var ctl = new Control();
                ctl.ControlName = c.Name;
                

                switch (c.GetType().Name) {
                    case "TrackBar":
                        ctl.Value = ((TrackBar)c).Value;
                        break;
                    case "ComboBox":
                        ctl.SelectedIndex = ((ComboBox)c).SelectedIndex;
                        break;
                    case "CheckBox":
                        ctl.Checked = ((CheckBox)c).Checked;
                        break;
                    default:
                        break;
                }
                newPatch.Controls.Add(ctl);

            }

            // Get Fourier etc from Oscillators passed in SynthEngine, they aren't stored in Controls on the form
            newPatch.WaveTableName = o1.WaveTableFileName;
            newPatch.WaveTableName1 = o2.WaveTableFileName; ;
            newPatch.WaveTableName2 = o3.WaveTableFileName; ;
            newPatch.FourierCoefficients = o1.FourierCoefficients;
            newPatch.FourierCoefficients1 = o2.FourierCoefficients;
            newPatch.FourierCoefficients2 = o3.FourierCoefficients;

            patches.Add(newPatch);
            Save(patches);
            return newPatch;
        }

        private static IEnumerable<System.Windows.Forms.Control> GetAllControls(System.Windows.Forms.Control container) {
            List<System.Windows.Forms.Control> controlList = new();
            foreach (System.Windows.Forms.Control c in container.Controls) {
                controlList.AddRange(GetAllControls(c));
                if (c is TrackBar || c is CheckBox || c is ComboBox || c is TextBox)
                    controlList.Add(c);
            }
            return controlList;
        }

        internal static bool DeletePatch(string PatchName) {
            // Check Name exists
            var patches = GetPatchList();
            if (patches.Where(p => p.PatchName.Trim().ToLower() == PatchName.Trim().ToLower()).Count() == 0) {
                MessageBox.Show($"Patch with name '{PatchName}' does not exist.");
                return false;
            }

            if (MessageBox.Show($"Confirm? Delete Patch '{PatchName}' ?", "Delete Patch", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.No)
                return false;

            patches = patches.Where(p => p.PatchName.Trim().ToLower() != PatchName.Trim().ToLower()).ToList();

            Save(patches);
            return true;
        }

        private static void Save(List<Patch> patches) {
            Utils.Json<Patch>.SaveListToFile(PATCH_PATH, patches);
        }
    }
}
