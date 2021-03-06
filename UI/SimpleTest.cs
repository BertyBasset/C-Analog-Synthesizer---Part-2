using Microsoft.Extensions.Configuration;
using Synth;
using Synth.Modules.Sources;
using Synth.Properties;
using System.Diagnostics;
using FftSharp;
using Synth.Modules.Modifiers;
using Synth.IO;
using Synth.Modules.Modulators;


/* To Do:
 *  Remove Volumes from Oscillators                                      DONE
 *  Add n way mixer                                                      DONE
 *  Tidy PD - do more in Osc                                             DONE
 *  Wavetable/Harmonic Preview                                           DONE
 *  Re-add wavetable/harmonic/sawtooth info to pathc save/load           DONE
 *  Default Wavetable to first available wave - init array to [1024]     DONE
 *  Observable maybe to dynamically size Level property of Mixer ?       DONE
 *  Glitch  (2 phase Tick or triple buffer)                              DONE
 *  Glitches on harmonic                                                 Park
 *  Glide (add Tick to frequecny)                                        DONE
 *  VCA                                                                  DONE
 *  Modulate PWM                                                         DONE
 *  ADSRs                                                                DONE
 *  FM patch bug                                                         Park
 *  Wavtable freeze                                                      DONE
 *  FM Mod - level mod                                                   DONE
 *  Reverse Saw                                                          DONE
 *  Unhook oscs                                                          DONE
 *  LFO Delay                                                            DONE
 *  FM LFO                                                               DONE
 *  S+H                                                                  DONE
 *  LFOs                                                                 DONE
 *  Filter Envelope                                                      DONE
 *  Pacth Bug                                                            DONE
 *  Tidy Library                                                         DONE
 *  Write Part 4
 *  Write Part 5
 * ----------
 *  Version 3
 * ----------
 *  Throwaway UI - apart from keyboard
 *  Rationalise CV/non CV
 *  Modulator/Modulatable/Modulators/Modulator Modulator - tidy
 *  Filter Q Version 3
 *  Filter kbd Tracking                                                  - to do
 *  Stereo Support (mono and stero output connectors)
 *  Stereo Processor (LFO, EG etc)
 *  Package filter as 4 pole unit state var
 *  Analog Switch
 *  Conditioner.   Gain 0.1 to 10, shift -10 to +10
 *  Sequencer
 *  Phasers
 *  Reverb
 *  UI
 *  ----------
 *  Version 4
 *  ----------
 *  Pico !!
 * */


namespace UI;
public partial class SimpleTest : Form {
    #region Init Synth
    Synth.SynthEngine synth;

    List<Note> _NoteList = Note.GetNoteList();



    Oscillator osc1;
    Oscillator osc2;
    Oscillator osc3;
    Mixer mixerOsc;
    AudioOut audioOut;
    Synth.IO.Keyboard keyboard;

    EnvGen envGenVca;
    EnvGen envGenPD;
    EnvGen envGenFM;
    EnvGen envGenVcf;

    LFO lfoOscFreq;
    LFO lfoOscFM;
    LFO lfoVcfCutoff;

    Mixer mixerVcoFmAmount1;
    Mixer mixerVcoFmAmount2;
    Mixer mixerVcoFmAmount3;

    Mixer mixerVcoPdAmount1;
    Mixer mixerVcoPdAmount2;
    Mixer mixerVcoPdAmount3;

    VCF vcf;
    VCA vca;


