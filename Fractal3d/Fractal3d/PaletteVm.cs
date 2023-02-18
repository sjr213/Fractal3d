using BasicWpfLibrary;
using FractureCommonLib;
using ImageCalculator;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using System.Windows.Forms;

namespace Fractal3d
{
    public class PaletteVm : ViewModelBase, IColorPointVmParent
    {
        private Palette _palette;
        private readonly Action<Palette> _onPaletteChanged;
        private readonly Action<DisplayInfo> _onDisplayInfoChanged;
        private readonly List<Palette> _oldPalettes = new();
        private int _paletteIndex = -1;
        private DisplayInfo _displayInfo;

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
        public PaletteVm(Palette palette, Action<Palette> paletteChanged, Action<DisplayInfo> displayInfoChanged, DisplayInfo displayInfo)
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
        {
            PalettePath = RegistryUtil.ReadStringFromRegistry(RegistryUtil.PalettePathKey);
            _palette = palette;
            _displayInfo = displayInfo;
            AddPalette(palette);
            _onPaletteChanged = paletteChanged;
            _onDisplayInfoChanged = displayInfoChanged;
            _saveCommand = new RelayCommand(_ => OnSave(), _ => true);
            _openCommand = new RelayCommand(_ => OnOpen(), _ => true);
            _loadPaletteCommand = new RelayCommand(_ => OnLoadPalette(), _ => CanLoadPalette());
            _undoPaletteCommand = new RelayCommand(_ => OnUndoPalette(), _ => CanUndoPalette());
            _redoPaletteCommand = new RelayCommand(_ => OnRedoPalette(), _ => CanRedoPalette());
            _changePathCommand = new RelayCommand(_ => OnChangePath(), _ => true);
            ColorPointViewModel = new ColorPointVm(this);
            CreatePaletteImage();
            CreateRectItems();
            CreateTicItems();
            PaletteItems = PaletteFileUtil.LoadPaletteItems(PalettePath);
            DisplayInfoViewModel = new DisplayInfoVm(displayInfo, OnDisplayInfoChanged);
        }

        private void OnDisplayInfoChanged(DisplayInfo displayInfo)
        {
            _displayInfo = displayInfo;
            CreatePaletteImage();
            _onDisplayInfoChanged(displayInfo);
        }

        private void AddPalette(Palette pal)
        {
            _oldPalettes.Add(pal);
            _paletteIndex = _oldPalettes.Count - 1;
        }

        private void CreateRectItems()
        {
            var items = new ObservableCollection<RectItem>();

            var ptList = _palette.GetCopyOfColorPointList();
            foreach(var pin in ptList)
            {
                var colorPt = pin.Value;
                items.Add(RectItem.MakeColorRect(colorPt, items, UpdatePins, r => SelectedRectItem = r));
            }

            RectItems = items;
        }

        private void CreatePaletteImage()
        {
            var bmp = PaletteImage.CreateBitmapForPalette(_palette, _displayInfo);
            Image = ImageUtil.BitmapToImageSource(bmp);
        }

        public string PaletteName
        {
            get => _palette.PaletteName;
            set
            {
                if (_palette.PaletteName == value)
                    return;

                _palette.PaletteName = value;
                OnPropertyChanged();
            }
        }

        private string _palettePath = "";
        public string PalettePath
        {
            get => _palettePath;
            set
            {
                if (_palettePath == value)
                    return;

                _palettePath = value;
                OnPropertyChanged();
            }
        }

        public int NumberOfColors
        {
            get => _palette.NumberOfColors;
            set
            {
                if (_palette.NumberOfColors == value)
                    return;

                _palette.NumberOfColors = value;
                OnPropertyChanged();

                _colorPointVm.NumberOfColors = value;
                CreateTicItems();
                CreatePaletteImage();
                CreateRectItems();
            }
        }

        private ObservableCollection<RectItem> _items = new();
        public ObservableCollection<RectItem> RectItems
        {
            get => _items;
            set => SetProperty(ref _items, value);
        }

        private int? GetPinNumber(RectItem item)
        {
            for(int i = 0; i < _items.Count; i++)
            {
                if (_items[i].CompareTo(item) == 0)
                    return i + 1;
            }

            return null;
        }

