using System.Runtime.InteropServices;

namespace FirstPersonTool;

[Flags]
internal enum XInputButtons : ushort
{
    DPadUp = 0x0001,
    DPadDown = 0x0002,
    DPadLeft = 0x0004,
    DPadRight = 0x0008,
    Start = 0x0010,
    Back = 0x0020,
    LeftThumb = 0x0040,
    RightThumb = 0x0080,
    LeftShoulder = 0x0100,
    RightShoulder = 0x0200,
    A = 0x1000,
    B = 0x2000,
    X = 0x4000,
    Y = 0x8000,
}

[StructLayout(LayoutKind.Sequential)]
internal struct XInputGamepad
{
    public XInputButtons wButtons;
    public byte bLeftTrigger;
    public byte bRightTrigger;
    public short sThumbLX;
    public short sThumbLY;
    public short sThumbRX;
    public short sThumbRY;
}

[StructLayout(LayoutKind.Sequential)]
internal struct XInputStateRaw
{
    public uint dwPacketNumber;
    public XInputGamepad Gamepad;
}

internal readonly record struct ControllerState(
    XInputButtons Buttons,
    byte LeftTrigger,
    byte RightTrigger,
    short ThumbRX,
    short ThumbRY
);

internal static class XInput
{
    [DllImport("xinput1_4.dll", EntryPoint = "XInputGetState")]
    private static extern int XInputGetState14(int dwUserIndex, out XInputStateRaw pState);

    public static bool TryGetState(int userIndex, out ControllerState state)
    {
        try
        {
            var rc = XInputGetState14(userIndex, out var raw);
            if (rc != 0)
            {
                state = default;
                return false;
            }

            state = new ControllerState(
                raw.Gamepad.wButtons,
                raw.Gamepad.bLeftTrigger,
                raw.Gamepad.bRightTrigger,
                raw.Gamepad.sThumbRX,
                raw.Gamepad.sThumbRY
            );
            return true;
        }
        catch (DllNotFoundException)
        {
            state = default;
            return false;
        }
    }
}

