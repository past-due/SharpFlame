

using System;
using System.Collections.ObjectModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using Ninject;
using NLog;
using SharpFlame.Bitmaps;
using SharpFlame.Core.Extensions;
using SharpFlame.Mapping.IO.Minimap;
using SharpFlame.Controls;
using SharpFlame.Core;
using SharpFlame.Core.Collections;
using SharpFlame.Core.Domain;
using SharpFlame.Domain;
using SharpFlame.FileIO;
using SharpFlame.Generators;
using SharpFlame.Mapping;
using SharpFlame.Mapping.IO.Heightmap;
using SharpFlame.Mapping.IO.LND;
using SharpFlame.Mapping.IO.TTP;
using SharpFlame.Mapping.Objects;
using SharpFlame.Mapping.Script;
using SharpFlame.Mapping.Tiles;
using SharpFlame.Mapping.Tools;
using SharpFlame.Maths;
using SharpFlame.MouseTools;
using SharpFlame.Painters;
using SharpFlame.Util;
using SharpFlame.Settings;



namespace SharpFlame
{
    public partial class frmMain
    {
        private static readonly Logger logger = LogManager.GetCurrentClassLogger();

        public class clsMaps : ConnectedList<Map, frmMain>
        {
            private Map _MainMap;

            public clsMaps(frmMain Owner) : base(Owner)
            {
            }

            public Map MainMap
            {
                get { return _MainMap; }
                set
                {
                    if ( value == _MainMap )
                    {
                        return;
                    }
                    var MainForm = Owner;
                    MainForm.MainMapBeforeChanged();
                    if ( value == null )
                    {
                        _MainMap = null;
                    }
                    else
                    {
                        _MainMap = value;
                    }
                    MainForm.MainMapAfterChanged();
                }
            }

            public override void Add(IConnectedListItem<Map, frmMain> newItem)
            {
                var NewMap = newItem.Item;

                if ( !NewMap.ReadyForUserInput )
                {
                    NewMap.InitializeUserInput();
                }

//                NewMap.MapViewTabPage = new TabPage();
//                NewMap.MapViewTabPage.Tag = NewMap;

                NewMap.SetTabText();

                base.Add(newItem);

                Owner.MapViewControl.UpdateTabs();
            }

            public override void Remove(int position)
            {
                var Map = this[position];

                base.Remove(position);

                if ( Map == _MainMap )
                {
                    var NewNum = Math.Min(Convert.ToInt32(Owner.MapViewControl.tabMaps.SelectedIndex), Count - 1);
                    if ( NewNum < 0 )
                    {
                        MainMap = null;
                    }
                    else
                    {
                        MainMap = this[NewNum];
                    }
                }

                Owner.MapViewControl.UpdateTabs();
            }
        }

        private readonly clsMaps _LoadedMaps;

        public MapViewControl MapViewControl;
        public TextureViewControl TextureViewControl;

        public Body[] cboBody_Objects;
        public Propulsion[] cboPropulsion_Objects;
        public Turret[] cboTurret_Objects;

        public byte[] HeightSetPalette = new byte[8];

        public ConnectedList<UnitTypeBase, frmMain> SelectedObjectTypes;

        public TextureTerrainAction TextureTerrainAction = TextureTerrainAction.Reinterpret;

        public FillCliffAction FillCliffAction = FillCliffAction.Ignore;

        public Timer tmrKey;
        public Timer tmrTool;

        public PlayerNumControl NewPlayerNumControl;
        public PlayerNumControl ObjectPlayerNumControl;

        public BrushControl ctrlTextureBrush;
        public BrushControl ctrlTerrainBrush;
        public BrushControl ctrlCliffRemoveBrush;
        public BrushControl ctrlHeightBrush;

        public KeyboardManager KeyboardManager { get; set; }

        public frmMain()
        {
            _LoadedMaps = new clsMaps(this);
            SelectedObjectTypes = new ConnectedList<UnitTypeBase, frmMain>(this);

            KeyboardManager = App.KeyboardManager;

            InitializeComponent();

            MapViewControl = new MapViewControl(this);
            TextureViewControl = new TextureViewControl(this);

            Program.frmGeneratorInstance = new frmGenerator(this);

            tmrKey = new Timer();
            tmrKey.Tick += tmrKey_Tick;
            tmrKey.Interval = 30;

            tmrTool = new Timer();
            tmrTool.Tick += tmrTool_Tick;
            tmrTool.Interval = 100;

            NewPlayerNumControl = new PlayerNumControl();
            ObjectPlayerNumControl = new PlayerNumControl();
        }

        private Result LoadInterfaceImages()
        {
          /*  var ReturnResult = new Result("Loading interface images", false);
            logger.Info("Loading interface images");

            Bitmap InterfaceImage_DisplayAutoTexture = null;
            Bitmap InterfaceImage_DrawTileOrientation = null;
            Bitmap InterfaceImage_QuickSave = null;
            Bitmap InterfaceImage_Selection = null;
            Bitmap InterfaceImage_ObjectSelect = null;
            Bitmap InterfaceImage_SelectionCopy = null;
            Bitmap InterfaceImage_SelectionFlipX = null;
            Bitmap InterfaceImage_SelectionRotateClockwise = null;
            Bitmap InterfaceImage_SelectionRotateCounterClockwise = null;
            Bitmap InterfaceImage_SelectionPaste = null;
            Bitmap InterfaceImage_SelectionPasteOptions = null;
            Bitmap InterfaceImage_Gateways = null;

            /*InterfaceImage_DisplayAutoTexture = Resources.displayautotexture;
            InterfaceImage_DrawTileOrientation = Resources.drawtileorientation;
            InterfaceImage_QuickSave = Resources.save;
            InterfaceImage_Selection = Resources.selection;
            InterfaceImage_ObjectSelect = Resources.objectsselect;
            InterfaceImage_SelectionCopy = Resources.selectioncopy;
            InterfaceImage_SelectionFlipX = Resources.selectionflipx;
            InterfaceImage_SelectionPaste = Resources.selectionpaste;
            InterfaceImage_SelectionPasteOptions = Resources.selectionpasteoptions;
            InterfaceImage_SelectionRotateClockwise = Resources.selectionrotateclockwise;
            InterfaceImage_SelectionRotateCounterClockwise = Resources.selectionrotateanticlockwise;
            InterfaceImage_Gateways = Resources.gateways;

            var InterfaceImage_Problem = Resources.problem;
            var InterfaceImage_Warning = Resources.warning;

            modWarnings.WarningImages.ImageSize = new Size(16, 16);
            if ( InterfaceImage_Problem != null )
            {
                modWarnings.WarningImages.Images.Add("problem", InterfaceImage_Problem);
            }
            if ( InterfaceImage_Warning != null )
            {
                modWarnings.WarningImages.Images.Add("warning", InterfaceImage_Warning);
            }

            tsbDrawAutotexture.Image = InterfaceImage_DisplayAutoTexture;
            tsbDrawTileOrientation.Image = InterfaceImage_DrawTileOrientation;
            tsbSave.Image = InterfaceImage_QuickSave;
            tsbSelection.Image = InterfaceImage_Selection;
            tsbSelectionObjects.Image = InterfaceImage_ObjectSelect;
            tsbSelectionCopy.Image = InterfaceImage_SelectionCopy;
            tsbSelectionFlipX.Image = InterfaceImage_SelectionFlipX;
            tsbSelectionRotateClockwise.Image = InterfaceImage_SelectionRotateClockwise;
            tsbSelectionRotateCounterClockwise.Image = InterfaceImage_SelectionRotateCounterClockwise;
            tsbSelectionPaste.Image = InterfaceImage_SelectionPaste;
            tsbSelectionPasteOptions.Image = InterfaceImage_SelectionPasteOptions;
            tsbGateways.Image = InterfaceImage_Gateways;
            btnTextureAnticlockwise.Image = InterfaceImage_SelectionRotateCounterClockwise;
            btnTextureClockwise.Image = InterfaceImage_SelectionRotateClockwise;
            btnTextureFlipX.Image = InterfaceImage_SelectionFlipX;

            return ReturnResult;*/
	        return null;
        }

        public void frmMain_FormClosing(object sender, FormClosingEventArgs e)
        {
            var ChangedPrompt = false;

            var Map = default(Map);
            foreach ( var tempLoopVar_Map in _LoadedMaps )
            {
                Map = tempLoopVar_Map;
                if ( Map.ChangedSinceSave )
                {
                    ChangedPrompt = true;
                    break;
                }
            }
            if ( ChangedPrompt )
            {
                var QuitPrompt = new frmQuit();
                var QuitResult = QuitPrompt.ShowDialog(Program.frmMainInstance);
                switch ( QuitResult )
                {
                    case DialogResult.Yes:
                        while ( _LoadedMaps.Count > 0 )
                        {
                            var RemoveMap = _LoadedMaps[0];
                            SetMainMap(RemoveMap);
                            if ( !RemoveMap.ClosePrompt() )
                            {
                                e.Cancel = true;
                                return;
                            }
                            RemoveMap.Deallocate();
                        }
                        break;
                    case DialogResult.No:
                        break;

                    case DialogResult.Cancel:
                        e.Cancel = true;
                        return;
                }
            }
        }

        public string InitializeStatus = "";

        public void Initialize(object sender, EventArgs e)
        {
            if ( App.ProgramInitialized )
            {
                Debugger.Break();
                return;
            }
            if ( !(MapViewControl.IsGLInitialized && TextureViewControl.IsGLInitialized) )
            {
                return;
            }

            App.ProgramInitialized = true;

            Program.InitializeDelay.Enabled = false;
            Program.InitializeDelay.Tick -= Initialize;
            Program.InitializeDelay.Dispose();
            Program.InitializeDelay = null;

            Program.InitializeResult.Add(LoadInterfaceImages());

            Matrix3DMath.MatrixSetToPY(App.SunAngleMatrix, new Angles.AnglePY(-22.5D * MathUtil.RadOf1Deg, 157.5D * MathUtil.RadOf1Deg));

            NewPlayerNumControl.Left = 112;
            NewPlayerNumControl.Top = 10;
            Panel1.Controls.Add(NewPlayerNumControl);

            ObjectPlayerNumControl.Left = 72;
            ObjectPlayerNumControl.Top = 60;
            ObjectPlayerNumControl.Target = new clsUnitGroupContainer();
            ObjectPlayerNumControl.Target.Changed += tabPlayerNum_SelectedIndexChanged;
            Panel14.Controls.Add(ObjectPlayerNumControl);

            ctrlTextureBrush = new BrushControl(App.TextureBrush);
            pnlTextureBrush.Controls.Add(ctrlTextureBrush);

            ctrlTerrainBrush = new BrushControl(App.TerrainBrush);
            pnlTerrainBrush.Controls.Add(ctrlTerrainBrush);

            ctrlCliffRemoveBrush = new BrushControl(App.CliffBrush);
            pnlCliffRemoveBrush.Controls.Add(ctrlCliffRemoveBrush);

            ctrlHeightBrush = new BrushControl(App.HeightBrush);
            pnlHeightSetBrush.Controls.Add(ctrlHeightBrush);

            CreateTileTypes();

            for ( var i = 0; i <= 15; i++ )
            {
                App.PlayerColour[i] = new Player();
            }
            App.PlayerColour[0].Colour.Red = 0.0F;
            App.PlayerColour[0].Colour.Green = 96.0F / 255.0F;
            App.PlayerColour[0].Colour.Blue = 0.0F;
            App.PlayerColour[1].Colour.Red = 160.0F / 255.0F;
            App.PlayerColour[1].Colour.Green = 112.0F / 255.0F;
            App.PlayerColour[1].Colour.Blue = 0.0F;
            App.PlayerColour[2].Colour.Red = 128.0F / 255.0F;
            App.PlayerColour[2].Colour.Green = 128.0F / 255.0F;
            App.PlayerColour[2].Colour.Blue = 128.0F / 255.0F;
            App.PlayerColour[3].Colour.Red = 0.0F;
            App.PlayerColour[3].Colour.Green = 0.0F;
            App.PlayerColour[3].Colour.Blue = 0.0F;
            App.PlayerColour[4].Colour.Red = 128.0F / 255.0F;
            App.PlayerColour[4].Colour.Green = 0.0F;
            App.PlayerColour[4].Colour.Blue = 0.0F;
            App.PlayerColour[5].Colour.Red = 32.0F / 255.0F;
            App.PlayerColour[5].Colour.Green = 48.0F / 255.0F;
            App.PlayerColour[5].Colour.Blue = 96.0F / 255.0F;
            App.PlayerColour[6].Colour.Red = 144.0F / 255.0F;
            App.PlayerColour[6].Colour.Green = 0.0F;
            App.PlayerColour[6].Colour.Blue = 112 / 255.0F;
            App.PlayerColour[7].Colour.Red = 0.0F;
            App.PlayerColour[7].Colour.Green = 128.0F / 255.0F;
            App.PlayerColour[7].Colour.Blue = 128.0F / 255.0F;
            App.PlayerColour[8].Colour.Red = 128.0F / 255.0F;
            App.PlayerColour[8].Colour.Green = 192.0F / 255.0F;
            App.PlayerColour[8].Colour.Blue = 0.0F;
            App.PlayerColour[9].Colour.Red = 176.0F / 255.0F;
            App.PlayerColour[9].Colour.Green = 112.0F / 255.0F;
            App.PlayerColour[9].Colour.Blue = 112.0F / 255.0F;
            App.PlayerColour[10].Colour.Red = 224.0F / 255.0F;
            App.PlayerColour[10].Colour.Green = 224.0F / 255.0F;
            App.PlayerColour[10].Colour.Blue = 224.0F / 255.0F;
            App.PlayerColour[11].Colour.Red = 32.0F / 255.0F;
            App.PlayerColour[11].Colour.Green = 32.0F / 255.0F;
            App.PlayerColour[11].Colour.Blue = 255.0F / 255.0F;
            App.PlayerColour[12].Colour.Red = 0.0F;
            App.PlayerColour[12].Colour.Green = 160.0F / 255.0F;
            App.PlayerColour[12].Colour.Blue = 0.0F;
            App.PlayerColour[13].Colour.Red = 64.0F / 255.0F;
            App.PlayerColour[13].Colour.Green = 0.0F;
            App.PlayerColour[13].Colour.Blue = 0.0F;
            App.PlayerColour[14].Colour.Red = 16.0F / 255.0F;
            App.PlayerColour[14].Colour.Green = 0.0F;
            App.PlayerColour[14].Colour.Blue = 64.0F / 255.0F;
            App.PlayerColour[15].Colour.Red = 64.0F / 255.0F;
            App.PlayerColour[15].Colour.Green = 96.0F / 255.0F;
            App.PlayerColour[15].Colour.Blue = 0.0F;
            for ( var i = 0; i <= 15; i++ )
            {
                App.PlayerColour[i].CalcMinimapColour();
            }

            App.MinimapFeatureColour.Red = 0.5F;
            App.MinimapFeatureColour.Green = 0.5F;
            App.MinimapFeatureColour.Blue = 0.5F;

//            SettingsManager.UpdateSettings(SettingsManager.InitializeSettings);
//            SettingsManager.InitializeSettings = null;
//
//            if ( SettingsManager.Settings.DirectoriesPrompt )
//            {
//                Program.frmOptionsInstance = new frmOptions();
//                Program.frmOptionsInstance.FormClosing += Program.frmOptionsInstance.frmOptions_FormClosing;
//                if ( Program.frmOptionsInstance.ShowDialog() == DialogResult.Cancel )
//                {
//                    Application.Exit();
//                }
//            }

            // var tilesetNum = Convert.ToInt32(SettingsManager.Settings.get_Value(SettingsManager.Setting_DefaultTilesetsPathNum));
//            var tilesetsList = (List<string>)SettingsManager.Settings.GetValue(SettingsManager.SettingTilesetDirectories);
//            foreach (var path in tilesetsList) {
//                if (path != null && path != "") {
//                    InitializeStatus = "Loading tilesets";
//                    Program.InitializeResult.Add(App.LoadTilesets(PathUtil.EndWithPathSeperator(path)));
//                    InitializeStatus = "";
//                }
//            }

            cboTileset_Update(-1);

            Program.InitializeResult.Add(NoTile_Texture_Load());
            cboTileType_Update();

            App.CreateTemplateDroidTypes(); //do before loading data

//            App.ObjectData = new clsObjectData();
//            // var ObjectDataNum = Convert.ToInt32(SettingsManager.Settings.get_Value(SettingsManager.Setting_DefaultObjectDataPathNum));
//            var objectDataList = (List<string>)(SettingsManager.Settings.GetValue(SettingsManager.SettingObjectDataDirectories));
//            foreach (var path in objectDataList) {
//                if (path != null && path != "") {
//                    InitializeStatus = "Loading object data";
//                    Program.InitializeResult.Add(App.ObjectData.LoadDirectory(path));
//                    InitializeStatus = "";
//                }
//            }

            DefaultGenerator.CreateGeneratorTilesets();
            PainterFactory.CreatePainterArizona();
            PainterFactory.CreatePainterUrban();
            PainterFactory.CreatePainterRockies();

            Components_Update();

            MapViewControl.Dock = DockStyle.Fill;
            pnlView.Controls.Add(MapViewControl);

            App.VisionRadius_2E = 10;
            App.VisionRadius_2E_Changed();

            HeightSetPalette[0] = 0;
            HeightSetPalette[1] = 85;
            HeightSetPalette[2] = 170;
            HeightSetPalette[3] = 255;
            HeightSetPalette[4] = 64;
            HeightSetPalette[5] = 128;
            HeightSetPalette[6] = 192;
            HeightSetPalette[7] = 255;
            for ( var A = 0; A <= 7; A++ )
            {
                tabHeightSetL.TabPages[A].Text = HeightSetPalette[A].ToStringInvariant();
                tabHeightSetR.TabPages[A].Text = HeightSetPalette[A].ToStringInvariant();
            }
            tabHeightSetL.SelectedIndex = 1;
            tabHeightSetR.SelectedIndex = 0;
            tabHeightSetL_SelectedIndexChanged(null, null);
            tabHeightSetR_SelectedIndexChanged(null, null);

            if ( App.CommandLinePaths.Count >= 1 )
            {
                var Path = "";
                var LoadResult = new Result("Loading startup command-line maps", false);
                logger.Info("Loading startup command-line maps");
                foreach ( var tempLoopVar_Path in App.CommandLinePaths )
                {
                    Path = tempLoopVar_Path;
                    LoadResult.Take(LoadMap(Path));
                }
                App.ShowWarnings(LoadResult);
            }

            TextureViewControl.Dock = DockStyle.Fill;
            TableLayoutPanel6.Controls.Add(TextureViewControl, 0, 1);

            MainMapAfterChanged();

            MapViewControl.DrawViewSetEnabled(true);
            TextureViewControl.DrawView_SetEnabled(true);

            WindowState = FormWindowState.Maximized;

            Activated += Me_Activated;

            tmrKey.Enabled = true;
            tmrTool.Enabled = true;

            App.ShowWarnings(Program.InitializeResult);

            App.ProgramInitializeFinished = true;
        }