    void InitSynth() {
        // Get Synth module config, and inject into constructor
        var builder = new ConfigurationBuilder().SetBasePath(Directory.GetCurrentDirectory()).AddJsonFile("appsettings.json", optional: false);
        IConfiguration config = builder.Build();
        var synthConfig = config.GetSection("Synth").Get<Synth.Config>();
        synth = new Synth.SynthEngine(synthConfig);

        // Interconnect Modules in code here

        osc1 = new Oscillator() { WaveForm = WaveForm.GetByType(WaveformType.Sine) };
        osc2 = new Oscillator() { WaveForm = WaveForm.GetByType(WaveformType.Sine) };
        osc3 = new Oscillator() { WaveForm = WaveForm.GetByType(WaveformType.Sine) };



        // Create mixer, and connect three oscillators to it, first one with Level up
        mixerOsc = new Mixer();
        mixerOsc.Sources.Add(osc1);
        mixerOsc.Sources.Add(osc2);
        mixerOsc.Sources.Add(osc3);
        mixerOsc.Levels[0] = 1;
        
        // Created keyboard and connect to oscillators - we can set to null to make keyboard independent
        keyboard = new Synth.IO.Keyboard();
        osc1.Frequency.Keyboard = keyboard;
        osc2.Frequency.Keyboard = keyboard;
        osc3.Frequency.Keyboard = keyboard;


        mixerVcoPdAmount1 = new Mixer();
        mixerVcoPdAmount2 = new Mixer();
        mixerVcoPdAmount3 = new Mixer();

        envGenPD = new EnvGen() { Keyboard = keyboard };
        mixerVcoPdAmount1.Sources.Add(envGenPD);
        mixerVcoPdAmount2.Sources.Add(envGenPD);
        mixerVcoPdAmount3.Sources.Add(envGenPD);

        osc1.Duty.Modulator = mixerVcoPdAmount1;
        osc1.Duty.ModulationAmount = 1f;
        osc2.Duty.Modulator = mixerVcoPdAmount2;
        osc2.Duty.ModulationAmount = 0f;
        osc3.Duty.Modulator = mixerVcoPdAmount3;
        osc3.Duty.ModulationAmount = 0f;


        // LFOs
        lfoOscFreq = new LFO() { Frequency = 0, WaveForm = WaveForm.GetByType(WaveformType.Sine), Keyboard = keyboard };        // Keyboard to add delay
        lfoOscFM = new LFO() { Frequency = 0, WaveForm = WaveForm.GetByType(WaveformType.Sine), Keyboard = keyboard };
        lfoVcfCutoff = new LFO() { Frequency = 0, WaveForm = WaveForm.GetByType(WaveformType.Sine), Keyboard = keyboard };

        osc1.Frequency.ModulatorLFO = lfoOscFreq;

        envGenFM = new EnvGen() { Keyboard = keyboard };

        mixerVcoFmAmount1  = new Mixer();
        mixerVcoFmAmount1.Sources.Add(envGenFM);
        mixerVcoFmAmount1.Sources.Add(lfoOscFM);

        mixerVcoFmAmount2 = new Mixer();
        mixerVcoFmAmount2.Sources.Add(envGenFM);
        mixerVcoFmAmount2.Sources.Add(lfoOscFM);

        mixerVcoFmAmount3 = new Mixer();
        mixerVcoFmAmount3.Sources.Add(envGenFM);
        mixerVcoFmAmount3.Sources.Add(lfoOscFM);

        osc1.Frequency.ModulationAmountModulator = mixerVcoFmAmount1;
        osc1.Frequency.ModulationAmountModulatorAmount = 1f;            // Set to max as the EG and LFO levels will be set on the mocer
        osc2.Frequency.ModulationAmountModulatorAmount = 1f;
        osc2.Frequency.ModulationAmountModulator = mixerVcoFmAmount2;
        osc3.Frequency.ModulationAmountModulatorAmount = 1f;
        osc3.Frequency.ModulationAmountModulator = mixerVcoFmAmount3;

        envGenVcf = new() { Keyboard = keyboard};
        vcf = new() {  CutoffFrequency = 1, Source = mixerOsc, Modulator = lfoVcfCutoff, Modulator2 = envGenVcf };
        
        envGenVca = new EnvGen() { Keyboard = keyboard };
        vca = new VCA() { Modulator = envGenVca, Source = vcf };

        // Hook mixer output to VCA
        audioOut = new AudioOut() { Source = vca };




        // Add ALL modules to synth, even if they are children of other modules as SynthEngine needs to enumarate ALL modules
        // Might be able to do away with this at some point
        synth.Modules.Add(osc1);
        synth.Modules.Add(osc2);
        synth.Modules.Add(osc3);
        synth.Modules.Add(mixerOsc);
        synth.Modules.Add(audioOut);
        synth.Modules.Add(keyboard);
        synth.Modules.Add(envGenVca);
        synth.Modules.Add(vca);
        synth.Modules.Add(envGenPD);
        synth.Modules.Add(envGenFM);
        synth.Modules.Add(lfoOscFreq);
        synth.Modules.Add(lfoOscFM);
        synth.Modules.Add(lfoVcfCutoff);
        synth.Modules.Add(mixerVcoFmAmount1);
        synth.Modules.Add(mixerVcoFmAmount2);
        synth.Modules.Add(mixerVcoFmAmount3);
        synth.Modules.Add(mixerVcoPdAmount1);
        synth.Modules.Add(mixerVcoPdAmount2);
        synth.Modules.Add(mixerVcoPdAmount3);
        synth.Modules.Add(vcf);
        synth.Modules.Add(envGenVcf);
        



        // Temporarfy hard coded mod wheel 
        //osc1.Duty.Modulator = synth.ModWheel;
        // osc2.Duty.Modulator = synth.ModWheel;
        //osc3.Duty.Modulator = synth.ModWheel;


    }

    #endregion

    #region Init Form
    public SimpleTest() {
        InitializeComponent();

        InitUI();
        AddEventHandlers();

        InitSynth();
        LoadPatchList();

        #region not null asserts
        Debug.Assert(synth != null);
        Debug.Assert(osc1 != null);
        Debug.Assert(osc2 != null);
        Debug.Assert(osc3 != null);
        Debug.Assert(mixerOsc != null);
        Debug.Assert(audioOut != null);
        Debug.Assert(keyboard != null);
        Debug.Assert(envGenPD != null);
        Debug.Assert(vca != null);
        Debug.Assert(envGenVca != null);
        Debug.Assert(envGenFM != null);
        #endregion



        SetToolTips();
        ddlSyncSource.SelectedIndex = 0;
        ddlSyncSource1.SelectedIndex = 0;
        ddlSyncSource2.SelectedIndex = 0;

        ddlModSource.SelectedIndex = 0;
        ddlModSource1.SelectedIndex = 0;
        ddlModSource2.SelectedIndex = 0;

        timDisplay.Enabled = true;
        virtualKeyboard.Focus();

        cmdStart.Enabled = false;
        cmdStop.Enabled = true;
        synth.Start();
    }

    private void SetToolTips() {
        toolTip1.SetToolTip(picSine, "Sine Wave");
        toolTip1.SetToolTip(picSaw, "Saw Wave");
        toolTip1.SetToolTip(picTriangle, "Triangle Wave");
        toolTip1.SetToolTip(picSquare, "Square Wave");
        toolTip1.SetToolTip(picNoise, "White Noise");
        toolTip1.SetToolTip(picWaveTable, "Wave Table");
        toolTip1.SetToolTip(picHarmonic, "Harmonics");
        toolTip1.SetToolTip(picSuperSaw, "Super Saw");
        toolTip1.SetToolTip(picSine1, "Sine Wave");
        toolTip1.SetToolTip(picSaw1, "Saw Wave");
        toolTip1.SetToolTip(picTriangle1, "Triangle Wave");
        toolTip1.SetToolTip(picSquare1, "Square Wave");
        toolTip1.SetToolTip(picNoise1, "White Noise");
        toolTip1.SetToolTip(picWaveTable1, "Wave Table");
        toolTip1.SetToolTip(picHarmonic1, "Harmonics");
        toolTip1.SetToolTip(picSuperSaw1, "Super Saw");
        toolTip1.SetToolTip(picSine2, "Sine Wave");
        toolTip1.SetToolTip(picSaw2, "Saw Wave");
        toolTip1.SetToolTip(picTriangle2, "Triangle Wave");
        toolTip1.SetToolTip(picSquare2, "Square Wave");
        toolTip1.SetToolTip(picNoise2, "White Noise");
        toolTip1.SetToolTip(picWaveTable2, "Wave Table");
        toolTip1.SetToolTip(picHarmonic2, "Harmonics");
        toolTip1.SetToolTip(picSuperSaw2, "Super Saw");
    }

