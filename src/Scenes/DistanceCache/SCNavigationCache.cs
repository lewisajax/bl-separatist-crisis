using SeparatistCrisis.Scenes.Tasks;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.ComponentInterfaces;
using TaleWorlds.CampaignSystem.Map.DistanceCache;
using TaleWorlds.CampaignSystem.Party;
using TaleWorlds.Engine;
using TaleWorlds.Library;
using TaleWorlds.LinQuick;
using TaleWorlds.ModuleManager;
using TaleWorlds.MountAndBlade;

// Only used by the editor, we will stick with SandBoxNavigationCache for the game
namespace SeparatistCrisis.Scenes.DistanceCache
{
    public class SCNavigationCache
    {
        private ConcurrentDictionary<int, NavigationCacheElement<SCSettlementRecord>> _closestSettlementsToFaceIndices;
        private ConcurrentDictionary<SCSettlementRecord, MBReadOnlyList<SCSettlementRecord>> _fortificationNeighbors;
        private ConcurrentDictionary<NavigationCacheElement<SCSettlementRecord>, Dictionary<NavigationCacheElement<SCSettlementRecord>, ValueTuple<float, float>>> _settlementToSettlementDistanceWithLandRatio;
        protected const float AgentRadius = 0.3f;
        protected const float ExtraCostMultiplierForNeighborDetection = 2f;

        public float MaximumDistanceBetweenTwoConnectedSettlements { get; protected set; }

        protected MobileParty.NavigationType _navigationType { get; private set; }

        public SCNavigationCache(List<SCSettlementRecord> settlementRecords, Scene scene, MapDistanceModel mapDistanceModel, PartyNavigationModel partyNavigationModel, MobileParty.NavigationType navigationType)
        {
            this.Scene = scene;
            this._settlementRecords = settlementRecords;
            this._excludedFaceIds = partyNavigationModel.GetInvalidTerrainTypesForNavigationType(this._navigationType);
            this._regionSwitchCostTo0 = mapDistanceModel.RegionSwitchCostFromLandToSea;
            this._regionSwitchCostTo1 = mapDistanceModel.RegionSwitchCostFromSeaToLand;
            this._navigationType = navigationType;
            this._settlementToSettlementDistanceWithLandRatio = new ConcurrentDictionary<NavigationCacheElement<SCSettlementRecord>, Dictionary<NavigationCacheElement<SCSettlementRecord>, ValueTuple<float, float>>>();
            this._fortificationNeighbors = new ConcurrentDictionary<SCSettlementRecord, MBReadOnlyList<SCSettlementRecord>>();
            this._closestSettlementsToFaceIndices = new ConcurrentDictionary<int, NavigationCacheElement<SCSettlementRecord>>();
        }

        // We only need this and the called methods
        public async Task GenerateCacheData()
        {
            Debug.Print($"Start GenerateCacheData {DateTime.UtcNow.ToString()}\r\n");
            SCParallel.InitializeAndSetImplementation(); // Sets the driver
            await this.SCGenerateClosestSettlementToFaceCache();
            await this.SCGenerateSettlementToSettlementDistanceCache();
            await this.SCGenerateNeighborSettlementsCache();
            Debug.Print($"End GenerateCacheData {DateTime.UtcNow.ToString()}\r\n");

            //Task.WhenAll(new Task[] {
            //    Task.Run(() => this.SCGenerateClosestSettlementToFaceCache()),
            //    Task.Run(() => this.SCGenerateSettlementToSettlementDistanceCache()),
            //    Task.Run(() => this.SCGenerateNeighborSettlementsCache()),
            //});
        }

        public async Task SCGenerateClosestSettlementToFaceCache()
        {
            Debug.Print($"Start SCGenerateClosestSettlementToFaceCache() {DateTime.UtcNow.ToString()}\r\n");
            int navMeshFaceCount = this.GetNavMeshFaceCount();
            Debug.Print($"navMeshFaceCount: {navMeshFaceCount}");
            await SCParallel.ForWhenAll(0, navMeshFaceCount, delegate (int startInclusive, int endExclusive)
            {
                for (int i = startInclusive; i < endExclusive; i++)
                {
                    // Debug.Print(string.Format("Face-Settlement cache creation progress % {0}     {1}", i * 100 / navMeshFaceCount, _navigationType), 0, Debug.DebugColor.White, 17592186044416UL);
                    Vec2 navMeshFaceCenterPosition = GetNavMeshFaceCenterPosition(i);
                    PathFaceRecord faceRecordAtIndex = GetFaceRecordAtIndex(i);
                    bool isPortUsed = false;
                    SCSettlementRecord closestSettlementToPosition = GetClosestSettlementToPosition(navMeshFaceCenterPosition, faceRecordAtIndex, GetExcludedFaceIds(), GetAllRegisteredSettlements(), GetRegionSwitchCostTo0(), GetRegionSwitchCostTo1(), float.MaxValue, out isPortUsed);
                    if (!Equals(closestSettlementToPosition, default(ISettlementDataHolder)))
                    {
                        SetClosestSettlementToFaceIndex(i, new NavigationCacheElement<SCSettlementRecord>(closestSettlementToPosition, isPortUsed));
                    }
                }
            }, 16);
            Debug.Print($"End SCGenerateClosestSettlementToFaceCache {DateTime.UtcNow.ToString()}\r\n");
        }