        private RectItem? _selectedRectItem;
        public RectItem? SelectedRectItem
        {
            get => _selectedRectItem;
            set
            {
                if (_selectedRectItem == value)
                    return;

                _selectedRectItem = value;
                OnPropertyChanged();

                if (_selectedRectItem != null)
                {
                    _colorPointVm.ColorPt = RectItem.GetColorPoint(_selectedRectItem);
                    _colorPointVm.PinNumber = GetPinNumber(_selectedRectItem);
                }
            }
        }

        private BitmapImage _image = new();
        public BitmapImage Image
        {
            get => _image;
            set => SetProperty(ref _image, value);
        }

        public int ImageWidth => RectItem.CanvasRight;

        private void UpdatePins()
        {
            List<ColorPoint> pts = new();
            foreach(var item in _items)
            {
                pts.Add(RectItem.GetColorPoint(item));
            }
            var newPalette = PaletteFactory.CreatePaletteFromPins(_palette.NumberOfColors, pts);

            _palette = newPalette;
            AddPalette(newPalette);
            CreatePaletteImage();
            CreateRectItems();
            _onPaletteChanged(_palette);
        }

        private readonly RelayCommand _saveCommand;
        public ICommand SaveCommand => _saveCommand;

        private readonly RelayCommand _openCommand;
        public ICommand OpenCommand => _openCommand;

        protected void OnSave()
        {
            Microsoft.Win32.SaveFileDialog saveFileDialog = new Microsoft.Win32.SaveFileDialog
            {
                Filter = "Palette file (*.jpal)|*.jpal"
            };
            if (saveFileDialog.ShowDialog() == true)
            {
                try
                {
                    string jsonString = JsonConvert.SerializeObject(_palette);

                    //write string to file
                    System.IO.File.WriteAllText(saveFileDialog.FileName, jsonString);
                }
                catch (Exception ex)
                {
                    System.Windows.MessageBox.Show(ex.Message, "Cannot save palette to file");
                }
            }
        }

        protected void OnOpen()
        {
            Microsoft.Win32.OpenFileDialog openFileDialog = new Microsoft.Win32.OpenFileDialog
            {
                Filter = "Fractal3D file (*.jpal|*.jpal|All files (*.*)|*.*"
            };
            if (openFileDialog.ShowDialog() == true)
            {
                try
                {
                    string jsonString = System.IO.File.ReadAllText(openFileDialog.FileName);

                    var palette = JsonConvert.DeserializeObject<Palette>(jsonString);

                    if (palette != null)
                    {
                        _palette = palette;
                        CreatePaletteImage();
                        CreateRectItems();
                        CreateTicItems();           // might not be necessary
                        _onPaletteChanged(_palette);
                    }
                }
                catch (Exception ex)
                {
                    System.Windows.MessageBox.Show(ex.Message, "Cannot load palette from file");
                }
            }
        }

        private ObservableCollection<PaletteItem> _paletteItems = new();
        public ObservableCollection<PaletteItem> PaletteItems
        {
            get => _paletteItems;
            set
            {
                _paletteItems = value;
                OnPropertyChanged();
            }
        }

        private PaletteItem? _selectedPalette;
        public PaletteItem? SelectedPalette
        {
            get => _selectedPalette;
            set
            {
                _selectedPalette = value;
                SelectedPaletteName = _selectedPalette != null ? _selectedPalette.Name : "";

                OnPropertyChanged();
            }
        }

        private string _selectedPaletteName = "";
        public string SelectedPaletteName
        {
            get => _selectedPaletteName;
            set
            {
                _selectedPaletteName = value;
                OnPropertyChanged();
            }
        }

        private readonly RelayCommand _loadPaletteCommand;
        public ICommand LoadPaletteCommand => _loadPaletteCommand;

        protected void OnLoadPalette()
        {
            if (SelectedPalette == null)
                return;

            var newPalette = (Palette)SelectedPalette.Pal.Clone();
            newPalette.NumberOfColors = _palette.NumberOfColors;
            _palette = newPalette;
            AddPalette(newPalette);

            CreatePaletteImage();
            CreateRectItems();
            CreateTicItems();
            _onPaletteChanged(_palette);
        }

        protected bool CanLoadPalette()
        {
            return SelectedPalette != null;
        }

        private readonly RelayCommand _undoPaletteCommand;
        public ICommand UndoPaletteCommand => _undoPaletteCommand;

