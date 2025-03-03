using ClickableTransparentOverlay;
using ImGuiNET;
using System.Numerics;

public class Renderer : Overlay
{

    // checkbox values
    public bool aimbot = true;
    public bool aimOnTeam = false;
    public bool fovAimbot = false;
    public bool antiFlash = false;
    public bool radarHack = false;
    public bool bhop = false;
    public bool triggerbot = false;

    // fov information 
    public Vector2 screenSize = new Vector2(1920, 1080);
    public float FOV = 50; // in pixels
    public Vector4 circleColor = new Vector4(1, 1, 1, 1); // r, g, b, a

    // toggle variables, indicating whether the task for the feature is running or not.
    public int antiFlashRunning = 0;
    public int radarHackRunning = 0;
    public int bhopRunning = 0;
    public int triggerbotRunning = 0;

    // aimbot mode for toggling between aimbot and fov aimbot ( 0 = aimbot, 1 = fov aimbot)
    public int aimbotMode = 0;

    // window aesthetics
    public Vector4 windowBgColor = new Vector4(0.1f, 0.1f, 0.1f, 0.85f); // Dark semi-transparent window
    public Vector4 accentColor = new Vector4(0.9f, 0.3f, 0.3f, 1.0f); // Highlight color

    protected override void Render()
    {   
        // set window pos
        ImGui.SetNextWindowPos(new Vector2(320, 40), ImGuiCond.FirstUseEver); // (320,40) is right next to radar on 19020x1080. FirstUseEver sets position only the first time the window is drawn, allowing it to be moved afterwards

        // Because "CS2 Cheat Settings" is the longest text, we can use it to set the initial window size
        Vector2 titleSize = ImGui.CalcTextSize("CS2 Cheat Settings");

        if (!fovAimbot)
        {
            ImGui.SetNextWindowSize(new Vector2(titleSize.X + 40, 0));
        }

        ImGui.Begin("CS2 Cheat Settings", ImGuiWindowFlags.AlwaysAutoResize); // resize window automatically based on content

        ImGui.Checkbox("AntiFlash", ref antiFlash);
        ImGui.Checkbox("Radar Hack", ref radarHack);
        ImGui.Checkbox("Bhop", ref bhop);
        ImGui.Checkbox("Triggerbot", ref triggerbot);
        ImGui.Checkbox("Target Teammates", ref aimOnTeam);

        if (ImGui.RadioButton("Aimbot", aimbotMode == 0)) {
            aimbotMode = 0;
            fovAimbot = false;
        }
        if (ImGui.RadioButton("FOV Aimbot", aimbotMode == 1)) {
            aimbotMode = 1;
            fovAimbot = true;
        }

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

            if (ImGui.CollapsingHeader("FOV Circle Color")) // minimize color picker
            {
                // change color
                // take up full width of available space in the current layout (there used to be the preview color but because it was removed in the next line and i wanted to fill the empty space)
                ImGui.PushItemWidth(-1f);  // -1f fills up horizontal space
                ImGui.ColorPicker4("##circlecolor", ref circleColor, ImGuiColorEditFlags.NoSidePreview);
            }

            // draw cirlce
            DrawOverlay();
            ImDrawListPtr drawList = ImGui.GetWindowDrawList();
            drawList.AddCircle(new Vector2(screenSize.X / 2, screenSize.Y / 2), FOV, ImGui.ColorConvertFloat4ToU32(circleColor)); // center, radius, color
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