using ClickableTransparentOverlay;
using CS2Cheats.Utils;
using ImGuiNET;
using System.Collections.Concurrent;
using System.Numerics;

public class Renderer : Overlay
{
    // window state (used for closing the window)
    public bool isWindowOpen = true;

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
    public bool esp = false;
    public bool drawBones = true;
    public bool drawJoints = true;
    public bool drawLines = true;
    public bool drawRectangles = true;
    public bool drawHealth = true;

    // initial smooth aimbot value  
    public float smoothAimbotValue = 1.0f; 

    // fov information 
    public Vector2 screenSize = new Vector2(1920, 1080);
    public float FOV = 50; // in pixels
    public Vector4 circleColor = new Vector4(1, 1, 1, 1); // r, g, b, a
    public float circleThickness = 1.5f;

    // esp information

    // entity queue and objects (thread safe)
    public ConcurrentQueue<Entity> entities = new ConcurrentQueue<Entity>(); 
    public Entity localPlayer = new Entity();
    public readonly object entityLock = new object(); // lock for entities 
    ImDrawListPtr drawList;

    // esp colors
    Vector4 teamColor = new Vector4(0, 1, 0, 1); // green
    Vector4 enemyColor = new Vector4(1, 0, 0, 1);  // red
    Vector4 boneColor = new Vector4(1, 1, 1, 1); // default white
    Vector4 jointColor = new Vector4(255, 0, 210, 1); // default purple

    // esp settings
    public float boneThickness = 4;
    public float joinRadius = 3;
    public float lineThickness = 1.0f;
    public float rectThickness = 1.0f;

    // toggle variables, indicating whether the task for the feature is running or not.
    public int antiFlashRunning = 0;
    public int radarRunning = 0;
    public int bhopRunning = 0;
    public int triggerbotRunning = 0;
    public int aimbotRunning = 0;
    public int fovAimbotRunning = 0;
    public int espRunning = 0;

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

        // because the title is the longest text on initial state, we can use it to set the initial window size + some padding
        Vector2 titleSize = ImGui.CalcTextSize("CS2 Cheat Settings");
        float minSWidth = titleSize.X + 50; // minimum width
        float extraWidth = smoothAimbot ? 120 : 0; // add extra width if smooth aimbot is enabled (because slider is wider than minSWidth)

        // have small window opened if none of fovaimbot not esp are enabled as these take up more space
        if (!fovAimbot && !esp)
        {
            ImGui.SetNextWindowSize(new Vector2(minSWidth + extraWidth, 0));
        }

        // title bar
        ImGui.Begin("CS2 Cheat Settings", ref isWindowOpen, ImGuiWindowFlags.AlwaysAutoResize); // resize window automatically based on content

        // close window and stop process if close button is pressed
        if (!isWindowOpen)
        {
            Environment.Exit(0);
        } 

        // render hotkey button(s)
        RenderHotkey();

        // checkboxes
        ImGui.Checkbox("AntiFlash", ref antiFlash);
        ImGui.Checkbox("Radar Hack", ref radar);
        ImGui.Checkbox("Bhop", ref bhop);
        ImGui.Checkbox("Triggerbot", ref triggerbot);
        ImGui.Checkbox("Target Teammates", ref aimOnTeam);
        ImGui.Checkbox("Check Visability", ref visablityCheck);
        ImGui.Checkbox("Smooth Aimbot", ref smoothAimbot);

        // if smooth aimbot is enabled, show slider
        if (smoothAimbot)
        {
            ImGui.SliderFloat("Smoothness", ref smoothAimbotValue, 1f, 50.0f); // min, max
        }

        ImGui.Checkbox("Bone ESP", ref esp);

        RenderEsp();

        // render radio buttons for aimbot and fov aimbot (toggle between them)
        RenderAimboRadioButtons();

        // sets window aesthetics
        SetWindowAesthetics();

        // render fov aimbot
        RenderFovAimbot();

