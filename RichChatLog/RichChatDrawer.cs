using CkCommons.Gui;
using CkCommons.Raii;
using CkCommons.RichText;
using Dalamud.Bindings.ImGui;
using Dalamud.Interface.Colors;
using Dalamud.Interface.Utility.Raii;

namespace CkCommons.RichChat;

/// <summary>
///   Base class for a CkChatlogDrawer. <br/>
///   The Chatlog is nullable and safely checked for validity before drawing.
/// </summary>
public class RichChatDrawer<T> where T : IChatMessage
{
    protected RichChatLog<T>? ChatLog;

    // Phase out if possible.
    protected bool didInitialScroll = false;

    // CkRichText Preview TextHeight Cache.
    protected float prevValidHeight = 0f;

    // Input Text Assistance.
    protected int setChatCursorPos = -1;
    protected int lastChatCursorPos = 0;
    protected bool shouldFocusInput = false;

    // Sent Message History
    protected int historyIdx = 0;
    // Caches input prior to swapping between messages.
    protected string lastInput = string.Empty;
    protected string previewMessage = string.Empty;
    protected List<string> sentHistory = [];

    protected T? lastHovered;
    protected T? lastSelected;
    protected T? inPopup;

    protected int MaxMessagesDrawn = 250;

    public RichChatDrawer()
    { }

    public RichChatDrawer(RichChatLog<T> chatLog)
    {
        ChatLog = chatLog;
    }

    protected virtual float GetInputHeight()
        => ImGui.GetFrameHeightWithSpacing();

    protected virtual void FlushLocalData()
    {
        prevValidHeight = 0f;
        setChatCursorPos = -1;
        lastChatCursorPos = 0;
        shouldFocusInput = false;
        historyIdx = 0;
        previewMessage = string.Empty;
        sentHistory = [];

        lastHovered = default;
        lastSelected = default;
        inPopup = default;
    }

    protected virtual void PreDraw()
    { }

    protected virtual void PostDraw(Vector2 inputMin)
    {
        // Preview Text padding area
        using var style = ImRaii.PushStyle(ImGuiStyleVar.WindowPadding, new Vector2(5));
        // if we should show the preview, do so.
        if (!string.IsNullOrWhiteSpace(previewMessage))
            DrawTextPreview(previewMessage, inputMin);
    }

    public void Draw(WFlags flags = WFlags.None)
        => Draw(ImGui.GetContentRegionAvail(), flags);

    protected virtual void DrawInvalidPlaceholder(Vector2 region)
    {
        using var _ = CkRaii.Child("invalid-chat", region);
        CkGui.ColorTextCentered("No chat selected", ImGuiColors.DPSRed);
    }

    public void Draw(Vector2 region, WFlags flags = WFlags.None)
    {
        if (ChatLog is null)
        {
            DrawInvalidPlaceholder(region);
            return;
        }

        PreDraw();
        Vector2 inputMin;

        using (var frame = CkRaii.Child($"chat-frame-{ChatLog.ID}", region))
        {
            var historySize = frame.InnerRegion - new Vector2(0, GetInputHeight());
            DrawChatHistory(historySize, flags);
            DrawChatInputRow();
            inputMin = ImGui.GetItemRectMin();
        }
        // Handle post chatlog drawing addons.
        PostDraw(inputMin);
    }

    public virtual void DrawChatHistory(Vector2 region, WFlags flags = WFlags.None)
    {
        using var _ = ImRaii.Child($"history-{ChatLog!.ID}", region, false, flags);
        // Inner child that respects the scrollbar offset, if scrollbar was enabled. (helpful safeguard)
        var messages = ChatLog.Messages.Skip(Math.Max(0, ChatLog.Messages.Count - MaxMessagesDrawn)).Take(MaxMessagesDrawn);
        var drawWidth = ImGui.GetContentRegionAvail().X - ImGui.GetStyle().ScrollbarSize;
        DrawHistoryInternal(messages, drawWidth);

        HandleAutoScroll(true);
        HandleContentMenu();
    }

    protected virtual void DrawHistoryInternal(IEnumerable<T> messages, float width)
    {
        foreach (var msg in messages)
            DrawChatMessage(msg, width);
    }

