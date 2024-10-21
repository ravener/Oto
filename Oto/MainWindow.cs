using Mapping_Tools_Core.BeatmapHelper.BeatDivisors;
using Mapping_Tools_Core.BeatmapHelper.Enums;
using Mapping_Tools_Core.BeatmapHelper.IO.Decoding;
using Mapping_Tools_Core.BeatmapHelper.IO.Encoding;
using Mapping_Tools_Core.Exceptions;
using Mapping_Tools_Core.Tools.HitsoundCopierStuff;


namespace Oto
{
    public class MainWindow : Adw.ApplicationWindow
    {
        [Gtk.Connect] private readonly Gtk.Button _runButton = default!;
        [Gtk.Connect] private readonly Gtk.Button _sourceButton = default!;
        [Gtk.Connect] private readonly Gtk.Label _sourceLabel = default!;
        [Gtk.Connect] private readonly Gtk.Button _targetButton = default!;
        [Gtk.Connect] private readonly Gtk.Label _targetLabel = default!;
        [Gtk.Connect] private readonly Gtk.SpinButton _temporalLeniency = default!;
        [Gtk.Connect] private readonly Gtk.Switch _copyHitsounds = default!;
        [Gtk.Connect] private readonly Gtk.Switch _copySliderbodyHitsounds = default!;
        [Gtk.Connect] private readonly Gtk.Switch _copySamplesets = default!;
        [Gtk.Connect] private readonly Gtk.Switch _copyVolumes = default!;
        [Gtk.Connect] private readonly Gtk.Switch _alwaysPreserve5Volume = default!;
        [Gtk.Connect] private readonly Adw.ActionRow _alwaysPreserve5VolumeRow = default!;
        [Gtk.Connect] private readonly Gtk.Switch _copyStoryboardedSamples = default!;
        [Gtk.Connect] private readonly Gtk.Switch _muteSliderends = default!;
        [Gtk.Connect] private readonly Adw.ToastOverlay _toastOverlay = default!;
        [Gtk.Connect] private readonly Gtk.Popover _allBeatDivisorsPopup = default!;
        [Gtk.Connect] private readonly Gtk.Button _allBeatDivisorsButton = default!;
        [Gtk.Connect] private readonly Gtk.Popover _mutedBeatDivisorsPopup = default!;
        [Gtk.Connect] private readonly Gtk.Button _mutedBeatDivisorsButton = default!;
        [Gtk.Connect] private readonly Adw.PreferencesGroup _sliderendMutingFilter = default!;
        [Gtk.Connect] private readonly Adw.PreferencesGroup _mutedConfig = default!;

