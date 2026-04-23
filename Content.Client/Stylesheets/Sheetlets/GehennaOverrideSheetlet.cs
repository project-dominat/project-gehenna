using Content.Client.ContextMenu.UI;
using Content.Client.Examine;
using Content.Client.Resources;
using Content.Client.Stylesheets.Fonts;
using Content.Client.Stylesheets.Stylesheets;
using Content.Client.UserInterface.Controls;
using Content.Client.UserInterface.Controls.FancyTree;
using Content.Client.UserInterface.Screens;
using Content.Client.UserInterface.Systems.Chat.Controls;
using Content.Client.Verbs.UI;
using Content.Shared.Verbs;
using Robust.Client.Graphics;
using Robust.Client.UserInterface;
using Robust.Client.UserInterface.Controls;
using Robust.Client.UserInterface.CustomControls;
using static Content.Client.Stylesheets.StylesheetHelpers;

namespace Content.Client.Stylesheets.Sheetlets;

/// <summary>
///     Final Gehenna theme pass. Applied explicitly after Nanotrasen sheetlets so it can replace
///     the stock primitives instead of trying to fight them mid-pipeline.
/// </summary>
public sealed class GehennaThemeSheetlet : Sheetlet<GehennaStylesheet>
{
    private static readonly Color BgDeep = Color.FromHex("#0a0a0c");
    private static readonly Color BgWindow = Color.FromHex("#2c2b36");
    private static readonly Color BgPanel = Color.FromHex("#31303c");
    private static readonly Color BgPanelAlt = Color.FromHex("#25242d");
    private static readonly Color BgInset = Color.FromHex("#17171d");
    private static readonly Color Line = Color.FromHex("#2a2a35");
    private static readonly Color LineBright = Color.FromHex("#3d3d4a");
    private static readonly Color Ink = Color.FromHex("#d4cdb8");
    private static readonly Color InkDim = Color.FromHex("#8a8478");
    private static readonly Color InkBright = Color.FromHex("#f0e8d2");
    private static readonly Color Gold = Color.FromHex("#b08840");
    private static readonly Color GoldBright = Color.FromHex("#d4a960");
    private static readonly Color GoldDeep = Color.FromHex("#6b4f1f");
    private static readonly Color Blood = Color.FromHex("#6b0f0f");
    private static readonly Color BloodBright = Color.FromHex("#9a1a1a");

