#region

using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using Ionic.Zip;
using Ionic.Zlib;
using SharpFlame.Core.Domain;
using SharpFlame.Core.Parsers.Ini;
using SharpFlame.Domain;
using SharpFlame.FileIO;
using SharpFlame.Mapping.FMap;
using SharpFlame.Mapping.Objects;
using SharpFlame.Mapping.Script;
using SharpFlame.Mapping.Tiles;
using IniReader = SharpFlame.FileIO.Ini.IniReader;
using Section = SharpFlame.FileIO.Ini.Section;

#endregion

namespace SharpFlame.Mapping
{
    public partial class clsMap
    {
        public clsResult Write_FMap(string path, bool overwrite, bool compress)
        {
            var returnResult = new clsResult(string.Format("Writing FMap to \"{0}\"", path), false);
            logger.Info(string.Format("Writing FMap to \"{0}\"", path));

            if ( !overwrite )
            {
                if ( File.Exists(path) )
                {
                    returnResult.ProblemAdd("The file already exists");
                    return returnResult;
                }
            }
            try
            {
                using ( var zip = new ZipOutputStream(path) )
                {
                    // Set encoding
                    zip.AlternateEncoding = Encoding.GetEncoding("UTF-8");
                    zip.AlternateEncodingUsage = ZipOption.Always;

                    // Set compression
                    if ( compress )
                    {
                        zip.CompressionLevel = CompressionLevel.BestCompression;
                    }
                    else
                    {
                        zip.CompressionMethod = CompressionMethod.None;
                    }

                    var binaryWriter = new BinaryWriter(zip, App.UTF8Encoding);
                    var streamWriter = new StreamWriter(zip, App.UTF8Encoding);

                    zip.PutNextEntry("Info.ini");
                    var infoIniWriter = new IniWriter(streamWriter);
                    returnResult.Add(Serialize_FMap_Info(infoIniWriter));
                    streamWriter.Flush();

                    zip.PutNextEntry("VertexHeight.dat");
                    returnResult.Add(Serialize_FMap_VertexHeight(binaryWriter));
                    binaryWriter.Flush();

                    zip.PutNextEntry("VertexTerrain.dat");
                    returnResult.Add(Serialize_FMap_VertexTerrain(binaryWriter));
                    binaryWriter.Flush();

                    zip.PutNextEntry("TileTexture.dat");
                    returnResult.Add(Serialize_FMap_TileTexture(binaryWriter));
                    binaryWriter.Flush();

                    zip.PutNextEntry("TileOrientation.dat");
                    returnResult.Add(Serialize_FMap_TileOrientation(binaryWriter));
                    binaryWriter.Flush();

                    zip.PutNextEntry("TileCliff.dat");
                    returnResult.Add(Serialize_FMap_TileCliff(binaryWriter));
                    binaryWriter.Flush();

                    zip.PutNextEntry("Roads.dat");
                    returnResult.Add(Serialize_FMap_Roads(binaryWriter));
                    binaryWriter.Flush();

                    zip.PutNextEntry("Objects.ini");
                    var objectsIniWriter = new IniWriter(streamWriter);
                    returnResult.Add(Serialize_FMap_Objects(objectsIniWriter));
                    streamWriter.Flush();

                    zip.PutNextEntry("Gateways.ini");
                    var gatewaysIniWriter = new IniWriter(streamWriter);
                    returnResult.Add(Serialize_FMap_Gateways(gatewaysIniWriter));
                    streamWriter.Flush();

                    zip.PutNextEntry("TileTypes.dat");
                    returnResult.Add(Serialize_FMap_TileTypes(binaryWriter));
                    binaryWriter.Flush();

                    zip.PutNextEntry("ScriptLabels.ini");
                    var scriptLabelsIniWriter = new IniWriter(streamWriter);
                    returnResult.Add(Serialize_WZ_LabelsINI(scriptLabelsIniWriter));
                    streamWriter.Flush();

                    streamWriter.Close();
                    binaryWriter.Close();
                }
            }
            catch ( Exception ex )
            {
                returnResult.ProblemAdd(ex.Message);
                return returnResult;
            }

            return returnResult;
        }

        public clsResult Serialize_WZ_LabelsINI(IniWriter File)
        {
            var returnResult = new clsResult("Serializing labels INI", false);
            logger.Info("Serializing labels INI");

            try
            {
                var scriptPosition = default(clsScriptPosition);
                foreach ( var tempLoopVar_ScriptPosition in ScriptPositions )
                {
                    scriptPosition = tempLoopVar_ScriptPosition;
                    scriptPosition.WriteWZ(File);
                }
                var ScriptArea = default(clsScriptArea);
                foreach ( var tempLoopVar_ScriptArea in ScriptAreas )
                {
                    ScriptArea = tempLoopVar_ScriptArea;
                    ScriptArea.WriteWZ(File);
                }
            }
            catch ( Exception ex )
            {
                returnResult.WarningAdd(ex.Message);
                logger.ErrorException("Got an exception", ex);
            }

            return returnResult;
        }

        public clsResult Serialize_FMap_Info(IniWriter File)
        {
            var ReturnResult = new clsResult("Serializing general map info", false);
            logger.Info("Serializing general map info");

            try
            {
                if ( Tileset == null )
                {
                }
                else if ( Tileset == App.Tileset_Arizona )
                {
                    File.AddProperty("Tileset", "Arizona");
                }
                else if ( Tileset == App.Tileset_Urban )
                {
                    File.AddProperty("Tileset", "Urban");
                }
                else if ( Tileset == App.Tileset_Rockies )
                {
                    File.AddProperty("Tileset", "Rockies");
                }

                File.AddProperty("Size", Terrain.TileSize.X.ToStringInvariant() + ", " + Terrain.TileSize.Y.ToStringInvariant());

                File.AddProperty("AutoScrollLimits", InterfaceOptions.AutoScrollLimits.ToStringInvariant());
                File.AddProperty("ScrollMinX", InterfaceOptions.ScrollMin.X.ToStringInvariant());
                File.AddProperty("ScrollMinY", InterfaceOptions.ScrollMin.Y.ToStringInvariant());
                File.AddProperty("ScrollMaxX", InterfaceOptions.ScrollMax.X.ToStringInvariant());
                File.AddProperty("ScrollMaxY", InterfaceOptions.ScrollMax.Y.ToStringInvariant());

                File.AddProperty("Name", InterfaceOptions.CompileName);
                File.AddProperty("Players", InterfaceOptions.CompileMultiPlayers);
                File.AddProperty("XPlayerLev", InterfaceOptions.CompileMultiXPlayers.ToStringInvariant());
                File.AddProperty("Author", InterfaceOptions.CompileMultiAuthor);
                File.AddProperty("License", InterfaceOptions.CompileMultiLicense);
                if ( InterfaceOptions.CampaignGameType >= 0 )
                {
                    File.AddProperty("CampType", InterfaceOptions.CampaignGameType.ToStringInvariant());
                }
            }
            catch ( Exception ex )
            {
                ReturnResult.ProblemAdd(ex.Message);
            }

            return ReturnResult;
        }