    private void LoadPatchList() { 
        var patches = Patch.GetPatchList(true);
        ddlPatches.DataSource = patches;
    }


    private void InitUI() {
        ddlNote.DataSource = _NoteList;
        ddlNote.SelectedIndex = 24;     // Go to A2

            
        ddlSuperSaw.DataSource = Data.SuperSaw.GetSampleList(true);
        ddlSuperSaw1.DataSource = Data.SuperSaw.GetSampleList(true);
        ddlSuperSaw2.DataSource = Data.SuperSaw.GetSampleList(true);




    }

    private void AddEventHandlers() {
        ddlNote.SelectedIndexChanged += DdlNote_SelectedIndexChanged;

        sldWaveForm.ValueChanged += SldWaveForm_ValueChanged;
        sldOctave.ValueChanged += SldOctave_ValueChanged;
        sldTune.ValueChanged += SldTune_ValueChanged;
        sldFineTune.ValueChanged += SldFineTune_ValueChanged;
        cmdReset.Click += CmdReset_Click;
        sldPWM.ValueChanged += SldPWM_ValueChanged;
        sldLevel.ValueChanged += SldLevel_ValueChanged;
        ddlSuperSaw.SelectedIndexChanged += DdlSuperSaw_SelectedIndexChanged;
        ddlSyncSource.SelectedIndexChanged += DdlSyncSource_SelectedIndexChanged;
        sldModAmount.ValueChanged += SldModAmount_ValueChanged;
        ddlModSource.SelectedIndexChanged += DdlModSource_SelectedIndexChanged;
        chkKbd.CheckedChanged += ChkKbd_CheckedChanged;

        sldWaveForm1.ValueChanged += SldWaveForm1_ValueChanged;
        sldOctave1.ValueChanged += SldOctave1_ValueChanged;
        sldTune1.ValueChanged += SldTune1_ValueChanged;
        sldFineTune1.ValueChanged += SldFineTune1_ValueChanged;
        cmdReset1.Click += CmdReset1_Click;
        sldPWM1.ValueChanged += SldPWM1_ValueChanged;
        sldLevel1.ValueChanged += SldLevel1_ValueChanged;
        ddlSuperSaw1.SelectedIndexChanged += DdlSuperSaw1_SelectedIndexChanged;
        ddlSyncSource1.SelectedIndexChanged += DdlSyncSource1_SelectedIndexChanged;
        sldModAmount1.ValueChanged += SldModAmount1_ValueChanged;
        ddlModSource1.SelectedIndexChanged += DdlModSource1_SelectedIndexChanged;
        chkKbd1.CheckedChanged += ChkKbd1_CheckedChanged;

        sldWaveForm2.ValueChanged += SldWaveForm2_ValueChanged;
        sldOctave2.ValueChanged += SldOctave2_ValueChanged;
        sldTune2.ValueChanged += SldTune2_ValueChanged;
        sldFineTune2.ValueChanged += SldFineTune2_ValueChanged;
        cmdReset2.Click += CmdReset2_Click;

        sldPWM2.ValueChanged += SldPWM2_ValueChanged;
        sldLevel2.ValueChanged += SldLevel2_ValueChanged;
        ddlSuperSaw2.SelectedIndexChanged += DdlSuperSaw2_SelectedIndexChanged;
        ddlSyncSource2.SelectedIndexChanged += DdlSyncSource2_SelectedIndexChanged;
        sldModAmount2.ValueChanged += SldModAmount2_ValueChanged;
        ddlModSource2.SelectedIndexChanged += DdlModSource2_SelectedIndexChanged;
        chkKbd2.CheckedChanged += ChkKbd2_CheckedChanged;

        sldGlide.ValueChanged += (o, e) => keyboard.Glide = sldGlide.Value / 10f;


        // Refresh display every 2s
        timDisplay.Tick += TimDisplay_Tick;
        cmdPauseGraph.Click += CmdPauseGraph_Click;

        cmdSelectOscSetting.Click += CmdOsctSetting_Click;
        cmdSelectOscSetting1.Click += CmdOsctSetting1_Click;
        cmdSelectOscSetting2.Click += CmdOsctSetting2_Click;

        // VCA Env Gen
        sldAttack.ValueChanged += (o, e) => envGenVca.Attack = sldAttack.Value / 100f;
        sldDecay.ValueChanged += (o, e) => envGenVca.Decay = sldDecay.Value / 100f;
        sldSustain.ValueChanged += (o, e) => envGenVca.Sustain = sldSustain.Value / 1000f;
        sldRelease.ValueChanged += (o, e) => envGenVca.Release = sldRelease.Value / 100f;

        // PD Env Gen
        sldPDAttack.ValueChanged += (o, e) => envGenPD.Attack = sldPDAttack.Value / 100f;
        sldPDDecay.ValueChanged += (o, e) => envGenPD.Decay = sldPDDecay.Value / 100f;
        sldPDSustain.ValueChanged += (o, e) => envGenPD.Sustain = sldPDSustain.Value / 1000f;
        sldPDRelease.ValueChanged += (o, e) => envGenPD.Release = sldPDRelease.Value / 100f;

        sldPDModAmount.ValueChanged += (o, e) => {
            mixerVcoPdAmount1.Levels[0] = sldPDModAmount.Value / 1000f;
            mixerVcoPdAmount2.Levels[0] = sldPDModAmount.Value / 1000f;
            mixerVcoPdAmount3.Levels[0] = sldPDModAmount.Value / 1000f;
        };

        chkPDOsc1.CheckedChanged += (o, e) => { if(chkPDOsc1.Checked) osc1.Duty.ModulationAmount = 1f; else osc1.Duty.ModulationAmount = 0f; };
        chkPDOsc2.CheckedChanged += (o, e) => { if (chkPDOsc2.Checked) osc2.Duty.ModulationAmount = 1f; else osc2.Duty.ModulationAmount = 0f; };
        chkPDOsc3.CheckedChanged += (o, e) => { if (chkPDOsc3.Checked) osc3.Duty.ModulationAmount = 1f; else osc3.Duty.ModulationAmount = 0f; };



        // FM Env Gen
        sldFMAttack.ValueChanged += (o, e) => envGenFM.Attack = sldFMAttack.Value / 100f;
        sldFMDecay.ValueChanged += (o, e) => envGenFM.Decay = sldFMDecay.Value / 100f;
        sldFMSustain.ValueChanged += (o, e) => envGenFM.Sustain = sldFMSustain.Value / 1000f;
        sldFMRelease.ValueChanged += (o, e) => envGenFM.Release = sldFMRelease.Value / 100f;

        sldFMModAmount.ValueChanged += (o, e) => {
            mixerVcoFmAmount1.Levels[0] = sldFMModAmount.Value / 1000f;
            mixerVcoFmAmount2.Levels[0] = sldFMModAmount.Value / 1000f;
            mixerVcoFmAmount3.Levels[0] = sldFMModAmount.Value / 1000f;
        };

        chkFMOsc1.CheckStateChanged += (o, e) => {
            if (chkFMOsc1.Checked)
                osc1.Frequency.ModulationAmountModulator = mixerVcoFmAmount1;
            else
                osc1.Frequency.ModulationAmountModulator = null;
        };

        chkFMOsc2.CheckStateChanged += (o, e) => {
            if (chkFMOsc2.Checked)
                osc2.Frequency.ModulationAmountModulator = mixerVcoFmAmount2;
            else
                osc2.Frequency.ModulationAmountModulator = null;
        };

        chkFMOsc3.CheckStateChanged += (o, e) => {
            if (chkFMOsc3.Checked)
                osc3.Frequency.ModulationAmountModulator = mixerVcoFmAmount3;
            else
                osc3.Frequency.ModulationAmountModulator = null;
        };




        // Lfos
        sldLfoVcfDelay.ValueChanged += (o, e) => { lfoVcfCutoff.Delay = sldLfoVcfDelay.Value / 1000f; };
        sldLfoVcfWave.ValueChanged += (o, e) => { lfoVcfCutoff.WaveForm = getWaveFormByLfoSlider(sldLfoVcfWave); };
        sldLfoVcfFreq.ValueChanged += (o, e) => { lfoVcfCutoff.Frequency = sldLfoVcfFreq.Value / 1000f; };
        sldLfoVcfAmount.ValueChanged += (o, e) => { vcf.ModulatorAmount = sldLfoVcfAmount.Value / 1000f; };



        sldLfoVibDelay.ValueChanged += (o, e) => { lfoOscFreq.Delay = sldLfoVibDelay.Value / 1000f; };
        sldLfoVibWave.ValueChanged += (o, e) => { lfoOscFreq.WaveForm = getWaveFormByLfoSlider(sldLfoVibWave); };
        sldLfoVibFreq.ValueChanged += (o, e) => { lfoOscFreq.Frequency = sldLfoVibFreq.Value / 1000f; };
        sldLfoVibAmount.ValueChanged += (o, e) => { osc1.Frequency.LFOAmount = sldLfoVibAmount.Value; osc2.Frequency.LFOAmount = sldLfoVibAmount.Value; osc3.Frequency.LFOAmount = sldLfoVibAmount.Value; };
        chkLfoVibOsc1.CheckedChanged += (o, e) => { if (chkLfoVibOsc1.Checked) osc1.Frequency.ModulatorLFO = lfoOscFreq; else osc1.Frequency.ModulatorLFO = null; };
        chkLfoVibOsc2.CheckedChanged += (o, e) => { if (chkLfoVibOsc2.Checked) osc2.Frequency.ModulatorLFO = lfoOscFreq; else osc2.Frequency.ModulatorLFO = null; };
        chkLfoVibOsc3.CheckedChanged += (o, e) => { if (chkLfoVibOsc3.Checked) osc3.Frequency.ModulatorLFO = lfoOscFreq; else osc3.Frequency.ModulatorLFO = null; };


        sldLfoFMDelay.ValueChanged += (o, e) => { lfoOscFM.Delay = sldLfoFMDelay.Value / 1000f; };
        sldLfoFMWave.ValueChanged += (o, e) => { lfoOscFM.WaveForm = getWaveFormByLfoSlider(sldLfoFMWave); };
        sldLfoFMFreq.ValueChanged += (o, e) => { lfoOscFM.Frequency = sldLfoFMFreq.Value / 1000f; };
        sldLfoFMAmount.ValueChanged += (o, e) => {
            mixerVcoFmAmount1.Levels[1] = sldLfoFMAmount.Value / 5000f;
            mixerVcoFmAmount2.Levels[1] = sldLfoFMAmount.Value / 5000f;
            mixerVcoFmAmount3.Levels[1] = sldLfoFMAmount.Value / 5000f;
        };





        // Filter
        sldVcfCutoff.ValueChanged += (o, e) => vcf.CutoffFrequency = sldVcfCutoff.Value / 1000f;
        sldVcfEnvAmount.ValueChanged += (o, e) => vcf.ModulatorAmount2 = sldVcfEnvAmount.Value / 1000f;
        ///sldVcfQ.ValueChanged += (o, e) => vcf.QAmount = sldVcfQ.Value / 1000f;

        sldVcfEnvAttack.ValueChanged += (o,e) => envGenVcf.Attack = sldVcfEnvAttack.Value / 100f;
        sldVcfEnvDecay.ValueChanged += (o, e) => envGenVcf.Decay = sldVcfEnvDecay.Value / 100f;
        sldVcfEnvSustain.ValueChanged += (o, e) => envGenVcf.Sustain = sldVcfEnvSustain.Value / 1000f;
        sldVcfEnvRelease.ValueChanged += (o, e) => envGenVcf.Release = sldVcfEnvRelease.Value / 100f;


        virtualKeyboard.NoteChanged += virtualKeyboard_NoteChanged;
        virtualKeyboard.KeyStateChanged += virtualKeyboard_KeyStateChanged;
        virtualKeyboard.PitchWheelChanged += (o, e) => synth.PitchWheel = virtualKeyboard.CurrentPitchWheel;
        virtualKeyboard.ModWheelChanged += (o, e) => 
            synth.ModWheel.Value = virtualKeyboard.CurrentModWheel;

        this.KeyUp += SimpleTest_KeyUp;

        cmdInitPatch.Click += CmdInitPatch_Click;
        ddlPatches.SelectedIndexChanged += DdlPatches_SelectedIndexChanged;
        cmdSavePatch.Click += CmdSavePatch_Click;
        cmdDeletePatch.Click += CmdDeletePatch_Click;
    }



