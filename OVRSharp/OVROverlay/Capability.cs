using System;
using System.Numerics;
using OVRSharp.Exceptions;
using OVRSharp.Math;
using Valve.VR;

namespace OVRSharp.OVROverlay
{
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

    public class DashboardOverlayIdentity(
        OverlayIdentity identity,
        ulong thumbnailHandle = OpenVR.k_ulOverlayHandleInvalid) : IOverlayIdentity
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

    public class DashboardOverlay : IDashboardOverlay
    {
        public void Dispose() => Identity.Dispose();

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
        public void Dispose() => Identity.Dispose();

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


    public class OverlayOpacity(ulong overlayHandle)
    {
        public float Value
        {
            get
            {
                var value = 0.0f;
                OpenVR.Overlay.GetOverlayAlpha(overlayHandle, ref value).ThrowIfError();
                return value;
            }
            set => OpenVR.Overlay.SetOverlayAlpha(overlayHandle, value).ThrowIfError();
        }
    }

    public class OverlayWidthInMeters(ulong overlayHandle)
    {
        public float Value
        {
            get
            {
                var value = 0.0f;
                OpenVR.Overlay.GetOverlayWidthInMeters(overlayHandle, ref value).ThrowIfError();
                return value;
            }
            set => OpenVR.Overlay.SetOverlayWidthInMeters(overlayHandle, value).ThrowIfError();
        }
    }

    public class OverlayCurvature(ulong overlayHandle)
    {
        public float Value
        {
            get
            {
                var value = 0.0f;
                OpenVR.Overlay.GetOverlayCurvature(overlayHandle, ref value).ThrowIfError();
                return value;
            }
            set => OpenVR.Overlay.SetOverlayCurvature(overlayHandle, value).ThrowIfError();
        }
    }

    public abstract class OverlayTransform
    {
        private Vector3 _position;
        private Quaternion _rotation = Quaternion.Identity;

        public Vector3 Position
        {
            get => _position;
            set
            {
                _position = value;
                Apply();
            }
        }

        public Quaternion Rotation
        {
            get => _rotation;
            set
            {
                _rotation = value;
                Apply();
            }
        }


        protected Matrix4x4 GetMatrix() =>
            Matrix4x4.CreateFromQuaternion(_rotation) * Matrix4x4.CreateTranslation(_position);

        public abstract void Apply();
    }

    public sealed class WorldOverlayTransform(ulong overlayHandle) : OverlayTransform
    {
        public ETrackingUniverseOrigin Origin
        {
            get => _origin;
            set
            {
                _origin = value;
                Apply();
            }
        }

        private ETrackingUniverseOrigin _origin = ETrackingUniverseOrigin.TrackingUniverseStanding;


        public override void Apply()
        {
            var hmd = GetMatrix().ToHmdMatrix34_t();

            OpenVR.Overlay.SetOverlayTransformAbsolute(
                overlayHandle,
                _origin,
                ref hmd).ThrowIfError();
        }
    }

    public sealed class DeviceTrackedOverlayTransform(ulong overlayHandle) : OverlayTransform
    {
        public enum TrackedDevice
        {
            LeftController,
            RightController,
            HMD
        }

        public TrackedDevice Target { get; set; }

        private uint ResolveDeviceIndex() => Target switch
        {
            TrackedDevice.HMD => OpenVR.k_unTrackedDeviceIndex_Hmd,
            TrackedDevice.LeftController => OpenVR.System.GetTrackedDeviceIndexForControllerRole(ETrackedControllerRole
                .LeftHand),
            TrackedDevice.RightController => OpenVR.System.GetTrackedDeviceIndexForControllerRole(ETrackedControllerRole
                .RightHand),
            _ => OpenVR.k_unTrackedDeviceIndexInvalid
        };

        public override void Apply()
        {
            var matrix = GetMatrix().ToHmdMatrix34_t();
            OpenVR.Overlay.SetOverlayTransformTrackedDeviceRelative(overlayHandle, ResolveDeviceIndex(), ref matrix)
                .ThrowIfError();
        }
    }

    public class OverlayTexture(ulong overlayHandle)
    {
        public void FromFile(string path) => OpenVR.Overlay.SetOverlayFromFile(overlayHandle, path).ThrowIfError();

        public void FromTexture_t(Texture_t texture) =>
            OpenVR.Overlay.SetOverlayTexture(overlayHandle, ref texture).ThrowIfError();
    }

    public class OverlayVisibility(ulong overlayHandle)
    {
        public void Show() => OpenVR.Overlay.ShowOverlay(overlayHandle).ThrowIfError();

        public void Hide() => OpenVR.Overlay.HideOverlay(overlayHandle).ThrowIfError();
    }

    public class OverlayFlags(ulong overlayHandle)
    {
        public void SetFlag(VROverlayFlags flag, bool value)
        {
            OpenVR.Overlay.SetOverlayFlag(overlayHandle, flag, value).ThrowIfError();
        }

        public bool GetFlag(VROverlayFlags flag)
        {
            bool value = false;
            OpenVR.Overlay.GetOverlayFlag(overlayHandle, flag, ref value).ThrowIfError();
            return value;
        }

        public bool this[VROverlayFlags flags]
        {
            get => GetFlag(flags);
            set => SetFlag(flags, value);
        }
    }
}