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
using MouseEventArgs = System.Windows.Input.MouseEventArgs;
using System.Windows.Controls;

namespace Fractal3d
{
    public class PaletteVm : ViewModelBase, IColorPointVmParent
    {
        #region fields

        private Palette _palette;
        private readonly Action<Palette> _onPaletteChanged;
        private readonly List<Palette> _oldPalettes = new();
        private int _paletteIndex = -1;
        private DisplayInfo _displayInfo;
        private double _mousePosX = -1.0;
        private string _palettePath = "";
        private ObservableCollection<RectItem> _items = new();
        private RectItem? _selectedRectItem;
        private BitmapImage _image = new();
        private double _windowWidth = 1000;
        private double _realWidth = 1000;
        private readonly RelayCommand _saveCommand;
        private readonly RelayCommand _openCommand;
        private readonly RelayCommand _loadPaletteCommand;
        private readonly RelayCommand _undoPaletteCommand;
        private readonly RelayCommand _redoPaletteCommand;
        private readonly RelayCommand _changePathCommand;
        private ObservableCollection<PaletteItem> _paletteItems = new();
        private PaletteItem? _selectedPalette;
        private string _selectedPaletteName = "";
        private ColorPointVm _colorPointVm;
        private ObservableCollection<TicItem> _ticItems = new();
        private ObservableCollection<TextItem> _textItems = new();

        #endregion

        #region construction

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
        public PaletteVm(Palette palette, Action<Palette> paletteChanged, DisplayInfo displayInfo)
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
        {
            PalettePath = RegistryUtil.ReadStringFromRegistry(RegistryUtil.PalettePathKey);
            _palette = palette;
            _displayInfo = displayInfo;
            AddPalette(palette);
            _onPaletteChanged = paletteChanged;
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

#pragma warning disable CS8604 // Possible null reference argument.
            PaletteVmLeftMouseUpCommand = new RelayCommand(param => ExecuteLeftMouseUp((param as MouseEventArgs)));
#pragma warning restore CS8604 // Possible null reference argument.
        }

        // This is in the constructor region because it allows us to bypass the constructor
        public void SetNewPalette(Palette palette, DisplayInfo displayInfo)
        {
            _palette = palette;
            _displayInfo = displayInfo;
            AddPalette(palette);
            ColorPointViewModel = new ColorPointVm(this);
            CreatePaletteImage();
            CreateRectItems();
            CreateTicItems();
            PaletteItems = PaletteFileUtil.LoadPaletteItems(PalettePath);
        }

        #endregion

        #region privateMethods

        private void AddPalette(Palette pal)
        {
            _oldPalettes.Add(pal);
            _paletteIndex = _oldPalettes.Count - 1;
        }

        private void CreateRectItems()
        {
            var items = new ObservableCollection<RectItem>();

            var ptList = _palette.GetCopyOfColorPointList();
            foreach (var pin in ptList)
            {
                var colorPt = pin.Value;
                items.Add(RectItem.MakeColorRect(colorPt, items, UpdatePins, r => SelectedRectItem = r, _realWidth));
            }

            RectItems = items;
        }

        private void CreateTicItems()
        {
            var ticItems = new ObservableCollection<TicItem>();
            var textItems = new ObservableCollection<TextItem>();

            var nColors = NumberOfColors;
            var nTics = nColors > 20 ? 11 : 3;
            var relativeDistance = 1.0 / (double)(nTics - 1);

            for (int i = 0; i < nTics; i++)
            {
                var relPos = i * relativeDistance;
                ticItems.Add(new TicItem(relPos, nColors, _realWidth));
                textItems.Add(new TextItem(relPos, nColors, _realWidth));
            }

            TicItems = ticItems;
            TextItems = textItems;
        }

        private void CreatePaletteImage()
        {
            var bmp = PaletteImage.CreateBitmapForPalette(_palette, _displayInfo);
            Image = ImageUtil.BitmapToImageSource(bmp);
        }

        private int? GetPinNumber(RectItem item)
        {
            for (int i = 0; i < _items.Count; i++)
            {
                if (_items[i].CompareTo(item) == 0)
                    return i + 1;
            }

            return null;
        }