    WaveForm getWaveFormByLfoSlider(TrackBar sldr) {
        switch (sldr.Value) {
            case 0:
                return WaveForm.GetByType(WaveformType.Sine);
            case 1:
                return WaveForm.GetByType(WaveformType.Triangle);
            case 2:
                return WaveForm.GetByType(WaveformType.Saw);
            case 3:
                return WaveForm.GetByType(WaveformType.SawFalling);
            case 4:
                return WaveForm.GetByType(WaveformType.Square);
            case 5:
                return WaveForm.GetByType(WaveformType.SH);
            default: return null;
        }
    }



    #endregion

    #region Virtual Keyboard keypress detection
    protected override bool ProcessCmdKey(ref Message msg, Keys keyData) {
        // This captures keys for the entire form. We can process, or let the base method handle

        if (virtualKeyboard.ProcessKeyDown(keyData))
            return true;
        else
            return base.ProcessCmdKey(ref msg, keyData);
    }

    // Global 'Key Up' handler. We've set Form KeyPreview property to true so that form rather than controls can capture this event
    private void SimpleTest_KeyUp(object? sender, KeyEventArgs e) {
        virtualKeyboard.CurrentKeyState = VirtualKeyboard.KeyState.Up;
        virtualKeyboard_KeyStateChanged(null, new EventArgs());
    }
    #endregion

