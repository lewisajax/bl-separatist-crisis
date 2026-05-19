using HarmonyLib;
using SandBox;
using SandBox.View.Map;
using SandBox.View.Map.Managers;
using SandBox.View.Map.Visuals;
using SeparatistCrisis.Map.Views;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Map;
using TaleWorlds.CampaignSystem.Party;
using TaleWorlds.CampaignSystem.Settlements;
using TaleWorlds.CampaignSystem.Siege;
using TaleWorlds.Core;
using TaleWorlds.Engine;
using TaleWorlds.Library;
using TaleWorlds.ScreenSystem;

namespace SeparatistCrisis.Map
{
    // - We inherit from SettlementVisualManager
    // - We use a transpiler on MapScreen.InitializeVisuals to swap out SettlementVisualManager for this
    // - SettlementVisualManager.Current returns the entity component which will be this

    public class SCSettlementVisualManager: SettlementVisualManager
    {
        protected static FieldInfo SettlementVisualsField = AccessTools.Field(typeof(SettlementVisualManager), "_settlementVisuals");
        protected static MethodInfo FrameAndVisualOfEnginesGetter = AccessTools.PropertyGetter(typeof(MapScreen), "FrameAndVisualOfEngines");

        private bool _isNewDecalScaleImplementationEnabled; // Doesn't seem to be assigned a true value from anywhere
        private bool _playerSiegeMachineSlotMeshesAdded;
        private MapView _mapSiegeOverlayView;
        private GameEntity[] _defenderMachinesCircleEntities;
        private GameEntity[] _attackerRamMachinesCircleEntities;
        private GameEntity[] _attackerTowerMachinesCircleEntities;
        private GameEntity[] _attackerRangedMachinesCircleEntities;
        private float _timeSinceCreation;
        private UIntPtr _hoveredSiegeEntityID;

        // private readonly Dictionary<PartyBase, SettlementVisual> _settlementVisuals = new Dictionary<PartyBase, SettlementVisual>();
        private readonly List<SCSettlementVisual> _visualsFlattened = new List<SCSettlementVisual>();
        private int _dirtyPartyVisualCount;
        private SCSettlementVisual[] _dirtyPartiesList = new SCSettlementVisual[2500];

        protected List<SCSettlementVisual> VisualsFlattened => this._visualsFlattened;
        protected int DirtyPartyVisualCount => _dirtyPartyVisualCount;
        protected SCSettlementVisual[] DirtyPartiesList => _dirtyPartiesList;

        public GameEntity[] DefenderMachinesCircleEntities => this._defenderMachinesCircleEntities;
        public GameEntity[] AttackerRamMachinesCircleEntities => this._attackerRamMachinesCircleEntities;
        public GameEntity[] AttackerTowerMachinesCircleEntities => this._attackerTowerMachinesCircleEntities;
        public GameEntity[] AttackerRangedMachinesCircleEntities => this._attackerRangedMachinesCircleEntities;

        public float TimeSinceCreation
        {
            get => this._timeSinceCreation;
            protected set => this._timeSinceCreation = value;
        }

        public bool PlayerSiegeMachineSlotMeshesAdded => this._playerSiegeMachineSlotMeshesAdded;

        protected MapView MapSiegeOverlayView => this._mapSiegeOverlayView;

        
        // Try changing SettVis.OnTick to SCOnTick
        public override void OnTick(float realDt, float dt)
        {
            this._dirtyPartyVisualCount = -1;
            TWParallel.For(0, this._visualsFlattened.Count, delegate (int startInclusive, int endExclusive)
            {
                for (int j = startInclusive; j < endExclusive; j++)
                {
                    this._visualsFlattened[j].Tick(dt, ref this._dirtyPartyVisualCount, ref this._dirtyPartiesList);
                }
            }, 16);
            for (int i = 0; i < this._dirtyPartyVisualCount + 1; i++)
            {
                this._dirtyPartiesList[i].ValidateIsDirty();
            }
        }

        protected override void OnInitialize()
        {
            // _settlementVisuals has a public getter that is used in a few other classes
            Dictionary<PartyBase, SettlementVisual> settVisuals = (Dictionary<PartyBase, SettlementVisual>)SettlementVisualsField.GetValue(this);
            List<SCSettlementVisual> visFlattened = this._visualsFlattened;

            foreach (Settlement settlement in Settlement.All)
            {
                SCSettlementVisual settlementVisual = new SCSettlementVisual(settlement.Party);
                settlementVisual.OnStartup();
                settVisuals.Add(settlement.Party, settlementVisual);
                visFlattened.Add(settlementVisual);
            }
        }

