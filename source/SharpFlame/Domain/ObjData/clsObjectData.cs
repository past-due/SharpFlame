#region

using System;
using System.Drawing;
using System.IO;
using SharpFlame.Bitmaps;
using SharpFlame.Collections;
using SharpFlame.Core.Domain;
using SharpFlame.Core.Extensions;
using SharpFlame.FileIO;
using SharpFlame.Util;

#endregion

namespace SharpFlame.Domain.ObjData
{
    public class clsObjectData
    {
        public ConnectedList<Body, clsObjectData> Bodies;
        public ConnectedList<Brain, clsObjectData> Brains;
        public ConnectedList<Construct, clsObjectData> Constructors;
        public ConnectedList<DroidTemplate, clsObjectData> DroidTemplates;
        public ConnectedList<Ecm, clsObjectData> ECMs;
        public ConnectedList<FeatureTypeBase, clsObjectData> FeatureTypes;
        public ConnectedList<Propulsion, clsObjectData> Propulsions;
        public ConnectedList<Repair, clsObjectData> Repairs;
        public ConnectedList<Sensor, clsObjectData> Sensors;
        public ConnectedList<StructureTypeBase, clsObjectData> StructureTypes;

        public SimpleList<clsTexturePage> TexturePages = new SimpleList<clsTexturePage>();
        public ConnectedList<Turret, clsObjectData> Turrets;
        public ConnectedList<UnitTypeBase, clsObjectData> UnitTypes;
        public ConnectedList<clsWallType, clsObjectData> WallTypes;
        public ConnectedList<Weapon, clsObjectData> Weapons;

        public clsObjectData()
        {
            UnitTypes = new ConnectedList<UnitTypeBase, clsObjectData>(this);
            FeatureTypes = new ConnectedList<FeatureTypeBase, clsObjectData>(this);
            StructureTypes = new ConnectedList<StructureTypeBase, clsObjectData>(this);
            DroidTemplates = new ConnectedList<DroidTemplate, clsObjectData>(this);
            WallTypes = new ConnectedList<clsWallType, clsObjectData>(this);
            Bodies = new ConnectedList<Body, clsObjectData>(this);
            Propulsions = new ConnectedList<Propulsion, clsObjectData>(this);
            Turrets = new ConnectedList<Turret, clsObjectData>(this);
            Weapons = new ConnectedList<Weapon, clsObjectData>(this);
            Sensors = new ConnectedList<Sensor, clsObjectData>(this);
            Repairs = new ConnectedList<Repair, clsObjectData>(this);
            Constructors = new ConnectedList<Construct, clsObjectData>(this);
            Brains = new ConnectedList<Brain, clsObjectData>(this);
            ECMs = new ConnectedList<Ecm, clsObjectData>(this);
        }