    #region Event Handlers
    private void virtualKeyboard_KeyStateChanged(object? sender, EventArgs e) {
        if (virtualKeyboard.CurrentKeyState == VirtualKeyboard.KeyState.Up)
            keyboard.KeyUp = true;
        else
            keyboard.KeyDown = true;
    }

    private void virtualKeyboard_NoteChanged(object? sender, EventArgs e) {
        ddlNote.SelectedIndex = virtualKeyboard.CurrentNote.ID - 1;
    }

    private void CmdPauseGraph_Click(object? sender, EventArgs e) {
        timDisplay.Enabled = !timDisplay.Enabled;
        if (timDisplay.Enabled)
            cmdPauseGraph.Text = "Pause";
        else
            cmdPauseGraph.Text = "Show";
    }

    private void TimDisplay_Tick(object? sender, EventArgs e) {
        timDisplay.Enabled = false;
        DrawDisplay();
        timDisplay.Enabled = true;
    }

    private void DdlNote_SelectedIndexChanged(object? sender, EventArgs e) {
        keyboard.Note = (Note)ddlNote.SelectedItem;


    }

    private void SldWaveForm_ValueChanged(object? sender, EventArgs e) {
        osc1.WaveFormSelectByID(sldWaveForm.Value);
        lblWaveform.Text = (WaveForm.GetByID(sldWaveForm.Value)).Name;

        lblWaveTable.Visible = sldWaveForm.Value == (int)WaveformType.WaveTable;
        cmdSelectOscSetting.Enabled = sldWaveForm.Value == (int)WaveformType.WaveTable || sldWaveForm.Value == (int)WaveformType.Harmonic;
        ddlSuperSaw.Visible = sldWaveForm.Value == (int)WaveformType.SuperSaw;

        if (sldWaveForm.Value == (int)WaveformType.SuperSaw || sldWaveForm.Value == (int)WaveformType.Harmonic)
            sldPWM.Enabled = false;
        else
            sldPWM.Enabled = true;


        if (sldWaveForm.Value == (int)WaveformType.Square)
            lblPWM.Text = "PWM:";
        else
            lblPWM.Text = "Phase Dist:";
    }

    private void SldOctave_ValueChanged(object? sender, EventArgs e) {
        osc1.Frequency.Octave = sldOctave.Value;
    }

    private void SldTune_ValueChanged(object? sender, EventArgs e) {
        osc1.Frequency.Tune = (float)sldTune.Value / 12f;
    }

