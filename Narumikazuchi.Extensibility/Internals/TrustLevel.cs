namespace Narumikazuchi.Extensibility;

internal enum TrustLevel
{
    NONE = 0x0,
    TRUSTED_ONLY = 0x1,
    USER_CONFIRMED_ONLY = 0x2,
    TRUSTED_AND_USER_CONFIRMED = TRUSTED_ONLY | USER_CONFIRMED_ONLY,
    NOT_TRUSTED = 0x4,
    ALL = TRUSTED_AND_USER_CONFIRMED | NOT_TRUSTED
}