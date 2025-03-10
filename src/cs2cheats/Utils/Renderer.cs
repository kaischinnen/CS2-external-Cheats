using ClickableTransparentOverlay;
using ImGuiNET;
using System.Numerics;

public class Renderer : Overlay
{

    // checkbox values
    public bool aimbot = false;
    public bool fovAimbot = false;
    public bool aimOnTeam = false;
    public bool antiFlash = false;
    public bool radar = false;
    public bool bhop = false;
    public bool triggerbot = false;
    public bool smoothAimbot = false;
    public bool visablityCheck = false;

    public float smoothAimbotValue = 1.0f; // default value

    // fov information 
    public Vector2 screenSize = new Vector2(1920, 1080);
    public float FOV = 50; // in pixels
    public Vector4 circleColor = new Vector4(1, 1, 1, 1); // r, g, b, a
    public float circleThickness = 1.5f;

    // toggle variables, indicating whether the task for the feature is running or not.
    public int antiFlashRunning = 0;
    public int radarRunning = 0;
    public int bhopRunning = 0;
    public int triggerbotRunning = 0;
    public int aimbotRunning = 0;
    public int fovAimbotRunning = 0;

    // aimbot mode for toggling between aimbot and fov aimbot
    public int aimbotMode = -1; // -1 = OFF, 0 = aimbot, 1 = fov aimbot

    // window aesthetics
    public Vector4 windowBgColor = new Vector4(0.1f, 0.1f, 0.1f, 0.85f); // dark-ish semi-transparent window
    public Vector4 accentColor = new Vector4(0.9f, 0.3f, 0.3f, 1.0f); // highlight color

    // hotkey var and state
    public int hotkey = ImGuiKeyToVkey(ImGuiKey.MouseX2);
    public ImGuiKey iKey = ImGuiKey.MouseX2;
    public bool isWaitingForHotkey = false;