    protected virtual void DrawChatMessage(T message, float width)
    {
        // use CkRichText for enhanced display.
        NewRichText.TextFlowWrappedOrDummy(message.Message, width, ChatLog!.ID + message.MsgId);
        ImGui.GetWindowDrawList().AddRect(ImGui.GetItemRectMin() - new Vector2(2), ImGui.GetItemRectMax() + new Vector2(2), ImGuiColors.ParsedGold.ToUint(), 4f);
        HandleDetections(message);
    }

    protected void DrawChatEndDummy(IEnumerable<T> data, float width)
    {
        var remaining = data.Count();
        if (remaining is 0)
            return;

        var spacing = ImGui.GetStyle().ItemSpacing.Y;
        var dummyH = 0f;
        foreach (var msg in data)
            dummyH += NewRichText.GetTextSize(msg.Message, ChatLog!.ID + msg.MsgId).Y + spacing;
        ImGui.Dummy(new Vector2(width, dummyH - spacing));
    }

    protected void HandleAutoScroll(bool clearMentions = false)
    {
        if (!didInitialScroll || (ChatLog!.AutoScroll && ChatLog.UnreadMessages > 0))
        {
            ImGui.SetScrollHereY(1.0f);
            ChatLog!.MarkAsRead(clearMentions);
            didInitialScroll = true;
        }
    }

    public virtual void DrawChatInputRow()
    {
        using var _ = ImRaii.Group();
        var scrollIcon = ChatLog!.AutoScroll ? FAI.ArrowDownUpLock : FAI.ArrowDownUpAcrossLine;
        var width = ImGui.GetContentRegionAvail().X;

        if (shouldFocusInput)
        {
            ImGui.SetWindowFocus();
            ImGui.SetKeyboardFocusHere(0);
            shouldFocusInput = false;
        }

        ImGui.SetNextItemWidth(width - CkGui.IconButtonSize(scrollIcon).X - ImGui.GetStyle().ItemInnerSpacing.X);
        ImGui.InputTextWithHint($"##chat-input-{ChatLog!.ID}", $"Message {ChatLog.ID}...", ref previewMessage, 400, ImGuiInputTextFlags.CallbackHistory, OnChatInputCallback);
        // Process submission Prevent losing chat focus after pressing the Enter key.
        if (ImGui.IsItemFocused() && (ImGui.IsKeyPressed(ImGuiKey.Enter) || ImGui.IsKeyPressed(ImGuiKey.KeypadEnter)))
        {
            shouldFocusInput = true;
            SendMessage(previewMessage);
        }

        CkGui.SameLineInner();
        if (CkGui.IconButton(scrollIcon))
            ChatLog!.AutoScroll = !ChatLog.AutoScroll;
        CkGui.AttachTooltip($"Toggles AutoScroll (Current: {(ChatLog!.AutoScroll ? "Enabled" : "Disabled")})");
    }

    /// <summary>
    ///   Virtual method for input text callback. Without overrides this only handles sent message history navigation.
    /// </summary>
    protected virtual unsafe int OnChatInputCallback(ref ImGuiInputTextCallbackData dataPtr)
    {
        fixed (ImGuiInputTextCallbackData* data = &dataPtr)
        {
            // Handle message history cycling up and down between messages.
            if (data->EventFlag is ImGuiInputTextFlags.CallbackHistory)
            {
                // This will go from most recent to oldest sent messages
                if (data->EventKey is ImGuiKey.UpArrow)
                {
                    // If at the start, there is nothing to store.
                    if (historyIdx is 0)
                        lastInput = previewMessage;
                    // Otherwise, we should swap out the data.
                    if (historyIdx < sentHistory.Count)
                    {
                        historyIdx++;
                        data->DeleteChars(0, data->BufTextLen);
                        data->InsertChars(0, sentHistory[^historyIdx]);
                    }
                }
                // This moves back towards our most message.
                else if (data->EventKey is ImGuiKey.DownArrow)
                {
                    if (historyIdx > 0)
                    {
                        historyIdx--;
                        var text = historyIdx == 0 ? lastInput : sentHistory[^historyIdx];
                        data->DeleteChars(0, data->BufTextLen);
                        data->InsertChars(0, text);
                    }
                }
            }
        }
        return 0;
    }

    protected virtual void SendMessage(string message)
    {
        if (string.IsNullOrWhiteSpace(message))
            return;
        // Standardize the history push here so derived classes don't have to rewrite it
        sentHistory.Add(message);
        historyIdx = 0;
        lastInput = string.Empty;
        previewMessage = string.Empty;
    }