        // The damn checkboxes, oh no, idk if there is a better way to manage them.
        [Gtk.Connect] private readonly Gtk.CheckButton _all_1_1 = default!;
        [Gtk.Connect] private readonly Gtk.CheckButton _all_1_2 = default!;
        [Gtk.Connect] private readonly Gtk.CheckButton _all_1_3 = default!;
        [Gtk.Connect] private readonly Gtk.CheckButton _all_1_4 = default!;
        [Gtk.Connect] private readonly Gtk.CheckButton _all_1_6 = default!;
        [Gtk.Connect] private readonly Gtk.CheckButton _all_1_8 = default!;
        [Gtk.Connect] private readonly Gtk.CheckButton _all_1_12 = default!;
        [Gtk.Connect] private readonly Gtk.CheckButton _all_1_16 = default!;
        [Gtk.Connect] private readonly Gtk.CheckButton _all_1_5 = default!;
        [Gtk.Connect] private readonly Gtk.CheckButton _all_1_7 = default!;
        [Gtk.Connect] private readonly Gtk.CheckButton _all_1_9 = default!;
        [Gtk.Connect] private readonly Gtk.CheckButton _all_1_11 = default!;
        [Gtk.Connect] private readonly Gtk.CheckButton _all_1_13 = default!;
        [Gtk.Connect] private readonly Gtk.CheckButton _muted_1_1 = default!;
        [Gtk.Connect] private readonly Gtk.CheckButton _muted_1_2 = default!;
        [Gtk.Connect] private readonly Gtk.CheckButton _muted_1_3 = default!;
        [Gtk.Connect] private readonly Gtk.CheckButton _muted_1_4 = default!;
        [Gtk.Connect] private readonly Gtk.CheckButton _muted_1_6 = default!;
        [Gtk.Connect] private readonly Gtk.CheckButton _muted_1_8 = default!;
        [Gtk.Connect] private readonly Gtk.CheckButton _muted_1_12 = default!;
        [Gtk.Connect] private readonly Gtk.CheckButton _muted_1_16 = default!;
        [Gtk.Connect] private readonly Gtk.CheckButton _muted_1_5 = default!;
        [Gtk.Connect] private readonly Gtk.CheckButton _muted_1_7 = default!;
        [Gtk.Connect] private readonly Gtk.CheckButton _muted_1_9 = default!;
        [Gtk.Connect] private readonly Gtk.CheckButton _muted_1_11 = default!;
        [Gtk.Connect] private readonly Gtk.CheckButton _muted_1_13 = default!;
        [Gtk.Connect] private readonly Gtk.DropDown _sampleset = default!;
        [Gtk.Connect] private readonly Gtk.SpinButton _minLength = default!;
        [Gtk.Connect] private readonly Gtk.SpinButton _mutedSampleIndex = default!;

        private Gio.File? _sourceFile;
        private Gio.File? _targetFile;
        private string? _lastError;
        private Gtk.DropTarget _drop1;
        private Gtk.DropTarget _drop2;

        private MainWindow(Adw.Application app, Gtk.Builder builder) : base(builder.GetPointer("_root"), false)
        {
            Application = app;
            builder.Connect(this);

            _runButton.OnClicked += OnRun;
            _sourceButton.OnClicked += OnFileSelect;
            _targetButton.OnClicked += OnFileSelect;

            _muteSliderends.OnStateSet += (sender, args) =>
            {
                _sliderendMutingFilter.Visible = args.State;
                _mutedConfig.Visible = args.State;
                return false;
            };

            // 'Always Preserve 5% Volume' implies Copy Volumes
            // So if Copy Volumes is off, disable that option.
            _copyVolumes.OnStateSet += (sender, args) =>
            {
                _alwaysPreserve5Volume.SetSensitive(args.State);

                if (args.State)
                {
                    _alwaysPreserve5VolumeRow.RemoveCssClass("dim-label");
                }
                else
                {
                    _alwaysPreserve5VolumeRow.AddCssClass("dim-label");
                }

                return false;
            };

            var action = Gio.SimpleAction.New("show_error", null);
            action.OnActivate += (sender, args) =>
            {
                if (_lastError != null)
                {
                    var dialog = Adw.MessageDialog.New(this, "Error", _lastError);
                    dialog.AddResponse("ok", "Okay");
                    dialog.SetCloseResponse("ok");
                    dialog.Show();
                }
            };
            AddAction(action);

            _allBeatDivisorsPopup.SetParent(_allBeatDivisorsButton);
            _mutedBeatDivisorsPopup.SetParent(_mutedBeatDivisorsButton);

            _allBeatDivisorsButton.OnClicked += (sender, args) =>
            {
                _allBeatDivisorsPopup.Show();
            };

            _mutedBeatDivisorsButton.OnClicked += (sender, args) =>
            {
                _mutedBeatDivisorsPopup.Show();
            };

            _drop1 = Gtk.DropTarget.New(Gio.FileHelper.GetGType(), Gdk.DragAction.Copy);
            _drop1.OnDrop += OnDrop;
            _sourceButton.AddController(_drop1);

            _drop2 = Gtk.DropTarget.New(Gio.FileHelper.GetGType(), Gdk.DragAction.Copy);
            _drop2.OnDrop += OnDrop;
            _targetButton.AddController(_drop2);
        }