        public clsResult LoadDirectory(string path)
        {
            var returnResult = new clsResult(string.Format("Loading object data from \"{0}\"", path));

            path = PathUtil.EndWithPathSeperator(path);

            var subDirNames = "";
            var subDirStructures = "";
            var subDirBrain = "";
            var subDirBody = "";
            var subDirPropulsion = "";
            var subDirBodyPropulsion = "";
            var subDirConstruction = "";
            var subDirSensor = "";
            var subDirRepair = "";
            var subDirTemplates = "";
            var subDirWeapons = "";
            var subDirEcm = "";
            var subDirFeatures = "";
            var subDirTexpages = "";
            var subDirAssignWeapons = "";
            var subDirStructureWeapons = "";
            var subDirPiEs = "";

            subDirNames = "messages".CombinePathWith("strings").CombinePathWith("names.txt");
            subDirStructures = "stats".CombinePathWith("structures.txt");
            subDirBrain = "stats".CombinePathWith("brain.txt");
            subDirBody = "stats".CombinePathWith("body.txt");
            subDirPropulsion = "stats".CombinePathWith("propulsion.txt");
            subDirBodyPropulsion = "stats".CombinePathWith("bodypropulsionimd.txt");
            subDirConstruction = "stats".CombinePathWith("construction.txt");
            subDirSensor = "stats".CombinePathWith("sensor.txt");
            subDirRepair = "stats".CombinePathWith("repair.txt");
            subDirTemplates = "stats".CombinePathWith("templates.txt");
            subDirWeapons = "stats".CombinePathWith("weapons.txt");
            subDirEcm = "stats".CombinePathWith("ecm.txt");
            subDirFeatures = "stats".CombinePathWith("features.txt");
            subDirPiEs = "pies".CombinePathWith("", endWithPathSeparator: true);
            subDirTexpages = "texpages".CombinePathWith("", endWithPathSeparator: true);
            subDirAssignWeapons = "stats".CombinePathWith("assignweapons.txt");
            subDirStructureWeapons = "stats".CombinePathWith("structureweapons.txt");

            var commaFiles = new SimpleList<clsTextFile>();

            var dataNames = new clsTextFile
                {
                    SubDirectory = subDirNames,
                    UniqueField = 0
                };

            returnResult.Add(dataNames.LoadNamesFile(path));
            if ( !dataNames.CalcUniqueField() )
            {
                returnResult.ProblemAdd("There are two entries for the same code in " + subDirNames + ".");
            }

            var dataStructures = new clsTextFile{SubDirectory = subDirStructures, FieldCount = 25};
            commaFiles.Add(dataStructures);

            var dataBrain = new clsTextFile{SubDirectory = subDirBrain, FieldCount = 9};
            commaFiles.Add(dataBrain);

            var dataBody = new clsTextFile{SubDirectory = subDirBody, FieldCount = 25};
            commaFiles.Add(dataBody);

            var dataPropulsion = new clsTextFile{SubDirectory = subDirPropulsion, FieldCount = 12};
            commaFiles.Add(dataPropulsion);

            var dataBodyPropulsion = new clsTextFile{SubDirectory = subDirBodyPropulsion, FieldCount = 5, UniqueField = -1};
            commaFiles.Add(dataBodyPropulsion);

            var dataConstruction = new clsTextFile{SubDirectory = subDirConstruction, FieldCount = 12};
            commaFiles.Add(dataConstruction);

            var dataSensor = new clsTextFile{SubDirectory = subDirSensor, FieldCount = 16};
            commaFiles.Add(dataSensor);

            var dataRepair = new clsTextFile {SubDirectory = subDirRepair, FieldCount = 14};
            commaFiles.Add(dataRepair);

            var dataTemplates = new clsTextFile {SubDirectory = subDirTemplates, FieldCount = 12};
            commaFiles.Add(dataTemplates);

            var dataEcm = new clsTextFile {SubDirectory = subDirEcm, FieldCount = 14};
            commaFiles.Add(dataEcm);

            var dataFeatures = new clsTextFile {SubDirectory = subDirFeatures, FieldCount = 11};
            commaFiles.Add(dataFeatures);

            var dataAssignWeapons = new clsTextFile {SubDirectory = subDirAssignWeapons, FieldCount = 5};
            commaFiles.Add(dataAssignWeapons);

            var dataWeapons = new clsTextFile {SubDirectory = subDirWeapons, FieldCount = 53};
            commaFiles.Add(dataWeapons);

            var dataStructureWeapons = new clsTextFile {SubDirectory = subDirStructureWeapons, FieldCount = 6};
            commaFiles.Add(dataStructureWeapons);

            foreach ( var textFile in commaFiles )
            {
                var result = textFile.LoadCommaFile(path);
                returnResult.Add(result);
                if ( !result.HasProblems )
                {
                    if ( textFile.CalcIsFieldCountValid() )
                    {
                        if ( !textFile.CalcUniqueField() )
                        {
                            returnResult.ProblemAdd("An entry in field " + Convert.ToString(textFile.UniqueField) + " was not unique for file " +
                                                    textFile.SubDirectory + ".");
                        }
                    }
                    else
                    {
                        returnResult.ProblemAdd("There were entries with the wrong number of fields for file " + textFile.SubDirectory + ".");
                    }
                }
            }

            if ( returnResult.HasProblems )
            {
                return returnResult;
            }

            //load texpages

            string[] texFiles = null;

            try
            {
                texFiles = Directory.GetFiles(path + subDirTexpages);
            }
            catch ( Exception )
            {
                returnResult.WarningAdd("Unable to access texture pages.");
                texFiles = new string[0];
            }

            var text = "";
            Bitmap bitmap = null;

            foreach ( var texFile in texFiles )
            {
                text = texFile;
                if ( text.Substring(text.Length - 4, 4).ToLower() == ".png" )
                {
                    var result = new clsResult(string.Format("Loading texture page \"{0}\"", text));
                    if ( File.Exists(text) )
                    {
                        sResult bitmapResult = BitmapUtil.LoadBitmap(text, ref bitmap);
                        var newPage = new clsTexturePage();
                        if ( bitmapResult.Success )
                        {
                            result.Take(BitmapUtil.BitmapIsGlCompatible(bitmap));
                            newPage.GLTexture_Num = BitmapUtil.CreateGLTexture (bitmap, 0);
                        }
                        else
                        {
                            result.WarningAdd(bitmapResult.Problem);
                        }
                        var instrPos2 = text.LastIndexOf(Path.DirectorySeparatorChar);
                        newPage.FileTitle = text.Substring(instrPos2 + 1, text.Length - 5 - instrPos2);

                        TexturePages.Add(newPage);
                    }
                    else
                    {
                        result.WarningAdd("Texture page missing (" + text + ").");
                    }
                    returnResult.Add(result);
                }
            }

            //load PIEs

            string[] pieFiles = null;
            var pieList = new SimpleList<clsPIE>();

            try
            {
                pieFiles = Directory.GetFiles(path + subDirPiEs);
            }
            catch ( Exception )
            {
                returnResult.WarningAdd("Unable to access PIE files.");
                pieFiles = new string[0];
            }

            foreach ( var tempLoopVar_Text in pieFiles )
            {
                text = tempLoopVar_Text;
                var splitPath = new sSplitPath(text);
                if ( splitPath.FileExtension.ToLower() == "pie" )
                {
                    var newPie = new clsPIE {Path = text, LCaseFileTitle = splitPath.FileTitle.ToLower()};
                    pieList.Add(newPie);
                }
            }

            //interpret stats

            clsAttachment attachment;
            clsAttachment baseAttachment;
            Body body;
            Propulsion propulsion;
            Weapon weapon;
            Sensor sensor;
            Ecm ecm;
            string[] fields = null;

            //interpret body

            foreach ( var tempLoopVar_Fields in dataBody.ResultData )
            {
                fields = tempLoopVar_Fields;
                body = new Body();
                body.ObjectDataLink.Connect(Bodies);
                body.Code = fields[0];
                SetComponentName(dataNames.ResultData, body, returnResult);
                IOUtil.InvariantParse(fields[6], ref body.Hitpoints);
                body.Designable = fields[24] != "0";
                body.Attachment.Models.Add(GetModelForPIE(pieList, fields[7].ToLower(), returnResult));
            }

            //interpret propulsion

            foreach ( var tempLoopVar_Fields in dataPropulsion.ResultData )
            {
                fields = tempLoopVar_Fields;
                propulsion = new Propulsion(Bodies.Count);
                propulsion.ObjectDataLink.Connect(Propulsions);
                propulsion.Code = fields[0];
                SetComponentName(dataNames.ResultData, propulsion, returnResult);
                IOUtil.InvariantParse(fields[7], ref propulsion.HitPoints);
                //.Propulsions(Propulsion_Num).PIE = LCase(DataPropulsion.Entries(Propulsion_Num).FieldValues(8))
                propulsion.Designable = fields[11] != "0";
            }

            //interpret body-propulsions

            var bodyPropulsionPIEs = new BodyProp[Bodies.Count, Propulsions.Count];
            for ( var A = 0; A <= Bodies.Count - 1; A++ )
            {
                for ( var B = 0; B <= Propulsions.Count - 1; B++ )
                {
                    bodyPropulsionPIEs[A, B] = new BodyProp();
                    bodyPropulsionPIEs[A, B].LeftPIE = "0";
                    bodyPropulsionPIEs[A, B].RightPIE = "0";
                }
            }

            foreach ( var tempLoopVar_Fields in dataBodyPropulsion.ResultData )
            {
                fields = tempLoopVar_Fields;
                body = FindBodyCode(fields[0]);
                propulsion = FindPropulsionCode(fields[1]);
                if ( body != null && propulsion != null )
                {
                    if ( fields[2] != "0" )
                    {
                        bodyPropulsionPIEs[body.ObjectDataLink.ArrayPosition, propulsion.ObjectDataLink.ArrayPosition].LeftPIE = fields[2].ToLower();
                    }
                    if ( fields[3] != "0" )
                    {
                        bodyPropulsionPIEs[body.ObjectDataLink.ArrayPosition, propulsion.ObjectDataLink.ArrayPosition].RightPIE = fields[3].ToLower();
                    }
                }
            }

            //set propulsion-body PIEs

            for ( var a = 0; a <= Propulsions.Count - 1; a++ )
            {
                propulsion = Propulsions[a];
                for ( var B = 0; B <= Bodies.Count - 1; B++ )
                {
                    body = Bodies[B];
                    propulsion.Bodies[B].LeftAttachment = new clsAttachment();
                    propulsion.Bodies[B].LeftAttachment.Models.Add(GetModelForPIE(pieList, bodyPropulsionPIEs[B, a].LeftPIE, returnResult));
                    propulsion.Bodies[B].RightAttachment = new clsAttachment();
                    propulsion.Bodies[B].RightAttachment.Models.Add(GetModelForPIE(pieList, bodyPropulsionPIEs[B, a].RightPIE, returnResult));
                }
            }

            //interpret construction

            foreach ( var tempLoopVar_Fields in dataConstruction.ResultData )
            {
                fields = tempLoopVar_Fields;
                Construct Construct = new Construct();
                Construct.ObjectDataLink.Connect(Constructors);
                Construct.TurretObjectDataLink.Connect(Turrets);
                Construct.Code = fields[0];
                SetComponentName(dataNames.ResultData, Construct, returnResult);
                Construct.Designable = fields[11] != "0";
                Construct.Attachment.Models.Add(GetModelForPIE(pieList, fields[8].ToLower(), returnResult));
            }

            //interpret weapons

            foreach ( var tempLoopVar_Fields in dataWeapons.ResultData )
            {
                fields = tempLoopVar_Fields;
                weapon = new Weapon();
                weapon.ObjectDataLink.Connect(Weapons);
                weapon.TurretObjectDataLink.Connect(Turrets);
                weapon.Code = fields[0];
                SetComponentName(dataNames.ResultData, weapon, returnResult);
                IOUtil.InvariantParse(fields[7], ref weapon.HitPoints);
                weapon.Designable = fields[51] != "0";
                weapon.Attachment.Models.Add(GetModelForPIE(pieList, Convert.ToString(fields[8].ToLower()), returnResult));
                weapon.Attachment.Models.Add(GetModelForPIE(pieList, fields[9].ToLower(), returnResult));
            }

            //interpret sensor

            foreach ( var tempLoopVar_Fields in dataSensor.ResultData )
            {
                fields = tempLoopVar_Fields;
                sensor = new Sensor();
                sensor.ObjectDataLink.Connect(Sensors);
                sensor.TurretObjectDataLink.Connect(Turrets);
                sensor.Code = fields[0];
                SetComponentName(dataNames.ResultData, sensor, returnResult);
                IOUtil.InvariantParse(fields[7], ref sensor.HitPoints);
                sensor.Designable = fields[15] != "0";
                switch ( fields[11].ToLower() )
                {
                    case "turret":
                        sensor.Location = SensorLocationType.Turret;
                        break;
                    case "default":
                        sensor.Location = SensorLocationType.Invisible;
                        break;
                    default:
                        sensor.Location = SensorLocationType.Invisible;
                        break;
                }
                sensor.Attachment.Models.Add(GetModelForPIE(pieList, fields[8].ToLower(), returnResult));
                sensor.Attachment.Models.Add(GetModelForPIE(pieList, fields[9].ToLower(), returnResult));
            }

            //interpret repair

            foreach ( var tempLoopVar_Fields in dataRepair.ResultData )
            {
                fields = tempLoopVar_Fields;
                Repair Repair = new Repair();
                Repair.ObjectDataLink.Connect(Repairs);
                Repair.TurretObjectDataLink.Connect(Turrets);
                Repair.Code = fields[0];
                SetComponentName(dataNames.ResultData, Repair, returnResult);
                Repair.Designable = fields[13] != "0";
                Repair.Attachment.Models.Add(GetModelForPIE(pieList, fields[9].ToLower(), returnResult));
                Repair.Attachment.Models.Add(GetModelForPIE(pieList, fields[10].ToLower(), returnResult));
            }

            //interpret brain

            foreach ( var tempLoopVar_Fields in dataBrain.ResultData )
            {
                fields = tempLoopVar_Fields;
                Brain Brain = new Brain();
                Brain.ObjectDataLink.Connect(Brains);
                Brain.TurretObjectDataLink.Connect(Turrets);
                Brain.Code = fields[0];
                SetComponentName(dataNames.ResultData, Brain, returnResult);
                Brain.Designable = true;
                weapon = FindWeaponCode(fields[7]);
                if ( weapon != null )
                {
                    Brain.Weapon = weapon;
                    Brain.Attachment = weapon.Attachment;
                }
            }

            //interpret ecm

            foreach ( var tempLoopVar_Fields in dataEcm.ResultData )
            {
                fields = tempLoopVar_Fields;
                ecm = new Ecm();
                ecm.ObjectDataLink.Connect(ECMs);
                ecm.TurretObjectDataLink.Connect(Turrets);
                ecm.Code = fields[0];
                SetComponentName(dataNames.ResultData, ecm, returnResult);
                IOUtil.InvariantParse(fields[7], ref ecm.HitPoints);
                ecm.Designable = false;
                ecm.Attachment.Models.Add(GetModelForPIE(pieList, fields[8].ToLower(), returnResult));
            }

            //interpret feature

            foreach ( var tempLoopVar_Fields in dataFeatures.ResultData )
            {
                fields = tempLoopVar_Fields;
                FeatureTypeBase featureTypeBase = new FeatureTypeBase();
                featureTypeBase.UnitType_ObjectDataLink.Connect(UnitTypes);
                featureTypeBase.FeatureType_ObjectDataLink.Connect(FeatureTypes);
                featureTypeBase.Code = fields[0];
                if ( fields[7] == "OIL RESOURCE" ) //type
                {
                    featureTypeBase.FeatureType = FeatureType.OilResource;
                }
                SetFeatureName(dataNames.ResultData, featureTypeBase, returnResult);
                if ( !IOUtil.InvariantParse(fields[1], ref featureTypeBase.Footprint.X) )
                {
                    returnResult.WarningAdd("Feature footprint-x was not an integer for " + featureTypeBase.Code + ".");
                }
                if ( !IOUtil.InvariantParse(fields[2], ref featureTypeBase.Footprint.Y) )
                {
                    returnResult.WarningAdd("Feature footprint-y was not an integer for " + featureTypeBase.Code + ".");
                }
                featureTypeBase.BaseAttachment = new clsAttachment();
                baseAttachment = featureTypeBase.BaseAttachment;
                text = fields[6].ToLower();
                attachment = baseAttachment.CreateAttachment();
                attachment.Models.Add(GetModelForPIE(pieList, text, returnResult));
            }

            //interpret structure

            foreach ( var tempLoopVar_Fields in dataStructures.ResultData )
            {
                fields = tempLoopVar_Fields;
                var StructureCode = fields[0];
                var StructureTypeText = fields[1];
                var StructurePIEs = fields[21].ToLower().Split('@');
                var StructureFootprint = new XYInt();
                var StructureBasePIE = fields[22].ToLower();
                if ( !IOUtil.InvariantParse(fields[5], ref StructureFootprint.X) )
                {
                    returnResult.WarningAdd("Structure footprint-x was not an integer for " + StructureCode + ".");
                }
                if ( !IOUtil.InvariantParse(fields[6], ref StructureFootprint.Y) )
                {
                    returnResult.WarningAdd("Structure footprint-y was not an integer for " + StructureCode + ".");
                }
                if ( StructureTypeText != "WALL" || StructurePIEs.GetLength(0) != 4 )
                {
                    //this is NOT a generic wall
                    StructureTypeBase structureTypeBase = new StructureTypeBase();
                    structureTypeBase.UnitType_ObjectDataLink.Connect(UnitTypes);
                    structureTypeBase.StructureType_ObjectDataLink.Connect(StructureTypes);
                    structureTypeBase.Code = StructureCode;
                    SetStructureName(dataNames.ResultData, structureTypeBase, returnResult);
                    structureTypeBase.Footprint = StructureFootprint;
                    switch ( StructureTypeText )
                    {
                        case "DEMOLISH":
                            structureTypeBase.StructureType = StructureType.Demolish;
                            break;
                        case "WALL":
                            structureTypeBase.StructureType = StructureType.Wall;
                            break;
                        case "CORNER WALL":
                            structureTypeBase.StructureType = StructureType.CornerWall;
                            break;
                        case "FACTORY":
                            structureTypeBase.StructureType = StructureType.Factory;
                            break;
                        case "CYBORG FACTORY":
                            structureTypeBase.StructureType = StructureType.CyborgFactory;
                            break;
                        case "VTOL FACTORY":
                            structureTypeBase.StructureType = StructureType.VTOLFactory;
                            break;
                        case "COMMAND":
                            structureTypeBase.StructureType = StructureType.Command;
                            break;
                        case "HQ":
                            structureTypeBase.StructureType = StructureType.HQ;
                            break;
                        case "DEFENSE":
                            structureTypeBase.StructureType = StructureType.Defense;
                            break;
                        case "POWER GENERATOR":
                            structureTypeBase.StructureType = StructureType.PowerGenerator;
                            break;
                        case "POWER MODULE":
                            structureTypeBase.StructureType = StructureType.PowerModule;
                            break;
                        case "RESEARCH":
                            structureTypeBase.StructureType = StructureType.Research;
                            break;
                        case "RESEARCH MODULE":
                            structureTypeBase.StructureType = StructureType.ResearchModule;
                            break;
                        case "FACTORY MODULE":
                            structureTypeBase.StructureType = StructureType.FactoryModule;
                            break;
                        case "DOOR":
                            structureTypeBase.StructureType = StructureType.DOOR;
                            break;
                        case "REPAIR FACILITY":
                            structureTypeBase.StructureType = StructureType.RepairFacility;
                            break;
                        case "SAT UPLINK":
                            structureTypeBase.StructureType = StructureType.DOOR;
                            break;
                        case "REARM PAD":
                            structureTypeBase.StructureType = StructureType.RearmPad;
                            break;
                        case "MISSILE SILO":
                            structureTypeBase.StructureType = StructureType.MissileSilo;
                            break;
                        case "RESOURCE EXTRACTOR":
                            structureTypeBase.StructureType = StructureType.ResourceExtractor;
                            break;
                        default:
                            structureTypeBase.StructureType = StructureType.Unknown;
                            break;
                    }

                    baseAttachment = structureTypeBase.BaseAttachment;
                    if ( StructurePIEs.GetLength(0) > 0 )
                    {
                        baseAttachment.Models.Add(GetModelForPIE(pieList, StructurePIEs[0], returnResult));
                    }
                    structureTypeBase.StructureBasePlate = GetModelForPIE(pieList, StructureBasePIE, returnResult);
                    if ( baseAttachment.Models.Count == 1 )
                    {
                        if ( baseAttachment.Models[0].ConnectorCount >= 1 )
                        {
                            XYZDouble connector = baseAttachment.Models[0].Connectors[0];
                            var StructureWeapons = default(SimpleList<string[]>);
                            StructureWeapons = GetRowsWithValue(dataStructureWeapons.ResultData, structureTypeBase.Code);
                            if ( StructureWeapons.Count > 0 )
                            {
                                weapon = FindWeaponCode(Convert.ToString(StructureWeapons[0][1]));
                            }
                            else
                            {
                                weapon = null;
                            }
                            ecm = FindECMCode(fields[18]);
                            sensor = FindSensorCode(fields[19]);
                            if ( weapon != null )
                            {
                                if ( weapon.Code != "ZNULLWEAPON" )
                                {
                                    attachment = baseAttachment.CopyAttachment(weapon.Attachment);
                                    attachment.PosOffset = connector;
                                }
                            }
                            if ( ecm != null )
                            {
                                if ( ecm.Code != "ZNULLECM" )
                                {
                                    attachment = baseAttachment.CopyAttachment(ecm.Attachment);
                                    attachment.PosOffset = connector;
                                }
                            }
                            if ( sensor != null )
                            {
                                if ( sensor.Code != "ZNULLSENSOR" )
                                {
                                    attachment = baseAttachment.CopyAttachment(sensor.Attachment);
                                    attachment.PosOffset = connector;
                                }
                            }
                        }
                    }
                }
                else
                {
                    //this is a generic wall
                    var NewWall = new clsWallType();
                    NewWall.WallType_ObjectDataLink.Connect(WallTypes);
                    NewWall.Code = StructureCode;
                    SetWallName(dataNames.ResultData, NewWall, returnResult);
                    var WallBasePlate = GetModelForPIE(pieList, StructureBasePIE, returnResult);

                    var WallNum = 0;
                    var wallStructureTypeBase = default(StructureTypeBase);
                    for ( WallNum = 0; WallNum <= 3; WallNum++ )
                    {
                        wallStructureTypeBase = new StructureTypeBase();
                        wallStructureTypeBase.UnitType_ObjectDataLink.Connect(UnitTypes);
                        wallStructureTypeBase.StructureType_ObjectDataLink.Connect(StructureTypes);
                        wallStructureTypeBase.WallLink.Connect(NewWall.Segments);
                        wallStructureTypeBase.Code = StructureCode;
                        text = NewWall.Name;
                        switch ( WallNum )
                        {
                            case 0:
                                text += " - ";
                                break;
                            case 1:
                                text += " + ";
                                break;
                            case 2:
                                text += " T ";
                                break;
                            case 3:
                                text += " L ";
                                break;
                        }
                        wallStructureTypeBase.Name = text;
                        wallStructureTypeBase.Footprint = StructureFootprint;
                        wallStructureTypeBase.StructureType = StructureType.Wall;

                        baseAttachment = wallStructureTypeBase.BaseAttachment;

                        text = StructurePIEs[WallNum];
                        baseAttachment.Models.Add(GetModelForPIE(pieList, text, returnResult));
                        wallStructureTypeBase.StructureBasePlate = WallBasePlate;
                    }
                }
            }

            //interpret templates

            var TurretConflictCount = 0;
            foreach ( var tempLoopVar_Fields in dataTemplates.ResultData )
            {
                fields = tempLoopVar_Fields;
                DroidTemplate template = new DroidTemplate();
                template.UnitType_ObjectDataLink.Connect(UnitTypes);
                template.DroidTemplate_ObjectDataLink.Connect(DroidTemplates);
                template.Code = fields[0];
                SetTemplateName(dataNames.ResultData, template, returnResult);
                switch ( fields[9] ) //type
                {
                    case "ZNULLDROID":
                        template.TemplateDroidType = App.TemplateDroidType_Null;
                        break;
                    case "DROID":
                        template.TemplateDroidType = App.TemplateDroidType_Droid;
                        break;
                    case "CYBORG":
                        template.TemplateDroidType = App.TemplateDroidType_Cyborg;
                        break;
                    case "CYBORG_CONSTRUCT":
                        template.TemplateDroidType = App.TemplateDroidType_CyborgConstruct;
                        break;
                    case "CYBORG_REPAIR":
                        template.TemplateDroidType = App.TemplateDroidType_CyborgRepair;
                        break;
                    case "CYBORG_SUPER":
                        template.TemplateDroidType = App.TemplateDroidType_CyborgSuper;
                        break;
                    case "TRANSPORTER":
                        template.TemplateDroidType = App.TemplateDroidType_Transporter;
                        break;
                    case "PERSON":
                        template.TemplateDroidType = App.TemplateDroidType_Person;
                        break;
                    default:
                        template.TemplateDroidType = null;
                        returnResult.WarningAdd("Template " + template.GetDisplayTextCode() + " had an unrecognised type.");
                        break;
                }
                var loadPartsArgs = new DroidDesign.sLoadPartsArgs();
                loadPartsArgs.Body = FindBodyCode(fields[2]);
                loadPartsArgs.Brain = FindBrainCode(fields[3]);
                loadPartsArgs.Construct = FindConstructorCode(fields[4]);
                loadPartsArgs.ECM = FindECMCode(fields[5]);
                loadPartsArgs.Propulsion = FindPropulsionCode(fields[7]);
                loadPartsArgs.Repair = FindRepairCode(fields[8]);
                loadPartsArgs.Sensor = FindSensorCode(fields[10]);
                var TemplateWeapons = GetRowsWithValue(dataAssignWeapons.ResultData, template.Code);
                if ( TemplateWeapons.Count > 0 )
                {
                    text = Convert.ToString(TemplateWeapons[0][1]);
                    if ( text != "NULL" )
                    {
                        loadPartsArgs.Weapon1 = FindWeaponCode(text);
                    }
                    text = Convert.ToString(TemplateWeapons[0][2]);
                    if ( text != "NULL" )
                    {
                        loadPartsArgs.Weapon2 = FindWeaponCode(text);
                    }
                    text = Convert.ToString(TemplateWeapons[0][3]);
                    if ( text != "NULL" )
                    {
                        loadPartsArgs.Weapon3 = FindWeaponCode(text);
                    }
                }
                if ( !template.LoadParts(loadPartsArgs) )
                {
                    if ( TurretConflictCount < 16 )
                    {
                        returnResult.WarningAdd("Template " + template.GetDisplayTextCode() + " had multiple conflicting turrets.");
                    }
                    TurretConflictCount++;
                }
            }
            if ( TurretConflictCount > 0 )
            {
                returnResult.WarningAdd(TurretConflictCount + " templates had multiple conflicting turrets.");
            }

            return returnResult;
        }

