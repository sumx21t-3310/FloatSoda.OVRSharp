using Valve.VR;

namespace OVRSharp.Exceptions;

public static class EVROverlayErrorExtension
{
    public static void ThrowIfError(this EVROverlayError error)
    {
        if (error == EVROverlayError.None) return;
        throw new OpenVRSystemException<EVROverlayError>($"An error occurred within an Overlay. {error}", error);
    }
}