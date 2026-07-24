// using System.Numerics;
// using Content.Client.Stylesheets;
// using Content.Shared.MatchSystem;
// using Robust.Client.Graphics;
// using Robust.Client.ResourceManagement;
//
// namespace Content.Client.Matches;
//
// public sealed class MatchOverlay : Overlay
// {
//     private readonly Font _font;
//
//     private MatchRoundStats Stats;
//
//     public MatchOverlay(IResourceCache resource, MatchRoundStats stats)
//     {
//         _font = resource.NotoStack();
//         Stats = stats;
//     }
//
//     protected override void Draw(in OverlayDrawArgs args)
//     {
//         var overlayString = "";
//
//         overlayString += Stats.RoundNumber + "/" + Stats.MaxRounds + "\n";
//
//         foreach (var player in Stats.PlayerToRoundsWon)
//         {
//             overlayString += "    " + player.Key + ": " + player.Value + "\n";
//         }
//
//         args.ScreenHandle.DrawString(_font, new Vector2(20, args.Viewport.Size.Y/2), overlayString, 2, new Color(255, 0, 0));
//     }
// }