        public void Me_Activated(Object eventSender, EventArgs eventArgs)
        {
            MapViewControl.DrawViewLater();
            TextureViewControl.DrawViewLater();
        }

        public void Me_LostFocus(Object eventSender, EventArgs eventArgs)
        {
            App.ViewKeyDown_Clear();
        }

        private void tmrKey_Tick(Object sender, EventArgs e)
        {
        }

        public void Load_Map_Prompt()
        {
            var dialog = new OpenFileDialog();

            dialog.InitialDirectory = App.SettingsManager.OpenPath;
            dialog.FileName = "";
            dialog.Filter = "Warzone Map Files (*.fmap, *.wz, *.gam, *.lnd)|*.fmap;*.wz;*.gam;*.lnd|All Files (*.*)|*.*";
            dialog.Multiselect = true;
            if ( dialog.ShowDialog(this) != DialogResult.OK )
            {
                return;
            }
            App.SettingsManager.OpenPath = Path.GetDirectoryName(dialog.FileName);
            var fileName = "";
            var Results = new Result("Loading maps", false);
            foreach ( var tempLoopVar_FileName in dialog.FileNames )
            {
                fileName = tempLoopVar_FileName;
                logger.Info("Loading map '{0}'", fileName);
                Results.Take(LoadMap(fileName));
            }
            App.ShowWarnings(Results);
        }

        public void Load_Heightmap_Prompt()
        {
            var Dialog = new OpenFileDialog();

            Dialog.InitialDirectory = App.SettingsManager.OpenPath;
            Dialog.FileName = "";
            Dialog.Filter = "Image Files (*.bmp, *.png)|*.bmp;*.png|All Files (*.*)|*.*";
            if ( Dialog.ShowDialog(this) != DialogResult.OK )
            {
                return;
            }
            App.SettingsManager.OpenPath = Path.GetDirectoryName(Dialog.FileName);

            Bitmap HeightmapBitmap = null;
            var Result = BitmapUtil.LoadBitmap(Dialog.FileName, ref HeightmapBitmap);
            if ( !Result.Success )
            {
                MessageBox.Show("Failed to load image: " + Result.Problem);
                return;
            }

            var Map = MainMap;
            Map ApplyToMap = null;
            if ( Map == null )
            {
            }
            else if ( MessageBox.Show("Apply heightmap to the current map?", "", MessageBoxButtons.YesNo) == DialogResult.Yes )
            {
                ApplyToMap = Map;
            }
            if ( ApplyToMap == null )
            {
                // ApplyToMap = new Map(new XYInt(HeightmapBitmap.Width - 1, HeightmapBitmap.Height - 1));
                ApplyToMap = App.Kernel.Get<Map>().Create(new XYInt(HeightmapBitmap.Width - 1, HeightmapBitmap.Height - 1));
            }

            var X = 0;
            var Y = 0;
            var PixelColor = new Color();

            for ( Y = 0; Y <= Math.Min(HeightmapBitmap.Height - 1, ApplyToMap.Terrain.TileSize.Y); Y++ )
            {
                for ( X = 0; X <= Math.Min(HeightmapBitmap.Width - 1, ApplyToMap.Terrain.TileSize.X); X++ )
                {
                    PixelColor = HeightmapBitmap.GetPixel(X, Y);
                    ApplyToMap.Terrain.Vertices[X, Y].Height =
                        (byte)Math.Min(Math.Round(Convert.ToDouble(((PixelColor.R) + PixelColor.G + PixelColor.B) / 3.0D)), byte.MaxValue);
                }
            }

            if ( ApplyToMap == Map )
            {
                ApplyToMap.SectorTerrainUndoChanges.SetAllChanged();
                ApplyToMap.SectorUnitHeightsChanges.SetAllChanged();
                ApplyToMap.SectorGraphicsChanges.SetAllChanged();
                ApplyToMap.Update(null);
                ApplyToMap.UndoStepCreate("Apply heightmap");
            }
            else
            {
                NewMainMap(ApplyToMap);
                ApplyToMap.Update(null);
            }

            View_DrawViewLater();
        }

        public void Load_TTP_Prompt()
        {
            var Dialog = new OpenFileDialog();

            var Map = MainMap;

            if ( Map == null )
            {
                return;
            }

            Dialog.InitialDirectory = App.SettingsManager.OpenPath;
            Dialog.FileName = "";
            Dialog.Filter = "TTP Files (*.ttp)|*.ttp|All Files (*.*)|*.*";
            if ( !(Dialog.ShowDialog(this) == DialogResult.OK) )
            {
                return;
            }
            App.SettingsManager.OpenPath = Path.GetDirectoryName(Dialog.FileName);
            var ttpLoader = App.Kernel.Get<TTPLoader>();
            var result = ttpLoader.Load(Dialog.FileName, Map);
            if (!result.HasProblems && !result.HasWarnings)
            {
                TextureViewControl.DrawViewLater();
            }
            else
            {
                App.ShowWarnings(result.ToResult());
            }
        }

        public void cboTileset_Update(int NewSelectedIndex)
        {
            var A = 0;

            cboTileset.Items.Clear();
            for ( A = 0; A <= App.Tilesets.Count - 1; A++ )
            {
                cboTileset.Items.Add(App.Tilesets[A].Name);
            }
            cboTileset.SelectedIndex = NewSelectedIndex;
        }

        public void MainMapTilesetChanged()
        {
            var A = 0;
            var Map = MainMap;

            if ( Map == null )
            {
                cboTileset.SelectedIndex = -1;
                return;
            }

            for ( A = 0; A <= App.Tilesets.Count - 1; A++ )
            {
                if ( App.Tilesets[A] == Map.Tileset )
                {
                    break;
                }
            }
            if ( A == App.Tilesets.Count )
            {
                cboTileset.SelectedIndex = -1;
            }
            else
            {
                cboTileset.SelectedIndex = A;
            }
        }

        public void cboTileset_SelectedIndexChanged(Object sender, EventArgs e)
        {
            var NewTileset = default(Tileset);
            var Map = MainMap;

            if ( Map == null )
            {
                return;
            }

            if ( cboTileset.SelectedIndex < 0 )
            {
                NewTileset = null;
            }
            else
            {
                NewTileset = App.Tilesets[cboTileset.SelectedIndex];
            }
            if ( NewTileset != Map.Tileset )
            {
                Map.Tileset = NewTileset;
                if ( Map.Tileset != null )
                {
                    App.SelectedTextureNum = Math.Min(0, Map.Tileset.Tiles.Count - 1);
                }
                Map.TileType_Reset();

                Map.SetPainterToDefaults();
                PainterTerrains_Refresh(-1, -1);

                Map.SectorGraphicsChanges.SetAllChanged();
                Map.Update(null);
                View_DrawViewLater();
                TextureViewControl.ScrollUpdate();
                TextureViewControl.DrawViewLater();
            }
        }

        public void PainterTerrains_Refresh(int Terrain_NewSelectedIndex, int Road_NewSelectedIndex)
        {
            lstAutoTexture.Items.Clear();
            lstAutoRoad.Items.Clear();

            var Map = MainMap;
            if ( Map == null )
            {
                return;
            }

            for ( var a = 0; a < Map.Painter.TerrainCount; a++ )
            {
                lstAutoTexture.Items.Add(Map.Painter.Terrains[a].Name);
            }
            for ( var a = 0; a < Map.Painter.RoadCount; a++ )
            {
                lstAutoRoad.Items.Add(Map.Painter.Roads[a].Name);
            }
            lstAutoTexture.SelectedIndex = Terrain_NewSelectedIndex;
            lstAutoRoad.SelectedIndex = Road_NewSelectedIndex;
        }

        public void TabControl_SelectedIndexChanged(object sender, EventArgs e)
        {
            if ( TabControl.SelectedTab == tpTextures )
            {
                App.ToolOptions.MouseTool = MouseTool.TextureBrush;
                TextureViewControl.DrawViewLater();
            }
            else if ( TabControl.SelectedTab == tpHeight )
            {
                if ( rdoHeightSet.Checked )
                {
                    App.ToolOptions.MouseTool = MouseTool.HeightSetBrush;
                }
                else if ( rdoHeightSmooth.Checked )
                {
                    App.ToolOptions.MouseTool = MouseTool.HeightSmoothBrush;
                }
                else if ( rdoHeightChange.Checked )
                {
                    App.ToolOptions.MouseTool = MouseTool.HeightChangeBrush;
                }
            }
            else if ( TabControl.SelectedTab == tpAutoTexture )
            {
                if ( rdoAutoTexturePlace.Checked )
                {
                    App.ToolOptions.MouseTool = MouseTool.TerrainBrush;
                }
                else if ( rdoAutoRoadPlace.Checked )
                {
                    App.ToolOptions.MouseTool = MouseTool.RoadPlace;
                }
                else if ( rdoCliffTriBrush.Checked )
                {
                    App.ToolOptions.MouseTool = MouseTool.CliffTriangle;
                }
                else if ( rdoAutoCliffBrush.Checked )
                {
                    App.ToolOptions.MouseTool = MouseTool.CliffBrush;
                }
                else if ( rdoAutoCliffRemove.Checked )
                {
                    App.ToolOptions.MouseTool = MouseTool.CliffRemove;
                }
                else if ( rdoAutoTextureFill.Checked )
                {
                    App.ToolOptions.MouseTool = MouseTool.TerrainFill;
                }
                else if ( rdoAutoRoadLine.Checked )
                {
                    App.ToolOptions.MouseTool = MouseTool.RoadLines;
                }
                else if ( rdoRoadRemove.Checked )
                {
                    App.ToolOptions.MouseTool = MouseTool.RoadRemove;
                }
                else
                {
                    App.ToolOptions.MouseTool = MouseTool.ObjectSelect;
                }
            }
            else if ( TabControl.SelectedTab == tpObjects )
            {
                ObjectsUpdate();
                if ( rdoObjectPlace.Checked )
                {
                    App.ToolOptions.MouseTool = MouseTool.ObjectPlace;
                }
                else if ( rdoObjectLines.Checked )
                {
                    App.ToolOptions.MouseTool = MouseTool.ObjectLines;
                }
                else
                {
                    App.ToolOptions.MouseTool = MouseTool.ObjectSelect;
                }
            }
            else
            {
                App.ToolOptions.MouseTool = MouseTool.ObjectSelect;
            }
        }

        public void rdoHeightSet_CheckedChanged(Object sender, EventArgs e)
        {
            if ( rdoHeightSet.Checked )
            {
                rdoHeightSmooth.Checked = false;
                rdoHeightChange.Checked = false;

                App.ToolOptions.MouseTool = MouseTool.HeightSetBrush;
            }
        }

        public void rdoHeightChange_CheckedChanged(Object sender, EventArgs e)
        {
            if ( rdoHeightChange.Checked )
            {
                rdoHeightSet.Checked = false;
                rdoHeightSmooth.Checked = false;

                App.ToolOptions.MouseTool = MouseTool.HeightChangeBrush;
            }
        }

        public void rdoHeightSmooth_CheckedChanged(Object sender, EventArgs e)
        {
            if ( rdoHeightSmooth.Checked )
            {
                rdoHeightSet.Checked = false;
                rdoHeightChange.Checked = false;

                App.ToolOptions.MouseTool = MouseTool.HeightSmoothBrush;
            }
        }

        private void tmrTool_Tick(Object sender, EventArgs e)
        {
//            if ( !App.ProgramInitialized )
//            {
//                return;
//            }
//
//            var Map = MainMap;
//
//            if ( Map == null )
//            {
//                return;
//            }
//
//            Map.viewInfo.TimedTools();
        }

        public void btnResize_Click(Object sender, EventArgs e)
        {
            var NewSize = new XYInt();
            var Offset = new XYInt();

            if ( !IOUtil.InvariantParse(txtSizeX.Text, ref NewSize.X) )
            {
                return;
            }
            if ( !IOUtil.InvariantParse(txtSizeY.Text, ref NewSize.Y) )
            {
                return;
            }
            if ( !IOUtil.InvariantParse(txtOffsetX.Text, ref Offset.X) )
            {
                return;
            }
            if ( !IOUtil.InvariantParse(txtOffsetY.Text, ref Offset.Y) )
            {
                return;
            }

            Map_Resize(Offset, NewSize);
        }

        public void Map_Resize(XYInt Offset, XYInt NewSize)
        {
            var Map = MainMap;

            if ( Map == null )
            {
                return;
            }

            if ( MessageBox.Show("Resizing can\'t be undone. Continue?", "", MessageBoxButtons.OKCancel) != DialogResult.OK )
            {
                return;
            }

            if ( NewSize.X < 1 | NewSize.Y < 1 )
            {
                MessageBox.Show("Map sizes must be at least 1.", "", MessageBoxButtons.OK);
                return;
            }
            if ( NewSize.X > Constants.WzMapMaxSize | NewSize.Y > Constants.WzMapMaxSize )
            {
                if ( MessageBox.Show(
                    "Warzone doesn\'t support map sizes above {0} Continue anyway?".Format2(Constants.WzMapMaxSize),
                    "", MessageBoxButtons.YesNo) != DialogResult.Yes )
                {
                    return;
                }
            }

            Map.TerrainResize(Offset, NewSize);

            Resize_Update();
            ScriptMarkerLists_Update();

            Map.SectorGraphicsChanges.SetAllChanged();

            Map.Update(null);

            View_DrawViewLater();
        }

        public void Resize_Update()
        {
            var Map = MainMap;

            if ( Map == null )
            {
                txtSizeX.Text = "";
                txtSizeY.Text = "";
                txtOffsetX.Text = "";
                txtOffsetY.Text = "";
            }
            else
            {
                txtSizeX.Text = Map.Terrain.TileSize.X.ToStringInvariant();
                txtSizeY.Text = Map.Terrain.TileSize.Y.ToStringInvariant();
                txtOffsetX.Text = "0";
                txtOffsetY.Text = "0";
            }
        }