        protected void SetClosestSettlementToFaceIndex(int faceId, NavigationCacheElement<SCSettlementRecord> settlement)
        {
            Debug.Assert(!this._closestSettlementsToFaceIndices.ContainsKey(faceId), "Face key is already set, this is not possible", "C:\\BuildAgent\\work\\mb3\\Source\\Bannerlord\\TaleWorlds.CampaignSystem\\Map\\DistanceCache\\NavigationCache.cs", "SetClosestSettlementToFaceIndex", 289);
            this._closestSettlementsToFaceIndices.TryAdd(faceId, settlement);
        }

        public async Task SCGenerateSettlementToSettlementDistanceCache()
        {
            Debug.Print($"Start SCGenerateSettlementToSettlementDistanceCache() {DateTime.UtcNow.ToString()}\r\n");
            List<SCSettlementRecord> allRegisteredSettlements = this.GetAllRegisteredSettlements();
            Debug.Print($"allRegisteredSettlements.Count: {allRegisteredSettlements.Count}");
            await SCParallel.ForWhenAll(0, allRegisteredSettlements.Count, delegate (int startInclusive, int endExclusive)
            {
                for (int i = startInclusive; i < endExclusive; i++)
                {
                    // Debug.Print(string.Format("Settlement to settlement cache creation index {0},    total count: {1}     {2}", i, allRegisteredSettlements.Count, this._navigationType), 0, Debug.DebugColor.White, 17592186044416UL);
                    SCSettlementRecord settlement = allRegisteredSettlements[i];
                    for (int j = i + 1; j < allRegisteredSettlements.Count; j++)
                    {
                        SCSettlementRecord settlement2 = allRegisteredSettlements[j];
                        if (this._navigationType == MobileParty.NavigationType.Default)
                        {
                            this.SCAddClosestEntrancePairBase(settlement, false, settlement2, false);
                        }
                        else if (this._navigationType == MobileParty.NavigationType.Naval)
                        {
                            if (settlement.HasPort && settlement2.HasPort)
                            {
                                this.SCAddClosestEntrancePairBase(settlement, true, settlement2, true);
                            }
                        }
                        else if (this._navigationType == MobileParty.NavigationType.All)
                        {
                            this.SCAddClosestEntrancePairBase(settlement, false, settlement2, false);
                            if (settlement.HasPort && settlement2.HasPort)
                            {
                                this.SCAddClosestEntrancePairBase(settlement, true, settlement2, true);
                            }
                            if (settlement2.HasPort)
                            {
                                this.SCAddClosestEntrancePairBase(settlement, false, settlement2, true);
                            }
                            if (settlement.HasPort)
                            {
                                this.SCAddClosestEntrancePairBase(settlement, true, settlement2, false);
                            }
                        }
                    }
                }
            }, 16);

            Debug.Print($"End SCGenerateSettlementToSettlementDistanceCache() {DateTime.UtcNow.ToString()}\r\n");
        }

        protected async Task SCGenerateNeighborSettlementsCache()
        {
            Debug.Print($"Start SCGenerateNeighborSettlementsCache() {DateTime.UtcNow.ToString()}\r\n");
            this._fortificationNeighbors.Clear();

            List<SCSettlementRecord> updatedSettlementsForNeighborDetection = this.GetUpdatedSettlementsForNeighborDetection(this.GetAllRegisteredSettlements());
            await SCParallel.ForWhenAll(0, updatedSettlementsForNeighborDetection.Count, delegate (int startInclusive, int endExclusive)
            {
                for (int index = startInclusive; index < endExclusive; index++)
                {
                    // Debug.Print(string.Format("Neighbor cache progress for navigation {0}, current index: {1}  - total count: {2}", this._navigationType, index, updatedSettlementsForNeighborDetection.Count), 0, Debug.DebugColor.White, 17592186044416UL);
                    SCSettlementRecord settlement = updatedSettlementsForNeighborDetection[index];
                    if (settlement.IsFortification)
                    {
                        for (int j = index + 1; j < updatedSettlementsForNeighborDetection.Count; j++)
                        {
                            SCSettlementRecord settlement2 = updatedSettlementsForNeighborDetection[j];
                            if (settlement2.IsFortification && this.CheckBeingNeighbor(updatedSettlementsForNeighborDetection, settlement, settlement2))
                            {
                                this.AddNeighbor(settlement, settlement2);
                            }
                        }
                    }
                }
            }, 16);

            Debug.Print($"End SCGenerateNeighborSettlementsCache() {DateTime.UtcNow.ToString()}\r\n");
        }

        protected void FinalizeCacheInitialization()
        {
            if (this._fortificationNeighbors != null)
            {
                if (!this._fortificationNeighbors.AnyQ((KeyValuePair<SCSettlementRecord, MBReadOnlyList<SCSettlementRecord>> x) => x.Value.Count == 0))
                {
                    return;
                }
            }
            Debug.FailedAssert("There is settlement with zero neighbor in neighbor cache, this should not be happening, check here", "C:\\BuildAgent\\work\\mb3\\Source\\Bannerlord\\TaleWorlds.CampaignSystem\\Map\\DistanceCache\\NavigationCache.cs", "FinalizeCacheInitialization", 44);
            this.SCGenerateNeighborSettlementsCache();
        }

