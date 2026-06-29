using SandBox;
using SeparatistCrisis.Scenes.DistanceCache;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.ComponentInterfaces;
using TaleWorlds.CampaignSystem.GameComponents;
using TaleWorlds.CampaignSystem.Map.DistanceCache;
using TaleWorlds.CampaignSystem.Party;
using TaleWorlds.DotNet;
using TaleWorlds.Engine;
using TaleWorlds.Library;
using TaleWorlds.ModuleManager;
using TaleWorlds.MountAndBlade;

// Copied SettlementPositonScript, SettlementPositonScript.SettlementPositionScriptNavigationCache and SettlementPositonScript.SettlementRecord
// All of that for me to add some parallelism to the compute distance cache methods
namespace SeparatistCrisis.Scenes.ScriptComponents
{
    public class SCSettlementPositionScript : ScriptComponentBehavior
    {
        private string SettlementsXmlPath
        {
            get
            {
                string text = base.Scene.GetModulePath();
                if (text.Contains("$BASE"))
                {
                    text = text.Remove(0, 6);
                    text = BasePath.Name + text;
                }
                return text + "ModuleData/settlements.xml";
            }
        }

        protected override void OnInit()
        {
            try
            {
                this.InitializeCachedVariables();
                bool useNavalNavigation = false;
                if (this.GetMapIsNavalDLC() || (!this.GetMapIsSandBox() && ModuleHelper.IsModuleActive("NavalDLC")))
                {
                    useNavalNavigation = true;
                }
                this.RegisterNavigationCachesOnGameLoad(useNavalNavigation);
            }
            catch (Exception ex)
            {
                Debug.Print("Error when reading distance cache " + ex.Message, 0, Debug.DebugColor.White, 17592186044416UL);
                Debug.Print("SettlementsDistanceCacheFilePath could not be read!. Campaign starting performance will be affected very badly, cache will be initialized now.", 0, Debug.DebugColor.White, 17592186044416UL);
                Debug.FailedAssert("SettlementsDistanceCacheFilePath could not be read!. Campaign starting performance will be affected very badly, cache will be initialized now.", "C:\\BuildAgent\\work\\mb3\\Source\\Bannerlord\\SandBox.View\\Map\\SCSettlementPositionScript.cs", "OnInit", 536);
            }
        }

        private void RegisterNavigationCachesOnGameLoad(bool useNavalNavigation)
        {
            SandBoxNavigationCache cacheToRegister = this.ReadNavigationCacheForNavigationTypeOnGameLoad(MobileParty.NavigationType.Default);
            this._mapDistanceModel.RegisterDistanceCache(MobileParty.NavigationType.Default, cacheToRegister);
            if (useNavalNavigation)
            {
                SandBoxNavigationCache cacheToRegister2 = this.ReadNavigationCacheForNavigationTypeOnGameLoad(MobileParty.NavigationType.Naval);
                SandBoxNavigationCache cacheToRegister3 = this.ReadNavigationCacheForNavigationTypeOnGameLoad(MobileParty.NavigationType.All);
                this._mapDistanceModel.RegisterDistanceCache(MobileParty.NavigationType.Naval, cacheToRegister2);
                this._mapDistanceModel.RegisterDistanceCache(MobileParty.NavigationType.All, cacheToRegister3);
            }
        }