        public void CloseToolStripMenuItem_Click(Object sender, EventArgs e)
        {
            Close();
        }

        public void OpenToolStripMenuItem_Click(Object sender, EventArgs e)
        {
            Load_Map_Prompt();
        }

        public void LNDToolStripMenuItem1_Click(Object sender, EventArgs e)
        {
            Save_LND_Prompt();
        }

        public void Save_LND_Prompt()
        {
            var Map = MainMap;

            if ( Map == null )
            {
                return;
            }

            var Dialog = new SaveFileDialog();

            Dialog.InitialDirectory = App.SettingsManager.SavePath;
            Dialog.FileName = "";
            Dialog.Filter = "Editworld Files (*.lnd)|*.lnd";
            if ( Dialog.ShowDialog(this) != DialogResult.OK )
            {
                return;
            }
            App.SettingsManager.SavePath = Path.GetDirectoryName(Dialog.FileName);
                 
            var lndSaver = App.Kernel.Get<LNDSaver>();
            var result = lndSaver.Save(Dialog.FileName, Map, true);
            App.ShowWarnings(result);
        }

        public void Save_Minimap_Prompt()
        {
            var Map = MainMap;

            if ( Map == null )
            {
                return;
            }

            var Dialog = new SaveFileDialog();

            Dialog.InitialDirectory = App.SettingsManager.SavePath;
            Dialog.FileName = "";
            Dialog.Filter = "Bitmap File (*.bmp)|*.bmp";
            if ( Dialog.ShowDialog(this) != DialogResult.OK )
            {
                return;
            }
            App.SettingsManager.SavePath = Path.GetDirectoryName(Dialog.FileName);
            var minimapSaver = App.Kernel.Get<MinimapSaver>();
            var result = minimapSaver.Save(Dialog.FileName, Map, true);
            if (result.HasProblems || result.HasWarnings) {
                App.ShowWarnings (result);
            }
        }

        public void Save_Heightmap_Prompt()
        {
            var Map = MainMap;

            if ( Map == null )
            {
                return;
            }

            var Dialog = new SaveFileDialog();

            Dialog.InitialDirectory = App.SettingsManager.SavePath;
            Dialog.FileName = "";
            Dialog.Filter = "Bitmap File (*.bmp)|*.bmp";
            if ( Dialog.ShowDialog(this) != DialogResult.OK )
            {
                return;
            }
            App.SettingsManager.SavePath = Path.GetDirectoryName(Dialog.FileName);
            var hmSaver = App.Kernel.Get<HeightmapSaver>();
            var result = hmSaver.Save(Dialog.FileName, Map, true);
            if (result.HasProblems || result.HasWarnings) {
                App.ShowWarnings (result);
            }
        }

        public void PromptSave_TTP()
        {
            var Map = MainMap;

            if ( Map == null )
            {
                return;
            }

            var Dialog = new SaveFileDialog();

            Dialog.InitialDirectory = App.SettingsManager.SavePath;
            Dialog.FileName = "";
            Dialog.Filter = "TTP Files (*.ttp)|*.ttp";
            if ( Dialog.ShowDialog(this) != DialogResult.OK )
            {
                return;
            }
            App.SettingsManager.SavePath = Path.GetDirectoryName(Dialog.FileName);
            var ttpSaver = App.Kernel.Get<TTPSaver>();
            var result = ttpSaver.Save(Dialog.FileName, Map, true);
            if (result.HasProblems || result.HasWarnings) {
                App.ShowWarnings (result);
            }
        }

        public void New_Prompt()
        {
            NewMap();
        }

        public void NewMap()
        {
            var NewMap = App.Kernel.Get<Map>().Create(new XYInt(64, 64));
            NewMainMap(NewMap);

            NewMap.RandomizeTileOrientations();
            NewMap.Update(null);
            NewMap.UndoClear();
        }

        public void rdoAutoCliffRemove_Click(Object sender, EventArgs e)
        {
            App.ToolOptions.MouseTool = MouseTool.CliffRemove;
        }

        public void rdoAutoCliffBrush_Click(Object sender, EventArgs e)
        {
            App.ToolOptions.MouseTool = MouseTool.CliffBrush;
        }

        public void MinimapBMPToolStripMenuItem_Click(Object sender, EventArgs e)
        {
            Save_Minimap_Prompt();
        }

        public void FMapToolStripMenuItem_Click(Object sender, EventArgs e)
        {
            var Map = MainMap;

            if ( Map == null )
            {
                return;
            }

            //if ( Map.Save_FMap_Prompt() )
            {
                Program.frmMainInstance.tsbSave.Enabled = false;
                TitleTextUpdate();
            }
        }

        public void menuSaveFMapQuick_Click(Object sender, EventArgs e)
        {
            QuickSave();
        }

        public void rdoAutoTextureFill_CheckedChanged(Object sender, EventArgs e)
        {
            App.ToolOptions.MouseTool = MouseTool.TerrainFill;
        }

        public void btnHeightOffsetSelection_Click(Object sender, EventArgs e)
        {
            var Map = MainMap;

            if ( Map == null )
            {
                return;
            }

            if ( Map.SelectedAreaVertexA == null || Map.SelectedAreaVertexB == null )
            {
                return;
            }

            var X = 0;
            var Y = 0;
            double Offset = 0;
            var StartXY = new XYInt();
            var FinishXY = new XYInt();
            var Pos = new XYInt();

            if ( !IOUtil.InvariantParse(txtHeightOffset.Text, ref Offset) )
            {
                return;
            }

            MathUtil.ReorderXY(Map.SelectedAreaVertexA, Map.SelectedAreaVertexB, ref StartXY, ref FinishXY);
            for ( Y = StartXY.Y; Y <= FinishXY.Y; Y++ )
            {
                for ( X = StartXY.X; X <= FinishXY.X; X++ )
                {
                    Map.Terrain.Vertices[X, Y].Height =
                        (byte)(Math.Round(MathUtil.ClampDbl(Convert.ToDouble(Map.Terrain.Vertices[X, Y].Height + Offset), byte.MinValue, byte.MaxValue)));
                    Pos.X = X;
                    Pos.Y = Y;
                    Map.SectorGraphicsChanges.VertexAndNormalsChanged(Pos);
                    Map.SectorUnitHeightsChanges.VertexChanged(Pos);
                    Map.SectorTerrainUndoChanges.VertexChanged(Pos);
                }
            }

            Map.Update(null);

            Map.UndoStepCreate("Selection Heights Offset");

            View_DrawViewLater();
        }

        public void rdoAutoTexturePlace_Click(Object sender, EventArgs e)
        {
            App.ToolOptions.MouseTool = MouseTool.TerrainBrush;
        }

        public void rdoAutoRoadPlace_Click(Object sender, EventArgs e)
        {
            App.ToolOptions.MouseTool = MouseTool.RoadPlace;
        }

        public void rdoAutoRoadLine_Click(Object sender, EventArgs e)
        {
            App.ToolOptions.MouseTool = MouseTool.RoadLines;
            var Map = MainMap;
            if ( Map != null )
            {
                Map.SelectedTileA = null;
                Map.SelectedTileB = null;
            }
        }

        public void rdoRoadRemove_Click(Object sender, EventArgs e)
        {
            App.ToolOptions.MouseTool = MouseTool.RoadRemove;
        }

        public void btnAutoRoadRemove_Click(Object sender, EventArgs e)
        {
            lstAutoRoad.SelectedIndex = -1;
        }

        public void btnAutoTextureRemove_Click(Object sender, EventArgs e)
        {
            lstAutoTexture.SelectedIndex = -1;
        }

        public void NewMapToolStripMenuItem_Click(Object sender, EventArgs e)
        {
            New_Prompt();
        }

        public void ToolStripMenuItem3_Click(Object sender, EventArgs e)
        {
            Save_Heightmap_Prompt();
        }

        public void Components_Update()
        {
            if ( App.ObjectData == null )
            {
                return;
            }

            var Body = default(Body);
            var Propulsion = default(Propulsion);
            var Turret = default(Turret);
            var Text = "";
            var TypeName = "";
            var ListPosition = 0;

            cboDroidBody.Items.Clear();
            cboBody_Objects = new Body[App.ObjectData.Bodies.Count];
            foreach ( var tempLoopVar_Body in App.ObjectData.Bodies )
            {
                Body = tempLoopVar_Body;
                if ( Body.Designable || (!cbxDesignableOnly.Checked) )
                {
                    ListPosition = cboDroidBody.Items.Add("(" + Body.Name + ") " + Body.Code);
                    cboBody_Objects[ListPosition] = Body;
                }
            }
            Array.Resize(ref cboBody_Objects, cboDroidBody.Items.Count);

            cboDroidPropulsion.Items.Clear();
            cboPropulsion_Objects = new Propulsion[App.ObjectData.Propulsions.Count];
            foreach ( var tempLoopVar_Propulsion in App.ObjectData.Propulsions )
            {
                Propulsion = tempLoopVar_Propulsion;
                if ( Propulsion.Designable || (!cbxDesignableOnly.Checked) )
                {
                    ListPosition = cboDroidPropulsion.Items.Add("(" + Propulsion.Name + ") " + Propulsion.Code);
                    cboPropulsion_Objects[ListPosition] = Propulsion;
                }
            }
            Array.Resize(ref cboPropulsion_Objects, cboDroidPropulsion.Items.Count);

            cboDroidTurret1.Items.Clear();
            cboDroidTurret2.Items.Clear();
            cboDroidTurret3.Items.Clear();
            cboTurret_Objects = new Turret[App.ObjectData.Turrets.Count];
            foreach ( var tempLoopVar_Turret in App.ObjectData.Turrets )
            {
                Turret = tempLoopVar_Turret;
                if ( Turret.Designable || (!cbxDesignableOnly.Checked) )
                {
                    TypeName = null;
                    Turret.GetTurretTypeName(ref TypeName);
                    Text = "(" + TypeName + " - " + Turret.Name + ") " + Turret.Code;
                    ListPosition = cboDroidTurret1.Items.Add(Text);
                    cboDroidTurret2.Items.Add(Text);
                    cboDroidTurret3.Items.Add(Text);
                    cboTurret_Objects[ListPosition] = Turret;
                }
            }
            Array.Resize(ref cboTurret_Objects, cboDroidTurret1.Items.Count);

            cboDroidType.Items.Clear();
            for ( var A = 0; A <= App.TemplateDroidTypeCount - 1; A++ )
            {
                cboDroidType.Items.Add(App.TemplateDroidTypes[A].Name);
            }
        }

        private void ObjectListFill<ObjectType>(ObservableCollection<ObjectType> objects, DataGridView gridView) where ObjectType : UnitTypeBase
        {
            ObservableCollection<ObjectType> filtered;
            var searchText = txtObjectFind.Text;
            bool doSearch;
            if ( searchText == null )
            {
                doSearch = false;
            }
            else if ( string.IsNullOrEmpty(searchText) )
            {
                doSearch = false;
            }
            else
            {
                doSearch = true;
            }
            if ( doSearch )
            {
                filtered = ObjectFindText(objects, searchText);
            }
            else
            {
                filtered = objects;
            }

            var table = new DataTable();
            table.Columns.Add("Item", typeof(UnitTypeBase));
            table.Columns.Add("Internal Name", typeof(string));
            table.Columns.Add("In-Game Name", typeof(string));
            //table.Columns.Add("Type")
            for ( var i = 0; i <= filtered.Count - 1; i++ )
            {
                var item = filtered[i];
                string code = null;
                item.GetCode(ref code);
                table.Rows.Add(new object[] {item, code, item.GetName().Replace("*", "")});
            }
            gridView.DataSource = table;
            gridView.Columns[0].Visible = false;
            gridView.Columns[1].AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
            gridView.Columns[2].AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;

#if !Mono
            ObjectTypeSelectionUpdate(gridView);
#else
            gridView.ClearSelection(); //mono selects its rows too late
#endif
        }

        public ObservableCollection<ItemType> ObjectFindText<ItemType>(ObservableCollection<ItemType> list, string text) where ItemType : UnitTypeBase
        {
            var result = new ObservableCollection<ItemType>();
            //result.MaintainOrder = true;

            text = text.ToLower();

            for ( var i = 0; i <= list.Count - 1; i++ )
            {
                var item = list[i];
                string code = null;
                if ( item.GetCode(ref code) )
                {
                    if ( code.ToLower().IndexOf(text) >= 0 || item.GetName().ToLower().IndexOf(text) >= 0 )
                    {
                        result.Add(item);
                    }
                }
                else if ( item.GetName().ToLower().IndexOf(text) >= 0 )
                {
                    result.Add(item);
                }
            }

            return result;
        }

        public void txtObjectRotation_LostFocus(Object sender, EventArgs e)
        {
            if ( !txtObjectRotation.Enabled )
            {
                return;
            }
            if ( txtObjectRotation.Text == "" )
            {
                return;
            }

            var Map = MainMap;

            if ( Map == null )
            {
                return;
            }
            if ( Map.SelectedUnits.Count <= 0 )
            {
                return;
            }

            var Angle = 0;
            if ( !IOUtil.InvariantParse(txtObjectRotation.Text, ref Angle) )
            {
                //MsgBox("Invalid rotation value.", (MsgBoxStyle.OkOnly or MsgBoxStyle.Information), "")
                //SelectedObject_Changed()
                //todo
                return;
            }

            Angle = MathUtil.ClampInt(Angle, 0, 359);

            if ( Map.SelectedUnits.Count > 1 )
            {
                if ( MessageBox.Show("Change rotation of multiple objects?", "", MessageBoxButtons.OKCancel,
                    MessageBoxIcon.None) != DialogResult.OK )
                {
                    //SelectedObject_Changed()
                    return;
                }
            }

            var ObjectRotation = new clsObjectRotation();
            ObjectRotation.Map = Map;
            ObjectRotation.Angle = Angle;
            Map.SelectedUnitsAction(ObjectRotation);

            Map.Update(null);
            SelectedObject_Changed();
            Map.UndoStepCreate("Object Rotated");
            View_DrawViewLater();
        }

