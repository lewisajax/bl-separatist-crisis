using System;
using System.Collections.Generic;
using System.Linq;
using Helpers;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.CharacterCreationContent;
using TaleWorlds.CampaignSystem.CharacterDevelopment;
using TaleWorlds.CampaignSystem.Extensions;
using TaleWorlds.CampaignSystem.GameState;
using TaleWorlds.CampaignSystem.Party;
using TaleWorlds.CampaignSystem.Settlements;
using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.Localization;

namespace SeparatistCrisis.Character
{
    // public class SCCharacterCreationContent : CharacterCreationContentBase
    // {
    //     protected const int FocusToAddYouthStart = 2;
    //     protected const int FocusToAddAdultStart = 4;
    //     protected const int FocusToAddMiddleAgedStart = 6;
    //     protected const int FocusToAddElderlyStart = 8;
    //     protected const int AttributeToAddYouthStart = 1;
    //     protected const int AttributeToAddAdultStart = 2;
    //     protected const int AttributeToAddMiddleAgedStart = 3;
    //     protected const int AttributeToAddElderlyStart = 4;

    //     protected SCCharacterCreationContent.SandboxAgeOptions _startingAge =
    //         ScCharacterCreationContent.SandboxAgeOptions.YoungAdult;

    //     protected SCCharacterCreationContent.OccupationTypes _familyOccupationType;
    //     protected TextObject _educationIntroductoryText = new TextObject("{=!}{EDUCATION_INTRO}");
    //     protected TextObject _youthIntroductoryText = new TextObject("{=!}{YOUTH_INTRO}");

    //     public override TextObject ReviewPageDescription => new TextObject(
    //         "{=W6pKpEoT}You prepare to set off for a grand adventure in Calradia! Here is your character. Continue if you are ready, or go back to make changes.");

    //     public override IEnumerable<Type> CharacterCreationStages
    //     {
    //         get
    //         {
    //             yield return typeof(CharacterCreationCultureStage);
    //             yield return typeof(CharacterCreationFaceGeneratorStage);
    //             yield return typeof(CharacterCreationGenericStage);
    //             yield return typeof(CharacterCreationBannerEditorStage);
    //             yield return typeof(CharacterCreationClanNamingStage);
    //             yield return typeof(CharacterCreationReviewStage);
    //             yield return typeof(CharacterCreationOptionsStage);
    //         }
    //     }

    //     protected override void OnCultureSelected()
    //     {
    //         this.SelectedTitleType = 1;
    //         this.SelectedParentType = 0;
    //         TextObject clanNameforPlayer = FactionHelper.GenerateClanNameforPlayer();
    //         Clan.PlayerClan.ChangeClanName(clanNameforPlayer, clanNameforPlayer);
    //     }

    //     public override int GetSelectedParentType() => this.SelectedParentType;

    //     public override void OnCharacterCreationFinalized()
    //     {
    //         CultureObject culture = GetSelectedCulture();
    //         Vec2 settlementLocation = Settlement.FindFirst(s => s.Culture == culture).GatePosition;

    //         MobileParty.MainParty.Position2D = settlementLocation;
    //         ((MapState)GameStateManager.Current.ActiveState).Handler.TeleportCameraToMainParty();

    //         this.SetHeroAge((float) this._startingAge);
    //     }

    //     protected override void OnInitialized(CharacterCreation characterCreation)
    //     {
    //         this.AddParentsMenu(characterCreation);
    //         this.AddChildhoodMenu(characterCreation);
    //         this.AddEducationMenu(characterCreation);
    //         this.AddYouthMenu(characterCreation);
    //         this.AddAdulthoodMenu(characterCreation);
    //         this.AddAgeSelectionMenu(characterCreation);
    //     }

    //     protected override void OnApplyCulture()
    //     {
    //     }

    //     protected void AddParentsMenu(CharacterCreation characterCreation)
    //     {
    //         CharacterCreationMenu menu = new CharacterCreationMenu(new TextObject("{=b4lDDcli}Family"),
    //             new TextObject("{=XgFU1pCx}You were born into a family of..."),
    //             new CharacterCreationOnInit(this.ParentsOnInit));
    //         AddRepublicParentOptions(menu);
    //         AddSeparatistParentOptions(menu);
    //         AddMandalorianParentOptions(menu);

    //         characterCreation.AddNewMenu(menu);
    //     }

    //     private void AddMandalorianParentOptions(CharacterCreationMenu menu)
    //     {
    //         CharacterCreationCategory creationCategory3 =
    //             menu.AddMenuCategory(new CharacterCreationOnCondition(this.MandalorianParentsOnCondition));
    //         TextObject text1 = new TextObject("{=mc78FEbA}A boyar's companions");
    //         List<SkillObject> effectedSkills1 = new List<SkillObject>();
    //         effectedSkills1.Add(DefaultSkills.Riding);
    //         effectedSkills1.Add(DefaultSkills.TwoHanded);
    //         CharacterAttribute social1 = DefaultCharacterAttributes.Social;
    //         int focusToAdd1 = this.FocusToAdd;
    //         int skillLevelToAdd1 = this.SkillLevelToAdd;
    //         int attributeLevelToAdd1 = this.AttributeLevelToAdd;
    //         CharacterCreationOnSelect onSelect1 =
    //             new CharacterCreationOnSelect(this.MandalorianBoyarsCompanionOnConsequence);
    //         CharacterCreationApplyFinalEffects onApply1 =
    //             new CharacterCreationApplyFinalEffects(this.MandalorianBoyarsCompanionOnApply);
    //         TextObject descriptionText1 = new TextObject(
    //             "{=hob3WVkU}Your father was a member of a boyar's druzhina, the 'companions' that make up his retinue. He sat at his lord's table in the great hall, oversaw the boyar's estates, and stood by his side in the center of the shield wall in battle.");
    //         creationCategory3.AddCategoryOption(text1, effectedSkills1, social1, focusToAdd1, skillLevelToAdd1,
    //             attributeLevelToAdd1, (CharacterCreationOnCondition) null, onSelect1, onApply1, descriptionText1);
    //         TextObject text2 = new TextObject("{=HqzVBfpl}Urban traders");
    //         List<SkillObject> effectedSkills2 = new List<SkillObject>();
    //         effectedSkills2.Add(DefaultSkills.Trade);
    //         effectedSkills2.Add(DefaultSkills.Tactics);
    //         CharacterAttribute cunning1 = DefaultCharacterAttributes.Cunning;
    //         int focusToAdd2 = this.FocusToAdd;
    //         int skillLevelToAdd2 = this.SkillLevelToAdd;
    //         int attributeLevelToAdd2 = this.AttributeLevelToAdd;
    //         CharacterCreationOnSelect onSelect2 = new CharacterCreationOnSelect(this.MandalorianTraderOnConsequence);
    //         CharacterCreationApplyFinalEffects onApply2 =
    //             new CharacterCreationApplyFinalEffects(this.MandalorianTraderOnApply);
    //         TextObject descriptionText2 =
    //             new TextObject(
    //                 "{=bjVMtW3W}Your family were merchants who lived in one of Sturgia's great river ports, organizing the shipment of the north's bounty of furs, honey and other goods to faraway lands.");
    //         creationCategory3.AddCategoryOption(text2, effectedSkills2, cunning1, focusToAdd2, skillLevelToAdd2,
    //             attributeLevelToAdd2, (CharacterCreationOnCondition) null, onSelect2, onApply2, descriptionText2);
    //         TextObject text3 = new TextObject("{=zrpqSWSh}Free farmers");
    //         List<SkillObject> effectedSkills3 = new List<SkillObject>();
    //         effectedSkills3.Add(DefaultSkills.Athletics);
    //         effectedSkills3.Add(DefaultSkills.Polearm);
    //         CharacterAttribute endurance1 = DefaultCharacterAttributes.Endurance;
    //         int focusToAdd3 = this.FocusToAdd;
    //         int skillLevelToAdd3 = this.SkillLevelToAdd;
    //         int attributeLevelToAdd3 = this.AttributeLevelToAdd;
    //         CharacterCreationOnSelect onSelect3 = new CharacterCreationOnSelect(this.MandalorianFreemanOnConsequence);
    //         CharacterCreationApplyFinalEffects onApply3 =
    //             new CharacterCreationApplyFinalEffects(this.MandalorianFreemanOnApply);
    //         TextObject descriptionText3 =
    //             new TextObject(
    //                 "{=Mcd3ZyKq}Your family had just enough land to feed themselves and make a small profit. People like them were the pillars of the kingdom's economy, as well as the backbone of the levy.");
    //         creationCategory3.AddCategoryOption(text3, effectedSkills3, endurance1, focusToAdd3, skillLevelToAdd3,
    //             attributeLevelToAdd3, (CharacterCreationOnCondition) null, onSelect3, onApply3, descriptionText3);
    //         TextObject text4 = new TextObject("{=v48N6h1t}Urban artisans");
    //         List<SkillObject> effectedSkills4 = new List<SkillObject>();
    //         effectedSkills4.Add(DefaultSkills.Crafting);
    //         effectedSkills4.Add(DefaultSkills.OneHanded);
    //         CharacterAttribute intelligence1 = DefaultCharacterAttributes.Intelligence;
    //         int focusToAdd4 = this.FocusToAdd;
    //         int skillLevelToAdd4 = this.SkillLevelToAdd;
    //         int attributeLevelToAdd4 = this.AttributeLevelToAdd;
    //         CharacterCreationOnSelect onSelect4 = new CharacterCreationOnSelect(this.MandalorianArtisanOnConsequence);
    //         CharacterCreationApplyFinalEffects onApply4 =
    //             new CharacterCreationApplyFinalEffects(this.MandalorianArtisanOnApply);
    //         TextObject descriptionText4 = new TextObject(
    //             "{=ueCm5y1C}Your family owned their own workshop in a city, making goods from raw materials brought in from the countryside. Your father played an active if minor role in the town council, and also served in the militia.");
    //         creationCategory3.AddCategoryOption(text4, effectedSkills4, intelligence1, focusToAdd4, skillLevelToAdd4,
    //             attributeLevelToAdd4, (CharacterCreationOnCondition) null, onSelect4, onApply4, descriptionText4);
    //         TextObject text5 = new TextObject("{=YcnK0Thk}Hunters");
    //         List<SkillObject> effectedSkills5 = new List<SkillObject>();
    //         effectedSkills5.Add(DefaultSkills.Scouting);
    //         effectedSkills5.Add(DefaultSkills.Bow);
    //         CharacterAttribute vigor1 = DefaultCharacterAttributes.Vigor;
    //         int focusToAdd5 = this.FocusToAdd;
    //         int skillLevelToAdd5 = this.SkillLevelToAdd;
    //         int attributeLevelToAdd5 = this.AttributeLevelToAdd;
    //         CharacterCreationOnSelect onSelect5 = new CharacterCreationOnSelect(this.MandalorianHunterOnConsequence);
    //         CharacterCreationApplyFinalEffects onApply5 =
    //             new CharacterCreationApplyFinalEffects(this.MandalorianHunterOnApply);
    //         TextObject descriptionText5 = new TextObject(
    //             "{=WyZ2UtFF}Your family had no taste for the authority of the boyars. They made their living deep in the woods, slashing and burning fields which they tended for a year or two before moving on. They hunted and trapped fox, hare, ermine, and other fur-bearing animals.");
    //         creationCategory3.AddCategoryOption(text5, effectedSkills5, vigor1, focusToAdd5, skillLevelToAdd5,
    //             attributeLevelToAdd5, (CharacterCreationOnCondition) null, onSelect5, onApply5, descriptionText5);
    //         TextObject text6 = new TextObject("{=TPoK3GSj}Vagabonds");
    //         List<SkillObject> effectedSkills6 = new List<SkillObject>();
    //         effectedSkills6.Add(DefaultSkills.Roguery);
    //         effectedSkills6.Add(DefaultSkills.Throwing);
    //         CharacterAttribute control1 = DefaultCharacterAttributes.Control;
    //         int focusToAdd6 = this.FocusToAdd;
    //         int skillLevelToAdd6 = this.SkillLevelToAdd;
    //         int attributeLevelToAdd6 = this.AttributeLevelToAdd;
    //         CharacterCreationOnSelect onSelect6 = new CharacterCreationOnSelect(this.MandalorianVagabondOnConsequence);
    //         CharacterCreationApplyFinalEffects onApply6 =
    //             new CharacterCreationApplyFinalEffects(this.MandalorianVagabondOnApply);
    //         TextObject descriptionText6 = new TextObject(
    //             "{=2SDWhGmQ}Your family numbered among the poor migrants living in the slums that grow up outside the walls of the river cities, making whatever money they could from a variety of odd jobs. Sometimes they did services for one of the region's many criminal gangs.");
    //         creationCategory3.AddCategoryOption(text6, effectedSkills6, control1, focusToAdd6, skillLevelToAdd6,
    //             attributeLevelToAdd6, (CharacterCreationOnCondition) null, onSelect6, onApply6, descriptionText6);
    //     }

    //     private void AddSeparatistParentOptions(CharacterCreationMenu menu)
    //     {
    //         CharacterCreationCategory creationCategory2 =
    //             menu.AddMenuCategory(new CharacterCreationOnCondition(this.SeparatistParentsOnCondition));
    //         creationCategory2.AddCategoryOption(new TextObject("{=2TptWc4m}A baron's retainers"),
    //             new List<SkillObject>()
    //             {
    //                 DefaultSkills.Riding,
    //                 DefaultSkills.Polearm
    //             }, DefaultCharacterAttributes.Social, this.FocusToAdd, this.SkillLevelToAdd, this.AttributeLevelToAdd,
    //             (CharacterCreationOnCondition) null,
    //             new CharacterCreationOnSelect(this.SeparatistBaronsRetainerOnConsequence),
    //             new CharacterCreationApplyFinalEffects(this.SeparatistBaronsRetainerOnApply),
    //             new TextObject(
    //                 "{=0Suu1Q9q}Your father was a bailiff for a local feudal magnate. He looked after his liege's estates, resolved disputes in the village, and helped train the village levy. He rode with the lord's cavalry, fighting as an armored knight."));
    //         creationCategory2.AddCategoryOption(new TextObject("{=651FhzdR}Urban merchants"), new List<SkillObject>()
    //             {
    //                 DefaultSkills.Trade,
    //                 DefaultSkills.Charm
    //             }, DefaultCharacterAttributes.Intelligence, this.FocusToAdd, this.SkillLevelToAdd,
    //             this.AttributeLevelToAdd,
    //             (CharacterCreationOnCondition) null, new CharacterCreationOnSelect(this.SeparatistMerchantOnConsequence),
    //             new CharacterCreationApplyFinalEffects(this.SeparatistMerchantOnApply),
    //             new TextObject(
    //                 "{=qNZFkxJb}Your family were merchants in one of the main cities of the kingdom. They organized caravans to nearby towns, were active in the local merchant's guild."));
    //         creationCategory2.AddCategoryOption(new TextObject("{=RDfXuVxT}Yeomen"), new List<SkillObject>()
    //             {
    //                 DefaultSkills.Polearm,
    //                 DefaultSkills.Crossbow
    //             }, DefaultCharacterAttributes.Endurance, this.FocusToAdd, this.SkillLevelToAdd,
    //             this.AttributeLevelToAdd,
    //             (CharacterCreationOnCondition) null, new CharacterCreationOnSelect(this.SeparatistYeomanOnConsequence),
    //             new CharacterCreationApplyFinalEffects(this.SeparatistYeomanOnApply),
    //             new TextObject(
    //                 "{=BLZ4mdhb}Your family were small farmers with just enough land to feed themselves and make a small profit. People like them were the pillars of the kingdom's economy, as well as the backbone of the levy."));
    //         creationCategory2.AddCategoryOption(new TextObject("{=p2KIhGbE}Urban blacksmith"), new List<SkillObject>()
    //             {
    //                 DefaultSkills.Crafting,
    //                 DefaultSkills.TwoHanded
    //             }, DefaultCharacterAttributes.Vigor, this.FocusToAdd, this.SkillLevelToAdd, this.AttributeLevelToAdd,
    //             (CharacterCreationOnCondition) null, new CharacterCreationOnSelect(this.SeparatistBlacksmithOnConsequence),
    //             new CharacterCreationApplyFinalEffects(this.SeparatistBlacksmithOnApply),
    //             new TextObject(
    //                 "{=btsMpRcA}Your family owned a smithy in a city. Your father played an active if minor role in the town council, and also served in the militia."));
    //         creationCategory2.AddCategoryOption(new TextObject("{=YcnK0Thk}Hunters"), new List<SkillObject>()
    //             {
    //                 DefaultSkills.Scouting,
    //                 DefaultSkills.Crossbow
    //             }, DefaultCharacterAttributes.Control, this.FocusToAdd, this.SkillLevelToAdd, this.AttributeLevelToAdd,
    //             (CharacterCreationOnCondition) null, new CharacterCreationOnSelect(this.SeparatistHunterOnConsequence),
    //             new CharacterCreationApplyFinalEffects(this.SeparatistHunterOnApply),
    //             new TextObject(
    //                 "{=yRFSzSDZ}Your family lived in a village, but did not own their own land. Instead, your father supplemented paid jobs with long trips in the woods, hunting and trapping, always keeping a wary eye for the lord's game wardens."));
    //         creationCategory2.AddCategoryOption(new TextObject("{=ipQP6aVi}Mercenaries"), new List<SkillObject>()
    //             {
    //                 DefaultSkills.Roguery,
    //                 DefaultSkills.Crossbow
    //             }, DefaultCharacterAttributes.Cunning, this.FocusToAdd, this.SkillLevelToAdd, this.AttributeLevelToAdd,
    //             (CharacterCreationOnCondition) null, new CharacterCreationOnSelect(this.SeparatistMercenaryOnConsequence),
    //             new CharacterCreationApplyFinalEffects(this.SeparatistMercenaryOnApply),
    //             new TextObject(
    //                 "{=yYhX6JQC}Your father joined one of Vlandia's many mercenary companies, composed of men who got such a taste for war in their lord's service that they never took well to peace. Their crossbowmen were much valued across Calradia. Your mother was a camp follower, taking you along in the wake of bloody campaigns."));
    //     }