    private void SldFineTune_ValueChanged(object? sender, EventArgs e) {
        // sldFineTune value -100 to +100, so for +/- 1 semitone:  Value / 1200f
        osc1.Frequency.FineTune = (float)sldFineTune.Value / 1200f;
    }

    private void CmdReset_Click(object? sender, EventArgs e) {
        sldFineTune.Value = 0;
    }

    private void SldPWM_ValueChanged(object? sender, EventArgs e) {
        osc1.Duty.Value = (float)sldPWM.Value / 1000f;
    }

    private void SldLevel_ValueChanged(object? sender, EventArgs e) {
        mixerOsc.Levels[0] = sldLevel.Value / 100f;
    }
    private void CmdOsctSetting_Click(object? sender, EventArgs e) {
        bool originalSynthStateStarted = synth.Started;
        synth.Stop();

        switch ((WaveformType)sldWaveForm.Value) {
            case WaveformType.WaveTable:
                var fileName = frmSelectWavetable.Show(osc1.WaveTableFileName);
                if (fileName != "") {
                    lblWaveTable.Text = truncateFileName(fileName, 14);
                    osc1.WaveTableFileName = fileName;
                }
                break;
            case WaveformType.Harmonic:
                var coefficients = frmSelectFourierCoefficients.Show(osc1.FourierCoefficients);
                if (!coefficients.All(c => c==0)) {         // All coeffs 0 is Cancel pressed
                    osc1.FourierCoefficients = coefficients;
                }
                break;
            default: break;  // Do Nothing
        }

        if (originalSynthStateStarted)
            synth.Start();
    }

    private void DdlSuperSaw_SelectedIndexChanged(object? sender, EventArgs e) {
        if (ddlSuperSaw.SelectedIndex <= 0)
            return;

        var sawSettings = (Data.SuperSaw)ddlSuperSaw.SelectedItem;
        osc1.FrequencyRatios = sawSettings.FrequencyRatios;
    }
    private void DdlSyncSource_SelectedIndexChanged(object? sender, EventArgs e) {
        switch (ddlSyncSource.SelectedIndex) {
            case 1:
                osc2.SyncDestination = osc1; break;
            case 2:
                osc3.SyncDestination = osc1; break;
            default:
                if (osc2.SyncDestination == osc1)
                    osc2.SyncDestination = null;
                if (osc3.SyncDestination == osc1)
                    osc3.SyncDestination = null;
                break;
        }
    }
    private void SldModAmount_ValueChanged(object? sender, EventArgs e) {
        // Not sure what max value should be, so let's cap at 1 for now
        osc1.Frequency.ModulationAmount = sldModAmount.Value;
    }

    private void DdlModSource_SelectedIndexChanged(object? sender, EventArgs e) {
        switch (ddlModSource.SelectedIndex) {
            case 1:
                osc1.Frequency.Modulator = osc2; break;
            case 2:
                osc1.Frequency.Modulator = osc3; break;
            default:
                osc1.Frequency.Modulator = null; break;
        }
    }

    private void ChkKbd_CheckedChanged(object? sender, EventArgs e) {
        if (chkKbd.Checked) osc1.Frequency.Keyboard = keyboard; else osc1.Frequency.Keyboard = null;
    }


    private void SldWaveForm1_ValueChanged(object? sender, EventArgs e) {
        osc2.WaveFormSelectByID(sldWaveForm1.Value);
        lblWaveform1.Text = (WaveForm.GetByID(sldWaveForm1.Value)).Name;

        lblWaveTable1.Visible = sldWaveForm1.Value == (int)WaveformType.WaveTable;
        cmdSelectOscSetting1.Enabled = sldWaveForm1.Value == (int)WaveformType.WaveTable || sldWaveForm1.Value == (int)WaveformType.Harmonic;
        ddlSuperSaw1.Visible = sldWaveForm1.Value == (int)WaveformType.SuperSaw;

        if (sldWaveForm1.Value == (int)WaveformType.SuperSaw || sldWaveForm1.Value == (int)WaveformType.Harmonic)
            sldPWM1.Enabled = false;
        else
            sldPWM1.Enabled = true;

        if (sldWaveForm1.Value == (int)WaveformType.Square)
            lblPWM1.Text = "PWM:";
        else
            lblPWM1.Text = "Phase Dist:";
    }

    private void SldOctave1_ValueChanged(object? sender, EventArgs e) {
        osc2.Frequency.Octave = sldOctave1.Value;
    }
    private void SldTune1_ValueChanged(object? sender, EventArgs e) {
        osc2.Frequency.Tune = (float)sldTune1.Value / 12f;
    }

    private void SldFineTune1_ValueChanged(object? sender, EventArgs e) {
        // sldFineTune value -100 to +100, so for +/- 1 semitone:  Value / 1200f
        osc2.Frequency.FineTune = (float)sldFineTune1.Value / 1200f;
    }

    private void CmdReset1_Click(object? sender, EventArgs e) {
        sldFineTune1.Value = 0;
    }

    private void SldPWM1_ValueChanged(object? sender, EventArgs e) {
        osc2.Duty.Value = (float)sldPWM1.Value / 100f;
    }
    private void SldLevel1_ValueChanged(object? sender, EventArgs e) {
        mixerOsc.Levels[1] = sldLevel1.Value / 100f;
    }