        private SandBoxNavigationCache ReadNavigationCacheForNavigationTypeOnGameLoad(MobileParty.NavigationType navigationCapability)
        {
            string text = string.Empty;
            foreach (ModuleInfo moduleInfo in ModuleHelper.GetActiveModules())
            {
                string text2;
                if (moduleInfo.IsActive && this.GetSettlementsDistanceCacheFileForCapability(moduleInfo.Id, navigationCapability, out text2))
                {
                    text = text2;
                }
            }
            SandBoxNavigationCache sandBoxNavigationCache;
            if (!string.IsNullOrEmpty(text))
            {
                sandBoxNavigationCache = this.ReadNavigationCacheOnGameLoad(text, navigationCapability);
            }
            else
            {
                Debug.FailedAssert(string.Format("Navigation type with id {0} file is not found, this should not be happening, will generate cache (this will take some time)", navigationCapability), "C:\\BuildAgent\\work\\mb3\\Source\\Bannerlord\\SandBox.View\\Map\\SCSettlementPositionScript.cs", "ReadNavigationCacheForNavigationTypeOnGameLoad", 576);
                sandBoxNavigationCache = new SandBoxNavigationCache(navigationCapability);
                sandBoxNavigationCache.GenerateCacheData();
            }
            return sandBoxNavigationCache;
        }

        private SandBoxNavigationCache ReadNavigationCacheOnGameLoad(string path, MobileParty.NavigationType navigationCapability)
        {
            SandBoxNavigationCache sandBoxNavigationCache = new SandBoxNavigationCache(navigationCapability);
            sandBoxNavigationCache.Deserialize(path);
            return sandBoxNavigationCache;
        }

        protected override void OnEditorInit()
        {
            base.OnEditorInit();
            this._partyNavigationModelOverriddenClassName = "";
            this._distanceModelOverridenClassName = "";
            this.InitializeCachedVariables();
        }

        protected override void OnEditorVariableChanged(string variableName)
        {
            base.OnEditorVariableChanged(variableName);
            if (variableName == "SavePositions")
            {
                this.SaveSettlementPositions();
            }
            if (variableName == "ComputeAndSaveSettlementDistanceCache")
            {
                this.SaveSettlementDistanceCacheEditor();
            }
            if (variableName == "CheckPositions")
            {
                this.CheckSettlementPositions();
            }
            if (variableName == "_partyNavigationModelOverriddenClassName" || variableName == "_distanceModelOverridenClassName")
            {
                this.InitializeCachedVariables();
            }
        }

        protected override void OnSceneSave(string saveFolder)
        {
            base.OnSceneSave(saveFolder);
            this.SaveSettlementPositions();
        }

        private void CheckSettlementPositions()
        {
            XmlDocument xmlDocument = this.LoadXmlFile(this.SettlementsXmlPath);
            base.GameEntity.RemoveAllChildren();
            PartyNavigationModel partyNavigationModel = this.GetPartyNavigationModel();
            bool[] regionMapping = SandBoxHelpers.MapSceneHelper.GetRegionMapping(partyNavigationModel);
            base.GameEntity.Scene.SetNavMeshRegionMap(regionMapping);
            List<int> list = partyNavigationModel.GetInvalidTerrainTypesForNavigationType(MobileParty.NavigationType.Default).ToList<int>();
            list.Add(0);
            List<int> list2 = null;
            foreach (object obj in xmlDocument.DocumentElement.SelectNodes("Settlement"))
            {
                string value = ((XmlNode)obj).Attributes["id"].Value;
                GameEntity campaignEntityWithName = base.Scene.GetCampaignEntityWithName(value);
                if (campaignEntityWithName != null)
                {
                    Vec3 origin = campaignEntityWithName.GetGlobalFrame().origin;
                    Vec3 vec = default(Vec3);
                    Vec3 pos = default(Vec3);
                    List<GameEntity> list3 = new List<GameEntity>();
                    campaignEntityWithName.GetChildrenRecursive(ref list3);
                    bool flag = false;
                    bool flag2 = false;
                    foreach (GameEntity gameEntity in list3)
                    {
                        if (gameEntity.HasTag("main_map_city_gate"))
                        {
                            vec = gameEntity.GetGlobalFrame().origin;
                            flag = true;
                        }
                        if (gameEntity.HasTag("main_map_city_port"))
                        {
                            pos = gameEntity.GetGlobalFrame().origin;
                            flag2 = true;
                        }
                    }
                    Vec3 pos2 = origin;
                    if (flag)
                    {
                        pos2 = vec;
                    }
                    PathFaceRecord nullFaceRecord = PathFaceRecord.NullFaceRecord;
                    base.GameEntity.Scene.GetNavMeshFaceIndex(ref nullFaceRecord, pos2.AsVec2, true, true, false);
                    int item = 0;
                    if (nullFaceRecord.IsValid())
                    {
                        item = nullFaceRecord.FaceGroupIndex;
                    }
                    if (list.Contains(item))
                    {
                        Debug.Print(string.Format("There is gate position problem with settlement {0} at position:  {1}", campaignEntityWithName.Name, pos2.AsVec2), 0, Debug.DebugColor.White, 17592186044416UL);
                        MBEditor.ZoomToPosition(pos2);
                        break;
                    }
                    if (flag2)
                    {
                        if (list2 == null)
                        {
                            list2 = partyNavigationModel.GetInvalidTerrainTypesForNavigationType(MobileParty.NavigationType.Naval).ToList<int>();
                            list2.Add(0);
                        }
                        nullFaceRecord = PathFaceRecord.NullFaceRecord;
                        base.GameEntity.Scene.GetNavMeshFaceIndex(ref nullFaceRecord, pos.AsVec2, false, true, false);
                        item = 0;
                        if (nullFaceRecord.IsValid())
                        {
                            item = nullFaceRecord.FaceGroupIndex;
                        }
                        if (list2.Contains(item))
                        {
                            Debug.Print(string.Format("There is port position problem with settlement {0} at position:  {1}", campaignEntityWithName.Name, pos.AsVec2), 0, Debug.DebugColor.White, 17592186044416UL);
                            MBEditor.ZoomToPosition(pos);
                            break;
                        }
                    }
                }
            }
        }