    protected override void Render()
    {   
        // set window pos
        ImGui.SetNextWindowPos(new Vector2(320, 40), ImGuiCond.FirstUseEver); // (320,40) is right next to radar on 19020x1080. FirstUseEver sets position only the first time the window is drawn, allowing it to be moved afterwards

        // Because "CS2 Cheat Settings" is the longest text, we can use it to set the initial window size
        Vector2 titleSize = ImGui.CalcTextSize("CS2 Cheat Settings");
        float minSWidth = titleSize.X + 50; // minimum width
        float extraWidth = smoothAimbot ? 100 : 0; // adjust width if smooth aimbot is enabled


        if (!fovAimbot)
        {
            ImGui.SetNextWindowSize(new Vector2(minSWidth + extraWidth, 0));
        }

        ImGui.Begin("CS2 Cheat Settings", ImGuiWindowFlags.AlwaysAutoResize); // resize window automatically based on content

        // hotkey config
        if (ImGui.CollapsingHeader("Change Hotkey"))
        {
            // set the header background color
            ImGui.PushStyleColor(ImGuiCol.Header, accentColor * new Vector4(0.3f, 0.3f, 0.3f, 1.0f)); // header bg
            ImGui.PushStyleColor(ImGuiCol.HeaderHovered, accentColor * new Vector4(0.3f, 0.3f, 0.3f, 1.0f)); // header hovered color
            ImGui.PushStyleColor(ImGuiCol.HeaderActive, accentColor * new Vector4(0.3f, 0.3f, 0.3f, 1.0f)); // header active color

            // set button color 
            ImGui.PushStyleColor(ImGuiCol.Button, accentColor * new Vector4(0.3f, 0.3f, 0.3f, 1.0f)); // button background 
            ImGui.PushStyleColor(ImGuiCol.ButtonHovered, accentColor * new Vector4(0.3f, 0.3f, 0.3f, 1.0f)); // hovererd button color
            ImGui.PushStyleColor(ImGuiCol.ButtonActive, accentColor * new Vector4(0.3f, 0.3f, 0.3f, 1.0f)); // active button color

            // if "Change hotkey" is pressed, open another button which waits until a button for a (new) hotkey is pressed:
            if (ImGui.Button("Set Hotkey"))
            {
                isWaitingForHotkey = true;
            }

            // while we are waiting for a button to be pressed:
            if (isWaitingForHotkey)
            {
                ImGui.Text("Press a key");

                // iterate through all the possible ImGui keys
                foreach (ImGuiKey key in Enum.GetValues(typeof(ImGuiKey)))
                {
                    // check if the key is pressed
                    if (ImGui.IsKeyPressed(key))
                    {
                        hotkey = ImGuiKeyToVkey(key);
                        iKey = key;
                        isWaitingForHotkey = false;
                        break;
                    }
                }
            }
            else
            {   
                // display current hotkey
                ImGui.Text("Current Hotkey: " + iKey.ToString());
            }
            ImGui.PopStyleColor(3);
            ImGui.PopStyleVar(3);
        }


        // checkboxes
        ImGui.Checkbox("AntiFlash", ref antiFlash);
        ImGui.Checkbox("Radar Hack", ref radar);
        ImGui.Checkbox("Bhop", ref bhop);
        ImGui.Checkbox("Triggerbot", ref triggerbot);
        ImGui.Checkbox("Target Teammates", ref aimOnTeam);
        ImGui.Checkbox("VisablityCheck", ref visablityCheck);
        ImGui.Checkbox("Smooth Aimbot", ref smoothAimbot);

        // if smooth aimbot is enabled, show slider
        if (smoothAimbot)
        {
            ImGui.SliderFloat("Smoothness", ref smoothAimbotValue, 1f, 50.0f); // min, max
        }

        // radio buttons to toggle between fov aimbot and aimbot
        if (ImGui.RadioButton("Aimbot", aimbotMode == 0))
        {
            // If the current mode is Aimbot, turn it off
            if (aimbotMode != 0)
            {
                aimbotMode = 0; // Set aimbot mode
                aimbot = true;   // Enable aimbot
                fovAimbot = false; // Disable FOV aimbot
                fovAimbotRunning = 0; // Reset FOV aimbot task
                aimbotRunning = 1;    // Enable aimbot task
            }
            else
            {
                aimbotMode = -1; // Turn off aimbot mode
                aimbot = false;  // Disable aimbot
                fovAimbot = false; // Disable FOV aimbot
                fovAimbotRunning = 0; // Reset FOV aimbot task
                aimbotRunning = 0;    // Disable aimbot task
            }
        }

        if (ImGui.RadioButton("FOV Aimbot", aimbotMode == 1))
        {
            // If the current mode is FOV Aimbot, turn it off
            if (aimbotMode != 1)
            {
                aimbotMode = 1; // Set FOV aimbot mode
                fovAimbot = true;  // Enable FOV aimbot
                aimbot = false;    // Disable aimbot
                aimbotRunning = 0;  // Reset aimbot task
                fovAimbotRunning = 1; // Enable FOV aimbot task
            }
            else
            {
                aimbotMode = -1;  // Turn off FOV aimbot mode
                aimbot = false;   // Disable aimbot
                fovAimbot = false; // Disable FOV aimbot
                fovAimbotRunning = 0; // Reset FOV aimbot task
                aimbotRunning = 0;   // Disable aimbot task
            }
        }

        // set window aesthetics
        ImGui.PushStyleColor(ImGuiCol.WindowBg, windowBgColor); // Set window background color
        ImGui.PushStyleColor(ImGuiCol.TitleBgActive, new Vector4(0.5f, 0.1f, 0.1f, 1.0f)); // Highlight title bar
        ImGui.PushStyleColor(ImGuiCol.FrameBg, new Vector4(0.9f, 0.3f, 0.3f, 1.0f)); // Frame background
        ImGui.PushStyleColor(ImGuiCol.CheckMark, accentColor); // Checkbox highlight
        ImGui.PushStyleVar(ImGuiStyleVar.WindowRounding, 10f); // Rounded corners
        ImGui.PushStyleVar(ImGuiStyleVar.FrameRounding, 5f); // Rounded frames
        ImGui.PushStyleVar(ImGuiStyleVar.GrabRounding, 5f); // Rounded sliders

        // set the slider's handle and background colors, including hover
        ImGui.PushStyleColor(ImGuiCol.SliderGrab, accentColor); // Slider handle color
        ImGui.PushStyleColor(ImGuiCol.SliderGrabActive, accentColor); // Active slider handle color
        ImGui.PushStyleColor(ImGuiCol.FrameBg, accentColor * new Vector4(0.3f, 0.3f, 0.3f, 1.0f)); // Slider background 

        // if fov aimbot is enabled, show fov settings
        if (fovAimbot)
        {
            ImGui.SliderFloat("FOV (Pixels)", ref FOV, 10, 300); // min, max
            ImGui.SliderFloat("FOV Circle Thickness", ref circleThickness, 0.5f, 8.0f); // adjust thickness

            // set the header background color
            ImGui.PushStyleColor(ImGuiCol.Header, accentColor * new Vector4(0.3f, 0.3f, 0.3f, 1.0f)); // header bg
            ImGui.PushStyleColor(ImGuiCol.HeaderHovered, accentColor * new Vector4(0.3f, 0.3f, 0.3f, 1.0f)); // header hovered color
            ImGui.PushStyleColor(ImGuiCol.HeaderActive, accentColor * new Vector4(0.3f, 0.3f, 0.3f, 1.0f)); // header active color

            if (ImGui.CollapsingHeader("FOV Circle Color")) // minimize color picker
            {
                // change color
                // take up full width of available space in the current layout (there used to be the preview color but because it was removed in the next line and i wanted to fill the empty space)
                ImGui.PushItemWidth(-1f);  // -1f fills up horizontal space
                ImGui.ColorPicker4("##circlecolor", ref circleColor, ImGuiColorEditFlags.NoSidePreview);
            }

            // draw cirlce
            DrawOverlay();
            ImDrawListPtr drawList = ImGui.GetForegroundDrawList(); // get the draw list

            int segmentCount = (int)Math.Max(20, FOV / 2); // amount of "line pieces" are used to draw the circle and the larger the circle (fov), the more segments you want for proper smoothness 

            drawList.AddCircle(new Vector2(screenSize.X / 2, screenSize.Y / 2), FOV, ImGui.ColorConvertFloat4ToU32(circleColor), segmentCount, (int)circleThickness); // center, radius, color, thickness
            ImGui.PopStyleColor(3);
            
            ImGui.End();
        } 
        ImGui.End();
    }