    //     private void AddRepublicParentOptions(CharacterCreationMenu menu)
    //     {
    //         CharacterCreationCategory creationCategory1 =
    //             menu.AddMenuCategory(new CharacterCreationOnCondition(this.RepublicParentsOnCondition));
    //         creationCategory1.AddCategoryOption(new TextObject("{=InN5ZZt3}A landlord's retainers"),
    //             new List<SkillObject>()
    //             {
    //                 DefaultSkills.Riding,
    //                 DefaultSkills.Polearm
    //             }, DefaultCharacterAttributes.Vigor, this.FocusToAdd, this.SkillLevelToAdd, this.AttributeLevelToAdd,
    //             (CharacterCreationOnCondition) null,
    //             new CharacterCreationOnSelect(this.RepublicLandlordsRetainerOnConsequence),
    //             new CharacterCreationApplyFinalEffects(this.RepublicLandlordsRetainerOnApply),
    //             new TextObject(
    //                 "{=ivKl4mV2}Your father was a trusted lieutenant of the local landowning aristocrat. He rode with the lord's cavalry, fighting as an armored lancer."));
    //         creationCategory1.AddCategoryOption(new TextObject("{=651FhzdR}Urban merchants"), new List<SkillObject>()
    //             {
    //                 DefaultSkills.Trade,
    //                 DefaultSkills.Charm
    //             }, DefaultCharacterAttributes.Social, this.FocusToAdd, this.SkillLevelToAdd, this.AttributeLevelToAdd,
    //             (CharacterCreationOnCondition) null, new CharacterCreationOnSelect(this.RepublicMerchantOnConsequence),
    //             new CharacterCreationApplyFinalEffects(this.RepublicMerchantOnApply),
    //             new TextObject(
    //                 "{=FQntPChs}Your family were merchants in one of the main cities of the Empire. They sometimes organized caravans to nearby towns, and discussed issues in the town council."));
    //         creationCategory1.AddCategoryOption(new TextObject("{=sb4gg8Ak}Freeholders"), new List<SkillObject>()
    //             {
    //                 DefaultSkills.Athletics,
    //                 DefaultSkills.Polearm
    //             }, DefaultCharacterAttributes.Endurance, this.FocusToAdd, this.SkillLevelToAdd,
    //             this.AttributeLevelToAdd,
    //             (CharacterCreationOnCondition) null, new CharacterCreationOnSelect(this.RepublicFreeholderOnConsequence),
    //             new CharacterCreationApplyFinalEffects(this.RepublicFreeholderOnApply),
    //             new TextObject(
    //                 "{=09z8Q08f}Your family were small farmers with just enough land to feed themselves and make a small profit. People like them were the pillars of the imperial rural economy, as well as the backbone of the levy."));
    //         creationCategory1.AddCategoryOption(new TextObject("{=v48N6h1t}Urban artisans"), new List<SkillObject>()
    //             {
    //                 DefaultSkills.Crafting,
    //                 DefaultSkills.Crossbow
    //             }, DefaultCharacterAttributes.Intelligence, this.FocusToAdd, this.SkillLevelToAdd,
    //             this.AttributeLevelToAdd,
    //             (CharacterCreationOnCondition) null, new CharacterCreationOnSelect(this.RepublicArtisanOnConsequence),
    //             new CharacterCreationApplyFinalEffects(this.RepublicArtisanOnApply),
    //             new TextObject(
    //                 "{=ZKynvffv}Your family owned their own workshop in a city, making goods from raw materials brought in from the countryside. Your father played an active if minor role in the town council, and also served in the militia."));
    //         creationCategory1.AddCategoryOption(new TextObject("{=7eWmU2mF}Foresters"), new List<SkillObject>()
    //             {
    //                 DefaultSkills.Scouting,
    //                 DefaultSkills.Bow
    //             }, DefaultCharacterAttributes.Control, this.FocusToAdd, this.SkillLevelToAdd, this.AttributeLevelToAdd,
    //             (CharacterCreationOnCondition) null, new CharacterCreationOnSelect(this.RepublicWoodsmanOnConsequence),
    //             new CharacterCreationApplyFinalEffects(this.RepublicWoodsmanOnApply),
    //             new TextObject(
    //                 "{=yRFSzSDZ}Your family lived in a village, but did not own their own land. Instead, your father supplemented paid jobs with long trips in the woods, hunting and trapping, always keeping a wary eye for the lord's game wardens."));
    //         creationCategory1.AddCategoryOption(new TextObject("{=aEke8dSb}Urban vagabonds"), new List<SkillObject>()
    //             {
    //                 DefaultSkills.Roguery,
    //                 DefaultSkills.Throwing
    //             }, DefaultCharacterAttributes.Cunning, this.FocusToAdd, this.SkillLevelToAdd, this.AttributeLevelToAdd,
    //             (CharacterCreationOnCondition) null, new CharacterCreationOnSelect(this.RepublicVagabondOnConsequence),
    //             new CharacterCreationApplyFinalEffects(this.RepublicVagabondOnApply),
    //             new TextObject(
    //                 "{=Jvf6K7TZ}Your family numbered among the many poor migrants living in the slums that grow up outside the walls of imperial cities, making whatever money they could from a variety of odd jobs. Sometimes they did service for one of the Empire's many criminal gangs, and you had an early look at the dark side of life."));
    //     }

    //     protected void AddChildhoodMenu(CharacterCreation characterCreation)
    //     {
    //         CharacterCreationMenu menu = new CharacterCreationMenu(new TextObject("{=8Yiwt1z6}Early Childhood"),
    //             new TextObject("{=character_creation_content_16}As a child you were noted for..."),
    //             new CharacterCreationOnInit(this.ChildhoodOnInit));
    //         CharacterCreationCategory creationCategory = menu.AddMenuCategory();
    //         TextObject text1 = new TextObject("{=kmM68Qx4}your leadership skills.");
    //         List<SkillObject> effectedSkills1 = new List<SkillObject>();
    //         effectedSkills1.Add(DefaultSkills.Leadership);
    //         effectedSkills1.Add(DefaultSkills.Tactics);
    //         CharacterAttribute cunning = DefaultCharacterAttributes.Cunning;
    //         int focusToAdd1 = this.FocusToAdd;
    //         int skillLevelToAdd1 = this.SkillLevelToAdd;
    //         int attributeLevelToAdd1 = this.AttributeLevelToAdd;
    //         CharacterCreationOnSelect onSelect1 =
    //             new CharacterCreationOnSelect(
    //                 ScCharacterCreationContent.ChildhoodYourLeadershipSkillsOnConsequence);
    //         CharacterCreationApplyFinalEffects onApply1 =
    //             new CharacterCreationApplyFinalEffects(ScCharacterCreationContent.ChildhoodGoodLeadingOnApply);
    //         TextObject descriptionText1 = new TextObject(
    //             "{=FfNwXtii}If the wolf pup gang of your early childhood had an alpha, it was definitely you. All the other kids followed your lead as you decided what to play and where to play, and led them in games and mischief.");
    //         creationCategory.AddCategoryOption(text1, effectedSkills1, cunning, focusToAdd1, skillLevelToAdd1,
    //             attributeLevelToAdd1, (CharacterCreationOnCondition) null, onSelect1, onApply1, descriptionText1);
    //         TextObject text2 = new TextObject("{=5HXS8HEY}your brawn.");
    //         List<SkillObject> effectedSkills2 = new List<SkillObject>();
    //         effectedSkills2.Add(DefaultSkills.TwoHanded);
    //         effectedSkills2.Add(DefaultSkills.Throwing);
    //         CharacterAttribute vigor = DefaultCharacterAttributes.Vigor;
    //         int focusToAdd2 = this.FocusToAdd;
    //         int skillLevelToAdd2 = this.SkillLevelToAdd;
    //         int attributeLevelToAdd2 = this.AttributeLevelToAdd;
    //         CharacterCreationOnSelect onSelect2 =
    //             new CharacterCreationOnSelect(ScCharacterCreationContent.ChildhoodYourBrawnOnConsequence);
    //         CharacterCreationApplyFinalEffects onApply2 =
    //             new CharacterCreationApplyFinalEffects(ScCharacterCreationContent.ChildhoodGoodAthleticsOnApply);
    //         TextObject descriptionText2 =
    //             new TextObject(
    //                 "{=YKzuGc54}You were big, and other children looked to have you around in any scrap with children from a neighboring village. You pushed a plough and throw an axe like an adult.");
    //         creationCategory.AddCategoryOption(text2, effectedSkills2, vigor, focusToAdd2, skillLevelToAdd2,
    //             attributeLevelToAdd2, (CharacterCreationOnCondition) null, onSelect2, onApply2, descriptionText2);
    //         TextObject text3 = new TextObject("{=QrYjPUEf}your attention to detail.");
    //         List<SkillObject> effectedSkills3 = new List<SkillObject>();
    //         effectedSkills3.Add(DefaultSkills.Athletics);
    //         effectedSkills3.Add(DefaultSkills.Bow);
    //         CharacterAttribute control = DefaultCharacterAttributes.Control;
    //         int focusToAdd3 = this.FocusToAdd;
    //         int skillLevelToAdd3 = this.SkillLevelToAdd;
    //         int attributeLevelToAdd3 = this.AttributeLevelToAdd;
    //         CharacterCreationOnSelect onSelect3 =
    //             new CharacterCreationOnSelect(ScCharacterCreationContent.ChildhoodAttentionToDetailOnConsequence);
    //         CharacterCreationApplyFinalEffects onApply3 =
    //             new CharacterCreationApplyFinalEffects(ScCharacterCreationContent.ChildhoodGoodMemoryOnApply);
    //         TextObject descriptionText3 = new TextObject(
    //             "{=JUSHAPnu}You were quick on your feet and attentive to what was going on around you. Usually you could run away from trouble, though you could give a good account of yourself in a fight with other children if cornered.");
    //         creationCategory.AddCategoryOption(text3, effectedSkills3, control, focusToAdd3, skillLevelToAdd3,
    //             attributeLevelToAdd3, (CharacterCreationOnCondition) null, onSelect3, onApply3, descriptionText3);
    //         TextObject text4 = new TextObject("{=Y3UcaX74}your aptitude for numbers.");
    //         List<SkillObject> effectedSkills4 = new List<SkillObject>();
    //         effectedSkills4.Add(DefaultSkills.Engineering);
    //         effectedSkills4.Add(DefaultSkills.Trade);
    //         CharacterAttribute intelligence = DefaultCharacterAttributes.Intelligence;
    //         int focusToAdd4 = this.FocusToAdd;
    //         int skillLevelToAdd4 = this.SkillLevelToAdd;
    //         int attributeLevelToAdd4 = this.AttributeLevelToAdd;
    //         CharacterCreationOnSelect onSelect4 =
    //             new CharacterCreationOnSelect(ScCharacterCreationContent.ChildhoodAptitudeForNumbersOnConsequence);
    //         CharacterCreationApplyFinalEffects onApply4 =
    //             new CharacterCreationApplyFinalEffects(ScCharacterCreationContent.ChildhoodGoodMathOnApply);
    //         TextObject descriptionText4 = new TextObject(
    //             "{=DFidSjIf}Most children around you had only the most rudimentary education, but you lingered after class to study letters and mathematics. You were fascinated by the marketplace - weights and measures, tallies and accounts, the chatter about profits and losses.");
    //         creationCategory.AddCategoryOption(text4, effectedSkills4, intelligence, focusToAdd4, skillLevelToAdd4,
    //             attributeLevelToAdd4, (CharacterCreationOnCondition) null, onSelect4, onApply4, descriptionText4);
    //         TextObject text5 = new TextObject("{=GEYzLuwb}your way with people.");
    //         List<SkillObject> effectedSkills5 = new List<SkillObject>();
    //         effectedSkills5.Add(DefaultSkills.Charm);
    //         effectedSkills5.Add(DefaultSkills.Leadership);
    //         CharacterAttribute social = DefaultCharacterAttributes.Social;
    //         int focusToAdd5 = this.FocusToAdd;
    //         int skillLevelToAdd5 = this.SkillLevelToAdd;
    //         int attributeLevelToAdd5 = this.AttributeLevelToAdd;
    //         CharacterCreationOnSelect onSelect5 =
    //             new CharacterCreationOnSelect(ScCharacterCreationContent.ChildhoodWayWithPeopleOnConsequence);
    //         CharacterCreationApplyFinalEffects onApply5 =
    //             new CharacterCreationApplyFinalEffects(ScCharacterCreationContent.ChildhoodGoodMannersOnApply);
    //         TextObject descriptionText5 =
    //             new TextObject(
    //                 "{=w2TEQq26}You were always attentive to other people, good at guessing their motivations. You studied how individuals were swayed, and tried out what you learned from adults on your friends.");
    //         creationCategory.AddCategoryOption(text5, effectedSkills5, social, focusToAdd5, skillLevelToAdd5,
    //             attributeLevelToAdd5, (CharacterCreationOnCondition) null, onSelect5, onApply5, descriptionText5);
    //         TextObject text6 = new TextObject("{=MEgLE2kj}your skill with horses.");
    //         List<SkillObject> effectedSkills6 = new List<SkillObject>();
    //         effectedSkills6.Add(DefaultSkills.Riding);
    //         effectedSkills6.Add(DefaultSkills.Medicine);
    //         CharacterAttribute endurance = DefaultCharacterAttributes.Endurance;
    //         int focusToAdd6 = this.FocusToAdd;
    //         int skillLevelToAdd6 = this.SkillLevelToAdd;
    //         int attributeLevelToAdd6 = this.AttributeLevelToAdd;
    //         CharacterCreationOnSelect onSelect6 =
    //             new CharacterCreationOnSelect(ScCharacterCreationContent.ChildhoodSkillsWithHorsesOnConsequence);
    //         CharacterCreationApplyFinalEffects onApply6 =
    //             new CharacterCreationApplyFinalEffects(ScCharacterCreationContent
    //                 .ChildhoodAffinityWithAnimalsOnApply);
    //         TextObject descriptionText6 = new TextObject(
    //             "{=ngazFofr}You were always drawn to animals, and spent as much time as possible hanging out in the village stables. You could calm horses, and were sometimes called upon to break in new colts. You learned the basics of veterinary arts, much of which is applicable to humans as well.");
    //         creationCategory.AddCategoryOption(text6, effectedSkills6, endurance, focusToAdd6, skillLevelToAdd6,
    //             attributeLevelToAdd6, (CharacterCreationOnCondition) null, onSelect6, onApply6, descriptionText6);
    //         characterCreation.AddNewMenu(menu);
    //     }

