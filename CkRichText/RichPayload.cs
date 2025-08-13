using Dalamud.Bindings.ImGui;

namespace CkCommons.RichText;

/// <summary>
///     All payloads have various attributes, but the one thing they must all share is a way to update their cache.
/// </summary>
/// <remarks>
///     It is intentional that these do not have a virtual draw method.
///     using a virtual draw method will increase drawtime by upwards of 200%-300%. <para/>
///     keep whatever is being called for drawframes NON-VIRTUAL.
/// </remarks>
public abstract class RichPayload
{
    /// <summary>
    ///     Updates the _splitCache with the given <see cref="ImFontPtr"/> and <paramref name="wrapWidth"/>.
    ///     Calculation begins on the line's start width of <paramref name="startWidth"/>.
    /// </summary>
    /// <param name="font"> The font used to draw this out, allowing for it to work with any font or any size. </param>
    /// <param name="wrapWidth"> the wrap width that the cached item should abide by. </param>
    /// <param name="curLineWidth"> the current width of the line the item is being drawn on. Less than full wrap width. </param>
    /// <returns> the number of lines to be added </returns>
    public abstract int UpdateCache(ImFontPtr font, float wrapWidth, ref float curLineWidth);
}