        protected void OnUndoPalette()
        {
            if (_paletteIndex <= 0)
                return;

            _palette = _oldPalettes[--_paletteIndex];

            CreatePaletteImage();
            CreateRectItems();
            _onPaletteChanged(_palette);
        }

        protected bool CanUndoPalette()
        {
            return _paletteIndex < _oldPalettes.Count && _paletteIndex > 0;
        }

        private readonly RelayCommand _redoPaletteCommand;
        public ICommand RedoPaletteCommand => _redoPaletteCommand;

        protected void OnRedoPalette()
        {
            if (_paletteIndex > _oldPalettes.Count - 2)
                return;

            _palette = _oldPalettes[++_paletteIndex];

            CreatePaletteImage();
            CreateRectItems();
            _onPaletteChanged(_palette);
        }

        protected bool CanRedoPalette()
        {
            return _paletteIndex < _oldPalettes.Count - 1;
        }

        private readonly RelayCommand _changePathCommand;
        public ICommand ChangePathCommand => _changePathCommand;

        protected void OnChangePath()
        {
            using var browserDlg = new FolderBrowserDialog();
            DialogResult result = browserDlg.ShowDialog();

            if (result == DialogResult.OK && !string.IsNullOrWhiteSpace(browserDlg.SelectedPath))
            {
                PalettePath = browserDlg.SelectedPath;
                RegistryUtil.WriteToRegistry(RegistryUtil.PalettePathKey, PalettePath);
                PaletteItems = PaletteFileUtil.LoadPaletteItems(PalettePath);
            }
        }

        private ColorPointVm _colorPointVm;
        public ColorPointVm ColorPointViewModel
        {
            get => _colorPointVm;
            set => SetProperty(ref _colorPointVm, value);
        }

        // IColorPointParent
        #region 

        // This does not save the old palette
        public void UpdateRectItem(ColorPoint cp, int index)
        {
            for (int i = 0; i < _items.Count; i++)
            {
                if (i == index)
                {
                    _items[i].SetColorPoint(cp);
                    UpdatePins();
                    return;
                }                   
            }
        }

        public bool CanAdd(double colorPosition)
        {
            foreach (RectItem item in _items)
            {
                var pt = RectItem.GetColorPoint(item);
                if (Math.Abs(pt.Position - colorPosition) < 0.001)
                    return false;
            }

            return true;
        }

        public void AddRectItem(ColorPoint cp)
        {
            var newPalette = (Palette)_palette.Clone();
            newPalette.NumberOfColors = _palette.NumberOfColors;
            newPalette.AddColorPoint(cp);
            _palette = newPalette;
            AddPalette(newPalette);

            CreatePaletteImage();
            CreateRectItems();
            _onPaletteChanged(_palette);

            _colorPointVm.PinNumber = null;
        }

        public void DeleteRectItem(int index)
        {
            var pts = _items.Where((_, i) => i != index).Select(RectItem.GetColorPoint).ToList();
            var newPalette = PaletteFactory.CreatePaletteFromPins(_palette.NumberOfColors, pts);
            _palette = newPalette;
            AddPalette(newPalette);

            CreatePaletteImage();
            CreateRectItems();
            _onPaletteChanged(_palette);

            _colorPointVm.PinNumber = null;
        }

        #endregion

        private DisplayInfoVm _displayInfoVm;
        public DisplayInfoVm DisplayInfoViewModel
        {
            get => _displayInfoVm;
            set => SetProperty(ref _displayInfoVm, value);
        }

        private void CreateTicItems()
        {
            var ticItems = new ObservableCollection<TicItem>();
            var textItems = new ObservableCollection<TextItem>();

            var nColors = NumberOfColors;
            var nTics = nColors > 20 ? 11 : 3;
            var relativeDistance = 1.0/(double)(nTics-1);

            for (int i = 0; i < nTics; i++)
            {
                var relPos = i * relativeDistance;
                ticItems.Add(new TicItem(relPos, nColors));
                textItems.Add(new TextItem(relPos, nColors));
            }

            TicItems = ticItems;
            TextItems = textItems;
        }

        private ObservableCollection<TicItem> _ticItems = new();
        public ObservableCollection<TicItem> TicItems
        {
            get => _ticItems;
            set => SetProperty(ref _ticItems, value);
        }

        private ObservableCollection<TextItem> _textItems = new();

        public ObservableCollection<TextItem> TextItems
        {
            get => _textItems;
            set => SetProperty(ref _textItems, value);
        }
    }


}