        ImGui.End();
    }

    // ------------------ Render Functions ------------------ // 
    private void RenderHotkey()
    {
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
    }
    private void RenderEsp()
    {
        // if esp is enabled
        if (esp)
        {
            ImGui.Separator();
            ImGui.Spacing();
            ImGui.Separator();

            ImGui.PushStyleColor(ImGuiCol.Header, accentColor * new Vector4(0.3f, 0.3f, 0.3f, 1.0f)); // header bg
            ImGui.PushStyleColor(ImGuiCol.HeaderHovered, accentColor * new Vector4(0.3f, 0.3f, 0.3f, 1.0f)); // header hovered color
            ImGui.PushStyleColor(ImGuiCol.HeaderActive, accentColor * new Vector4(0.3f, 0.3f, 0.3f, 1.0f)); // header active color

            // drawing functions 

            // draw health
            ImGui.Checkbox("Draw Health", ref drawHealth);

            // draw bones
            ImGui.Checkbox("Draw Bones", ref drawBones);
            if (drawBones)
            {
                ImGui.SliderFloat("Bone Thickness", ref boneThickness, 4, 600);

                if (ImGui.CollapsingHeader("Bone Color"))
                {
                    ImGui.PushItemWidth(-1f);
                    ImGui.ColorPicker4("##bonecolor", ref boneColor, ImGuiColorEditFlags.NoSidePreview);
                    ImGui.PopItemWidth();
                }
            }

            ImGui.Separator();

            // draw joints
            ImGui.Checkbox("Draw Joints", ref drawJoints);

            if (drawJoints)
            {
                ImGui.SliderFloat("Joint Radius", ref joinRadius, 1, 8);

                if (ImGui.CollapsingHeader("Joint Color"))
                {
                    ImGui.PushItemWidth(-1f);
                    ImGui.ColorPicker4("##jointcolor", ref jointColor, ImGuiColorEditFlags.NoSidePreview);
                    ImGui.PopItemWidth();

                }
            }

            ImGui.Separator();

            // draw lines
            ImGui.Checkbox("Draw Lines", ref drawLines);
            if (drawLines) ImGui.SliderFloat("Line Thickness", ref lineThickness, 1, 5);

            // draw rectangles
            ImGui.Checkbox("Draw Rectangles", ref drawRectangles);
            if (drawRectangles) ImGui.SliderFloat("Rectangle Thickness", ref rectThickness, 1, 5);

            // team
            if (drawRectangles || drawLines)
            {
                if (ImGui.CollapsingHeader("Team Color"))
                {
                    ImGui.PushItemWidth(-1f);
                    ImGui.ColorPicker4("##teamcolor", ref teamColor, ImGuiColorEditFlags.NoSidePreview);
                    ImGui.PopItemWidth();

                }

                if (ImGui.CollapsingHeader("Enemy Color"))
                {
                    ImGui.PushItemWidth(-1f);
                    ImGui.ColorPicker4("##enemycolor", ref enemyColor, ImGuiColorEditFlags.NoSidePreview);
                    ImGui.PopItemWidth();

                }
            }

            ImGui.Separator();
            ImGui.Spacing();
            ImGui.Separator();

            DrawOverlay();
            drawList = ImGui.GetWindowDrawList();

            foreach (var entity in entities)
            {
                // if entity is on screen, render esp accordingly
                if (IsEntityOnScreen(entity))
                {
                    // draw health bar
                    if (drawHealth) DrawHealth(entity);

                    // draw skeleton
                    if (drawBones) DrawBones(entity);

                    // draw joints
                    if (drawJoints) DrawJoints(entity);

                    // draw line
                    if (drawLines) DrawLine(entity);

                    // draw rectangle
                    if (drawRectangles) DrawRecangle(entity);
                }
            }
            ImGui.PopStyleColor(3);
            ImGui.End();
        }
    }
    private void SetWindowAesthetics()
    {
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
    }

    private void RenderFovAimbot()
    {
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
            ImDrawListPtr drawList2 = ImGui.GetForegroundDrawList(); // get the draw list

            int segmentCount = (int)Math.Max(20, FOV / 2); // amount of "line pieces" are used to draw the circle and the larger the circle (fov), the more segments you want for proper smoothness 

            drawList2.AddCircle(new Vector2(screenSize.X / 2, screenSize.Y / 2), FOV, ImGui.ColorConvertFloat4ToU32(circleColor), segmentCount, (int)circleThickness); // center, radius, color, thickness
            ImGui.PopStyleColor(3);

            ImGui.End();
        }
    }

    private void RenderAimboRadioButtons()
    {
        // radio buttons to toggle between fov aimbot and aimbot

        // if aimbot is enabled
        if (ImGui.RadioButton("Aimbot", aimbotMode == 0))
        {
            // turn on aimbot if it is not already enabled
            if (aimbotMode != 0)
            {
                aimbotMode = 0;
                aimbot = true;
                fovAimbot = false;
                fovAimbotRunning = 0;
                aimbotRunning = 1;
            }
            // turn off aimbot if it is already enabled
            else
            {
                aimbotMode = -1;
                aimbot = false;
                fovAimbot = false;
                fovAimbotRunning = 0;
                aimbotRunning = 0;
            }
        }

        if (ImGui.RadioButton("FOV Aimbot", aimbotMode == 1))
        {
            // turn on fov aimbot if it is not already enabled
            if (aimbotMode != 1)
            {
                aimbotMode = 1;
                fovAimbot = true;
                aimbot = false;
                aimbotRunning = 0;
                fovAimbotRunning = 1;
            }
            // turn off fov aimbot if it is already enabled
            else
            {
                aimbotMode = -1;
                aimbot = false;
                fovAimbot = false;
                fovAimbotRunning = 0;
                aimbotRunning = 0;
            }
        }
    }

    // ------------------ ESP Functions ------------------ //
    private void DrawBones(Entity entity)
    {   
        uint uintColor = ImGui.ColorConvertFloat4ToU32(boneColor);

        float currentBoneThickness = boneThickness / entity.distance; // scale thickness based on distance

        // draw lines beween bones
        drawList.AddLine(entity.bones2d[1], entity.bones2d[2], uintColor, currentBoneThickness); // neck to head
        drawList.AddLine(entity.bones2d[1], entity.bones2d[3], uintColor, currentBoneThickness); // neck to shoulderLeft
        drawList.AddLine(entity.bones2d[1], entity.bones2d[6], uintColor, currentBoneThickness); // neck to shoulderRight
        drawList.AddLine(entity.bones2d[3], entity.bones2d[4], uintColor, currentBoneThickness); // shoulderLeft to armLeft
        drawList.AddLine(entity.bones2d[6], entity.bones2d[7], uintColor, currentBoneThickness); // shoulderRight to armRight
        drawList.AddLine(entity.bones2d[4], entity.bones2d[5], uintColor, currentBoneThickness); // armLeft to handLeft
        drawList.AddLine(entity.bones2d[7], entity.bones2d[8], uintColor, currentBoneThickness); // armRight to handRight
        drawList.AddLine(entity.bones2d[1], entity.bones2d[0], uintColor, currentBoneThickness); // neck to waist
        drawList.AddLine(entity.bones2d[0], entity.bones2d[9], uintColor, currentBoneThickness); // waist to kneeLeft
        drawList.AddLine(entity.bones2d[0], entity.bones2d[11], uintColor, currentBoneThickness); // waist to kneeRight
        drawList.AddLine(entity.bones2d[9], entity.bones2d[10], uintColor, currentBoneThickness); // kneeLeft to feetLeft
        drawList.AddLine(entity.bones2d[11], entity.bones2d[12], uintColor, currentBoneThickness); // kneeLeft to feetLeft

        // draw circle on head
        drawList.AddCircle(entity.bones2d[2], currentBoneThickness + 3, uintColor);
    }

    private void DrawJoints(Entity entity)
    {
        foreach (Vector2 joints in entity.bones2d)
        {
            drawList.AddCircleFilled(joints, joinRadius, ImGui.ColorConvertFloat4ToU32(jointColor));
        }
    }

    private void DrawLine(Entity entity)
    {
        Vector4 lineColor = localPlayer.team == entity.team ? teamColor : enemyColor; // set color based on team

        // draw line from the bottom mid of the screen to the entity
        drawList.AddLine(new Vector2(screenSize.X / 2, screenSize.Y), entity.position2d, ImGui.ColorConvertFloat4ToU32(lineColor), lineThickness); 
    }

    private void DrawRecangle(Entity entity)
    {
        // set color based on team
        Vector4 rectColor = localPlayer.team == entity.team ? teamColor : enemyColor; 

        // height = head2d - feet2d
        float entityHeight = entity.viewPosition2d.Y - entity.position2d.Y;

        // calc rect dimensions

        // top left corner
        Vector2 rectTop = new Vector2(entity.position2d.X - entityHeight / 4, entity.position2d.Y); // position2d.X - entityHeight / 3, push X to the left

        // bottom right corner
        Vector2 rectBottom = new Vector2(entity.position2d.X + entityHeight / 4, entity.viewPosition2d.Y); // push X to the right

        drawList.AddRect(rectTop, rectBottom, ImGui.ColorConvertFloat4ToU32(rectColor), 0.0f, ImDrawFlags.None, rectThickness);
    }

    private void DrawHealth(Entity entity)
    {
        float entityHeight = entity.position2d.Y - entity.viewPosition2d.Y;

        float boxLeft = entity.position2d.X - entityHeight / 4;
        float boxRight = entity.position2d.X + entityHeight / 4;

        float barPercentWidth = 0.05f; // 5% of the box width
        float barPixelWidth = barPercentWidth * (boxRight - boxLeft);

        float barHeight = entityHeight * (entity.health / 100f);

        Vector2 barTop = new Vector2(boxLeft - barPixelWidth, entity.position2d.Y - barHeight);
        Vector2 barBottom = new Vector2(boxLeft, entity.position2d.Y);

        Vector4 barColor = new Vector4(0, 1, 0, 1);

        drawList.AddRectFilled(barTop, barBottom, ImGui.ColorConvertFloat4ToU32(barColor));
    }

    private bool IsEntityOnScreen(Entity entity)
    {
        if (entity.position2d.X > 0 && entity.position2d.X < screenSize.X && entity.position2d.Y > 0 && entity.position2d.Y < screenSize.Y)
        {
            return true;
        }
        return false;
    }

    public void UpdateEntities(IEnumerable<Entity> newEntities) // update entity queue
    {
        entities = new ConcurrentQueue<Entity>(newEntities);
    } 

    public void UpdateLocalPlayer(Entity newEntity)
    {
        lock (entityLock)
        {
            localPlayer = newEntity;
        }
    }

    // ------------------ ImGui Functions ------------------ //
    private void DrawOverlay()
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

    // mapping ImGuiKey to Virtual Key Codes (see: https://learn.microsoft.com/de-de/windows/win32/inputdev/virtual-key-codes)
    private static int ImGuiKeyToVkey(ImGuiKey key)
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