        public override void OnFrameTick(float dt)
        {
            this.RefreshMapSiegeOverlayRequired();
            if (PlayerSiege.PlayerSiegeEvent != null && this._playerSiegeMachineSlotMeshesAdded)
            {
                this.TickSiegeMachineCircles();
            }
            if (GameStateManager.Current.ActiveStateDisabledByUser)
            {
                this.HandleSiegeEngineHoverEnd();
            }
            this._timeSinceCreation += dt;
        }

        public override bool OnVisualIntersected(Ray mouseRay, UIntPtr[] intersectedEntityIDs, Intersection[] intersectionInfos, int entityCount, Vec3 worldMouseNear, Vec3 worldMouseFar, Vec3 terrainIntersectionPoint, ref MapEntityVisual hoveredVisual, ref MapEntityVisual selectedVisual)
        {
            bool flag = false;
            for (int i = entityCount - 1; i >= 0; i--)
            {
                UIntPtr uintPtr = intersectedEntityIDs[i];
                if (uintPtr != UIntPtr.Zero)
                {
                    MapEntityVisual mapEntityVisual;
                    if (MapScreen.VisualsOfEntities.TryGetValue(uintPtr, out mapEntityVisual) && mapEntityVisual is SettlementVisual && mapEntityVisual.IsVisibleOrFadingOut())
                    {
                        if (hoveredVisual == null)
                        {
                            hoveredVisual = mapEntityVisual;
                        }
                        selectedVisual = mapEntityVisual;
                    }


                    if (PlayerSiege.PlayerSiegeEvent != null && ScreenManager.FirstHitLayer == MapScreen.Instance.SceneLayer)
                    {
                        Dictionary<UIntPtr, Tuple<MatrixFrame, SettlementVisual>> engFramesAndVisuals = (Dictionary<UIntPtr, Tuple<MatrixFrame, SettlementVisual>>)FrameAndVisualOfEnginesGetter.Invoke(this, new object[] { });
                        if (engFramesAndVisuals.ContainsKey(uintPtr))
                        {
                            flag = true;
                            this.HandleSiegeEngineHover(uintPtr);
                        }
                    }
                }
            }
            if (!flag)
            {
                this.HandleSiegeEngineHoverEnd();
            }
            return selectedVisual != null;
        }

        public override bool OnMouseClick(MapEntityVisual visualOfSelectedEntity, Vec3 intersectionPoint, PathFaceRecord mouseOverFaceIndex, bool isDoubleClick)
        {
            bool result = false;
            if (MapScreen.Instance.MapState.AtMenu && this._hoveredSiegeEntityID != UIntPtr.Zero)
            {
                Dictionary<UIntPtr, Tuple<MatrixFrame, SettlementVisual>> engFramesAndVisuals = (Dictionary<UIntPtr, Tuple<MatrixFrame, SettlementVisual>>)FrameAndVisualOfEnginesGetter.Invoke(this, new object[] { });

                Tuple<MatrixFrame, SettlementVisual> tuple = engFramesAndVisuals[this._hoveredSiegeEntityID];
                MapScreen.Instance.OnSiegeEngineFrameClick(tuple.Item1);
                result = true;
            }
            return result;
        }

        protected override void OnFinalize()
        {
            base.OnFinalize();
        }

        // We moved the inner block to OnIntilizadadasdad
        //private void AddNewPartyVisualForParty(PartyBase partyBase)
        //{
        //    SCSettlementVisual settlementVisual = new SCSettlementVisual(partyBase);
        //    settlementVisual.OnStartup();
        //    this._settlementVisuals.Add(partyBase, settlementVisual);
        //    this._visualsFlattened.Add(settlementVisual);
        //}

