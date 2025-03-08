using ClickableTransparentOverlay;
using ImGuiNET;
using System.Diagnostics;
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
    public Vector4 windowBgColor = new Vector4(0.1f, 0.1f, 0.1f, 0.85f); // Dark semi-transparent window
    public Vector4 accentColor = new Vector4(0.9f, 0.3f, 0.3f, 1.0f); // Highlight color

    protected override void Render()
    {   
        // set window pos
        ImGui.SetNextWindowPos(new Vector2(320, 40), ImGuiCond.FirstUseEver); // (320,40) is right next to radar on 19020x1080. FirstUseEver sets position only the first time the window is drawn, allowing it to be moved afterwards

        // Because "CS2 Cheat Settings" is the longest text, we can use it to set the initial window size
        Vector2 titleSize = ImGui.CalcTextSize("CS2 Cheat Settings");
        float minSWidth = titleSize.X + 40; // minimum width
        float extraWidth = smoothAimbot ? 100 : 0; // adjust width if smooth aimbot is enabled


        if (!fovAimbot)
        {
            ImGui.SetNextWindowSize(new Vector2(minSWidth + extraWidth, 0));
        }

        ImGui.Begin("CS2 Cheat Settings", ImGuiWindowFlags.AlwaysAutoResize); // resize window automatically based on content

        // checkboxes
        ImGui.Checkbox("AntiFlash", ref antiFlash);
        ImGui.Checkbox("Radar Hack", ref radar);
        ImGui.Checkbox("Bhop", ref bhop);
        ImGui.Checkbox("Triggerbot", ref triggerbot);
        ImGui.Checkbox("Target Teammates", ref aimOnTeam);
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

        // Set the slider's handle and background colors, including hover
        ImGui.PushStyleColor(ImGuiCol.SliderGrab, accentColor); // Slider handle color
        ImGui.PushStyleColor(ImGuiCol.SliderGrabActive, accentColor); // Active slider handle color
        ImGui.PushStyleColor(ImGuiCol.FrameBg, accentColor * new Vector4(0.3f, 0.3f, 0.3f, 1.0f)); // Slider background 

        // if fov aimbot is enabled, show fov settings
        if (fovAimbot)
        {
            ImGui.SliderFloat("FOV (Pixels)", ref FOV, 10, 300); // min, max
            ImGui.SliderFloat("FOV Circle Thickness", ref circleThickness, 0.5f, 8.0f); // adjust thickness

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
}