    //     protected void AddEducationMenu(CharacterCreation characterCreation)
    //     {
    //         CharacterCreationMenu menu = new CharacterCreationMenu(new TextObject("{=rcoueCmk}Adolescence"),
    //             this._educationIntroductoryText, new CharacterCreationOnInit(this.EducationOnInit));
    //         CharacterCreationCategory creationCategory = menu.AddMenuCategory();
    //         TextObject text1 = new TextObject("{=RKVNvimC}herded the sheep.");
    //         List<SkillObject> effectedSkills1 = new List<SkillObject>();
    //         effectedSkills1.Add(DefaultSkills.Athletics);
    //         effectedSkills1.Add(DefaultSkills.Throwing);
    //         CharacterAttribute control1 = DefaultCharacterAttributes.Control;
    //         int focusToAdd1 = this.FocusToAdd;
    //         int skillLevelToAdd1 = this.SkillLevelToAdd;
    //         int attributeLevelToAdd1 = this.AttributeLevelToAdd;
    //         CharacterCreationOnCondition optionCondition1 =
    //             new CharacterCreationOnCondition(this.RuralAdolescenceOnCondition);
    //         CharacterCreationOnSelect onSelect1 =
    //             new CharacterCreationOnSelect(this.RuralAdolescenceHerderOnConsequence);
    //         CharacterCreationApplyFinalEffects onApply1 =
    //             new CharacterCreationApplyFinalEffects(ScCharacterCreationContent.RuralAdolescenceHerderOnApply);
    //         TextObject descriptionText1 = new TextObject(
    //             "{=KfaqPpbK}You went with other fleet-footed youths to take the villages' sheep, goats or cattle to graze in pastures near the village. You were in charge of chasing down stray beasts, and always kept a big stone on hand to be hurled at lurking predators if necessary.");
    //         creationCategory.AddCategoryOption(text1, effectedSkills1, control1, focusToAdd1, skillLevelToAdd1,
    //             attributeLevelToAdd1, optionCondition1, onSelect1, onApply1, descriptionText1);
    //         TextObject text2 = new TextObject("{=bTKiN0hr}worked in the village smithy.");
    //         List<SkillObject> effectedSkills2 = new List<SkillObject>();
    //         effectedSkills2.Add(DefaultSkills.TwoHanded);
    //         effectedSkills2.Add(DefaultSkills.Crafting);
    //         CharacterAttribute vigor1 = DefaultCharacterAttributes.Vigor;
    //         int focusToAdd2 = this.FocusToAdd;
    //         int skillLevelToAdd2 = this.SkillLevelToAdd;
    //         int attributeLevelToAdd2 = this.AttributeLevelToAdd;
    //         CharacterCreationOnCondition optionCondition2 =
    //             new CharacterCreationOnCondition(this.RuralAdolescenceOnCondition);
    //         CharacterCreationOnSelect onSelect2 =
    //             new CharacterCreationOnSelect(this.RuralAdolescenceSmithyOnConsequence);
    //         CharacterCreationApplyFinalEffects onApply2 =
    //             new CharacterCreationApplyFinalEffects(ScCharacterCreationContent.RuralAdolescenceSmithyOnApply);
    //         TextObject descriptionText2 =
    //             new TextObject(
    //                 "{=y6j1bJTH}You were apprenticed to the local smith. You learned how to heat and forge metal, hammering for hours at a time until your muscles ached.");
    //         creationCategory.AddCategoryOption(text2, effectedSkills2, vigor1, focusToAdd2, skillLevelToAdd2,
    //             attributeLevelToAdd2, optionCondition2, onSelect2, onApply2, descriptionText2);
    //         TextObject text3 = new TextObject("{=tI8ZLtoA}repaired projects.");
    //         List<SkillObject> effectedSkills3 = new List<SkillObject>();
    //         effectedSkills3.Add(DefaultSkills.Crafting);
    //         effectedSkills3.Add(DefaultSkills.Engineering);
    //         CharacterAttribute intelligence1 = DefaultCharacterAttributes.Intelligence;
    //         int focusToAdd3 = this.FocusToAdd;
    //         int skillLevelToAdd3 = this.SkillLevelToAdd;
    //         int attributeLevelToAdd3 = this.AttributeLevelToAdd;
    //         CharacterCreationOnCondition optionCondition3 =
    //             new CharacterCreationOnCondition(this.RuralAdolescenceOnCondition);
    //         CharacterCreationOnSelect onSelect3 =
    //             new CharacterCreationOnSelect(this.RuralAdolescenceRepairmanOnConsequence);
    //         CharacterCreationApplyFinalEffects onApply3 =
    //             new CharacterCreationApplyFinalEffects(ScCharacterCreationContent
    //                 .RuralAdolescenceRepairmanOnApply);
    //         TextObject descriptionText3 =
    //             new TextObject(
    //                 "{=6LFj919J}You helped dig wells, rethatch houses, and fix broken plows. You learned about the basics of construction, as well as what it takes to keep a farming community prosperous.");
    //         creationCategory.AddCategoryOption(text3, effectedSkills3, intelligence1, focusToAdd3, skillLevelToAdd3,
    //             attributeLevelToAdd3, optionCondition3, onSelect3, onApply3, descriptionText3);
    //         TextObject text4 = new TextObject("{=TRwgSLD2}gathered herbs in the wild.");
    //         List<SkillObject> effectedSkills4 = new List<SkillObject>();
    //         effectedSkills4.Add(DefaultSkills.Medicine);
    //         effectedSkills4.Add(DefaultSkills.Scouting);
    //         CharacterAttribute endurance1 = DefaultCharacterAttributes.Endurance;
    //         int focusToAdd4 = this.FocusToAdd;
    //         int skillLevelToAdd4 = this.SkillLevelToAdd;
    //         int attributeLevelToAdd4 = this.AttributeLevelToAdd;
    //         CharacterCreationOnCondition optionCondition4 =
    //             new CharacterCreationOnCondition(this.RuralAdolescenceOnCondition);
    //         CharacterCreationOnSelect onSelect4 =
    //             new CharacterCreationOnSelect(this.RuralAdolescenceGathererOnConsequence);
    //         CharacterCreationApplyFinalEffects onApply4 =
    //             new CharacterCreationApplyFinalEffects(ScCharacterCreationContent.RuralAdolescenceGathererOnApply);
    //         TextObject descriptionText4 =
    //             new TextObject(
    //                 "{=9ks4u5cH}You were sent by the village healer up into the hills to look for useful medicinal plants. You learned which herbs healed wounds or brought down a fever, and how to find them.");
    //         creationCategory.AddCategoryOption(text4, effectedSkills4, endurance1, focusToAdd4, skillLevelToAdd4,
    //             attributeLevelToAdd4, optionCondition4, onSelect4, onApply4, descriptionText4);
    //         TextObject text5 = new TextObject("{=T7m7ReTq}hunted small game.");
    //         List<SkillObject> effectedSkills5 = new List<SkillObject>();
    //         effectedSkills5.Add(DefaultSkills.Bow);
    //         effectedSkills5.Add(DefaultSkills.Tactics);
    //         CharacterAttribute control2 = DefaultCharacterAttributes.Control;
    //         int focusToAdd5 = this.FocusToAdd;
    //         int skillLevelToAdd5 = this.SkillLevelToAdd;
    //         int attributeLevelToAdd5 = this.AttributeLevelToAdd;
    //         CharacterCreationOnCondition optionCondition5 =
    //             new CharacterCreationOnCondition(this.RuralAdolescenceOnCondition);
    //         CharacterCreationOnSelect onSelect5 =
    //             new CharacterCreationOnSelect(this.RuralAdolescenceHunterOnConsequence);
    //         CharacterCreationApplyFinalEffects onApply5 =
    //             new CharacterCreationApplyFinalEffects(ScCharacterCreationContent.RuralAdolescenceHunterOnApply);
    //         TextObject descriptionText5 =
    //             new TextObject(
    //                 "{=RuvSk3QT}You accompanied a local hunter as he went into the wilderness, helping him set up traps and catch small animals.");
    //         creationCategory.AddCategoryOption(text5, effectedSkills5, control2, focusToAdd5, skillLevelToAdd5,
    //             attributeLevelToAdd5, optionCondition5, onSelect5, onApply5, descriptionText5);
    //         TextObject text6 = new TextObject("{=qAbMagWq}sold produce at the market.");
    //         List<SkillObject> effectedSkills6 = new List<SkillObject>();
    //         effectedSkills6.Add(DefaultSkills.Trade);
    //         effectedSkills6.Add(DefaultSkills.Charm);
    //         CharacterAttribute social1 = DefaultCharacterAttributes.Social;
    //         int focusToAdd6 = this.FocusToAdd;
    //         int skillLevelToAdd6 = this.SkillLevelToAdd;
    //         int attributeLevelToAdd6 = this.AttributeLevelToAdd;
    //         CharacterCreationOnCondition optionCondition6 =
    //             new CharacterCreationOnCondition(this.RuralAdolescenceOnCondition);
    //         CharacterCreationOnSelect onSelect6 =
    //             new CharacterCreationOnSelect(this.RuralAdolescenceHelperOnConsequence);
    //         CharacterCreationApplyFinalEffects onApply6 =
    //             new CharacterCreationApplyFinalEffects(ScCharacterCreationContent.RuralAdolescenceHelperOnApply);
    //         TextObject descriptionText6 =
    //             new TextObject(
    //                 "{=DIgsfYfz}You took your family's goods to the nearest town to sell your produce and buy supplies. It was hard work, but you enjoyed the hubbub of the marketplace.");
    //         creationCategory.AddCategoryOption(text6, effectedSkills6, social1, focusToAdd6, skillLevelToAdd6,
    //             attributeLevelToAdd6, optionCondition6, onSelect6, onApply6, descriptionText6);
    //         TextObject text7 = new TextObject("{=nOfSqRnI}at the town watch's training ground.");
    //         List<SkillObject> effectedSkills7 = new List<SkillObject>();
    //         effectedSkills7.Add(DefaultSkills.Crossbow);
    //         effectedSkills7.Add(DefaultSkills.Tactics);
    //         CharacterAttribute control3 = DefaultCharacterAttributes.Control;
    //         int focusToAdd7 = this.FocusToAdd;
    //         int skillLevelToAdd7 = this.SkillLevelToAdd;
    //         int attributeLevelToAdd7 = this.AttributeLevelToAdd;
    //         CharacterCreationOnCondition optionCondition7 =
    //             new CharacterCreationOnCondition(this.UrbanAdolescenceOnCondition);
    //         CharacterCreationOnSelect onSelect7 =
    //             new CharacterCreationOnSelect(this.UrbanAdolescenceWatcherOnConsequence);
    //         CharacterCreationApplyFinalEffects onApply7 =
    //             new CharacterCreationApplyFinalEffects(ScCharacterCreationContent.UrbanAdolescenceWatcherOnApply);
    //         TextObject descriptionText7 =
    //             new TextObject(
    //                 "{=qnqdEJOv}You watched the town's watch practice shooting and perfect their plans to defend the walls in case of a siege.");
    //         creationCategory.AddCategoryOption(text7, effectedSkills7, control3, focusToAdd7, skillLevelToAdd7,
    //             attributeLevelToAdd7, optionCondition7, onSelect7, onApply7, descriptionText7);
    //         TextObject text8 = new TextObject("{=8a6dnLd2}with the alley gangs.");
    //         List<SkillObject> effectedSkills8 = new List<SkillObject>();
    //         effectedSkills8.Add(DefaultSkills.Roguery);
    //         effectedSkills8.Add(DefaultSkills.OneHanded);
    //         CharacterAttribute cunning = DefaultCharacterAttributes.Cunning;
    //         int focusToAdd8 = this.FocusToAdd;
    //         int skillLevelToAdd8 = this.SkillLevelToAdd;
    //         int attributeLevelToAdd8 = this.AttributeLevelToAdd;
    //         CharacterCreationOnCondition optionCondition8 =
    //             new CharacterCreationOnCondition(this.UrbanAdolescenceOnCondition);
    //         CharacterCreationOnSelect onSelect8 =
    //             new CharacterCreationOnSelect(this.UrbanAdolescenceGangerOnConsequence);
    //         CharacterCreationApplyFinalEffects onApply8 =
    //             new CharacterCreationApplyFinalEffects(ScCharacterCreationContent.UrbanAdolescenceGangerOnApply);
    //         TextObject descriptionText8 = new TextObject(
    //             "{=1SUTcF0J}The gang leaders who kept watch over the slums of Calradian cities were always in need of poor youth to run messages and back them up in turf wars, while thrill-seeking merchants' sons and daughters sometimes slummed it in their company as well.");
    //         creationCategory.AddCategoryOption(text8, effectedSkills8, cunning, focusToAdd8, skillLevelToAdd8,
    //             attributeLevelToAdd8, optionCondition8, onSelect8, onApply8, descriptionText8);
    //         TextObject text9 = new TextObject("{=7Hv984Sf}at docks and building sites.");
    //         List<SkillObject> effectedSkills9 = new List<SkillObject>();
    //         effectedSkills9.Add(DefaultSkills.Athletics);
    //         effectedSkills9.Add(DefaultSkills.Crafting);
    //         CharacterAttribute vigor2 = DefaultCharacterAttributes.Vigor;
    //         int focusToAdd9 = this.FocusToAdd;
    //         int skillLevelToAdd9 = this.SkillLevelToAdd;
    //         int attributeLevelToAdd9 = this.AttributeLevelToAdd;
    //         CharacterCreationOnCondition optionCondition9 =
    //             new CharacterCreationOnCondition(this.UrbanAdolescenceOnCondition);
    //         CharacterCreationOnSelect onSelect9 =
    //             new CharacterCreationOnSelect(this.UrbanAdolescenceDockerOnConsequence);
    //         CharacterCreationApplyFinalEffects onApply9 =
    //             new CharacterCreationApplyFinalEffects(ScCharacterCreationContent.UrbanAdolescenceDockerOnApply);
    //         TextObject descriptionText9 = new TextObject(
    //             "{=bhdkegZ4}All towns had their share of projects that were constantly in need of both skilled and unskilled labor. You learned how hoists and scaffolds were constructed, how planks and stones were hewn and fitted, and other skills.");
    //         creationCategory.AddCategoryOption(text9, effectedSkills9, vigor2, focusToAdd9, skillLevelToAdd9,
    //             attributeLevelToAdd9, optionCondition9, onSelect9, onApply9, descriptionText9);
    //         TextObject text10 = new TextObject("{=kbcwb5TH}in the markets and caravanserais.");
    //         List<SkillObject> effectedSkills10 = new List<SkillObject>();
    //         effectedSkills10.Add(DefaultSkills.Trade);
    //         effectedSkills10.Add(DefaultSkills.Charm);
    //         CharacterAttribute social2 = DefaultCharacterAttributes.Social;
    //         int focusToAdd10 = this.FocusToAdd;
    //         int skillLevelToAdd10 = this.SkillLevelToAdd;
    //         int attributeLevelToAdd10 = this.AttributeLevelToAdd;
    //         CharacterCreationOnCondition optionCondition10 =
    //             new CharacterCreationOnCondition(this.UrbanPoorAdolescenceOnCondition);
    //         CharacterCreationOnSelect onSelect10 =
    //             new CharacterCreationOnSelect(this.UrbanAdolescenceMarketerOnConsequence);
    //         CharacterCreationApplyFinalEffects onApply10 =
    //             new CharacterCreationApplyFinalEffects(ScCharacterCreationContent.UrbanAdolescenceMarketerOnApply);
    //         TextObject descriptionText10 =
    //             new TextObject(
    //                 "{=lLJh7WAT}You worked in the marketplace, selling trinkets and drinks to busy shoppers.");
    //         creationCategory.AddCategoryOption(text10, effectedSkills10, social2, focusToAdd10, skillLevelToAdd10,
    //             attributeLevelToAdd10, optionCondition10, onSelect10, onApply10, descriptionText10);
    //         TextObject text11 = new TextObject("{=kbcwb5TH}in the markets and caravanserais.");
    //         List<SkillObject> effectedSkills11 = new List<SkillObject>();
    //         effectedSkills11.Add(DefaultSkills.Trade);
    //         effectedSkills11.Add(DefaultSkills.Charm);
    //         CharacterAttribute social3 = DefaultCharacterAttributes.Social;
    //         int focusToAdd11 = this.FocusToAdd;
    //         int skillLevelToAdd11 = this.SkillLevelToAdd;
    //         int attributeLevelToAdd11 = this.AttributeLevelToAdd;
    //         CharacterCreationOnCondition optionCondition11 =
    //             new CharacterCreationOnCondition(this.UrbanRichAdolescenceOnCondition);
    //         CharacterCreationOnSelect onSelect11 =
    //             new CharacterCreationOnSelect(this.UrbanAdolescenceMarketerOnConsequence);
    //         CharacterCreationApplyFinalEffects onApply11 =
    //             new CharacterCreationApplyFinalEffects(ScCharacterCreationContent.UrbanAdolescenceMarketerOnApply);
    //         TextObject descriptionText11 =
    //             new TextObject(
    //                 "{=rmMcwSn8}You helped your family handle their business affairs, going down to the marketplace to make purchases and oversee the arrival of caravans.");
    //         creationCategory.AddCategoryOption(text11, effectedSkills11, social3, focusToAdd11, skillLevelToAdd11,
    //             attributeLevelToAdd11, optionCondition11, onSelect11, onApply11, descriptionText11);
    //         TextObject text12 = new TextObject("{=mfRbx5KE}reading and studying.");
    //         List<SkillObject> effectedSkills12 = new List<SkillObject>();
    //         effectedSkills12.Add(DefaultSkills.Engineering);
    //         effectedSkills12.Add(DefaultSkills.Leadership);
    //         CharacterAttribute intelligence2 = DefaultCharacterAttributes.Intelligence;
    //         int focusToAdd12 = this.FocusToAdd;
    //         int skillLevelToAdd12 = this.SkillLevelToAdd;
    //         int attributeLevelToAdd12 = this.AttributeLevelToAdd;
    //         CharacterCreationOnCondition optionCondition12 =
    //             new CharacterCreationOnCondition(this.UrbanPoorAdolescenceOnCondition);
    //         CharacterCreationOnSelect onSelect12 =
    //             new CharacterCreationOnSelect(this.UrbanAdolescenceTutorOnConsequence);
    //         CharacterCreationApplyFinalEffects onApply12 =
    //             new CharacterCreationApplyFinalEffects(ScCharacterCreationContent.UrbanAdolescenceDockerOnApply);
    //         TextObject descriptionText12 = new TextObject(
    //             "{=elQnygal}Your family scraped up the money for a rudimentary schooling and you took full advantage, reading voraciously on history, mathematics, and philosophy and discussing what you read with your tutor and classmates.");
    //         creationCategory.AddCategoryOption(text12, effectedSkills12, intelligence2, focusToAdd12, skillLevelToAdd12,
    //             attributeLevelToAdd12, optionCondition12, onSelect12, onApply12, descriptionText12);
    //         TextObject text13 = new TextObject("{=etG87fB7}with your tutor.");
    //         List<SkillObject> effectedSkills13 = new List<SkillObject>();
    //         effectedSkills13.Add(DefaultSkills.Engineering);
    //         effectedSkills13.Add(DefaultSkills.Leadership);
    //         CharacterAttribute intelligence3 = DefaultCharacterAttributes.Intelligence;
    //         int focusToAdd13 = this.FocusToAdd;
    //         int skillLevelToAdd13 = this.SkillLevelToAdd;
    //         int attributeLevelToAdd13 = this.AttributeLevelToAdd;
    //         CharacterCreationOnCondition optionCondition13 =
    //             new CharacterCreationOnCondition(this.UrbanRichAdolescenceOnCondition);
    //         CharacterCreationOnSelect onSelect13 =
    //             new CharacterCreationOnSelect(this.UrbanAdolescenceTutorOnConsequence);
    //         CharacterCreationApplyFinalEffects onApply13 =
    //             new CharacterCreationApplyFinalEffects(ScCharacterCreationContent.UrbanAdolescenceDockerOnApply);
    //         TextObject descriptionText13 = new TextObject(
    //             "{=hXl25avg}Your family arranged for a private tutor and you took full advantage, reading voraciously on history, mathematics, and philosophy and discussing what you read with your tutor and classmates.");
    //         creationCategory.AddCategoryOption(text13, effectedSkills13, intelligence3, focusToAdd13, skillLevelToAdd13,
    //             attributeLevelToAdd13, optionCondition13, onSelect13, onApply13, descriptionText13);
    //         TextObject text14 = new TextObject("{=FKpLEamz}caring for horses.");
    //         List<SkillObject> effectedSkills14 = new List<SkillObject>();
    //         effectedSkills14.Add(DefaultSkills.Riding);
    //         effectedSkills14.Add(DefaultSkills.Steward);
    //         CharacterAttribute endurance2 = DefaultCharacterAttributes.Endurance;
    //         int focusToAdd14 = this.FocusToAdd;
    //         int skillLevelToAdd14 = this.SkillLevelToAdd;
    //         int attributeLevelToAdd14 = this.AttributeLevelToAdd;
    //         CharacterCreationOnCondition optionCondition14 =
    //             new CharacterCreationOnCondition(this.UrbanRichAdolescenceOnCondition);
    //         CharacterCreationOnSelect onSelect14 =
    //             new CharacterCreationOnSelect(this.UrbanAdolescenceHorserOnConsequence);
    //         CharacterCreationApplyFinalEffects onApply14 =
    //             new CharacterCreationApplyFinalEffects(ScCharacterCreationContent.UrbanAdolescenceDockerOnApply);
    //         TextObject descriptionText14 = new TextObject(
    //             "{=Ghz90npw}Your family owned a few horses at the town stables and you took charge of their care. Many evenings you would take them out beyond the walls and gallup through the fields, racing other youth.");
    //         creationCategory.AddCategoryOption(text14, effectedSkills14, endurance2, focusToAdd14, skillLevelToAdd14,
    //             attributeLevelToAdd14, optionCondition14, onSelect14, onApply14, descriptionText14);
    //         TextObject text15 = new TextObject("{=vH7GtuuK}working at the stables.");
    //         List<SkillObject> effectedSkills15 = new List<SkillObject>();
    //         effectedSkills15.Add(DefaultSkills.Riding);
    //         effectedSkills15.Add(DefaultSkills.Steward);
    //         CharacterAttribute endurance3 = DefaultCharacterAttributes.Endurance;
    //         int focusToAdd15 = this.FocusToAdd;
    //         int skillLevelToAdd15 = this.SkillLevelToAdd;
    //         int attributeLevelToAdd15 = this.AttributeLevelToAdd;
    //         CharacterCreationOnCondition optionCondition15 =
    //             new CharacterCreationOnCondition(this.UrbanPoorAdolescenceOnCondition);
    //         CharacterCreationOnSelect onSelect15 =
    //             new CharacterCreationOnSelect(this.UrbanAdolescenceHorserOnConsequence);
    //         CharacterCreationApplyFinalEffects onApply15 =
    //             new CharacterCreationApplyFinalEffects(ScCharacterCreationContent.UrbanAdolescenceDockerOnApply);
    //         TextObject descriptionText15 = new TextObject(
    //             "{=csUq1RCC}You were employed as a hired hand at the town's stables. The overseers recognized that you had a knack for horses, and you were allowed to exercise them and sometimes even break in new steeds.");
    //         creationCategory.AddCategoryOption(text15, effectedSkills15, endurance3, focusToAdd15, skillLevelToAdd15,
    //             attributeLevelToAdd15, optionCondition15, onSelect15, onApply15, descriptionText15);
    //         characterCreation.AddNewMenu(menu);
    //     }

