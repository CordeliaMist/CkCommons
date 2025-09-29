using FFXIVClientStructs.FFXIV.Client.Game.Character;
using FFXIVClientStructs.FFXIV.Client.Game.Object;
#nullable disable
namespace CkCommons;

/// <summary> 
///     Static accessors for FFXIVClientStructs in regards to owned client objects.
/// </summary>
public static unsafe class OwnedObjects
{
    public static BattleChara* PlayerChara => CharacterManager.Instance()->BattleCharas[0].Value;
    public static GameObject* PlayerObject => GameObjectManager.Instance()->Objects.IndexSorted[0].Value;
    public static BattleChara* MinionOrMountChara => CharacterManager.Instance()->BattleCharas[1].Value;
    public static GameObject* MinionOrMountObject => GameObjectManager.Instance()->Objects.IndexSorted[1].Value;
    public static IntPtr PlayerAddress => (IntPtr)PlayerObject;
    public static IntPtr MinionOrMountAddress => (IntPtr)MinionOrMountObject;
    public static IntPtr PetAddress => (nint)CharacterManager.Instance()->LookupPetByOwnerObject(PlayerChara);
    public static IntPtr CompanionAddress => (nint)CharacterManager.Instance()->LookupBuddyByOwnerObject(PlayerChara);
}