    public override StyleRule[] GetRules(GehennaStylesheet sheet, object config)
    {
        var ui10 = UiFont(sheet, 10);
        var ui11 = UiFont(sheet, 11);
        var ui12 = UiFont(sheet, 12);
        var ui12Bold = UiFont(sheet, 12, FontKind.Bold);
        var ui13Bold = UiFont(sheet, 13, FontKind.Bold);
        var ui14Bold = UiFont(sheet, 14, FontKind.Bold);

        var body11 = BodyFont(sheet, 11);
        var body12 = BodyFont(sheet, 12);
        var body12Italic = BodyFont(sheet, 12, FontKind.Italic);
        var body13 = BodyFont(sheet, 13);

        var heading16 = HeadingFont(sheet, 16);
        var heading20 = HeadingFont(sheet, 20);
        var heading22 = HeadingFont(sheet, 22);
        var mono12 = MonoFont(sheet, 12);

        var panelLight = FlatBox(BgPanel, LineBright, new Thickness(1), 8, 6);
        var panelDark = FlatBox(BgPanelAlt, Line, new Thickness(1), 8, 6);
        var panelInset = FlatBox(BgInset, Line, new Thickness(1), 6, 4);
        var panelBorderless = FlatBox(BgPanelAlt, Line, new Thickness(0), 8, 6);
        var panelDropTarget = FlatBox(BgPanelAlt, GoldDeep, new Thickness(1), 8, 6);

        var windowPanel = FlatBox(BgWindow, GoldDeep, new Thickness(1), 8, 6);
        var borderedWindowPanel = FlatBox(BgWindow, Gold, new Thickness(1), 8, 6);
        var windowHeader = FlatBox(BgPanelAlt, GoldDeep, new Thickness(0, 0, 0, 1), 10, 4);
        var alertHeader = FlatBox(Blood, BloodBright, new Thickness(0, 0, 0, 2), 10, 4);

        var buttonNormal = FlatBox(BgPanelAlt, GoldDeep, new Thickness(1), 10, 3);
        var buttonHover = FlatBox(BgPanel, Gold, new Thickness(1), 10, 3);
        var buttonPressed = FlatBox(GoldDeep, GoldBright, new Thickness(1), 10, 3);
        var buttonDisabled = FlatBox(BgInset, Line, new Thickness(1), 10, 3);

        var buttonCompactNormal = FlatBox(BgPanelAlt, GoldDeep, new Thickness(1), 8, 2);
        var buttonCompactHover = FlatBox(BgPanel, Gold, new Thickness(1), 8, 2);
        var buttonCompactPressed = FlatBox(GoldDeep, GoldBright, new Thickness(1), 8, 2);
        var buttonCompactDisabled = FlatBox(BgInset, Line, new Thickness(1), 8, 2);

        var optionDropdown = FlatBox(BgPanelAlt, GoldDeep, new Thickness(1), 4, 4);

        var tabPanel = FlatBox(BgWindow, GoldDeep, new Thickness(1), 4, 4);
        var tabActive = FlatBox(BgPanel, Gold, new Thickness(1, 1, 1, 0), 8, 3);
        var tabInactive = FlatBox(BgPanelAlt, LineBright, new Thickness(1, 1, 1, 0), 8, 3);

        var lineEdit = FlatBox(BgInset, GoldDeep, new Thickness(1), 6, 2);
        var chatPanel = FlatBox(BgWindow.WithAlpha(0.92f), GoldDeep, new Thickness(1), 8, 6);
        var chatOutput = FlatBox(BgInset.WithAlpha(0.92f), Line, new Thickness(1), 6, 4);
        var chatInput = FlatBox(BgInset, GoldDeep, new Thickness(1), 6, 2);

        var tooltipPanel = FlatBox(BgPanelAlt, GoldDeep, new Thickness(1), 7, 3);
        var whisperPanel = FlatBox(BgPanel, GoldDeep, new Thickness(1), 7, 3);

        var itemListBackground = FlatBox(BgInset, GoldDeep, new Thickness(1), 0, 0);
        var itemListRow = FlatBox(BgPanelAlt, Line, new Thickness(0), 4, 2);
        var itemListRowSelected = FlatBox(BgPanel, GoldDeep, new Thickness(1), 4, 2);

        var listRowNormal = FlatBox(BgPanelAlt, Line, new Thickness(1), 6, 2);
        var listRowHover = FlatBox(BgPanel, GoldDeep, new Thickness(1), 6, 2);
        var listRowPressed = FlatBox(GoldDeep, Gold, new Thickness(1), 6, 2);
        var listRowDisabled = FlatBox(BgInset, Line, new Thickness(1), 6, 2);

        var treeBackground = FlatBox(BgPanelAlt, GoldDeep, new Thickness(1), 0, 0);
        var treeSelected = FlatBox(BgPanel, Gold, new Thickness(1), 4, 2);

        var contextPopup = FlatBox(BgWindow, GoldDeep, new Thickness(1), 4, 4);
        var contextNormal = FlatBox(BgPanelAlt, Line, new Thickness(1), 6, 2);
        var contextHover = FlatBox(BgPanel, GoldDeep, new Thickness(1), 6, 2);
        var contextDisabled = FlatBox(BgInset, Line, new Thickness(1), 6, 2);
        var contextConfirm = FlatBox(Blood, BloodBright, new Thickness(1), 6, 2);
        var contextConfirmHover = FlatBox(BloodBright, GoldDeep, new Thickness(1), 6, 2);

        var lobbyStripe = FlatBox(BgPanelAlt, GoldDeep, new Thickness(0, 1, 0, 1), 6, 2);

        var rules = new List<StyleRule>
        {
            E().Prop(Label.StylePropertyFont, ui12),
            E<Label>().FontColor(Ink),
            E<RichTextLabel>().Prop("font", body12).FontColor(Ink),
            E<OutputPanel>().Prop("font", body12),

            E().Class(StyleClass.FontSmall).Font(ui10),
            E().Class(StyleClass.FontLarge).Font(ui14Bold),
            E().Class(StyleClass.Italic).Font(UiFont(sheet, 12, FontKind.Italic)),
            E().Class(StyleClass.Monospace).Prop("font", mono12),

            E<Label>().Class(StyleClass.LabelHeading).Font(heading16).FontColor(GoldBright),
            E<Label>().Class(StyleClass.LabelHeadingBigger).Font(heading20).FontColor(GoldBright),
            E<Label>().Class(StyleClass.LabelKeyText).Font(ui10).FontColor(Gold),
            E<Label>().Class(StyleClass.LabelSubText).Font(ui11).FontColor(InkDim),
            E<RichTextLabel>().Class(StyleClass.LabelSubText).Prop("font", body11).FontColor(InkDim),
            E<Label>().Class(StyleClass.LabelWeak).FontColor(InkDim),
            E<Label>().Class(StyleClass.LabelMonospaceText).Prop("font", mono12).FontColor(Ink),
            E<Label>().Class(StyleClass.LabelMonospaceHeading).Prop("font", MonoFont(sheet, 14, FontKind.Bold)).FontColor(GoldBright),
            E<Label>().Class(StyleClass.LabelMonospaceSubHeading).Prop("font", MonoFont(sheet, 12, FontKind.Bold)).FontColor(Gold),

            E<Label>().Class(DefaultWindow.StyleClassWindowTitle).Font(heading16).FontColor(GoldBright),
            E<Label>().Class("FancyWindowTitle").Font(heading16).FontColor(GoldBright),
            E<Label>().Class("windowTitleAlert").Font(ui13Bold).FontColor(InkBright),
            E<Label>().Class("WindowFooterText").Font(ui10).FontColor(InkDim),

            E<NanoHeading>().ParentOf(E<PanelContainer>()).Panel(FlatBox(BgPanelAlt, GoldDeep, new Thickness(0, 0, 0, 1), 8, 2)),
            E<NanoHeading>().ParentOf(E<Label>()).Font(heading16).FontColor(GoldBright),

            E<PanelContainer>().Class(StyleClass.PanelLight).Panel(panelLight).Prop(Control.StylePropertyModulateSelf, Color.White),
            E<PanelContainer>().Class(StyleClass.PanelDark).Panel(panelDark).Prop(Control.StylePropertyModulateSelf, Color.White),
            E<PanelContainer>().Class(StyleClass.PanelDropTarget).Panel(panelDropTarget).Prop(Control.StylePropertyModulateSelf, Color.White),
            E<PanelContainer>().Class("BackgroundDark").Panel(panelInset).Prop(Control.StylePropertyModulateSelf, Color.White),
            E().Class(StyleClass.BackgroundPanel).Panel(panelLight).Prop(Control.StylePropertyModulateSelf, Color.White),
            E().Class(StyleClass.BackgroundPanelDark).Panel(panelDark).Prop(Control.StylePropertyModulateSelf, Color.White),
            E().Class(StyleClass.BackgroundPanelOpenLeft).Panel(panelLight).Prop(Control.StylePropertyModulateSelf, Color.White),
            E().Class(StyleClass.BackgroundPanelOpenRight).Panel(panelLight).Prop(Control.StylePropertyModulateSelf, Color.White),

            E().Class(DefaultWindow.StyleClassWindowPanel).Panel(windowPanel),
            E().Class(StyleClass.BorderedWindowPanel).Panel(borderedWindowPanel),
            E().Class(DefaultWindow.StyleClassWindowHeader).Panel(windowHeader),
            E<PanelContainer>().Class("WindowHeadingBackground").Panel(windowHeader).Prop(Control.StylePropertyModulateSelf, Color.White),
            E<PanelContainer>().Class("WindowHeadingBackgroundLight").Panel(windowHeader).Prop(Control.StylePropertyModulateSelf, Color.White),
            E().Class(StyleClass.AlertWindowHeader).Panel(alertHeader),

            E<PanelContainer>().Class(StyleClass.LowDivider).Panel(FlatBox(GoldDeep, GoldDeep, new Thickness(0), 0, 0)),
            E<PanelContainer>().Class(StyleClass.HighDivider).Panel(FlatBox(LineBright, LineBright, new Thickness(0), 0, 0)),

            E<TextureButton>().Class(DefaultWindow.StyleClassWindowCloseButton).PseudoNormal().Modulate(InkDim),
            E<TextureButton>().Class(DefaultWindow.StyleClassWindowCloseButton).PseudoHovered().Modulate(BloodBright),
            E<TextureButton>().Class(DefaultWindow.StyleClassWindowCloseButton).PseudoPressed().Modulate(Blood),
            E<TextureButton>().Class(DefaultWindow.StyleClassWindowCloseButton).PseudoDisabled().Modulate(LineBright),

            E<TextureButton>().Class(FancyWindow.StyleClassWindowHelpButton).PseudoNormal().Modulate(Gold),
            E<TextureButton>().Class(FancyWindow.StyleClassWindowHelpButton).PseudoHovered().Modulate(GoldBright),
            E<TextureButton>().Class(FancyWindow.StyleClassWindowHelpButton).PseudoPressed().Modulate(GoldDeep),
            E<TextureButton>().Class(FancyWindow.StyleClassWindowHelpButton).PseudoDisabled().Modulate(LineBright),

            E<TabContainer>()
                .Prop(TabContainer.StylePropertyPanelStyleBox, tabPanel)
                .Prop(TabContainer.StylePropertyTabStyleBox, tabActive)
                .Prop(TabContainer.StylePropertyTabStyleBoxInactive, tabInactive),

            E<TextureRect>().Class(OptionButton.StyleClassOptionTriangle).Modulate(GoldBright),
            E<Label>().Class(OptionButton.StyleClassOptionButton).Font(ui12).FontColor(InkBright),
            E<PanelContainer>().Class(OptionButton.StyleClassOptionsBackground).Panel(optionDropdown).Prop(Control.StylePropertyModulateSelf, Color.White),

            E<LineEdit>()
                .Prop(LineEdit.StylePropertyStyleBox, lineEdit)
                .Prop(Label.StylePropertyFontColor, InkBright)
                .Prop(LineEdit.StylePropertyCursorColor, GoldBright)
                .Prop(LineEdit.StylePropertySelectionColor, GoldDeep.WithAlpha(0.45f)),
            E<LineEdit>().Class(LineEdit.StyleClassLineEditNotEditable).Prop(Label.StylePropertyFontColor, InkDim),
            E<LineEdit>().Pseudo(LineEdit.StylePseudoClassPlaceholder).Prop(Label.StylePropertyFontColor, InkDim),
            E<TextEdit>().Pseudo(TextEdit.StylePseudoClassPlaceholder).Prop(Label.StylePropertyFontColor, InkDim),

            E<PanelContainer>().Class(ChatInputBox.StyleClassChatPanel).Panel(chatPanel).Prop(Control.StylePropertyModulateSelf, Color.White),
            E<PanelContainer>().Class(ChatInputBox.StyleClassChatPanel).ParentOf(E<OutputPanel>())
                .Prop(OutputPanel.StylePropertyStyleBox, chatOutput)
                .Prop("font", body12),
            E<PanelContainer>().Class(ChatInputBox.StyleClassChatPanel).ParentOf(E<OutputPanel>())
                .ParentOf(E<Button>().Class(OutputPanel.StyleClassOutputPanelScrollDownButton))
                .Font(ui12Bold),
            E<LineEdit>().Class(ChatInputBox.StyleClassChatLineEdit)
                .Prop(LineEdit.StylePropertyStyleBox, chatInput)
                .Prop("font", body13)
                .Prop(Label.StylePropertyFontColor, InkBright)
                .Prop(LineEdit.StylePropertyCursorColor, GoldBright)
                .Prop(LineEdit.StylePropertySelectionColor, GoldDeep.WithAlpha(0.5f)),
            E<LineEdit>().Class(ChatInputBox.StyleClassChatLineEdit).Pseudo(LineEdit.StylePseudoClassPlaceholder).Prop(Label.StylePropertyFontColor, InkDim),
            E<PanelContainer>().Class(SeparatedChatGameScreen.StyleClassChatContainer).Panel(panelLight).Prop(Control.StylePropertyModulateSelf, Color.White),

            E<PanelContainer>().Class(StyleClass.TooltipPanel).Panel(tooltipPanel).Prop(Control.StylePropertyModulateSelf, Color.White),
            E<Tooltip>().Prop(Tooltip.StylePropertyPanel, tooltipPanel),
            E<PanelContainer>().Class(ExamineSystem.StyleClassEntityTooltip).Panel(tooltipPanel).Prop(Control.StylePropertyModulateSelf, Color.White),
            E<PanelContainer>().Class("speechBox", "sayBox").Panel(tooltipPanel).Prop(Control.StylePropertyModulateSelf, Color.White),
            E<PanelContainer>().Class("speechBox", "whisperBox").Panel(whisperPanel).Prop(Control.StylePropertyModulateSelf, Color.White),
            E<RichTextLabel>().Class(StyleClass.TooltipTitle).Prop("font", ui13Bold).FontColor(GoldBright),
            E<RichTextLabel>().Class(StyleClass.TooltipDesc).Prop("font", body12).FontColor(Ink),
            E<PanelContainer>().Class("speechBox", "whisperBox").ParentOf(E<RichTextLabel>().Class("bubbleContent")).Prop("font", body12Italic),
            E<PanelContainer>().Class("speechBox", "emoteBox").ParentOf(E<RichTextLabel>().Class("bubbleContent")).Prop("font", body12Italic),
            E<PanelContainer>().Class("speechBox", "sayBox").ParentOf(E<RichTextLabel>().Class("bubbleContent")).Prop("font", body12),

            E<PanelContainer>().Class(ContextMenuPopup.StyleClassContextMenuPopup).Panel(contextPopup).Prop(Control.StylePropertyModulateSelf, Color.White),

            E<RichTextLabel>().Class(InteractionVerb.DefaultTextStyleClass).Prop("font", ui12Bold).FontColor(InkBright),
            E<RichTextLabel>().Class(ActivationVerb.DefaultTextStyleClass).Prop("font", ui12Bold).FontColor(InkBright),
            E<RichTextLabel>().Class(AlternativeVerb.DefaultTextStyleClass).Prop("font", UiFont(sheet, 12, FontKind.Italic)).FontColor(Ink),
            E<RichTextLabel>().Class(Verb.DefaultTextStyleClass).Prop("font", ui12).FontColor(Ink),

            E<ItemList>()
                .Prop(ItemList.StylePropertyBackground, itemListBackground)
                .Prop(ItemList.StylePropertyItemBackground, itemListRow)
                .Prop(ItemList.StylePropertyDisabledItemBackground, itemListRow)
                .Prop(ItemList.StylePropertySelectedItemBackground, itemListRowSelected),

            E<Tree>()
                .Prop(Tree.StylePropertyBackground, treeBackground)
                .Prop(Tree.StylePropertyItemBoxSelected, treeSelected),
            E<FancyTree>().Prop(FancyTree.StylePropertyLineColor, GoldDeep).Prop(FancyTree.StylePropertyIconColor, GoldBright),

            E<ContainerButton>().Identifier(TreeItem.StyleIdentifierTreeButton).Class(TreeItem.StyleClassEvenRow).Box(FlatBox(BgPanelAlt, Line, new Thickness(0), 4, 2)),
            E<ContainerButton>().Identifier(TreeItem.StyleIdentifierTreeButton).Class(TreeItem.StyleClassOddRow).Box(FlatBox(BgWindow, Line, new Thickness(0), 4, 2)),
            E<ContainerButton>().Identifier(TreeItem.StyleIdentifierTreeButton).Class(TreeItem.StyleClassSelected).Box(treeSelected),
            E<ContainerButton>().Identifier(TreeItem.StyleIdentifierTreeButton).PseudoHovered().Box(FlatBox(BgPanel, GoldDeep, new Thickness(1), 4, 2)),

            E<PanelContainer>().Class("tooltipBox").Panel(tooltipPanel).Prop(Control.StylePropertyModulateSelf, Color.White),
            E<PanelContainer>().Class("PopupPanel").Panel(panelDark).Prop(Control.StylePropertyModulateSelf, Color.White),

            E<VScrollBar>().Prop(ScrollBar.StylePropertyGrabber, FlatBox(GoldDeep.WithAlpha(0.55f), GoldDeep.WithAlpha(0.55f), new Thickness(0), 10, 10)),
            E<VScrollBar>().PseudoHovered().Prop(ScrollBar.StylePropertyGrabber, FlatBox(Gold.WithAlpha(0.7f), Gold.WithAlpha(0.7f), new Thickness(0), 10, 10)),
            E<VScrollBar>().PseudoPressed().Prop(ScrollBar.StylePropertyGrabber, FlatBox(GoldBright.WithAlpha(0.85f), GoldBright.WithAlpha(0.85f), new Thickness(0), 10, 10)),
            E<HScrollBar>().Prop(ScrollBar.StylePropertyGrabber, FlatBox(GoldDeep.WithAlpha(0.55f), GoldDeep.WithAlpha(0.55f), new Thickness(0), 10, 10)),
            E<HScrollBar>().PseudoHovered().Prop(ScrollBar.StylePropertyGrabber, FlatBox(Gold.WithAlpha(0.7f), Gold.WithAlpha(0.7f), new Thickness(0), 10, 10)),
            E<HScrollBar>().PseudoPressed().Prop(ScrollBar.StylePropertyGrabber, FlatBox(GoldBright.WithAlpha(0.85f), GoldBright.WithAlpha(0.85f), new Thickness(0), 10, 10)),

            E<ProgressBar>()
                .Prop(ProgressBar.StylePropertyBackground, FlatBox(BgInset, BgInset, new Thickness(0), 0, 0))
                .Prop(ProgressBar.StylePropertyForeground, FlatBox(Gold, Gold, new Thickness(0), 0, 0)),

            E<StripeBack>().Class("LobbyStripe").Prop(StripeBack.StylePropertyBackground, lobbyStripe).Prop(StripeBack.StylePropertyEdgeColor, GoldDeep),
            E<StripeBack>().Class("LateJoinStripe").Prop(StripeBack.StylePropertyBackground, lobbyStripe).Prop(StripeBack.StylePropertyEdgeColor, GoldDeep),
            E<PanelContainer>().Class("GuidebookSidebarPanel").Panel(panelDark).Prop(Control.StylePropertyModulateSelf, Color.White),
            E<PanelContainer>().Class("GuidebookDocumentPanel").Panel(panelLight).Prop(Control.StylePropertyModulateSelf, Color.White),
        };

        AddContainerButtonRules(rules, null, buttonNormal, buttonHover, buttonPressed, buttonDisabled);
        AddContainerButtonRules(rules, StyleClass.ButtonOpenLeft, buttonNormal, buttonHover, buttonPressed, buttonDisabled);
        AddContainerButtonRules(rules, StyleClass.ButtonOpenRight, buttonNormal, buttonHover, buttonPressed, buttonDisabled);
        AddContainerButtonRules(rules, StyleClass.ButtonOpenBoth, buttonNormal, buttonHover, buttonPressed, buttonDisabled);
        AddContainerButtonRules(rules, StyleClass.ButtonSquare, buttonCompactNormal, buttonCompactHover, buttonCompactPressed, buttonCompactDisabled);
        AddContainerButtonRules(rules, StyleClass.ButtonSmall, buttonCompactNormal, buttonCompactHover, buttonCompactPressed, buttonCompactDisabled);

        AddButtonRules(rules, ChannelSelectorItemButton.StyleClassChatSelectorOptionButton, buttonNormal, buttonHover, buttonPressed, buttonDisabled);
        AddContainerButtonRules(rules, ChatInputBox.StyleClassChatFilterOptionButton, buttonCompactNormal, buttonCompactHover, buttonCompactPressed, buttonCompactDisabled, baseClass: null);
        AddOptionButtonRules(rules, buttonNormal, buttonHover, buttonPressed, buttonDisabled);
        AddContextMenuRules(rules, contextNormal, contextHover, contextDisabled, contextConfirm, contextConfirmHover);
        AddListContainerRules(rules, listRowNormal, listRowHover, listRowPressed, listRowDisabled);

        rules.AddRange(
        [
            E<ContainerButton>().Class(ContainerButton.StyleClassButton).ParentOf(E<Label>()).Font(ui12).FontColor(InkBright),
            E<ContainerButton>().Class(ContainerButton.StyleClassButton).ParentOf(E<RichTextLabel>()).Prop("font", ui12).FontColor(InkBright),
            E<ContainerButton>().Class(ContainerButton.StyleClassButton).PseudoDisabled().ParentOf(E<Label>()).FontColor(InkDim),
            E<ContainerButton>().Class(ContainerButton.StyleClassButton).PseudoDisabled().ParentOf(E<RichTextLabel>()).FontColor(InkDim),
            E<Button>().Class(ChannelSelectorItemButton.StyleClassChatSelectorOptionButton).Font(ui12Bold),
            E<ContainerButton>().Class(ChatInputBox.StyleClassChatFilterOptionButton).ParentOf(E<Label>()).Font(ui12Bold).FontColor(InkBright),
        ]);

        return rules.ToArray();
    }

