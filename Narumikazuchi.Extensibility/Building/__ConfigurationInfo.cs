namespace Narumikazuchi.Extensibility;

internal sealed partial class __ConfigurationInfo
{
    public __ConfigurationInfo(in TrustLevel trustLevel)
    {
        this._trustLevel = trustLevel;
    }

    public TrustLevel TrustLevel => 
        this._trustLevel;
    public IEnumerable<Guid> TrustedAddIns => 
        this._trustedAddIns;
    public IEnumerable<Guid> UserTrustedAddIns => 
        this._userTrustedAddIns;
    public Boolean ShouldFailWhenNotSystemTrusted => 
        this._shouldFailWhenNotSystemTrusted;
    public Boolean ShouldFailWhenNotUserTrusted =>
        this._shouldFailWhenNotUserTrusted;
    public Action<IAddInDefinition>? UserNotificationDelegate =>
        this._userNotificationDelegate;
    public Func<IAddInDefinition, Boolean>? UserPromptDelegate =>
        this._userPromptDelegate;
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
    private Boolean _shouldFailWhenNotSystemTrusted;
    private Boolean _shouldFailWhenNotUserTrusted;
    private Action<IAddInDefinition>? _userNotificationDelegate;
    private Func<IAddInDefinition, Boolean>? _userPromptDelegate;
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

    IAddInSystemConfiguredNotUserTrustedConfigurator IAddInNotBothTrustedConfigurator.NotifyUserWhenNotSystemTrusted(Action<IAddInDefinition> notification)
    {
        ExceptionHelpers.ThrowIfArgumentNull(notification);

        this._shouldFailWhenNotSystemTrusted = false;
        this._userNotificationDelegate = notification;
        return this;
    }

    IAddInUserConfiguredNotSystemTrustedConfigurator IAddInNotBothTrustedConfigurator.PromptUserWhenNotUserTrusted(Func<IAddInDefinition, Boolean> userPrompt)
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

    IAddInSystemTrustOnlyListConfigurator IAddInNotSystemTrustedConfigurator.NotifyUserWhenNotSystemTrusted(Action<IAddInDefinition> notification)
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

    IAddInUserTrustOnlyListConfigurator IAddInNotUserTrustedConfigurator.PromptUserWhenNotUserTrusted(Func<IAddInDefinition, Boolean> userPrompt)
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

    IAddInTrustBothListConfigurator IAddInSystemConfiguredNotUserTrustedConfigurator.PromptUserWhenNotUserTrusted(Func<IAddInDefinition, Boolean> userPrompt)
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

    IAddInTrustBothListConfigurator IAddInUserConfiguredNotSystemTrustedConfigurator.NotifyUserWhenNotSystemTrusted(Action<IAddInDefinition> notification)
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