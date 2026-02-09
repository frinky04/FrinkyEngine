using System.Numerics;
using Hexa.NET.ImGui;
using Raylib_cs;

namespace FrinkyEngine.Editor;

public enum EditorThemeId
{
    Dark,
    Light,
    Nord,
    SolarizedDark,
    Catppuccin
}

public static class EditorTheme
{
    public static EditorThemeId Current { get; private set; } = EditorThemeId.Dark;
    public static Color ClearColor { get; private set; } = new(30, 30, 30, 255);

    public static void Apply(EditorThemeId themeId)
    {
        Current = themeId;

        // Start from dark baseline for safety
        ImGui.StyleColorsDark();

        switch (themeId)
        {
            case EditorThemeId.Dark:
                ApplyDark();
                break;
            case EditorThemeId.Light:
                ApplyLight();
                break;
            case EditorThemeId.Nord:
                ApplyNord();
                break;
            case EditorThemeId.SolarizedDark:
                ApplySolarizedDark();
                break;
            case EditorThemeId.Catppuccin:
                ApplyCatppuccin();
                break;
        }
    }

    private static unsafe void SetColor(ImGuiCol col, float r, float g, float b, float a = 1.0f)
    {
        var colors = ImGui.GetStyle().Colors;
        colors[(int)col] = new Vector4(r, g, b, a);
    }

    private static void SetColor(ImGuiCol col, int r, int g, int b, int a = 255)
    {
        SetColor(col, r / 255f, g / 255f, b / 255f, a / 255f);
    }

    private static void ApplyDark()
    {
        ClearColor = new Color(30, 30, 30, 255);

        SetColor(ImGuiCol.Text, 230, 230, 230);
        SetColor(ImGuiCol.TextDisabled, 128, 128, 128);
        SetColor(ImGuiCol.WindowBg, 30, 30, 30);
        SetColor(ImGuiCol.ChildBg, 30, 30, 30, 0);
        SetColor(ImGuiCol.PopupBg, 36, 36, 36);
        SetColor(ImGuiCol.Border, 60, 60, 60);
        SetColor(ImGuiCol.BorderShadow, 0, 0, 0, 0);
        SetColor(ImGuiCol.FrameBg, 50, 50, 50);
        SetColor(ImGuiCol.FrameBgHovered, 65, 65, 65);
        SetColor(ImGuiCol.FrameBgActive, 75, 75, 75);
        SetColor(ImGuiCol.TitleBg, 22, 22, 22);
        SetColor(ImGuiCol.TitleBgActive, 28, 28, 28);
        SetColor(ImGuiCol.TitleBgCollapsed, 22, 22, 22);
        SetColor(ImGuiCol.MenuBarBg, 36, 36, 36);
        SetColor(ImGuiCol.ScrollbarBg, 22, 22, 22);
        SetColor(ImGuiCol.ScrollbarGrab, 60, 60, 60);
        SetColor(ImGuiCol.ScrollbarGrabHovered, 80, 80, 80);
        SetColor(ImGuiCol.ScrollbarGrabActive, 100, 100, 100);
        SetColor(ImGuiCol.CheckMark, 100, 160, 230);
        SetColor(ImGuiCol.SliderGrab, 80, 130, 200);
        SetColor(ImGuiCol.SliderGrabActive, 100, 160, 230);
        SetColor(ImGuiCol.Button, 55, 55, 55);
        SetColor(ImGuiCol.ButtonHovered, 70, 70, 70);
        SetColor(ImGuiCol.ButtonActive, 85, 85, 85);
        SetColor(ImGuiCol.Header, 50, 50, 50);
        SetColor(ImGuiCol.HeaderHovered, 65, 65, 65);
        SetColor(ImGuiCol.HeaderActive, 75, 75, 75);
        SetColor(ImGuiCol.Separator, 60, 60, 60);
        SetColor(ImGuiCol.SeparatorHovered, 80, 130, 200);
        SetColor(ImGuiCol.SeparatorActive, 100, 160, 230);
        SetColor(ImGuiCol.ResizeGrip, 50, 50, 50, 64);
        SetColor(ImGuiCol.ResizeGripHovered, 80, 130, 200);
        SetColor(ImGuiCol.ResizeGripActive, 100, 160, 230);
        SetColor(ImGuiCol.Tab, 36, 36, 36);
        SetColor(ImGuiCol.TabHovered, 65, 65, 65);
        SetColor(ImGuiCol.TabSelected, 50, 50, 50);
        SetColor(ImGuiCol.TabDimmed, 30, 30, 30);
        SetColor(ImGuiCol.TabDimmedSelected, 40, 40, 40);
        SetColor(ImGuiCol.DockingPreview, 80, 130, 200, 178);
        SetColor(ImGuiCol.DockingEmptyBg, 20, 20, 20);
        SetColor(ImGuiCol.PlotLines, 155, 155, 155);
        SetColor(ImGuiCol.PlotLinesHovered, 255, 110, 90);
        SetColor(ImGuiCol.PlotHistogram, 230, 180, 50);
        SetColor(ImGuiCol.PlotHistogramHovered, 255, 150, 0);
        SetColor(ImGuiCol.TableHeaderBg, 40, 40, 40);
        SetColor(ImGuiCol.TableBorderStrong, 55, 55, 55);
        SetColor(ImGuiCol.TableBorderLight, 40, 40, 40);
        SetColor(ImGuiCol.TableRowBg, 0, 0, 0, 0);
        SetColor(ImGuiCol.TableRowBgAlt, 255, 255, 255, 15);
        SetColor(ImGuiCol.TextSelectedBg, 80, 130, 200, 90);
        SetColor(ImGuiCol.DragDropTarget, 100, 160, 230);
        SetColor(ImGuiCol.NavCursor, 100, 160, 230);
        SetColor(ImGuiCol.NavWindowingHighlight, 255, 255, 255, 178);
        SetColor(ImGuiCol.NavWindowingDimBg, 200, 200, 200, 50);
        SetColor(ImGuiCol.ModalWindowDimBg, 0, 0, 0, 90);
    }

