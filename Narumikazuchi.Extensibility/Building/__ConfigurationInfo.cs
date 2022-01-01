namespace Narumikazuchi.Extensibility;

internal sealed partial class __ConfigurationInfo
{
    public __ConfigurationInfo(String defaultCachePath)
    {
        this._defaultCachePath = defaultCachePath;
    }

    public String DefaultCachePath => this._defaultCachePath;
    public TrustLevel TrustLevel => this._trustLevel;
    public IEnumerable<Guid> TrustedAddIns => this._trustedAddIns;
    public IEnumerable<Guid> UserTrustedAddIns => this._userTrustedAddIns;
    public Boolean ShouldFailWhenNotSystemTrusted => _shouldFailWhenNotSystemTrusted;
    public Boolean ShouldFailWhenNotUserTrusted => _shouldFailWhenNotUserTrusted;
    public Action<AddInDefinition>? UserNotificationDelegate => _userNotificationDelegate;
    public Func<AddInDefinition, Boolean>? UserPromptDelegate => _userPromptDelegate;
}

// Non-Public
partial class __ConfigurationInfo :
    IAddInSystemTrustOnlyListConfiguratorOrFinalizer,
    IAddInTrustBothListConfiguratorOrFinalizer,
    IAddInUserTrustOnlyListConfiguratorOrFinalizer
{
    private readonly List<Guid> _trustedAddIns = new();
    private readonly List<Guid> _userTrustedAddIns = new();
    private TrustLevel _trustLevel = TrustLevel.NONE;
    private String _defaultCachePath;
    private Boolean _shouldFailWhenNotSystemTrusted;
    private Boolean _shouldFailWhenNotUserTrusted;
    private Action<AddInDefinition>? _userNotificationDelegate;
    private Func<AddInDefinition, Boolean>? _userPromptDelegate;
}

// IAddInNotBothTrustedConfigurator
partial class __ConfigurationInfo : IAddInNotBothTrustedConfigurator
{
    IAddInSystemConfiguredNotUserTrustedConfigurator IAddInNotBothTrustedConfigurator.FailWhenNotSystemTrusted()
    {
        this._shouldFailWhenNotSystemTrusted = true;
        this._userNotificationDelegate = null;
        return this;
    }

    IAddInUserConfiguredNotSystemTrustedConfigurator IAddInNotBothTrustedConfigurator.FailWhenNotUserTrusted()
    {
        this._shouldFailWhenNotUserTrusted = true;
        this._userPromptDelegate = null;
        return this;
    }

    IAddInSystemConfiguredNotUserTrustedConfigurator IAddInNotBothTrustedConfigurator.IgnoreWhenNotSystemTrusted()
    {
        this._shouldFailWhenNotSystemTrusted = false;
        this._userNotificationDelegate = null;
        return this;
    }

    IAddInUserConfiguredNotSystemTrustedConfigurator IAddInNotBothTrustedConfigurator.IgnoreWhenNotUserTrusted()
    {
        this._shouldFailWhenNotUserTrusted = false;
        this._userPromptDelegate = null;
        return this;
    }

    IAddInSystemConfiguredNotUserTrustedConfigurator IAddInNotBothTrustedConfigurator.NotifyUserWhenNotSystemTrusted(Action<AddInDefinition> notification)
    {
        ExceptionHelpers.ThrowIfArgumentNull(notification);

        this._shouldFailWhenNotSystemTrusted = false;
        this._userNotificationDelegate = notification;
        return this;
    }

    IAddInUserConfiguredNotSystemTrustedConfigurator IAddInNotBothTrustedConfigurator.PromptUserWhenNotUserTrusted(Func<AddInDefinition, Boolean> userPrompt)
    {
        ExceptionHelpers.ThrowIfArgumentNull(userPrompt);

        this._shouldFailWhenNotUserTrusted = false;
        this._userPromptDelegate = userPrompt;
        return this;
    }
}

// IAddInNotSystemTrustedConfigurator
partial class __ConfigurationInfo : IAddInNotSystemTrustedConfigurator
{
    IAddInSystemTrustOnlyListConfigurator IAddInNotSystemTrustedConfigurator.FailWhenNotSystemTrusted()
    {
        this._shouldFailWhenNotSystemTrusted = true;
        this._userNotificationDelegate = null;
        return this;
    }

    IAddInSystemTrustOnlyListConfigurator IAddInNotSystemTrustedConfigurator.IgnoreWhenNotSystemTrusted()
    {
        this._shouldFailWhenNotSystemTrusted = false;
        this._userNotificationDelegate = null;
        return this;
    }

    IAddInSystemTrustOnlyListConfigurator IAddInNotSystemTrustedConfigurator.NotifyUserWhenNotSystemTrusted(Action<AddInDefinition> notification)
    {
        ExceptionHelpers.ThrowIfArgumentNull(notification);

        this._shouldFailWhenNotSystemTrusted = false;
        this._userNotificationDelegate = notification;
        return this;
    }
}

// IAddInNotUserTrustedConfigurator
partial class __ConfigurationInfo : IAddInNotUserTrustedConfigurator
{
    IAddInUserTrustOnlyListConfigurator IAddInNotUserTrustedConfigurator.FailWhenNotUserTrusted()
    {
        this._shouldFailWhenNotUserTrusted = true;
        this._userPromptDelegate = null;
        return this;
    }

    IAddInUserTrustOnlyListConfigurator IAddInNotUserTrustedConfigurator.IgnoreWhenNotUserTrusted()
    {
        this._shouldFailWhenNotUserTrusted = false;
        this._userPromptDelegate = null;
        return this;
    }