        protected List<SCSettlementRecord> GetUpdatedSettlementsForNeighborDetection(List<SCSettlementRecord> settlements)
        {
            if (this._navigationType == MobileParty.NavigationType.Naval)
            {
                return (from x in settlements
                        where x.IsFortification && x.HasPort
                        select x).ToList<SCSettlementRecord>();
            }
            return (from x in settlements
                    where x.IsFortification
                    select x).ToList<SCSettlementRecord>();
        }

        protected void AddNeighbor(SCSettlementRecord settlement1, SCSettlementRecord settlement2)
		{
			bool flag = false;
			foreach (KeyValuePair<SCSettlementRecord, MBReadOnlyList<SCSettlementRecord>> keyValuePair in this._fortificationNeighbors)
			{
                SCSettlementRecord key = keyValuePair.Key;
				if (!key.StringId.Equals(settlement1.StringId) || !keyValuePair.Value.Contains(settlement2))
				{
					key = keyValuePair.Key;
					if (!key.StringId.Equals(settlement2.StringId) || !keyValuePair.Value.Contains(settlement1))
					{
						continue;
					}
				}
				flag = true;
				break;
			}
			if (!flag)
			{
				MBReadOnlyList<SCSettlementRecord> mbreadOnlyList;
				if (!this._fortificationNeighbors.TryGetValue(settlement1, out mbreadOnlyList))
				{
					this._fortificationNeighbors.TryAdd(settlement1, new MBReadOnlyList<SCSettlementRecord>());
				}
				MBList<SCSettlementRecord> mblist;
				if (mbreadOnlyList != null)
				{
					mblist = new MBList<SCSettlementRecord>(mbreadOnlyList.Count + 1);
					mblist.AddRange(mbreadOnlyList);
				}
				else
				{
					mblist = new MBList<SCSettlementRecord>(1);
				}
				Debug.Assert(!mblist.Contains(settlement2), "Settlement already added to neighbor cache", "C:\\BuildAgent\\work\\mb3\\Source\\Bannerlord\\TaleWorlds.CampaignSystem\\Map\\DistanceCache\\NavigationCache.cs", "AddNeighbor", 260);
				mblist.Add(settlement2);
				this._fortificationNeighbors[settlement1] = mblist;
				MBReadOnlyList<SCSettlementRecord> mbreadOnlyList2;
				if (!this._fortificationNeighbors.TryGetValue(settlement2, out mbreadOnlyList2))
				{
					this._fortificationNeighbors.TryAdd(settlement2, new MBReadOnlyList<SCSettlementRecord>());
				}
				if (mbreadOnlyList2 != null)
				{
					mblist = new MBList<SCSettlementRecord>(mbreadOnlyList2.Count + 1);
					mblist.AddRange(mbreadOnlyList2);
				}
				else
				{
					mblist = new MBList<SCSettlementRecord>(1);
				}
				Debug.Assert(!mblist.Contains(settlement1), "Settlement already added to neighbor cache", "C:\\BuildAgent\\work\\mb3\\Source\\Bannerlord\\TaleWorlds.CampaignSystem\\Map\\DistanceCache\\NavigationCache.cs", "AddNeighbor", 280);
				mblist.Add(settlement1);
				this._fortificationNeighbors[settlement2] = mblist;
			}
		}

        private void SCAddClosestEntrancePairBase(SCSettlementRecord settlement1, bool isPort1, SCSettlementRecord settlement2, bool isPort2)
        {
            NavigationCacheElement<SCSettlementRecord> cacheElement = this.GetCacheElement(settlement1, isPort1);
            NavigationCacheElement<SCSettlementRecord> cacheElement2 = this.GetCacheElement(settlement2, isPort2);
            float num;
            float realDistanceAndLandRatioBetweenSettlements = this.GetRealDistanceAndLandRatioBetweenSettlements(cacheElement, cacheElement2, out num);
            float num2;
            float realDistanceAndLandRatioBetweenSettlements2 = this.GetRealDistanceAndLandRatioBetweenSettlements(cacheElement2, cacheElement, out num2);
            float num3 = (realDistanceAndLandRatioBetweenSettlements + realDistanceAndLandRatioBetweenSettlements2) * 0.5f;
            if (num3 > 0f)
            {
                float landRatio = 1f;
                if (this._navigationType == MobileParty.NavigationType.Naval)
                {
                    landRatio = 0f;
                }
                else if (this._navigationType == MobileParty.NavigationType.All)
                {
                    landRatio = num;
                }
                bool flag;
                NavigationCacheElement<SCSettlementRecord>.Sort(ref cacheElement, ref cacheElement2, out flag);
                if (flag)
                {
                    landRatio = num2;
                }
                this.SetSettlementToSettlementDistanceWithLandRatio(cacheElement, cacheElement2, num3, landRatio);
            }
        }