        private void InitializeCachedVariables()
        {
            this._mapIsNavalDLC = string.Equals("NavalDLC", this.GetMapModuleId(), StringComparison.CurrentCultureIgnoreCase);
            this._mapIsSandBox = string.Equals("Sandbox", this.GetMapModuleId(), StringComparison.CurrentCultureIgnoreCase);
            this._partyNavigationModel = this.GetPartyNavigationModel();
            this._mapDistanceModel = this.GetMapDistanceModel();
        }

        protected override bool IsOnlyVisual()
        {
            return true;
        }

        private bool GetMapIsNavalDLC()
        {
            return this._mapIsNavalDLC;
        }

        private bool GetMapIsSandBox()
        {
            return this._mapIsSandBox;
        }

        private string GetMapModuleId()
        {
            return base.Scene.GetModulePath().Trim().TrimEnd(new char[]
            {
                '/'
            }).Split(new char[]
            {
                '/'
            }).Last<string>();
        }

        private PartyNavigationModel GetPartyNavigationModel()
        {
            if (Campaign.Current != null)
            {
                return Campaign.Current.Models.PartyNavigationModel;
            }
            if (string.IsNullOrEmpty(this._partyNavigationModelOverriddenClassName))
            {
                if (this.GetMapIsSandBox())
                {
                    this._partyNavigationModelOverriddenClassName = "DefaultPartyNavigationModel";
                    return SCSettlementPositionScript.CreateBaseNavigationModel(false);
                }
                if (this.GetMapIsNavalDLC())
                {
                    if (!ModuleHelper.IsModuleActive("NavalDLC"))
                    {
                        throw new ApplicationException("NavalDlc map changes can not be made without NavalDlc module!");
                    }
                    this._partyNavigationModelOverriddenClassName = "NavalPartyNavigationModel";
                    return SCSettlementPositionScript.CreateBaseNavigationModel(true);
                }
                else
                {
                    if (ModuleHelper.IsModuleActive("NavalDLC"))
                    {
                        this._partyNavigationModelOverriddenClassName = "NavalPartyNavigationModel";
                        return SCSettlementPositionScript.CreateBaseNavigationModel(true);
                    }
                    this._partyNavigationModelOverriddenClassName = "DefaultPartyNavigationModel";
                    return SCSettlementPositionScript.CreateBaseNavigationModel(false);
                }
            }
            else
            {
                if (SCSettlementPositionScript.FindClass(this._partyNavigationModelOverriddenClassName) == null)
                {
                    Debug.FailedAssert("Cant find custom navigation model", "C:\\BuildAgent\\work\\mb3\\Source\\Bannerlord\\SandBox.View\\Map\\SCSettlementPositionScript.cs", "GetPartyNavigationModel", 826);
                    return SCSettlementPositionScript.CreateBaseNavigationModel(this.GetMapIsNavalDLC());
                }
                return SCSettlementPositionScript.CreateCustomNavigationModel(this._partyNavigationModelOverriddenClassName, !this.GetMapIsSandBox() && ModuleHelper.IsModuleActive("NavalDLC"));
            }
        }