        private void UpdatePins()
        {
            List<ColorPoint> pts = new();
            foreach (var item in _items)
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

        private void OnSave()
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

        private void OnOpen()
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
                        palette.NumberOfColors = NumberOfColors;
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

        private void OnLoadPalette()
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

        private bool CanLoadPalette()
        {
            return SelectedPalette != null;
        }

        private void ExecuteLeftMouseUp(MouseEventArgs e)
        {
            var canvas = e.Source as Canvas;
            if (canvas == null)
                return;

            var canvasPos = e.GetPosition(canvas);
            if (canvasPos.Y is < RectItem.ItemTop or > RectItem.ItemTop + RectItem.ItemHeight)
            {
                return;
            }

            if (canvas.Width == 0)
                return;

            _mousePosX = canvasPos.X / canvas.Width;
        }

        private void OnUndoPalette()
        {
            if (_paletteIndex <= 0)
                return;

            _palette = _oldPalettes[--_paletteIndex];

            CreatePaletteImage();
            CreateRectItems();
            _onPaletteChanged(_palette);
        }

        private bool CanUndoPalette()
        {
            return _paletteIndex < _oldPalettes.Count && _paletteIndex > 0;
        }

        private void OnRedoPalette()
        {
            if (_paletteIndex > _oldPalettes.Count - 2)
                return;

            _palette = _oldPalettes[++_paletteIndex];

            CreatePaletteImage();
            CreateRectItems();
            _onPaletteChanged(_palette);
        }

        private bool CanRedoPalette()
        {
            return _paletteIndex < _oldPalettes.Count - 1;
        }

        private void OnChangePath()
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

        // Use the ColorPoint position if it's not close to an existing rect.
        // Otherwise use the mousePos if it's not close to an existing rect.
        // If both are in a rect return null
        private double? GetBestColorPosition(double colorPosition)
        {
            var found = false;
            foreach (var item in _items)
            {
                var pt = RectItem.GetColorPoint(item);
                if (Math.Abs(pt.Position - colorPosition) < 0.001)
                {
                    found = true;
                    break;
                }
            }

            if (found == false)
                return colorPosition;

            // The position has to be within the canvas width
            if (_mousePosX < 0 || _mousePosX > 1.0)
                return null;

            foreach (var item in _items)
            {
                var pt = RectItem.GetColorPoint(item);
                if (Math.Abs(pt.Position - _mousePosX) < 0.001)
                {
                    return null;
                }
            }

            return _mousePosX;
        }

        #endregion

        #region properties

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
                _palette.NumberOfColors = value;
                OnPropertyChanged();

                _colorPointVm.NumberOfColors = value;
                CreateTicItems();
                CreatePaletteImage();
                CreateRectItems();
            }
        }

        public ObservableCollection<RectItem> RectItems
        {
            get => _items;
            set => SetProperty(ref _items, value);
        }

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

        public BitmapImage Image
        {
            get => _image;
            set => SetProperty(ref _image, value);
        }

        public double WindowWidth
        {
            get => _windowWidth;
            set
            {
                if (Math.Abs(value - _windowWidth) < 0.1)
                    return;

                if (value < 500)
                    value = 500;

                SetProperty(ref _windowWidth, value);
                RealWidth = _windowWidth - 64;      // margins 2*(4+4+2+20+2?) - don't know what an extra 2 is from
            }
        }

        public double RealWidth
        {
            get => _realWidth;
            set
            {
                if (Math.Abs(value - _realWidth) < 0.1)
                    return;

                if (value < 500)
                    value = 500;

                SetProperty(ref _realWidth, value);

                CreateRectItems();
                CreateTicItems();
            }
        }

        public ObservableCollection<PaletteItem> PaletteItems
        {
            get => _paletteItems;
            set
            {
                _paletteItems = value;
                OnPropertyChanged();
            }
        }

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

        public string SelectedPaletteName
        {
            get => _selectedPaletteName;
            set
            {
                _selectedPaletteName = value;
                OnPropertyChanged();
            }
        }

        public ColorPointVm ColorPointViewModel
        {
            get => _colorPointVm;
            set => SetProperty(ref _colorPointVm, value);
        }

        public ObservableCollection<TicItem> TicItems
        {
            get => _ticItems;
            set => SetProperty(ref _ticItems, value);
        }

        public ObservableCollection<TextItem> TextItems
        {
            get => _textItems;
            set => SetProperty(ref _textItems, value);
        }

        public ICommand SaveCommand => _saveCommand;

        public ICommand OpenCommand => _openCommand;

        public ICommand LoadPaletteCommand => _loadPaletteCommand;

        public RelayCommand PaletteVmLeftMouseUpCommand { get; }

        public ICommand UndoPaletteCommand => _undoPaletteCommand;

        public ICommand RedoPaletteCommand => _redoPaletteCommand;

        public ICommand ChangePathCommand => _changePathCommand;

        #endregion

        // IColorPointParent
        #region IColorPointParent

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
            if (GetBestColorPosition(colorPosition) == null)
                return false;

            return true;
        }

        public void AddRectItem(ColorPoint cp)
        {
            var newPalette = (Palette)_palette.Clone();
            newPalette.NumberOfColors = _palette.NumberOfColors;
            var pos = GetBestColorPosition(cp.Position);
            if (pos == null)
                return;

            cp.Position = pos.Value;
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

    }


}