        protected void SetSettlementToSettlementDistanceWithLandRatio(NavigationCacheElement<SCSettlementRecord> settlement1, NavigationCacheElement<SCSettlementRecord> settlement2, float distance, float landRatio)
        {
            bool flag;
            NavigationCacheElement<SCSettlementRecord>.Sort(ref settlement1, ref settlement2, out flag);
            Dictionary<NavigationCacheElement<SCSettlementRecord>, ValueTuple<float, float>> dictionary;
            if (!this._settlementToSettlementDistanceWithLandRatio.TryGetValue(settlement1, out dictionary))
            {
                dictionary = new Dictionary<NavigationCacheElement<SCSettlementRecord>, ValueTuple<float, float>>();
                this._settlementToSettlementDistanceWithLandRatio.TryAdd(settlement1, dictionary);
            }
            ValueTuple<float, float> valueTuple;
            if (dictionary.TryGetValue(settlement2, out valueTuple))
            {
                Debug.FailedAssert("Element already exists", "C:\\BuildAgent\\work\\mb3\\Source\\Bannerlord\\TaleWorlds.CampaignSystem\\Map\\DistanceCache\\NavigationCache.cs", "SetSettlementToSettlementDistanceWithLandRatio", 215);
            }
            dictionary.Add(settlement2, new ValueTuple<float, float>(distance, landRatio));
            if (distance < 100000000f && distance > this.MaximumDistanceBetweenTwoConnectedSettlements)
            {
                this.MaximumDistanceBetweenTwoConnectedSettlements = distance;
            }
        }

        protected NavigationCacheElement<SCSettlementRecord> GetCacheElement(SCSettlementRecord settlement, bool isPortUsed)
        {
            return new NavigationCacheElement<SCSettlementRecord>(settlement, isPortUsed);
        }

        protected SCSettlementRecord GetCacheElement(string settlementId)
        {
            return this._settlementRecords.Single((SCSettlementRecord x) => x.SettlementId == settlementId);
        }

        public void GetSceneXmlCrcValues(out uint sceneXmlCrc, out uint sceneNavigationMeshCrc)
        {
            sceneXmlCrc = this.Scene.GetSceneXMLCRC();
            sceneNavigationMeshCrc = this.Scene.GetNavigationMeshCRC();
        }

        protected int GetNavMeshFaceCount()
        {
            return this.Scene.GetNavMeshFaceCount();
        }

        protected Vec2 GetNavMeshFaceCenterPosition(int faceIndex)
        {
            Vec3 zero = Vec3.Zero;
            this.Scene.GetNavMeshCenterPosition(faceIndex, ref zero);
            return zero.AsVec2;
        }

        protected PathFaceRecord GetFaceRecordAtIndex(int faceIndex)
        {
            return this.Scene.GetNavMeshPathFaceRecord(faceIndex);
        }

        protected int[] GetExcludedFaceIds()
        {
            return this._excludedFaceIds;
        }

        protected int GetRegionSwitchCostTo0()
        {
            return this._regionSwitchCostTo0;
        }

        protected int GetRegionSwitchCostTo1()
        {
            return this._regionSwitchCostTo1;
        }

        protected IEnumerable<SCSettlementRecord> GetClosestSettlementsToPositionInCache(Vec2 checkPosition, List<SCSettlementRecord> settlements)
        {
            if (this._navigationType == MobileParty.NavigationType.Naval)
            {
                return from x in settlements
                       where x.HasPort
                       orderby checkPosition.DistanceSquared(x.PortPosition)
                       select x;
            }
            if (this._navigationType == MobileParty.NavigationType.Default)
            {
                return from x in settlements
                       orderby checkPosition.DistanceSquared(x.GatePosition)
                       select x;
            }
            return settlements.OrderBy(delegate (SCSettlementRecord x)
            {
                if (!x.HasPort)
                {
                    return checkPosition.DistanceSquared(x.GatePosition);
                }
                return TaleWorlds.Library.MathF.Min(checkPosition.DistanceSquared(x.GatePosition), checkPosition.DistanceSquared(x.PortPosition));
            });
        }

        protected float GetRealPathDistanceFromPositionToSettlement(Vec2 checkPosition, PathFaceRecord currentFaceRecord, float maxDistanceToLookForPathDetection, SCSettlementRecord currentSettlementToLook, out bool isPort)
        {
            float result = float.MaxValue;
            isPort = false;
            PathFaceRecord nullFaceRecord = PathFaceRecord.NullFaceRecord;
            switch (this._navigationType)
            {
                case MobileParty.NavigationType.Default:
                    {
                        this.Scene.GetNavMeshFaceIndex(ref nullFaceRecord, currentSettlementToLook.GatePosition, true, false, true);
                        float num;
                        if (this.Scene.GetPathDistanceBetweenAIFaces(currentFaceRecord.FaceIndex, nullFaceRecord.FaceIndex, checkPosition, currentSettlementToLook.GatePosition, 0.3f, maxDistanceToLookForPathDetection, out num, this._excludedFaceIds, this._regionSwitchCostTo0, this._regionSwitchCostTo1))
                        {
                            result = num;
                        }
                        break;
                    }
                case MobileParty.NavigationType.Naval:
                    {
                        this.Scene.GetNavMeshFaceIndex(ref nullFaceRecord, currentSettlementToLook.PortPosition, false, false, true);
                        float num2;
                        if (this.Scene.GetPathDistanceBetweenAIFaces(currentFaceRecord.FaceIndex, nullFaceRecord.FaceIndex, checkPosition, currentSettlementToLook.PortPosition, 0.3f, maxDistanceToLookForPathDetection, out num2, this._excludedFaceIds, this._regionSwitchCostTo0, this._regionSwitchCostTo1))
                        {
                            result = num2;
                            isPort = true;
                        }
                        break;
                    }
                case MobileParty.NavigationType.All:
                    {
                        this.Scene.GetNavMeshFaceIndex(ref nullFaceRecord, currentSettlementToLook.GatePosition, true, false, true);
                        float num3;
                        if (this.Scene.GetPathDistanceBetweenAIFaces(currentFaceRecord.FaceIndex, nullFaceRecord.FaceIndex, checkPosition, currentSettlementToLook.GatePosition, 0.3f, maxDistanceToLookForPathDetection, out num3, this._excludedFaceIds, this._regionSwitchCostTo0, this._regionSwitchCostTo1))
                        {
                            result = num3;
                        }
                        if (currentSettlementToLook.HasPort)
                        {
                            this.Scene.GetNavMeshFaceIndex(ref nullFaceRecord, currentSettlementToLook.PortPosition, false, false, true);
                            float num4;
                            if (this.Scene.GetPathDistanceBetweenAIFaces(currentFaceRecord.FaceIndex, nullFaceRecord.FaceIndex, checkPosition, currentSettlementToLook.PortPosition, 0.3f, maxDistanceToLookForPathDetection, out num4, this._excludedFaceIds, this._regionSwitchCostTo0, this._regionSwitchCostTo1) && num4 < num3)
                            {
                                result = num4;
                                isPort = true;
                            }
                        }
                        break;
                    }
            }
            return result;
        }