        private MapDistanceModel GetMapDistanceModel()
        {
            if (Campaign.Current != null)
            {
                return Campaign.Current.Models.MapDistanceModel;
            }
            if (string.IsNullOrEmpty(this._distanceModelOverridenClassName))
            {
                if (this.GetMapIsSandBox())
                {
                    this._distanceModelOverridenClassName = "DefaultMapDistanceModel";
                    return SCSettlementPositionScript.CreateBaseDistanceModel(false);
                }
                if (this.GetMapIsNavalDLC())
                {
                    if (!ModuleHelper.IsModuleActive("NavalDLC"))
                    {
                        throw new ApplicationException("NavalDlc map changes can not be made without NavalDlc module!");
                    }
                    this._distanceModelOverridenClassName = "NavalDLCMapDistanceModel";
                    return SCSettlementPositionScript.CreateBaseDistanceModel(true);
                }
                else
                {
                    if (ModuleHelper.IsModuleActive("NavalDLC"))
                    {
                        this._distanceModelOverridenClassName = "NavalDLCMapDistanceModel";
                        return SCSettlementPositionScript.CreateBaseDistanceModel(true);
                    }
                    this._distanceModelOverridenClassName = "DefaultMapDistanceModel";
                    return SCSettlementPositionScript.CreateBaseDistanceModel(false);
                }
            }
            else
            {
                if (SCSettlementPositionScript.FindClass(this._distanceModelOverridenClassName) == null)
                {
                    Debug.FailedAssert("Cant find custom navigation model", "C:\\BuildAgent\\work\\mb3\\Source\\Bannerlord\\SandBox.View\\Map\\SCSettlementPositionScript.cs", "GetMapDistanceModel", 882);
                    return SCSettlementPositionScript.CreateBaseDistanceModel(this.GetMapIsNavalDLC());
                }
                return SCSettlementPositionScript.CreateCustomMapDistanceModel(this._distanceModelOverridenClassName, !this.GetMapIsSandBox() && ModuleHelper.IsModuleActive("NavalDLC"));
            }
        }

        private static PartyNavigationModel CreateCustomNavigationModel(string name, bool naval)
        {
            if (name == "DefaultPartyNavigationModel")
            {
                return SCSettlementPositionScript.CreateBaseNavigationModel(false);
            }
            Type type = SCSettlementPositionScript.FindClass(name);
            if (type == null)
            {
                Debug.FailedAssert("Cant find custom navigation model", "C:\\BuildAgent\\work\\mb3\\Source\\Bannerlord\\SandBox.View\\Map\\SCSettlementPositionScript.cs", "CreateCustomNavigationModel", 903);
                return SCSettlementPositionScript.CreateBaseNavigationModel(naval);
            }
            if (type.GetConstructor(new Type[]
            {
                typeof(PartyNavigationModel)
            }) != null)
            {
                return (PartyNavigationModel)Activator.CreateInstance(type, new object[]
                {
                    SCSettlementPositionScript.CreateBaseNavigationModel(naval)
                });
            }
            return (PartyNavigationModel)Activator.CreateInstance(type);
        }