        public clsResult Serialize_FMap_VertexHeight(BinaryWriter File)
        {
            var ReturnResult = new clsResult("Serializing vertex heights", false);
            logger.Info("Serializing vertex heights");
            var X = 0;
            var Y = 0;

            try
            {
                for ( Y = 0; Y <= Terrain.TileSize.Y; Y++ )
                {
                    for ( X = 0; X <= Terrain.TileSize.X; X++ )
                    {
                        File.Write(Terrain.Vertices[X, Y].Height);
                    }
                }
            }
            catch ( Exception ex )
            {
                ReturnResult.ProblemAdd(ex.Message);
            }

            return ReturnResult;
        }

        public clsResult Serialize_FMap_VertexTerrain(BinaryWriter File)
        {
            var ReturnResult = new clsResult("Serializing vertex terrain", false);
            logger.Info("Serializing vertex terrain");

            var X = 0;
            var Y = 0;
            var ErrorCount = 0;
            var Value = 0;

            try
            {
                for ( Y = 0; Y <= Terrain.TileSize.Y; Y++ )
                {
                    for ( X = 0; X <= Terrain.TileSize.X; X++ )
                    {
                        if ( Terrain.Vertices[X, Y].Terrain == null )
                        {
                            Value = 0;
                        }
                        else if ( Terrain.Vertices[X, Y].Terrain.Num < 0 )
                        {
                            ErrorCount++;
                            Value = 0;
                        }
                        else
                        {
                            Value = Convert.ToInt32(Terrain.Vertices[X, Y].Terrain.Num + 1);
                            if ( Value > 255 )
                            {
                                ErrorCount++;
                                Value = 0;
                            }
                        }
                        File.Write((byte)Value);
                    }
                }
            }
            catch ( Exception ex )
            {
                ReturnResult.ProblemAdd(ex.Message);
            }

            if ( ErrorCount > 0 )
            {
                ReturnResult.WarningAdd(ErrorCount + " vertices had an invalid painted terrain number.");
            }

            return ReturnResult;
        }

        public clsResult Serialize_FMap_TileTexture(BinaryWriter File)
        {
            var ReturnResult = new clsResult("Serializing tile textures", false);
            logger.Info("Serializing tile textures");

            var X = 0;
            var Y = 0;
            var ErrorCount = 0;
            var Value = 0;

            try
            {
                for ( Y = 0; Y <= Terrain.TileSize.Y - 1; Y++ )
                {
                    for ( X = 0; X <= Terrain.TileSize.X - 1; X++ )
                    {
                        Value = Convert.ToInt32(Terrain.Tiles[X, Y].Texture.TextureNum + 1);
                        if ( Value < 0 | Value > 255 )
                        {
                            ErrorCount++;
                            Value = 0;
                        }
                        File.Write((byte)Value);
                    }
                }
            }
            catch ( Exception ex )
            {
                ReturnResult.ProblemAdd(ex.Message);
            }

            if ( ErrorCount > 0 )
            {
                ReturnResult.WarningAdd(ErrorCount + " tiles had an invalid texture number.");
            }

            return ReturnResult;
        }

        public clsResult Serialize_FMap_TileOrientation(BinaryWriter File)
        {
            var ReturnResult = new clsResult("Serializing tile orientations", false);
            logger.Info("Serializing tile orientations");
            var X = 0;
            var Y = 0;
            var Value = 0;

            try
            {
                for ( Y = 0; Y <= Terrain.TileSize.Y - 1; Y++ )
                {
                    for ( X = 0; X <= Terrain.TileSize.X - 1; X++ )
                    {
                        Value = 0;
                        if ( Terrain.Tiles[X, Y].Texture.Orientation.SwitchedAxes )
                        {
                            Value += 8;
                        }
                        if ( Terrain.Tiles[X, Y].Texture.Orientation.ResultXFlip )
                        {
                            Value += 4;
                        }
                        if ( Terrain.Tiles[X, Y].Texture.Orientation.ResultYFlip )
                        {
                            Value += 2;
                        }
                        if ( Terrain.Tiles[X, Y].Tri )
                        {
                            Value++;
                        }
                        File.Write((byte)Value);
                    }
                }
            }
            catch ( Exception ex )
            {
                ReturnResult.ProblemAdd(ex.Message);
            }

            return ReturnResult;
        }

        public clsResult Serialize_FMap_TileCliff(BinaryWriter File)
        {
            var ReturnResult = new clsResult("Serializing tile cliffs", false);
            logger.Info("Serializing tile cliffs");

            var X = 0;
            var Y = 0;
            var Value = 0;
            var DownSideValue = 0;
            var ErrorCount = 0;

            try
            {
                for ( Y = 0; Y <= Terrain.TileSize.Y - 1; Y++ )
                {
                    for ( X = 0; X <= Terrain.TileSize.X - 1; X++ )
                    {
                        Value = 0;
                        if ( Terrain.Tiles[X, Y].Tri )
                        {
                            if ( Terrain.Tiles[X, Y].TriTopLeftIsCliff )
                            {
                                Value += 2;
                            }
                            if ( Terrain.Tiles[X, Y].TriBottomRightIsCliff )
                            {
                                Value++;
                            }
                        }
                        else
                        {
                            if ( Terrain.Tiles[X, Y].TriBottomLeftIsCliff )
                            {
                                Value += 2;
                            }
                            if ( Terrain.Tiles[X, Y].TriTopRightIsCliff )
                            {
                                Value++;
                            }
                        }
                        if ( Terrain.Tiles[X, Y].Terrain_IsCliff )
                        {
                            Value += 4;
                        }
                        if ( TileUtil.IdenticalTileDirections(Terrain.Tiles[X, Y].DownSide, TileUtil.None) )
                        {
                            DownSideValue = 0;
                        }
                        else if ( TileUtil.IdenticalTileDirections(Terrain.Tiles[X, Y].DownSide, TileUtil.Top) )
                        {
                            DownSideValue = 1;
                        }
                        else if ( TileUtil.IdenticalTileDirections(Terrain.Tiles[X, Y].DownSide, TileUtil.Left) )
                        {
                            DownSideValue = 2;
                        }
                        else if ( TileUtil.IdenticalTileDirections(Terrain.Tiles[X, Y].DownSide, TileUtil.Right) )
                        {
                            DownSideValue = 3;
                        }
                        else if ( TileUtil.IdenticalTileDirections(Terrain.Tiles[X, Y].DownSide, TileUtil.Bottom) )
                        {
                            DownSideValue = 4;
                        }
                        else
                        {
                            ErrorCount++;
                            DownSideValue = 0;
                        }
                        Value += DownSideValue * 8;
                        File.Write((byte)Value);
                    }
                }
            }
            catch ( Exception ex )
            {
                ReturnResult.ProblemAdd(ex.Message);
            }

            if ( ErrorCount > 0 )
            {
                ReturnResult.WarningAdd(ErrorCount + " tiles had an invalid cliff down side.");
            }

            return ReturnResult;
        }