    private static void AddContainerButtonRules(
        List<StyleRule> rules,
        string? variantClass,
        StyleBox normal,
        StyleBox hover,
        StyleBox pressed,
        StyleBox disabled,
        string? baseClass = ContainerButton.StyleClassButton)
    {
        rules.AddRange(
        [
            ButtonSelector(variantClass, baseClass).PseudoNormal().Box(normal).Prop(Control.StylePropertyModulateSelf, Color.White),
            ButtonSelector(variantClass, baseClass).PseudoHovered().Box(hover).Prop(Control.StylePropertyModulateSelf, Color.White),
            ButtonSelector(variantClass, baseClass).PseudoPressed().Box(pressed).Prop(Control.StylePropertyModulateSelf, Color.White),
            ButtonSelector(variantClass, baseClass).PseudoDisabled().Box(disabled).Prop(Control.StylePropertyModulateSelf, Color.White),
        ]);
    }

    private static void AddButtonRules(
        List<StyleRule> rules,
        string styleClass,
        StyleBox normal,
        StyleBox hover,
        StyleBox pressed,
        StyleBox disabled)
    {
        rules.AddRange(
        [
            E<Button>().Class(styleClass).PseudoNormal().Box(normal).Prop(Control.StylePropertyModulateSelf, Color.White),
            E<Button>().Class(styleClass).PseudoHovered().Box(hover).Prop(Control.StylePropertyModulateSelf, Color.White),
            E<Button>().Class(styleClass).PseudoPressed().Box(pressed).Prop(Control.StylePropertyModulateSelf, Color.White),
            E<Button>().Class(styleClass).PseudoDisabled().Box(disabled).Prop(Control.StylePropertyModulateSelf, Color.White),
        ]);
    }

