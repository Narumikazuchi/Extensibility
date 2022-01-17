namespace Narumikazuchi.Extensibility;

/// <summary>
/// Represents the functionality of an <see cref="IAddInStore"/> to manage a list of user trusted AddIns.
/// </summary>
public interface IAddInTrustList
{
    /// <summary>
    /// Clears the list of user trusted AddIns.
    /// </summary>
    public void ClearUserTrustedList();

    /// <summary>
    /// Reads a list of user trusted addins from the specified file and adds them to the <see cref="IAddInTrustList"/>.
    /// </summary>
    /// <param name="file">The file to read.</param>
    /// <exception cref="ArgumentNullException"/>
    /// <exception cref="PathTooLongException"/>
    /// <exception cref="DirectoryNotFoundException"/>
    /// <exception cref="FileNotFoundException"/>
    /// <exception cref="IOException"/>
    /// <exception cref="System.Security.SecurityException"/>
    /// <exception cref="UnauthorizedAccessException"/>
    public void ReadUserTrustedListFrom([DisallowNull] FileInfo file);
    /// <summary>
    /// Reads a list of user trusted addins from the specified file and adds them to the <see cref="IAddInTrustList"/>.
    /// </summary>
    /// <param name="filePath">The path to the file to read.</param>
    /// <exception cref="ArgumentNullException"/>
    /// <exception cref="PathTooLongException"/>
    /// <exception cref="DirectoryNotFoundException"/>
    /// <exception cref="FileNotFoundException"/>
    /// <exception cref="IOException"/>
    /// <exception cref="System.Security.SecurityException"/>
    /// <exception cref="UnauthorizedAccessException"/>
    public void ReadUserTrustedListFrom([DisallowNull] String filePath);
    /// <summary>
    /// Reads a list of user trusted addins from the specified stream and adds them to the <see cref="IAddInTrustList"/>.
    /// </summary>
    /// <param name="stream">The stream to read from.</param>
    /// <exception cref="ArgumentNullException"/>
    /// <exception cref="PathTooLongException"/>
    /// <exception cref="DirectoryNotFoundException"/>
    /// <exception cref="FileNotFoundException"/>
    /// <exception cref="IOException"/>
    /// <exception cref="System.Security.SecurityException"/>
    /// <exception cref="UnauthorizedAccessException"/>
    public void ReadUserTrustedListFrom([DisallowNull] Stream stream);

    /// <summary>
    /// Removes the specified <see cref="Guid"/> from the list of user trusted AddIns.
    /// </summary>
    /// <param name="guid">The guid to remove.</param>
    public void RemoveFromUserTrustedList(in Guid guid);

    /// <summary>
    /// Writes the current list of user trusted addins to the specified file.
    /// </summary>
    /// <param name="file">The file to write to.</param>
    /// <exception cref="ArgumentNullException"/>
    /// <exception cref="PathTooLongException"/>
    /// <exception cref="DirectoryNotFoundException"/>
    /// <exception cref="FileNotFoundException"/>
    /// <exception cref="IOException"/>
    /// <exception cref="System.Security.SecurityException"/>
    /// <exception cref="UnauthorizedAccessException"/>
    public void WriteUserTrustedListTo([DisallowNull] FileInfo file);
    /// <summary>
    /// Writes the current list of user trusted addins to the specified file.
    /// </summary>
    /// <param name="filePath">The path to the file to write to.</param>
    /// <exception cref="ArgumentNullException"/>
    /// <exception cref="PathTooLongException"/>
    /// <exception cref="DirectoryNotFoundException"/>
    /// <exception cref="FileNotFoundException"/>
    /// <exception cref="IOException"/>
    /// <exception cref="System.Security.SecurityException"/>
    /// <exception cref="UnauthorizedAccessException"/>
    public void WriteUserTrustedListTo([DisallowNull] String filePath);
    /// <summary>
    /// Writes the current list of user trusted addins to the specified stream.
    /// </summary>
    /// <param name="stream">The stream to write to.</param>
    /// <exception cref="ArgumentNullException"/>
    /// <exception cref="PathTooLongException"/>
    /// <exception cref="DirectoryNotFoundException"/>
    /// <exception cref="FileNotFoundException"/>
    /// <exception cref="IOException"/>
    /// <exception cref="System.Security.SecurityException"/>
    /// <exception cref="UnauthorizedAccessException"/>
    public void WriteUserTrustedListTo([DisallowNull] Stream stream);
}