    //     protected void AddYouthMenu(CharacterCreation characterCreation)
    //     {
    //         CharacterCreationMenu menu = new CharacterCreationMenu(new TextObject("{=ok8lSW6M}Youth"),
    //             this._youthIntroductoryText, new CharacterCreationOnInit(this.YouthOnInit));
    //         CharacterCreationCategory creationCategory = menu.AddMenuCategory();
    //         TextObject text1 = new TextObject("{=CITG915d}joined a commander's staff.");
    //         List<SkillObject> effectedSkills1 = new List<SkillObject> {DefaultSkills.Steward, DefaultSkills.Tactics};
    //         CharacterAttribute cunning1 = DefaultCharacterAttributes.Cunning;
    //         int focusToAdd1 = this.FocusToAdd;
    //         int skillLevelToAdd1 = this.SkillLevelToAdd;
    //         int attributeLevelToAdd1 = this.AttributeLevelToAdd;
    //         CharacterCreationOnCondition optionCondition1 =
    //             new CharacterCreationOnCondition(this.YouthCommanderOnCondition);
    //         CharacterCreationOnSelect onSelect1 = new CharacterCreationOnSelect(this.YouthCommanderOnConsequence);
    //         CharacterCreationApplyFinalEffects onApply1 =
    //             new CharacterCreationApplyFinalEffects(this.YouthCommanderOnApply);
    //         TextObject descriptionText1 = new TextObject(
    //             "{=Ay0G3f7I}Your family arranged for you to be part of the staff of an imperial strategos. You were not given major responsibilities - mostly carrying messages and tending to his horse -- but it did give you a chance to see how campaigns were planned and men were deployed in battle.");
    //         creationCategory.AddCategoryOption(text1, effectedSkills1, cunning1, focusToAdd1, skillLevelToAdd1,
    //             attributeLevelToAdd1, optionCondition1, onSelect1, onApply1, descriptionText1);
    //         TextObject text2 = new TextObject("{=bhE2i6OU}served as a baron's groom.");
    //         List<SkillObject> effectedSkills2 = new List<SkillObject>();
    //         effectedSkills2.Add(DefaultSkills.Steward);
    //         effectedSkills2.Add(DefaultSkills.Tactics);
    //         CharacterAttribute cunning2 = DefaultCharacterAttributes.Cunning;
    //         int focusToAdd2 = this.FocusToAdd;
    //         int skillLevelToAdd2 = this.SkillLevelToAdd;
    //         int attributeLevelToAdd2 = this.AttributeLevelToAdd;
    //         CharacterCreationOnCondition optionCondition2 =
    //             new CharacterCreationOnCondition(this.YouthGroomOnCondition);
    //         CharacterCreationOnSelect onSelect2 = new CharacterCreationOnSelect(this.YouthGroomOnConsequence);
    //         CharacterCreationApplyFinalEffects
    //             onApply2 = new CharacterCreationApplyFinalEffects(this.YouthGroomOnApply);
    //         TextObject descriptionText2 = new TextObject(
    //             "{=iZKtGI6Y}Your family arranged for you to accompany a minor baron of the Vlandian kingdom. You were not given major responsibilities - mostly carrying messages and tending to his horse -- but it did give you a chance to see how campaigns were planned and men were deployed in battle.");
    //         creationCategory.AddCategoryOption(text2, effectedSkills2, cunning2, focusToAdd2, skillLevelToAdd2,
    //             attributeLevelToAdd2, optionCondition2, onSelect2, onApply2, descriptionText2);
    //         TextObject text3 = new TextObject("{=F2bgujPo}were a chieftain's servant.");
    //         List<SkillObject> effectedSkills3 = new List<SkillObject>();
    //         effectedSkills3.Add(DefaultSkills.Steward);
    //         effectedSkills3.Add(DefaultSkills.Tactics);
    //         CharacterAttribute cunning3 = DefaultCharacterAttributes.Cunning;
    //         int focusToAdd3 = this.FocusToAdd;
    //         int skillLevelToAdd3 = this.SkillLevelToAdd;
    //         int attributeLevelToAdd3 = this.AttributeLevelToAdd;
    //         CharacterCreationOnCondition optionCondition3 =
    //             new CharacterCreationOnCondition(this.YouthChieftainOnCondition);
    //         CharacterCreationOnSelect onSelect3 = new CharacterCreationOnSelect(this.YouthChieftainOnConsequence);
    //         CharacterCreationApplyFinalEffects onApply3 =
    //             new CharacterCreationApplyFinalEffects(this.YouthChieftainOnApply);
    //         TextObject descriptionText3 = new TextObject(
    //             "{=7AYJ3SjK}Your family arranged for you to accompany a chieftain of your people. You were not given major responsibilities - mostly carrying messages and tending to his horse -- but it did give you a chance to see how campaigns were planned and men were deployed in battle.");
    //         creationCategory.AddCategoryOption(text3, effectedSkills3, cunning3, focusToAdd3, skillLevelToAdd3,
    //             attributeLevelToAdd3, optionCondition3, onSelect3, onApply3, descriptionText3);
    //         TextObject text4 = new TextObject("{=h2KnarLL}trained with the cavalry.");
    //         List<SkillObject> effectedSkills4 = new List<SkillObject>();
    //         effectedSkills4.Add(DefaultSkills.Riding);
    //         effectedSkills4.Add(DefaultSkills.Polearm);
    //         CharacterAttribute endurance1 = DefaultCharacterAttributes.Endurance;
    //         int focusToAdd4 = this.FocusToAdd;
    //         int skillLevelToAdd4 = this.SkillLevelToAdd;
    //         int attributeLevelToAdd4 = this.AttributeLevelToAdd;
    //         CharacterCreationOnCondition optionCondition4 =
    //             new CharacterCreationOnCondition(this.YouthCavalryOnCondition);
    //         CharacterCreationOnSelect onSelect4 = new CharacterCreationOnSelect(this.YouthCavalryOnConsequence);
    //         CharacterCreationApplyFinalEffects onApply4 =
    //             new CharacterCreationApplyFinalEffects(this.YouthCavalryOnApply);
    //         TextObject descriptionText4 = new TextObject(
    //             "{=7cHsIMLP}You could never have bought the equipment on your own but you were a good enough rider so that the local lord lent you a horse and equipment. You joined the armored cavalry, training with the lance.");
    //         creationCategory.AddCategoryOption(text4, effectedSkills4, endurance1, focusToAdd4, skillLevelToAdd4,
    //             attributeLevelToAdd4, optionCondition4, onSelect4, onApply4, descriptionText4);
    //         TextObject text5 = new TextObject("{=zsC2t5Hb}trained with the hearth guard.");
    //         List<SkillObject> effectedSkills5 = new List<SkillObject>();
    //         effectedSkills5.Add(DefaultSkills.Riding);
    //         effectedSkills5.Add(DefaultSkills.Polearm);
    //         CharacterAttribute endurance2 = DefaultCharacterAttributes.Endurance;
    //         int focusToAdd5 = this.FocusToAdd;
    //         int skillLevelToAdd5 = this.SkillLevelToAdd;
    //         int attributeLevelToAdd5 = this.AttributeLevelToAdd;
    //         CharacterCreationOnCondition optionCondition5 =
    //             new CharacterCreationOnCondition(this.YouthHearthGuardOnCondition);
    //         CharacterCreationOnSelect onSelect5 = new CharacterCreationOnSelect(this.YouthHearthGuardOnConsequence);
    //         CharacterCreationApplyFinalEffects onApply5 =
    //             new CharacterCreationApplyFinalEffects(this.YouthHearthGuardOnApply);
    //         TextObject descriptionText5 =
    //             new TextObject(
    //                 "{=RmbWW6Bm}You were a big and imposing enough youth that the chief's guard allowed you to train alongside them, in preparation to join them some day.");
    //         creationCategory.AddCategoryOption(text5, effectedSkills5, endurance2, focusToAdd5, skillLevelToAdd5,
    //             attributeLevelToAdd5, optionCondition5, onSelect5, onApply5, descriptionText5);
    //         TextObject text6 = new TextObject("{=aTncHUfL}stood guard with the garrisons.");
    //         List<SkillObject> effectedSkills6 = new List<SkillObject>();
    //         effectedSkills6.Add(DefaultSkills.Crossbow);
    //         effectedSkills6.Add(DefaultSkills.Engineering);
    //         CharacterAttribute intelligence1 = DefaultCharacterAttributes.Intelligence;
    //         int focusToAdd6 = this.FocusToAdd;
    //         int skillLevelToAdd6 = this.SkillLevelToAdd;
    //         int attributeLevelToAdd6 = this.AttributeLevelToAdd;
    //         CharacterCreationOnCondition optionCondition6 =
    //             new CharacterCreationOnCondition(this.YouthGarrisonOnCondition);
    //         CharacterCreationOnSelect onSelect6 = new CharacterCreationOnSelect(this.YouthGarrisonOnConsequence);
    //         CharacterCreationApplyFinalEffects onApply6 =
    //             new CharacterCreationApplyFinalEffects(this.YouthGarrisonOnApply);
    //         TextObject descriptionText6 =
    //             new TextObject(
    //                 "{=63TAYbkx}Urban troops spend much of their time guarding the town walls. Most of their training was in missile weapons, especially useful during sieges.");
    //         creationCategory.AddCategoryOption(text6, effectedSkills6, intelligence1, focusToAdd6, skillLevelToAdd6,
    //             attributeLevelToAdd6, optionCondition6, onSelect6, onApply6, descriptionText6);
    //         TextObject text7 = new TextObject("{=aTncHUfL}stood guard with the garrisons.");
    //         List<SkillObject> effectedSkills7 = new List<SkillObject>();
    //         effectedSkills7.Add(DefaultSkills.Bow);
    //         effectedSkills7.Add(DefaultSkills.Engineering);
    //         CharacterAttribute intelligence2 = DefaultCharacterAttributes.Intelligence;
    //         int focusToAdd7 = this.FocusToAdd;
    //         int skillLevelToAdd7 = this.SkillLevelToAdd;
    //         int attributeLevelToAdd7 = this.AttributeLevelToAdd;
    //         CharacterCreationOnCondition optionCondition7 =
    //             new CharacterCreationOnCondition(this.YouthOtherGarrisonOnCondition);
    //         CharacterCreationOnSelect onSelect7 = new CharacterCreationOnSelect(this.YouthOtherGarrisonOnConsequence);
    //         CharacterCreationApplyFinalEffects onApply7 =
    //             new CharacterCreationApplyFinalEffects(this.YouthOtherGarrisonOnApply);
    //         TextObject descriptionText7 =
    //             new TextObject(
    //                 "{=1EkEElZd}Urban troops spend much of their time guarding the town walls. Most of their training was in missile.");
    //         creationCategory.AddCategoryOption(text7, effectedSkills7, intelligence2, focusToAdd7, skillLevelToAdd7,
    //             attributeLevelToAdd7, optionCondition7, onSelect7, onApply7, descriptionText7);
    //         TextObject text8 = new TextObject("{=VlXOgIX6}rode with the scouts.");
    //         List<SkillObject> effectedSkills8 = new List<SkillObject>();
    //         effectedSkills8.Add(DefaultSkills.Riding);
    //         effectedSkills8.Add(DefaultSkills.Bow);
    //         CharacterAttribute endurance3 = DefaultCharacterAttributes.Endurance;
    //         int focusToAdd8 = this.FocusToAdd;
    //         int skillLevelToAdd8 = this.SkillLevelToAdd;
    //         int attributeLevelToAdd8 = this.AttributeLevelToAdd;
    //         CharacterCreationOnCondition optionCondition8 =
    //             new CharacterCreationOnCondition(this.YouthOutridersOnCondition);
    //         CharacterCreationOnSelect onSelect8 = new CharacterCreationOnSelect(this.YouthOutridersOnConsequence);
    //         CharacterCreationApplyFinalEffects onApply8 =
    //             new CharacterCreationApplyFinalEffects(this.YouthOutridersOnApply);
    //         TextObject descriptionText8 = new TextObject(
    //             "{=888lmJqs}All of Calradia's kingdoms recognize the value of good light cavalry and horse archers, and are sure to recruit nomads and borderers with the skills to fulfill those duties. You were a good enough rider that your neighbors pitched in to buy you a small pony and a good bow so that you could fulfill their levy obligations.");
    //         creationCategory.AddCategoryOption(text8, effectedSkills8, endurance3, focusToAdd8, skillLevelToAdd8,
    //             attributeLevelToAdd8, optionCondition8, onSelect8, onApply8, descriptionText8);
    //         TextObject text9 = new TextObject("{=VlXOgIX6}rode with the scouts.");
    //         List<SkillObject> effectedSkills9 = new List<SkillObject>();
    //         effectedSkills9.Add(DefaultSkills.Riding);
    //         effectedSkills9.Add(DefaultSkills.Bow);
    //         CharacterAttribute endurance4 = DefaultCharacterAttributes.Endurance;
    //         int focusToAdd9 = this.FocusToAdd;
    //         int skillLevelToAdd9 = this.SkillLevelToAdd;
    //         int attributeLevelToAdd9 = this.AttributeLevelToAdd;
    //         CharacterCreationOnCondition optionCondition9 =
    //             new CharacterCreationOnCondition(this.YouthOtherOutridersOnCondition);
    //         CharacterCreationOnSelect onSelect9 = new CharacterCreationOnSelect(this.YouthOtherOutridersOnConsequence);
    //         CharacterCreationApplyFinalEffects onApply9 =
    //             new CharacterCreationApplyFinalEffects(this.YouthOtherOutridersOnApply);
    //         TextObject descriptionText9 = new TextObject(
    //             "{=sYuN6hPD}All of Calradia's kingdoms recognize the value of good light cavalry, and are sure to recruit nomads and borderers with the skills to fulfill those duties. You were a good enough rider that your neighbors pitched in to buy you a small pony and a sheaf of javelins so that you could fulfill their levy obligations.");
    //         creationCategory.AddCategoryOption(text9, effectedSkills9, endurance4, focusToAdd9, skillLevelToAdd9,
    //             attributeLevelToAdd9, optionCondition9, onSelect9, onApply9, descriptionText9);
    //         TextObject text10 = new TextObject("{=a8arFSra}trained with the infantry.");
    //         List<SkillObject> effectedSkills10 = new List<SkillObject>();
    //         effectedSkills10.Add(DefaultSkills.Polearm);
    //         effectedSkills10.Add(DefaultSkills.OneHanded);
    //         CharacterAttribute vigor = DefaultCharacterAttributes.Vigor;
    //         int focusToAdd10 = this.FocusToAdd;
    //         int skillLevelToAdd10 = this.SkillLevelToAdd;
    //         int attributeLevelToAdd10 = this.AttributeLevelToAdd;
    //         CharacterCreationOnSelect onSelect10 = new CharacterCreationOnSelect(this.YouthInfantryOnConsequence);
    //         CharacterCreationApplyFinalEffects onApply10 =
    //             new CharacterCreationApplyFinalEffects(this.YouthInfantryOnApply);
    //         TextObject descriptionText10 =
    //             new TextObject(
    //                 "{=afH90aNs}Levy armed with spear and shield, drawn from smallholding farmers, have always been the backbone of most armies of Calradia.");
    //         creationCategory.AddCategoryOption(text10, effectedSkills10, vigor, focusToAdd10, skillLevelToAdd10,
    //             attributeLevelToAdd10, (CharacterCreationOnCondition) null, onSelect10, onApply10, descriptionText10);
    //         TextObject text11 = new TextObject("{=oMbOIPc9}joined the skirmishers.");
    //         List<SkillObject> effectedSkills11 = new List<SkillObject>();
    //         effectedSkills11.Add(DefaultSkills.Throwing);
    //         effectedSkills11.Add(DefaultSkills.OneHanded);
    //         CharacterAttribute control1 = DefaultCharacterAttributes.Control;
    //         int focusToAdd11 = this.FocusToAdd;
    //         int skillLevelToAdd11 = this.SkillLevelToAdd;
    //         int attributeLevelToAdd11 = this.AttributeLevelToAdd;
    //         CharacterCreationOnCondition optionCondition10 =
    //             new CharacterCreationOnCondition(this.YouthSkirmisherOnCondition);
    //         CharacterCreationOnSelect onSelect11 = new CharacterCreationOnSelect(this.YouthSkirmisherOnConsequence);
    //         CharacterCreationApplyFinalEffects onApply11 =
    //             new CharacterCreationApplyFinalEffects(this.YouthSkirmisherOnApply);
    //         TextObject descriptionText11 = new TextObject(
    //             "{=bXAg5w19}Younger recruits, or those of a slighter build, or those too poor to buy shield and armor tend to join the skirmishers. Fighting with bow and javelin, they try to stay out of reach of the main enemy forces.");
    //         creationCategory.AddCategoryOption(text11, effectedSkills11, control1, focusToAdd11, skillLevelToAdd11,
    //             attributeLevelToAdd11, optionCondition10, onSelect11, onApply11, descriptionText11);
    //         TextObject text12 = new TextObject("{=cDWbwBwI}joined the kern.");
    //         List<SkillObject> effectedSkills12 = new List<SkillObject>();
    //         effectedSkills12.Add(DefaultSkills.Throwing);
    //         effectedSkills12.Add(DefaultSkills.OneHanded);
    //         CharacterAttribute control2 = DefaultCharacterAttributes.Control;
    //         int focusToAdd12 = this.FocusToAdd;
    //         int skillLevelToAdd12 = this.SkillLevelToAdd;
    //         int attributeLevelToAdd12 = this.AttributeLevelToAdd;
    //         CharacterCreationOnCondition optionCondition11 =
    //             new CharacterCreationOnCondition(this.YouthKernOnCondition);
    //         CharacterCreationOnSelect onSelect12 = new CharacterCreationOnSelect(this.YouthKernOnConsequence);
    //         CharacterCreationApplyFinalEffects
    //             onApply12 = new CharacterCreationApplyFinalEffects(this.YouthKernOnApply);
    //         TextObject descriptionText12 =
    //             new TextObject(
    //                 "{=tTb28jyU}Many Battanians fight as kern, versatile troops who could both harass the enemy line with their javelins or join in the final screaming charge once it weakened.");
    //         creationCategory.AddCategoryOption(text12, effectedSkills12, control2, focusToAdd12, skillLevelToAdd12,
    //             attributeLevelToAdd12, optionCondition11, onSelect12, onApply12, descriptionText12);
    //         TextObject text13 = new TextObject("{=GFUggps8}marched with the camp followers.");
    //         List<SkillObject> effectedSkills13 = new List<SkillObject>();
    //         effectedSkills13.Add(DefaultSkills.Roguery);
    //         effectedSkills13.Add(DefaultSkills.Throwing);
    //         CharacterAttribute cunning4 = DefaultCharacterAttributes.Cunning;
    //         int focusToAdd13 = this.FocusToAdd;
    //         int skillLevelToAdd13 = this.SkillLevelToAdd;
    //         int attributeLevelToAdd13 = this.AttributeLevelToAdd;
    //         CharacterCreationOnCondition optionCondition12 =
    //             new CharacterCreationOnCondition(this.YouthCamperOnCondition);
    //         CharacterCreationOnSelect onSelect13 = new CharacterCreationOnSelect(this.YouthCamperOnConsequence);
    //         CharacterCreationApplyFinalEffects onApply13 =
    //             new CharacterCreationApplyFinalEffects(this.YouthCamperOnApply);
    //         TextObject descriptionText13 = new TextObject(
    //             "{=64rWqBLN}You avoided service with one of the main forces of your realm's armies, but followed instead in the train - the troops' wives, lovers and servants, and those who make their living by caring for, entertaining, or cheating the soldiery.");
    //         creationCategory.AddCategoryOption(text13, effectedSkills13, cunning4, focusToAdd13, skillLevelToAdd13,
    //             attributeLevelToAdd13, optionCondition12, onSelect13, onApply13, descriptionText13);
    //         characterCreation.AddNewMenu(menu);
    //     }

