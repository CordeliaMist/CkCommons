using CkCommons.Classes;
using CkCommons.Gui;
using CkCommons.Raii;
using CkCommons.RichText;
using Dalamud.Interface.Colors;
using Dalamud.Interface.Utility.Raii;
using ImGuiNET;
using System.Globalization;

namespace CkCommons.Chat;
public abstract class CkChatlog<T> where T : CkChatMessage
{
    protected readonly int ID;
    protected readonly string Label;
    protected CircularBuffer<T> Messages;
    protected ConcurrentDictionary<string, Vector4> UserColors = new();
    protected T? LastInteractedMsg = null;
    protected List<string> SilenceList = new();
    
    protected static int unreadSinceScroll = 0;
    protected string previewMessage = string.Empty;
    protected bool shouldFocusChatInput = false;
    protected float prevValidHeight = 0f;

    public CkChatlog(int chatlogId, string label, int capacity)
    {
        ID = chatlogId;
        Label = label;
        Messages = new CircularBuffer<T>(capacity);
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

        } while (brightness < 0.55f || UserColors.Values.Any(c => c == color)); // Adjust threshold as needed (e.g., 0.7 for lighter colors)
        UserColors[message.UID] = color;
        return color;
    }

    public void ClearMessages() => Messages.Clear();

    protected virtual string ToTooltip(T message)
        => $"Sent @ {message.Timestamp.ToString("T", CultureInfo.CurrentCulture)}\n[Right-Click] View Interactions";
    protected abstract void OnMiddleClick(T message);
    protected abstract void OnSendMessage(string message);
    public void DrawChat(Vector2 region)
    {
        // Create a windows drawlist here so we have the outermost drawlist.
        Vector2 inputMin;

        using (var c = CkRaii.Child($"##GlobalChatLogFrame-{Label}", region))
        {   
            var chatlogSize = c.InnerRegion - new Vector2(0, ImGui.GetFrameHeightWithSpacing());
            // temporarily cleave the pushcliprect so that the chatlog confines to it.
            DrawChatLog(chatlogSize);

            DrawChatInputRow();
            inputMin = ImGui.GetItemRectMin();
        }

        // Handle post chatlog drawing addons.
        DrawPostChatLog(inputMin);
    }

    public void DrawChatLog(Vector2 region, WFlags flags = WFlags.NoScrollbar)
    {
        using var _ = CkRaii.Child($"##GlobalChatLog-{Label}", region, flags);
        var messages = Messages.Skip(Math.Max(0, Messages.Size - 250)).Take(250);
        var remainder = CkGuiClip.DynamicClippedDraw(messages, DrawChatMessage, region.X);

        HandleAutoScroll();
        // Attempt to handle any popups we may have had (within the same context)
        ShowPopups();
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
            ImGui.OpenPopup($"CkChatMessageActions_{message.UID}");
        }
        // Optional Middle click function.
        if (ImGui.IsItemClicked(ImGuiMouseButton.Middle))
            OnMiddleClick(message);
        CkGui.AttachToolTip(ToTooltip(message), color: CkColor.VibrantPink.Vec4());
    }

    public virtual void DrawChatInputRow()
    {
        using var _ = ImRaii.Group();

        var width = ImGui.GetContentRegionAvail().X;

        if (shouldFocusChatInput && ImGui.IsWindowFocused())
        {
            ImGui.SetKeyboardFocusHere(0);
            shouldFocusChatInput = false;
        }

        ImGui.SetNextItemWidth(width);
        ImGui.InputTextWithHint($"##ChatInput{Label}{ID}", $"message {Label}...", ref previewMessage, 400);

        // Process submission Prevent losing chat focus after pressing the Enter key.
        if (ImGui.IsItemFocused() && ImGui.IsKeyPressed(ImGuiKey.Enter))
        {
            shouldFocusChatInput = true;
            OnSendMessage(previewMessage);
        }
    }

    protected virtual void DrawPostChatLog(Vector2 inputPosMin)
    {
        // Preview Text padding area
        using var style = ImRaii.PushStyle(ImGuiStyleVar.WindowPadding, new Vector2(5));
        // if we should show the preview, do so.
        if (!string.IsNullOrWhiteSpace(previewMessage))
            DrawTextPreview(previewMessage, inputPosMin);
    }

    protected void DrawTextPreview(string message, Vector2 textInputMinPos)
    {
        // we need to firstly get the calculated height of the CkRichText message.
        var fetchedHeight = CkRichText.GetRichTextLineHeight(message, ID);

        // if it is between frames calculating, for 1 drawframe the value can be 0.
        // This occurs because when we type a new character for our input string, we
        // technically have a new message, so it has to be regenerated and re-cached.
        // to account for this, a backup value is used.
        var finalHeight = fetchedHeight == 0 ? prevValidHeight : fetchedHeight;

        // update the cached height if non-zero.
        if (fetchedHeight != 0)
            prevValidHeight = finalHeight;

        // set the next position of the window to be the 

        var winHeight = (ImGui.GetTextLineHeightWithSpacing() * finalHeight) - ImGui.GetStyle().ItemSpacing.Y;
        var winPos = textInputMinPos - new Vector2(0, winHeight.AddWinPadY());

        ImGui.SetNextWindowPos(winPos);
        using (var c = CkRaii.ChildPaddedW("##InputPreview", ImGui.GetContentRegionAvail().X, winHeight))
        {
            // This inside window drawlist layer is the same Z-Depth as the chatlog,
            // and drawn after, so it will be rendered above. Giving the child a bg
            // color will prevent it from being layered correctly, and must be drawn
            // inside of the child for full effect.
            var wdl = ImGui.GetWindowDrawList();
            wdl.PushClipRect(winPos, winPos + c.InnerRegion.WithWinPadding(), false);
            wdl.AddRectFilled(winPos, winPos + c.InnerRegion.WithWinPadding(), 0xCC000000, 5, DFlags.RoundCornersAll);
            wdl.AddRect(winPos, winPos + c.InnerRegion.WithWinPadding(), ImGuiColors.ParsedGold.ToUint(), 5, DFlags.RoundCornersAll);
            wdl.PopClipRect();
            CkRichText.Text(message, ID);
        }

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

        using (var popup = ImRaii.Popup($"CkChatMessageActions_{LastInteractedMsg.UID}"))
        {
            if (popup)
                DrawPopupInternal();
        }
    }

    protected abstract void DrawPopupInternal();
}
