using HarmonyLib;
using Helpers;
using SandBox;
using SandBox.View.Map;
using SandBox.View.Map.Visuals;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Map;
using TaleWorlds.CampaignSystem.Party;
using TaleWorlds.Engine;
using TaleWorlds.Library;

namespace SeparatistCrisis.Map
{
    public class SCSettlementVisual : SettlementVisual
    {
        protected static MethodInfo StratEntitySetter = AccessTools.PropertySetter(typeof(SettlementVisual), "StrategicEntity");
        protected static MethodInfo MapSceneGetter = AccessTools.PropertyGetter(typeof(SettlementVisual), "MapScene");
        protected static MethodInfo PopulateSiegeEngineFrameListsFromChildrenMethod = AccessTools.Method(typeof(SettlementVisual), "PopulateSiegeEngineFrameListsFromChildren");
        protected static MethodInfo UpdateDefenderSiegeEntitiesCacheMethod = AccessTools.Method(typeof(SettlementVisual), "UpdateDefenderSiegeEntitiesCache");
        protected static MethodInfo TownPhysicalEntitiesSetter = AccessTools.PropertySetter(typeof(SettlementVisual), "TownPhysicalEntities");
        protected static FieldInfo GateBannerEntitiesWithLevelsField = AccessTools.Field(typeof(SettlementVisual), "_gateBannerEntitiesWithLevels");

        protected static MethodInfo CampEntWithNameMethod = AccessTools.Method(typeof(Scene), "GetCampaignEntityWithName");
        protected static MethodInfo VisualsOfEntitiesGetter = AccessTools.PropertyGetter(typeof(MapScreen), "VisualsOfEntities");
        protected static MethodInfo FrameAndVisualOfEnginesGetter = AccessTools.PropertyGetter(typeof(MapScreen), "FrameAndVisualOfEngines");

        public SCSettlementVisual(PartyBase entity) : base(entity)
        {
        }