        public MainWindow(Adw.Application app) : this(app, new Gtk.Builder("window.ui"))
        {
        }

        private bool IsFlatpak()
        {
            return Environment.GetEnvironmentVariable("FLATPAK_ID") != null || File.Exists("/.flatpak-info");
        }

        private bool OnDrop(Gtk.DropTarget drop, Gtk.DropTarget.DropSignalArgs e)
        {
            var file = new Gio.FileHelper(e.Value.GetObject()!.Handle, false);
            var path = file.GetPath();
            Console.WriteLine(path);

            if (File.Exists(path))
            {
                if (drop == _drop1)
                {
                    _sourceFile = file;
                    _sourceLabel.SetLabel(_sourceFile!.GetBasename()!);
                }
                else if (drop == _drop2)
                {
                    _targetFile = file;
                    _targetLabel.SetLabel(_targetFile!.GetBasename()!);
                }

                // Enable the Run button if both files are provided.
                if (_sourceFile != null && _targetFile != null)
                {
                    _runButton.SetSensitive(true);
                    _runButton.SetTooltipText(null);
                }

                return true;
            }

            return false;
        }

        private void OnFileSelect(Gtk.Button button, EventArgs args)
        {
            var chooser = Gtk.FileChooserNative.New("Select a Beatmap File", this, Gtk.FileChooserAction.Open, null, null);

            // Filter the selectable files.
            chooser.Filter = Gtk.FileFilter.New();
            chooser.Filter.AddSuffix("osu");
            chooser.Filter.AddMimeType("application/x-osu-beatmap");

            chooser.OnResponse += (sender, args) =>
            {
                if (args.ResponseId == (int)Gtk.ResponseType.Accept)
                {
                    if (button == _sourceButton)
                    {
                        _sourceFile = chooser.GetFile();
                        _sourceLabel.SetLabel(_sourceFile!.GetBasename()!);
                    }
                    else if (button == _targetButton)
                    {
                        _targetFile = chooser.GetFile();
                        _targetLabel.SetLabel(_targetFile!.GetBasename()!);
                    }

                    // Enable the Run button if both files are provided.
                    if (_sourceFile != null && _targetFile != null)
                    {
                        _runButton.SetSensitive(true);
                        _runButton.SetTooltipText(null);
                    }
                }
            };

            chooser.Show();
        }


        private async void OnRun(Gtk.Button sender, EventArgs args)
        {
            Console.WriteLine("OnRun!");
            if (_muteSliderends.Active && GetAllBeatDivisors().Length < 1)
            {
                _toastOverlay.AddToast(Adw.Toast.New("Please select at least 1 beat divisor!"));
                return;
            }

            var beatmapFrom = await File.ReadAllTextAsync(_sourceFile!.GetPath()!);
            var beatmapTo = await File.ReadAllTextAsync(_targetFile!.GetPath()!);

            Process(beatmapFrom, beatmapTo);
        }

        private void SaveFile(string beatmap)
        {
            var chooser = Gtk.FileChooserNative.New("Save Beatmap", this, Gtk.FileChooserAction.Save, null, null);
            Console.WriteLine(_targetFile!.GetParent()!.GetPath());
            Console.WriteLine(_targetFile!.GetBasename());

            // Flatpak sandbox gives access to files within /run/user/... directories.
            // So we do not see the actual path, this code is meant to hint to the user
            // the same path as original file, so they can quickly save it in the same place.
            // But this won't work on flatpak so disable it till we find a better way.
            if (!IsFlatpak())
            {
                chooser.SetCurrentFolder(_targetFile!.GetParent()!);
                chooser.SetCurrentName(_targetFile!.GetBasename()!);
            }

            chooser.OnResponse += async (sender, args) =>
            {
                Console.WriteLine("on response!");
                if (args.ResponseId == (int)Gtk.ResponseType.Accept)
                {
                    var file = chooser.GetFile();
                    await File.WriteAllTextAsync(file!.GetPath()!, beatmap);
                    _toastOverlay.AddToast(Adw.Toast.New("Successfully copied hitsounds!"));
                }
            };

            chooser.Show();
        }