        public clsResult Serialize_FMap_Roads(BinaryWriter File)
        {
            var ReturnResult = new clsResult("Serializing roads", false);
            logger.Info("Serializing roads");

            var X = 0;
            var Y = 0;
            var Value = 0;
            var ErrorCount = 0;

            try
            {
                for ( Y = 0; Y <= Terrain.TileSize.Y; Y++ )
                {
                    for ( X = 0; X <= Terrain.TileSize.X - 1; X++ )
                    {
                        if ( Terrain.SideH[X, Y].Road == null )
                        {
                            Value = 0;
                        }
                        else if ( Terrain.SideH[X, Y].Road.Num < 0 )
                        {
                            ErrorCount++;
                            Value = 0;
                        }
                        else
                        {
                            Value = Convert.ToInt32(Terrain.SideH[X, Y].Road.Num + 1);
                            if ( Value > 255 )
                            {
                                ErrorCount++;
                                Value = 0;
                            }
                        }
                        File.Write((byte)Value);
                    }
                }
                for ( Y = 0; Y <= Terrain.TileSize.Y - 1; Y++ )
                {
                    for ( X = 0; X <= Terrain.TileSize.X; X++ )
                    {
                        if ( Terrain.SideV[X, Y].Road == null )
                        {
                            Value = 0;
                        }
                        else if ( Terrain.SideV[X, Y].Road.Num < 0 )
                        {
                            ErrorCount++;
                            Value = 0;
                        }
                        else
                        {
                            Value = Convert.ToInt32(Terrain.SideV[X, Y].Road.Num + 1);
                            if ( Value > 255 )
                            {
                                ErrorCount++;
                                Value = 0;
                            }
                        }
                        File.Write((byte)Value);
                    }
                }
            }
            catch ( Exception ex )
            {
                ReturnResult.ProblemAdd(ex.Message);
            }

            if ( ErrorCount > 0 )
            {
                ReturnResult.WarningAdd(ErrorCount + " sides had an invalid road number.");
            }

            return ReturnResult;
        }

        public clsResult Serialize_FMap_Objects(IniWriter File)
        {
            var ReturnResult = new clsResult("Serializing objects", false);
            logger.Info("Serializing objects");

            var A = 0;
            var Unit = default(clsUnit);
            var Droid = default(DroidDesign);
            var WarningCount = 0;
            string Text = null;

            try
            {
                for ( A = 0; A <= Units.Count - 1; A++ )
                {
                    Unit = Units[A];
                    File.AddSection(A.ToStringInvariant());
                    switch ( Unit.TypeBase.Type )
                    {
                        case UnitType.Feature:
                            File.AddProperty("Type", "Feature, " + ((FeatureTypeBase)Unit.TypeBase).Code);
                            break;
                        case UnitType.PlayerStructure:
                            var structureTypeBase = (StructureTypeBase)Unit.TypeBase;
                            File.AddProperty("Type", "Structure, " + structureTypeBase.Code);
                            if ( structureTypeBase.WallLink.IsConnected )
                            {
                                File.AddProperty("WallType", structureTypeBase.WallLink.ArrayPosition.ToStringInvariant());
                            }
                            break;
                        case UnitType.PlayerDroid:
                            Droid = (DroidDesign)Unit.TypeBase;
                            if ( Droid.IsTemplate )
                            {
                                File.AddProperty("Type", "DroidTemplate, " + ((DroidTemplate)Unit.TypeBase).Code);
                            }
                            else
                            {
                                File.AddProperty("Type", "DroidDesign");
                                if ( Droid.TemplateDroidType != null )
                                {
                                    File.AddProperty("DroidType", Droid.TemplateDroidType.TemplateCode);
                                }
                                if ( Droid.Body != null )
                                {
                                    File.AddProperty("Body", Droid.Body.Code);
                                }
                                if ( Droid.Propulsion != null )
                                {
                                    File.AddProperty("Propulsion", Droid.Propulsion.Code);
                                }
                                File.AddProperty("TurretCount", Droid.TurretCount.ToStringInvariant());
                                if ( Droid.Turret1 != null )
                                {
                                    if ( Droid.Turret1.GetTurretTypeName(ref Text) )
                                    {
                                        File.AddProperty("Turret1", Text + ", " + Droid.Turret1.Code);
                                    }
                                }
                                if ( Droid.Turret2 != null )
                                {
                                    if ( Droid.Turret2.GetTurretTypeName(ref Text) )
                                    {
                                        File.AddProperty("Turret2", Text + ", " + Droid.Turret2.Code);
                                    }
                                }
                                if ( Droid.Turret3 != null )
                                {
                                    if ( Droid.Turret3.GetTurretTypeName(ref Text) )
                                    {
                                        File.AddProperty("Turret3", Text + ", " + Droid.Turret3.Code);
                                    }
                                }
                            }
                            break;
                        default:
                            WarningCount++;
                            break;
                    }
                    File.AddProperty("ID", Unit.ID.ToStringInvariant());
                    File.AddProperty("Priority", Unit.SavePriority.ToStringInvariant());
                    File.AddProperty("Pos", Unit.Pos.Horizontal.X.ToStringInvariant() + ", " + Unit.Pos.Horizontal.Y.ToStringInvariant());
                    File.AddProperty("Heading", Unit.Rotation.ToStringInvariant());
                    File.AddProperty("UnitGroup", Unit.UnitGroup.GetFMapINIPlayerText());
                    if ( Unit.Health < 1.0D )
                    {
                        File.AddProperty("Health", Unit.Health.ToStringInvariant());
                    }
                    if ( Unit.Label != null )
                    {
                        File.AddProperty("ScriptLabel", Unit.Label);
                    }
                }
            }
            catch ( Exception ex )
            {
                ReturnResult.ProblemAdd(ex.Message);
            }

            if ( WarningCount > 0 )
            {
                ReturnResult.WarningAdd("Error: " + Convert.ToString(WarningCount) + " units were of an unhandled type.");
            }

            return ReturnResult;
        }

        public clsResult Serialize_FMap_Gateways(IniWriter File)
        {
            var ReturnResult = new clsResult("Serializing gateways", false);
            logger.Info("Serializing gateways");
            var A = 0;
            var Gateway = default(clsGateway);

            try
            {
                for ( A = 0; A <= Gateways.Count - 1; A++ )
                {
                    Gateway = Gateways[A];
                    File.AddSection(A.ToStringInvariant());
                    File.AddProperty("AX", Gateway.PosA.X.ToStringInvariant());
                    File.AddProperty("AY", Gateway.PosA.Y.ToStringInvariant());
                    File.AddProperty("BX", Gateway.PosB.X.ToStringInvariant());
                    File.AddProperty("BY", Gateway.PosB.Y.ToStringInvariant());
                }
            }
            catch ( Exception ex )
            {
                ReturnResult.ProblemAdd(ex.Message);
            }

            return ReturnResult;
        }

        public clsResult Serialize_FMap_TileTypes(BinaryWriter File)
        {
            var ReturnResult = new clsResult("Serializing tile types", false);
            logger.Info("Serializing tile types");
            var A = 0;

            try
            {
                if ( Tileset != null )
                {
                    for ( A = 0; A <= Tileset.TileCount - 1; A++ )
                    {
                        File.Write(Tile_TypeNum[A]);
                    }
                }
            }
            catch ( Exception ex )
            {
                ReturnResult.ProblemAdd(ex.Message);
            }

            return ReturnResult;
        }