        public void Serialize(string path)
        {
            System.IO.BinaryWriter binaryWriter = new System.IO.BinaryWriter(File.Open(path, FileMode.Create));
            uint value;
            uint value2;
            this.GetSceneXmlCrcValues(out value, out value2);
            binaryWriter.Write(value);
            binaryWriter.Write(value2);
            binaryWriter.Write(this._settlementToSettlementDistanceWithLandRatio.Count);
            foreach (KeyValuePair<NavigationCacheElement<SCSettlementRecord>, Dictionary<NavigationCacheElement<SCSettlementRecord>, ValueTuple<float, float>>> keyValuePair in this._settlementToSettlementDistanceWithLandRatio)
            {
                binaryWriter.Write(keyValuePair.Key.StringId);
                binaryWriter.Write(keyValuePair.Key.IsPortUsed);
                binaryWriter.Write(keyValuePair.Value.Count);
                foreach (KeyValuePair<NavigationCacheElement<SCSettlementRecord>, ValueTuple<float, float>> keyValuePair2 in keyValuePair.Value)
                {
                    binaryWriter.Write(keyValuePair2.Key.StringId);
                    binaryWriter.Write(keyValuePair2.Key.IsPortUsed);
                    binaryWriter.Write(keyValuePair2.Value.Item1);
                    if (this._navigationType == MobileParty.NavigationType.All)
                    {
                        binaryWriter.Write(keyValuePair2.Value.Item2);
                    }
                }
            }
            binaryWriter.Write(this._fortificationNeighbors.SumQ((KeyValuePair<SCSettlementRecord, MBReadOnlyList<SCSettlementRecord>> x) => x.Value.Count));
            foreach (KeyValuePair<SCSettlementRecord, MBReadOnlyList<SCSettlementRecord>> keyValuePair3 in this._fortificationNeighbors)
            {
                SCSettlementRecord key = keyValuePair3.Key;
                string stringId = key.StringId;
                foreach (SCSettlementRecord t in keyValuePair3.Value)
                {
                    binaryWriter.Write(stringId);
                    binaryWriter.Write(t.StringId);
                }
            }
            binaryWriter.Write(this._closestSettlementsToFaceIndices.Count);
            foreach (KeyValuePair<int, NavigationCacheElement<SCSettlementRecord>> keyValuePair4 in this._closestSettlementsToFaceIndices)
            {
                binaryWriter.Write(keyValuePair4.Key);
                binaryWriter.Write(keyValuePair4.Value.StringId);
                binaryWriter.Write(keyValuePair4.Value.IsPortUsed);
            }
            binaryWriter.Close();
        }