    private static void AddOptionButtonRules(
        List<StyleRule> rules,
        StyleBox normal,
        StyleBox hover,
        StyleBox pressed,
        StyleBox disabled)
    {
        rules.AddRange(
        [
            E<OptionButton>().PseudoNormal().Box(normal).Prop(Control.StylePropertyModulateSelf, Color.White),
            E<OptionButton>().PseudoHovered().Box(hover).Prop(Control.StylePropertyModulateSelf, Color.White),
            E<OptionButton>().PseudoPressed().Box(pressed).Prop(Control.StylePropertyModulateSelf, Color.White),
            E<OptionButton>().PseudoDisabled().Box(disabled).Prop(Control.StylePropertyModulateSelf, Color.White),
        ]);
    }

    private static void AddContextMenuRules(
        List<StyleRule> rules,
        StyleBox normal,
        StyleBox hover,
        StyleBox disabled,
        StyleBox confirm,
        StyleBox confirmHover)
    {
        rules.AddRange(
        [
            E<ContextMenuElement>().Class(ContextMenuElement.StyleClassContextMenuButton).PseudoNormal().Box(normal).Prop(Control.StylePropertyModulateSelf, Color.White),
            E<ContextMenuElement>().Class(ContextMenuElement.StyleClassContextMenuButton).PseudoHovered().Box(hover).Prop(Control.StylePropertyModulateSelf, Color.White),
            E<ContextMenuElement>().Class(ContextMenuElement.StyleClassContextMenuButton).PseudoPressed().Box(hover).Prop(Control.StylePropertyModulateSelf, Color.White),
            E<ContextMenuElement>().Class(ContextMenuElement.StyleClassContextMenuButton).PseudoDisabled().Box(disabled).Prop(Control.StylePropertyModulateSelf, Color.White),

            E<ConfirmationMenuElement>().Class(ConfirmationMenuElement.StyleClassConfirmationContextMenuButton).PseudoNormal().Box(confirm).Prop(Control.StylePropertyModulateSelf, Color.White),
            E<ConfirmationMenuElement>().Class(ConfirmationMenuElement.StyleClassConfirmationContextMenuButton).PseudoHovered().Box(confirmHover).Prop(Control.StylePropertyModulateSelf, Color.White),
            E<ConfirmationMenuElement>().Class(ConfirmationMenuElement.StyleClassConfirmationContextMenuButton).PseudoPressed().Box(confirmHover).Prop(Control.StylePropertyModulateSelf, Color.White),
            E<ConfirmationMenuElement>().Class(ConfirmationMenuElement.StyleClassConfirmationContextMenuButton).PseudoDisabled().Box(disabled).Prop(Control.StylePropertyModulateSelf, Color.White),
        ]);
    }