        public clsResult Load_FMap(string path)
        {
            var returnResult = new clsResult(string.Format("Loading FMap from \"{0}\"", path), false);
            logger.Info(string.Format("Loading FMap from \"{0}\"", path));

            using ( var zip = ZipFile.Read(path) )
            {
                /*
                 * Info.ini loading
                 */
                var infoIniEntry = zip["info.ini"]; // Case insensetive.
                if ( infoIniEntry == null )
                {
                    returnResult.ProblemAdd("Unable to find file \"info.ini\".");
                    return returnResult;
                }

                FMapInfo resultInfo = null;
                using ( Stream s = infoIniEntry.OpenReader() )
                {
                    var reader = new StreamReader(s);
                    returnResult.Add(Read_FMap_Info(reader, ref resultInfo));
                    reader.Close();
                    if ( returnResult.HasProblems )
                    {
                        return returnResult;
                    }
                }

                var newTerrainSize = resultInfo.TerrainSize;
                Tileset = resultInfo.Tileset;

                if ( newTerrainSize.X <= 0 | newTerrainSize.X > Constants.MapMaxSize )
                {
                    returnResult.ProblemAdd(string.Format("Map width of {0} is not valid.", newTerrainSize.X));
                }
                if ( newTerrainSize.Y <= 0 | newTerrainSize.Y > Constants.MapMaxSize )
                {
                    returnResult.ProblemAdd(string.Format("Map height of {0} is not valid.", newTerrainSize.Y));
                }
                if ( returnResult.HasProblems )
                {
                    return returnResult;
                }

                SetPainterToDefaults(); //depends on tileset. must be called before loading the terrains.
                TerrainBlank(newTerrainSize);
                TileType_Reset();

                // vertexheight.dat
                var vhEntry = zip["vertexheight.dat"]; // Case insensetive.
                if ( vhEntry == null )
                {
                    returnResult.ProblemAdd("Unable to find file \"vertexheight.dat\".");
                }
                else
                {
                    using ( Stream s = vhEntry.OpenReader() )
                    {
                        var reader = new BinaryReader(s);
                        returnResult.Add(Read_FMap_VertexHeight(reader));
                        reader.Close();
                    }
                }

                // vertexterrain.dat
                var vtEntry = zip["vertexterrain.dat"]; // Case insensetive.
                if ( vtEntry == null )
                {
                    returnResult.ProblemAdd("Unable to find file \"vertexterrain.dat\".");
                }
                else
                {
                    using ( Stream s = vtEntry.OpenReader() )
                    {
                        var reader = new BinaryReader(s);
                        returnResult.Add(Read_FMap_VertexTerrain(reader));
                        reader.Close();
                    }
                }

                // tiletexture.dat
                var ttEntry = zip["tiletexture.dat"]; // Case insensetive.
                if ( vtEntry == null )
                {
                    returnResult.ProblemAdd("Unable to find file \"tiletexture.dat\".");
                }
                else
                {
                    using ( Stream s = ttEntry.OpenReader() )
                    {
                        var reader = new BinaryReader(s);
                        returnResult.Add(Read_FMap_TileTexture(reader));
                        reader.Close();
                    }
                }

                // tileorientation.dat
                var toEntry = zip["tileorientation.dat"]; // Case insensetive.
                if ( toEntry == null )
                {
                    returnResult.ProblemAdd("Unable to find file \"tileorientation.dat\".");
                }
                else
                {
                    using ( Stream s = toEntry.OpenReader() )
                    {
                        var reader = new BinaryReader(s);
                        returnResult.Add(Read_FMap_TileOrientation(reader));
                        reader.Close();
                    }
                }

                // tilecliff.dat
                var tcEntry = zip["tilecliff.dat"]; // Case insensetive.
                if ( tcEntry == null )
                {
                    returnResult.ProblemAdd("Unable to find file \"tilecliff.dat\".");
                }
                else
                {
                    using ( Stream s = tcEntry.OpenReader() )
                    {
                        var reader = new BinaryReader(s);
                        returnResult.Add(Read_FMap_TileCliff(reader));
                        reader.Close();
                    }
                }

                // roads.dat
                var roEntry = zip["roads.dat"]; // Case insensetive.
                if ( roEntry == null )
                {
                    returnResult.ProblemAdd("Unable to find file \"roads.dat\".");
                }
                else
                {
                    using ( Stream s = roEntry.OpenReader() )
                    {
                        var reader = new BinaryReader(s);
                        returnResult.Add(Read_FMap_Roads(reader));
                        reader.Close();
                    }
                }

                // objects.ini
                var obEntry = zip["objects.ini"]; // Case insensetive.
                if ( obEntry == null )
                {
                    returnResult.ProblemAdd("Unable to find file \"objects.ini\".");
                }
                else
                {
                    using ( Stream s = obEntry.OpenReader() )
                    {
                        var reader = new StreamReader(s);
                        returnResult.Add(Read_FMap_Objects(reader));
                        reader.Close();
                    }
                }

                // gateways.ini
                var gaEntry = zip["gateways.ini"]; // Case insensetive.
                if ( gaEntry == null )
                {
                    returnResult.ProblemAdd("Unable to find file \"gateways.ini\".");
                    return returnResult;
                }
                using ( Stream s = gaEntry.OpenReader() )
                {
                    var reader = new StreamReader(s);
                    returnResult.Add(Read_FMap_Gateways(reader));
                    reader.Close();
                }

                // tiletypes.dat
                var tileTypesEntry = zip["tiletypes.dat"]; // Case insensetive.
                if ( tileTypesEntry == null )
                {
                    returnResult.ProblemAdd("Unable to find file \"tiletypes.dat\".");
                }
                else
                {
                    using ( Stream s = tileTypesEntry.OpenReader() )
                    {
                        var reader = new BinaryReader(s);
                        returnResult.Add(Read_FMap_TileTypes(reader));
                        reader.Close();
                    }
                }

                // scriptlabels.ini
                var scriptLabelsEntry = zip["scriptlabels.ini"]; // Case insensetive.
                if ( scriptLabelsEntry == null )
                {
                    returnResult.ProblemAdd("Unable to find file \"scriptlabels.ini\".");
                    return returnResult;
                }
                if ( scriptLabelsEntry != null )
                {
                    using ( var reader = new StreamReader(scriptLabelsEntry.OpenReader()) )
                    {
                        var text = reader.ReadToEnd();
                        returnResult.Add(Read_INI_Labels(text));
                    }
                }

                InterfaceOptions = resultInfo.InterfaceOptions;
            }

            return returnResult;
        }

        private clsResult Read_FMap_Info(StreamReader File, ref FMapInfo ResultInfo)
        {
            var ReturnResult = new clsResult("Read general map info", false);
            logger.Info("Read general map info");

            var InfoINI = new Section();
            ReturnResult.Take(InfoINI.ReadFile(File));

            ResultInfo = new FMapInfo();
            ReturnResult.Take(InfoINI.Translate(ResultInfo));

            if ( ResultInfo.TerrainSize.X < 0 | ResultInfo.TerrainSize.Y < 0 )
            {
                ReturnResult.ProblemAdd("Map size was not specified or was invalid.");
            }

            return ReturnResult;
        }