        public void Deserialize(string path)
        {
            Debug.Print("Reading SettlementsDistanceCacheFilePath: " + path, 0, Debug.DebugColor.White, 17592186044416UL);
            System.IO.BinaryReader binaryReader = new System.IO.BinaryReader(File.Open(path, FileMode.Open, FileAccess.Read));
            uint num = binaryReader.ReadUInt32();
            uint num2 = binaryReader.ReadUInt32();
            uint sceneXmlCrc = Campaign.Current.MapSceneWrapper.GetSceneXmlCrc();
            uint sceneNavigationMeshCrc = Campaign.Current.MapSceneWrapper.GetSceneNavigationMeshCrc();
            Debug.Assert(num == sceneXmlCrc, "SceneXmlCrc cache created for different map version, cache should be created after any map change", "C:\\BuildAgent\\work\\mb3\\Source\\Bannerlord\\TaleWorlds.CampaignSystem\\Map\\DistanceCache\\NavigationCache.cs", "Deserialize", 705);
            Debug.Assert(num2 == sceneNavigationMeshCrc, "SceneNavigationMeshXmlCrc cache created for different map version, cache should be created after any map change", "C:\\BuildAgent\\work\\mb3\\Source\\Bannerlord\\TaleWorlds.CampaignSystem\\Map\\DistanceCache\\NavigationCache.cs", "Deserialize", 706);
            int num3 = binaryReader.ReadInt32();
            // Keep thread count at half for now
            this._settlementToSettlementDistanceWithLandRatio = new ConcurrentDictionary<NavigationCacheElement<SCSettlementRecord>, Dictionary<NavigationCacheElement<SCSettlementRecord>, ValueTuple<float, float>>>(Environment.ProcessorCount, num3);
            for (int i = 0; i < num3; i++)
            {
                SCSettlementRecord cacheElement = this.GetCacheElement(binaryReader.ReadString());
                bool isPortUsed = binaryReader.ReadBoolean();
                NavigationCacheElement<SCSettlementRecord> cacheElement2 = this.GetCacheElement(cacheElement, isPortUsed);
                int num4 = binaryReader.ReadInt32();
                this._settlementToSettlementDistanceWithLandRatio.TryAdd(cacheElement2, new Dictionary<NavigationCacheElement<SCSettlementRecord>, ValueTuple<float, float>>(num4));
                for (int j = 0; j < num4; j++)
                {
                    SCSettlementRecord cacheElement3 = this.GetCacheElement(binaryReader.ReadString());
                    bool isPortUsed2 = binaryReader.ReadBoolean();
                    NavigationCacheElement<SCSettlementRecord> cacheElement4 = this.GetCacheElement(cacheElement3, isPortUsed2);
                    bool flag;
                    NavigationCacheElement<SCSettlementRecord>.Sort(ref cacheElement2, ref cacheElement4, out flag);
                    Debug.Assert(!flag, "order changed between read and write", "C:\\BuildAgent\\work\\mb3\\Source\\Bannerlord\\TaleWorlds.CampaignSystem\\Map\\DistanceCache\\NavigationCache.cs", "Deserialize", 729);
                    float distance = binaryReader.ReadSingle();
                    float landRatio = (this._navigationType == MobileParty.NavigationType.Naval) ? 0f : 1f;
                    if (this._navigationType == MobileParty.NavigationType.All)
                    {
                        landRatio = binaryReader.ReadSingle();
                    }
                    this.SetSettlementToSettlementDistanceWithLandRatio(cacheElement2, cacheElement4, distance, landRatio);
                }
            }
            int num5 = binaryReader.ReadInt32();
            this._fortificationNeighbors = new ConcurrentDictionary<SCSettlementRecord, MBReadOnlyList<SCSettlementRecord>>(Environment.ProcessorCount, num5);
            for (int k = 0; k < num5; k++)
            {
                SCSettlementRecord cacheElement5 = this.GetCacheElement(binaryReader.ReadString());
                SCSettlementRecord cacheElement6 = this.GetCacheElement(binaryReader.ReadString());
                this.AddNeighbor(cacheElement5, cacheElement6);
            }
            int num6 = binaryReader.ReadInt32();
            this._closestSettlementsToFaceIndices = new ConcurrentDictionary<int, NavigationCacheElement<SCSettlementRecord>>(Environment.ProcessorCount, num6);
            for (int l = 0; l < num6; l++)
            {
                int faceId = binaryReader.ReadInt32();
                SCSettlementRecord cacheElement7 = this.GetCacheElement(binaryReader.ReadString());
                bool isPortUsed3 = binaryReader.ReadBoolean();
                NavigationCacheElement<SCSettlementRecord> cacheElement8 = this.GetCacheElement(cacheElement7, isPortUsed3);
                this.SetClosestSettlementToFaceIndex(faceId, cacheElement8);
            }
            binaryReader.Close();
        }

        protected float GetRealDistanceAndLandRatioBetweenSettlements(NavigationCacheElement<SCSettlementRecord> settlement1, NavigationCacheElement<SCSettlementRecord> settlement2, out float landRatio)
        {
            Vec2 vec = settlement1.IsPortUsed ? settlement1.PortPosition.ToVec2() : settlement1.GatePosition.ToVec2();
            Vec2 vec2 = settlement2.IsPortUsed ? settlement2.PortPosition.ToVec2() : settlement2.GatePosition.ToVec2();
            PathFaceRecord nullFaceRecord = PathFaceRecord.NullFaceRecord;
            this.Scene.GetNavMeshFaceIndex(ref nullFaceRecord, vec, !settlement1.IsPortUsed, false, true);
            PathFaceRecord nullFaceRecord2 = PathFaceRecord.NullFaceRecord;
            this.Scene.GetNavMeshFaceIndex(ref nullFaceRecord2, vec2, !settlement2.IsPortUsed, false, true);
            landRatio = 1f;
            if (this._navigationType == MobileParty.NavigationType.Naval)
            {
                landRatio = 0f;
            }
            else if (this._navigationType == MobileParty.NavigationType.All)
            {
                NavigationPath path = new NavigationPath();
                this.Scene.GetPathBetweenAIFaces(nullFaceRecord.FaceIndex, nullFaceRecord2.FaceIndex, vec, vec2, 0.3f, path, this._excludedFaceIds, 1f, this._regionSwitchCostTo0, this._regionSwitchCostTo1);
                landRatio = this.GetLandRatioOfPath(path, vec);
            }
            float result;
            this.Scene.GetPathDistanceBetweenAIFaces(nullFaceRecord.FaceIndex, nullFaceRecord2.FaceIndex, vec, vec2, 0.3f, float.PositiveInfinity, out result, this._excludedFaceIds, this._regionSwitchCostTo0, this._regionSwitchCostTo1);
            return result;
        }