    private static void ApplyLight()
    {
        ClearColor = new Color(220, 220, 220, 255);

        SetColor(ImGuiCol.Text, 30, 30, 30);
        SetColor(ImGuiCol.TextDisabled, 140, 140, 140);
        SetColor(ImGuiCol.WindowBg, 240, 240, 240);
        SetColor(ImGuiCol.ChildBg, 240, 240, 240, 0);
        SetColor(ImGuiCol.PopupBg, 250, 250, 250);
        SetColor(ImGuiCol.Border, 190, 190, 190);
        SetColor(ImGuiCol.BorderShadow, 0, 0, 0, 0);
        SetColor(ImGuiCol.FrameBg, 225, 225, 225);
        SetColor(ImGuiCol.FrameBgHovered, 210, 210, 210);
        SetColor(ImGuiCol.FrameBgActive, 195, 195, 195);
        SetColor(ImGuiCol.TitleBg, 210, 210, 210);
        SetColor(ImGuiCol.TitleBgActive, 200, 200, 200);
        SetColor(ImGuiCol.TitleBgCollapsed, 220, 220, 220);
        SetColor(ImGuiCol.MenuBarBg, 230, 230, 230);
        SetColor(ImGuiCol.ScrollbarBg, 230, 230, 230);
        SetColor(ImGuiCol.ScrollbarGrab, 180, 180, 180);
        SetColor(ImGuiCol.ScrollbarGrabHovered, 160, 160, 160);
        SetColor(ImGuiCol.ScrollbarGrabActive, 140, 140, 140);
        SetColor(ImGuiCol.CheckMark, 40, 100, 180);
        SetColor(ImGuiCol.SliderGrab, 50, 110, 190);
        SetColor(ImGuiCol.SliderGrabActive, 40, 100, 180);
        SetColor(ImGuiCol.Button, 215, 215, 215);
        SetColor(ImGuiCol.ButtonHovered, 200, 200, 200);
        SetColor(ImGuiCol.ButtonActive, 185, 185, 185);
        SetColor(ImGuiCol.Header, 210, 210, 210);
        SetColor(ImGuiCol.HeaderHovered, 195, 195, 195);
        SetColor(ImGuiCol.HeaderActive, 180, 180, 180);
        SetColor(ImGuiCol.Separator, 190, 190, 190);
        SetColor(ImGuiCol.SeparatorHovered, 50, 110, 190);
        SetColor(ImGuiCol.SeparatorActive, 40, 100, 180);
        SetColor(ImGuiCol.ResizeGrip, 190, 190, 190, 64);
        SetColor(ImGuiCol.ResizeGripHovered, 50, 110, 190);
        SetColor(ImGuiCol.ResizeGripActive, 40, 100, 180);
        SetColor(ImGuiCol.Tab, 220, 220, 220);
        SetColor(ImGuiCol.TabHovered, 200, 200, 200);
        SetColor(ImGuiCol.TabSelected, 235, 235, 235);
        SetColor(ImGuiCol.TabDimmed, 230, 230, 230);
        SetColor(ImGuiCol.TabDimmedSelected, 240, 240, 240);
        SetColor(ImGuiCol.DockingPreview, 50, 110, 190, 178);
        SetColor(ImGuiCol.DockingEmptyBg, 230, 230, 230);
        SetColor(ImGuiCol.PlotLines, 100, 100, 100);
        SetColor(ImGuiCol.PlotLinesHovered, 220, 80, 60);
        SetColor(ImGuiCol.PlotHistogram, 200, 150, 30);
        SetColor(ImGuiCol.PlotHistogramHovered, 230, 120, 0);
        SetColor(ImGuiCol.TableHeaderBg, 210, 210, 210);
        SetColor(ImGuiCol.TableBorderStrong, 180, 180, 180);
        SetColor(ImGuiCol.TableBorderLight, 200, 200, 200);
        SetColor(ImGuiCol.TableRowBg, 0, 0, 0, 0);
        SetColor(ImGuiCol.TableRowBgAlt, 0, 0, 0, 15);
        SetColor(ImGuiCol.TextSelectedBg, 50, 110, 190, 90);
        SetColor(ImGuiCol.DragDropTarget, 40, 100, 180);
        SetColor(ImGuiCol.NavCursor, 40, 100, 180);
        SetColor(ImGuiCol.NavWindowingHighlight, 255, 255, 255, 178);
        SetColor(ImGuiCol.NavWindowingDimBg, 50, 50, 50, 50);
        SetColor(ImGuiCol.ModalWindowDimBg, 0, 0, 0, 90);
    }