        public SimpleList<string[]> GetRowsWithValue(SimpleList<string[]> TextLines, string Value)
        {
            var Result = new SimpleList<string[]>();

            string[] Line = null;
            foreach ( var tempLoopVar_Line in TextLines )
            {
                Line = tempLoopVar_Line;
                if ( Line[0] == Value )
                {
                    Result.Add(Line);
                }
            }

            return Result;
        }

        public clsModel GetModelForPIE(SimpleList<clsPIE> PIE_List, string PIE_LCaseFileTitle, clsResult ResultOutput)
        {
            if ( PIE_LCaseFileTitle == "0" )
            {
                return null;
            }

            var A = 0;
            var PIEFile = default(StreamReader);
            var PIE = default(clsPIE);

            var Result = new clsResult("Loading PIE file " + PIE_LCaseFileTitle);

            for ( A = 0; A <= PIE_List.Count - 1; A++ )
            {
                PIE = PIE_List[A];
                if ( PIE.LCaseFileTitle == PIE_LCaseFileTitle )
                {
                    if ( PIE.Model == null )
                    {
                        PIE.Model = new clsModel();
                        try
                        {
                            PIEFile = new StreamReader(PIE.Path);
                            try
                            {
                                Result.Take(PIE.Model.ReadPIE(PIEFile, this));
                            }
                            catch ( Exception ex )
                            {
                                PIEFile.Close();
                                Result.WarningAdd(ex.Message);
                                ResultOutput.Add(Result);
                                return PIE.Model;
                            }
                        }
                        catch ( Exception ex )
                        {
                            Result.WarningAdd(ex.Message);
                        }
                    }
                    ResultOutput.Add(Result);
                    return PIE.Model;
                }
            }

            if ( !Result.HasWarnings )
            {
                Result.WarningAdd("file is missing");
            }
            ResultOutput.Add(Result);

            return null;
        }