        public void SelectedObject_Changed()
        {
            var ClearControls = default(bool);
            var Map = MainMap;

            lblObjectType.Enabled = false;
            ObjectPlayerNumControl.Enabled = false;
            txtObjectRotation.Enabled = false;
            txtObjectID.Enabled = false;
            txtObjectLabel.Enabled = false;
            txtObjectPriority.Enabled = false;
            txtObjectHealth.Enabled = false;
            btnDroidToDesign.Enabled = false;
            cboDroidType.Enabled = false;
            cboDroidBody.Enabled = false;
            cboDroidPropulsion.Enabled = false;
            cboDroidTurret1.Enabled = false;
            cboDroidTurret2.Enabled = false;
            cboDroidTurret3.Enabled = false;
            rdoDroidTurret0.Enabled = false;
            rdoDroidTurret1.Enabled = false;
            rdoDroidTurret2.Enabled = false;
            rdoDroidTurret3.Enabled = false;
            if ( Map == null )
            {
                ClearControls = true;
            }
            else if ( Map.SelectedUnits.Count == 0 )
            {
                ClearControls = true;
            }
            if ( ClearControls )
            {
                lblObjectType.Text = "";
                ObjectPlayerNumControl.Target.Item = null;
                txtObjectRotation.Text = "";
                txtObjectID.Text = "";
                txtObjectLabel.Text = "";
                txtObjectPriority.Text = "";
                txtObjectHealth.Text = "";
                cboDroidType.SelectedIndex = -1;
                cboDroidBody.SelectedIndex = -1;
                cboDroidPropulsion.SelectedIndex = -1;
                cboDroidTurret1.SelectedIndex = -1;
                cboDroidTurret2.SelectedIndex = -1;
                cboDroidTurret3.SelectedIndex = -1;
                rdoDroidTurret0.Checked = false;
                rdoDroidTurret1.Checked = false;
                rdoDroidTurret2.Checked = false;
                rdoDroidTurret3.Checked = false;
            }
            else if ( Map.SelectedUnits.Count > 1 )
            {
                lblObjectType.Text = "Multiple objects";
                var A = 0;
                var UnitGroup = Map.SelectedUnits[0].UnitGroup;
                for ( A = 1; A <= Map.SelectedUnits.Count - 1; A++ )
                {
                    if ( Map.SelectedUnits[A].UnitGroup != UnitGroup )
                    {
                        break;
                    }
                }
                if ( A == Map.SelectedUnits.Count )
                {
                    ObjectPlayerNumControl.Target.Item = UnitGroup;
                }
                else
                {
                    ObjectPlayerNumControl.Target.Item = null;
                }
                txtObjectRotation.Text = "";
                txtObjectID.Text = "";
                txtObjectLabel.Text = "";
                lblObjectType.Enabled = true;
                ObjectPlayerNumControl.Enabled = true;
                txtObjectRotation.Enabled = true;
                txtObjectPriority.Text = "";
                txtObjectPriority.Enabled = true;
                txtObjectHealth.Text = "";
                txtObjectHealth.Enabled = true;
                //design
                var Unit = default(Unit);
                foreach ( var tempLoopVar_Unit in Map.SelectedUnits )
                {
                    Unit = tempLoopVar_Unit;
                    if ( Unit.TypeBase.Type == UnitType.PlayerDroid )
                    {
                        if ( ((DroidDesign)Unit.TypeBase).IsTemplate )
                        {
                            break;
                        }
                    }
                }
                if ( A < Map.SelectedUnits.Count )
                {
                    btnDroidToDesign.Enabled = true;
                }

                foreach ( var tempLoopVar_Unit in Map.SelectedUnits )
                {
                    Unit = tempLoopVar_Unit;
                    if ( Unit.TypeBase.Type == UnitType.PlayerDroid )
                    {
                        if ( !((DroidDesign)Unit.TypeBase).IsTemplate )
                        {
                            break;
                        }
                    }
                }
                if ( A < Map.SelectedUnits.Count )
                {
                    cboDroidType.SelectedIndex = -1;
                    cboDroidBody.SelectedIndex = -1;
                    cboDroidPropulsion.SelectedIndex = -1;
                    cboDroidTurret1.SelectedIndex = -1;
                    cboDroidTurret2.SelectedIndex = -1;
                    cboDroidTurret3.SelectedIndex = -1;
                    rdoDroidTurret1.Checked = false;
                    rdoDroidTurret2.Checked = false;
                    rdoDroidTurret3.Checked = false;
                    cboDroidType.Enabled = true;
                    cboDroidBody.Enabled = true;
                    cboDroidPropulsion.Enabled = true;
                    cboDroidTurret1.Enabled = true;
                    cboDroidTurret2.Enabled = true;
                    cboDroidTurret3.Enabled = true;
                    rdoDroidTurret0.Enabled = true;
                    rdoDroidTurret1.Enabled = true;
                    rdoDroidTurret2.Enabled = true;
                    rdoDroidTurret3.Enabled = true;
                }
            }
            else if ( Map.SelectedUnits.Count == 1 )
            {
                var A = 0;
                var with_1 = Map.SelectedUnits[0];
                lblObjectType.Text = Convert.ToString(with_1.TypeBase.GetDisplayTextCode());
                ObjectPlayerNumControl.Target.Item = with_1.UnitGroup;
                txtObjectRotation.Text = Convert.ToInt32(with_1.Rotation).ToStringInvariant();
                txtObjectID.Text = with_1.ID.ToStringInvariant();
                txtObjectPriority.Text = Convert.ToInt32(with_1.SavePriority).ToStringInvariant();
                txtObjectHealth.Text = Convert.ToDouble(with_1.Health * 100.0D).ToStringInvariant();
                lblObjectType.Enabled = true;
                ObjectPlayerNumControl.Enabled = true;
                txtObjectRotation.Enabled = true;
                //txtObjectID.Enabled = True 'no known need to change IDs
                txtObjectPriority.Enabled = true;
                txtObjectHealth.Enabled = true;
                var LabelEnabled = true;
                if ( with_1.TypeBase.Type == UnitType.PlayerStructure )
                {
                    if ( ((StructureTypeBase)with_1.TypeBase).IsModule() )
                    {
                        LabelEnabled = false;
                    }
                }
                if ( LabelEnabled )
                {
                    txtObjectLabel.Text = Convert.ToString(with_1.Label);
                    txtObjectLabel.Enabled = true;
                }
                else
                {
                    txtObjectLabel.Text = "";
                }
                var ClearDesignControls = false;
                if ( with_1.TypeBase.Type == UnitType.PlayerDroid )
                {
                    var DroidType = (DroidDesign)with_1.TypeBase;
                    if ( DroidType.IsTemplate )
                    {
                        btnDroidToDesign.Enabled = true;
                        ClearDesignControls = true;
                    }
                    else
                    {
                        if ( DroidType.TemplateDroidType == null )
                        {
                            cboDroidType.SelectedIndex = -1;
                        }
                        else
                        {
                            cboDroidType.SelectedIndex = DroidType.TemplateDroidType.Num;
                        }

                        if ( DroidType.Body == null )
                        {
                            cboDroidBody.SelectedIndex = -1;
                        }
                        else
                        {
                            for ( A = 0; A <= cboDroidBody.Items.Count - 1; A++ )
                            {
                                if ( cboBody_Objects[A] == DroidType.Body )
                                {
                                    break;
                                }
                            }
                            if ( A < 0 )
                            {
                            }
                            else if ( A < cboDroidBody.Items.Count )
                            {
                                cboDroidBody.SelectedIndex = A;
                            }
                            else
                            {
                                cboDroidBody.SelectedIndex = -1;
                                cboDroidBody.Text = DroidType.Body.Code;
                            }
                        }

                        if ( DroidType.Propulsion == null )
                        {
                            cboDroidPropulsion.SelectedIndex = -1;
                        }
                        else
                        {
                            for ( A = 0; A <= cboDroidPropulsion.Items.Count - 1; A++ )
                            {
                                if ( cboPropulsion_Objects[A] == DroidType.Propulsion )
                                {
                                    break;
                                }
                            }
                            if ( A < cboDroidPropulsion.Items.Count )
                            {
                                cboDroidPropulsion.SelectedIndex = A;
                            }
                            else
                            {
                                cboDroidPropulsion.SelectedIndex = -1;
                                cboDroidPropulsion.Text = DroidType.Propulsion.Code;
                            }
                        }

                        if ( DroidType.Turret1 == null )
                        {
                            cboDroidTurret1.SelectedIndex = -1;
                        }
                        else
                        {
                            for ( A = 0; A <= cboDroidTurret1.Items.Count - 1; A++ )
                            {
                                if ( cboTurret_Objects[A] == DroidType.Turret1 )
                                {
                                    break;
                                }
                            }
                            if ( A < cboDroidTurret1.Items.Count )
                            {
                                cboDroidTurret1.SelectedIndex = A;
                            }
                            else
                            {
                                cboDroidTurret1.SelectedIndex = -1;
                                cboDroidTurret1.Text = DroidType.Turret1.Code;
                            }
                        }

                        if ( DroidType.Turret2 == null )
                        {
                            cboDroidTurret2.SelectedIndex = -1;
                        }
                        else
                        {
                            for ( A = 0; A <= cboDroidTurret2.Items.Count - 1; A++ )
                            {
                                if ( cboTurret_Objects[A] == DroidType.Turret2 )
                                {
                                    break;
                                }
                            }
                            if ( A < cboDroidTurret2.Items.Count )
                            {
                                cboDroidTurret2.SelectedIndex = A;
                            }
                            else
                            {
                                cboDroidTurret2.SelectedIndex = -1;
                                cboDroidTurret2.Text = DroidType.Turret2.Code;
                            }
                        }

                        if ( DroidType.Turret3 == null )
                        {
                            cboDroidTurret3.SelectedIndex = -1;
                        }
                        else
                        {
                            for ( A = 0; A <= cboDroidTurret3.Items.Count - 1; A++ )
                            {
                                if ( cboTurret_Objects[A] == DroidType.Turret3 )
                                {
                                    break;
                                }
                            }
                            if ( A < cboDroidTurret3.Items.Count )
                            {
                                cboDroidTurret3.SelectedIndex = A;
                            }
                            else
                            {
                                cboDroidTurret3.SelectedIndex = -1;
                                cboDroidTurret3.Text = DroidType.Turret3.Code;
                            }
                        }

                        if ( DroidType.TurretCount == 3 )
                        {
                            rdoDroidTurret0.Checked = false;
                            rdoDroidTurret1.Checked = false;
                            rdoDroidTurret2.Checked = false;
                            rdoDroidTurret3.Checked = true;
                        }
                        else if ( DroidType.TurretCount == 2 )
                        {
                            rdoDroidTurret0.Checked = false;
                            rdoDroidTurret1.Checked = false;
                            rdoDroidTurret2.Checked = true;
                            rdoDroidTurret3.Checked = false;
                        }
                        else if ( DroidType.TurretCount == 1 )
                        {
                            rdoDroidTurret0.Checked = false;
                            rdoDroidTurret1.Checked = true;
                            rdoDroidTurret2.Checked = false;
                            rdoDroidTurret3.Checked = false;
                        }
                        else if ( DroidType.TurretCount == 0 )
                        {
                            rdoDroidTurret0.Checked = true;
                            rdoDroidTurret1.Checked = false;
                            rdoDroidTurret2.Checked = false;
                            rdoDroidTurret3.Checked = false;
                        }
                        else
                        {
                            rdoDroidTurret0.Checked = false;
                            rdoDroidTurret1.Checked = false;
                            rdoDroidTurret2.Checked = false;
                            rdoDroidTurret3.Checked = false;
                        }

                        cboDroidType.Enabled = true;
                        cboDroidBody.Enabled = true;
                        cboDroidPropulsion.Enabled = true;
                        cboDroidTurret1.Enabled = true;
                        cboDroidTurret2.Enabled = true;
                        cboDroidTurret3.Enabled = true;
                        rdoDroidTurret0.Enabled = true;
                        rdoDroidTurret1.Enabled = true;
                        rdoDroidTurret2.Enabled = true;
                        rdoDroidTurret3.Enabled = true;
                    }
                }
                else
                {
                    ClearDesignControls = true;
                }
                if ( ClearDesignControls )
                {
                    cboDroidType.SelectedIndex = -1;
                    cboDroidBody.SelectedIndex = -1;
                    cboDroidPropulsion.SelectedIndex = -1;
                    cboDroidTurret1.SelectedIndex = -1;
                    cboDroidTurret2.SelectedIndex = -1;
                    cboDroidTurret3.SelectedIndex = -1;
                    rdoDroidTurret1.Checked = false;
                    rdoDroidTurret2.Checked = false;
                    rdoDroidTurret3.Checked = false;
                }
            }
        }

        public void tsbSelection_Click(Object sender, EventArgs e)
        {
            App.ToolOptions.MouseTool = MouseTool.TerrainSelect;
        }

        public void tsbSelectionCopy_Click(Object sender, EventArgs e)
        {
            var Map = MainMap;

            if ( Map == null )
            {
                return;
            }
            if ( Map.SelectedAreaVertexA == null || Map.SelectedAreaVertexB == null )
            {
                return;
            }
            if ( App.Copied_Map != null )
            {
                App.Copied_Map.Deallocate();
            }
            var Area = new XYInt();
            var Start = new XYInt();
            var Finish = new XYInt();
            MathUtil.ReorderXY(Map.SelectedAreaVertexA, Map.SelectedAreaVertexB, ref Start, ref Finish);
            Area.X = Finish.X - Start.X;
            Area.Y = Finish.Y - Start.Y;
            App.Copied_Map = App.Kernel.Get<Map>().Copy(Map, Start, Area);
        }

        public void tsbSelectionPaste_Click(Object sender, EventArgs e)
        {
            var Map = MainMap;

            if ( Map == null )
            {
                return;
            }
            if ( Map.SelectedAreaVertexA == null || Map.SelectedAreaVertexB == null )
            {
                return;
            }
            if ( App.Copied_Map == null )
            {
                MessageBox.Show("Nothing to paste.");
                return;
            }
            if (
                !(menuSelPasteHeights.Checked
				|| menuSelPasteTextures.Checked
				|| menuSelPasteUnits.Checked 
				|| menuSelPasteDeleteUnits.Checked
				||menuSelPasteGateways.Checked || menuSelPasteDeleteGateways.Checked) )
            {
                return;
            }
            var Area = new XYInt();
            var Start = new XYInt();
            var Finish = new XYInt();
            MathUtil.ReorderXY(Map.SelectedAreaVertexA, Map.SelectedAreaVertexB, ref Start, ref Finish);
            Area.X = Finish.X - Start.X;
            Area.Y = Finish.Y - Start.Y;
            Map.MapInsert(App.Copied_Map, Start, Area, menuSelPasteHeights.Checked, menuSelPasteTextures.Checked, menuSelPasteUnits.Checked,
                menuSelPasteDeleteUnits.Checked, menuSelPasteGateways.Checked, menuSelPasteDeleteGateways.Checked);

            SelectedObject_Changed();
            Map.UndoStepCreate("Paste");

            View_DrawViewLater();
        }

        public void btnSelResize_Click(Object sender, EventArgs e)
        {
            var Map = MainMap;

            if ( Map == null )
            {
                return;
            }

            if ( Map.SelectedAreaVertexA == null || Map.SelectedAreaVertexB == null )
            {
                MessageBox.Show("You haven\'t selected anything.");
                return;
            }

            var Start = new XYInt();
            var Finish = new XYInt();
            MathUtil.ReorderXY(Map.SelectedAreaVertexA, Map.SelectedAreaVertexB, ref Start, ref Finish);
            var Area = new XYInt();
            Area.X = Finish.X - Start.X;
            Area.Y = Finish.Y - Start.Y;

            Map_Resize(Start, Area);
        }

        public void tsbSelectionRotateClockwise_Click(Object sender, EventArgs e)
        {
            if ( App.Copied_Map == null )
            {
                MessageBox.Show("Nothing to rotate.");
                return;
            }

            App.Copied_Map.Rotate(TileUtil.Clockwise, Program.frmMainInstance.PasteRotateObjects);
        }

        public void tsbSelectionRotateAnticlockwise_Click(Object sender, EventArgs e)
        {
            if ( App.Copied_Map == null )
            {
                MessageBox.Show("Nothing to rotate.");
                return;
            }

            App.Copied_Map.Rotate(TileUtil.CounterClockwise, Program.frmMainInstance.PasteRotateObjects);
        }

        public void menuMiniShowTex_Click(Object sender, EventArgs e)
        {
            UpdateMinimap();
        }

        public void menuMiniShowHeight_Click(Object sender, EventArgs e)
        {
            UpdateMinimap();
        }

        public void menuMiniShowUnits_Click(Object sender, EventArgs e)
        {
            UpdateMinimap();
        }

        private Result NoTile_Texture_Load()
        {
            var ReturnResult = new Result("Loading error terrain textures", false);
            logger.Info("Loading error terrain textures");

            Bitmap Bitmap = null;

            Bitmap = new Bitmap("");
            {
                var Result = new Result("Loading notile.png", false);
                logger.Info("Loading notile.png");
                Result.Take(BitmapUtil.BitmapIsGlCompatible(Bitmap));
                ReturnResult.Add (Result);
                App.GLTexture_NoTile = BitmapUtil.CreateGLTexture (Bitmap, 0);
            }

            Bitmap = new Bitmap("");
            {
                var Result = new Result("Loading overflow.png", false);
                logger.Info("Loading overflow.png");
                Result.Take(BitmapUtil.BitmapIsGlCompatible(Bitmap));
                ReturnResult.Add(Result);
                App.GLTexture_OverflowTile = BitmapUtil.CreateGLTexture (Bitmap, 0);
            }

            return ReturnResult;
        }

        public void tsbGateways_Click(Object sender, EventArgs e)
        {
            if ( App.ToolOptions.MouseTool == MouseTool.Gateways )
            {
                App.Draw_Gateways = false;
                App.ToolOptions.MouseTool = MouseTool.ObjectSelect;
                tsbGateways.Checked = false;
            }
            else
            {
                App.Draw_Gateways = true;
                App.ToolOptions.MouseTool = MouseTool.Gateways;
                tsbGateways.Checked = true;
            }
            var Map = MainMap;
            if ( Map != null )
            {
                Map.SelectedTileA = null;
                Map.SelectedTileB = null;
                View_DrawViewLater();
            }
        }

        public void tsbDrawAutotexture_Click(Object sender, EventArgs e)
        {
            if ( MapViewControl != null )
            {
                if ( App.Draw_VertexTerrain != tsbDrawAutotexture.Checked )
                {
                    App.Draw_VertexTerrain = tsbDrawAutotexture.Checked;
                    View_DrawViewLater();
                }
            }
        }

