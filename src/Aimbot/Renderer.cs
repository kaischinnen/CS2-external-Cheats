using ClickableTransparentOverlay;
using ImGuiNET;

public class Renderer : Overlay {
    
    // checkbox values
    public bool aimbot = true;
    public bool aimOnTeam = false;
    protected override void Render()
    {
        ImGui.Begin("Aimbot");
        
        ImGui.Checkbox("Aimbot", ref aimbot);
        ImGui.Checkbox("Aim on teammates, aswell", ref aimOnTeam);
        ImGui.End();
    }
}