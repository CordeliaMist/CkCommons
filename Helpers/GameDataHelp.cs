namespace CkCommons;
public static class GameDataHelp
{
    public static readonly Dictionary<ResidentialAetheryteKind, string> ResidentialNames = new()
    {
        [ResidentialAetheryteKind.Gridania] = "Lavender Beds",
        [ResidentialAetheryteKind.Limsa] = "Mist",
        [ResidentialAetheryteKind.Uldah] = "Goblet",
        [ResidentialAetheryteKind.Kugane] = "Shirogane",
        [ResidentialAetheryteKind.Foundation] = "Empyreum",
    };

    // Lifestream
    // https://github.com/NightmareXIV/Lifestream/blob/main/Lifestream/Data/AddressBookEntry.cs#L1
    public enum ResidentialAetheryteKind
    {
        Uldah = 9,
        Gridania = 2,
        Limsa = 8,
        Foundation = 70,
        Kugane = 111,
    }

    public enum PropertyType
    {
        House, Apartment
    }


}
