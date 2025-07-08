using CkCommons.Gui;
using CkCommons.Helpers;
using CkCommons.Raii;
using CkCommons.RichText;
using Dalamud.Interface.Colors;
using Dalamud.Interface.Utility.Raii;
using FFXIVClientStructs.FFXIV.Client.UI;
using ImGuiNET;
using OtterGui.Text.Widget.Editors;
using OtterGui.Text;
using System.Drawing;
using System.Globalization;

namespace CkCommons.Chat;
public abstract class CkChatlog<T> where T : CkChatMessage
{
    protected readonly int ID;
    protected readonly string Label;
    protected CkChatlogBuffer<T> Messages;
    protected Dictionary<string, Vector4> UserColors = new();
    protected T? LastInteractedMsg = null;
    protected List<string> SilenceList = new();
    
    protected int unreadSinceScroll = 0;
    protected string previewMessage = string.Empty;
    protected bool shouldFocusChatInput = false;

    public CkChatlog(int chatlogId, string label, int capacity)
    {
        ID = chatlogId;
        Label = label;
        Messages = new CkChatlogBuffer<T>(capacity);
        DoAutoScroll = true;
        ShouldScrollToBottom = true;
    }

    public static DateTime TimeCreated { get; private set; } = DateTime.Now;
    public bool DoAutoScroll { get; protected set; } = true;
    public bool ShouldScrollToBottom { get; protected set; } = false;

    public void AddMessages(IEnumerable<T> messages)
    {
        foreach (var message in messages)
            AddMessage(message);
    }

    protected virtual void AddMessage(T message)
    {
        // Assign the sender color
        var col = AssignSenderColor(message);
        // prepend the special payload strings based on context.
        Messages.PushBack(message);
        unreadSinceScroll++;
    }

    protected virtual Vector4 AssignSenderColor(T message)
    {
        if (UserColors.TryGetValue(message.UID, out var col))
            return col;

        Vector4 color;
        float brightness;
        do
        {
            var r = (float)new Random().NextDouble();
            var g = (float)new Random().NextDouble();
            var b = (float)new Random().NextDouble();
            // Calculate brightness as the average of RGB values
            brightness = (r + g + b) / 3.0f;
            color = new Vector4(r, g, b, 1.0f);

        } while (brightness < 0.55f || UserColors.ContainsValue(color)); // Adjust threshold as needed (e.g., 0.7 for lighter colors)
        UserColors[message.UID] = color;
        return color;
    }

    public void ClearMessages() => Messages.Clear();

    protected virtual string ToTooltip(T message)
        => $"Sent @ {message.Timestamp.ToString("T", CultureInfo.CurrentCulture)}\n[Right-Click] View Interactions";
    protected abstract void OnMiddleClick(T message);
    protected abstract void OnSendMessage(string message);
    public void DrawChat(Vector2 region, ref bool displayPreview)
    {
        // Create a windows drawlist here so we have the outermost drawlist.
        var outerWdl = ImGui.GetWindowDrawList();
        ImDrawListPtr innerWdl; // capture the inner drawlist.
        using (var c = CkRaii.Child($"##GlobalChatLogFrame-{Label}", region))
        {
            // Draw the chat log history.
            innerWdl = ImGui.GetWindowDrawList();
            DrawChatLog(c.InnerRegion - new Vector2(0, ImGui.GetFrameHeightWithSpacing()));
            // draw the text preview if we should.
            DrawChatInputRow(ref displayPreview);

            // maybe fix this by putting a child in a child inside the chatlog and doing some layer voodoo there idk.
            if (displayPreview)
                DrawTextPreview(previewMessage, innerWdl);

            // Attempt to handle any popups we may have had (within the same context)
            ShowPopups();
        }
    }

    public void DrawChatLog(Vector2 region)
    {
        using var _ = ImRaii.Child($"##GlobalChatLogHistory-{Label}", region);

        var messages = Messages.Skip(Math.Max(0, Messages.Size - 250)).Take(250);
        var remainder = CkGuiClip.DynamicClippedDraw(messages, DrawChatMessage, region.X);

        HandleAutoScroll();
    }