    private void CmdOsctSetting1_Click(object? sender, EventArgs e) {
        bool originalSynthStateStarted = synth.Started;
        synth.Stop();

        switch ((WaveformType)sldWaveForm1.Value) {
            case WaveformType.WaveTable:
                var fileName = frmSelectWavetable.Show(osc2.WaveTableFileName);
                if (fileName != "") {
                    lblWaveTable1.Text = truncateFileName(fileName, 14);
                    osc2.WaveTableFileName = fileName;
                }
                break;
            case WaveformType.Harmonic:
                var coefficients = frmSelectFourierCoefficients.Show(osc2.FourierCoefficients);
                if (!coefficients.All(c => c == 0)) {               // All coeffs 0 is Cancel pressed
                    osc2.FourierCoefficients = coefficients;
                }
                break;
            default: break;  // Do Nothing
        }

        if (originalSynthStateStarted)
            synth.Start();
    }

    private void DdlSuperSaw1_SelectedIndexChanged(object? sender, EventArgs e) {
        if (ddlSuperSaw1.SelectedIndex <= 0)
            return;

        var sawSettings = (Data.SuperSaw)ddlSuperSaw1.SelectedItem;
        osc2.FrequencyRatios = sawSettings.FrequencyRatios;
    }

    private void DdlSyncSource1_SelectedIndexChanged(object? sender, EventArgs e) {
        switch (ddlSyncSource1.SelectedIndex) {
            case 1:
                osc1.SyncDestination = osc2; break;
            case 2:
                osc3.SyncDestination = osc2; break;
            default:
                if (osc1.SyncDestination == osc2)
                    osc1.SyncDestination = null;
                if (osc3.SyncDestination == osc2)
                    osc3.SyncDestination = null;
                break;
        }
    }
    private void SldModAmount1_ValueChanged(object? sender, EventArgs e) {
        // Not sure what max value should be, so let's cap at 1 for now
        osc2.Frequency.ModulationAmount = sldModAmount1.Value;
    }

    private void DdlModSource1_SelectedIndexChanged(object? sender, EventArgs e) {
        switch (ddlModSource1.SelectedIndex) {
            case 1:
                osc2.Frequency.Modulator = osc1; break;
            case 2:
                osc2.Frequency.Modulator = osc3; break;
            default:
                osc2.Frequency.Modulator = null; break;
        }
    }

    private void ChkKbd1_CheckedChanged(object? sender, EventArgs e) {
        if (chkKbd1.Checked) osc2.Frequency.Keyboard = keyboard; else osc2.Frequency.Keyboard = null;
    }


    private void SldWaveForm2_ValueChanged(object? sender, EventArgs e) {
        osc3.WaveFormSelectByID(sldWaveForm2.Value);
        lblWaveform2.Text = (WaveForm.GetByID(sldWaveForm2.Value)).Name;

        lblWaveTable2.Visible = sldWaveForm2.Value == (int)WaveformType.WaveTable;
        cmdSelectOscSetting2.Enabled = sldWaveForm2.Value == (int)WaveformType.WaveTable || sldWaveForm2.Value == (int)WaveformType.Harmonic;
        ddlSuperSaw2.Visible = sldWaveForm2.Value == (int)WaveformType.SuperSaw;

        if (sldWaveForm2.Value == (int)WaveformType.SuperSaw || sldWaveForm2.Value == (int)WaveformType.Harmonic)
            sldPWM2.Enabled = false;
        else
            sldPWM2.Enabled = true;

        if (sldWaveForm2.Value == (int)WaveformType.Square)
            lblPWM2.Text = "PWM:";
        else
            lblPWM2.Text = "Phase Dist:";
    }

    private void SldOctave2_ValueChanged(object? sender, EventArgs e) {
        osc3.Frequency.Octave = sldOctave2.Value;
    }

    private void SldTune2_ValueChanged(object? sender, EventArgs e) {
        osc3.Frequency.Tune = (float)sldTune2.Value / 12f;
    }

    private void SldFineTune2_ValueChanged(object? sender, EventArgs e) {
        // sldFineTune value -100 to +100, so for +/- 1 semitone:  Value / 1200f
        osc3.Frequency.FineTune = (float)sldFineTune2.Value / 1200f;
    }

    private void CmdReset2_Click(object? sender, EventArgs e) {
        sldFineTune2.Value = 0;
    }

    private void SldPWM2_ValueChanged(object? sender, EventArgs e) {
        osc3.Duty.Value = (float)sldPWM2.Value / 100f;
    }
    private void SldLevel2_ValueChanged(object? sender, EventArgs e) {
        mixerOsc.Levels[2] = sldLevel2.Value / 100f;
    }

    private void CmdOsctSetting2_Click(object? sender, EventArgs e) {
        bool originalSynthStateStarted = synth.Started;
        synth.Stop();

        switch ((WaveformType)sldWaveForm2.Value) {
            case WaveformType.WaveTable:
                var fileName = frmSelectWavetable.Show(osc3.WaveTableFileName);
                if (fileName != "") {
                    lblWaveTable2.Text = truncateFileName(fileName, 14);
                    osc3.WaveTableFileName = fileName;
                }
                break;
            case WaveformType.Harmonic:
                var coefficients = frmSelectFourierCoefficients.Show(osc3.FourierCoefficients);
                if (!coefficients.All(c => c == 0)) {                   // All coeffs 0 is Cancel pressed
                    osc3.FourierCoefficients = coefficients;
                }
                break;
            default: break;  // Do Nothing
        }

        if (originalSynthStateStarted)
            synth.Start();
    }

    private void DdlSuperSaw2_SelectedIndexChanged(object? sender, EventArgs e) {
        if (ddlSuperSaw2.SelectedIndex <= 0)
            return;

        var sawSettings = (Data.SuperSaw)ddlSuperSaw2.SelectedItem;
        osc3.FrequencyRatios = sawSettings.FrequencyRatios;
    }

    void DdlSyncSource2_SelectedIndexChanged(object? sender, EventArgs e) {
        switch (ddlSyncSource2.SelectedIndex) {
            case 1:
                osc1.SyncDestination = osc3; break;
            case 2:
                osc2.SyncDestination = osc3; break;
            default:
                if (osc1.SyncDestination == osc3)
                    osc1.SyncDestination = null;
                if (osc2.SyncDestination == osc3)
                    osc2.SyncDestination = null;
                break;
        }
    }