        public void SetComponentName(SimpleList<string[]> Names, ComponentBase componentBase, clsResult Result)
        {
            var ValueSearchResults = default(SimpleList<string[]>);

            ValueSearchResults = GetRowsWithValue(Names, componentBase.Code);
            if ( ValueSearchResults.Count == 0 )
            {
                Result.WarningAdd("No name for component " + componentBase.Code + ".");
            }
            else
            {
                componentBase.Name = Convert.ToString(ValueSearchResults[0][1]);
            }
        }

        public void SetFeatureName(SimpleList<string[]> Names, FeatureTypeBase featureTypeBase, clsResult Result)
        {
            var ValueSearchResults = default(SimpleList<string[]>);

            ValueSearchResults = GetRowsWithValue(Names, featureTypeBase.Code);
            if ( ValueSearchResults.Count == 0 )
            {
                Result.WarningAdd("No name for feature type " + featureTypeBase.Code + ".");
            }
            else
            {
                featureTypeBase.Name = Convert.ToString(ValueSearchResults[0][1]);
            }
        }

        public void SetStructureName(SimpleList<string[]> Names, StructureTypeBase structureTypeBase, clsResult Result)
        {
            var ValueSearchResults = default(SimpleList<string[]>);

            ValueSearchResults = GetRowsWithValue(Names, structureTypeBase.Code);
            if ( ValueSearchResults.Count == 0 )
            {
                Result.WarningAdd("No name for structure type " + structureTypeBase.Code + ".");
            }
            else
            {
                structureTypeBase.Name = Convert.ToString(ValueSearchResults[0][1]);
            }
        }