        protected void TickSiegeMachineCircles()
        {
            SiegeEvent playerSiegeEvent = PlayerSiege.PlayerSiegeEvent;
            bool isPlayerLeader = playerSiegeEvent != null && playerSiegeEvent.IsPlayerSiegeEvent && Campaign.Current.Models.EncounterModel.GetLeaderOfSiegeEvent(playerSiegeEvent, PlayerSiege.PlayerSide) == Hero.MainHero;
            Settlement besiegedSettlement = playerSiegeEvent.BesiegedSettlement;
            SCSettlementVisual settlementVisual = (SCSettlementVisual)this.GetSettlementVisual(besiegedSettlement);
            Tuple<MatrixFrame, SettlementVisual> tuple = null;
            if (this._hoveredSiegeEntityID != UIntPtr.Zero)
            {
                Dictionary<UIntPtr, Tuple<MatrixFrame, SettlementVisual>> engFramesAndVisuals = (Dictionary<UIntPtr, Tuple<MatrixFrame, SettlementVisual>>)FrameAndVisualOfEnginesGetter.Invoke(this, new object[] { });
                tuple = engFramesAndVisuals[this._hoveredSiegeEntityID];
            }
            for (int i = 0; i < settlementVisual.SCGetDefenderRangedSiegeEngineFrames().Length; i++)
            {
                bool isEmpty = playerSiegeEvent.GetSiegeEventSide(BattleSideEnum.Defender).SiegeEngines.DeployedRangedSiegeEngines[i] == null;
                bool isEnemy = PlayerSiege.PlayerSide > 0;
                string desiredMaterialName = this.GetDesiredMaterialName(true, false, false);

                // Set it to an invisible material
                // string desiredMaterialName = "editor_gizmo";

                Decal decal = this._defenderMachinesCircleEntities[i].GetComponentAtIndex(0, GameEntity.ComponentType.Decal) as Decal;
                Material material = decal.GetMaterial();

                // If the current decal material is not what the same as desiredMaterialName, then we change it
                if (((material != null) ? material.Name : null) != desiredMaterialName)
                {
                    decal.SetMaterial(Material.GetFromResource(desiredMaterialName));
                }
                bool isHovered = tuple != null && this._defenderMachinesCircleEntities[i].GetGlobalFrame().NearlyEquals(tuple.Item1, 1E-05f);
                uint desiredDecalColor = this.GetDesiredDecalColor(isHovered, isEnemy, isEmpty, isPlayerLeader);
                if (desiredDecalColor != decal.GetFactor1())
                {
                    decal.SetFactor1(desiredDecalColor);
                }
            }
            for (int j = 0; j < settlementVisual.SCGetAttackerRangedSiegeEngineFrames().Length; j++)
            {
                bool isEmpty2 = playerSiegeEvent.GetSiegeEventSide(BattleSideEnum.Attacker).SiegeEngines.DeployedRangedSiegeEngines[j] == null;
                bool isEnemy2 = PlayerSiege.PlayerSide != BattleSideEnum.Attacker;
                string desiredMaterialName2 = this.GetDesiredMaterialName(true, true, false);
                Decal decal2 = this._attackerRangedMachinesCircleEntities[j].GetComponentAtIndex(0, GameEntity.ComponentType.Decal) as Decal;
                Material material2 = decal2.GetMaterial();
                if (((material2 != null) ? material2.Name : null) != desiredMaterialName2)
                {
                    decal2.SetMaterial(Material.GetFromResource(desiredMaterialName2));
                }
                bool isHovered2 = tuple != null && this._attackerRangedMachinesCircleEntities[j].GetGlobalFrame().NearlyEquals(tuple.Item1, 1E-05f);
                uint desiredDecalColor2 = this.GetDesiredDecalColor(isHovered2, isEnemy2, isEmpty2, isPlayerLeader);
                if (desiredDecalColor2 != decal2.GetFactor1())
                {
                    decal2.SetFactor1(desiredDecalColor2);
                }
            }
            for (int k = 0; k < settlementVisual.SCGetAttackerBatteringRamSiegeEngineFrames().Length; k++)
            {
                bool isEmpty3 = playerSiegeEvent.GetSiegeEventSide(BattleSideEnum.Attacker).SiegeEngines.DeployedMeleeSiegeEngines[k] == null;
                bool isEnemy3 = PlayerSiege.PlayerSide != BattleSideEnum.Attacker;
                string desiredMaterialName3 = this.GetDesiredMaterialName(false, true, false);
                Decal decal3 = this._attackerRamMachinesCircleEntities[k].GetComponentAtIndex(0, GameEntity.ComponentType.Decal) as Decal;
                Material material3 = decal3.GetMaterial();
                if (((material3 != null) ? material3.Name : null) != desiredMaterialName3)
                {
                    decal3.SetMaterial(Material.GetFromResource(desiredMaterialName3));
                }
                bool isHovered3 = tuple != null && this._attackerRamMachinesCircleEntities[k].GetGlobalFrame().NearlyEquals(tuple.Item1, 1E-05f);
                uint desiredDecalColor3 = this.GetDesiredDecalColor(isHovered3, isEnemy3, isEmpty3, isPlayerLeader);
                if (desiredDecalColor3 != decal3.GetFactor1())
                {
                    decal3.SetFactor1(desiredDecalColor3);
                }
            }
            for (int l = 0; l < settlementVisual.SCGetAttackerTowerSiegeEngineFrames().Length; l++)
            {
                bool isEmpty4 = playerSiegeEvent.GetSiegeEventSide(BattleSideEnum.Attacker).SiegeEngines.DeployedMeleeSiegeEngines[settlementVisual.SCGetAttackerBatteringRamSiegeEngineFrames().Length + l] == null;
                bool isEnemy4 = PlayerSiege.PlayerSide != BattleSideEnum.Attacker;
                string desiredMaterialName4 = this.GetDesiredMaterialName(false, true, true);
                Decal decal4 = this._attackerTowerMachinesCircleEntities[l].GetComponentAtIndex(0, GameEntity.ComponentType.Decal) as Decal;
                Material material4 = decal4.GetMaterial();
                if (((material4 != null) ? material4.Name : null) != desiredMaterialName4)
                {
                    decal4.SetMaterial(Material.GetFromResource(desiredMaterialName4));
                }
                bool isHovered4 = tuple != null && this._attackerTowerMachinesCircleEntities[l].GetGlobalFrame().NearlyEquals(tuple.Item1, 1E-05f);
                uint desiredDecalColor4 = this.GetDesiredDecalColor(isHovered4, isEnemy4, isEmpty4, isPlayerLeader);
                if (desiredDecalColor4 != decal4.GetFactor1())
                {
                    decal4.SetFactor1(desiredDecalColor4);
                }
            }
        }