        public void tsbDrawTileOrientation_Click(Object sender, EventArgs e)
        {
            if ( MapViewControl != null )
            {
                if ( App.DisplayTileOrientation != tsbDrawTileOrientation.Checked )
                {
                    App.DisplayTileOrientation = tsbDrawTileOrientation.Checked;
                    View_DrawViewLater();
                    TextureViewControl.DrawViewLater();
                }
            }
        }

        public void menuMiniShowGateways_Click(Object sender, EventArgs e)
        {
            UpdateMinimap();
        }

        public void menuMiniShowCliffs_Click(object sender, EventArgs e)
        {
            UpdateMinimap();
        }

        private void UpdateMinimap()
        {
            var Map = MainMap;

            if ( Map == null )
            {
                return;
            }

            // Map.MinimapMakeLater();
        }

        public void cmbTileType_SelectedIndexChanged(Object sender, EventArgs e)
        {
            if ( !cboTileType.Enabled )
            {
                return;
            }

            var Map = MainMap;

            if ( Map == null )
            {
                return;
            }
            if ( cboTileType.SelectedIndex < 0 )
            {
                return;
            }
            if ( App.SelectedTextureNum < 0 | App.SelectedTextureNum >= Map.Tileset.Tiles.Count )
            {
                return;
            }

            Map.TileTypeNum[App.SelectedTextureNum] = (byte)cboTileType.SelectedIndex;

            TextureViewControl.DrawViewLater();
        }

        public void chkTileTypes_CheckedChanged(Object sender, EventArgs e)
        {
            TextureViewControl.DisplayTileTypes = cbxTileTypes.Checked;
            TextureViewControl.DrawViewLater();
        }

        public void chkTileNumbers_CheckedChanged(Object sender, EventArgs e)
        {
            TextureViewControl.DisplayTileNumbers = cbxTileNumbers.Checked;
            TextureViewControl.DrawViewLater();
        }

        private void cboTileType_Update()
        {
            cboTileType.Items.Clear();
            var tileType = default(clsTileType);
            foreach ( var tempLoopVar_tileType in App.TileTypes )
            {
                tileType = tempLoopVar_tileType;
                cboTileType.Items.Add(tileType.Name);
            }
        }

        private void CreateTileTypes()
        {
            var NewTileType = default(clsTileType);

            NewTileType = new clsTileType();
            NewTileType.Name = "Sand";
            NewTileType.DisplayColour.Red = 1.0F;
            NewTileType.DisplayColour.Green = 1.0F;
            NewTileType.DisplayColour.Blue = 0.0F;
            App.TileTypes.Add(NewTileType);

            NewTileType = new clsTileType();
            NewTileType.Name = "Sandy Brush";
            NewTileType.DisplayColour.Red = 0.5F;
            NewTileType.DisplayColour.Green = 0.5F;
            NewTileType.DisplayColour.Blue = 0.0F;
            App.TileTypes.Add(NewTileType);

            NewTileType = new clsTileType();
            NewTileType.Name = "Rubble";
            NewTileType.DisplayColour.Red = 0.25F;
            NewTileType.DisplayColour.Green = 0.25F;
            NewTileType.DisplayColour.Blue = 0.25F;
            App.TileTypes.Add(NewTileType);

            NewTileType = new clsTileType();
            NewTileType.Name = "Green Mud";
            NewTileType.DisplayColour.Red = 0.0F;
            NewTileType.DisplayColour.Green = 0.5F;
            NewTileType.DisplayColour.Blue = 0.0F;
            App.TileTypes.Add(NewTileType);

            NewTileType = new clsTileType();
            NewTileType.Name = "Red Brush";
            NewTileType.DisplayColour.Red = 1.0F;
            NewTileType.DisplayColour.Green = 0.0F;
            NewTileType.DisplayColour.Blue = 0.0F;
            App.TileTypes.Add(NewTileType);

            NewTileType = new clsTileType();
            NewTileType.Name = "Pink Rock";
            NewTileType.DisplayColour.Red = 1.0F;
            NewTileType.DisplayColour.Green = 0.5F;
            NewTileType.DisplayColour.Blue = 0.5F;
            App.TileTypes.Add(NewTileType);

            NewTileType = new clsTileType();
            NewTileType.Name = "Road";
            NewTileType.DisplayColour.Red = 0.0F;
            NewTileType.DisplayColour.Green = 0.0F;
            NewTileType.DisplayColour.Blue = 0.0F;
            App.TileTypes.Add(NewTileType);

            NewTileType = new clsTileType();
            NewTileType.Name = "Water";
            NewTileType.DisplayColour.Red = 0.0F;
            NewTileType.DisplayColour.Green = 0.0F;
            NewTileType.DisplayColour.Blue = 1.0F;
            App.TileTypes.Add(NewTileType);

            NewTileType = new clsTileType();
            NewTileType.Name = "Cliff Face";
            NewTileType.DisplayColour.Red = 0.5F;
            NewTileType.DisplayColour.Green = 0.5F;
            NewTileType.DisplayColour.Blue = 0.5F;
            App.TileTypes.Add(NewTileType);

            NewTileType = new clsTileType();
            NewTileType.Name = "Baked Earth";
            NewTileType.DisplayColour.Red = 0.5F;
            NewTileType.DisplayColour.Green = 0.0F;
            NewTileType.DisplayColour.Blue = 0.0F;
            App.TileTypes.Add(NewTileType);

            NewTileType = new clsTileType();
            NewTileType.Name = "Sheet Ice";
            NewTileType.DisplayColour.Red = 1.0F;
            NewTileType.DisplayColour.Green = 1.0F;
            NewTileType.DisplayColour.Blue = 1.0F;
            App.TileTypes.Add(NewTileType);

            NewTileType = new clsTileType();
            NewTileType.Name = "Slush";
            NewTileType.DisplayColour.Red = 0.75F;
            NewTileType.DisplayColour.Green = 0.75F;
            NewTileType.DisplayColour.Blue = 0.75F;
            App.TileTypes.Add(NewTileType);
        }

        public void menuExportMapTileTypes_Click(Object sender, EventArgs e)
        {
            PromptSave_TTP();
        }

        public void ImportHeightmapToolStripMenuItem_Click(Object sender, EventArgs e)
        {
            Load_Heightmap_Prompt();
        }

        public void menuImportTileTypes_Click(Object sender, EventArgs e)
        {
            Load_TTP_Prompt();
        }

        public void tsbSave_Click(Object sender, EventArgs e)
        {
            QuickSave();
        }

        private void QuickSave()
        {
            var Map = MainMap;

            if ( Map == null )
            {
                return;
            }

            //if ( Map.Save_FMap_Quick() )
            {
                Program.frmMainInstance.tsbSave.Enabled = false;
                TitleTextUpdate();
            }
        }

        public void TitleTextUpdate()
        {
            var Map = MainMap;
            var MapFileTitle = "";

            menuSaveFMapQuick.Text = "Quick Save fmap";
            menuSaveFMapQuick.Enabled = false;
            if ( Map == null )
            {
                MapFileTitle = "No Map";
                tsbSave.ToolTipText = "No Map";
            }
            else
            {
                if ( Map.PathInfo == null )
                {
                    MapFileTitle = "Unsaved map";
                    tsbSave.ToolTipText = "Save FMap...";
                }
                else
                {
                    var SplitPath = new sSplitPath(Map.PathInfo.Path);
                    if ( Map.PathInfo.IsFMap )
                    {
                        MapFileTitle = SplitPath.FileTitleWithoutExtension;
                        var quickSavePath = Map.PathInfo.Path;
                        tsbSave.ToolTipText = "Quick save FMap to {0}".Format2(quickSavePath);
                        menuSaveFMapQuick.Text = "Quick Save fmap to \"";
                        if ( quickSavePath.Length <= 32 )
                        {
                            menuSaveFMapQuick.Text += quickSavePath;
                        }
                        else
                        {
                            menuSaveFMapQuick.Text += quickSavePath.Substring(0, 10) + "..." + quickSavePath.Substring(quickSavePath.Length - 20, 20);
                        }
                        menuSaveFMapQuick.Text += "\"";
                        menuSaveFMapQuick.Enabled = true;
                    }
                    else
                    {
                        MapFileTitle = SplitPath.FileTitle;
                        tsbSave.ToolTipText = "Save FMap...";
                    }
                }
                Map.SetTabText();
            }

            Text = MapFileTitle + " - " + Constants.ProgramName + " " + Constants.ProgramVersion();
        }

        public void lstAutoTexture_SelectedIndexChanged(Object sender, EventArgs e)
        {
            if ( !lstAutoTexture.Enabled )
            {
                return;
            }

            if ( !(App.ToolOptions.MouseTool == MouseTool.TerrainBrush || App.ToolOptions.MouseTool == MouseTool.TerrainFill) )
            {
                rdoAutoTexturePlace.Checked = true;
                rdoAutoTexturePlace_Click(null, null);
            }
        }

        public void lstAutoRoad_Click(Object sender, EventArgs e)
        {
            if ( !(App.ToolOptions.MouseTool == MouseTool.RoadPlace || App.ToolOptions.MouseTool == MouseTool.RoadLines) )
            {
                rdoAutoRoadLine.Checked = true;
                rdoAutoRoadLine_Click(null, null);
            }
        }

        private void tabPlayerNum_SelectedIndexChanged()
        {
            //ObjectPlayerNum.Focus() 'so that the rotation textbox and anything else loses focus, and performs its effects

            if ( !ObjectPlayerNumControl.Enabled )
            {
                return;
            }

            var Map = MainMap;

            if ( Map == null )
            {
                return;
            }
            if ( Map.SelectedUnits.Count <= 0 )
            {
                return;
            }
            if ( ObjectPlayerNumControl.Target.Item == null )
            {
                return;
            }

            if ( Map.SelectedUnits.Count > 1 )
            {
                if ( MessageBox.Show("Change player of multiple objects?", "", MessageBoxButtons.OKCancel, MessageBoxIcon.None) != DialogResult.OK )
                {
                    //SelectedObject_Changed()
                    //todo
                    return;
                }
            }

            var ObjectUnitGroup = new clsObjectUnitGroup();
            ObjectUnitGroup.Map = Map;
            ObjectUnitGroup.UnitGroup = ObjectPlayerNumControl.Target.Item;
            Map.SelectedUnitsAction(ObjectUnitGroup);

            SelectedObject_Changed();
            Map.UndoStepCreate("Object Player Changed");
            if ( App.SettingsManager.MinimapTeamColours )
            {
                // Map.MinimapMakeLater();
            }
            View_DrawViewLater();
        }

        public void txtHeightSetL_LostFocus(object sender, EventArgs e)
        {
            byte Height = 0;
            var Text = "";
            double Height_dbl = 0;

            if ( !IOUtil.InvariantParse(txtHeightSetL.Text, ref Height_dbl) )
            {
                return;
            }
            Height = (byte)(MathUtil.ClampDbl(Height_dbl, byte.MinValue, byte.MaxValue));
            HeightSetPalette[tabHeightSetL.SelectedIndex] = Height;
            if ( tabHeightSetL.SelectedIndex == tabHeightSetL.SelectedIndex )
            {
                tabHeightSetL_SelectedIndexChanged(null, null);
            }
            Text = Height.ToStringInvariant();
            tabHeightSetL.TabPages[tabHeightSetL.SelectedIndex].Text = Text;
            tabHeightSetR.TabPages[tabHeightSetL.SelectedIndex].Text = Text;
        }

        public void txtHeightSetR_LostFocus(object sender, EventArgs e)
        {
            byte Height = 0;
            var Text = "";
            double Height_dbl = 0;

            if ( !IOUtil.InvariantParse(txtHeightSetL.Text, ref Height_dbl) )
            {
                return;
            }
            Height = (byte)(MathUtil.ClampDbl(Height_dbl, byte.MinValue, byte.MaxValue));
            HeightSetPalette[tabHeightSetR.SelectedIndex] = Height;
            if ( tabHeightSetL.SelectedIndex == tabHeightSetR.SelectedIndex )
            {
                tabHeightSetL_SelectedIndexChanged(null, null);
            }
            Text = Height.ToStringInvariant();
            tabHeightSetL.TabPages[tabHeightSetR.SelectedIndex].Text = Text;
            tabHeightSetR.TabPages[tabHeightSetR.SelectedIndex].Text = Text;
        }

        public void tabHeightSetL_SelectedIndexChanged(object sender, EventArgs e)
        {
            txtHeightSetL.Text = HeightSetPalette[tabHeightSetL.SelectedIndex].ToStringInvariant();
        }

        public void tabHeightSetR_SelectedIndexChanged(object sender, EventArgs e)
        {
            txtHeightSetR.Text = HeightSetPalette[tabHeightSetR.SelectedIndex].ToStringInvariant();
        }

        public void tsbSelectionObjects_Click(Object sender, EventArgs e)
        {
            var Map = MainMap;

            if ( Map == null )
            {
                return;
            }

            if ( Map.SelectedAreaVertexA == null || Map.SelectedAreaVertexB == null )
            {
                return;
            }
            var Start = new XYInt();
            var Finish = new XYInt();
            var A = 0;

            MathUtil.ReorderXY(Map.SelectedAreaVertexA, Map.SelectedAreaVertexB, ref Start, ref Finish);
            for ( A = 0; A <= Map.Units.Count - 1; A++ )
            {
                if ( App.PosIsWithinTileArea(Map.Units[A].Pos.Horizontal, Start, Finish) )
                {
                    if ( !Map.Units[A].MapSelectedUnitLink.IsConnected )
                    {
                        Map.Units[A].MapSelectedUnitLink.Connect(Map.SelectedUnits);
                    }
                }
            }

            SelectedObject_Changed();
            App.ToolOptions.MouseTool = MouseTool.ObjectSelect;
            View_DrawViewLater();
        }

        public void View_DrawViewLater()
        {
            if ( MapViewControl != null )
            {
                MapViewControl.DrawViewLater();
            }
        }

        public void tsbSelectionFlipX_Click(Object sender, EventArgs e)
        {
            if ( App.Copied_Map == null )
            {
                MessageBox.Show("Nothing to flip.");
                return;
            }

            App.Copied_Map.Rotate(TileUtil.FlipX, Program.frmMainInstance.PasteRotateObjects);
        }

        public void btnHeightsMultiplySelection_Click(Object sender, EventArgs e)
        {
            var Map = MainMap;

            if ( Map == null )
            {
                return;
            }

            if ( Map.SelectedAreaVertexA == null || Map.SelectedAreaVertexB == null )
            {
                return;
            }

            var X = 0;
            var Y = 0;
            double Multiplier = 0;
            var StartXY = new XYInt();
            var FinishXY = new XYInt();
            var Pos = new XYInt();
            double dblTemp = 0;

            if ( !IOUtil.InvariantParse(txtHeightMultiply.Text, ref dblTemp) )
            {
                return;
            }
            Multiplier = MathUtil.ClampDbl(dblTemp, 0.0D, 255.0D);
            MathUtil.ReorderXY(Map.SelectedAreaVertexA, Map.SelectedAreaVertexB, ref StartXY, ref FinishXY);
            for ( Y = StartXY.Y; Y <= FinishXY.Y; Y++ )
            {
                for ( X = StartXY.X; X <= FinishXY.X; X++ )
                {
                    Map.Terrain.Vertices[X, Y].Height =
                        (byte)(Math.Round(MathUtil.ClampDbl(Convert.ToDouble(Map.Terrain.Vertices[X, Y].Height * Multiplier), byte.MinValue, byte.MaxValue)));
                    Pos.X = X;
                    Pos.Y = Y;
                    Map.SectorGraphicsChanges.VertexAndNormalsChanged(Pos);
                    Map.SectorUnitHeightsChanges.VertexChanged(Pos);
                    Map.SectorTerrainUndoChanges.VertexChanged(Pos);
                }
            }

            Map.Update(null);

            Map.UndoStepCreate("Selection Heights Multiply");

            View_DrawViewLater();
        }

        public void btnTextureAnticlockwise_Click(Object sender, EventArgs e)
        {
            App.TextureOrientation.RotateAntiClockwise();

            TextureViewControl.DrawViewLater();
        }

        public void btnTextureClockwise_Click(Object sender, EventArgs e)
        {
            App.TextureOrientation.RotateClockwise();

            TextureViewControl.DrawViewLater();
        }

        public void btnTextureFlipX_Click(Object sender, EventArgs e)
        {
            if ( App.TextureOrientation.SwitchedAxes )
            {
                App.TextureOrientation.YFlip = !App.TextureOrientation.YFlip;
            }
            else
            {
                App.TextureOrientation.XFlip = !App.TextureOrientation.XFlip;
            }

            TextureViewControl.DrawViewLater();
        }