    //     protected void AddAdulthoodMenu(CharacterCreation characterCreation)
    //     {
    //         MBTextManager.SetTextVariable("EXP_VALUE", this.SkillLevelToAdd);
    //         CharacterCreationMenu menu = new CharacterCreationMenu(new TextObject("{=MafIe9yI}Young Adulthood"),
    //             new TextObject(
    //                 "{=4WYY0X59}Before you set out for a life of adventure, your biggest achievement was..."),
    //             new CharacterCreationOnInit(this.AccomplishmentOnInit));
    //         CharacterCreationCategory creationCategory = menu.AddMenuCategory();
    //         TextObject text1 = new TextObject("{=8bwpVpgy}you defeated an enemy in battle.");
    //         List<SkillObject> effectedSkills1 = new List<SkillObject>
    //         {
    //             DefaultSkills.OneHanded, DefaultSkills.TwoHanded
    //         };
    //         CharacterAttribute vigor = DefaultCharacterAttributes.Vigor;
    //         int focusToAdd1 = this.FocusToAdd;
    //         int skillLevelToAdd1 = this.SkillLevelToAdd;
    //         int attributeLevelToAdd1 = this.AttributeLevelToAdd;
    //         CharacterCreationOnSelect onSelect1 =
    //             new CharacterCreationOnSelect(this.AccomplishmentDefeatedEnemyOnConsequence);
    //         CharacterCreationApplyFinalEffects onApply1 =
    //             new CharacterCreationApplyFinalEffects(this.AccomplishmentDefeatedEnemyOnApply);
    //         TextObject descriptionText1 = new TextObject(
    //             "{=1IEroJKs}Not everyone who musters for the levy marches to war, and not everyone who goes on campaign sees action. You did both, and you also took down an enemy warrior in direct one-to-one combat, in the full view of your comrades.");
    //         creationCategory.AddCategoryOption(text1, effectedSkills1, vigor, focusToAdd1, skillLevelToAdd1,
    //             attributeLevelToAdd1, (CharacterCreationOnCondition) null, onSelect1, onApply1, descriptionText1,
    //             new List<TraitObject>()
    //             {
    //                 DefaultTraits.Valor
    //             }, 1, 20);
    //         TextObject text2 = new TextObject("{=mP3uFbcq}you led a successful manhunt.");
    //         List<SkillObject> effectedSkills2 = new List<SkillObject> {DefaultSkills.Tactics, DefaultSkills.Leadership};
    //         CharacterAttribute cunning1 = DefaultCharacterAttributes.Cunning;
    //         int focusToAdd2 = this.FocusToAdd;
    //         int skillLevelToAdd2 = this.SkillLevelToAdd;
    //         int attributeLevelToAdd2 = this.AttributeLevelToAdd;
    //         CharacterCreationOnCondition optionCondition1 =
    //             new CharacterCreationOnCondition(this.AccomplishmentPosseOnConditions);
    //         CharacterCreationOnSelect onSelect2 =
    //             new CharacterCreationOnSelect(this.AccomplishmentExpeditionOnConsequence);
    //         CharacterCreationApplyFinalEffects onApply2 =
    //             new CharacterCreationApplyFinalEffects(this.AccomplishmentExpeditionOnApply);
    //         TextObject descriptionText2 = new TextObject(
    //             "{=4f5xwzX0}When your community needed to organize a posse to pursue horse thieves, you were the obvious choice. You hunted down the raiders, surrounded them and forced their surrender, and took back your stolen property.");
    //         creationCategory.AddCategoryOption(text2, effectedSkills2, cunning1, focusToAdd2, skillLevelToAdd2,
    //             attributeLevelToAdd2, optionCondition1, onSelect2, onApply2, descriptionText2, new List<TraitObject>()
    //             {
    //                 DefaultTraits.Calculating
    //             }, 1, 10);
    //         TextObject text3 = new TextObject("{=wfbtS71d}you led a caravan.");
    //         List<SkillObject> effectedSkills3 = new List<SkillObject>();
    //         effectedSkills3.Add(DefaultSkills.Tactics);
    //         effectedSkills3.Add(DefaultSkills.Leadership);
    //         CharacterAttribute cunning2 = DefaultCharacterAttributes.Cunning;
    //         int focusToAdd3 = this.FocusToAdd;
    //         int skillLevelToAdd3 = this.SkillLevelToAdd;
    //         int attributeLevelToAdd3 = this.AttributeLevelToAdd;
    //         CharacterCreationOnCondition optionCondition2 =
    //             new CharacterCreationOnCondition(this.AccomplishmentMerchantOnCondition);
    //         CharacterCreationOnSelect onSelect3 =
    //             new CharacterCreationOnSelect(this.AccomplishmentMerchantOnConsequence);
    //         CharacterCreationApplyFinalEffects onApply3 =
    //             new CharacterCreationApplyFinalEffects(this.AccomplishmentExpeditionOnApply);
    //         TextObject descriptionText3 = new TextObject(
    //             "{=joRHKCkm}Your family needed someone trustworthy to take a caravan to a neighboring town. You organized supplies, ensured a constant watch to keep away bandits, and brought it safely to its destination.");
    //         creationCategory.AddCategoryOption(text3, effectedSkills3, cunning2, focusToAdd3, skillLevelToAdd3,
    //             attributeLevelToAdd3, optionCondition2, onSelect3, onApply3, descriptionText3, new List<TraitObject>()
    //             {
    //                 DefaultTraits.Calculating
    //             }, 1, 10);
    //         TextObject text4 = new TextObject("{=x1HTX5hq}you saved your village from a flood.");
    //         List<SkillObject> effectedSkills4 = new List<SkillObject>();
    //         effectedSkills4.Add(DefaultSkills.Tactics);
    //         effectedSkills4.Add(DefaultSkills.Leadership);
    //         CharacterAttribute cunning3 = DefaultCharacterAttributes.Cunning;
    //         int focusToAdd4 = this.FocusToAdd;
    //         int skillLevelToAdd4 = this.SkillLevelToAdd;
    //         int attributeLevelToAdd4 = this.AttributeLevelToAdd;
    //         CharacterCreationOnCondition optionCondition3 =
    //             new CharacterCreationOnCondition(this.AccomplishmentSavedVillageOnCondition);
    //         CharacterCreationOnSelect onSelect4 =
    //             new CharacterCreationOnSelect(this.AccomplishmentSavedVillageOnConsequence);
    //         CharacterCreationApplyFinalEffects onApply4 =
    //             new CharacterCreationApplyFinalEffects(this.AccomplishmentExpeditionOnApply);
    //         TextObject descriptionText4 =
    //             new TextObject(
    //                 "{=bWlmGDf3}When a sudden storm caused the local stream to rise suddenly, your neighbors needed quick-thinking leadership. You provided it, directing them to build levees to save their homes.");
    //         creationCategory.AddCategoryOption(text4, effectedSkills4, cunning3, focusToAdd4, skillLevelToAdd4,
    //             attributeLevelToAdd4, optionCondition3, onSelect4, onApply4, descriptionText4, new List<TraitObject>()
    //             {
    //                 DefaultTraits.Calculating
    //             }, 1, 10);
    //         TextObject text5 = new TextObject("{=s8PNllPN}you saved your city quarter from a fire.");
    //         List<SkillObject> effectedSkills5 = new List<SkillObject>();
    //         effectedSkills5.Add(DefaultSkills.Tactics);
    //         effectedSkills5.Add(DefaultSkills.Leadership);
    //         CharacterAttribute cunning4 = DefaultCharacterAttributes.Cunning;
    //         int focusToAdd5 = this.FocusToAdd;
    //         int skillLevelToAdd5 = this.SkillLevelToAdd;
    //         int attributeLevelToAdd5 = this.AttributeLevelToAdd;
    //         CharacterCreationOnCondition optionCondition4 =
    //             new CharacterCreationOnCondition(this.AccomplishmentSavedStreetOnCondition);
    //         CharacterCreationOnSelect onSelect5 =
    //             new CharacterCreationOnSelect(this.AccomplishmentSavedStreetOnConsequence);
    //         CharacterCreationApplyFinalEffects onApply5 =
    //             new CharacterCreationApplyFinalEffects(this.AccomplishmentExpeditionOnApply);
    //         TextObject descriptionText5 = new TextObject(
    //             "{=ZAGR6PYc}When a sudden blaze broke out in a back alley, your neighbors needed quick-thinking leadership and you provided it. You organized a bucket line to the nearest well, putting the fire out before any homes were lost.");
    //         creationCategory.AddCategoryOption(text5, effectedSkills5, cunning4, focusToAdd5, skillLevelToAdd5,
    //             attributeLevelToAdd5, optionCondition4, onSelect5, onApply5, descriptionText5, new List<TraitObject>()
    //             {
    //                 DefaultTraits.Calculating
    //             }, 1, 10);
    //         TextObject text6 = new TextObject("{=xORjDTal}you invested some money in a workshop.");
    //         List<SkillObject> effectedSkills6 = new List<SkillObject>();
    //         effectedSkills6.Add(DefaultSkills.Trade);
    //         effectedSkills6.Add(DefaultSkills.Crafting);
    //         CharacterAttribute intelligence1 = DefaultCharacterAttributes.Intelligence;
    //         int focusToAdd6 = this.FocusToAdd;
    //         int skillLevelToAdd6 = this.SkillLevelToAdd;
    //         int attributeLevelToAdd6 = this.AttributeLevelToAdd;
    //         CharacterCreationOnCondition optionCondition5 =
    //             new CharacterCreationOnCondition(this.AccomplishmentUrbanOnCondition);
    //         CharacterCreationOnSelect onSelect6 =
    //             new CharacterCreationOnSelect(this.AccomplishmentWorkshopOnConsequence);
    //         CharacterCreationApplyFinalEffects onApply6 =
    //             new CharacterCreationApplyFinalEffects(this.AccomplishmentWorkshopOnApply);
    //         TextObject descriptionText6 = new TextObject(
    //             "{=PyVqDLBu}Your parents didn't give you much money, but they did leave just enough for you to secure a loan against a larger amount to build a small workshop. You paid back what you borrowed, and sold your enterprise for a profit.");
    //         creationCategory.AddCategoryOption(text6, effectedSkills6, intelligence1, focusToAdd6, skillLevelToAdd6,
    //             attributeLevelToAdd6, optionCondition5, onSelect6, onApply6, descriptionText6, new List<TraitObject>()
    //             {
    //                 DefaultTraits.Calculating
    //             }, 1, 10);
    //         TextObject text7 = new TextObject("{=xKXcqRJI}you invested some money in land.");
    //         List<SkillObject> effectedSkills7 = new List<SkillObject>();
    //         effectedSkills7.Add(DefaultSkills.Trade);
    //         effectedSkills7.Add(DefaultSkills.Crafting);
    //         CharacterAttribute intelligence2 = DefaultCharacterAttributes.Intelligence;
    //         int focusToAdd7 = this.FocusToAdd;
    //         int skillLevelToAdd7 = this.SkillLevelToAdd;
    //         int attributeLevelToAdd7 = this.AttributeLevelToAdd;
    //         CharacterCreationOnCondition optionCondition6 =
    //             new CharacterCreationOnCondition(this.AccomplishmentRuralOnCondition);
    //         CharacterCreationOnSelect onSelect7 =
    //             new CharacterCreationOnSelect(this.AccomplishmentWorkshopOnConsequence);
    //         CharacterCreationApplyFinalEffects onApply7 =
    //             new CharacterCreationApplyFinalEffects(this.AccomplishmentWorkshopOnApply);
    //         TextObject descriptionText7 = new TextObject(
    //             "{=cbF9jdQo}Your parents didn't give you much money, but they did leave just enough for you to purchase a plot of unused land at the edge of the village. You cleared away rocks and dug an irrigation ditch, raised a few seasons of crops, than sold it for a considerable profit.");
    //         creationCategory.AddCategoryOption(text7, effectedSkills7, intelligence2, focusToAdd7, skillLevelToAdd7,
    //             attributeLevelToAdd7, optionCondition6, onSelect7, onApply7, descriptionText7, new List<TraitObject>()
    //             {
    //                 DefaultTraits.Calculating
    //             }, 1, 10);
    //         TextObject text8 = new TextObject("{=TbNRtUjb}you hunted a dangerous animal.");
    //         List<SkillObject> effectedSkills8 = new List<SkillObject>();
    //         effectedSkills8.Add(DefaultSkills.Polearm);
    //         effectedSkills8.Add(DefaultSkills.Crossbow);
    //         CharacterAttribute control1 = DefaultCharacterAttributes.Control;
    //         int focusToAdd8 = this.FocusToAdd;
    //         int skillLevelToAdd8 = this.SkillLevelToAdd;
    //         int attributeLevelToAdd8 = this.AttributeLevelToAdd;
    //         CharacterCreationOnCondition optionCondition7 =
    //             new CharacterCreationOnCondition(this.AccomplishmentRuralOnCondition);
    //         CharacterCreationOnSelect onSelect8 =
    //             new CharacterCreationOnSelect(this.AccomplishmentSiegeHunterOnConsequence);
    //         CharacterCreationApplyFinalEffects onApply8 =
    //             new CharacterCreationApplyFinalEffects(this.AccomplishmentSiegeHunterOnApply);
    //         TextObject descriptionText8 = new TextObject(
    //             "{=I3PcdaaL}Wolves, bears are a constant menace to the flocks of northern Calradia, while hyenas and leopards trouble the south. You went with a group of your fellow villagers and fired the missile that brought down the beast.");
    //         creationCategory.AddCategoryOption(text8, effectedSkills8, control1, focusToAdd8, skillLevelToAdd8,
    //             attributeLevelToAdd8, optionCondition7, onSelect8, onApply8, descriptionText8, renownToAdd: 5);
    //         TextObject text9 = new TextObject("{=WbHfGCbd}you survived a siege.");
    //         List<SkillObject> effectedSkills9 = new List<SkillObject>();
    //         effectedSkills9.Add(DefaultSkills.Bow);
    //         effectedSkills9.Add(DefaultSkills.Crossbow);
    //         CharacterAttribute control2 = DefaultCharacterAttributes.Control;
    //         int focusToAdd9 = this.FocusToAdd;
    //         int skillLevelToAdd9 = this.SkillLevelToAdd;
    //         int attributeLevelToAdd9 = this.AttributeLevelToAdd;
    //         CharacterCreationOnCondition optionCondition8 =
    //             new CharacterCreationOnCondition(this.AccomplishmentUrbanOnCondition);
    //         CharacterCreationOnSelect onSelect9 =
    //             new CharacterCreationOnSelect(this.AccomplishmentSiegeHunterOnConsequence);
    //         CharacterCreationApplyFinalEffects onApply9 =
    //             new CharacterCreationApplyFinalEffects(this.AccomplishmentSiegeHunterOnApply);
    //         TextObject descriptionText9 = new TextObject(
    //             "{=FhZPjhli}Your hometown was briefly placed under siege, and you were called to defend the walls. Everyone did their part to repulse the enemy assault, and everyone is justly proud of what they endured.");
    //         creationCategory.AddCategoryOption(text9, effectedSkills9, control2, focusToAdd9, skillLevelToAdd9,
    //             attributeLevelToAdd9, optionCondition8, onSelect9, onApply9, descriptionText9, renownToAdd: 5);
    //         TextObject text10 = new TextObject("{=kNXet6Um}you had a famous escapade in town.");
    //         List<SkillObject> effectedSkills10 = new List<SkillObject>();
    //         effectedSkills10.Add(DefaultSkills.Athletics);
    //         effectedSkills10.Add(DefaultSkills.Roguery);
    //         CharacterAttribute endurance1 = DefaultCharacterAttributes.Endurance;
    //         int focusToAdd10 = this.FocusToAdd;
    //         int skillLevelToAdd10 = this.SkillLevelToAdd;
    //         int attributeLevelToAdd10 = this.AttributeLevelToAdd;
    //         CharacterCreationOnCondition optionCondition9 =
    //             new CharacterCreationOnCondition(this.AccomplishmentRuralOnCondition);
    //         CharacterCreationOnSelect onSelect10 =
    //             new CharacterCreationOnSelect(this.AccomplishmentEscapadeOnConsequence);
    //         CharacterCreationApplyFinalEffects onApply10 =
    //             new CharacterCreationApplyFinalEffects(this.AccomplishmentEscapadeOnApply);
    //         TextObject descriptionText10 = new TextObject(
    //             "{=DjeAJtix}Maybe it was a love affair, or maybe you cheated at dice, or maybe you just chose your words poorly when drinking with a dangerous crowd. Anyway, on one of your trips into town you got into the kind of trouble from which only a quick tongue or quick feet get you out alive.");
    //         creationCategory.AddCategoryOption(text10, effectedSkills10, endurance1, focusToAdd10, skillLevelToAdd10,
    //             attributeLevelToAdd10, optionCondition9, onSelect10, onApply10, descriptionText10,
    //             new List<TraitObject>()
    //             {
    //                 DefaultTraits.Valor
    //             }, 1, 5);
    //         TextObject text11 = new TextObject("{=qlOuiKXj}you had a famous escapade.");
    //         List<SkillObject> effectedSkills11 = new List<SkillObject>();
    //         effectedSkills11.Add(DefaultSkills.Athletics);
    //         effectedSkills11.Add(DefaultSkills.Roguery);
    //         CharacterAttribute endurance2 = DefaultCharacterAttributes.Endurance;
    //         int focusToAdd11 = this.FocusToAdd;
    //         int skillLevelToAdd11 = this.SkillLevelToAdd;
    //         int attributeLevelToAdd11 = this.AttributeLevelToAdd;
    //         CharacterCreationOnCondition optionCondition10 =
    //             new CharacterCreationOnCondition(this.AccomplishmentUrbanOnCondition);
    //         CharacterCreationOnSelect onSelect11 =
    //             new CharacterCreationOnSelect(this.AccomplishmentEscapadeOnConsequence);
    //         CharacterCreationApplyFinalEffects onApply11 =
    //             new CharacterCreationApplyFinalEffects(this.AccomplishmentEscapadeOnApply);
    //         TextObject descriptionText11 = new TextObject(
    //             "{=lD5Ob3R4}Maybe it was a love affair, or maybe you cheated at dice, or maybe you just chose your words poorly when drinking with a dangerous crowd. Anyway, you got into the kind of trouble from which only a quick tongue or quick feet get you out alive.");
    //         creationCategory.AddCategoryOption(text11, effectedSkills11, endurance2, focusToAdd11, skillLevelToAdd11,
    //             attributeLevelToAdd11, optionCondition10, onSelect11, onApply11, descriptionText11,
    //             new List<TraitObject>()
    //             {
    //                 DefaultTraits.Valor
    //             }, 1, 5);
    //         TextObject text12 = new TextObject("{=Yqm0Dics}you treated people well.");
    //         List<SkillObject> effectedSkills12 = new List<SkillObject>();
    //         effectedSkills12.Add(DefaultSkills.Charm);
    //         effectedSkills12.Add(DefaultSkills.Steward);
    //         CharacterAttribute social = DefaultCharacterAttributes.Social;
    //         int focusToAdd12 = this.FocusToAdd;
    //         int skillLevelToAdd12 = this.SkillLevelToAdd;
    //         int attributeLevelToAdd12 = this.AttributeLevelToAdd;
    //         CharacterCreationOnSelect onSelect12 =
    //             new CharacterCreationOnSelect(this.AccomplishmentTreaterOnConsequence);
    //         CharacterCreationApplyFinalEffects onApply12 =
    //             new CharacterCreationApplyFinalEffects(this.AccomplishmentTreaterOnApply);
    //         TextObject descriptionText12 = new TextObject(
    //             "{=dDmcqTzb}Yours wasn't the kind of reputation that local legends are made of, but it was the kind that wins you respect among those around you. You were consistently fair and honest in your business dealings and helpful to those in trouble. In doing so, you got a sense of what made people tick.");
    //         creationCategory.AddCategoryOption(text12, effectedSkills12, social, focusToAdd12, skillLevelToAdd12,
    //             attributeLevelToAdd12, (CharacterCreationOnCondition) null, onSelect12, onApply12, descriptionText12,
    //             new List<TraitObject>()
    //             {
    //                 DefaultTraits.Mercy,
    //                 DefaultTraits.Generosity,
    //                 DefaultTraits.Honor
    //             }, 1, 5);
    //         characterCreation.AddNewMenu(menu);
    //     }