        protected uint GetDesiredDecalColor(bool isHovered, bool isEnemy, bool isEmpty, bool isPlayerLeader)
        {
            if (isEnemy)
            {
                return 4287064638U;
            }
            if (isHovered && isPlayerLeader)
            {
                return 4293956364U;
            }
            if (!isEmpty)
            {
                return 4283683126U;
            }
            if (isPlayerLeader)
            {
                float num = TaleWorlds.Library.MathF.PingPong(0f, 0.5f, this._timeSinceCreation) / 0.5f;
                Color color = Color.FromUint(4278394186U);
                Color color2 = Color.FromUint(4284320212U);
                return Color.Lerp(color, color2, num).ToUnsignedInteger();
            }
            return 4278394186U;
        }

        protected string GetDesiredMaterialName(bool isRanged, bool isAttacker, bool isTower)
        {
            if (isRanged)
            {
                if (!isAttacker)
                {
                    return "decal_defender_ranged_siege";
                }
                return "decal_siege_ranged";
            }
            else
            {
                if (!isTower)
                {
                    return "decal_siege_ram";
                }
                return "decal_siege_tower";
            }
        }

        protected void RemoveSiegeCircleVisuals()
        {
            if (this._playerSiegeMachineSlotMeshesAdded)
            {
                MapScene mapScene = Campaign.Current.MapSceneWrapper as MapScene;
                for (int i = 0; i < this._defenderMachinesCircleEntities.Length; i++)
                {
                    this._defenderMachinesCircleEntities[i].SetVisibilityExcludeParents(false);
                    mapScene.Scene.RemoveEntity(this._defenderMachinesCircleEntities[i], 107);
                    this._defenderMachinesCircleEntities[i] = null;
                }
                for (int j = 0; j < this._attackerRamMachinesCircleEntities.Length; j++)
                {
                    this._attackerRamMachinesCircleEntities[j].SetVisibilityExcludeParents(false);
                    mapScene.Scene.RemoveEntity(this._attackerRamMachinesCircleEntities[j], 108);
                    this._attackerRamMachinesCircleEntities[j] = null;
                }
                for (int k = 0; k < this._attackerTowerMachinesCircleEntities.Length; k++)
                {
                    this._attackerTowerMachinesCircleEntities[k].SetVisibilityExcludeParents(false);
                    mapScene.Scene.RemoveEntity(this._attackerTowerMachinesCircleEntities[k], 109);
                    this._attackerTowerMachinesCircleEntities[k] = null;
                }
                for (int l = 0; l < this._attackerRangedMachinesCircleEntities.Length; l++)
                {
                    this._attackerRangedMachinesCircleEntities[l].SetVisibilityExcludeParents(false);
                    mapScene.Scene.RemoveEntity(this._attackerRangedMachinesCircleEntities[l], 110);
                    this._attackerRangedMachinesCircleEntities[l] = null;
                }
                this._playerSiegeMachineSlotMeshesAdded = false;
            }
        }

