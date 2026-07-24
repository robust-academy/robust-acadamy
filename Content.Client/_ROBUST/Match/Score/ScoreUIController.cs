using Content.Client.UserInterface.Screens;
using Content.Client.UserInterface.Systems.Gameplay;
using JetBrains.Annotations;
using Robust.Client.ResourceManagement;
using Robust.Client.UserInterface;
using Robust.Client.UserInterface.Controllers;
using Robust.Client.UserInterface.Controls;

namespace Content.Client._ROBUST.Match.Score;

[UsedImplicitly]
public sealed partial class ScoreUIController : UIController
{
    [Dependency] private ScoreManager _score = default!;

    public override void Initialize()
    {
        base.Initialize();
        var gameplayStateLoad = UIManager.GetUIController<GameplayStateLoadController>();
        gameplayStateLoad.OnScreenLoad += OnScreenLoad;
        gameplayStateLoad.OnScreenUnload += OnScreenUnload;
    }

    private void OnScreenLoad()
    {
        switch (UIManager.ActiveScreen)
        {
            case DefaultGameScreen game:

                var UI = new ScoreBarUI();
                UI.SetFonts();
                UI.Visible = false; // todo change to false
                game.TopBar.Parent?.AddChild(UI);

                _score.SetUIElement(UI);

                break;
            case SeparatedChatGameScreen separated:
                var UI2 = new ScoreBarUI();
                UI2.SetFonts();
                UI2.Visible = false; // todo change to false
                separated.TopBar.Parent?.AddChild(UI2);

                _score.SetUIElement(UI2);
                break;
        }
    }

    // TODO: figure this out...
    private void OnScreenUnload()
    {
        // _votes.ClearPopupContainer();
    }

    /*
        <BoxContainer Name="Test" Access="Public" Margin="30 0 0 0" Orientation="Vertical">
           <PanelContainer StyleClasses="BackgroundPanel" ModulateSelfOverride="#2b2b31">
               <BoxContainer Orientation="Vertical">
                   <BoxContainer Orientation="Horizontal" SeparationOverride="30" Margin="0 0 0 5">
                       <Label Text="4" FontColorOverride="Orange" />
                       <Label Text="7" FontColorOverride="Red" />
                   </BoxContainer>

                   <BoxContainer Align="Center" Margin="0 0 0 2">
                       <Label Text="2:27" />
                   </BoxContainer>

                   <BoxContainer>
                       <Label Text="First to 5" StyleClasses="WindowFooterText" />
                   </BoxContainer>
               </BoxContainer>

           </PanelContainer>
       </BoxContainer>
     */

    // TODO: Change this to use XAML, this is a lot faster to set up but not good to maintain.
    // private BoxContainer CreateUI()
    // {
    //     var outerBox = new BoxContainer()
    //     {
    //         Name = "ScoreContainer",
    //         Access = AccessLevel.Public, // TODO: this is probably not needed?
    //         Margin = new Thickness(30, 0, 0, 0),
    //         Orientation = BoxContainer.LayoutOrientation.Vertical,
    //         Visible = false,
    //     };
    //
    //     var panelContainer = new PanelContainer()
    //     {
    //         StyleClasses = { "BackgroundPanel" },
    //         ModulateSelfOverride = new Color(43, 43, 49),
    //     };
    //
    //     var scoreBoxContainer = new BoxContainer()
    //     {
    //         Orientation = BoxContainer.LayoutOrientation.Horizontal,
    //         SeparationOverride = 30,
    //         Margin = new Thickness(0, 0, 0, 5),
    //     };
    //
    //     // TODO: Get better fonts that are larger
    //     var yourPointsLabel = new Label() { Name = "YourPoints", FontColorOverride = new Color(255, 153, 0), Access = AccessLevel.Public};
    //     var otherPoints     = new Label() { Name = "OtherPoints", FontColorOverride = new Color(255, 0, 0)};
    //
    //     scoreBoxContainer.AddChild(yourPointsLabel);
    //     scoreBoxContainer.AddChild(otherPoints);
    //
    //     var timeContainer = new BoxContainer()
    //     {
    //         Align = BoxContainer.AlignMode.Center,
    //         Margin = new Thickness(0, 0, 0, 2),
    //     };
    //
    //     var timeLabel = new Label() { Name = "Time" };
    //
    //     timeContainer.AddChild(timeLabel);
    //
    //     var footerContainer = new BoxContainer()
    //     {
    //         Name = "FooterContainer",
    //     };
    //
    //     var footerLabel = new Label() { StyleClasses = { "WindowFooterText" } }; // TODO: Change this to a custom font etc...
    //
    //     footerContainer.AddChild(footerLabel);
    //
    //     panelContainer.AddChild(scoreBoxContainer);
    //     panelContainer.AddChild(timeContainer);
    //     panelContainer.AddChild(footerContainer);
    //
    //     outerBox.AddChild(panelContainer);
    //
    //     return outerBox;
    // }
}
