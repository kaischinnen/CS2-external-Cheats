using ClickableTransparentOverlay;
using ImGuiNET;
using System.Numerics;

public class Renderer : Overlay {

    // checkbox values
    public bool aimbot = true;
    public bool aimOnTeam = false;
    public bool fovAimbot = false;

    // fov information 
    public Vector2 screenSize = new Vector2(1920, 1080);
    public float FOV = 50; // in pixels
    public Vector4 circleColor = new Vector4(1, 1, 1, 1); // r, g, b, a

    // window aesthetics
    public Vector4 windowBgColor = new Vector4(0.1f, 0.1f, 0.1f, 0.85f); // Dark semi-transparent window
    public Vector4 accentColor = new Vector4(0.9f, 0.3f, 0.3f, 1.0f); // Highlight color

    protected override void Render()
    {
        ImGui.Begin("Aimbot Settings", ImGuiWindowFlags.AlwaysAutoResize);
        ImGui.Checkbox("Enable Aimbot", ref aimbot);
        ImGui.Checkbox("Target Teammates", ref aimOnTeam);
        ImGui.Checkbox("Enable FOV Aimbot", ref fovAimbot);

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
        ImGui.PushStyleColor(ImGuiCol.FrameBg, accentColor * new Vector4(0.3f, 0.3f, 0.3f, 1.0f)); // Slider background color

        // if fov aimbot is enabled, show fov settings
        if (fovAimbot)
        {
            ImGui.SliderFloat("FOV (Pixels)", ref FOV, 10, 300); // min, max

            if (ImGui.CollapsingHeader("FOV Circle Color")) // minimize color picker
            {   
                // change color
                ImGui.ColorPicker4("##circlecolor", ref circleColor);
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
        ImGui.SetNextWindowSize(screenSize);
        ImGui.SetNextWindowPos(new Vector2(0, 0)); // top left
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