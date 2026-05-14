using System;
using OVRSharp.Exceptions;
using Valve.VR;

namespace OVRSharp.Overlay;

public interface IOverlayIdentity : IDisposable
{
    string Key { get; }
    string Name { get; }
    ulong Handle { get; }
}

public class OverlayIdentity(string key, string name, ulong overlayHandle = OpenVR.k_ulOverlayHandleInvalid)
    : IOverlayIdentity
{
    public string Key { get; } = key;
    public string Name { get; } = name;
    public ulong Handle { get; } = overlayHandle;

    public void Dispose()
    {
        if (Handle != OpenVR.k_ulOverlayHandleInvalid) OpenVR.Overlay.DestroyOverlay(Handle).ThrowIfError();
    }
}

public class DashboardOverlayIdentity(OverlayIdentity identity, ulong thumbnailHandle = OpenVR.k_ulOverlayHandleInvalid) : IOverlayIdentity
{
    public string Key { get; } = identity.Key;

    public string Name { get; } = identity.Name;

    public ulong Handle { get; } = identity.Handle;

    public ulong ThumbnailHandle { get; } = thumbnailHandle;

    public void Dispose()
    {
        identity?.Dispose();
        if (ThumbnailHandle != OpenVR.k_ulOverlayHandleInvalid)
            OpenVR.Overlay.DestroyOverlay(ThumbnailHandle).ThrowIfError();
    }
}

public class OverlayIdentityFactory(string appName)
{
    public static OverlayIdentityFactory OfAssemblyName() =>
        new(System.Reflection.Assembly.GetExecutingAssembly().GetName().Name);

    public OverlayIdentity Create(string name)
    {
        var uniqueKey = GetUniqueKey(name);

        ulong key = OpenVR.k_ulOverlayHandleInvalid;

        OpenVR.Overlay.CreateOverlay(uniqueKey, name, ref key).ThrowIfError();

        return new OverlayIdentity(uniqueKey, name, key);
    }

    public DashboardOverlayIdentity CreateDashboard(string name)
    {
        var uniqueKey = GetUniqueKey(name);

        ulong key = OpenVR.k_ulOverlayHandleInvalid;
        ulong thumbnailHandle = OpenVR.k_ulOverlayHandleInvalid;

        OpenVR.Overlay.CreateDashboardOverlay(uniqueKey, name, ref key, ref thumbnailHandle).ThrowIfError();

        return new DashboardOverlayIdentity(new(uniqueKey, name, key), thumbnailHandle);
    }

    private string GetUniqueKey(string name)
    {
        return $"{Sanitize(appName)}.{Sanitize(name)}";
    }

    private string Sanitize(string text)
    {
        // 英数字とアンダースコアのみ許可
        return System.Text.RegularExpressions.Regex.Replace(text, @"[^\w]", "_").ToLower();
    }
}