        protected float GetLandRatioOfPath(NavigationPath path, Vec2 startPosition)
        {
            float num = 0f;
            float num2 = 0f;
            List<Vec2> list = new List<Vec2>(path.PathPoints);
            list.Insert(0, startPosition);
            for (int i = 0; i < list.Count - 1; i++)
            {
                Vec2 vec = list[i];
                Vec2 v = list[i + 1];
                if (v == Vec2.Zero)
                {
                    break;
                }
                Vec2 v2 = v - vec;
                float num3 = v2.Length / 0.5f;
                v2.Normalize();
                int num4 = 0;
                while ((float)num4 < num3 - 1f)
                {
                    Vec2 position = vec + v2 * (float)num4 * 0.5f;
                    Vec2 vec2 = vec + v2 * (float)(num4 + 1) * 0.5f;
                    bool flag;
                    this.GetFaceRecordForPoint(position, out flag);
                    bool flag2;
                    this.GetFaceRecordForPoint(vec2, out flag2);
                    float num5 = position.Distance(vec2);
                    if (flag2 && flag)
                    {
                        num += num5;
                    }
                    else if (flag2 != flag)
                    {
                        num += num5 / 2f;
                    }
                    num2 += num5;
                    num4++;
                }
            }
            if (list.Count != 1)
            {
                return MBMath.ClampFloat(num / num2, 0f, 1f);
            }
            bool flag3;
            this.GetFaceRecordForPoint(list[0], out flag3);
            if (flag3)
            {
                return 1f;
            }
            return 0f;
        }

        protected void GetFaceRecordForPoint(Vec2 position, out bool isOnRegion1)
        {
            isOnRegion1 = true;
            PathFaceRecord nullFaceRecord = PathFaceRecord.NullFaceRecord;
            this.Scene.GetNavMeshFaceIndex(ref nullFaceRecord, position, isOnRegion1, false, true);
            if (!nullFaceRecord.IsValid())
            {
                isOnRegion1 = false;
                this.Scene.GetNavMeshFaceIndex(ref nullFaceRecord, position, isOnRegion1, false, true);
            }
            if (!nullFaceRecord.IsValid())
            {
                Debug.Print(string.Format("{0} has no region data.", position), 0, Debug.DebugColor.Red, 17592186044416UL);
            }
        }

        private void CheckNeighbourAux(List<SCSettlementRecord> settlementsToConsider, SCSettlementRecord settlement1, SCSettlementRecord settlement2, bool useGate1, bool useGate2, ref float distance, ref bool isNeighbour)
        {
            float num;
            bool flag = this.CheckBeingNeighbor(settlementsToConsider, settlement1, settlement2, useGate1, useGate2, out num);
            if (num < distance)
            {
                distance = num;
                isNeighbour = flag;
            }
        }

        protected bool CheckBeingNeighbor(List<SCSettlementRecord> settlementsToConsider, SCSettlementRecord settlement1, SCSettlementRecord settlement2)
        {
            float maxValue = float.MaxValue;
            bool result = false;
            if (this._navigationType == MobileParty.NavigationType.Default || this._navigationType == MobileParty.NavigationType.All)
            {
                this.CheckNeighbourAux(settlementsToConsider, settlement1, settlement2, true, true, ref maxValue, ref result);
                this.CheckNeighbourAux(settlementsToConsider, settlement2, settlement1, true, true, ref maxValue, ref result);
            }
            if (this._navigationType == MobileParty.NavigationType.Naval || this._navigationType == MobileParty.NavigationType.All)
            {
                bool hasPort = settlement1.HasPort;
                bool hasPort2 = settlement2.HasPort;
                if (hasPort)
                {
                    this.CheckNeighbourAux(settlementsToConsider, settlement1, settlement2, false, true, ref maxValue, ref result);
                    this.CheckNeighbourAux(settlementsToConsider, settlement2, settlement1, true, false, ref maxValue, ref result);
                }
                if (hasPort2)
                {
                    this.CheckNeighbourAux(settlementsToConsider, settlement1, settlement2, true, false, ref maxValue, ref result);
                    this.CheckNeighbourAux(settlementsToConsider, settlement2, settlement1, false, true, ref maxValue, ref result);
                }
                if (hasPort2 && hasPort)
                {
                    this.CheckNeighbourAux(settlementsToConsider, settlement1, settlement2, false, false, ref maxValue, ref result);
                    this.CheckNeighbourAux(settlementsToConsider, settlement2, settlement1, false, false, ref maxValue, ref result);
                }
            }
            return result;
        }