    private static void ApplyNord()
    {
        // Nord palette: https://www.nordtheme.com/
        // Polar Night: 46,52,64 / 59,66,82 / 67,76,94 / 76,86,106
        // Snow Storm: 216,222,233 / 229,233,240 / 236,239,244
        // Frost: 143,188,187 / 136,192,208 / 129,161,193 / 94,129,172
        // Aurora: 191,97,106 / 208,135,112 / 235,203,139 / 163,190,140 / 180,142,173
        ClearColor = new Color(46, 52, 64, 255);

        SetColor(ImGuiCol.Text, 236, 239, 244);
        SetColor(ImGuiCol.TextDisabled, 76, 86, 106);
        SetColor(ImGuiCol.WindowBg, 46, 52, 64);
        SetColor(ImGuiCol.ChildBg, 46, 52, 64, 0);
        SetColor(ImGuiCol.PopupBg, 52, 58, 72);
        SetColor(ImGuiCol.Border, 67, 76, 94);
        SetColor(ImGuiCol.BorderShadow, 0, 0, 0, 0);
        SetColor(ImGuiCol.FrameBg, 59, 66, 82);
        SetColor(ImGuiCol.FrameBgHovered, 67, 76, 94);
        SetColor(ImGuiCol.FrameBgActive, 76, 86, 106);
        SetColor(ImGuiCol.TitleBg, 40, 46, 56);
        SetColor(ImGuiCol.TitleBgActive, 46, 52, 64);
        SetColor(ImGuiCol.TitleBgCollapsed, 40, 46, 56);
        SetColor(ImGuiCol.MenuBarBg, 52, 58, 72);
        SetColor(ImGuiCol.ScrollbarBg, 46, 52, 64);
        SetColor(ImGuiCol.ScrollbarGrab, 67, 76, 94);
        SetColor(ImGuiCol.ScrollbarGrabHovered, 76, 86, 106);
        SetColor(ImGuiCol.ScrollbarGrabActive, 94, 129, 172);
        SetColor(ImGuiCol.CheckMark, 136, 192, 208);
        SetColor(ImGuiCol.SliderGrab, 129, 161, 193);
        SetColor(ImGuiCol.SliderGrabActive, 136, 192, 208);
        SetColor(ImGuiCol.Button, 59, 66, 82);
        SetColor(ImGuiCol.ButtonHovered, 67, 76, 94);
        SetColor(ImGuiCol.ButtonActive, 76, 86, 106);
        SetColor(ImGuiCol.Header, 59, 66, 82);
        SetColor(ImGuiCol.HeaderHovered, 67, 76, 94);
        SetColor(ImGuiCol.HeaderActive, 76, 86, 106);
        SetColor(ImGuiCol.Separator, 67, 76, 94);
        SetColor(ImGuiCol.SeparatorHovered, 129, 161, 193);
        SetColor(ImGuiCol.SeparatorActive, 136, 192, 208);
        SetColor(ImGuiCol.ResizeGrip, 67, 76, 94, 64);
        SetColor(ImGuiCol.ResizeGripHovered, 129, 161, 193);
        SetColor(ImGuiCol.ResizeGripActive, 136, 192, 208);
        SetColor(ImGuiCol.Tab, 52, 58, 72);
        SetColor(ImGuiCol.TabHovered, 67, 76, 94);
        SetColor(ImGuiCol.TabSelected, 59, 66, 82);
        SetColor(ImGuiCol.TabDimmed, 46, 52, 64);
        SetColor(ImGuiCol.TabDimmedSelected, 52, 58, 72);
        SetColor(ImGuiCol.DockingPreview, 129, 161, 193, 178);
        SetColor(ImGuiCol.DockingEmptyBg, 40, 46, 56);
        SetColor(ImGuiCol.PlotLines, 143, 188, 187);
        SetColor(ImGuiCol.PlotLinesHovered, 191, 97, 106);
        SetColor(ImGuiCol.PlotHistogram, 235, 203, 139);
        SetColor(ImGuiCol.PlotHistogramHovered, 208, 135, 112);
        SetColor(ImGuiCol.TableHeaderBg, 52, 58, 72);
        SetColor(ImGuiCol.TableBorderStrong, 67, 76, 94);
        SetColor(ImGuiCol.TableBorderLight, 59, 66, 82);
        SetColor(ImGuiCol.TableRowBg, 0, 0, 0, 0);
        SetColor(ImGuiCol.TableRowBgAlt, 255, 255, 255, 10);
        SetColor(ImGuiCol.TextSelectedBg, 129, 161, 193, 90);
        SetColor(ImGuiCol.DragDropTarget, 136, 192, 208);
        SetColor(ImGuiCol.NavCursor, 136, 192, 208);
        SetColor(ImGuiCol.NavWindowingHighlight, 236, 239, 244, 178);
        SetColor(ImGuiCol.NavWindowingDimBg, 200, 200, 200, 50);
        SetColor(ImGuiCol.ModalWindowDimBg, 0, 0, 0, 90);
    }