        public void OnStartup()
        {

            bool flag = false;
            Scene mapScene = (Scene)MapSceneGetter.Invoke(this, new object[] {});
            GameEntity? stratEnt = (GameEntity?)CampEntWithNameMethod.Invoke(mapScene, new object[] { base.MapEntity.Id });
            StratEntitySetter.Invoke(this, new object[] { stratEnt });
            if (this.StrategicEntity == null)
            {
                IMapScene mapSceneWrapper = Campaign.Current.MapSceneWrapper;
                string stringId = base.MapEntity.Settlement.StringId;
                CampaignVec2 position = base.MapEntity.Settlement.Position;
                mapSceneWrapper.AddNewEntityToMapScene(stringId, position);

                GameEntity stratEnt2 = (GameEntity)CampEntWithNameMethod.Invoke(mapScene, new object[] { base.MapEntity.Id });
                StratEntitySetter.Invoke(this, new object[] { stratEnt2 });
            }

            bool flag2 = false;
            if (base.MapEntity.Settlement.IsFortification)
            {
                List<GameEntity> list = new List<GameEntity>();
                this.StrategicEntity.GetChildrenRecursive(ref list);

                PopulateSiegeEngineFrameListsFromChildrenMethod.Invoke(this, new object[] { list });
                UpdateDefenderSiegeEntitiesCacheMethod.Invoke(this, new object[] {});
                TownPhysicalEntitiesSetter.Invoke(this, new object[] { list.FindAll((GameEntity x) => x.HasTag("bo_town")) });

                List<GameEntity> list2 = new List<GameEntity>();
                Dictionary<int, List<GameEntity>> dictionary = new Dictionary<int, List<GameEntity>>
                {
                    {
                        1, new List<GameEntity>()
                    },
                    {
                        2, new List<GameEntity>()
                    },
                    {
                        3, new List<GameEntity>()
                    }
                };

                foreach (GameEntity gameEntity in list)
                {
                    if (gameEntity.HasTag("main_map_city_gate"))
                    {
                        NavigationHelper.IsPositionValidForNavigationType(new CampaignVec2(gameEntity.GetGlobalFrame().origin.AsVec2, true), MobileParty.NavigationType.Default);
                        flag2 = true;
                        list2.Add(gameEntity);
                    }
                    if (gameEntity.HasTag("map_settlement_circle"))
                    {
                        this.CircleLocalFrame = gameEntity.GetGlobalFrame();
                        flag = true;
                        gameEntity.SetVisibilityExcludeParents(false);
                        list2.Add(gameEntity);
                    }
                    if (gameEntity.HasTag("map_banner_placeholder"))
                    {
                        int upgradeLevelOfEntity = gameEntity.Parent.GetUpgradeLevelOfEntity();
                        if (upgradeLevelOfEntity == 0)
                        {
                            dictionary[1].Add(gameEntity);
                            dictionary[2].Add(gameEntity);
                            dictionary[3].Add(gameEntity);
                        }
                        else
                        {
                            dictionary[upgradeLevelOfEntity].Add(gameEntity);
                        }
                        list2.Add(gameEntity);
                    }
                }

                GateBannerEntitiesWithLevelsField.SetValue(this, dictionary);

                if (base.MapEntity.Settlement.IsFortification)
                {
                    List<MatrixFrame> list3;
                    List<MatrixFrame> list4;
                    Campaign.Current.MapSceneWrapper.GetSiegeCampFrames(base.MapEntity.Settlement, out list3, out list4);
                    base.MapEntity.Settlement.Town.BesiegerCampPositions1 = list3.ToArray();
                    base.MapEntity.Settlement.Town.BesiegerCampPositions2 = list4.ToArray();
                }

                foreach (GameEntity gameEntity2 in list2)
                {
                    gameEntity2.Remove(112);
                }

                if (!flag2 && !base.MapEntity.Settlement.IsTown)
                {
                    bool isCastle = base.MapEntity.Settlement.IsCastle;
                }

                bool flag3 = false;
                if (base.MapEntity.IsSettlement)
                {
                    foreach (GameEntity gameEntity3 in this.StrategicEntity.GetChildren())
                    {
                        if (gameEntity3.HasTag("main_map_city_port"))
                        {
                            NavigationHelper.IsPositionValidForNavigationType(new CampaignVec2(gameEntity3.GetGlobalFrame().origin.AsVec2, false), MobileParty.NavigationType.Naval);
                            flag3 = true;
                        }
                    }
                    if ((flag3 || !base.MapEntity.Settlement.HasPort) && flag3)
                    {
                        bool hasPort = base.MapEntity.Settlement.HasPort;
                    }
                }
            }

            if (!flag)
            {
                this.CircleLocalFrame = MatrixFrame.Identity;
                MatrixFrame circleLocalFrame = this.CircleLocalFrame;
                Mat3 rotation = circleLocalFrame.rotation;
                if (base.MapEntity.Settlement.IsVillage)
                {
                    rotation.ApplyScaleLocal(1.75f);
                }
                else if (base.MapEntity.Settlement.IsTown)
                {
                    rotation.ApplyScaleLocal(5.75f);
                }
                else if (base.MapEntity.Settlement.IsCastle)
                {
                    rotation.ApplyScaleLocal(2.75f);
                }
                else
                {
                    rotation.ApplyScaleLocal(1.75f);
                }
                circleLocalFrame.rotation = rotation;
                this.CircleLocalFrame = circleLocalFrame;
            }

            this.StrategicEntity.SetVisibilityExcludeParents(base.MapEntity.IsVisible);
            this.StrategicEntity.SetReadyToRender(true);
            this.StrategicEntity.SetEntityEnvMapVisibility(false);
            List<GameEntity> list5 = new List<GameEntity>();
            this.StrategicEntity.GetChildrenRecursive(ref list5);

            Dictionary<UIntPtr, MapEntityVisual> entVisuals = (Dictionary<UIntPtr, MapEntityVisual>)VisualsOfEntitiesGetter.Invoke(this, new object[] {});
            Dictionary<UIntPtr, Tuple<MatrixFrame, SettlementVisual>> engFramesAndVisuals = (Dictionary<UIntPtr, Tuple<MatrixFrame, SettlementVisual>>)FrameAndVisualOfEnginesGetter.Invoke(this, new object[] { });


            if (!entVisuals.ContainsKey(this.StrategicEntity.Pointer))
            {
                entVisuals.Add(this.StrategicEntity.Pointer, this);
            }
            foreach (GameEntity gameEntity4 in list5)
            {
                if (!entVisuals.ContainsKey(gameEntity4.Pointer) && !engFramesAndVisuals.ContainsKey(gameEntity4.Pointer))
                {
                    entVisuals.Add(gameEntity4.Pointer, this);
                }
            }
            this.StrategicEntity.SetAsPredisplayEntity();
        }
    }
}