        protected bool CheckBeingNeighbor(List<SCSettlementRecord> settlementsToConsider, SCSettlementRecord settlement1, SCSettlementRecord settlement2, bool useGate1, bool useGate2, out float distance)
        {
            Vec2 vec = useGate1 ? settlement1.GatePosition : settlement1.PortPosition;
            Vec2 vec2 = useGate2 ? settlement2.GatePosition : settlement2.PortPosition;
            PathFaceRecord nullFaceRecord = PathFaceRecord.NullFaceRecord;
            this.Scene.GetNavMeshFaceIndex(ref nullFaceRecord, vec, useGate1, false, true);
            PathFaceRecord nullFaceRecord2 = PathFaceRecord.NullFaceRecord;
            this.Scene.GetNavMeshFaceIndex(ref nullFaceRecord2, vec2, useGate2, false, true);
            if (!nullFaceRecord.IsValid() || !nullFaceRecord2.IsValid())
            {
                Debug.FailedAssert("Settlement navFace index should not be -1, check here", "C:\\BuildAgent\\work\\mb3\\Source\\Bannerlord\\SandBox.View\\Map\\SCSettlementPositionScript.cs", "CheckBeingNeighbor", 392);
            }
            NavigationPath navigationPath = new NavigationPath();
            float num = ((float)(this._regionSwitchCostTo0 + this._regionSwitchCostTo1) > 0f) ? 2f : 0f;
            if (num > 0f)
            {
                this.Scene.GetPathBetweenAIFaces(nullFaceRecord.FaceIndex, nullFaceRecord2.FaceIndex, vec, vec2, 0.3f, navigationPath, this._excludedFaceIds, num, this._regionSwitchCostTo0, this._regionSwitchCostTo1);
            }
            else
            {
                this.Scene.GetPathBetweenAIFaces(nullFaceRecord.FaceIndex, nullFaceRecord2.FaceIndex, vec, vec2, 0.3f, navigationPath, this._excludedFaceIds, 0f);
            }
            bool flag = navigationPath.Size > 0 || nullFaceRecord.FaceIndex == nullFaceRecord2.FaceIndex;
            bool flag2 = useGate1;
            if (!this.Scene.GetPathDistanceBetweenAIFaces(nullFaceRecord.FaceIndex, nullFaceRecord2.FaceIndex, vec, vec2, 0.3f, 1784684f, out distance, this.GetExcludedFaceIds(), this._regionSwitchCostTo0, this._regionSwitchCostTo1))
            {
                distance = 1784684f;
            }
            int num2 = 0;
            while (num2 < navigationPath.Size && flag)
            {
                Vec2 v = navigationPath[num2] - ((num2 == 0) ? vec : navigationPath[num2 - 1]);
                float num3 = v.Length / 1f;
                v.Normalize();
                int num4 = 0;
                while ((float)num4 < num3)
                {
                    Vec2 vec3 = ((num2 == 0) ? vec : navigationPath[num2 - 1]) + v * 1f * (float)num4;
                    if (vec3 != vec && vec3 != vec2)
                    {
                        PathFaceRecord nullFaceRecord3 = PathFaceRecord.NullFaceRecord;
                        this.Scene.GetNavMeshFaceIndex(ref nullFaceRecord3, vec3, flag2, false, true);
                        if (nullFaceRecord3.FaceIndex == -1)
                        {
                            flag2 = !flag2;
                            this.Scene.GetNavMeshFaceIndex(ref nullFaceRecord3, vec3, flag2, false, true);
                        }
                        bool flag3;
                        float realPathDistanceFromPositionToSettlement = this.GetRealPathDistanceFromPositionToSettlement(vec3, nullFaceRecord3, distance, settlement1, out flag3);
                        float realPathDistanceFromPositionToSettlement2 = this.GetRealPathDistanceFromPositionToSettlement(vec3, nullFaceRecord3, distance, settlement2, out flag3);
                        float num5 = (realPathDistanceFromPositionToSettlement < realPathDistanceFromPositionToSettlement2) ? realPathDistanceFromPositionToSettlement : realPathDistanceFromPositionToSettlement2;
                        if (nullFaceRecord3.FaceIndex != -1)
                        {
                            SCSettlementRecord closestSettlementToPosition = this.GetClosestSettlementToPosition(vec3, nullFaceRecord3, this._excludedFaceIds, settlementsToConsider, this._regionSwitchCostTo0, this._regionSwitchCostTo1, num5 * 0.8f, out flag3);
                            if (closestSettlementToPosition != null && closestSettlementToPosition != settlement1 && closestSettlementToPosition != settlement2)
                            {
                                flag = false;
                                break;
                            }
                        }
                    }
                    num4++;
                }
                num2++;
            }
            return flag;
        }

        protected SCSettlementRecord GetClosestSettlementToPosition(Vec2 checkPosition, PathFaceRecord currentFaceRecord, int[] excludedFaceIds, List<SCSettlementRecord> settlementRecords, int regionSwitchCostTo0, int regionSwitchCostTo1, float minPathScoreEverFound, out bool isPort)
        {
            isPort = false;
            SCSettlementRecord t = default(SCSettlementRecord);
            foreach (SCSettlementRecord t2 in this.GetClosestSettlementsToPositionInCache(checkPosition, settlementRecords))
            {
                bool flag;
                float realPathDistanceFromPositionToSettlement = this.GetRealPathDistanceFromPositionToSettlement(checkPosition, currentFaceRecord, minPathScoreEverFound * 2f, t2, out flag);
                if (realPathDistanceFromPositionToSettlement < minPathScoreEverFound)
                {
                    minPathScoreEverFound = realPathDistanceFromPositionToSettlement;
                    t = t2;
                    isPort = flag;
                }
            }
            Debug.Assert((this._navigationType != MobileParty.NavigationType.Naval | isPort) || object.Equals(t, default(SCSettlementRecord)), "isPort should be true for naval navigation type", "C:\\BuildAgent\\work\\mb3\\Source\\Bannerlord\\TaleWorlds.CampaignSystem\\Map\\DistanceCache\\NavigationCache.cs", "GetClosestSettlementToPosition", 605);
            return t;
        }

        protected  List<SCSettlementRecord> GetAllRegisteredSettlements()
        {
            return this._settlementRecords;
        }

        private readonly Scene Scene;

        private readonly List<SCSettlementRecord> _settlementRecords;

        private readonly int[] _excludedFaceIds;

        private readonly int _regionSwitchCostTo0;

        private readonly int _regionSwitchCostTo1;
    }
}