    private static void ApplySolarizedDark()
    {
        // Solarized palette
        // Base03: 0,43,54  Base02: 7,54,66  Base01: 88,110,117
        // Base00: 101,123,131  Base0: 131,148,150  Base1: 147,161,161
        // Yellow: 181,137,0  Orange: 203,75,22  Red: 220,50,47
        // Magenta: 211,54,130  Violet: 108,113,196  Blue: 38,139,210
        // Cyan: 42,161,152  Green: 133,153,0
        ClearColor = new Color(0, 43, 54, 255);

        SetColor(ImGuiCol.Text, 147, 161, 161);
        SetColor(ImGuiCol.TextDisabled, 88, 110, 117);
        SetColor(ImGuiCol.WindowBg, 0, 43, 54);
        SetColor(ImGuiCol.ChildBg, 0, 43, 54, 0);
        SetColor(ImGuiCol.PopupBg, 4, 48, 60);
        SetColor(ImGuiCol.Border, 7, 54, 66);
        SetColor(ImGuiCol.BorderShadow, 0, 0, 0, 0);
        SetColor(ImGuiCol.FrameBg, 7, 54, 66);
        SetColor(ImGuiCol.FrameBgHovered, 12, 62, 75);
        SetColor(ImGuiCol.FrameBgActive, 18, 70, 84);
        SetColor(ImGuiCol.TitleBg, 0, 36, 46);
        SetColor(ImGuiCol.TitleBgActive, 0, 43, 54);
        SetColor(ImGuiCol.TitleBgCollapsed, 0, 36, 46);
        SetColor(ImGuiCol.MenuBarBg, 4, 48, 60);
        SetColor(ImGuiCol.ScrollbarBg, 0, 36, 46);
        SetColor(ImGuiCol.ScrollbarGrab, 7, 54, 66);
        SetColor(ImGuiCol.ScrollbarGrabHovered, 88, 110, 117);
        SetColor(ImGuiCol.ScrollbarGrabActive, 101, 123, 131);
        SetColor(ImGuiCol.CheckMark, 38, 139, 210);
        SetColor(ImGuiCol.SliderGrab, 38, 139, 210);
        SetColor(ImGuiCol.SliderGrabActive, 42, 161, 152);
        SetColor(ImGuiCol.Button, 7, 54, 66);
        SetColor(ImGuiCol.ButtonHovered, 12, 62, 75);
        SetColor(ImGuiCol.ButtonActive, 18, 70, 84);
        SetColor(ImGuiCol.Header, 7, 54, 66);
        SetColor(ImGuiCol.HeaderHovered, 12, 62, 75);
        SetColor(ImGuiCol.HeaderActive, 18, 70, 84);
        SetColor(ImGuiCol.Separator, 7, 54, 66);
        SetColor(ImGuiCol.SeparatorHovered, 38, 139, 210);
        SetColor(ImGuiCol.SeparatorActive, 42, 161, 152);
        SetColor(ImGuiCol.ResizeGrip, 7, 54, 66, 64);
        SetColor(ImGuiCol.ResizeGripHovered, 38, 139, 210);
        SetColor(ImGuiCol.ResizeGripActive, 42, 161, 152);
        SetColor(ImGuiCol.Tab, 4, 48, 60);
        SetColor(ImGuiCol.TabHovered, 12, 62, 75);
        SetColor(ImGuiCol.TabSelected, 7, 54, 66);
        SetColor(ImGuiCol.TabDimmed, 0, 43, 54);
        SetColor(ImGuiCol.TabDimmedSelected, 4, 48, 60);
        SetColor(ImGuiCol.DockingPreview, 38, 139, 210, 178);
        SetColor(ImGuiCol.DockingEmptyBg, 0, 36, 46);
        SetColor(ImGuiCol.PlotLines, 42, 161, 152);
        SetColor(ImGuiCol.PlotLinesHovered, 220, 50, 47);
        SetColor(ImGuiCol.PlotHistogram, 181, 137, 0);
        SetColor(ImGuiCol.PlotHistogramHovered, 203, 75, 22);
        SetColor(ImGuiCol.TableHeaderBg, 4, 48, 60);
        SetColor(ImGuiCol.TableBorderStrong, 7, 54, 66);
        SetColor(ImGuiCol.TableBorderLight, 4, 48, 60);
        SetColor(ImGuiCol.TableRowBg, 0, 0, 0, 0);
        SetColor(ImGuiCol.TableRowBgAlt, 255, 255, 255, 8);
        SetColor(ImGuiCol.TextSelectedBg, 38, 139, 210, 90);
        SetColor(ImGuiCol.DragDropTarget, 42, 161, 152);
        SetColor(ImGuiCol.NavCursor, 38, 139, 210);
        SetColor(ImGuiCol.NavWindowingHighlight, 147, 161, 161, 178);
        SetColor(ImGuiCol.NavWindowingDimBg, 200, 200, 200, 50);
        SetColor(ImGuiCol.ModalWindowDimBg, 0, 0, 0, 90);
    }