    private void SldModAmount2_ValueChanged(object? sender, EventArgs e) {
        // Not sure what max value should be, so let's cap at 1 for now
        osc3.Frequency.ModulationAmount = sldModAmount2.Value;
    }

    private void DdlModSource2_SelectedIndexChanged(object? sender, EventArgs e) {
        switch (ddlModSource2.SelectedIndex) {
            case 1:
                osc3.Frequency.Modulator = osc1; break;
            case 2:
                osc3.Frequency.Modulator = osc2; break;
            default:
                osc3.Frequency.Modulator = null; break;
        }
    }

    private void ChkKbd2_CheckedChanged(object? sender, EventArgs e) {
        if (chkKbd2.Checked) osc3.Frequency.Keyboard = keyboard; else osc3.Frequency.Keyboard = null;
    }

    private void CmdSavePatch_Click(object? sender, EventArgs e) {
        // Get Patch Name

        var savedPatch = Patch.SaveNewPatch(this, osc1, osc2, osc3);           // Pass form across so control settings can be scraped

        // Patch object returned if sucesfull. Select from ddl if so, otherwise, just return
        if (savedPatch == null)
            return;

        LoadPatchList();
        foreach (var p in ddlPatches.Items) {
            if (((Patch)p).PatchName.Trim().ToLower() == ((Patch)savedPatch).PatchName.Trim().ToLower()) {
                ddlPatches.SelectedItem = p;
                break;
            }
        }
    }

    private void DdlPatches_SelectedIndexChanged(object? sender, EventArgs e) {
        var p = (Patch)ddlPatches.SelectedItem;
        if (p.PatchName != "")
            Patch.RecallPatch(this, p, osc1, osc2, osc3);
    }

    private void CmdInitPatch_Click(object? sender, EventArgs e) {
        var p = Patch.GetInitPatch();

        Patch.RecallPatch(this, p, osc1, osc2, osc3);
    }

    private void CmdDeletePatch_Click(object? sender, EventArgs e) {
        if (ddlPatches.SelectedIndex < 1) {
            MessageBox.Show("Please select a patch to delete from the list of Patches", "Delete Patch", MessageBoxButtons.OK,  MessageBoxIcon.Exclamation);
            ddlPatches.Focus();
            return;
        }

        if(((Patch)ddlPatches.SelectedItem).IsInit) {
            MessageBox.Show("'Init' patch must always be present, so unable to delete", "Delete Patch", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
            return;
        }

        // Returns true if sucesfully deleted
        if (Patch.DeletePatch(((Patch)ddlPatches.SelectedItem).PatchName))
            LoadPatchList();            
            
    }


    #endregion

    #region Private Methods
    string FormatFrequency(float frequency) {
        if (frequency < 1000)
            return $"{frequency.ToString("0.##")} Hz";
        else
            return $"{(frequency/ 1000).ToString("0.##")} kHz";

    }
    string truncateFileName(string fileName, int Length) {
        fileName.Substring(fileName.LastIndexOf('\\') + 1);

        if (fileName.Length <= Length)
            return fileName;

        fileName = fileName.Substring(fileName.Length - Length);
        return ".." + fileName;
    }
    #endregion

    #region Graph Display Wave + Spectrumn
    private async void DrawDisplay() {
        // Get sample wave data from synth engine
        var graphData = synth.GetGraphData(picWaveGraph.Width);

        var p = new Pen(Color.Lime);
        Graphics g = picWaveGraph.CreateGraphics();
        Point[] points = new Point[graphData.Length];
        for(int i = 0; i < graphData.Length; i++) {
            points[i] = new Point(i, (int)(graphData[i] * picWaveGraph.Height* .9f + picWaveGraph.Height / 2));
        }

        g.Clear(Color.Black);
        g.DrawLines(p, points);


        // Get and draw spectrum
        var s = await Task.Run(() => GetSpectrum(Array.ConvertAll(graphData, x => (double)x)));
        double maxCoeff = s.MaxBy(x => x);
        if (maxCoeff < .01)
            maxCoeff = 0.01;

        Point[] spectrum = new Point[s.Length];
        for (int i = 0; i < s.Length; i++)
            spectrum[i] = new Point(i*7, picWaveGraph.Height - (int)((s[i] * picWaveGraph.Height * .95/maxCoeff)  -1));

        p = new Pen(Color.CornflowerBlue);
        g.DrawLines(p, spectrum);

        graphData = null;           // Nullify these to ensure they get caught by GC
    }

    private double[] GetSpectrum(double[] signal) {
        // Uses nuget package from https://github.com/swharden/FftSharp

        // Begin with an array containing sample data
        //double[] signal = FftSharp.SampleData.SampleAudiosc1();

        // Shape the signal using a Hanning window
        var window = new FftSharp.Windows.Hanning();
        window.ApplyInPlace(signal);

        // Calculate the FFT as an array of complex numbers
        // Complex[] fftRaw = FftSharp.Transform.FFT(signal);

        // or get the magnitude (units²) or power (dB) as real numbers
        double[] fftMag = FftSharp.Transform.FFTmagnitude(signal);
        return fftMag;
    }

    #endregion

    #region Stop Start
    private void cmdStart_Click(object sender, EventArgs e) {
        cmdStart.Enabled = false;
        cmdStop.Enabled = true;
        synth.Start();

    }

    private void cmdStop_Click(object sender, EventArgs e) {
        cmdStart.Enabled = true;
        cmdStop.Enabled = false;

        synth.Stop();
    }

    #endregion


}
