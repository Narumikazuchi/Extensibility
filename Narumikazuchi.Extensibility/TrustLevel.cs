namespace Narumikazuchi.Extensibility;

public enum TrustLevel
{
    Nothing = 0x0,
    OnlySystem = 0x1,
    OnlyUserConfirmed = 0x2,
    SystemAndUserConfirmed = OnlySystem | OnlyUserConfirmed,
    OnlyNotTrusted = 0x4,
    All = SystemAndUserConfirmed | OnlyNotTrusted
}