    //     protected void AddAgeSelectionMenu(CharacterCreation characterCreation)
    //     {
    //         MBTextManager.SetTextVariable("EXP_VALUE", this.SkillLevelToAdd);
    //         CharacterCreationMenu menu = new CharacterCreationMenu(new TextObject("{=HDFEAYDk}Starting Age"),
    //             new TextObject("{=VlOGrGSn}Your character started off on the adventuring path at the age of..."),
    //             new CharacterCreationOnInit(this.StartingAgeOnInit));
    //         CharacterCreationCategory creationCategory = menu.AddMenuCategory();
    //         creationCategory.AddCategoryOption(new TextObject("{=!}20"), new List<SkillObject>(),
    //             (CharacterAttribute) null, 0, 0, 0, (CharacterCreationOnCondition) null,
    //             new CharacterCreationOnSelect(this.StartingAgeYoungOnConsequence),
    //             new CharacterCreationApplyFinalEffects(this.StartingAgeYoungOnApply),
    //             new TextObject(
    //                 "{=2k7adlh7}While lacking experience a bit, you are full with youthful energy, you are fully eager, for the long years of adventuring ahead."),
    //             unspentFocusPoint: 2, unspentAttributePoint: 1);
    //         creationCategory.AddCategoryOption(new TextObject("{=!}30"), new List<SkillObject>(),
    //             (CharacterAttribute) null, 0, 0, 0, (CharacterCreationOnCondition) null,
    //             new CharacterCreationOnSelect(this.StartingAgeAdultOnConsequence),
    //             new CharacterCreationApplyFinalEffects(this.StartingAgeAdultOnApply),
    //             new TextObject(
    //                 "{=NUlVFRtK}You are at your prime, You still have some youthful energy but also have a substantial amount of experience under your belt. "),
    //             unspentFocusPoint: 4, unspentAttributePoint: 2);
    //         creationCategory.AddCategoryOption(new TextObject("{=!}40"), new List<SkillObject>(),
    //             (CharacterAttribute) null, 0, 0, 0, (CharacterCreationOnCondition) null,
    //             new CharacterCreationOnSelect(this.StartingAgeMiddleAgedOnConsequence),
    //             new CharacterCreationApplyFinalEffects(this.StartingAgeMiddleAgedOnApply),
    //             new TextObject(
    //                 "{=5MxTYApM}This is the right age for starting off, you have years of experience, and you are old enough for people to respect you and gather under your banner."),
    //             unspentFocusPoint: 6, unspentAttributePoint: 3);
    //         creationCategory.AddCategoryOption(new TextObject("{=!}50"), new List<SkillObject>(),
    //             (CharacterAttribute) null, 0, 0, 0, (CharacterCreationOnCondition) null,
    //             new CharacterCreationOnSelect(this.StartingAgeElderlyOnConsequence),
    //             new CharacterCreationApplyFinalEffects(this.StartingAgeElderlyOnApply),
    //             new TextObject(
    //                 "{=ePD5Afvy}While you are past your prime, there is still enough time to go on that last big adventure for you. And you have all the experience you need to overcome anything!"),
    //             unspentFocusPoint: 8, unspentAttributePoint: 4);
    //         characterCreation.AddNewMenu(menu);
    //     }

    //     protected void ParentsOnInit(CharacterCreation characterCreation)
    //     {
    //         characterCreation.IsPlayerAlone = false;
    //         characterCreation.HasSecondaryCharacter = false;
    //         ScCharacterCreationContent.ClearMountEntity(characterCreation);
    //         characterCreation.ClearFaceGenPrefab();
    //         if (this.PlayerBodyProperties !=
    //             CharacterObject.PlayerCharacter.GetBodyProperties(CharacterObject.PlayerCharacter.Equipment, -1))
    //         {
    //             this.PlayerBodyProperties =
    //                 CharacterObject.PlayerCharacter.GetBodyProperties(CharacterObject.PlayerCharacter.Equipment, -1);
    //             BodyProperties motherBodyProperties = this.PlayerBodyProperties;
    //             BodyProperties fatherBodyProperties = this.PlayerBodyProperties;
    //             FaceGen.GenerateParentKey(this.PlayerBodyProperties, 0,ref motherBodyProperties,
    //                 ref fatherBodyProperties);
    //             motherBodyProperties = new BodyProperties(new DynamicBodyProperties(33f, 0.3f, 0.2f),
    //                 motherBodyProperties.StaticProperties);
    //             fatherBodyProperties = new BodyProperties(new DynamicBodyProperties(33f, 0.5f, 0.5f),
    //                 fatherBodyProperties.StaticProperties);
    //             this.MotherFacegenCharacter =
    //                 new FaceGenChar(motherBodyProperties, 0, new Equipment(), true, "anim_mother_1");
    //             this.FatherFacegenCharacter =
    //                 new FaceGenChar(fatherBodyProperties, 0, new Equipment(), false, "anim_father_1");
    //         }

    //         characterCreation.ChangeFaceGenChars(new List<FaceGenChar>()
    //         {
    //             this.MotherFacegenCharacter,
    //             this.FatherFacegenCharacter
    //         });
    //         this.ChangeParentsOutfit(characterCreation);
    //         this.ChangeParentsAnimation(characterCreation);
    //     }

    //     protected void ChangeParentsOutfit(
    //         CharacterCreation characterCreation,
    //         string fatherItemId = "",
    //         string motherItemId = "",
    //         bool isLeftHandItemForFather = true,
    //         bool isLeftHandItemForMother = true)
    //     {
    //         characterCreation.ClearFaceGenPrefab();
    //         List<Equipment> equipmentList = new List<Equipment>();
    //         Equipment equipment1 =
    //             Game.Current.ObjectManager.GetObject<MBEquipmentRoster>("mother_char_creation_" +
    //                                                                     (object) this.SelectedParentType + "_" +
    //                                                                     this.GetSelectedCulture().StringId)
    //                 ?.DefaultEquipment ?? MBEquipmentRoster.EmptyEquipment;
    //         Equipment equipment2 =
    //             Game.Current.ObjectManager.GetObject<MBEquipmentRoster>("father_char_creation_" +
    //                                                                     (object) this.SelectedParentType + "_" +
    //                                                                     this.GetSelectedCulture().StringId)
    //                 ?.DefaultEquipment ?? MBEquipmentRoster.EmptyEquipment;
    //         if (motherItemId != "")
    //         {
    //             ItemObject itemObject = Game.Current.ObjectManager.GetObject<ItemObject>(motherItemId);
    //             if (itemObject != null)
    //                 equipment1.AddEquipmentToSlotWithoutAgent(
    //                     isLeftHandItemForMother ? EquipmentIndex.WeaponItemBeginSlot : EquipmentIndex.Weapon1,
    //                     new EquipmentElement(itemObject));
    //             else
    //                 characterCreation.ChangeCharacterPrefab(motherItemId,
    //                     isLeftHandItemForMother
    //                         ? Game.Current.DefaultMonster.MainHandItemBoneIndex
    //                         : Game.Current.DefaultMonster.OffHandItemBoneIndex);
    //         }

    //         if (fatherItemId != "")
    //         {
    //             ItemObject itemObject = Game.Current.ObjectManager.GetObject<ItemObject>(fatherItemId);
    //             if (itemObject != null)
    //                 equipment2.AddEquipmentToSlotWithoutAgent(
    //                     isLeftHandItemForFather ? EquipmentIndex.WeaponItemBeginSlot : EquipmentIndex.Weapon1,
    //                     new EquipmentElement(itemObject));
    //         }

    //         equipmentList.Add(equipment1);
    //         equipmentList.Add(equipment2);
    //         characterCreation.ChangeCharactersEquipment(equipmentList);
    //     }

    //     protected void ChangeParentsAnimation(CharacterCreation characterCreation) =>
    //         characterCreation.ChangeCharsAnimation(new List<string>()
    //         {
    //             "anim_mother_" + (object) this.SelectedParentType,
    //             "anim_father_" + (object) this.SelectedParentType
    //         });

    //     protected void SetParentAndOccupationType(
    //         CharacterCreation characterCreation,
    //         int parentType,
    //         ScCharacterCreationContent.OccupationTypes occupationType,
    //         string fatherItemId = "",
    //         string motherItemId = "",
    //         bool isLeftHandItemForFather = true,
    //         bool isLeftHandItemForMother = true)
    //     {
    //         this.SelectedParentType = parentType;
    //         this._familyOccupationType = occupationType;
    //         characterCreation.ChangeFaceGenChars(new List<FaceGenChar>()
    //         {
    //             this.MotherFacegenCharacter,
    //             this.FatherFacegenCharacter
    //         });
    //         this.ChangeParentsAnimation(characterCreation);
    //         this.ChangeParentsOutfit(characterCreation, fatherItemId, motherItemId, isLeftHandItemForFather,
    //             isLeftHandItemForMother);
    //     }

    //     protected void RepublicLandlordsRetainerOnConsequence(CharacterCreation characterCreation) =>
    //         this.SetParentAndOccupationType(characterCreation, 1,
    //             ScCharacterCreationContent.OccupationTypes.Retainer);

    //     protected void RepublicMerchantOnConsequence(CharacterCreation characterCreation) =>
    //         this.SetParentAndOccupationType(characterCreation, 2,
    //             ScCharacterCreationContent.OccupationTypes.Merchant);

    //     protected void RepublicFreeholderOnConsequence(CharacterCreation characterCreation) =>
    //         this.SetParentAndOccupationType(characterCreation, 3,
    //             ScCharacterCreationContent.OccupationTypes.Farmer);

    //     protected void RepublicArtisanOnConsequence(CharacterCreation characterCreation) =>
    //         this.SetParentAndOccupationType(characterCreation, 4,
    //             ScCharacterCreationContent.OccupationTypes.Artisan);

    //     protected void RepublicWoodsmanOnConsequence(CharacterCreation characterCreation) =>
    //         this.SetParentAndOccupationType(characterCreation, 5,
    //             ScCharacterCreationContent.OccupationTypes.Hunter);

    //     protected void RepublicVagabondOnConsequence(CharacterCreation characterCreation) =>
    //         this.SetParentAndOccupationType(characterCreation, 6,
    //             ScCharacterCreationContent.OccupationTypes.Vagabond);

    //     protected void RepublicLandlordsRetainerOnApply(CharacterCreation characterCreation) => this.FinalizeParents();

    //     protected void RepublicMerchantOnApply(CharacterCreation characterCreation) => this.FinalizeParents();

    //     protected void RepublicFreeholderOnApply(CharacterCreation characterCreation) => this.FinalizeParents();

    //     protected void RepublicArtisanOnApply(CharacterCreation characterCreation) => this.FinalizeParents();

    //     protected void RepublicWoodsmanOnApply(CharacterCreation characterCreation) => this.FinalizeParents();

    //     protected void RepublicVagabondOnApply(CharacterCreation characterCreation) => this.FinalizeParents();

    //     protected void SeparatistBaronsRetainerOnConsequence(CharacterCreation characterCreation) =>
    //         this.SetParentAndOccupationType(characterCreation, 1,
    //             ScCharacterCreationContent.OccupationTypes.Retainer);

    //     protected void SeparatistMerchantOnConsequence(CharacterCreation characterCreation) =>
    //         this.SetParentAndOccupationType(characterCreation, 2,
    //             ScCharacterCreationContent.OccupationTypes.Merchant);

    //     protected void SeparatistYeomanOnConsequence(CharacterCreation characterCreation) =>
    //         this.SetParentAndOccupationType(characterCreation, 3,
    //             ScCharacterCreationContent.OccupationTypes.Farmer);

    //     protected void SeparatistBlacksmithOnConsequence(CharacterCreation characterCreation) =>
    //         this.SetParentAndOccupationType(characterCreation, 4,
    //             ScCharacterCreationContent.OccupationTypes.Artisan);

    //     protected void SeparatistHunterOnConsequence(CharacterCreation characterCreation) =>
    //         this.SetParentAndOccupationType(characterCreation, 5,
    //             ScCharacterCreationContent.OccupationTypes.Hunter);

    //     protected void SeparatistMercenaryOnConsequence(CharacterCreation characterCreation) =>
    //         this.SetParentAndOccupationType(characterCreation, 6,
    //             ScCharacterCreationContent.OccupationTypes.Mercenary);

    //     protected void SeparatistBaronsRetainerOnApply(CharacterCreation characterCreation) => this.FinalizeParents();

    //     protected void SeparatistMerchantOnApply(CharacterCreation characterCreation) => this.FinalizeParents();

    //     protected void SeparatistYeomanOnApply(CharacterCreation characterCreation) => this.FinalizeParents();

    //     protected void SeparatistBlacksmithOnApply(CharacterCreation characterCreation) => this.FinalizeParents();

    //     protected void SeparatistHunterOnApply(CharacterCreation characterCreation) => this.FinalizeParents();

    //     protected void SeparatistMercenaryOnApply(CharacterCreation characterCreation) => this.FinalizeParents();

    //     protected void MandalorianBoyarsCompanionOnConsequence(CharacterCreation characterCreation) =>
    //         this.SetParentAndOccupationType(characterCreation, 1,
    //             ScCharacterCreationContent.OccupationTypes.Retainer);

    //     protected void MandalorianTraderOnConsequence(CharacterCreation characterCreation) =>
    //         this.SetParentAndOccupationType(characterCreation, 2,
    //             ScCharacterCreationContent.OccupationTypes.Merchant);

    //     protected void MandalorianFreemanOnConsequence(CharacterCreation characterCreation) =>
    //         this.SetParentAndOccupationType(characterCreation, 3,
    //             ScCharacterCreationContent.OccupationTypes.Farmer);

    //     protected void MandalorianArtisanOnConsequence(CharacterCreation characterCreation) =>
    //         this.SetParentAndOccupationType(characterCreation, 4,
    //             ScCharacterCreationContent.OccupationTypes.Artisan);

    //     protected void MandalorianHunterOnConsequence(CharacterCreation characterCreation) =>
    //         this.SetParentAndOccupationType(characterCreation, 5,
    //             ScCharacterCreationContent.OccupationTypes.Hunter);

    //     protected void MandalorianVagabondOnConsequence(CharacterCreation characterCreation) =>
    //         this.SetParentAndOccupationType(characterCreation, 6,
    //             ScCharacterCreationContent.OccupationTypes.Vagabond);

    //     protected void MandalorianBoyarsCompanionOnApply(CharacterCreation characterCreation) => this.FinalizeParents();

    //     protected void MandalorianTraderOnApply(CharacterCreation characterCreation) => this.FinalizeParents();

    //     protected void MandalorianFreemanOnApply(CharacterCreation characterCreation) => this.FinalizeParents();

    //     protected void MandalorianArtisanOnApply(CharacterCreation characterCreation) => this.FinalizeParents();

    //     protected void MandalorianHunterOnApply(CharacterCreation characterCreation) => this.FinalizeParents();

    //     protected void MandalorianVagabondOnApply(CharacterCreation characterCreation) => this.FinalizeParents();

    //     protected bool RepublicParentsOnCondition() => this.GetSelectedCulture().StringId == "galactic_republic";
        
    //     protected bool SeparatistParentsOnCondition() => this.GetSelectedCulture().StringId == "separatistalliance";
        
    //     protected bool MandalorianParentsOnCondition() => this.GetSelectedCulture().StringId == "mandalorians";

    //     protected void FinalizeParents()
    //     {
    //         CharacterObject character1 = Game.Current.ObjectManager.GetObject<CharacterObject>("main_hero_mother");
    //         CharacterObject character2 = Game.Current.ObjectManager.GetObject<CharacterObject>("main_hero_father");
    //         character1.HeroObject.ModifyPlayersFamilyAppearance(this.MotherFacegenCharacter.BodyProperties
    //             .StaticProperties);
    //         character2.HeroObject.ModifyPlayersFamilyAppearance(this.FatherFacegenCharacter.BodyProperties
    //             .StaticProperties);
    //         character1.HeroObject.Weight = this.MotherFacegenCharacter.BodyProperties.Weight;
    //         character1.HeroObject.Build = this.MotherFacegenCharacter.BodyProperties.Build;
    //         character2.HeroObject.Weight = this.FatherFacegenCharacter.BodyProperties.Weight;
    //         character2.HeroObject.Build = this.FatherFacegenCharacter.BodyProperties.Build;
    //         EquipmentHelper.AssignHeroEquipmentFromEquipment(character1.HeroObject,
    //             this.MotherFacegenCharacter.Equipment);
    //         EquipmentHelper.AssignHeroEquipmentFromEquipment(character2.HeroObject,
    //             this.FatherFacegenCharacter.Equipment);
    //         character1.Culture = Hero.MainHero.Culture;
    //         character2.Culture = Hero.MainHero.Culture;
    //         StringHelpers.SetCharacterProperties("PLAYER", CharacterObject.PlayerCharacter);
    //         TextObject text1 = GameTexts.FindText("str_player_father_name", Hero.MainHero.Culture.StringId);
    //         character2.HeroObject.SetName(text1, text1);
    //         TextObject parent1 =
    //             new TextObject(
    //                 "{=XmvaRfLM}{PLAYER_FATHER.NAME} was the father of {PLAYER.LINK}. He was slain when raiders attacked the inn at which his family was staying.");
    //         StringHelpers.SetCharacterProperties("PLAYER_FATHER", character2, parent1);
    //         character2.HeroObject.EncyclopediaText = parent1;
    //         TextObject text2 = GameTexts.FindText("str_player_mother_name", Hero.MainHero.Culture.StringId);
    //         character1.HeroObject.SetName(text2, text2);
    //         TextObject parent2 =
    //             new TextObject(
    //                 "{=hrhvEWP8}{PLAYER_MOTHER.NAME} was the mother of {PLAYER.LINK}. She was slain when raiders attacked the inn at which her family was staying.");
    //         StringHelpers.SetCharacterProperties("PLAYER_MOTHER", character1, parent2);
    //         character1.HeroObject.EncyclopediaText = parent2;
    //         character1.HeroObject.UpdateHomeSettlement();
    //         character2.HeroObject.UpdateHomeSettlement();
    //         character1.HeroObject.HasMet = true;
    //         character2.HeroObject.HasMet = true;
    //     }