        private clsResult Read_FMap_VertexHeight(BinaryReader File)
        {
            var ReturnResult = new clsResult("Reading vertex heights", false);
            logger.Info("Reading vertex heights");

            var X = 0;
            var Y = 0;

            try
            {
                for ( Y = 0; Y <= Terrain.TileSize.Y; Y++ )
                {
                    for ( X = 0; X <= Terrain.TileSize.X; X++ )
                    {
                        Terrain.Vertices[X, Y].Height = File.ReadByte();
                    }
                }
            }
            catch ( Exception ex )
            {
                ReturnResult.ProblemAdd(ex.Message);
                return ReturnResult;
            }

            if ( File.PeekChar() >= 0 )
            {
                ReturnResult.WarningAdd("There were unread bytes at the end of the file.");
            }

            return ReturnResult;
        }

        private clsResult Read_FMap_VertexTerrain(BinaryReader File)
        {
            var ReturnResult = new clsResult("Reading vertex terrain", false);
            logger.Info("Reading vertex terrain");

            var X = 0;
            var Y = 0;
            var Value = 0;
            byte byteTemp = 0;
            var WarningCount = 0;

            try
            {
                for ( Y = 0; Y <= Terrain.TileSize.Y; Y++ )
                {
                    for ( X = 0; X <= Terrain.TileSize.X; X++ )
                    {
                        byteTemp = File.ReadByte();
                        Value = (byteTemp) - 1;
                        if ( Value < 0 )
                        {
                            Terrain.Vertices[X, Y].Terrain = null;
                        }
                        else if ( Value >= Painter.TerrainCount )
                        {
                            if ( WarningCount < 16 )
                            {
                                ReturnResult.WarningAdd("Painted terrain at vertex " + Convert.ToString(X) + ", " + Convert.ToString(Y) +
                                                        " was invalid.");
                            }
                            WarningCount++;
                            Terrain.Vertices[X, Y].Terrain = null;
                        }
                        else
                        {
                            Terrain.Vertices[X, Y].Terrain = Painter.Terrains[Value];
                        }
                    }
                }
            }
            catch ( Exception ex )
            {
                ReturnResult.ProblemAdd(ex.Message);
                return ReturnResult;
            }

            if ( WarningCount > 0 )
            {
                ReturnResult.WarningAdd(WarningCount + " painted terrain vertices were invalid.");
            }

            if ( File.PeekChar() >= 0 )
            {
                ReturnResult.WarningAdd("There were unread bytes at the end of the file.");
            }

            return ReturnResult;
        }

        public clsResult Read_FMap_TileTexture(BinaryReader File)
        {
            var ReturnResult = new clsResult("Reading tile textures", false);
            logger.Info("Reading tile textures");

            var X = 0;
            var Y = 0;
            byte byteTemp = 0;

            try
            {
                for ( Y = 0; Y <= Terrain.TileSize.Y - 1; Y++ )
                {
                    for ( X = 0; X <= Terrain.TileSize.X - 1; X++ )
                    {
                        byteTemp = File.ReadByte();
                        Terrain.Tiles[X, Y].Texture.TextureNum = (byteTemp) - 1;
                    }
                }
            }
            catch ( Exception ex )
            {
                ReturnResult.ProblemAdd(ex.Message);
                return ReturnResult;
            }

            if ( File.PeekChar() >= 0 )
            {
                ReturnResult.WarningAdd("There were unread bytes at the end of the file.");
            }

            return ReturnResult;
        }

        public clsResult Read_FMap_TileOrientation(BinaryReader File)
        {
            var ReturnResult = new clsResult("Reading tile orientations", false);
            logger.Info("Reading tile orientations");

            var X = 0;
            var Y = 0;
            var Value = 0;
            var PartValue = 0;
            var WarningCount = 0;

            try
            {
                for ( Y = 0; Y <= Terrain.TileSize.Y - 1; Y++ )
                {
                    for ( X = 0; X <= Terrain.TileSize.X - 1; X++ )
                    {
                        Value = File.ReadByte();

                        PartValue = (int)(Math.Floor(((double)Value / 16)));
                        if ( PartValue > 0 )
                        {
                            if ( WarningCount < 16 )
                            {
                                ReturnResult.WarningAdd("Unknown bits used for tile " + Convert.ToString(X) + ", " + Convert.ToString(Y) + ".");
                            }
                            WarningCount++;
                        }
                        Value -= PartValue * 16;

                        PartValue = (int)(Value / 8.0D);
                        Terrain.Tiles[X, Y].Texture.Orientation.SwitchedAxes = PartValue > 0;
                        Value -= PartValue * 8;

                        PartValue = (int)(Value / 4.0D);
                        Terrain.Tiles[X, Y].Texture.Orientation.ResultXFlip = PartValue > 0;
                        Value -= PartValue * 4;

                        PartValue = (int)(Value / 2.0D);
                        Terrain.Tiles[X, Y].Texture.Orientation.ResultYFlip = PartValue > 0;
                        Value -= PartValue * 2;

                        PartValue = Value;
                        Terrain.Tiles[X, Y].Tri = PartValue > 0;
                    }
                }
            }
            catch ( Exception ex )
            {
                ReturnResult.ProblemAdd(ex.Message);
                return ReturnResult;
            }

            if ( WarningCount > 0 )
            {
                ReturnResult.WarningAdd(WarningCount + " tiles had unknown bits used.");
            }

            if ( File.PeekChar() >= 0 )
            {
                ReturnResult.WarningAdd("There were unread bytes at the end of the file.");
            }

            return ReturnResult;
        }