        protected void RefreshMapSiegeOverlayRequired()
        {
            MapScreen.Instance.MapCameraView.OnRefreshMapSiegeOverlayRequired(this._mapSiegeOverlayView == null);
            if (this._playerSiegeMachineSlotMeshesAdded && PlayerSiege.PlayerSiegeEvent != null)
            {
                Settlement besiegedSettlement = PlayerSiege.PlayerSiegeEvent.BesiegedSettlement;
                if (besiegedSettlement != null && besiegedSettlement.CurrentSiegeState == Settlement.SiegeState.InTheLordsHall)
                {
                    this.RemoveSiegeCircleVisuals();
                    this._playerSiegeMachineSlotMeshesAdded = false;
                    return;
                }
            }
            if (PlayerSiege.PlayerSiegeEvent == null && this._mapSiegeOverlayView != null)
            {
                MapScreen.Instance.RemoveMapView(this._mapSiegeOverlayView);
                this._mapSiegeOverlayView = null;
                if (this._playerSiegeMachineSlotMeshesAdded)
                {
                    this.RemoveSiegeCircleVisuals();
                    this._playerSiegeMachineSlotMeshesAdded = false;
                    return;
                }
            }
            else if (PlayerSiege.PlayerSiegeEvent != null && this._mapSiegeOverlayView == null)
            {
                this._mapSiegeOverlayView = MapScreen.Instance.AddMapView<SCMapSiegeOverlayView>(Array.Empty<object>());
                if (!this._playerSiegeMachineSlotMeshesAdded)
                {
                    this.InitializeSiegeCircleVisuals();
                    this._playerSiegeMachineSlotMeshesAdded = true;
                }
            }
        }