        public void lstAutoTexture_SelectedIndexChanged_1(Object sender, EventArgs e)
        {
            var Map = MainMap;

            if ( Map == null )
            {
                return;
            }

            if ( lstAutoTexture.SelectedIndex < 0 )
            {
                App.SelectedTerrain = null;
            }
            else if ( lstAutoTexture.SelectedIndex < Map.Painter.TerrainCount )
            {
                App.SelectedTerrain = Map.Painter.Terrains[lstAutoTexture.SelectedIndex];
            }
            else
            {
                Debugger.Break();
                App.SelectedTerrain = null;
            }
        }

        public void lstAutoRoad_SelectedIndexChanged(Object sender, EventArgs e)
        {
            var Map = MainMap;

            if ( Map == null )
            {
                return;
            }

            if ( lstAutoRoad.SelectedIndex < 0 )
            {
                App.SelectedRoad = null;
            }
            else if ( lstAutoRoad.SelectedIndex < Map.Painter.RoadCount )
            {
                App.SelectedRoad = Map.Painter.Roads[lstAutoRoad.SelectedIndex];
            }
            else
            {
                Debugger.Break();
                App.SelectedRoad = null;
            }
        }

        public void txtObjectPriority_LostFocus(Object sender, EventArgs e)
        {
            if ( !txtObjectPriority.Enabled )
            {
                return;
            }
            var Map = MainMap;
            if ( Map == null )
            {
                return;
            }
            if ( Map.SelectedUnits.Count <= 0 )
            {
                return;
            }
            var Priority = 0;
            if ( !IOUtil.InvariantParse(txtObjectPriority.Text, ref Priority) )
            {
                //MsgBox("Entered text is not a valid number.", (MsgBoxStyle.OkOnly or MsgBoxStyle.Information), "")
                //SelectedObject_Changed()
                //todo
                return;
            }

            if ( Map.SelectedUnits.Count > 1 )
            {
                if ( MessageBox.Show("Change priority of multiple objects?", "", MessageBoxButtons.OKCancel, MessageBoxIcon.Question) != DialogResult.OK )
                {
                    //SelectedObject_Changed()
                    //todo
                    return;
                }
            }
            else if ( Map.SelectedUnits.Count == 1 )
            {
                if ( Priority == Map.SelectedUnits[0].SavePriority )
                {
                    return;
                }
            }

            var ObjectPriority = new clsObjectPriority();
            ObjectPriority.Map = Map;
            ObjectPriority.Priority = Priority;
            Map.SelectedUnitsAction(ObjectPriority);

            SelectedObject_Changed();
            Map.UndoStepCreate("Object Priority Changed");
            View_DrawViewLater();
        }

        public void txtObjectHealth_LostFocus(Object sender, EventArgs e)
        {
            if ( !txtObjectHealth.Enabled )
            {
                return;
            }
            var Map = MainMap;
            if ( Map == null )
            {
                return;
            }
            if ( Map.SelectedUnits.Count <= 0 )
            {
                return;
            }

            double Health = 0;
            if ( !IOUtil.InvariantParse(txtObjectHealth.Text, ref Health) )
            {
                //SelectedObject_Changed()
                //todo
                return;
            }

            Health = MathUtil.ClampDbl(Health, 1.0D, 100.0D) / 100.0D;

            if ( Map.SelectedUnits.Count > 1 )
            {
                if ( MessageBox.Show("Change health of multiple objects?", "", MessageBoxButtons.OKCancel, MessageBoxIcon.Question) != DialogResult.OK )
                {
                    //SelectedObject_Changed()
                    //todo
                    return;
                }
            }

            var ObjectHealth = new clsObjectHealth();
            ObjectHealth.Map = Map;
            ObjectHealth.Health = Health;
            Map.SelectedUnitsAction(ObjectHealth);

            SelectedObject_Changed();
            Map.UndoStepCreate("Object Health Changed");
            View_DrawViewLater();
        }

        public void btnDroidToDesign_Click(Object sender, EventArgs e)
        {
            var Map = MainMap;

            if ( Map == null )
            {
                return;
            }
            if ( Map.SelectedUnits.Count <= 0 )
            {
                return;
            }

            if ( Map.SelectedUnits.Count > 1 )
            {
                if ( MessageBox.Show("Change design of multiple droids?", "", MessageBoxButtons.OKCancel, MessageBoxIcon.Question) != DialogResult.OK )
                {
                    return;
                }
            }
            else
            {
                if ( MessageBox.Show("Change design of a droid?", "", MessageBoxButtons.OKCancel, MessageBoxIcon.Question) != DialogResult.OK )
                {
                    return;
                }
            }

            var ObjectTemplateToDesign = new clsObjectTemplateToDesign();
            ObjectTemplateToDesign.Map = Map;
            Map.SelectedUnitsAction(ObjectTemplateToDesign);

            SelectedObject_Changed();
            if ( ObjectTemplateToDesign.ActionPerformed )
            {
                Map.UndoStepCreate("Object Template Removed");
                View_DrawViewLater();
            }
        }

        public ObjectRotateMode PasteRotateObjects = ObjectRotateMode.Walls;

        public void menuRotateUnits_Click(Object sender, EventArgs e)
        {
            PasteRotateObjects = ObjectRotateMode.All;
            menuRotateUnits.Checked = true;
            menuRotateWalls.Checked = false;
            menuRotateNothing.Checked = false;
        }

        public void menuRotateWalls_Click(Object sender, EventArgs e)
        {
            PasteRotateObjects = ObjectRotateMode.Walls;
            menuRotateUnits.Checked = false;
            menuRotateWalls.Checked = true;
            menuRotateNothing.Checked = false;
        }

        public void menuRotateNothing_Click(Object sender, EventArgs e)
        {
            PasteRotateObjects = ObjectRotateMode.None;
            menuRotateUnits.Checked = false;
            menuRotateWalls.Checked = false;
            menuRotateNothing.Checked = true;
        }

        public void cboDroidBody_SelectedIndexChanged(Object sender, EventArgs e)
        {
            if ( !cboDroidBody.Enabled )
            {
                return;
            }
            if ( cboDroidBody.SelectedIndex < 0 )
            {
                return;
            }
            var Map = MainMap;
            if ( Map == null )
            {
                return;
            }
            if ( Map.SelectedUnits.Count <= 0 )
            {
                return;
            }

            if ( Map.SelectedUnits.Count > 1 )
            {
                if ( MessageBox.Show("Change body of multiple droids?", "", MessageBoxButtons.OKCancel, MessageBoxIcon.Question) != DialogResult.OK )
                {
                    return;
                }
            }

            var ObjectBody = new clsObjectBody();
            ObjectBody.Map = Map;
            ObjectBody.Body = cboBody_Objects[cboDroidBody.SelectedIndex];
            Map.SelectedUnitsAction(ObjectBody);

            SelectedObject_Changed();
            if ( ObjectBody.ActionPerformed )
            {
                Map.UndoStepCreate("Object Body Changed");
                View_DrawViewLater();
            }
        }

        public void cboDroidPropulsion_SelectedIndexChanged(Object sender, EventArgs e)
        {
            if ( !cboDroidPropulsion.Enabled )
            {
                return;
            }
            if ( cboDroidPropulsion.SelectedIndex < 0 )
            {
                return;
            }
            var Map = MainMap;
            if ( Map == null )
            {
                return;
            }
            if ( Map.SelectedUnits.Count <= 0 )
            {
                return;
            }

            if ( Map.SelectedUnits.Count > 1 )
            {
                if ( MessageBox.Show("Change propulsion of multiple droids?", "", MessageBoxButtons.OKCancel, MessageBoxIcon.Question) != DialogResult.OK )
                {
                    return;
                }
            }

            var ObjectPropulsion = new clsObjectPropulsion();
            ObjectPropulsion.Map = Map;
            ObjectPropulsion.Propulsion = cboPropulsion_Objects[cboDroidPropulsion.SelectedIndex];
            Map.SelectedUnitsAction(ObjectPropulsion);

            SelectedObject_Changed();
            if ( ObjectPropulsion.ActionPerformed )
            {
                Map.UndoStepCreate("Object Body Changed");
                View_DrawViewLater();
            }
            SelectedObject_Changed();
            if ( ObjectPropulsion.ActionPerformed )
            {
                Map.UndoStepCreate("Object Propulsion Changed");
                View_DrawViewLater();
            }
        }

        public void cboDroidTurret1_SelectedIndexChanged(Object sender, EventArgs e)
        {
            if ( !cboDroidTurret1.Enabled )
            {
                return;
            }
            if ( cboDroidTurret1.SelectedIndex < 0 )
            {
                return;
            }
            var Map = MainMap;
            if ( Map == null )
            {
                return;
            }
            if ( Map.SelectedUnits.Count <= 0 )
            {
                return;
            }

            if ( Map.SelectedUnits.Count > 1 )
            {
                if ( MessageBox.Show("Change turret of multiple droids?", "", MessageBoxButtons.OKCancel, MessageBoxIcon.Question) != DialogResult.OK )
                {
                    return;
                }
            }

            var ObjectTurret = new clsObjectTurret();
            ObjectTurret.Map = Map;
            ObjectTurret.Turret = cboTurret_Objects[cboDroidTurret1.SelectedIndex];
            ObjectTurret.TurretNum = 0;
            Map.SelectedUnitsAction(ObjectTurret);

            SelectedObject_Changed();
            if ( ObjectTurret.ActionPerformed )
            {
                Map.UndoStepCreate("Object Turret Changed");
                View_DrawViewLater();
            }
        }

        public void cboDroidTurret2_SelectedIndexChanged(Object sender, EventArgs e)
        {
            if ( !cboDroidTurret2.Enabled )
            {
                return;
            }
            if ( cboDroidTurret2.SelectedIndex < 0 )
            {
                return;
            }
            var Map = MainMap;
            if ( Map == null )
            {
                return;
            }
            if ( Map.SelectedUnits.Count <= 0 )
            {
                return;
            }

            if ( Map.SelectedUnits.Count > 1 )
            {
                if ( MessageBox.Show("Change turret of multiple droids?", "", MessageBoxButtons.OKCancel, MessageBoxIcon.Question) != DialogResult.OK )
                {
                    return;
                }
            }

            var ObjectTurret = new clsObjectTurret();
            ObjectTurret.Map = Map;
            ObjectTurret.Turret = cboTurret_Objects[cboDroidTurret2.SelectedIndex];
            ObjectTurret.TurretNum = 1;
            Map.SelectedUnitsAction(ObjectTurret);

            SelectedObject_Changed();
            if ( ObjectTurret.ActionPerformed )
            {
                Map.UndoStepCreate("Object Turret Changed");
                View_DrawViewLater();
            }
        }

        public void cboDroidTurret3_SelectedIndexChanged(Object sender, EventArgs e)
        {
            if ( !cboDroidTurret3.Enabled )
            {
                return;
            }
            if ( cboDroidTurret3.SelectedIndex < 0 )
            {
                return;
            }
            var Map = MainMap;
            if ( Map == null )
            {
                return;
            }
            if ( Map.SelectedUnits.Count <= 0 )
            {
                return;
            }

            if ( Map.SelectedUnits.Count > 1 )
            {
                if ( MessageBox.Show("Change turret of multiple droids?", "", MessageBoxButtons.OKCancel, MessageBoxIcon.Question) != DialogResult.OK )
                {
                    return;
                }
            }

            var ObjectTurret = new clsObjectTurret();
            ObjectTurret.Map = Map;
            ObjectTurret.Turret = cboTurret_Objects[cboDroidTurret3.SelectedIndex];
            ObjectTurret.TurretNum = 2;
            Map.SelectedUnitsAction(ObjectTurret);

            SelectedObject_Changed();
            if ( ObjectTurret.ActionPerformed )
            {
                Map.UndoStepCreate("Object Turret Changed");
                View_DrawViewLater();
            }
        }

        public void rdoDroidTurret0_CheckedChanged(Object sender, EventArgs e)
        {
            if ( !rdoDroidTurret0.Enabled )
            {
                return;
            }
            var Map = MainMap;
            if ( Map == null )
            {
                return;
            }
            if ( Map.SelectedUnits.Count <= 0 )
            {
                return;
            }

            if ( !rdoDroidTurret0.Checked )
            {
                return;
            }

            rdoDroidTurret1.Checked = false;
            rdoDroidTurret2.Checked = false;
            rdoDroidTurret3.Checked = false;

            if ( Map.SelectedUnits.Count > 1 )
            {
                if ( MessageBox.Show("Change number of turrets of multiple droids?", "", MessageBoxButtons.OKCancel, MessageBoxIcon.Question) != DialogResult.OK )
                {
                    return;
                }
            }

            SelectedObjects_SetTurretCount(0);
        }

        public void rdoDroidTurret1_CheckedChanged(Object sender, EventArgs e)
        {
            if ( !rdoDroidTurret1.Enabled )
            {
                return;
            }
            var Map = MainMap;
            if ( Map == null )
            {
                return;
            }
            if ( Map.SelectedUnits.Count <= 0 )
            {
                return;
            }

            if ( !rdoDroidTurret1.Checked )
            {
                return;
            }

            rdoDroidTurret0.Checked = false;
            rdoDroidTurret2.Checked = false;
            rdoDroidTurret3.Checked = false;

            if ( Map.SelectedUnits.Count > 1 )
            {
                if ( MessageBox.Show("Change number of turrets of multiple droids?", "", MessageBoxButtons.OKCancel, MessageBoxIcon.Question) != DialogResult.OK )
                {
                    return;
                }
            }

            SelectedObjects_SetTurretCount(1);
        }

        public void rdoDroidTurret2_CheckedChanged(Object sender, EventArgs e)
        {
            if ( !rdoDroidTurret2.Enabled )
            {
                return;
            }
            var Map = MainMap;
            if ( Map == null )
            {
                return;
            }
            if ( Map.SelectedUnits.Count <= 0 )
            {
                return;
            }

            if ( !rdoDroidTurret2.Checked )
            {
                return;
            }

            rdoDroidTurret0.Checked = false;
            rdoDroidTurret1.Checked = false;
            rdoDroidTurret3.Checked = false;

            if ( Map.SelectedUnits.Count > 1 )
            {
                if ( MessageBox.Show("Change number of turrets of multiple droids?", "", MessageBoxButtons.OKCancel, MessageBoxIcon.Question) != DialogResult.OK )
                {
                    return;
                }
            }

            SelectedObjects_SetTurretCount(2);
        }

        public void rdoDroidTurret3_CheckedChanged(Object sender, EventArgs e)
        {
            if ( !rdoDroidTurret2.Enabled )
            {
                return;
            }
            var Map = MainMap;
            if ( Map == null )
            {
                return;
            }
            if ( Map.SelectedUnits.Count <= 0 )
            {
                return;
            }

            if ( !rdoDroidTurret3.Checked )
            {
                return;
            }

            rdoDroidTurret0.Checked = false;
            rdoDroidTurret1.Checked = false;
            rdoDroidTurret2.Checked = false;

            if ( Map.SelectedUnits.Count > 1 )
            {
                if ( MessageBox.Show("Change number of turrets of multiple droids?", "", MessageBoxButtons.OKCancel, MessageBoxIcon.Question) != DialogResult.OK )
                {
                    return;
                }
            }

            SelectedObjects_SetTurretCount(3);
        }

        private void SelectedObjects_SetTurretCount(byte Count)
        {
            var Map = MainMap;

            if ( Map == null )
            {
                return;
            }

            var ObjectTurretCount = new clsObjectTurretCount();
            ObjectTurretCount.Map = Map;
            ObjectTurretCount.TurretCount = Count;
            Map.SelectedUnitsAction(ObjectTurretCount);

            SelectedObject_Changed();
            if ( ObjectTurretCount.ActionPerformed )
            {
                Map.UndoStepCreate("Object Number Of Turrets Changed");
                View_DrawViewLater();
            }
        }

        private void SelectedObjects_SetDroidType(DroidDesign.clsTemplateDroidType NewType)
        {
            var Map = MainMap;

            if ( Map == null )
            {
                return;
            }

            var ObjectDroidType = new clsObjectDroidType();
            ObjectDroidType.Map = Map;
            ObjectDroidType.DroidType = NewType;
            Map.SelectedUnitsAction(ObjectDroidType);

            SelectedObject_Changed();
            if ( ObjectDroidType.ActionPerformed )
            {
                Map.UndoStepCreate("Object Number Of Turrets Changed");
                View_DrawViewLater();
            }
        }