    private static void AddListContainerRules(
        List<StyleRule> rules,
        StyleBox normal,
        StyleBox hover,
        StyleBox pressed,
        StyleBox disabled)
    {
        rules.AddRange(
        [
            E<ContainerButton>().Class(ListContainer.StyleClassListContainerButton).PseudoNormal().Box(normal).Prop(Control.StylePropertyModulateSelf, Color.White),
            E<ContainerButton>().Class(ListContainer.StyleClassListContainerButton).PseudoHovered().Box(hover).Prop(Control.StylePropertyModulateSelf, Color.White),
            E<ContainerButton>().Class(ListContainer.StyleClassListContainerButton).PseudoPressed().Box(pressed).Prop(Control.StylePropertyModulateSelf, Color.White),
            E<ContainerButton>().Class(ListContainer.StyleClassListContainerButton).PseudoDisabled().Box(disabled).Prop(Control.StylePropertyModulateSelf, Color.White),
        ]);
    }

    private static MutableSelectorElement ButtonSelector(string? variantClass, string? baseClass)
    {
        var selector = E<ContainerButton>();

        if (baseClass is not null)
            selector.Class(baseClass);

        if (variantClass is not null)
            selector.Class(variantClass);

        return selector;
    }

    private static StyleBoxFlat FlatBox(Color background, Color border, Thickness borderThickness, float horizontalPadding, float verticalPadding)
    {
        return FlatBox(background, border, borderThickness, horizontalPadding, verticalPadding, horizontalPadding, verticalPadding);
    }