    protected virtual void HandleDetections(T message)
    {
        if (ImGui.IsItemHovered())
            lastHovered = message;

        // Handle Context Menus. (Maybe make a flag later. Would save on some drawtime.)
        if (ImGui.IsItemClicked(ImGuiMouseButton.Right))
        {
            inPopup = message;
            ImGui.OpenPopup($"ckchatlog-{ChatLog!.ID}-msg-actions");
        }
    }

    protected void HandleContentMenu()
    {
        // If the popup closed, clear inPopup
        if (!ImGui.IsPopupOpen($"ckchatlog-{ChatLog!.ID}-msg-actions"))
        {
            inPopup = default;
            return;
        }

        // If it has no value, also return.
        if (inPopup is null)
            return;

        using var style = ImRaii.PushStyle(ImGuiStyleVar.WindowPadding, Vector2.One * 4f)
            .Push(ImGuiStyleVar.PopupRounding, 5f)
            .Push(ImGuiStyleVar.PopupBorderSize, 1f);
        // For now, change later when we find a way to pull from common color themes, or defined ones rather.
        // That, or we would have modified this chatlog to be an instanced class with theme support.
        using var col = ImRaii.PushColor(ImGuiCol.Border, ImGuiColors.ParsedGold);

        using var popup = ImRaii.Popup($"ckchatlog-{ChatLog!.ID}-msg-actions");
        if (!popup) return;
        DrawContentMenu(inPopup);
    }

    protected virtual void DrawContentMenu(T message)
    { }


    protected virtual void DrawIgnoredMessageRow(T message, float width)
    {
        var txtWidth = ImGui.CalcTextSize("Ignored Message");
        var lineW = (width - CkGui.ItemInnerSpacing.X * 2 - txtWidth.X) / 2;
        var min = ImGui.GetCursorScreenPos();
        var lineY = min.Y + (CkGui.TextHeight / 2);

        ImGui.GetWindowDrawList().AddLine(new Vector2(min.X, lineY), new Vector2(min.X + lineW, lineY), ImGuiColors.ParsedGrey.ToUint(), 2f);
        CkGui.ColorTextCentered("Ignored Message", ImGuiColors.ParsedGrey);
        ImGui.GetWindowDrawList().AddLine(new Vector2(min.X + width - lineW, lineY), new Vector2(min.X + width, lineY), ImGuiColors.ParsedGrey.ToUint(), 2f);
    }

    protected void DrawTextPreview(string message, Vector2 textInputMinPos, string richTextId = "Preview-Input")
    {
        // we need to firstly get the calculated height of the CkRichText message.
        var fetchedSize = NewRichText.GetTextSize(message, ChatLog!.ID + richTextId);
        // if it is between frames calculating, for 1 draw frame the value can be 0.
        // This occurs because when we type a new character for our input string, we
        // technically have a new message, so it has to be regenerated and re-cached.
        // to account for this, a backup value is used.
        var finalHeight = fetchedSize.Y == 0 ? prevValidHeight : fetchedSize.Y;
        // update the cached height if non-zero.
        if (fetchedSize.Y != 0)
            prevValidHeight = fetchedSize.Y;

        // set the next position of the window to be the 
        var winPos = textInputMinPos - new Vector2(0, finalHeight.AddWinPadY());

        ImGui.SetNextWindowPos(winPos);
        using var c = CkRaii.ChildPaddedW("##InputPreview", ImGui.GetContentRegionAvail().X, finalHeight);
        // This inside window drawlist layer is the same Z-Depth as the chatlog,
        // and drawn after, so it will be rendered above. Giving the child a bg
        // color will prevent it from being layered correctly, and must be drawn
        // inside of the child for full effect.
        var wdl = ImGui.GetWindowDrawList();
        wdl.PushClipRect(winPos, winPos + c.InnerRegion.WithWinPadding(), false);
        wdl.AddRectFilled(winPos, winPos + c.InnerRegion.WithWinPadding(), 0xCC000000, 5, ImDrawFlags.RoundCornersAll);
        wdl.AddRect(winPos, winPos + c.InnerRegion.WithWinPadding(), ImGuiColors.ParsedGold.ToUint(), 5, ImDrawFlags.RoundCornersAll);
        wdl.PopClipRect();
        NewRichText.TextWrapped(message, ChatLog!.ID + richTextId);
    }
}