        public void cboDroidType_SelectedIndexChanged(Object sender, EventArgs e)
        {
            if ( !cboDroidType.Enabled )
            {
                return;
            }
            if ( cboDroidType.SelectedIndex < 0 )
            {
                return;
            }
            var Map = MainMap;
            if ( Map == null )
            {
                return;
            }
            if ( Map.SelectedUnits.Count <= 0 )
            {
                return;
            }

            if ( Map.SelectedUnits.Count > 1 )
            {
                if ( MessageBox.Show("Change type of multiple droids?", "", MessageBoxButtons.OKCancel, MessageBoxIcon.Question) != DialogResult.OK )
                {
                    return;
                }
            }

            SelectedObjects_SetDroidType(App.TemplateDroidTypes[cboDroidType.SelectedIndex]);
        }

        public void rdoTextureIgnoreTerrain_Click(Object sender, EventArgs e)
        {
            if ( rdoTextureIgnoreTerrain.Checked )
            {
                TextureTerrainAction = TextureTerrainAction.Ignore;
                rdoTextureReinterpretTerrain.Checked = false;
                rdoTextureRemoveTerrain.Checked = false;
            }
        }

        public void rdoTextureReinterpretTerrain_Click(Object sender, EventArgs e)
        {
            if ( rdoTextureReinterpretTerrain.Checked )
            {
                TextureTerrainAction = TextureTerrainAction.Reinterpret;
                rdoTextureIgnoreTerrain.Checked = false;
                rdoTextureRemoveTerrain.Checked = false;
            }
        }

        public void rdoTextureRemoveTerrain_Click(Object sender, EventArgs e)
        {
            if ( rdoTextureRemoveTerrain.Checked )
            {
                TextureTerrainAction = TextureTerrainAction.Remove;
                rdoTextureIgnoreTerrain.Checked = false;
                rdoTextureReinterpretTerrain.Checked = false;
            }
        }

        public void btnPlayerSelectObjects_Click(Object sender, EventArgs e)
        {
            var Map = MainMap;

            if ( Map == null )
            {
                return;
            }

//            if ( !KeyboardManager.KeyboardProfile.Active(KeyboardManager.UnitMultiselect) )
//            {
//                Map.SelectedUnits.Clear();
//            }

            var UnitGroup = Map.SelectedUnitGroup.Item;
            var Unit = default(Unit);
            foreach ( var tempLoopVar_Unit in Map.Units )
            {
                Unit = tempLoopVar_Unit;
                if ( Unit.UnitGroup == UnitGroup )
                {
                    if ( Unit.TypeBase.Type != UnitType.Feature )
                    {
                        if ( !Unit.MapSelectedUnitLink.IsConnected )
                        {
                            Unit.MapSelectedUnitLink.Connect(Map.SelectedUnits);
                        }
                    }
                }
            }

            View_DrawViewLater();
        }

//        public void menuOptions_Click(Object sender, EventArgs e)
//        {
//            if ( Program.frmOptionsInstance != null )
//            {
//                Program.frmOptionsInstance.Activate();
//                return;
//            }
//            Program.frmOptionsInstance = new frmOptions();
//            Program.frmOptionsInstance.FormClosing += Program.frmOptionsInstance.frmOptions_FormClosing;
//            Program.frmOptionsInstance.Show();
//        }

        public void rdoCliffTriBrush_CheckedChanged(Object sender, EventArgs e)
        {
            App.ToolOptions.MouseTool = MouseTool.CliffTriangle;
        }

        public void MainMapAfterChanged()
        {
            var Map = MainMap;

            MapViewControl.UpdateTabs();

            App.SelectedTerrain = null;
            App.SelectedRoad = null;

            Resize_Update();
            MainMapTilesetChanged();
            PainterTerrains_Refresh(-1, -1);
            ScriptMarkerLists_Update();

            NewPlayerNumControl.Enabled = false;
            ObjectPlayerNumControl.Enabled = false;
            if ( Map != null )
            {
                Map.CheckMessages();
//                Map.viewInfo.FovCalc();
                Map.SectorGraphicsChanges.SetAllChanged();
                Map.Update(null);
                tsbSave.Enabled = Map.ChangedSinceSave;
                NewPlayerNumControl.SetMap(Map);
                NewPlayerNumControl.Target = Map.SelectedUnitGroup;
                ObjectPlayerNumControl.SetMap(Map);
                MainMap.Changed += MainMap_Modified;
            }
            else
            {
                tsbSave.Enabled = false;
                NewPlayerNumControl.SetMap(null);
                NewPlayerNumControl.Target = null;
                ObjectPlayerNumControl.SetMap(null);
            }
            NewPlayerNumControl.Enabled = true;
            ObjectPlayerNumControl.Enabled = true;

            SelectedObject_Changed();

            TitleTextUpdate();

            TextureViewControl.ScrollUpdate();

            TextureViewControl.DrawViewLater();
            View_DrawViewLater();
        }

        public void MainMapBeforeChanged()
        {
            var Map = MainMap;

            MapViewControl.OpenGLControl.Focus(); //take focus from controls to trigger their lostfocuses

            if ( Map == null )
            {
                return;
            }

            MainMap.Changed -= MainMap_Modified;

            if ( Map.ReadyForUserInput )
            {
                Map.SectorAll_GLLists_Delete();
                Map.SectorGraphicsChanges.SetAllChanged();
            }
        }

        private void MainMap_Modified()
        {
            tsbSave.Enabled = true;
        }

        public void ObjectPicker(UnitTypeBase unitTypeBase)
        {
            App.ToolOptions.MouseTool = MouseTool.ObjectPlace;
//            if ( !KeyboardManager.KeyboardProfile.Active(KeyboardManager.UnitMultiselect) )
//            {
//                dgvFeatures.ClearSelection();
//                dgvStructures.ClearSelection();
//                dgvDroids.ClearSelection();
//            }
            SelectedObjectTypes.Clear();
            SelectedObjectTypes.Add(unitTypeBase.UnitType_frmMainSelectedLink);
            var Map = MainMap;
            if ( Map != null )
            {
                View_DrawViewLater();
            }
        }

        public void TerrainPicker()
        {
            // TODO: Implement me.
//            var Map = MainMap;
//
//            var MouseOverTerrain = Map.viewInfo.GetMouseOverTerrain();
//
//            if ( MouseOverTerrain == null )
//            {
//                return;
//            }
//
//            var Vertex = MouseOverTerrain.Vertex.Normal;
//            var A = 0;
//
//            lstAutoTexture.Enabled = false;
//            for ( A = 0; A <= lstAutoTexture.Items.Count - 1; A++ )
//            {
//                if ( Map.Painter.Terrains[A] == Map.Terrain.Vertices[Vertex.X, Vertex.Y].Terrain )
//                {
//                    lstAutoTexture.SelectedIndex = A;
//                    break;
//                }
//            }
//            if ( A == lstAutoTexture.Items.Count )
//            {
//                lstAutoTexture.SelectedIndex = -1;
//            }
//            lstAutoTexture.Enabled = true;
//            App.SelectedTerrain = Map.Terrain.Vertices[Vertex.X, Vertex.Y].Terrain;
        }

        public void TexturePicker()
        {
            // TODO: Implement me.
//            var Map = MainMap;
//
//            var MouseOverTerrain = Map.viewInfo.GetMouseOverTerrain();
//
//            if ( MouseOverTerrain == null )
//            {
//                return;
//            }
//
//            var Tile = MouseOverTerrain.Tile.Normal;
//
//            if ( Map.Tileset != null )
//            {
//                if ( Map.Terrain.Tiles[Tile.X, Tile.Y].Texture.TextureNum < Map.Tileset.Tiles.Count )
//                {
//                    App.SelectedTextureNum = Map.Terrain.Tiles[Tile.X, Tile.Y].Texture.TextureNum;
//                    TextureViewControl.DrawViewLater();
//                }
//            }
//
////            if ( SettingsManager.Settings.PickOrientation )
////            {
////                App.TextureOrientation = Map.Terrain.Tiles[Tile.X, Tile.Y].Texture.Orientation;
////                TextureViewControl.DrawViewLater();
////            }
        }

        public void HeightPickerL()
        {
            // TODO: Implement me.
//            var Map = MainMap;
//
//            var MouseOverTerrain = Map.viewInfo.GetMouseOverTerrain();
//
//            if ( MouseOverTerrain == null )
//            {
//                return;
//            }
//
//            txtHeightSetL.Text =
//                Convert.ToByte(Map.Terrain.Vertices[MouseOverTerrain.Vertex.Normal.X, MouseOverTerrain.Vertex.Normal.Y].Height).ToStringInvariant();
//            txtHeightSetL.Focus();
//            MapViewControl.OpenGLControl.Focus();
        }

        public void HeightPickerR()
        {
            // TODO: Implement me.
//            var Map = MainMap;
//
//            var MouseOverTerrain = Map.viewInfo.GetMouseOverTerrain();
//            if ( MouseOverTerrain == null )
//            {
//                return;
//            }
//
//            txtHeightSetR.Text = Map.Terrain.Vertices[MouseOverTerrain.Vertex.Normal.X, MouseOverTerrain.Vertex.Normal.Y].Height.ToStringInvariant();
//            txtHeightSetR.Focus();
//            MapViewControl.OpenGLControl.Focus();
        }

        public void OpenGL_DragEnter(object sender, DragEventArgs e)
        {
            if ( e.Data.GetDataPresent(DataFormats.FileDrop) )
            {
                e.Effect = DragDropEffects.Copy;
            }
            else
            {
                e.Effect = DragDropEffects.None;
            }
        }

        public void OpenGL_DragDrop(object sender, DragEventArgs e)
        {
            var Paths = (string[])(e.Data.GetData(DataFormats.FileDrop));
            var Result = new Result("Loading drag-dropped maps", false);
            logger.Info("Loading drag-dropped maps");
            var Path = "";

            foreach ( var tempLoopVar_Path in Paths )
            {
                Path = tempLoopVar_Path;
                Result.Take(LoadMap(Path));
            }
            App.ShowWarnings(Result);
        }

        public void btnFlatSelected_Click(object sender, EventArgs e)
        {
            var Map = MainMap;

            if ( Map == null )
            {
                return;
            }

            var FlattenTool = new clsObjectFlattenTerrain();
            Map.SelectedUnits.CopyList().PerformTool(FlattenTool);

            Map.Update(null);
            Map.UndoStepCreate("Flatten Under Structures");
        }

        public void rdoFillCliffIgnore_CheckedChanged(Object sender, EventArgs e)
        {
            FillCliffAction = FillCliffAction.Ignore;
        }

        public void rdoFillCliffStopBefore_CheckedChanged(Object sender, EventArgs e)
        {
            FillCliffAction = FillCliffAction.StopBefore;
        }

        public void rdoFillCliffStopAfter_CheckedChanged(Object sender, EventArgs e)
        {
            FillCliffAction = FillCliffAction.StopAfter;
        }

        public void btnScriptAreaCreate_Click(Object sender, EventArgs e)
        {
            if ( !btnScriptAreaCreate.Enabled )
            {
                return;
            }
            var Map = MainMap;
            if ( Map == null )
            {
                return;
            }

            if ( Map.SelectedAreaVertexA == null )
            {
                MessageBox.Show("Select something first.");
                return;
            }
            if ( Map.SelectedAreaVertexB == null )
            {
                MessageBox.Show("Select something first.");
                return;
            }

            var NewArea = new clsScriptArea(Map);
            if ( NewArea == null )
            {
                MessageBox.Show("Error: Creating area failed.");
                return;
            }

            NewArea.SetPositions(
                new XYInt(Map.SelectedAreaVertexA.X * Constants.TerrainGridSpacing, Map.SelectedAreaVertexA.Y * Constants.TerrainGridSpacing),
                new XYInt(Map.SelectedAreaVertexB.X * Constants.TerrainGridSpacing, Map.SelectedAreaVertexB.Y * Constants.TerrainGridSpacing));

            ScriptMarkerLists_Update();

            Map.SetChanged(); //todo: remove if areas become undoable
            View_DrawViewLater();
        }

        private clsScriptPosition[] lstScriptPositions_Objects = new clsScriptPosition[0];
        private clsScriptArea[] lstScriptAreas_Objects = new clsScriptArea[0];

        public void ScriptMarkerLists_Update()
        {
            var Map = MainMap;

            lstScriptPositions.Enabled = false;
            lstScriptAreas.Enabled = false;

            lstScriptPositions.Items.Clear();
            lstScriptAreas.Items.Clear();

            if ( Map == null )
            {
                _SelectedScriptMarker = null;
                return;
            }

            var ListPosition = 0;
            var ScriptPosition = default(clsScriptPosition);
            var ScriptArea = default(clsScriptArea);
            object NewSelectedScriptMarker = null;

            lstScriptPositions_Objects = new clsScriptPosition[Map.ScriptPositions.Count];
            lstScriptAreas_Objects = new clsScriptArea[Map.ScriptAreas.Count];

            foreach ( var tempLoopVar_ScriptPosition in Map.ScriptPositions )
            {
                ScriptPosition = tempLoopVar_ScriptPosition;
                ListPosition = lstScriptPositions.Items.Add(ScriptPosition.Label);
                lstScriptPositions_Objects[ListPosition] = ScriptPosition;
                if ( ScriptPosition == _SelectedScriptMarker )
                {
                    NewSelectedScriptMarker = ScriptPosition;
                    lstScriptPositions.SelectedIndex = ListPosition;
                }
            }

            foreach ( var tempLoopVar_ScriptArea in Map.ScriptAreas )
            {
                ScriptArea = tempLoopVar_ScriptArea;
                ListPosition = lstScriptAreas.Items.Add(ScriptArea.Label);
                lstScriptAreas_Objects[ListPosition] = ScriptArea;
                if ( ScriptArea == _SelectedScriptMarker )
                {
                    NewSelectedScriptMarker = ScriptArea;
                    lstScriptAreas.SelectedIndex = ListPosition;
                }
            }

            lstScriptPositions.Enabled = true;
            lstScriptAreas.Enabled = true;

            _SelectedScriptMarker = NewSelectedScriptMarker;

            SelectedScriptMarker_Update();
        }

        private object _SelectedScriptMarker;

        public object SelectedScriptMarker
        {
            get { return _SelectedScriptMarker; }
        }

        public void lstScriptPositions_SelectedIndexChanged(Object sender, EventArgs e)
        {
            if ( !lstScriptPositions.Enabled )
            {
                return;
            }

            _SelectedScriptMarker = lstScriptPositions_Objects[lstScriptPositions.SelectedIndex];

            lstScriptAreas.Enabled = false;
            lstScriptAreas.SelectedIndex = -1;
            lstScriptAreas.Enabled = true;

            SelectedScriptMarker_Update();

            View_DrawViewLater();
        }

        public void lstScriptAreas_SelectedIndexChanged(Object sender, EventArgs e)
        {
            if ( !lstScriptAreas.Enabled )
            {
                return;
            }

            _SelectedScriptMarker = lstScriptAreas_Objects[lstScriptAreas.SelectedIndex];

            lstScriptPositions.Enabled = false;
            lstScriptPositions.SelectedIndex = -1;
            lstScriptPositions.Enabled = true;

            SelectedScriptMarker_Update();

            View_DrawViewLater();
        }

        public void SelectedScriptMarker_Update()
        {
            txtScriptMarkerLabel.Enabled = false;
            txtScriptMarkerX.Enabled = false;
            txtScriptMarkerY.Enabled = false;
            txtScriptMarkerX2.Enabled = false;
            txtScriptMarkerY2.Enabled = false;
            txtScriptMarkerLabel.Text = "";
            txtScriptMarkerX.Text = "";
            txtScriptMarkerY.Text = "";
            txtScriptMarkerX2.Text = "";
            txtScriptMarkerY2.Text = "";
            if ( _SelectedScriptMarker != null )
            {
                if ( _SelectedScriptMarker is clsScriptPosition )
                {
                    var ScriptPosition = (clsScriptPosition)_SelectedScriptMarker;
                    txtScriptMarkerLabel.Text = ScriptPosition.Label;
                    txtScriptMarkerX.Text = ScriptPosition.PosX.ToStringInvariant();
                    txtScriptMarkerY.Text = ScriptPosition.PosY.ToStringInvariant();
                    txtScriptMarkerLabel.Enabled = true;
                    txtScriptMarkerX.Enabled = true;
                    txtScriptMarkerY.Enabled = true;
                }
                else if ( _SelectedScriptMarker is clsScriptArea )
                {
                    var ScriptArea = (clsScriptArea)_SelectedScriptMarker;
                    txtScriptMarkerLabel.Text = ScriptArea.Label;
                    txtScriptMarkerX.Text = ScriptArea.PosAX.ToStringInvariant();
                    txtScriptMarkerY.Text = ScriptArea.PosAY.ToStringInvariant();
                    txtScriptMarkerX2.Text = ScriptArea.PosBX.ToStringInvariant();
                    txtScriptMarkerY2.Text = ScriptArea.PosBY.ToStringInvariant();
                    txtScriptMarkerLabel.Enabled = true;
                    txtScriptMarkerX.Enabled = true;
                    txtScriptMarkerY.Enabled = true;
                    txtScriptMarkerX2.Enabled = true;
                    txtScriptMarkerY2.Enabled = true;
                }
            }
        }

