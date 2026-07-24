// using Content.Shared.MatchSystem;
// using Robust.Client.Graphics;
// using Robust.Client.ResourceManagement;
//
// namespace Content.Client.Matches;
//
// public sealed class MatchOverlaySystem : EntitySystem
// {
//     [Dependency] private IOverlayManager _overlayManager = default!;
//     [Dependency] private IResourceCache _resourceCache = default!;
//
//     public override void Initialize()
//     {
//         base.Initialize();
//
//         // if (_overlayManager.HasOverlay<MatchOverlay>())
//         //     return;
//
//         // var overlay = new MatchOverlay(_resourceCache);
//         //
//         // _overlayManager.AddOverlay(overlay);
//
//         SubscribeNetworkEvent<MatchRoundStartMessage>(OnMatchStart);
//     }
//
//     private void OnMatchStart(MatchRoundStartMessage message)
//     {
//         var overlay = new MatchOverlay(_resourceCache, message.stats);
//
//         _overlayManager.RemoveOverlay<MatchOverlay>();
//
//         _overlayManager.AddOverlay(overlay);
//     }
// }