        private static MapDistanceModel CreateCustomMapDistanceModel(string name, bool naval)
        {
            if (name == "DefaultMapDistanceModel")
            {
                return SCSettlementPositionScript.CreateBaseDistanceModel(false);
            }
            Type type = SCSettlementPositionScript.FindClass(name);
            if (type == null)
            {
                Debug.FailedAssert("Cant find custom navigation model", "C:\\BuildAgent\\work\\mb3\\Source\\Bannerlord\\SandBox.View\\Map\\SCSettlementPositionScript.cs", "CreateCustomMapDistanceModel", 930);
                return SCSettlementPositionScript.CreateBaseDistanceModel(naval);
            }
            return (MapDistanceModel)Activator.CreateInstance(type);
        }

        private static Type FindClass(string name)
        {
            Type result = null;
            Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies();
            for (int i = 0; i < assemblies.Length; i++)
            {
                foreach (Type type in assemblies[i].GetTypesSafe(null))
                {
                    if (type.Name == name)
                    {
                        result = type;
                        break;
                    }
                }
            }
            return result;
        }

        private static PartyNavigationModel CreateBaseNavigationModel(bool naval)
        {
            if (!naval)
            {
                return new DefaultPartyNavigationModel();
            }
            Type type = SCSettlementPositionScript.FindClass("NavalPartyNavigationModel");
            if (type == null)
            {
                throw new ArgumentException("Cant find naval navigation model");
            }
            return (PartyNavigationModel)Activator.CreateInstance(type, new object[]
            {
                SCSettlementPositionScript.CreateBaseNavigationModel(false)
            });
        }

        private static MapDistanceModel CreateBaseDistanceModel(bool naval)
        {
            if (!naval)
            {
                return new DefaultMapDistanceModel();
            }
            Type type = SCSettlementPositionScript.FindClass("NavalDLCMapDistanceModel");
            if (type == null)
            {
                throw new ArgumentException("Cant find naval navigation model");
            }
            return (MapDistanceModel)Activator.CreateInstance(type);
        }

        private static MapDistanceModel CreateBaseDistanceModel()
        {
            return new DefaultMapDistanceModel();
        }

        private bool GetSettlementsDistanceCacheFileForCapability(string moduleId, MobileParty.NavigationType navigationType, out string filePath)
        {
            string text = ModuleHelper.GetModuleFullPath(moduleId) + "ModuleData/DistanceCaches";
            string str = navigationType.ToString();
            filePath = text + "/settlements_distance_cache_" + str + ".bin";
            bool flag = File.Exists(filePath);
            if (flag)
            {
                Debug.Print(string.Format("Found distance cache at: {0}, {1}, {2}", moduleId, text, navigationType), 0, Debug.DebugColor.White, 17592186044416UL);
            }
            return flag;
        }