        public void btnScriptMarkerRemove_Click(Object sender, EventArgs e)
        {
            if ( _SelectedScriptMarker == null )
            {
                return;
            }

            var Map = MainMap;
            if ( Map == null )
            {
                return;
            }

            var Number = 0;
            if ( _SelectedScriptMarker is clsScriptPosition )
            {
                var ScripPosition = (clsScriptPosition)_SelectedScriptMarker;
                Number = ScripPosition.ParentMap.Position;
                ScripPosition.Deallocate();
                if ( Map.ScriptPositions.Count > 0 )
                {
                    _SelectedScriptMarker = Map.ScriptPositions[MathUtil.ClampInt(Number, 0, Map.ScriptPositions.Count - 1)];
                }
                else
                {
                    _SelectedScriptMarker = null;
                }
            }
            else if ( _SelectedScriptMarker is clsScriptArea )
            {
                var ScriptArea = (clsScriptArea)_SelectedScriptMarker;
                Number = ScriptArea.ParentMap.Position;
                ScriptArea.Deallocate();
                if ( Map.ScriptAreas.Count > 0 )
                {
                    _SelectedScriptMarker = Map.ScriptAreas[MathUtil.ClampInt(Number, 0, Map.ScriptAreas.Count - 1)];
                }
                else
                {
                    _SelectedScriptMarker = null;
                }
            }

            ScriptMarkerLists_Update();

            View_DrawViewLater();
        }

        public void txtScriptMarkerLabel_LostFocus(Object sender, EventArgs e)
        {
            if ( !txtScriptMarkerLabel.Enabled )
            {
                return;
            }
            if ( _SelectedScriptMarker == null )
            {
                return;
            }

            var Result = new SimpleResult();
            if ( _SelectedScriptMarker is clsScriptPosition )
            {
                var ScriptPosition = (clsScriptPosition)_SelectedScriptMarker;
                if ( ScriptPosition.Label == txtScriptMarkerLabel.Text )
                {
                    return;
                }
                Result = ScriptPosition.SetLabel(txtScriptMarkerLabel.Text);
            }
            else if ( _SelectedScriptMarker is clsScriptArea )
            {
                var ScriptArea = (clsScriptArea)_SelectedScriptMarker;
                if ( ScriptArea.Label == txtScriptMarkerLabel.Text )
                {
                    return;
                }
                Result = ScriptArea.SetLabel(txtScriptMarkerLabel.Text);
            }
            else
            {
                return;
            }

            if ( !Result.Success )
            {
                MessageBox.Show("Unable to change label: " + Result.Problem);
                SelectedScriptMarker_Update();
                return;
            }

            ScriptMarkerLists_Update();
        }

        public void txtScriptMarkerX_LostFocus(Object sender, EventArgs e)
        {
            if ( !txtScriptMarkerX.Enabled )
            {
                return;
            }
            if ( _SelectedScriptMarker == null )
            {
                return;
            }

            if ( _SelectedScriptMarker is clsScriptPosition )
            {
                var ScriptPosition = (clsScriptPosition)_SelectedScriptMarker;
                var temp_Result = ScriptPosition.PosX;
                IOUtil.InvariantParse(txtScriptMarkerX.Text, ref temp_Result);
                ScriptPosition.PosX = temp_Result;
            }
            else if ( _SelectedScriptMarker is clsScriptArea )
            {
                var ScriptArea = (clsScriptArea)_SelectedScriptMarker;
                var temp_Result2 = ScriptArea.PosAX;
                IOUtil.InvariantParse(txtScriptMarkerX.Text, ref temp_Result2);
                ScriptArea.PosAX = temp_Result2;
            }
            else
            {
                MessageBox.Show("Error: unhandled type.");
            }

            SelectedScriptMarker_Update();
            View_DrawViewLater();
        }

        public void txtScriptMarkerY_LostFocus(Object sender, EventArgs e)
        {
            if ( !txtScriptMarkerY.Enabled )
            {
                return;
            }
            if ( _SelectedScriptMarker == null )
            {
                return;
            }

            if ( _SelectedScriptMarker is clsScriptPosition )
            {
                var ScriptPosition = (clsScriptPosition)_SelectedScriptMarker;
                var temp_Result = ScriptPosition.PosY;
                IOUtil.InvariantParse(txtScriptMarkerY.Text, ref temp_Result);
                ScriptPosition.PosY = temp_Result;
            }
            else if ( _SelectedScriptMarker is clsScriptArea )
            {
                var ScriptArea = (clsScriptArea)_SelectedScriptMarker;
                var temp_Result2 = ScriptArea.PosAY;
                IOUtil.InvariantParse(txtScriptMarkerY.Text, ref temp_Result2);
                ScriptArea.PosAY = temp_Result2;
            }
            else
            {
                MessageBox.Show("Error: unhandled type.");
            }

            SelectedScriptMarker_Update();
            View_DrawViewLater();
        }

        public void txtScriptMarkerX2_LostFocus(Object sender, EventArgs e)
        {
            if ( !txtScriptMarkerX2.Enabled )
            {
                return;
            }
            if ( _SelectedScriptMarker == null )
            {
                return;
            }

            if ( _SelectedScriptMarker is clsScriptArea )
            {
                var ScriptArea = (clsScriptArea)_SelectedScriptMarker;
                var temp_Result = ScriptArea.PosBX;
                IOUtil.InvariantParse(txtScriptMarkerX2.Text, ref temp_Result);
                ScriptArea.PosBX = temp_Result;
            }
            else
            {
                MessageBox.Show("Error: unhandled type.");
            }

            SelectedScriptMarker_Update();
            View_DrawViewLater();
        }

        public void txtScriptMarkerY2_LostFocus(Object sender, EventArgs e)
        {
            if ( !txtScriptMarkerY2.Enabled )
            {
                return;
            }
            if ( _SelectedScriptMarker == null )
            {
                return;
            }

            if ( _SelectedScriptMarker is clsScriptArea )
            {
                var ScriptArea = (clsScriptArea)_SelectedScriptMarker;
                var temp_Result = ScriptArea.PosBY;
                IOUtil.InvariantParse(txtScriptMarkerY2.Text, ref temp_Result);
                ScriptArea.PosBY = temp_Result;
            }
            else
            {
                MessageBox.Show("Error: unhandled type.");
            }

            SelectedScriptMarker_Update();
            View_DrawViewLater();
        }

        public void txtObjectLabel_LostFocus(Object sender, EventArgs e)
        {
            if ( !txtObjectLabel.Enabled )
            {
                return;
            }
            var Map = MainMap;
            if ( Map == null )
            {
                return;
            }
            if ( Map.SelectedUnits.Count != 1 )
            {
                return;
            }

            if ( txtObjectLabel.Text == Map.SelectedUnits[0].Label )
            {
                return;
            }

            var OldUnit = Map.SelectedUnits[0];
            var ResultUnit = new Unit(OldUnit, Map);
            Map.UnitSwap(OldUnit, ResultUnit);
            var Result = ResultUnit.SetLabel(txtObjectLabel.Text);
            if ( !Result.Success )
            {
                MessageBox.Show("Unable to set label: " + Result.Problem);
            }

            Map.SelectedUnits.Clear();
            ResultUnit.MapSelect();

            SelectedObject_Changed();

            Map.UndoStepCreate("Object Label Changed");
            View_DrawViewLater();
        }

        public void NewMainMap(Map NewMap)
        {
            SetMainMap(NewMap);
        }

        public Map MainMap
        {
            get { return _LoadedMaps.MainMap; }
        }

        public clsMaps LoadedMaps
        {
            get { return _LoadedMaps; }
        }

        public void SetMainMap(Map Map)
        {
            _LoadedMaps.MainMap = Map;
        }

        public Result LoadMap(string Path)
        {
            var ReturnResult = new Result("", false);
            var SplitPath = new sSplitPath(Path);
            var resultMap = App.Kernel.Get<Map>();

            return ReturnResult;
        }

        public void Load_Autosave_Prompt()
        {
            if ( !Directory.Exists(App.AutoSavePath) )
            {
                MessageBox.Show("Autosave directory does not exist. There are no autosaves.", "", MessageBoxButtons.OK, MessageBoxIcon.None);
                return;
            }
            var Dialog = new OpenFileDialog();

            Dialog.FileName = "";
            Dialog.Filter = Constants.ProgramName + " Files (*.fmap, *.fme)|*.fmap;*.fme|All Files (*.*)|*.*";
            Dialog.InitialDirectory = App.AutoSavePath;
            if ( Dialog.ShowDialog(this) != DialogResult.OK )
            {
                return;
            }
            App.SettingsManager.OpenPath = Path.GetDirectoryName(Dialog.FileName);
            var Result = new Result("Loading map", false);
            Result.Take(LoadMap(Dialog.FileName));
            App.ShowWarnings(Result);
        }

        public void btnAlignObjects_Click(Object sender, EventArgs e)
        {
            var Map = MainMap;

            if ( Map == null )
            {
                return;
            }

            var AlignTool = new clsObjectAlignment();
            AlignTool.Map = Map;
            Map.SelectedUnits.CopyList().PerformTool(AlignTool);

            Map.Update(null);
            Map.UndoStepCreate("Align Objects");
        }

        public void cbxDesignableOnly_CheckedChanged(Object sender, EventArgs e)
        {
            Components_Update();
        }

        private void ObjectsUpdate()
        {
            if ( !App.ProgramInitializeFinished )
            {
                return;
            }

            switch ( TabControl1.SelectedIndex )
            {
                case 0:
                    ObjectListFill(App.ObjectData.FeatureTypes.CopyList(), dgvFeatures);
                    break;
                case 1:
                    ObjectListFill(App.ObjectData.StructureTypes.CopyList(), dgvStructures);
                    break;
                case 2:
                    ObjectListFill(App.ObjectData.DroidTemplates.CopyList(), dgvDroids);
                    break;
            }
        }

        public void txtObjectFind_KeyDown(object sender, KeyEventArgs e)
        {
            if ( !txtObjectFind.Enabled )
            {
                return;
            }

            if ( e.KeyCode != Keys.Enter )
            {
                return;
            }

            txtObjectFind.Enabled = false;

            ObjectsUpdate();

            txtObjectFind.Enabled = true;
        }

        public void txtObjectFind_Leave(object sender, EventArgs e)
        {
            if ( !txtObjectFind.Enabled )
            {
                return;
            }

            txtObjectFind.Enabled = false;

            ObjectsUpdate();

            txtObjectFind.Enabled = true;
        }

        public void rdoObjectPlace_Click(Object sender, EventArgs e)
        {
            App.ToolOptions.MouseTool = MouseTool.ObjectPlace;
        }

        public void rdoObjectLine_Click(Object sender, EventArgs e)
        {
            App.ToolOptions.MouseTool = MouseTool.ObjectLines;
        }

        private void ActivateObjectTool()
        {
            if ( rdoObjectPlace.Checked )
            {
                App.ToolOptions.MouseTool = MouseTool.ObjectPlace;
            }
            else if ( rdoObjectLines.Checked )
            {
                App.ToolOptions.MouseTool = MouseTool.ObjectLines;
            }
        }

        public void TabControl1_SelectedIndexChanged(object sender, EventArgs e)
        {
            ActivateObjectTool();
            ObjectsUpdate();
        }

        public void GeneratorToolStripMenuItem_Click(Object sender, EventArgs e)
        {
            Program.frmGeneratorInstance.Show();
            Program.frmGeneratorInstance.Activate();
        }

        public void ReinterpretTerrainToolStripMenuItem_Click(Object sender, EventArgs e)
        {
            var Map = MainMap;

            if ( Map == null )
            {
                return;
            }

            Map.TerrainInterpretChanges.SetAllChanged();

            Map.Update(null);

            Map.UndoStepCreate("Interpret Terrain");
        }

        public void WaterTriangleCorrectionToolStripMenuItem_Click(Object sender, EventArgs e)
        {
            var Map = MainMap;

            if ( Map == null )
            {
                return;
            }

            Map.WaterTriCorrection();

            Map.Update(null);

            Map.UndoStepCreate("Water Triangle Correction");

            View_DrawViewLater();
        }

        public void ToolStripMenuItem5_Click(Object sender, EventArgs e)
        {
            var Map = MainMap;

            if ( Map == null )
            {
                return;
            }

            if ( DefaultGenerator.UnitTypeBaseOilResource == null )
            {
                MessageBox.Show("Unable. Oil resource is not loaded.");
                return;
            }

            var OilList = new ObservableCollection<Unit>();
            var Unit = default(Unit);
            foreach ( var tempLoopVar_Unit in Map.Units )
            {
                Unit = tempLoopVar_Unit;
                if ( Unit.TypeBase == DefaultGenerator.UnitTypeBaseOilResource )
                {
                    OilList.Add(Unit);
                }
            }
            var FlattenTool = new clsObjectFlattenTerrain();
            OilList.PerformTool(FlattenTool);

            Map.Update(null);
            Map.UndoStepCreate("Flatten Under Oil");
        }

        public void ToolStripMenuItem6_Click(Object sender, EventArgs e)
        {
            var Map = MainMap;

            if ( Map == null )
            {
                return;
            }

            var StructureList = new ObservableCollection<Unit>();
            var Unit = default(Unit);
            foreach ( var tempLoopVar_Unit in Map.Units )
            {
                Unit = tempLoopVar_Unit;
                if ( Unit.TypeBase.Type == UnitType.PlayerStructure )
                {
                    StructureList.Add(Unit);
                }
            }
            var FlattenTool = new clsObjectFlattenTerrain();
            StructureList.PerformTool(FlattenTool);

            Map.Update(null);
            Map.UndoStepCreate("Flatten Under Structures");
        }

        public void btnObjectTypeSelect_Click(Object sender, EventArgs e)
        {
            var Map = MainMap;

            if ( Map == null )
            {
                return;
            }

//            if ( !KeyboardManager.KeyboardProfile.Active(KeyboardManager.UnitMultiselect) )
//            {
//                Map.SelectedUnits.Clear();
//            }
            foreach ( var Unit in Map.Units )
            {
                if ( Unit.TypeBase.UnitType_frmMainSelectedLink.IsConnected )
                {
                    if ( !Unit.MapSelectedUnitLink.IsConnected )
                    {
                        Unit.MapSelectedUnitLink.Connect(Map.SelectedUnits);
                    }
                }
            }

            View_DrawViewLater();
        }

        public UnitTypeBase SingleSelectedObjectTypeBase
        {
            get
            {
                if ( SelectedObjectTypes.Count == 1 )
                {
                    return SelectedObjectTypes[0];
                }
                return null;
            }
        }

        private void ObjectTypeSelectionUpdate(DataGridView dataView)
        {
            SelectedObjectTypes.Clear();
            foreach ( DataGridViewRow selection in dataView.SelectedRows )
            {
                var objectTypeBase = (UnitTypeBase)(selection.Cells[0].Value);
                if ( !objectTypeBase.UnitType_frmMainSelectedLink.IsConnected )
                {
                    SelectedObjectTypes.Add(objectTypeBase.UnitType_frmMainSelectedLink);
                }
            }
            var Map = MainMap;
            if ( Map != null )
            {
                View_DrawViewLater();
            }
        }

        public void dgvFeatures_SelectionChanged(object sender, EventArgs e)
        {
            ActivateObjectTool();
            ObjectTypeSelectionUpdate(dgvFeatures);
        }

        public void dgvStructures_SelectionChanged(object sender, EventArgs e)
        {
            ActivateObjectTool();
            ObjectTypeSelectionUpdate(dgvStructures);
        }

        public void dgvDroids_SelectionChanged(object sender, EventArgs e)
        {
            ActivateObjectTool();
            ObjectTypeSelectionUpdate(dgvDroids);
        }
    }
}