        public void SetTemplateName(SimpleList<string[]> Names, DroidTemplate Template, clsResult Result)
        {
            var ValueSearchResults = default(SimpleList<string[]>);

            ValueSearchResults = GetRowsWithValue(Names, Template.Code);
            if ( ValueSearchResults.Count == 0 )
            {
                Result.WarningAdd("No name for droid template " + Template.Code + ".");
            }
            else
            {
                Template.Name = Convert.ToString(ValueSearchResults[0][1]);
            }
        }

        public void SetWallName(SimpleList<string[]> Names, clsWallType WallType, clsResult Result)
        {
            var ValueSearchResults = default(SimpleList<string[]>);

            ValueSearchResults = GetRowsWithValue(Names, WallType.Code);
            if ( ValueSearchResults.Count == 0 )
            {
                Result.WarningAdd("No name for structure type " + WallType.Code + ".");
            }
            else
            {
                WallType.Name = Convert.ToString(ValueSearchResults[0][1]);
            }
        }

        public Body FindBodyCode(string Code)
        {
            var Component = default(Body);

            foreach ( var tempLoopVar_Component in Bodies )
            {
                Component = tempLoopVar_Component;
                if ( Component.Code == Code )
                {
                    return Component;
                }
            }

            return null;
        }

        public Propulsion FindPropulsionCode(string Code)
        {
            var Component = default(Propulsion);

            foreach ( var tempLoopVar_Component in Propulsions )
            {
                Component = tempLoopVar_Component;
                if ( Component.Code == Code )
                {
                    return Component;
                }
            }

            return null;
        }