        public clsResult Read_FMap_TileCliff(BinaryReader File)
        {
            var ReturnResult = new clsResult("Reading tile cliffs", false);
            logger.Info("Reading tile cliffs");

            var X = 0;
            var Y = 0;
            var Value = 0;
            var PartValue = 0;
            var DownSideWarningCount = 0;
            var WarningCount = 0;

            try
            {
                for ( Y = 0; Y <= Terrain.TileSize.Y - 1; Y++ )
                {
                    for ( X = 0; X <= Terrain.TileSize.X - 1; X++ )
                    {
                        Value = File.ReadByte();

                        PartValue = (int)(Value / 64.0D);
                        if ( PartValue > 0 )
                        {
                            if ( WarningCount < 16 )
                            {
                                ReturnResult.WarningAdd("Unknown bits used for tile " + Convert.ToString(X) + ", " + Convert.ToString(Y) + ".");
                            }
                            WarningCount++;
                        }
                        Value -= PartValue * 64;

                        PartValue = (int)(Value / 8.0D);
                        switch ( PartValue )
                        {
                            case 0:
                                Terrain.Tiles[X, Y].DownSide = TileUtil.None;
                                break;
                            case 1:
                                Terrain.Tiles[X, Y].DownSide = TileUtil.Top;
                                break;
                            case 2:
                                Terrain.Tiles[X, Y].DownSide = TileUtil.Left;
                                break;
                            case 3:
                                Terrain.Tiles[X, Y].DownSide = TileUtil.Right;
                                break;
                            case 4:
                                Terrain.Tiles[X, Y].DownSide = TileUtil.Bottom;
                                break;
                            default:
                                Terrain.Tiles[X, Y].DownSide = TileUtil.None;
                                if ( DownSideWarningCount < 16 )
                                {
                                    ReturnResult.WarningAdd("Down side value for tile " + Convert.ToString(X) + ", " + Convert.ToString(Y) +
                                                            " was invalid.");
                                }
                                DownSideWarningCount++;
                                break;
                        }
                        Value -= PartValue * 8;

                        PartValue = (int)(Value / 4.0D);
                        Terrain.Tiles[X, Y].Terrain_IsCliff = PartValue > 0;
                        Value -= PartValue * 4;

                        PartValue = (int)(Value / 2.0D);
                        if ( Terrain.Tiles[X, Y].Tri )
                        {
                            Terrain.Tiles[X, Y].TriTopLeftIsCliff = PartValue > 0;
                        }
                        else
                        {
                            Terrain.Tiles[X, Y].TriBottomLeftIsCliff = PartValue > 0;
                        }
                        Value -= PartValue * 2;

                        PartValue = Value;
                        if ( Terrain.Tiles[X, Y].Tri )
                        {
                            Terrain.Tiles[X, Y].TriBottomRightIsCliff = PartValue > 0;
                        }
                        else
                        {
                            Terrain.Tiles[X, Y].TriTopRightIsCliff = PartValue > 0;
                        }
                    }
                }
            }
            catch ( Exception ex )
            {
                ReturnResult.ProblemAdd(ex.Message);
                return ReturnResult;
            }

            if ( WarningCount > 0 )
            {
                ReturnResult.WarningAdd(WarningCount + " tiles had unknown bits used.");
            }
            if ( DownSideWarningCount > 0 )
            {
                ReturnResult.WarningAdd(DownSideWarningCount + " tiles had invalid down side values.");
            }

            if ( File.PeekChar() >= 0 )
            {
                ReturnResult.WarningAdd("There were unread bytes at the end of the file.");
            }

            return ReturnResult;
        }

        public clsResult Read_FMap_Roads(BinaryReader File)
        {
            var ReturnResult = new clsResult("Reading roads", false);
            logger.Info("Reading roads");

            var X = 0;
            var Y = 0;
            var Value = 0;
            var WarningCount = 0;

            try
            {
                for ( Y = 0; Y <= Terrain.TileSize.Y; Y++ )
                {
                    for ( X = 0; X <= Terrain.TileSize.X - 1; X++ )
                    {
                        Value = File.ReadByte() - 1;
                        if ( Value < 0 )
                        {
                            Terrain.SideH[X, Y].Road = null;
                        }
                        else if ( Value >= Painter.RoadCount )
                        {
                            if ( WarningCount < 16 )
                            {
                                ReturnResult.WarningAdd("Invalid road value for horizontal side " + Convert.ToString(X) + ", " + Convert.ToString(Y) +
                                                        ".");
                            }
                            WarningCount++;
                            Terrain.SideH[X, Y].Road = null;
                        }
                        else
                        {
                            Terrain.SideH[X, Y].Road = Painter.Roads[Value];
                        }
                    }
                }
                for ( Y = 0; Y <= Terrain.TileSize.Y - 1; Y++ )
                {
                    for ( X = 0; X <= Terrain.TileSize.X; X++ )
                    {
                        Value = File.ReadByte() - 1;
                        if ( Value < 0 )
                        {
                            Terrain.SideV[X, Y].Road = null;
                        }
                        else if ( Value >= Painter.RoadCount )
                        {
                            if ( WarningCount < 16 )
                            {
                                ReturnResult.WarningAdd("Invalid road value for vertical side " + Convert.ToString(X) + ", " + Convert.ToString(Y) +
                                                        ".");
                            }
                            WarningCount++;
                            Terrain.SideV[X, Y].Road = null;
                        }
                        else
                        {
                            Terrain.SideV[X, Y].Road = Painter.Roads[Value];
                        }
                    }
                }
            }
            catch ( Exception ex )
            {
                ReturnResult.ProblemAdd(ex.Message);
                return ReturnResult;
            }

            if ( WarningCount > 0 )
            {
                ReturnResult.WarningAdd(WarningCount + " sides had an invalid road value.");
            }

            if ( File.PeekChar() >= 0 )
            {
                ReturnResult.WarningAdd("There were unread bytes at the end of the file.");
            }

            return ReturnResult;
        }