    private static StyleBoxFlat FlatBox(
        Color background,
        Color border,
        Thickness borderThickness,
        float left,
        float top,
        float right,
        float bottom)
    {
        var box = new StyleBoxFlat
        {
            BackgroundColor = background,
            BorderColor = border,
            BorderThickness = borderThickness,
        };

        box.SetContentMarginOverride(StyleBox.Margin.Left, left);
        box.SetContentMarginOverride(StyleBox.Margin.Top, top);
        box.SetContentMarginOverride(StyleBox.Margin.Right, right);
        box.SetContentMarginOverride(StyleBox.Margin.Bottom, bottom);
        return box;
    }

    private static Font UiFont(GehennaStylesheet sheet, int size, FontKind kind = FontKind.Regular)
    {
        return sheet.ResCache.GetFont(
        [
            $"/Fonts/PTSans/PTSans-{kind.AsFileName()}.ttf",
            kind.IsBold() ? "/Fonts/NotoSans/NotoSansSymbols-Bold.ttf" : "/Fonts/NotoSans/NotoSansSymbols-Regular.ttf",
            "/Fonts/NotoSans/NotoSansSymbols2-Regular.ttf",
            "/Fonts/NotoEmoji.ttf",
        ], size);
    }

    private static Font BodyFont(GehennaStylesheet sheet, int size, FontKind kind = FontKind.Regular)
    {
        return sheet.ResCache.GetFont(
        [
            $"/Fonts/PTSerif/PTSerif-{kind.AsFileName()}.ttf",
            kind.IsBold() ? "/Fonts/NotoSans/NotoSansSymbols-Bold.ttf" : "/Fonts/NotoSans/NotoSansSymbols-Regular.ttf",
            "/Fonts/NotoSans/NotoSansSymbols2-Regular.ttf",
            "/Fonts/NotoEmoji.ttf",
        ], size);
    }

    private static Font HeadingFont(GehennaStylesheet sheet, int size)
    {
        return sheet.ResCache.GetFont(
        [
            "/Fonts/YesevaOne/YesevaOne-Regular.ttf",
            "/Fonts/NotoSans/NotoSans-Regular.ttf",
            "/Fonts/NotoSans/NotoSansSymbols-Regular.ttf",
            "/Fonts/NotoSans/NotoSansSymbols2-Regular.ttf",
            "/Fonts/NotoEmoji.ttf",
        ], size);
    }

    private static Font MonoFont(GehennaStylesheet sheet, int size, FontKind kind = FontKind.Regular)
    {
        return sheet.ResCache.GetFont(
        [
            $"/Fonts/RobotoMono/RobotoMono-{kind.AsFileName()}.ttf",
            kind.IsBold() ? "/Fonts/NotoSans/NotoSansSymbols-Bold.ttf" : "/Fonts/NotoSans/NotoSansSymbols-Regular.ttf",
            "/Fonts/NotoSans/NotoSansSymbols2-Regular.ttf",
            "/Fonts/NotoEmoji.ttf",
        ], size);
    }
}
