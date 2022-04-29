namespace Narumikazuchi.Extensibility;

internal sealed partial class __ConfigurationInfo
{
    public __ConfigurationInfo(in __TrustLevel trustLevel)
    {
        m_TrustLevel = trustLevel;
    }

    public __TrustLevel TrustLevel => 
        m_TrustLevel;
    public IEnumerable<Guid> TrustedAddIns => 
        m_TrustedAddIns;
    public IEnumerable<Guid> UserTrustedAddIns => 
        m_UserTrustedAddIns;
    public Boolean ShouldFailWhenNotSystemTrusted => 
        m_ShouldFailWhenNotSystemTrusted;
    public Boolean ShouldFailWhenNotUserTrusted =>
        m_ShouldFailWhenNotUserTrusted;
    public Action<IAddInDefinition>? UserNotificationDelegate =>
        m_UserNotificationDelegate;
    public Func<IAddInDefinition, Boolean>? UserPromptDelegate =>
        m_UserPromptDelegate;
}

// Non-Public
partial class __ConfigurationInfo :
    IAddInSystemTrustOnlyListConfiguratorOrFinalizer,
    IAddInTrustBothListConfiguratorOrFinalizer,
    IAddInUserTrustOnlyListConfiguratorOrFinalizer
{
    private readonly List<Guid> m_TrustedAddIns = new();
    private readonly List<Guid> m_UserTrustedAddIns = new();
    private readonly __TrustLevel m_TrustLevel = __TrustLevel.NONE;
    private Boolean m_ShouldFailWhenNotSystemTrusted;
    private Boolean m_ShouldFailWhenNotUserTrusted;
    private Action<IAddInDefinition>? m_UserNotificationDelegate;
    private Func<IAddInDefinition, Boolean>? m_UserPromptDelegate;
}

// IAddInNotBothTrustedConfigurator
partial class __ConfigurationInfo : IAddInNotBothTrustedConfigurator
{
    IAddInSystemConfiguredNotUserTrustedConfigurator IAddInNotBothTrustedConfigurator.FailWhenNotSystemTrusted()
    {
        m_ShouldFailWhenNotSystemTrusted = true;
        m_UserNotificationDelegate = null;
        return this;
    }

    IAddInUserConfiguredNotSystemTrustedConfigurator IAddInNotBothTrustedConfigurator.FailWhenNotUserTrusted()
    {
        m_ShouldFailWhenNotUserTrusted = true;
        m_UserPromptDelegate = null;
        return this;
    }

    IAddInSystemConfiguredNotUserTrustedConfigurator IAddInNotBothTrustedConfigurator.IgnoreWhenNotSystemTrusted()
    {
        m_ShouldFailWhenNotSystemTrusted = false;
        m_UserNotificationDelegate = null;
        return this;
    }

    IAddInUserConfiguredNotSystemTrustedConfigurator IAddInNotBothTrustedConfigurator.IgnoreWhenNotUserTrusted()
    {
        m_ShouldFailWhenNotUserTrusted = false;
        m_UserPromptDelegate = null;
        return this;
    }

    IAddInSystemConfiguredNotUserTrustedConfigurator IAddInNotBothTrustedConfigurator.NotifyUserWhenNotSystemTrusted(Action<IAddInDefinition> notification)
    {
        ArgumentNullException.ThrowIfNull(notification);

        m_ShouldFailWhenNotSystemTrusted = false;
        m_UserNotificationDelegate = notification;
        return this;
    }

    IAddInUserConfiguredNotSystemTrustedConfigurator IAddInNotBothTrustedConfigurator.PromptUserWhenNotUserTrusted(Func<IAddInDefinition, Boolean> userPrompt)
    {
        ArgumentNullException.ThrowIfNull(userPrompt);

        m_ShouldFailWhenNotUserTrusted = false;
        m_UserPromptDelegate = userPrompt;
        return this;
    }
}

// IAddInNotSystemTrustedConfigurator
partial class __ConfigurationInfo : IAddInNotSystemTrustedConfigurator
{
    IAddInSystemTrustOnlyListConfigurator IAddInNotSystemTrustedConfigurator.FailWhenNotSystemTrusted()
    {
        m_ShouldFailWhenNotSystemTrusted = true;
        m_UserNotificationDelegate = null;
        return this;
    }

    IAddInSystemTrustOnlyListConfigurator IAddInNotSystemTrustedConfigurator.IgnoreWhenNotSystemTrusted()
    {
        m_ShouldFailWhenNotSystemTrusted = false;
        m_UserNotificationDelegate = null;
        return this;
    }

    IAddInSystemTrustOnlyListConfigurator IAddInNotSystemTrustedConfigurator.NotifyUserWhenNotSystemTrusted(Action<IAddInDefinition> notification)
    {
        ArgumentNullException.ThrowIfNull(notification);

        m_ShouldFailWhenNotSystemTrusted = false;
        m_UserNotificationDelegate = notification;
        return this;
    }
}