    private static void ApplyCatppuccin()
    {
        // Catppuccin Mocha palette
        // Rosewater: 245,224,220  Flamingo: 242,205,205  Pink: 245,194,231
        // Mauve: 203,166,247  Red: 243,139,168  Maroon: 235,160,172
        // Peach: 250,179,135  Yellow: 249,226,175  Green: 166,227,161
        // Teal: 148,226,213  Sky: 137,220,235  Sapphire: 116,199,236
        // Blue: 137,180,250  Lavender: 180,190,254
        // Text: 205,214,244  Subtext1: 186,194,222  Subtext0: 166,173,200
        // Overlay2: 147,153,178  Overlay1: 127,132,156  Overlay0: 108,112,134
        // Surface2: 88,91,112  Surface1: 69,71,90  Surface0: 49,50,68
        // Base: 30,30,46  Mantle: 24,24,37  Crust: 17,17,27
        ClearColor = new Color(30, 30, 46, 255);

        SetColor(ImGuiCol.Text, 205, 214, 244);
        SetColor(ImGuiCol.TextDisabled, 108, 112, 134);
        SetColor(ImGuiCol.WindowBg, 30, 30, 46);
        SetColor(ImGuiCol.ChildBg, 30, 30, 46, 0);
        SetColor(ImGuiCol.PopupBg, 24, 24, 37);
        SetColor(ImGuiCol.Border, 49, 50, 68);
        SetColor(ImGuiCol.BorderShadow, 0, 0, 0, 0);
        SetColor(ImGuiCol.FrameBg, 49, 50, 68);
        SetColor(ImGuiCol.FrameBgHovered, 69, 71, 90);
        SetColor(ImGuiCol.FrameBgActive, 88, 91, 112);
        SetColor(ImGuiCol.TitleBg, 24, 24, 37);
        SetColor(ImGuiCol.TitleBgActive, 30, 30, 46);
        SetColor(ImGuiCol.TitleBgCollapsed, 24, 24, 37);
        SetColor(ImGuiCol.MenuBarBg, 24, 24, 37);
        SetColor(ImGuiCol.ScrollbarBg, 24, 24, 37);
        SetColor(ImGuiCol.ScrollbarGrab, 69, 71, 90);
        SetColor(ImGuiCol.ScrollbarGrabHovered, 88, 91, 112);
        SetColor(ImGuiCol.ScrollbarGrabActive, 108, 112, 134);
        SetColor(ImGuiCol.CheckMark, 137, 180, 250);
        SetColor(ImGuiCol.SliderGrab, 137, 180, 250);
        SetColor(ImGuiCol.SliderGrabActive, 180, 190, 254);
        SetColor(ImGuiCol.Button, 49, 50, 68);
        SetColor(ImGuiCol.ButtonHovered, 69, 71, 90);
        SetColor(ImGuiCol.ButtonActive, 88, 91, 112);
        SetColor(ImGuiCol.Header, 49, 50, 68);
        SetColor(ImGuiCol.HeaderHovered, 69, 71, 90);
        SetColor(ImGuiCol.HeaderActive, 88, 91, 112);
        SetColor(ImGuiCol.Separator, 49, 50, 68);
        SetColor(ImGuiCol.SeparatorHovered, 137, 180, 250);
        SetColor(ImGuiCol.SeparatorActive, 180, 190, 254);
        SetColor(ImGuiCol.ResizeGrip, 49, 50, 68, 64);
        SetColor(ImGuiCol.ResizeGripHovered, 137, 180, 250);
        SetColor(ImGuiCol.ResizeGripActive, 180, 190, 254);
        SetColor(ImGuiCol.Tab, 24, 24, 37);
        SetColor(ImGuiCol.TabHovered, 69, 71, 90);
        SetColor(ImGuiCol.TabSelected, 49, 50, 68);
        SetColor(ImGuiCol.TabDimmed, 24, 24, 37);
        SetColor(ImGuiCol.TabDimmedSelected, 30, 30, 46);
        SetColor(ImGuiCol.DockingPreview, 137, 180, 250, 178);
        SetColor(ImGuiCol.DockingEmptyBg, 17, 17, 27);
        SetColor(ImGuiCol.PlotLines, 148, 226, 213);
        SetColor(ImGuiCol.PlotLinesHovered, 243, 139, 168);
        SetColor(ImGuiCol.PlotHistogram, 249, 226, 175);
        SetColor(ImGuiCol.PlotHistogramHovered, 250, 179, 135);
        SetColor(ImGuiCol.TableHeaderBg, 24, 24, 37);
        SetColor(ImGuiCol.TableBorderStrong, 49, 50, 68);
        SetColor(ImGuiCol.TableBorderLight, 30, 30, 46);
        SetColor(ImGuiCol.TableRowBg, 0, 0, 0, 0);
        SetColor(ImGuiCol.TableRowBgAlt, 255, 255, 255, 10);
        SetColor(ImGuiCol.TextSelectedBg, 137, 180, 250, 90);
        SetColor(ImGuiCol.DragDropTarget, 180, 190, 254);
        SetColor(ImGuiCol.NavCursor, 137, 180, 250);
        SetColor(ImGuiCol.NavWindowingHighlight, 205, 214, 244, 178);
        SetColor(ImGuiCol.NavWindowingDimBg, 200, 200, 200, 50);
        SetColor(ImGuiCol.ModalWindowDimBg, 0, 0, 0, 90);
    }

    public static string FormatThemeName(EditorThemeId themeId) => themeId switch
    {
        EditorThemeId.Dark => "Dark",
        EditorThemeId.Light => "Light",
        EditorThemeId.Nord => "Nord",
        EditorThemeId.SolarizedDark => "Solarized Dark",
        EditorThemeId.Catppuccin => "Catppuccin Mocha",
        _ => themeId.ToString()
    };
}
