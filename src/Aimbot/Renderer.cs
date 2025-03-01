using ClickableTransparentOverlay;
using ImGuiNET;
using System.Numerics;

public class Renderer : Overlay {
    
    // checkbox values
    public bool aimbot = true;
    public bool aimOnTeam = false;
    public bool fovAimbot = false;

    public Vector2 screenSize = new Vector2(1920, 1080);
    public float FOV = 50; // in pixels
    public Vector4 circleColor = new Vector4(1, 1, 1, 1); // r, g, b, a
    protected override void Render()
    {
        ImGui.Begin("Aimbot");
        ImGui.Checkbox("Aimbot", ref aimbot);
        ImGui.Checkbox("Aim on teammates aswell", ref aimOnTeam);
        ImGui.Checkbox("FOV Aimbot", ref fovAimbot);

        if (fovAimbot)
        {
            ImGui.SliderFloat("Pixel FOV", ref FOV, 10, 300); // min, max

            if (ImGui.CollapsingHeader("FOV Circle Color")) // minimize color picker
            {
                ImGui.ColorPicker4("##circlecolor", ref circleColor);
                // change color
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