    private void DrawChatMessage(T message, float width)
    {
        if (SilenceList.Contains(message.UID))
            return;

        // use CkRichText for enhanced display.
        CkRichText.Text(width, message.Message, ID);
        // Attach popup if clicked.
        if (ImGui.IsItemClicked(ImGuiMouseButton.Right))
        {
            LastInteractedMsg = message;
            ImGui.OpenPopup($"GlobalChatMessageActions_{message.UID}");
        }
        // Optional Middle click function.
        if (ImGui.IsItemClicked(ImGuiMouseButton.Middle))
            OnMiddleClick(message);
        CkGui.AttachToolTip(ToTooltip(message), color: CkColor.VibrantPink.Vec4());
    }

    public virtual void DrawChatInputRow(ref bool showPreview)
    {
        using var _ = ImRaii.Group();

        var width = ImGui.GetContentRegionAvail().X;

        // Set keyboard focus to the chat input box if needed
        if (shouldFocusChatInput)
        {
            // if we currently are focusing the window this is present on, set the keyboard focus.
            if (ImGui.IsWindowFocused())
            {
                ImGui.SetKeyboardFocusHere(0);
                shouldFocusChatInput = false;
            }
        }

        ImGui.SetNextItemWidth(width);
        ImGui.InputTextWithHint($"##ChatInput{Label}{ID}", $"message {Label}...", ref previewMessage, 400);

        // Process submission Prevent losing chat focus after pressing the Enter key.
        if (ImGui.IsItemFocused() && ImGui.IsKeyPressed(ImGuiKey.Enter))
        {
            shouldFocusChatInput = true;
            OnSendMessage(previewMessage);
        }

        // Update preview display based on input field activity
        showPreview = ImGui.IsItemActive();
    }

    private float prevValidHeight = 0f;
    private void DrawTextPreview(string message, ImDrawListPtr drawList)
    {
        // get the previous childs min and max positions.
        var inputRowMin = ImGui.GetItemRectMin();
        var inputRowSize = ImGui.GetItemRectSize();
        var padding = new Vector2(5, 5);
        var wrapWidth = inputRowSize.X - padding.X * 2;

        // Estimate text size with wrapping
        var calculatedHeight = CkRichText.GetRichTextLineHeight(message, ID);
        var finalHeight = calculatedHeight == 0 ? prevValidHeight : calculatedHeight;

        if (calculatedHeight != 0)
            prevValidHeight = finalHeight;

        var boxSize = new Vector2(inputRowSize.X, ImGui.GetTextLineHeightWithSpacing() * finalHeight + padding.Y * 2);
        var boxPos = new Vector2(inputRowMin.X, inputRowMin.Y - boxSize.Y);

        // Draw semi-transparent background
        drawList.AddRectFilled(boxPos, boxPos + boxSize, ImGui.GetColorU32(new Vector4(0.05f, 0.025f, 0.05f, .9f)), 5);

        var startPos = new Vector2(ImGui.GetCursorScreenPos().X + padding.X, inputRowMin.Y - boxSize.Y + padding.Y);
        ImGui.SetCursorScreenPos(startPos);
        CkRichText.Text(wrapWidth, message, ID);
    }

    private void HandleAutoScroll()
    {
        if (ShouldScrollToBottom || (DoAutoScroll && unreadSinceScroll > 0))
        {
            ShouldScrollToBottom = false;
            ImGui.SetScrollHereY(1.0f);
            unreadSinceScroll = 0;
        }
    }

    private void ShowPopups()
    {
        if (LastInteractedMsg is null)
            return;

        using var style = ImRaii.PushStyle(ImGuiStyleVar.WindowPadding, Vector2.One * 8f)
            .Push(ImGuiStyleVar.PopupRounding, 4f)
            .Push(ImGuiStyleVar.PopupBorderSize, 2f);
        using var col = ImRaii.PushColor(ImGuiCol.Border, ImGuiColors.ParsedPink);

        using (var popup = ImRaii.Popup($"GlobalChatMessageActions_{LastInteractedMsg.UID}"))
        {
            if (popup)
                DrawPopupInternal();
        }
    }

    protected abstract void DrawPopupInternal();
}
