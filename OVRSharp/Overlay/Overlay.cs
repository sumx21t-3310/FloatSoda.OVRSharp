using System;

namespace OVRSharp.Overlay;

public interface IOverlay : IDisposable
{
    IOverlayIdentity Identity { get; }
    OverlayOpacity Opacity { get; }
    OverlayWidthInMeters WidthInMeters { get; }
    OverlayCurvature Curvature { get; }
    OverlayTexture Texture { get; }
    OverlayFlags Flags { get; }
}

public interface IOverlay<out TIdentity> : IOverlay where TIdentity : IOverlayIdentity
{
    new TIdentity Identity { get; }
}

/// <summary>
/// ダッシュボードオーバーレイ。位置はSteamVRが管理するため
/// IMovableOverlay を継承しない。
/// </summary>
public interface IDashboardOverlay : IOverlay<DashboardOverlayIdentity>
{
    OverlayTexture Thumbnail { get; }
}

public interface IMovableOverlay : IOverlay
{
    OverlayVisibility Visibility { get; }

    OverlayTransform Transform { get; }
}

public class DashboardOverlay : IDashboardOverlay
{
    public void Dispose()
    {
        throw new NotImplementedException();
    }

    IOverlayIdentity IOverlay.Identity => Identity;

    public DashboardOverlayIdentity Identity { get; }
    public OverlayOpacity Opacity { get; }
    public OverlayWidthInMeters WidthInMeters { get; }
    public OverlayCurvature Curvature { get; }
    public OverlayTexture Texture { get; }
    public OverlayFlags Flags { get; }
    public OverlayTexture Thumbnail { get; }
}

public class MovableOverlay<TTransform>(TTransform transform) : IMovableOverlay where TTransform : OverlayTransform
{
    public void Dispose()
    {
        Identity.Dispose();
    }

    public IOverlayIdentity Identity { get; }
    public OverlayOpacity Opacity { get; }
    public OverlayWidthInMeters WidthInMeters { get; }
    public OverlayCurvature Curvature { get; }
    public OverlayTexture Texture { get; }
    public OverlayFlags Flags { get; }
    public OverlayVisibility Visibility { get; }
    public OverlayTransform Transform => transform;
}

public class DeviceTrackedOverlay(DeviceTrackedOverlayTransform transform)
    : MovableOverlay<DeviceTrackedOverlayTransform>(transform);

public class WorldSpaceOverlay(WorldOverlayTransform transform) : MovableOverlay<WorldOverlayTransform>(transform);