        private List<SCSettlementRecord> LoadSettlementData(XmlDocument settlementDocument)
        {
            List<SCSettlementRecord> list = new List<SCSettlementRecord>();
            base.GameEntity.RemoveAllChildren();
            foreach (object obj in settlementDocument.DocumentElement.SelectNodes("Settlement"))
            {
                XmlNode xmlNode = (XmlNode)obj;
                string value = xmlNode.Attributes["name"].Value;
                string value2 = xmlNode.Attributes["id"].Value;
                GameEntity campaignEntityWithName = base.Scene.GetCampaignEntityWithName(value2);
                if (!(campaignEntityWithName == null))
                {
                    Vec2 asVec = campaignEntityWithName.GetGlobalFrame().origin.AsVec2;
                    Vec2 vec = default(Vec2);
                    List<GameEntity> list2 = new List<GameEntity>();
                    campaignEntityWithName.GetChildrenRecursive(ref list2);
                    bool flag = false;
                    bool hasPort = false;
                    Vec2 portPosition = default(Vec2);
                    foreach (GameEntity gameEntity in list2)
                    {
                        if (gameEntity.HasTag("main_map_city_gate"))
                        {
                            vec = gameEntity.GetGlobalFrame().origin.AsVec2;
                            flag = true;
                        }
                        if (gameEntity.HasTag("main_map_city_port"))
                        {
                            portPosition = gameEntity.GetGlobalFrame().origin.AsVec2;
                            hasPort = true;
                        }
                    }
                    bool isFortification = false;
                    foreach (XmlNode childNode in xmlNode.ChildNodes)
                    {
                        if (childNode.Name.Equals("Components"))
                        {
                            IEnumerator enumerator = childNode.ChildNodes.GetEnumerator();
                            try
                            {
                                while (enumerator.MoveNext())
                                {
                                    XmlNode current = (XmlNode)enumerator.Current;
                                    if (current.Name.Equals("Town"))
                                    {
                                        int num = current.Attributes["is_castle"] == null ? 0 : (bool.Parse(current.Attributes["is_castle"].Value) ? 1 : 0);
                                        isFortification = true;
                                        break;
                                    }
                                }
                                break;
                            }
                            finally
                            {
                                if (enumerator is IDisposable disposable)
                                    disposable.Dispose();
                            }
                        }
                    }
                    list.Add(new SCSettlementRecord(value2, asVec, flag ? vec : asVec, xmlNode, flag, portPosition, hasPort, isFortification));
                }
            }
            return list;
        }

        private XmlDocument LoadXmlFile(string path)
        {
            Debug.Print("opening " + path, 0, Debug.DebugColor.White, 17592186044416UL);
            XmlDocument xmlDocument = new XmlDocument();
            StreamReader streamReader = new StreamReader(path);
            string xml = streamReader.ReadToEnd();
            xmlDocument.LoadXml(xml);
            streamReader.Close();
            return xmlDocument;
        }

        private void SaveSettlementPositions()
        {
            XmlDocument xmlDocument = this.LoadXmlFile(this.SettlementsXmlPath);
            foreach (SCSettlementRecord settlementRecord in this.LoadSettlementData(xmlDocument))
            {
                string value = settlementRecord.Node.Attributes["name"].Value;
                if (settlementRecord.Node.Attributes["posX"] == null)
                {
                    XmlAttribute node = xmlDocument.CreateAttribute("posX");
                    settlementRecord.Node.Attributes.Append(node);
                }
                settlementRecord.Node.Attributes["posX"].Value = settlementRecord.Position.X.ToString();
                if (settlementRecord.Node.Attributes["posY"] == null)
                {
                    XmlAttribute node2 = xmlDocument.CreateAttribute("posY");
                    settlementRecord.Node.Attributes.Append(node2);
                }
                settlementRecord.Node.Attributes["posY"].Value = settlementRecord.Position.Y.ToString();
                if (settlementRecord.HasGate)
                {
                    if (settlementRecord.Node.Attributes["gate_posX"] == null)
                    {
                        XmlAttribute node3 = xmlDocument.CreateAttribute("gate_posX");
                        settlementRecord.Node.Attributes.Append(node3);
                    }
                    settlementRecord.Node.Attributes["gate_posX"].Value = settlementRecord.GatePosition.X.ToString();
                    if (settlementRecord.Node.Attributes["gate_posY"] == null)
                    {
                        XmlAttribute node4 = xmlDocument.CreateAttribute("gate_posY");
                        settlementRecord.Node.Attributes.Append(node4);
                    }
                    settlementRecord.Node.Attributes["gate_posY"].Value = settlementRecord.GatePosition.Y.ToString();
                }
                if (settlementRecord.HasPort)
                {
                    if (settlementRecord.Node.Attributes["port_posX"] == null)
                    {
                        XmlAttribute node5 = xmlDocument.CreateAttribute("port_posX");
                        settlementRecord.Node.Attributes.Append(node5);
                    }
                    settlementRecord.Node.Attributes["port_posX"].Value = settlementRecord.PortPosition.X.ToString();
                    if (settlementRecord.Node.Attributes["port_posY"] == null)
                    {
                        XmlAttribute node6 = xmlDocument.CreateAttribute("port_posY");
                        settlementRecord.Node.Attributes.Append(node6);
                    }
                    settlementRecord.Node.Attributes["port_posY"].Value = settlementRecord.PortPosition.Y.ToString();
                }
            }
            xmlDocument.Save(this.SettlementsXmlPath);
        }