        protected void InitializeSiegeCircleVisuals()
        {
            Settlement besiegedSettlement = PlayerSiege.PlayerSiegeEvent.BesiegedSettlement;
            SCSettlementVisual settlementVisual = (SCSettlementVisual)this.GetSettlementVisual(besiegedSettlement);
            MapScene mapScene = Campaign.Current.MapSceneWrapper as MapScene;
            MatrixFrame[] array = settlementVisual.SCGetDefenderRangedSiegeEngineFrames();
            this._defenderMachinesCircleEntities = new GameEntity[array.Length];

            for (int i = 0; i < array.Length; i++)
            {
                MatrixFrame matrixFrame = array[i];
                this._defenderMachinesCircleEntities[i] = GameEntity.CreateEmpty(mapScene.Scene, true, true, true);
                this._defenderMachinesCircleEntities[i].Name = "dRangedMachineCircle_" + i;
                Decal decal = Decal.CreateDecal(null);
                decal.SetMaterial(Material.GetFromResource("decal_defender_ranged_siege"));
                decal.SetFactor1Linear(4287064638U);
                this._defenderMachinesCircleEntities[i].AddComponent(decal);
                MatrixFrame matrixFrame2 = matrixFrame;
                if (this._isNewDecalScaleImplementationEnabled)
                {
                    Vec3 vec = new Vec3(0.25f, 0.25f, 0.25f, -1f);
                    matrixFrame2.Scale(in vec);
                }
                this._defenderMachinesCircleEntities[i].SetGlobalFrame(in matrixFrame2, true);
                this._defenderMachinesCircleEntities[i].SetVisibilityExcludeParents(true);

                // Don't add any defender placament decals since everything is at the centre of the planet
                // mapScene.Scene.AddDecalInstance(decal, "editor_set", true);
            }

            array = settlementVisual.SCGetAttackerBatteringRamSiegeEngineFrames();
            this._attackerRamMachinesCircleEntities = new GameEntity[array.Length];

            for (int j = 0; j < array.Length; j++)
            {
                MatrixFrame matrixFrame3 = array[j];
                this._attackerRamMachinesCircleEntities[j] = GameEntity.CreateEmpty(mapScene.Scene, true, true, true);
                this._attackerRamMachinesCircleEntities[j].Name = "InitializeSiegeCircleVisuals";
                this._attackerRamMachinesCircleEntities[j].Name = "aRamMachineCircle_" + j;
                Decal decal2 = Decal.CreateDecal(null);
                decal2.SetMaterial(Material.GetFromResource("decal_siege_ram"));
                decal2.SetFactor1Linear(4287064638U);
                this._attackerRamMachinesCircleEntities[j].AddComponent(decal2);
                MatrixFrame matrixFrame4 = matrixFrame3;
                if (this._isNewDecalScaleImplementationEnabled)
                {
                    Vec3 vec = new Vec3(0.38f, 0.38f, 0.38f, -1f);
                    matrixFrame4.Scale(in vec);
                }
                this._attackerRamMachinesCircleEntities[j].SetGlobalFrame(in matrixFrame4, true);
                this._attackerRamMachinesCircleEntities[j].SetVisibilityExcludeParents(true);
                
                // Don't add any siege decals for the battering ram
                // mapScene.Scene.AddDecalInstance(decal2, "editor_set", true);
            }

            array = settlementVisual.SCGetAttackerTowerSiegeEngineFrames();
            this._attackerTowerMachinesCircleEntities = new GameEntity[array.Length];

            for (int k = 0; k < array.Length; k++)
            {
                MatrixFrame matrixFrame5 = array[k];
                this._attackerTowerMachinesCircleEntities[k] = GameEntity.CreateEmpty(mapScene.Scene, true, true, true);
                this._attackerTowerMachinesCircleEntities[k].Name = "aTowerMachineCircle_" + k;
                Decal decal3 = Decal.CreateDecal(null);
                decal3.SetMaterial(Material.GetFromResource("decal_siege_tower"));
                decal3.SetFactor1Linear(4287064638U);
                this._attackerTowerMachinesCircleEntities[k].AddComponent(decal3);
                MatrixFrame matrixFrame6 = matrixFrame5;
                if (this._isNewDecalScaleImplementationEnabled)
                {
                    Vec3 vec = new Vec3(0.38f, 0.38f, 0.38f, -1f);
                    matrixFrame6.Scale(in vec);
                }
                this._attackerTowerMachinesCircleEntities[k].SetGlobalFrame(in matrixFrame6, true);
                this._attackerTowerMachinesCircleEntities[k].SetVisibilityExcludeParents(true);

                // Don't add any siege decals for the siege towers
                // mapScene.Scene.AddDecalInstance(decal3, "editor_set", true);
            }

            array = settlementVisual.SCGetAttackerRangedSiegeEngineFrames();
            this._attackerRangedMachinesCircleEntities = new GameEntity[array.Length];

            for (int l = 0; l < array.Length; l++)
            {
                MatrixFrame matrixFrame7 = array[l];
                GameEntity circleEntity = GameEntity.CreateEmpty(mapScene.Scene, true, true, true);
                Mesh plane = MeshBuilder.CreateUnitMesh();
                // plane.SetMaterial(Material.GetDefaultMaterial());

                // Can't get the decal override working so we'll use a material for now. Would rather the decal though. 
                Material decalMat = Material.GetFromResource("sc_mat_siege_ranged").CreateCopy();
                plane.SetMaterial(decalMat);

                circleEntity.AddMesh(plane);

                this._attackerRangedMachinesCircleEntities[l] = circleEntity;
                this._attackerRangedMachinesCircleEntities[l].Name = "aRangedMachineCircle_" + l;

                // A copy of the native decal with render_on_terrain turned off and render_on_object enabled
                Decal decal4 = Decal.CreateDecal(null);
                decal4.SetMaterial(Material.GetFromResource("decal_siege_ranged"));
                decal4.SetFactor1Linear(4287064638U);
                this._attackerRangedMachinesCircleEntities[l].AddComponent(decal4);

                MatrixFrame matrixFrame8 = matrixFrame7;
                if (this._isNewDecalScaleImplementationEnabled)
                {
                    Vec3 vec = new Vec3(0.38f, 0.38f, 0.38f, -1f);
                    matrixFrame8.Scale(in vec);
                }

                this._attackerRangedMachinesCircleEntities[l].SetGlobalFrame(in matrixFrame8, true);
                this._attackerRangedMachinesCircleEntities[l].SetVisibilityExcludeParents(true);
                // mapScene.Scene.AddDecalInstance(decal4, "editor_set", true);
            }

            // We need to move this into a class that actually handles the siege engines
            this.InitMeleeSiegeEngines();
        }