        public Construct FindConstructorCode(string Code)
        {
            var Component = default(Construct);

            foreach ( var tempLoopVar_Component in Constructors )
            {
                Component = tempLoopVar_Component;
                if ( Component.Code == Code )
                {
                    return Component;
                }
            }

            return null;
        }

        public Sensor FindSensorCode(string Code)
        {
            var Component = default(Sensor);

            foreach ( var tempLoopVar_Component in Sensors )
            {
                Component = tempLoopVar_Component;
                if ( Component.Code == Code )
                {
                    return Component;
                }
            }

            return null;
        }

        public Repair FindRepairCode(string Code)
        {
            var Component = default(Repair);

            foreach ( var tempLoopVar_Component in Repairs )
            {
                Component = tempLoopVar_Component;
                if ( Component.Code == Code )
                {
                    return Component;
                }
            }

            return null;
        }

        public Ecm FindECMCode(string Code)
        {
            var Component = default(Ecm);

            foreach ( var tempLoopVar_Component in ECMs )
            {
                Component = tempLoopVar_Component;
                if ( Component.Code == Code )
                {
                    return Component;
                }
            }

            return null;
        }

        public Brain FindBrainCode(string Code)
        {
            var Component = default(Brain);

            foreach ( var tempLoopVar_Component in Brains )
            {
                Component = tempLoopVar_Component;
                if ( Component.Code == Code )
                {
                    return Component;
                }
            }

            return null;
        }