        private clsResult Read_FMap_Objects(StreamReader File)
        {
            var ReturnResult = new clsResult("Reading objects", false);
            logger.Info("Reading objects");

            var A = 0;

            var ObjectsINI = new IniReader();
            ReturnResult.Take(ObjectsINI.ReadFile(File));

            var INIObjects = new FMapIniObjects(ObjectsINI.Sections.Count);
            ReturnResult.Take(ObjectsINI.Translate(INIObjects));

            var DroidComponentUnknownCount = 0;
            var ObjectTypeMissingCount = 0;
            var ObjectPlayerNumInvalidCount = 0;
            var ObjectPosInvalidCount = 0;
            var DesignTypeUnspecifiedCount = 0;
            var UnknownUnitTypeCount = 0;
            var MaxUnknownUnitTypeWarningCount = 16;

            var DroidDesign = default(DroidDesign);
            var NewObject = default(clsUnit);
            var UnitAdd = new clsUnitAdd();
            var unitTypeBase = default(UnitTypeBase);
            var IsDesign = default(bool);
            var UnitGroup = default(clsUnitGroup);
            var ZeroPos = new XYInt(0, 0);
            UInt32 AvailableID = 0;

            UnitAdd.Map = this;

            AvailableID = 1U;
            for ( A = 0; A <= INIObjects.ObjectCount - 1; A++ )
            {
                if ( INIObjects.Objects[A].ID >= AvailableID )
                {
                    AvailableID = INIObjects.Objects[A].ID + 1U;
                }
            }
            for ( A = 0; A <= INIObjects.ObjectCount - 1; A++ )
            {
                if ( INIObjects.Objects[A].Pos == null )
                {
                    ObjectPosInvalidCount++;
                }
                else if ( !App.PosIsWithinTileArea(INIObjects.Objects[A].Pos, ZeroPos, Terrain.TileSize) )
                {
                    ObjectPosInvalidCount++;
                }
                else
                {
                    unitTypeBase = null;
                    if ( INIObjects.Objects[A].Type != UnitType.Unspecified )
                    {
                        IsDesign = false;
                        if ( INIObjects.Objects[A].Type == UnitType.PlayerDroid )
                        {
                            if ( !INIObjects.Objects[A].IsTemplate )
                            {
                                IsDesign = true;
                            }
                        }
                        if ( IsDesign )
                        {
                            DroidDesign = new DroidDesign();
                            DroidDesign.TemplateDroidType = INIObjects.Objects[A].TemplateDroidType;
                            if ( DroidDesign.TemplateDroidType == null )
                            {
                                DroidDesign.TemplateDroidType = App.TemplateDroidType_Droid;
                                DesignTypeUnspecifiedCount++;
                            }
                            if ( INIObjects.Objects[A].BodyCode != "" )
                            {
                                DroidDesign.Body = App.ObjectData.FindOrCreateBody(Convert.ToString(INIObjects.Objects[A].BodyCode));
                                if ( DroidDesign.Body.IsUnknown )
                                {
                                    DroidComponentUnknownCount++;
                                }
                            }
                            if ( INIObjects.Objects[A].PropulsionCode != "" )
                            {
                                DroidDesign.Propulsion = App.ObjectData.FindOrCreatePropulsion(INIObjects.Objects[A].PropulsionCode);
                                if ( DroidDesign.Propulsion.IsUnknown )
                                {
                                    DroidComponentUnknownCount++;
                                }
                            }
                            DroidDesign.TurretCount = (byte)(INIObjects.Objects[A].TurretCount);
                            if ( INIObjects.Objects[A].TurretCodes[0] != "" )
                            {
                                DroidDesign.Turret1 = App.ObjectData.FindOrCreateTurret(INIObjects.Objects[A].TurretTypes[0],
                                    Convert.ToString(INIObjects.Objects[A].TurretCodes[0]));
                                if ( DroidDesign.Turret1.IsUnknown )
                                {
                                    DroidComponentUnknownCount++;
                                }
                            }
                            if ( INIObjects.Objects[A].TurretCodes[1] != "" )
                            {
                                DroidDesign.Turret2 = App.ObjectData.FindOrCreateTurret(INIObjects.Objects[A].TurretTypes[1],
                                    Convert.ToString(INIObjects.Objects[A].TurretCodes[1]));
                                if ( DroidDesign.Turret2.IsUnknown )
                                {
                                    DroidComponentUnknownCount++;
                                }
                            }
                            if ( INIObjects.Objects[A].TurretCodes[2] != "" )
                            {
                                DroidDesign.Turret3 = App.ObjectData.FindOrCreateTurret(INIObjects.Objects[A].TurretTypes[2],
                                    Convert.ToString(INIObjects.Objects[A].TurretCodes[2]));
                                if ( DroidDesign.Turret3.IsUnknown )
                                {
                                    DroidComponentUnknownCount++;
                                }
                            }
                            DroidDesign.UpdateAttachments();
                            unitTypeBase = DroidDesign;
                        }
                        else
                        {
                            unitTypeBase = App.ObjectData.FindOrCreateUnitType(INIObjects.Objects[A].Code, INIObjects.Objects[A].Type, INIObjects.Objects[A].WallType);
                            if ( unitTypeBase.IsUnknown )
                            {
                                if ( UnknownUnitTypeCount < MaxUnknownUnitTypeWarningCount )
                                {
                                    ReturnResult.WarningAdd("\"{0}\" is ont a loaded object.".Format2(INIObjects.Objects[A].Code));
                                }
                                UnknownUnitTypeCount++;
                            }
                        }
                    }
                    if ( unitTypeBase == null )
                    {
                        ObjectTypeMissingCount++;
                    }
                    else
                    {
                        NewObject = new clsUnit();
                        NewObject.TypeBase = unitTypeBase;
                        NewObject.Pos.Horizontal.X = INIObjects.Objects[A].Pos.X;
                        NewObject.Pos.Horizontal.Y = INIObjects.Objects[A].Pos.Y;
                        NewObject.Health = INIObjects.Objects[A].Health;
                        NewObject.SavePriority = INIObjects.Objects[A].Priority;
                        NewObject.Rotation = (int)(INIObjects.Objects[A].Heading);
                        if ( NewObject.Rotation >= 360 )
                        {
                            NewObject.Rotation -= 360;
                        }
                        if ( INIObjects.Objects[A].UnitGroup == null || INIObjects.Objects[A].UnitGroup == "" )
                        {
                            if ( INIObjects.Objects[A].Type != UnitType.Feature )
                            {
                                ObjectPlayerNumInvalidCount++;
                            }
                            NewObject.UnitGroup = ScavengerUnitGroup;
                        }
                        else
                        {
                            if ( INIObjects.Objects[A].UnitGroup.ToLower() == "scavenger" )
                            {
                                NewObject.UnitGroup = ScavengerUnitGroup;
                            }
                            else
                            {
                                UInt32 PlayerNum = 0;
                                try
                                {
                                    if ( !IOUtil.InvariantParse(INIObjects.Objects[A].UnitGroup, ref PlayerNum) )
                                    {
                                        throw (new Exception());
                                    }
                                    if ( PlayerNum < Constants.PlayerCountMax )
                                    {
                                        UnitGroup = UnitGroups[Convert.ToInt32(PlayerNum)];
                                    }
                                    else
                                    {
                                        UnitGroup = ScavengerUnitGroup;
                                        ObjectPlayerNumInvalidCount++;
                                    }
                                }
                                catch ( Exception )
                                {
                                    ObjectPlayerNumInvalidCount++;
                                    UnitGroup = ScavengerUnitGroup;
                                }
                                NewObject.UnitGroup = UnitGroup;
                            }
                        }
                        if ( INIObjects.Objects[A].ID == 0U )
                        {
                            INIObjects.Objects[A].ID = AvailableID;
                            App.ZeroIDWarning(NewObject, INIObjects.Objects[A].ID, ReturnResult);
                        }
                        UnitAdd.NewUnit = NewObject;
                        UnitAdd.ID = INIObjects.Objects[A].ID;
                        UnitAdd.Label = INIObjects.Objects[A].Label;
                        UnitAdd.Perform();
                        App.ErrorIDChange(INIObjects.Objects[A].ID, NewObject, "Read_FMap_Objects");
                        if ( AvailableID == INIObjects.Objects[A].ID )
                        {
                            AvailableID = NewObject.ID + 1U;
                        }
                    }
                }
            }

            if ( UnknownUnitTypeCount > MaxUnknownUnitTypeWarningCount )
            {
                ReturnResult.WarningAdd(UnknownUnitTypeCount + " objects were not in the loaded object data.");
            }
            if ( ObjectTypeMissingCount > 0 )
            {
                ReturnResult.WarningAdd(ObjectTypeMissingCount + " objects did not specify a type and were ignored.");
            }
            if ( DroidComponentUnknownCount > 0 )
            {
                ReturnResult.WarningAdd(DroidComponentUnknownCount + " components used by droids were loaded as unknowns.");
            }
            if ( ObjectPlayerNumInvalidCount > 0 )
            {
                ReturnResult.WarningAdd(ObjectPlayerNumInvalidCount + " objects had an invalid player number and were set to player 0.");
            }
            if ( ObjectPosInvalidCount > 0 )
            {
                ReturnResult.WarningAdd(ObjectPosInvalidCount + " objects had a position that was off-map and were ignored.");
            }
            if ( DesignTypeUnspecifiedCount > 0 )
            {
                ReturnResult.WarningAdd(DesignTypeUnspecifiedCount + " designed droids did not specify a template droid type and were set to droid.");
            }

            return ReturnResult;
        }