        protected void InitMeleeSiegeEngines()
        {
            Settlement besiegedSettlement = PlayerSiege.PlayerSiegeEvent.BesiegedSettlement;
            SCSettlementVisual settlementVisual = (SCSettlementVisual)this.GetSettlementVisual(besiegedSettlement);
            SiegeEvent siegeEvent = settlementVisual.MapEntity.Settlement.SiegeEvent;

            // Ram
            // Need to test if the ram is always placed on the 0 index
            float batteringRamHitPoints = Campaign.Current.Models.SiegeEventModel.GetSiegeEngineHitPoints(siegeEvent, DefaultSiegeEngineTypes.Ram, siegeEvent.BesiegerCamp.BattleSide);
            SiegeEvent.SiegeEngineConstructionProgress batteringRam = new SiegeEvent.SiegeEngineConstructionProgress(DefaultSiegeEngineTypes.Ram, 1f, batteringRamHitPoints);
            siegeEvent.BesiegerCamp.SiegeEngines.DeploySiegeEngineAtIndex(batteringRam, 0);

            // 2 Towers
            // Can an attacker have less/more than 2 siege tower placements?
            float towerHitPoints = Campaign.Current.Models.SiegeEventModel.GetSiegeEngineHitPoints(siegeEvent, DefaultSiegeEngineTypes.SiegeTower, siegeEvent.BesiegerCamp.BattleSide);
            SiegeEvent.SiegeEngineConstructionProgress siegeTowerOne = new SiegeEvent.SiegeEngineConstructionProgress(DefaultSiegeEngineTypes.SiegeTower, 1f, towerHitPoints);
            SiegeEvent.SiegeEngineConstructionProgress siegeTowerTwo = new SiegeEvent.SiegeEngineConstructionProgress(DefaultSiegeEngineTypes.SiegeTower, 1f, towerHitPoints);
            siegeEvent.BesiegerCamp.SiegeEngines.DeploySiegeEngineAtIndex(siegeTowerOne, 1);
            siegeEvent.BesiegerCamp.SiegeEngines.DeploySiegeEngineAtIndex(siegeTowerTwo, 2);

            siegeEvent.BesiegedSettlement.Party.SetVisualAsDirty();
        }

        protected void HandleSiegeEngineHover(UIntPtr newID)
        {
            if (this._hoveredSiegeEntityID != newID)
            {
                Dictionary<UIntPtr, Tuple<MatrixFrame, SettlementVisual>> engFramesAndVisuals = (Dictionary<UIntPtr, Tuple<MatrixFrame, SettlementVisual>>)FrameAndVisualOfEnginesGetter.Invoke(this, new object[] { });
                this._hoveredSiegeEntityID = newID;
                Tuple<MatrixFrame, SettlementVisual> tuple = engFramesAndVisuals[this._hoveredSiegeEntityID];
                SCSettlementVisual setVis = (SCSettlementVisual)tuple.Item2;
                setVis.OnMapHoverSiegeEngine(tuple.Item1);
            }
        }

        protected void HandleSiegeEngineHoverEnd()
        {
            if (this._hoveredSiegeEntityID != UIntPtr.Zero)
            {
                Dictionary<UIntPtr, Tuple<MatrixFrame, SettlementVisual>> engFramesAndVisuals = (Dictionary<UIntPtr, Tuple<MatrixFrame, SettlementVisual>>)FrameAndVisualOfEnginesGetter.Invoke(this, new object[] { });
                SCSettlementVisual settVis = (SCSettlementVisual)engFramesAndVisuals[this._hoveredSiegeEntityID].Item2;
                settVis.OnMapHoverSiegeEngineEnd();
                this._hoveredSiegeEntityID = UIntPtr.Zero;
            }
        }
    }
}