        public Weapon FindWeaponCode(string Code)
        {
            var Component = default(Weapon);

            foreach ( var tempLoopVar_Component in Weapons )
            {
                Component = tempLoopVar_Component;
                if ( Component.Code == Code )
                {
                    return Component;
                }
            }

            return null;
        }

        public int Get_TexturePage_GLTexture(string FileTitle)
        {
            var LCaseTitle = FileTitle.ToLower();
            var TexPage = default(clsTexturePage);

            foreach ( var tempLoopVar_TexPage in TexturePages )
            {
                TexPage = tempLoopVar_TexPage;
                if ( TexPage.FileTitle.ToLower() == LCaseTitle )
                {
                    return TexPage.GLTexture_Num;
                }
            }
            return 0;
        }

        public Weapon FindOrCreateWeapon(string Code)
        {
            var Result = default(Weapon);

            Result = FindWeaponCode(Code);
            if ( Result != null )
            {
                return Result;
            }
            Result = new Weapon();
            Result.IsUnknown = true;
            Result.Code = Code;
            return Result;
        }

        public Construct FindOrCreateConstruct(string Code)
        {
            var Result = default(Construct);

            Result = FindConstructorCode(Code);
            if ( Result != null )
            {
                return Result;
            }
            Result = new Construct();
            Result.IsUnknown = true;
            Result.Code = Code;
            return Result;
        }