        private async void SaveSettlementDistanceCacheEditor()
        {
            bool[] regionMapping = SandBoxHelpers.MapSceneHelper.GetRegionMapping(this._partyNavigationModel);
            base.Scene.SetNavMeshRegionMap(regionMapping);
            List<MobileParty.NavigationType> list = new List<MobileParty.NavigationType>
            {
                MobileParty.NavigationType.Default
            };
            if (this.GetMapIsNavalDLC() || (!this.GetMapIsSandBox() && ModuleHelper.IsModuleActive("NavalDLC")))
            {
                list.Add(MobileParty.NavigationType.Naval);
                list.Add(MobileParty.NavigationType.All);
            }
            foreach (MobileParty.NavigationType navigationType in list)
            {
                int[] invalidTerrainTypesForNavigationType = this._partyNavigationModel.GetInvalidTerrainTypesForNavigationType(navigationType);
                try
                {
                    XmlDocument settlementDocument = this.LoadXmlFile(this.SettlementsXmlPath);
                    List<SCSettlementRecord> settlementRecords = this.LoadSettlementData(settlementDocument);
                    Debug.Print($"Num SettlementRecords: {settlementRecords.Count}");
                    foreach (int faceGroupId in invalidTerrainTypesForNavigationType)
                    {
                        base.Scene.SetAbilityOfFacesWithId(faceGroupId, false);
                    }
                    SCNavigationCache SCSettlementPositionScriptNavigationCache = new SCNavigationCache(settlementRecords, base.Scene, this._mapDistanceModel, this._partyNavigationModel, navigationType);
                    await SCSettlementPositionScriptNavigationCache.GenerateCacheData();
                    Debug.Print($"AfterGenerateCache SaveSettlementDistanceCacheEditor {DateTime.UtcNow.ToString()}\r\n");
                    string path;
                    this.GetSettlementsDistanceCacheFileForCapability(this.GetMapModuleId(), navigationType, out path);
                    SCSettlementPositionScriptNavigationCache.Serialize(path);
                }
                catch
                {
                }
                finally
                {
                    foreach (int faceGroupId2 in invalidTerrainTypesForNavigationType)
                    {
                        base.Scene.SetAbilityOfFacesWithId(faceGroupId2, true);
                    }

                    Debug.ShowWarning("Completed ComputeDistanceCache");
                }
            }
        }

        private const string SandBoxModuleId = "Sandbox";

        private const string NavalDLCModuleId = "NavalDLC";

        private const string NavalPartyNavigationModelName = "NavalPartyNavigationModel";

        private const string NavalMapDistanceModelName = "NavalDLCMapDistanceModel";

        private bool _mapIsSandBox;

        private bool _mapIsNavalDLC;

        [EditableScriptComponentVariable(true, "")]
        private string _partyNavigationModelOverriddenClassName;

        [EditableScriptComponentVariable(true, "")]
        private string _distanceModelOverridenClassName;

        private PartyNavigationModel _partyNavigationModel;

        private MapDistanceModel _mapDistanceModel;

        public SimpleButton CheckPositions;

        public SimpleButton SavePositions;

        public SimpleButton ComputeAndSaveSettlementDistanceCache;
    }
}