// IAddInNotUserTrustedConfigurator
partial class __ConfigurationInfo : IAddInNotUserTrustedConfigurator
{
    IAddInUserTrustOnlyListConfigurator IAddInNotUserTrustedConfigurator.FailWhenNotUserTrusted()
    {
        m_ShouldFailWhenNotUserTrusted = true;
        m_UserPromptDelegate = null;
        return this;
    }

    IAddInUserTrustOnlyListConfigurator IAddInNotUserTrustedConfigurator.IgnoreWhenNotUserTrusted()
    {
        m_ShouldFailWhenNotUserTrusted = false;
        m_UserPromptDelegate = null;
        return this;
    }

    IAddInUserTrustOnlyListConfigurator IAddInNotUserTrustedConfigurator.PromptUserWhenNotUserTrusted(Func<IAddInDefinition, Boolean> userPrompt)
    {
        ArgumentNullException.ThrowIfNull(userPrompt);

        m_ShouldFailWhenNotUserTrusted = false;
        m_UserPromptDelegate = userPrompt;
        return this;
    }
}

// IAddInSystemConfiguredNotUserTrustedConfigurator
partial class __ConfigurationInfo : IAddInSystemConfiguredNotUserTrustedConfigurator
{
    IAddInTrustBothListConfigurator IAddInSystemConfiguredNotUserTrustedConfigurator.FailWhenNotUserTrusted()
    {
        m_ShouldFailWhenNotUserTrusted = true;
        m_UserPromptDelegate = null;
        return this;
    }

    IAddInTrustBothListConfigurator IAddInSystemConfiguredNotUserTrustedConfigurator.IgnoreWhenNotUserTrusted()
    {
        m_ShouldFailWhenNotUserTrusted = false;
        m_UserPromptDelegate = null;
        return this;
    }

    IAddInTrustBothListConfigurator IAddInSystemConfiguredNotUserTrustedConfigurator.PromptUserWhenNotUserTrusted(Func<IAddInDefinition, Boolean> userPrompt)
    {
        ArgumentNullException.ThrowIfNull(userPrompt);

        m_ShouldFailWhenNotUserTrusted = false;
        m_UserPromptDelegate = userPrompt;
        return this;
    }
}

// IAddInSystemTrustOnlyListConfigurator
partial class __ConfigurationInfo : IAddInSystemTrustOnlyListConfigurator
{
    IAddInSystemTrustOnlyListConfiguratorOrFinalizer IAddInSystemTrustOnlyListConfigurator.ProvidingSystemTrustedAddIns(IEnumerable<Guid> systemTrusted)
    {
        ArgumentNullException.ThrowIfNull(systemTrusted);

        m_TrustedAddIns.AddRange(systemTrusted);
        return this;
    }
}

// IAddInTrustBothListConfigurator
partial class __ConfigurationInfo : IAddInTrustBothListConfigurator
{
    IAddInTrustBothListConfiguratorOrFinalizer IAddInTrustBothListConfigurator.ProvidingSystemTrustedAddIns(IEnumerable<Guid> systemTrusted)
    {
        ArgumentNullException.ThrowIfNull(systemTrusted);

        m_TrustedAddIns.AddRange(systemTrusted);
        return this;
    }

    IAddInTrustBothListConfiguratorOrFinalizer IAddInTrustBothListConfigurator.ProvidingUserTrustedAddIns(IEnumerable<Guid> userTrusted)
    {
        ArgumentNullException.ThrowIfNull(userTrusted);

        m_UserTrustedAddIns.AddRange(userTrusted);
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
        m_ShouldFailWhenNotSystemTrusted = true;
        m_UserNotificationDelegate = null;
        return this;
    }

    IAddInTrustBothListConfigurator IAddInUserConfiguredNotSystemTrustedConfigurator.IgnoreWhenNotSystemTrusted()
    {
        m_ShouldFailWhenNotSystemTrusted = false;
        m_UserNotificationDelegate = null;
        return this;
    }

    IAddInTrustBothListConfigurator IAddInUserConfiguredNotSystemTrustedConfigurator.NotifyUserWhenNotSystemTrusted(Action<IAddInDefinition> notification)
    {
        ArgumentNullException.ThrowIfNull(notification);

        m_ShouldFailWhenNotSystemTrusted = false;
        m_UserNotificationDelegate = notification;
        return this;
    }
}

// IAddInUserTrustOnlyListConfigurator
partial class __ConfigurationInfo : IAddInUserTrustOnlyListConfigurator
{
    IAddInUserTrustOnlyListConfiguratorOrFinalizer IAddInUserTrustOnlyListConfigurator.ProvidingUserTrustedAddIns(IEnumerable<Guid> userTrusted)
    {
        ArgumentNullException.ThrowIfNull(userTrusted);

        m_UserTrustedAddIns.AddRange(userTrusted);
        return this;
    }
}