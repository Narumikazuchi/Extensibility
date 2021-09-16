namespace Narumikazuchi.Extensibility
{
    /// <summary>
    /// The security level in which to handle AddIns.
    /// </summary>
    public enum AddInSecurity
    {
        /// <summary>
        /// The AddIn alongside it's assembly will be loaded into a seperate process and run isolated from the current <see cref="System.AppDomain"/>.
        /// </summary>
        Isolated = 0,
        /// <summary>
        /// The assembly of the AddIn will dynamically loaded into the running application.
        /// </summary>
        Trusted = 1
    }
}