        public clsResult Read_FMap_Gateways(StreamReader File)
        {
            var ReturnResult = new clsResult("Reading gateways", false);
            logger.Info("Reading gateways");

            var GatewaysINI = new IniReader();
            ReturnResult.Take(GatewaysINI.ReadFile(File));

            var INIGateways = new FMapIniGateways(GatewaysINI.Sections.Count);
            ReturnResult.Take(GatewaysINI.Translate(INIGateways));

            var A = 0;
            var InvalidGatewayCount = 0;

            for ( A = 0; A <= INIGateways.GatewayCount - 1; A++ )
            {
                if ( GatewayCreate(INIGateways.Gateways[A].PosA, INIGateways.Gateways[A].PosB) == null )
                {
                    InvalidGatewayCount++;
                }
            }

            if ( InvalidGatewayCount > 0 )
            {
                ReturnResult.WarningAdd(InvalidGatewayCount + " gateways were invalid.");
            }

            return ReturnResult;
        }

        public clsResult Read_FMap_TileTypes(BinaryReader File)
        {
            var ReturnResult = new clsResult("Reading tile types", false);
            logger.Info("Reading tile types");

            var A = 0;
            byte byteTemp = 0;
            var InvalidTypeCount = 0;

            try
            {
                if ( Tileset != null )
                {
                    for ( A = 0; A <= Tileset.TileCount - 1; A++ )
                    {
                        byteTemp = File.ReadByte();
                        if ( byteTemp >= App.TileTypes.Count )
                        {
                            InvalidTypeCount++;
                        }
                        else
                        {
                            Tile_TypeNum[A] = byteTemp;
                        }
                    }
                }
            }
            catch ( Exception ex )
            {
                ReturnResult.ProblemAdd(ex.Message);
                return ReturnResult;
            }

            if ( InvalidTypeCount > 0 )
            {
                ReturnResult.WarningAdd(InvalidTypeCount + " tile types were invalid.");
            }

            if ( File.PeekChar() >= 0 )
            {
                ReturnResult.WarningAdd("There were unread bytes at the end of the file.");
            }

            return ReturnResult;
        }

        public clsResult Read_INI_Labels(string iniText)
        {
            var resultObject = new clsResult("Reading labels", false);
            logger.Info("Reading labels.");

            var typeNum = 0;
            var NewPosition = default(clsScriptPosition);
            var NewArea = default(clsScriptArea);
            var nameText = "";
            var strLabel = "";
            var strPosA = "";
            var strPosB = "";
            var idText = "";
            UInt32 idNum = 0;
            XYInt xyIntA = null;
            XYInt xyIntB = null;

            var failedCount = 0;
            var modifiedCount = 0;

            try
            {
                var iniSections = Core.Parsers.Ini.IniReader.ReadString(iniText);
                foreach ( var iniSection in iniSections )
                {
                    var idx = iniSection.Name.IndexOf('_');
                    if ( idx > 0 )
                    {
                        nameText = iniSection.Name.Substring(0, idx);
                    }
                    else
                    {
                        nameText = iniSection.Name;
                    }
                    switch ( nameText )
                    {
                        case "position":
                            typeNum = 0;
                            break;
                        case "area":
                            typeNum = 1;
                            break;
                        case "object":
                            typeNum = int.MaxValue;
                            failedCount++;
                            continue;
                        default:
                            typeNum = int.MaxValue;
                            failedCount++;
                            continue;
                    }

                    // Raised an exception if nothing was found
                    try
                    {
                        strLabel = iniSection.Data.Where(d => d.Name == "label").First().Data;
                    }
                    catch ( Exception ex )
                    {
                        resultObject.WarningAdd(string.Format("Failed to parse \"label\", error was: {0}", ex.Message));
                        logger.WarnException("Failed to parse \"label\", error was", ex);
                        failedCount++;
                        continue;
                    }
                    strLabel = strLabel.Replace("\"", "");

                    switch ( typeNum )
                    {
                        case 0: //position
                            strPosA = iniSection.Data.Where(d => d.Name == "pos").First().Data;
                            if ( strPosA == null )
                            {
                                failedCount++;
                                continue;
                            }
                            try
                            {
                                xyIntA = XYInt.FromString(strPosA);
                                NewPosition = new clsScriptPosition(this);
                                NewPosition.PosX = xyIntA.X;
                                NewPosition.PosY = xyIntA.Y;
                                NewPosition.SetLabel(strLabel);
                                if ( NewPosition.Label != strLabel ||
                                     NewPosition.PosX != xyIntA.X || NewPosition.PosY != xyIntA.Y )
                                {
                                    modifiedCount++;
                                }
                            }
                            catch ( Exception ex )
                            {
                                resultObject.WarningAdd(string.Format("Failed to parse \"pos\", error was: {0}", ex.Message));
                                logger.WarnException("Failed to parse \"pos\", error was", ex);
                                failedCount++;
                            }
                            break;
                        case 1: //area
                            try
                            {
                                strPosA = iniSection.Data.Where(d => d.Name == "pos1").First().Data;
                                strPosB = iniSection.Data.Where(d => d.Name == "pos2").First().Data;

                                xyIntA = XYInt.FromString(strPosA);
                                xyIntB = XYInt.FromString(strPosA);
                                NewArea = new clsScriptArea(this);
                                NewArea.SetPositions(xyIntA, xyIntB);
                                NewArea.SetLabel(strLabel);
                                if ( NewArea.Label != strLabel || NewArea.PosAX != xyIntA.X | NewArea.PosAY != xyIntA.Y
                                     | NewArea.PosBX != xyIntB.X | NewArea.PosBY != xyIntB.Y )
                                {
                                    modifiedCount++;
                                }
                            }
                            catch ( Exception ex )
                            {
                                Debugger.Break();
                                resultObject.WarningAdd(string.Format("Failed to parse \"pos1\" or \"pos2\", error was: {0}", ex.Message));
                                logger.WarnException("Failed to parse \"pos1\" or \"pos2\".", ex);
                                failedCount++;
                            }
                            break;
                        case 2: //object
                            idText = iniSection.Data.Where(d => d.Name == "id").First().Data;
                            if ( IOUtil.InvariantParse(idText, ref idNum) )
                            {
                                var Unit = IDUsage(idNum);
                                if ( Unit != null )
                                {
                                    if ( !Unit.SetLabel(strLabel).Success )
                                    {
                                        failedCount++;
                                    }
                                }
                                else
                                {
                                    failedCount++;
                                }
                            }
                            break;
                        default:
                            resultObject.WarningAdd("Error! Bad type number for script label.");
                            break;
                    }
                }
            }
            catch ( Exception ex )
            {
                Debugger.Break();
                logger.ErrorException("Got exception while reading labels.ini", ex);
                resultObject.ProblemAdd(string.Format("Got exception: {0}", ex.Message), false);
                return resultObject;
            }

            if ( failedCount > 0 )
            {
                resultObject.WarningAdd(string.Format("Unable to translate {0} script labels.", failedCount));
            }
            if ( modifiedCount > 0 )
            {
                resultObject.WarningAdd(string.Format("{0} script labels had invalid values and were modified.", modifiedCount));
            }

            return resultObject;
        }
    }
}