    void DrawOverlay()
    {
        ImGui.Begin("overlay", ImGuiWindowFlags.NoDecoration
            | ImGuiWindowFlags.NoBackground
            | ImGuiWindowFlags.NoBringToFrontOnFocus
            | ImGuiWindowFlags.NoMove
            | ImGuiWindowFlags.NoInputs
            | ImGuiWindowFlags.NoCollapse
            | ImGuiWindowFlags.NoScrollbar
            | ImGuiWindowFlags.NoScrollWithMouse
            );
    }

    // mapping ImGuiKey to Virtual Key Codes (ref: https://learn.microsoft.com/de-de/windows/win32/inputdev/virtual-key-codes)
    public static int ImGuiKeyToVkey(ImGuiKey key)
    {
        switch (key)
        {   
            // keyboard buttons
            case ImGuiKey.Tab: return 0x09;  // VK_TAB
            case ImGuiKey.LeftArrow: return 0x25;  // VK_LEFT
            case ImGuiKey.RightArrow: return 0x27;  // VK_RIGHT
            case ImGuiKey.UpArrow: return 0x26;  // VK_UP
            case ImGuiKey.DownArrow: return 0x28;  // VK_DOWN
            case ImGuiKey.PageUp: return 0x21;  // VK_PRIOR
            case ImGuiKey.PageDown: return 0x22;  // VK_NEXT
            case ImGuiKey.Home: return 0x24;  // VK_HOME
            case ImGuiKey.End: return 0x23;  // VK_END
            case ImGuiKey.Insert: return 0x2D;  // VK_INSERT
            case ImGuiKey.Delete: return 0x2E;  // VK_DELETE
            case ImGuiKey.Backspace: return 0x08;  // VK_BACK
            case ImGuiKey.Space: return 0x20;  // VK_SPACE
            case ImGuiKey.Enter: return 0x0D;  // VK_RETURN
            case ImGuiKey.Escape: return 0x1B;  // VK_ESCAPE
            case ImGuiKey.LeftCtrl: return 0xA2;  // VK_LCONTROL
            case ImGuiKey.LeftShift: return 0xA0;  // VK_LSHIFT
            case ImGuiKey.LeftAlt: return 0xA4;  // VK_LMENU
            case ImGuiKey.LeftSuper: return 0x5B;  // VK_LWIN
            case ImGuiKey.RightCtrl: return 0xA3;  // VK_RCONTROL
            case ImGuiKey.RightShift: return 0xA1;  // VK_RSHIFT
            case ImGuiKey.RightAlt: return 0xA5;  // VK_RMENU
            case ImGuiKey.RightSuper: return 0x5C;  // VK_RWIN
            case ImGuiKey.Menu: return 0x5D;  // VK_APPS
            case ImGuiKey._0: return 0x30;  // VK_0
            case ImGuiKey._1: return 0x31;  // VK_1
            case ImGuiKey._2: return 0x32;  // VK_2
            case ImGuiKey._3: return 0x33;  // VK_3
            case ImGuiKey._4: return 0x34;  // VK_4
            case ImGuiKey._5: return 0x35;  // VK_5
            case ImGuiKey._6: return 0x36;  // VK_6
            case ImGuiKey._7: return 0x37;  // VK_7
            case ImGuiKey._8: return 0x38;  // VK_8
            case ImGuiKey._9: return 0x39;  // VK_9
            case ImGuiKey.A: return 0x41;  // VK_A
            case ImGuiKey.B: return 0x42;  // VK_B
            case ImGuiKey.C: return 0x43;  // VK_C
            case ImGuiKey.D: return 0x44;  // VK_D
            case ImGuiKey.E: return 0x45;  // VK_E
            case ImGuiKey.F: return 0x46;  // VK_F
            case ImGuiKey.G: return 0x47;  // VK_G
            case ImGuiKey.H: return 0x48;  // VK_H
            case ImGuiKey.I: return 0x49;  // VK_I
            case ImGuiKey.J: return 0x4A;  // VK_J
            case ImGuiKey.K: return 0x4B;  // VK_K
            case ImGuiKey.L: return 0x4C;  // VK_L
            case ImGuiKey.M: return 0x4D;  // VK_M
            case ImGuiKey.N: return 0x4E;  // VK_N
            case ImGuiKey.O: return 0x4F;  // VK_O
            case ImGuiKey.P: return 0x50;  // VK_P
            case ImGuiKey.Q: return 0x51;  // VK_Q
            case ImGuiKey.R: return 0x52;  // VK_R
            case ImGuiKey.S: return 0x53;  // VK_S
            case ImGuiKey.T: return 0x54;  // VK_T
            case ImGuiKey.U: return 0x55;  // VK_U
            case ImGuiKey.V: return 0x56;  // VK_V
            case ImGuiKey.W: return 0x57;  // VK_W
            case ImGuiKey.X: return 0x58;  // VK_X
            case ImGuiKey.Y: return 0x59;  // VK_Y
            case ImGuiKey.Z: return 0x5A;  // VK_Z
            case ImGuiKey.F1: return 0x70;  // VK_F1
            case ImGuiKey.F2: return 0x71;  // VK_F2
            case ImGuiKey.F3: return 0x72;  // VK_F3
            case ImGuiKey.F4: return 0x73;  // VK_F4
            case ImGuiKey.F5: return 0x74;  // VK_F5
            case ImGuiKey.F6: return 0x75;  // VK_F6
            case ImGuiKey.F7: return 0x76;  // VK_F7
            case ImGuiKey.F8: return 0x77;  // VK_F8
            case ImGuiKey.F9: return 0x78;  // VK_F9
            case ImGuiKey.F10: return 0x79;  // VK_F10
            case ImGuiKey.F11: return 0x7A;  // VK_F11
            case ImGuiKey.F12: return 0x7B;  // VK_F12
            case ImGuiKey.CapsLock: return 0x14;  // VK_CAPITAL
            case ImGuiKey.NumLock: return 0x90;  // VK_NUMLOCK
            case ImGuiKey.ScrollLock: return 0x91;  // VK_SCROLL
            case ImGuiKey.Keypad0: return 0x60;  // VK_NUMPAD0
            case ImGuiKey.Keypad1: return 0x61;  // VK_NUMPAD1
            case ImGuiKey.Keypad2: return 0x62;  // VK_NUMPAD2
            case ImGuiKey.Keypad3: return 0x63;  // VK_NUMPAD3
            case ImGuiKey.Keypad4: return 0x64;  // VK_NUMPAD4
            case ImGuiKey.Keypad5: return 0x65;  // VK_NUMPAD5
            case ImGuiKey.Keypad6: return 0x66;  // VK_NUMPAD6
            case ImGuiKey.Keypad7: return 0x67;  // VK_NUMPAD7
            case ImGuiKey.Keypad8: return 0x68;  // VK_NUMPAD8
            case ImGuiKey.Keypad9: return 0x69;  // VK_NUMPAD9
            case ImGuiKey.KeypadDecimal: return 0x6E;  // VK_DECIMAL
            case ImGuiKey.KeypadDivide: return 0x6F;  // VK_DIVIDE
            case ImGuiKey.KeypadMultiply: return 0x6A;  // VK_MULTIPLY
            case ImGuiKey.KeypadSubtract: return 0x6D;  // VK_SUBTRACT
            case ImGuiKey.KeypadAdd: return 0x6B;  // VK_ADD
            case ImGuiKey.KeypadEnter: return 0x0D;  // VK_RETURN (same as Enter)

            // mouse buttons
            case ImGuiKey.MouseLeft: return 0x01;  // VK_LBUTTON
            case ImGuiKey.MouseRight: return 0x02;  // VK_RBUTTON
            case ImGuiKey.MouseMiddle: return 0x04;  // VK_MBUTTON
            case ImGuiKey.MouseX1: return 0x05;  // VK_XBUTTON1
            case ImGuiKey.MouseX2: return 0x06;  // VK_XBUTTON2

            default: return -1; // return -1 if the key is not mapped
        }
    }
}