        public Repair FindOrCreateRepair(string Code)
        {
            var Result = default(Repair);

            Result = FindRepairCode(Code);
            if ( Result != null )
            {
                return Result;
            }
            Result = new Repair();
            Result.IsUnknown = true;
            Result.Code = Code;
            return Result;
        }

        public Sensor FindOrCreateSensor(string Code)
        {
            var Result = default(Sensor);

            Result = FindSensorCode(Code);
            if ( Result != null )
            {
                return Result;
            }
            Result = new Sensor();
            Result.IsUnknown = true;
            Result.Code = Code;
            return Result;
        }

        public Brain FindOrCreateBrain(string Code)
        {
            var Result = default(Brain);

            Result = FindBrainCode(Code);
            if ( Result != null )
            {
                return Result;
            }
            Result = new Brain();
            Result.IsUnknown = true;
            Result.Code = Code;
            return Result;
        }

        public Ecm FindOrCreateECM(string Code)
        {
            var Result = default(Ecm);

            Result = FindECMCode(Code);
            if ( Result != null )
            {
                return Result;
            }
            Result = new Ecm();
            Result.IsUnknown = true;
            Result.Code = Code;
            return Result;
        }

        public Turret FindOrCreateTurret(TurretType TurretType, string TurretCode)
        {
            switch ( TurretType )
            {
                case TurretType.Weapon:
                    return FindOrCreateWeapon(TurretCode);
                case TurretType.Construct:
                    return FindOrCreateConstruct(TurretCode);
                case TurretType.Repair:
                    return FindOrCreateRepair(TurretCode);
                case TurretType.Sensor:
                    return FindOrCreateSensor(TurretCode);
                case TurretType.Brain:
                    return FindOrCreateBrain(TurretCode);
                case TurretType.ECM:
                    return FindOrCreateECM(TurretCode);
                default:
                    return null;
            }
        }

        public Body FindOrCreateBody(string Code)
        {
            var Result = default(Body);

            Result = FindBodyCode(Code);
            if ( Result != null )
            {
                return Result;
            }
            Result = new Body();
            Result.IsUnknown = true;
            Result.Code = Code;
            return Result;
        }

        public Propulsion FindOrCreatePropulsion(string Code)
        {
            var Result = default(Propulsion);

            Result = FindPropulsionCode(Code);
            if ( Result != null )
            {
                return Result;
            }
            Result = new Propulsion(Bodies.Count);
            Result.IsUnknown = true;
            Result.Code = Code;
            return Result;
        }

        public UnitTypeBase FindOrCreateUnitType(string Code, UnitType Type, int WallType)
        {
            switch ( Type )
            {
                case UnitType.Feature:
                    var featureTypeBase = default(FeatureTypeBase);
                    foreach ( var tempLoopVar_FeatureType in FeatureTypes )
                    {
                        featureTypeBase = tempLoopVar_FeatureType;
                        if ( featureTypeBase.Code == Code )
                        {
                            return featureTypeBase;
                        }
                    }
                    featureTypeBase = new FeatureTypeBase();
                    featureTypeBase.IsUnknown = true;
                    featureTypeBase.Code = Code;
                    featureTypeBase.Footprint.X = 1;
                    featureTypeBase.Footprint.Y = 1;
                    return featureTypeBase;
                case UnitType.PlayerStructure:
                    var structureTypeBase = default(StructureTypeBase);
                    foreach ( var tempLoopVar_StructureType in StructureTypes )
                    {
                        structureTypeBase = tempLoopVar_StructureType;
                        if ( structureTypeBase.Code == Code )
                        {
                            if ( WallType < 0 )
                            {
                                return structureTypeBase;
                            }
                            if ( structureTypeBase.WallLink.IsConnected )
                            {
                                if ( structureTypeBase.WallLink.ArrayPosition == WallType )
                                {
                                    return structureTypeBase;
                                }
                            }
                        }
                    }
                    structureTypeBase = new StructureTypeBase();
                    structureTypeBase.IsUnknown = true;
                    structureTypeBase.Code = Code;
                    structureTypeBase.Footprint.X = 1;
                    structureTypeBase.Footprint.Y = 1;
                    return structureTypeBase;

                case UnitType.PlayerDroid:
                    var DroidType = default(DroidTemplate);
                    foreach ( var tempLoopVar_DroidType in DroidTemplates )
                    {
                        DroidType = tempLoopVar_DroidType;
                        if ( DroidType.IsTemplate )
                        {
                            if ( DroidType.Code == Code )
                            {
                                return DroidType;
                            }
                        }
                    }
                    DroidType = new DroidTemplate();
                    DroidType.IsUnknown = true;
                    DroidType.Code = Code;
                    return DroidType;
                default:
                    return null;
            }
        }

        public StructureTypeBase FindFirstStructureType(StructureType Type)
        {
            var structureTypeBase = default(StructureTypeBase);

            foreach ( var tempLoopVar_StructureType in StructureTypes )
            {
                structureTypeBase = tempLoopVar_StructureType;
                if ( structureTypeBase.StructureType == Type )
                {
                    return structureTypeBase;
                }
            }

            return null;
        }
    }
}