    //     protected static List<FaceGenChar> ChangePlayerFaceWithAge(
    //         float age,
    //         string actionName = "act_childhood_schooled")
    //     {
    //         List<FaceGenChar> faceGenCharList = new List<FaceGenChar>();
    //         BodyProperties originalBodyProperties =
    //             CharacterObject.PlayerCharacter.GetBodyProperties(CharacterObject.PlayerCharacter.Equipment, -1);
    //         originalBodyProperties = FaceGen.GetBodyPropertiesWithAge(ref originalBodyProperties, age);
    //         faceGenCharList.Add(new FaceGenChar(originalBodyProperties, 0,new Equipment(),
    //             CharacterObject.PlayerCharacter.IsFemale, actionName));
    //         return faceGenCharList;
    //     }

    //     protected Equipment ChangePlayerOutfit(
    //         CharacterCreation characterCreation,
    //         string outfit)
    //     {
    //         List<Equipment> equipmentList = new List<Equipment>();
    //         Equipment defaultEquipment =
    //             Game.Current.ObjectManager.GetObject<MBEquipmentRoster>(outfit)?.DefaultEquipment;
    //         if (defaultEquipment == null)
    //         {
    //             Debug.FailedAssert("item shouldn't be null! Check this!",
    //                 "C:\\Develop\\mb3\\Source\\Bannerlord\\TaleWorlds.CampaignSystem\\CharacterCreationContent\\ScCharacterCreationContent.cs",
    //                 nameof(ChangePlayerOutfit), 1039);
    //             defaultEquipment = Game.Current.ObjectManager
    //                 .GetObject<MBEquipmentRoster>("player_char_creation_default").DefaultEquipment;
    //         }

    //         equipmentList.Add(defaultEquipment);
    //         characterCreation.ChangeCharactersEquipment(equipmentList);
    //         return defaultEquipment;
    //     }

    //     protected static void ChangePlayerMount(CharacterCreation characterCreation, Hero hero)
    //     {
    //         if (!hero.CharacterObject.HasMount())
    //             return;
            
    //         ItemObject mountItem = hero.CharacterObject.Equipment[EquipmentIndex.ArmorItemEndSlot].Item;
    //         FaceGenMount newMount = new (
    //             MountCreationKey.GetRandomMountKey(mountItem, hero.CharacterObject.GetMountKeySeed()),
    //             hero.CharacterObject.Equipment[EquipmentIndex.ArmorItemEndSlot].Item,
    //             hero.CharacterObject.Equipment[EquipmentIndex.HorseHarness].Item, "act_horse_stand_1");
            
    //         characterCreation.SetFaceGenMount(newMount);
    //     }

    //     protected static void ClearMountEntity(CharacterCreation characterCreation) =>
    //         characterCreation.ClearFaceGenMounts();

    //     protected void ChildhoodOnInit(CharacterCreation characterCreation)
    //     {
    //         characterCreation.IsPlayerAlone = true;
    //         characterCreation.HasSecondaryCharacter = false;
    //         characterCreation.ClearFaceGenPrefab();
    //         characterCreation.ChangeFaceGenChars(
    //             ScCharacterCreationContent.ChangePlayerFaceWithAge((float) this.ChildhoodAge));
    //         string outfit = "player_char_creation_childhood_age_" + this.GetSelectedCulture().StringId + "_" +
    //                         (object) this.SelectedParentType + (Hero.MainHero.IsFemale ? "_f" : "_m");
    //         this.ChangePlayerOutfit(characterCreation, outfit);
    //         characterCreation.ChangeCharsAnimation(new List<string>()
    //         {
    //             "act_childhood_schooled"
    //         });
    //         ScCharacterCreationContent.ClearMountEntity(characterCreation);
    //     }

    //     protected static void ChildhoodYourLeadershipSkillsOnConsequence(
    //         CharacterCreation characterCreation)
    //     {
    //         characterCreation.ChangeCharsAnimation(new List<string>()
    //         {
    //             "act_childhood_leader"
    //         });
    //     }

    //     protected static void ChildhoodYourBrawnOnConsequence(CharacterCreation characterCreation) =>
    //         characterCreation.ChangeCharsAnimation(new List<string>()
    //         {
    //             "act_childhood_athlete"
    //         });

    //     protected static void ChildhoodAttentionToDetailOnConsequence(
    //         CharacterCreation characterCreation)
    //     {
    //         characterCreation.ChangeCharsAnimation(new List<string>()
    //         {
    //             "act_childhood_memory"
    //         });
    //     }

    //     protected static void ChildhoodAptitudeForNumbersOnConsequence(
    //         CharacterCreation characterCreation)
    //     {
    //         characterCreation.ChangeCharsAnimation(new List<string>()
    //         {
    //             "act_childhood_numbers"
    //         });
    //     }

    //     protected static void ChildhoodWayWithPeopleOnConsequence(CharacterCreation characterCreation) =>
    //         characterCreation.ChangeCharsAnimation(new List<string>()
    //         {
    //             "act_childhood_manners"
    //         });

    //     protected static void ChildhoodSkillsWithHorsesOnConsequence(CharacterCreation characterCreation) =>
    //         characterCreation.ChangeCharsAnimation(new List<string>()
    //         {
    //             "act_childhood_animals"
    //         });

    //     protected static void ChildhoodGoodLeadingOnApply(CharacterCreation characterCreation)
    //     {
    //     }

    //     protected static void ChildhoodGoodAthleticsOnApply(CharacterCreation characterCreation)
    //     {
    //     }

    //     protected static void ChildhoodGoodMemoryOnApply(CharacterCreation characterCreation)
    //     {
    //     }

    //     protected static void ChildhoodGoodMathOnApply(CharacterCreation characterCreation)
    //     {
    //     }

    //     protected static void ChildhoodGoodMannersOnApply(CharacterCreation characterCreation)
    //     {
    //     }

    //     protected static void ChildhoodAffinityWithAnimalsOnApply(CharacterCreation characterCreation)
    //     {
    //     }

    //     protected void EducationOnInit(CharacterCreation characterCreation)
    //     {
    //         characterCreation.IsPlayerAlone = true;
    //         characterCreation.HasSecondaryCharacter = false;
    //         characterCreation.ClearFaceGenPrefab();
    //         TextObject textObject1 =
    //             new TextObject("{=WYvnWcXQ}Like all village children you helped out in the fields. You also...");
    //         TextObject textObject2 = new TextObject("{=DsCkf6Pb}Growing up, you spent most of your time...");
    //         this._educationIntroductoryText.SetTextVariable("EDUCATION_INTRO",
    //             this.RuralType() ? textObject1 : textObject2);
    //         characterCreation.ChangeFaceGenChars(
    //             ScCharacterCreationContent.ChangePlayerFaceWithAge((float) this.EducationAge));
    //         string outfit = "player_char_creation_education_age_" + this.GetSelectedCulture().StringId + "_" +
    //                         (object) this.SelectedParentType + (Hero.MainHero.IsFemale ? "_f" : "_m");
    //         this.ChangePlayerOutfit(characterCreation, outfit);
    //         characterCreation.ChangeCharsAnimation(new List<string>()
    //         {
    //             "act_childhood_schooled"
    //         });
    //         ScCharacterCreationContent.ClearMountEntity(characterCreation);
    //     }

    //     protected bool RuralType() =>
    //         this._familyOccupationType == ScCharacterCreationContent.OccupationTypes.Retainer ||
    //         this._familyOccupationType == ScCharacterCreationContent.OccupationTypes.Farmer ||
    //         (this._familyOccupationType == ScCharacterCreationContent.OccupationTypes.Hunter ||
    //          this._familyOccupationType == ScCharacterCreationContent.OccupationTypes.Bard) ||
    //         (this._familyOccupationType == ScCharacterCreationContent.OccupationTypes.Herder ||
    //          this._familyOccupationType == ScCharacterCreationContent.OccupationTypes.Vagabond ||
    //          this._familyOccupationType == ScCharacterCreationContent.OccupationTypes.Healer) ||
    //         this._familyOccupationType == ScCharacterCreationContent.OccupationTypes.Artisan;

    //     protected bool RichParents() =>
    //         this._familyOccupationType == ScCharacterCreationContent.OccupationTypes.Retainer ||
    //         this._familyOccupationType == ScCharacterCreationContent.OccupationTypes.Merchant;

    //     protected bool RuralAdolescenceOnCondition() => this.RuralType();

    //     protected bool UrbanAdolescenceOnCondition() => !this.RuralType();

    //     protected bool UrbanRichAdolescenceOnCondition() => !this.RuralType() && this.RichParents();

    //     protected bool UrbanPoorAdolescenceOnCondition() => !this.RuralType() && !this.RichParents();

    //     protected void RefreshPropsAndClothing(
    //         CharacterCreation characterCreation,
    //         bool isChildhoodStage,
    //         string itemId,
    //         bool isLeftHand,
    //         string secondItemId = "")
    //     {
    //         characterCreation.ClearFaceGenPrefab();
    //         characterCreation.ClearCharactersEquipment();
    //         string str;
    //         if (!isChildhoodStage)
    //             str = "player_char_creation_education_age_" + this.GetSelectedCulture().StringId + "_" +
    //                   (object) this.SelectedParentType;
    //         else
    //             str = "player_char_creation_childhood_age_" + this.GetSelectedCulture().StringId + "_" +
    //                   (object) this.SelectedParentType;
    //         string outfit = str + (Hero.MainHero.IsFemale ? "_f" : "_m");
    //         Equipment equipment = this.ChangePlayerOutfit(characterCreation, outfit).Clone();
    //         if (Game.Current.ObjectManager.GetObject<ItemObject>(itemId) != null)
    //         {
    //             ItemObject itemObject1 = Game.Current.ObjectManager.GetObject<ItemObject>(itemId);
    //             equipment.AddEquipmentToSlotWithoutAgent(
    //                 isLeftHand ? EquipmentIndex.WeaponItemBeginSlot : EquipmentIndex.Weapon1,
    //                 new EquipmentElement(itemObject1));
    //             if (secondItemId != "")
    //             {
    //                 ItemObject itemObject2 = Game.Current.ObjectManager.GetObject<ItemObject>(secondItemId);
    //                 equipment.AddEquipmentToSlotWithoutAgent(
    //                     isLeftHand ? EquipmentIndex.Weapon1 : EquipmentIndex.WeaponItemBeginSlot,
    //                     new EquipmentElement(itemObject2));
    //             }
    //         }
    //         else
    //             characterCreation.ChangeCharacterPrefab(itemId,
    //                 isLeftHand
    //                     ? Game.Current.DefaultMonster.MainHandItemBoneIndex
    //                     : Game.Current.DefaultMonster.OffHandItemBoneIndex);

    //         characterCreation.ChangeCharactersEquipment(new List<Equipment>()
    //         {
    //             equipment
    //         });
    //     }

    //     protected void RuralAdolescenceHerderOnConsequence(CharacterCreation characterCreation)
    //     {
    //         characterCreation.ChangeCharsAnimation(new List<string>()
    //         {
    //             "act_childhood_streets"
    //         });
    //         this.RefreshPropsAndClothing(characterCreation, false, "carry_bostaff_rogue1", true);
    //     }

    //     protected void RuralAdolescenceSmithyOnConsequence(CharacterCreation characterCreation)
    //     {
    //         characterCreation.ChangeCharsAnimation(new List<string>()
    //         {
    //             "act_childhood_militia"
    //         });
    //         this.RefreshPropsAndClothing(characterCreation, false, "peasant_hammer_1_t1", true);
    //     }

    //     protected void RuralAdolescenceRepairmanOnConsequence(CharacterCreation characterCreation)
    //     {
    //         characterCreation.ChangeCharsAnimation(new List<string>()
    //         {
    //             "act_childhood_grit"
    //         });
    //         this.RefreshPropsAndClothing(characterCreation, false, "carry_hammer", true);
    //     }

    //     protected void RuralAdolescenceGathererOnConsequence(CharacterCreation characterCreation)
    //     {
    //         characterCreation.ChangeCharsAnimation(new List<string>()
    //         {
    //             "act_childhood_peddlers"
    //         });
    //         this.RefreshPropsAndClothing(characterCreation, false, "_to_carry_bd_basket_a", true);
    //     }

    //     protected void RuralAdolescenceHunterOnConsequence(CharacterCreation characterCreation)
    //     {
    //         characterCreation.ChangeCharsAnimation(new List<string>()
    //         {
    //             "act_childhood_sharp"
    //         });
    //         this.RefreshPropsAndClothing(characterCreation, false, "composite_bow", true);
    //     }

    //     protected void RuralAdolescenceHelperOnConsequence(CharacterCreation characterCreation)
    //     {
    //         characterCreation.ChangeCharsAnimation(new List<string>()
    //         {
    //             "act_childhood_peddlers_2"
    //         });
    //         this.RefreshPropsAndClothing(characterCreation, false, "_to_carry_bd_fabric_c", true);
    //     }

    //     protected void UrbanAdolescenceWatcherOnConsequence(CharacterCreation characterCreation)
    //     {
    //         characterCreation.ChangeCharsAnimation(new List<string>()
    //         {
    //             "act_childhood_fox"
    //         });
    //         this.RefreshPropsAndClothing(characterCreation, false, "", true);
    //     }

    //     protected void UrbanAdolescenceMarketerOnConsequence(CharacterCreation characterCreation)
    //     {
    //         characterCreation.ChangeCharsAnimation(new List<string>()
    //         {
    //             "act_childhood_manners"
    //         });
    //         this.RefreshPropsAndClothing(characterCreation, false, "", true);
    //     }

    //     protected void UrbanAdolescenceGangerOnConsequence(CharacterCreation characterCreation)
    //     {
    //         characterCreation.ChangeCharsAnimation(new List<string>()
    //         {
    //             "act_childhood_athlete"
    //         });
    //         this.RefreshPropsAndClothing(characterCreation, false, "", true);
    //     }

    //     protected void UrbanAdolescenceDockerOnConsequence(CharacterCreation characterCreation)
    //     {
    //         characterCreation.ChangeCharsAnimation(new List<string>()
    //         {
    //             "act_childhood_peddlers"
    //         });
    //         this.RefreshPropsAndClothing(characterCreation, false, "_to_carry_bd_basket_a", true);
    //     }

    //     protected void UrbanAdolescenceHorserOnConsequence(CharacterCreation characterCreation)
    //     {
    //         characterCreation.ChangeCharsAnimation(new List<string>()
    //         {
    //             "act_childhood_peddlers_2"
    //         });
    //         this.RefreshPropsAndClothing(characterCreation, false, "_to_carry_bd_fabric_c", true);
    //     }

    //     protected void UrbanAdolescenceTutorOnConsequence(CharacterCreation characterCreation)
    //     {
    //         characterCreation.ChangeCharsAnimation(new List<string>()
    //         {
    //             "act_childhood_book"
    //         });
    //         this.RefreshPropsAndClothing(characterCreation, false, "character_creation_notebook", false);
    //     }

    //     protected static void RuralAdolescenceHerderOnApply(CharacterCreation characterCreation)
    //     {
    //     }

    //     protected static void RuralAdolescenceSmithyOnApply(CharacterCreation characterCreation)
    //     {
    //     }

    //     protected static void RuralAdolescenceRepairmanOnApply(CharacterCreation characterCreation)
    //     {
    //     }

    //     protected static void RuralAdolescenceGathererOnApply(CharacterCreation characterCreation)
    //     {
    //     }

    //     protected static void RuralAdolescenceHunterOnApply(CharacterCreation characterCreation)
    //     {
    //     }

    //     protected static void RuralAdolescenceHelperOnApply(CharacterCreation characterCreation)
    //     {
    //     }

    //     protected static void UrbanAdolescenceWatcherOnApply(CharacterCreation characterCreation)
    //     {
    //     }

    //     protected static void UrbanAdolescenceMarketerOnApply(CharacterCreation characterCreation)
    //     {
    //     }

    //     protected static void UrbanAdolescenceGangerOnApply(CharacterCreation characterCreation)
    //     {
    //     }

    //     protected static void UrbanAdolescenceDockerOnApply(CharacterCreation characterCreation)
    //     {
    //     }

    //     protected void YouthOnInit(CharacterCreation characterCreation)
    //     {
    //         characterCreation.IsPlayerAlone = true;
    //         characterCreation.HasSecondaryCharacter = false;
    //         characterCreation.ClearFaceGenPrefab();
    //         TextObject textObject1 =
    //             new TextObject("{=F7OO5SAa}As a youngster growing up in Calradia, war was never too far away. You...");
    //         TextObject textObject2 =
    //             new TextObject(
    //                 "{=5kbeAC7k}In wartorn Calradia, especially in frontier or tribal areas, some women as well as men learn to fight from an early age. You...");
    //         this._youthIntroductoryText.SetTextVariable("YOUTH_INTRO",
    //             CharacterObject.PlayerCharacter.IsFemale ? textObject2 : textObject1);
    //         characterCreation.ChangeFaceGenChars(
    //             ScCharacterCreationContent.ChangePlayerFaceWithAge((float) this.YouthAge));
    //         characterCreation.ChangeCharsAnimation(new List<string>()
    //         {
    //             "act_childhood_schooled"
    //         });
    //         if (this.SelectedTitleType < 1 || this.SelectedTitleType > 10)
    //             this.SelectedTitleType = 1;
    //         this.RefreshPlayerAppearance(characterCreation);
    //     }

    //     protected void RefreshPlayerAppearance(CharacterCreation characterCreation)
    //     {
    //         string outfit = "player_char_creation_" + this.GetSelectedCulture().StringId + "_" +
    //                         (object) this.SelectedTitleType + (Hero.MainHero.IsFemale ? "_f" : "_m");
    //         this.ChangePlayerOutfit(characterCreation, outfit);
    //         this.ApplyEquipments(characterCreation);
    //     }