        private void ShowToast(string message)
        {
            var toast = Adw.Toast.New(message);
            toast.ButtonLabel = "Details";
            toast.ActionName = "win.show_error";
            _toastOverlay.AddToast(toast);
        }

        private static IBeatDivisor[] GetBeatDivisors(Gtk.CheckButton[] buttons)
        {
            var list = new List<IBeatDivisor>();
            var steps = new int[] { 1, 2, 3, 4, 6, 8, 12, 16, 5, 7, 9, 11, 13 };

            for (int i = 0; i < steps.Length; i++)
            {
                var button = buttons[i];

                if (button.Active)
                {
                    list.Add(new RationalBeatDivisor(1, steps[i]));
                }
            }

            return list.ToArray();
        }

        private IBeatDivisor[] GetAllBeatDivisors()
        {
            return GetBeatDivisors([
                _all_1_1,
                _all_1_2,
                _all_1_3,
                _all_1_4,
                _all_1_6,
                _all_1_8,
                _all_1_12,
                _all_1_16,
                _all_1_5,
                _all_1_7,
                _all_1_9,
                _all_1_11,
                _all_1_13
            ]);
        }

        private IBeatDivisor[] GetMutedBeatDivisors()
        {
            return GetBeatDivisors([
                _muted_1_1,
                _muted_1_2,
                _muted_1_3,
                _muted_1_4,
                _muted_1_6,
                _muted_1_8,
                _muted_1_12,
                _muted_1_16,
                _muted_1_5,
                _muted_1_7,
                _muted_1_9,
                _muted_1_11,
                _muted_1_13
            ]);
        }

        private void Process(string source, string destination)
        {
            SampleSet[] sampleSets = [SampleSet.None, SampleSet.Normal, SampleSet.Soft, SampleSet.Drum];

            try
            {
                var decoder = new OsuBeatmapDecoder();
                var beatmapFrom = decoder.Decode(source);
                var beatmapTo = decoder.Decode(destination);

                var copier = new HitsoundCopier
                {
                    TemporalLeniency = _temporalLeniency.Value,
                    DoCopyHitsounds = _copyHitsounds.Active,
                    DoCopyBodyHitsounds = _copySliderbodyHitsounds.Active,
                    DoCopySampleSets = _copySamplesets.Active,
                    DoCopyVolumes = _copyVolumes.Active,
                    AlwaysPreserve5Volume = _alwaysPreserve5Volume.Active
                };

                copier.CopyHitsoundsBasic(beatmapFrom, beatmapTo, out var processedTimeline);

                if (_copyStoryboardedSamples.Active)
                {
                    copier.CopyStoryboardedSamples(beatmapFrom, beatmapTo, true);
                }

                if (_muteSliderends.Active)
                {
                    var muter = new SliderendSilencer()
                    {
                        BeatDivisors = GetAllBeatDivisors(),
                        MutedDivisors = GetMutedBeatDivisors(),
                        MinLength = _minLength.Value,
                        MutedIndex = (int)_mutedSampleIndex.Value,
                        MutedSampleSet = sampleSets[_sampleset.GetSelected()]
                    };

                    muter.MuteSliderends(beatmapTo, processedTimeline);
                }

                var encoded = new OsuBeatmapEncoder().Encode(beatmapTo);
                SaveFile(encoded);
            }
            catch (BeatmapParsingException e)
            {
                _lastError = e.Message;
                ShowToast("Beatmap parsing failed");
                Console.WriteLine(e);
            }
            catch (Exception e)
            {
                _lastError = e.Message;
                ShowToast("An unexpected error occurred");
                Console.WriteLine(e);
            }
        }
    }
}