    IAddInUserTrustOnlyListConfigurator IAddInNotUserTrustedConfigurator.PromptUserWhenNotUserTrusted(Func<AddInDefinition, Boolean> userPrompt)
    {
        ExceptionHelpers.ThrowIfArgumentNull(userPrompt);

        this._shouldFailWhenNotUserTrusted = false;
        this._userPromptDelegate = userPrompt;
        return this;
    }
}

// IAddInSystemConfiguredNotUserTrustedConfigurator
partial class __ConfigurationInfo : IAddInSystemConfiguredNotUserTrustedConfigurator
{
    IAddInTrustBothListConfigurator IAddInSystemConfiguredNotUserTrustedConfigurator.FailWhenNotUserTrusted()
    {
        this._shouldFailWhenNotUserTrusted = true;
        this._userPromptDelegate = null;
        return this;
    }

    IAddInTrustBothListConfigurator IAddInSystemConfiguredNotUserTrustedConfigurator.IgnoreWhenNotUserTrusted()
    {
        this._shouldFailWhenNotUserTrusted = false;
        this._userPromptDelegate = null;
        return this;
    }

    IAddInTrustBothListConfigurator IAddInSystemConfiguredNotUserTrustedConfigurator.PromptUserWhenNotUserTrusted(Func<AddInDefinition, Boolean> userPrompt)
    {
        ExceptionHelpers.ThrowIfArgumentNull(userPrompt);

        this._shouldFailWhenNotUserTrusted = false;
        this._userPromptDelegate = userPrompt;
        return this;
    }
}

// IAddInSystemTrustOnlyListConfigurator
partial class __ConfigurationInfo : IAddInSystemTrustOnlyListConfigurator
{
    IAddInSystemTrustOnlyListConfiguratorOrFinalizer IAddInSystemTrustOnlyListConfigurator.ProvidingSystemTrustedAddIns(IEnumerable<Guid> systemTrusted)
    {
        ExceptionHelpers.ThrowIfArgumentNull(systemTrusted);

        this._trustedAddIns
            .AddRange(systemTrusted);
        return this;
    }
}

// IAddInTrustBothListConfigurator
partial class __ConfigurationInfo : IAddInTrustBothListConfigurator
{
    IAddInTrustBothListConfiguratorOrFinalizer IAddInTrustBothListConfigurator.ProvidingSystemTrustedAddIns(IEnumerable<Guid> systemTrusted)
    {
        ExceptionHelpers.ThrowIfArgumentNull(systemTrusted);

        this._trustedAddIns
            .AddRange(systemTrusted);
        return this;
    }

    IAddInTrustBothListConfiguratorOrFinalizer IAddInTrustBothListConfigurator.ProvidingUserTrustedAddIns(IEnumerable<Guid> userTrusted)
    {
        ExceptionHelpers.ThrowIfArgumentNull(userTrusted);

        this._userTrustedAddIns
            .AddRange(userTrusted);
        return this;
    }
}

// IAddInTrustConfigurator
partial class __ConfigurationInfo : IAddInTrustConfigurator
{
    IAddInTrustFinalizer IAddInTrustConfigurator.TrustAllAddIns()
    {
        this._trustLevel = TrustLevel.ALL;
        return this;
    }

    IAddInNotBothTrustedConfigurator IAddInTrustConfigurator.TrustBothProvidedAndUserApprovedAddIns()
    {
        this._trustLevel = TrustLevel.TRUSTED_AND_USER_CONFIRMED;
        return this;
    }

    IAddInNotSystemTrustedConfigurator IAddInTrustConfigurator.TrustProvidedAddInsOnly()
    {
        this._trustLevel = TrustLevel.TRUSTED_ONLY;
        return this;
    }

    IAddInNotUserTrustedConfigurator IAddInTrustConfigurator.TrustUserApprovedAddInsOnly()
    {
        this._trustLevel = TrustLevel.USER_CONFIRMED_ONLY;
        return this;
    }
}

// IAddInTrustFinalizer
partial class __ConfigurationInfo : IAddInTrustFinalizer
{
    IAddInStore IAddInTrustFinalizer.Construct() =>
        new __AddInStore(this);
}

// IAddInUserConfiguredNotSystemTrustedConfigurator
partial class __ConfigurationInfo : IAddInUserConfiguredNotSystemTrustedConfigurator
{
    IAddInTrustBothListConfigurator IAddInUserConfiguredNotSystemTrustedConfigurator.FailWhenNotSystemTrusted()
    {
        this._shouldFailWhenNotSystemTrusted = true;
        this._userNotificationDelegate = null;
        return this;
    }

    IAddInTrustBothListConfigurator IAddInUserConfiguredNotSystemTrustedConfigurator.IgnoreWhenNotSystemTrusted()
    {
        this._shouldFailWhenNotSystemTrusted = false;
        this._userNotificationDelegate = null;
        return this;
    }

    IAddInTrustBothListConfigurator IAddInUserConfiguredNotSystemTrustedConfigurator.NotifyUserWhenNotSystemTrusted(Action<AddInDefinition> notification)
    {
        ExceptionHelpers.ThrowIfArgumentNull(notification);

        this._shouldFailWhenNotSystemTrusted = false;
        this._userNotificationDelegate = notification;
        return this;
    }
}

// IAddInUserTrustOnlyListConfigurator
partial class __ConfigurationInfo : IAddInUserTrustOnlyListConfigurator
{
    IAddInUserTrustOnlyListConfiguratorOrFinalizer IAddInUserTrustOnlyListConfigurator.ProvidingUserTrustedAddIns(IEnumerable<Guid> userTrusted)
    {
        ExceptionHelpers.ThrowIfArgumentNull(userTrusted);

        this._userTrustedAddIns
            .AddRange(userTrusted);
        return this;
    }
}