    //     protected bool YouthCommanderOnCondition() => this.GetSelectedCulture().StringId == "empire" &&
    //                                                   this._familyOccupationType == ScCharacterCreationContent
    //                                                       .OccupationTypes.Retainer;

    //     protected void YouthCommanderOnApply(CharacterCreation characterCreation)
    //     {
    //     }

    //     protected bool YouthGroomOnCondition() => this.GetSelectedCulture().StringId == "vlandia" &&
    //                                               this._familyOccupationType == ScCharacterCreationContent
    //                                                   .OccupationTypes.Retainer;

    //     protected void YouthCommanderOnConsequence(CharacterCreation characterCreation)
    //     {
    //         this.SelectedTitleType = 10;
    //         this.RefreshPlayerAppearance(characterCreation);
    //         characterCreation.ChangeCharsAnimation(new List<string>()
    //         {
    //             "act_childhood_decisive"
    //         });
    //     }

    //     protected void YouthGroomOnConsequence(CharacterCreation characterCreation)
    //     {
    //         this.SelectedTitleType = 10;
    //         this.RefreshPlayerAppearance(characterCreation);
    //         characterCreation.ChangeCharsAnimation(new List<string>()
    //         {
    //             "act_childhood_sharp"
    //         });
    //     }

    //     protected void YouthChieftainOnConsequence(CharacterCreation characterCreation)
    //     {
    //         this.SelectedTitleType = 10;
    //         this.RefreshPlayerAppearance(characterCreation);
    //         characterCreation.ChangeCharsAnimation(new List<string>()
    //         {
    //             "act_childhood_ready"
    //         });
    //     }

    //     protected void YouthCavalryOnConsequence(CharacterCreation characterCreation)
    //     {
    //         this.SelectedTitleType = 9;
    //         this.RefreshPlayerAppearance(characterCreation);
    //         characterCreation.ChangeCharsAnimation(new List<string>()
    //         {
    //             "act_childhood_apprentice"
    //         });
    //     }

    //     protected void YouthHearthGuardOnConsequence(CharacterCreation characterCreation)
    //     {
    //         this.SelectedTitleType = 9;
    //         this.RefreshPlayerAppearance(characterCreation);
    //         characterCreation.ChangeCharsAnimation(new List<string>()
    //         {
    //             "act_childhood_athlete"
    //         });
    //     }

    //     protected void YouthOutridersOnConsequence(CharacterCreation characterCreation)
    //     {
    //         this.SelectedTitleType = 2;
    //         this.RefreshPlayerAppearance(characterCreation);
    //         characterCreation.ChangeCharsAnimation(new List<string>()
    //         {
    //             "act_childhood_gracious"
    //         });
    //     }

    //     protected void YouthOtherOutridersOnConsequence(CharacterCreation characterCreation)
    //     {
    //         this.SelectedTitleType = 2;
    //         this.RefreshPlayerAppearance(characterCreation);
    //         characterCreation.ChangeCharsAnimation(new List<string>()
    //         {
    //             "act_childhood_gracious"
    //         });
    //     }

    //     protected void YouthInfantryOnConsequence(CharacterCreation characterCreation)
    //     {
    //         this.SelectedTitleType = 3;
    //         this.RefreshPlayerAppearance(characterCreation);
    //         characterCreation.ChangeCharsAnimation(new List<string>()
    //         {
    //             "act_childhood_fierce"
    //         });
    //     }

    //     protected void YouthSkirmisherOnConsequence(CharacterCreation characterCreation)
    //     {
    //         this.SelectedTitleType = 4;
    //         this.RefreshPlayerAppearance(characterCreation);
    //         characterCreation.ChangeCharsAnimation(new List<string>()
    //         {
    //             "act_childhood_fox"
    //         });
    //     }

    //     protected void YouthGarrisonOnConsequence(CharacterCreation characterCreation)
    //     {
    //         this.SelectedTitleType = 1;
    //         this.RefreshPlayerAppearance(characterCreation);
    //         characterCreation.ChangeCharsAnimation(new List<string>()
    //         {
    //             "act_childhood_vibrant"
    //         });
    //     }

    //     protected void YouthOtherGarrisonOnConsequence(CharacterCreation characterCreation)
    //     {
    //         this.SelectedTitleType = 1;
    //         this.RefreshPlayerAppearance(characterCreation);
    //         characterCreation.ChangeCharsAnimation(new List<string>()
    //         {
    //             "act_childhood_sharp"
    //         });
    //     }

    //     protected void YouthKernOnConsequence(CharacterCreation characterCreation)
    //     {
    //         this.SelectedTitleType = 8;
    //         this.RefreshPlayerAppearance(characterCreation);
    //         characterCreation.ChangeCharsAnimation(new List<string>()
    //         {
    //             "act_childhood_apprentice"
    //         });
    //     }

    //     protected void YouthCamperOnConsequence(CharacterCreation characterCreation)
    //     {
    //         this.SelectedTitleType = 5;
    //         this.RefreshPlayerAppearance(characterCreation);
    //         characterCreation.ChangeCharsAnimation(new List<string>()
    //         {
    //             "act_childhood_militia"
    //         });
    //     }

    //     protected void YouthGroomOnApply(CharacterCreation characterCreation)
    //     {
    //     }

    //     protected bool YouthChieftainOnCondition() =>
    //         (this.GetSelectedCulture().StringId == "battania" || this.GetSelectedCulture().StringId == "khuzait") &&
    //         this._familyOccupationType == ScCharacterCreationContent.OccupationTypes.Retainer;

    //     protected void YouthChieftainOnApply(CharacterCreation characterCreation)
    //     {
    //     }

    //     protected bool YouthCavalryOnCondition() => this.GetSelectedCulture().StringId == "empire" ||
    //                                                 this.GetSelectedCulture().StringId == "khuzait" ||
    //                                                 this.GetSelectedCulture().StringId == "aserai" ||
    //                                                 this.GetSelectedCulture().StringId == "vlandia";

    //     protected void YouthCavalryOnApply(CharacterCreation characterCreation)
    //     {
    //     }

    //     protected bool YouthHearthGuardOnCondition() => this.GetSelectedCulture().StringId == "sturgia" ||
    //                                                     this.GetSelectedCulture().StringId == "battania";

    //     protected void YouthHearthGuardOnApply(CharacterCreation characterCreation)
    //     {
    //     }

    //     protected bool YouthOutridersOnCondition() => this.GetSelectedCulture().StringId == "empire" ||
    //                                                   this.GetSelectedCulture().StringId == "khuzait";

    //     protected void YouthOutridersOnApply(CharacterCreation characterCreation)
    //     {
    //     }

    //     protected bool YouthOtherOutridersOnCondition() => this.GetSelectedCulture().StringId != "empire" &&
    //                                                        this.GetSelectedCulture().StringId != "khuzait";

    //     protected void YouthOtherOutridersOnApply(CharacterCreation characterCreation)
    //     {
    //     }

    //     protected void YouthInfantryOnApply(CharacterCreation characterCreation)
    //     {
    //     }

    //     protected void YouthSkirmisherOnApply(CharacterCreation characterCreation)
    //     {
    //     }

    //     protected bool YouthGarrisonOnCondition() => this.GetSelectedCulture().StringId == "empire" ||
    //                                                  this.GetSelectedCulture().StringId == "vlandia";

    //     protected void YouthGarrisonOnApply(CharacterCreation characterCreation)
    //     {
    //     }

    //     protected bool YouthOtherGarrisonOnCondition() => this.GetSelectedCulture().StringId != "empire" &&
    //                                                       this.GetSelectedCulture().StringId != "vlandia";

    //     protected void YouthOtherGarrisonOnApply(CharacterCreation characterCreation)
    //     {
    //     }

    //     protected bool YouthSkirmisherOnCondition() => this.GetSelectedCulture().StringId != "battania";

    //     protected bool YouthKernOnCondition() => this.GetSelectedCulture().StringId == "battania";

    //     protected void YouthKernOnApply(CharacterCreation characterCreation)
    //     {
    //     }

    //     protected bool YouthCamperOnCondition() =>
    //         this._familyOccupationType != ScCharacterCreationContent.OccupationTypes.Retainer;

    //     protected void YouthCamperOnApply(CharacterCreation characterCreation)
    //     {
    //     }

    //     protected void AccomplishmentOnInit(CharacterCreation characterCreation)
    //     {
    //         characterCreation.IsPlayerAlone = true;
    //         characterCreation.HasSecondaryCharacter = false;
    //         characterCreation.ClearFaceGenPrefab();
    //         characterCreation.ChangeFaceGenChars(
    //             ScCharacterCreationContent.ChangePlayerFaceWithAge((float) this.AccomplishmentAge));
    //         characterCreation.ChangeCharsAnimation(new List<string>()
    //         {
    //             "act_childhood_schooled"
    //         });
    //         this.RefreshPlayerAppearance(characterCreation);
    //     }

    //     protected void AccomplishmentDefeatedEnemyOnApply(CharacterCreation characterCreation)
    //     {
    //     }

    //     protected void AccomplishmentExpeditionOnApply(CharacterCreation characterCreation)
    //     {
    //     }

    //     protected bool AccomplishmentRuralOnCondition() => this.RuralType();

    //     protected bool AccomplishmentMerchantOnCondition() =>
    //         this._familyOccupationType == ScCharacterCreationContent.OccupationTypes.Merchant;

    //     protected bool AccomplishmentPosseOnConditions() =>
    //         this._familyOccupationType == ScCharacterCreationContent.OccupationTypes.Retainer ||
    //         this._familyOccupationType == ScCharacterCreationContent.OccupationTypes.Herder ||
    //         this._familyOccupationType == ScCharacterCreationContent.OccupationTypes.Mercenary;

    //     protected bool AccomplishmentSavedVillageOnCondition() => this.RuralType() &&
    //                                                               this._familyOccupationType !=
    //                                                               ScCharacterCreationContent.OccupationTypes
    //                                                                   .Retainer && this._familyOccupationType !=
    //                                                               ScCharacterCreationContent.OccupationTypes
    //                                                                   .Herder;

    //     protected bool AccomplishmentSavedStreetOnCondition() => !this.RuralType() &&
    //                                                              this._familyOccupationType !=
    //                                                              ScCharacterCreationContent.OccupationTypes
    //                                                                  .Merchant && this._familyOccupationType !=
    //                                                              ScCharacterCreationContent.OccupationTypes
    //                                                                  .Mercenary;

    //     protected bool AccomplishmentUrbanOnCondition() => !this.RuralType();

    //     protected void AccomplishmentWorkshopOnApply(CharacterCreation characterCreation)
    //     {
    //     }

    //     protected void AccomplishmentSiegeHunterOnApply(CharacterCreation characterCreation)
    //     {
    //     }

    //     protected void AccomplishmentEscapadeOnApply(CharacterCreation characterCreation)
    //     {
    //     }

    //     protected void AccomplishmentTreaterOnApply(CharacterCreation characterCreation)
    //     {
    //     }

    //     protected void AccomplishmentDefeatedEnemyOnConsequence(CharacterCreation characterCreation) =>
    //         characterCreation.ChangeCharsAnimation(new List<string>()
    //         {
    //             "act_childhood_athlete"
    //         });

    //     protected void AccomplishmentExpeditionOnConsequence(CharacterCreation characterCreation) =>
    //         characterCreation.ChangeCharsAnimation(new List<string>()
    //         {
    //             "act_childhood_gracious"
    //         });

    //     protected void AccomplishmentMerchantOnConsequence(CharacterCreation characterCreation) =>
    //         characterCreation.ChangeCharsAnimation(new List<string>()
    //         {
    //             "act_childhood_ready"
    //         });

    //     protected void AccomplishmentSavedVillageOnConsequence(CharacterCreation characterCreation) =>
    //         characterCreation.ChangeCharsAnimation(new List<string>()
    //         {
    //             "act_childhood_vibrant"
    //         });

    //     protected void AccomplishmentSavedStreetOnConsequence(CharacterCreation characterCreation) =>
    //         characterCreation.ChangeCharsAnimation(new List<string>()
    //         {
    //             "act_childhood_vibrant"
    //         });

    //     protected void AccomplishmentWorkshopOnConsequence(CharacterCreation characterCreation) =>
    //         characterCreation.ChangeCharsAnimation(new List<string>()
    //         {
    //             "act_childhood_decisive"
    //         });

    //     protected void AccomplishmentSiegeHunterOnConsequence(CharacterCreation characterCreation) =>
    //         characterCreation.ChangeCharsAnimation(new List<string>()
    //         {
    //             "act_childhood_tough"
    //         });

    //     protected void AccomplishmentEscapadeOnConsequence(CharacterCreation characterCreation) =>
    //         characterCreation.ChangeCharsAnimation(new List<string>()
    //         {
    //             "act_childhood_clever"
    //         });

    //     protected void AccomplishmentTreaterOnConsequence(CharacterCreation characterCreation) =>
    //         characterCreation.ChangeCharsAnimation(new List<string>()
    //         {
    //             "act_childhood_manners"
    //         });

    //     protected void StartingAgeOnInit(CharacterCreation characterCreation)
    //     {
    //         characterCreation.IsPlayerAlone = true;
    //         characterCreation.HasSecondaryCharacter = false;
    //         characterCreation.ClearFaceGenPrefab();
    //         characterCreation.ChangeFaceGenChars(
    //             ScCharacterCreationContent.ChangePlayerFaceWithAge((float) this._startingAge));
    //         characterCreation.ChangeCharsAnimation(new List<string>()
    //         {
    //             "act_childhood_schooled"
    //         });
    //         this.RefreshPlayerAppearance(characterCreation);
    //     }

    //     protected void StartingAgeYoungOnConsequence(CharacterCreation characterCreation)
    //     {
    //         characterCreation.ClearFaceGenPrefab();
    //         characterCreation.ChangeFaceGenChars(ScCharacterCreationContent.ChangePlayerFaceWithAge(20f));
    //         characterCreation.ChangeCharsAnimation(new List<string>()
    //         {
    //             "act_childhood_focus"
    //         });
    //         this.RefreshPlayerAppearance(characterCreation);
    //         this._startingAge = ScCharacterCreationContent.SandboxAgeOptions.YoungAdult;
    //         this.SetHeroAge(20f);
    //     }

    //     protected void StartingAgeAdultOnConsequence(CharacterCreation characterCreation)
    //     {
    //         characterCreation.ClearFaceGenPrefab();
    //         characterCreation.ChangeFaceGenChars(ScCharacterCreationContent.ChangePlayerFaceWithAge(30f));
    //         characterCreation.ChangeCharsAnimation(new List<string>()
    //         {
    //             "act_childhood_ready"
    //         });
    //         this.RefreshPlayerAppearance(characterCreation);
    //         this._startingAge = ScCharacterCreationContent.SandboxAgeOptions.Adult;
    //         this.SetHeroAge(30f);
    //     }

    //     protected void StartingAgeMiddleAgedOnConsequence(CharacterCreation characterCreation)
    //     {
    //         characterCreation.ClearFaceGenPrefab();
    //         characterCreation.ChangeFaceGenChars(ScCharacterCreationContent.ChangePlayerFaceWithAge(40f));
    //         characterCreation.ChangeCharsAnimation(new List<string>()
    //         {
    //             "act_childhood_sharp"
    //         });
    //         this.RefreshPlayerAppearance(characterCreation);
    //         this._startingAge = ScCharacterCreationContent.SandboxAgeOptions.MiddleAged;
    //         this.SetHeroAge(40f);
    //     }

    //     protected void StartingAgeElderlyOnConsequence(CharacterCreation characterCreation)
    //     {
    //         characterCreation.ClearFaceGenPrefab();
    //         characterCreation.ChangeFaceGenChars(ScCharacterCreationContent.ChangePlayerFaceWithAge(50f));
    //         characterCreation.ChangeCharsAnimation(new List<string>()
    //         {
    //             "act_childhood_tough"
    //         });
    //         this.RefreshPlayerAppearance(characterCreation);
    //         this._startingAge = ScCharacterCreationContent.SandboxAgeOptions.Elder;
    //         this.SetHeroAge(50f);
    //     }

    //     protected void StartingAgeYoungOnApply(CharacterCreation characterCreation) =>
    //         this._startingAge = ScCharacterCreationContent.SandboxAgeOptions.YoungAdult;

    //     protected void StartingAgeAdultOnApply(CharacterCreation characterCreation) =>
    //         this._startingAge = ScCharacterCreationContent.SandboxAgeOptions.Adult;

    //     protected void StartingAgeMiddleAgedOnApply(CharacterCreation characterCreation) =>
    //         this._startingAge = ScCharacterCreationContent.SandboxAgeOptions.MiddleAged;

    //     protected void StartingAgeElderlyOnApply(CharacterCreation characterCreation) =>
    //         this._startingAge = ScCharacterCreationContent.SandboxAgeOptions.Elder;

    //     protected void ApplyEquipments(CharacterCreation characterCreation)
    //     {
    //         ScCharacterCreationContent.ClearMountEntity(characterCreation);
    //         MBEquipmentRoster instance = Game.Current.ObjectManager.GetObject<MBEquipmentRoster>(
    //             "player_char_creation_" + this.GetSelectedCulture().StringId + "_" + (object) this.SelectedTitleType +
    //             (Hero.MainHero.IsFemale ? "_f" : "_m"));
    //         this.PlayerStartEquipment = instance?.DefaultEquipment ?? MBEquipmentRoster.EmptyEquipment;
    //         this.PlayerCivilianEquipment =
    //             (instance != null ? instance.GetCivilianEquipments().FirstOrDefault<Equipment>() : (Equipment) null) ??
    //             MBEquipmentRoster.EmptyEquipment;
    //         if (this.PlayerStartEquipment != null && this.PlayerCivilianEquipment != null)
    //         {
    //             CharacterObject.PlayerCharacter.Equipment.FillFrom(this.PlayerStartEquipment);
    //             CharacterObject.PlayerCharacter.FirstCivilianEquipment.FillFrom(this.PlayerCivilianEquipment);
    //         }

    //         ScCharacterCreationContent.ChangePlayerMount(characterCreation, Hero.MainHero);
    //     }

    //     protected void SetHeroAge(float age) => Hero.MainHero.SetBirthDay(CampaignTime.YearsFromNow(-age));

    //     protected enum SandboxAgeOptions
    //     {
    //         YoungAdult = 20, // 0x00000014
    //         Adult = 30, // 0x0000001E
    //         MiddleAged = 40, // 0x00000028
    //         Elder = 50, // 0x00000032
    //     }

    //     protected enum OccupationTypes
    //     {
    //         Artisan,
    //         Bard,
    //         Retainer,
    //         Merchant,
    //         Farmer,
    //         Hunter,
    //         Vagabond,
    //         Mercenary,
    //         Herder,
    //         Healer,
    //         NumberOfTypes,
    //     }
    // }
}