namespace Narumikazuchi.Extensibility
{
    /// <summary>
    /// Provides an interface to control AddIns. 
    /// </summary>
    public interface IAddInController : System.IDisposable
    {
        /// <summary>
        /// Initializes the corresponding AddIn and makes it's members available for invokation.
        /// </summary>
        /// <returns><see langword="true"/> if the activation succeeded; otherwise, <see langword="false"/></returns>
        /// <exception cref="System.ObjectDisposedException"/>
        public System.Threading.Tasks.Task<System.Boolean> ActivateAsync();
        /// <summary>
        /// Releases the corresponding AddIn and makes all of it's members unavailable.
        /// </summary>
        /// <exception cref="System.ObjectDisposedException"/>
        public void Shutdown();
        /// <summary>
        /// Invokes the specified <see cref="MethodSignature"/> on the corresponding AddIn with the specified parameters.
        /// </summary>
        /// <param name="method">The method to invoke.</param>
        /// <param name="parameters">The parameters passed to the method.</param>
        /// <exception cref="System.ArgumentNullException"/>
        /// <exception cref="System.ArgumentOutOfRangeException"/>
        /// <exception cref="System.InvalidCastException"/>
        /// <exception cref="System.InvalidOperationException"/>
        /// <exception cref="System.NotSupportedException"/>
        /// <exception cref="System.ObjectDisposedException"/>
        public System.Threading.Tasks.Task InvokeMethodAsync(MethodSignature method,
                                                             [System.Diagnostics.CodeAnalysis.DisallowNull] params System.Object[] parameters);
        /// <summary>
        /// Invokes the specified <see cref="MethodSignature"/> on the corresponding AddIn with the specified parameters and passes it's return value through.
        /// </summary>
        /// <param name="method">The method to invoke.</param>
        /// <param name="parameters">The parameters passed to the method.</param>
        /// <returns>The result of the invoked method</returns>
        /// <exception cref="System.ArgumentNullException"/>
        /// <exception cref="System.ArgumentOutOfRangeException"/>
        /// <exception cref="System.InvalidCastException"/>
        /// <exception cref="System.InvalidOperationException"/>
        /// <exception cref="System.NotSupportedException"/>
        /// <exception cref="System.ObjectDisposedException"/>
        /// <exception cref="System.TimeoutException"/>
        [return: System.Diagnostics.CodeAnalysis.MaybeNull]
        public System.Threading.Tasks.Task<TReturn> InvokeMethodAsync<TReturn>(MethodSignature method,
                                                                               [System.Diagnostics.CodeAnalysis.DisallowNull] params System.Object[] parameters);
        /// <summary>
        /// Invokes the getter of the specified <see cref="PropertySignature"/> and passes it's value through.
        /// </summary>
        /// <param name="property">The property to invoke the getter of.</param>
        /// <returns>The value of the property</returns>
        /// <exception cref="System.InvalidCastException"/>
        /// <exception cref="System.InvalidOperationException"/>
        /// <exception cref="System.NotSupportedException"/>
        /// <exception cref="System.ObjectDisposedException"/>
        /// <exception cref="System.TimeoutException"/>
        [System.Diagnostics.Contracts.Pure]
        [return: System.Diagnostics.CodeAnalysis.MaybeNull]
        public System.Threading.Tasks.Task<TProperty> InvokePropertyGetterAsync<TProperty>(PropertySignature property);
        /// <summary>
        /// Invokes the setter of the specified <see cref="PropertySignature"/> with the specified value.
        /// </summary>
        /// <param name="property">The property to invoke the setter of.</param>
        /// <param name="value">The new value of the property.</param>
        /// <exception cref="System.InvalidCastException"/>
        /// <exception cref="System.InvalidOperationException"/>
        /// <exception cref="System.NotSupportedException"/>
        /// <exception cref="System.ObjectDisposedException"/>
        /// <exception cref="System.TimeoutException"/>
        public System.Threading.Tasks.Task InvokePropertySetterAsync<TProperty>(PropertySignature property,
                                                                                [System.Diagnostics.CodeAnalysis.AllowNull] TProperty value);
        /// <summary>
        /// Invokes the getter of the specified <see cref="IndexerSignature"/> and passes it's value through.
        /// </summary>
        /// <param name="indexer">The indexer to invoke the getter of.</param>
        /// <param name="indecies">The index parameters passed to the indexer.</param>
        /// <returns>The value of the indexer at the specified indecies</returns>
        /// <exception cref="System.ArgumentNullException"/>
        /// <exception cref="System.ArgumentOutOfRangeException"/>
        /// <exception cref="System.InvalidCastException"/>
        /// <exception cref="System.InvalidOperationException"/>
        /// <exception cref="System.NotSupportedException"/>
        /// <exception cref="System.ObjectDisposedException"/>
        /// <exception cref="System.TimeoutException"/>
        [System.Diagnostics.Contracts.Pure]
        [return: System.Diagnostics.CodeAnalysis.MaybeNull]
        public System.Threading.Tasks.Task<TIndex> InvokeIndexerGetterAsync<TIndex>(IndexerSignature indexer,
                                                                                    [System.Diagnostics.CodeAnalysis.DisallowNull] params System.Object[] indecies);
        /// <summary>
        /// Invokes the setter of the specified <see cref="IndexerSignature"/> with the specified value.
        /// </summary>
        /// <param name="indexer">The indexer to invoke the setter of.</param>
        /// <param name="value">The new value of the indexer at the specified indecies.</param>
        /// <param name="indecies">The index parameters passed to the indexer.</param>
        /// <exception cref="System.ArgumentNullException"/>
        /// <exception cref="System.ArgumentOutOfRangeException"/>
        /// <exception cref="System.InvalidCastException"/>
        /// <exception cref="System.InvalidOperationException"/>
        /// <exception cref="System.NotSupportedException"/>
        /// <exception cref="System.ObjectDisposedException"/>
        public System.Threading.Tasks.Task InvokeIndexerSetterAsync<TIndex>(IndexerSignature indexer,
                                                                            [System.Diagnostics.CodeAnalysis.AllowNull] TIndex value,
                                                                            [System.Diagnostics.CodeAnalysis.DisallowNull] params System.Object[] indecies);
        /// <summary>
        /// Gets the <see cref="MethodSignature"/> that matches the specified signature.
        /// </summary>
        /// <param name="name">The name of the method to get.</param>
        /// <param name="parameters">The ordered list of parameter types in the method signature.</param>
        /// <returns>An <see cref="MethodSignature"/> object with the corresponding return type</returns>
        /// <exception cref="System.ArgumentNullException"/>
        /// <exception cref="System.Collections.Generic.KeyNotFoundException"/>
        /// <exception cref="System.ObjectDisposedException"/>
        [System.Diagnostics.Contracts.Pure]
        [return: System.Diagnostics.CodeAnalysis.NotNull]
        public MethodSignature GetMethod([System.Diagnostics.CodeAnalysis.DisallowNull] System.String name,
                                         [System.Diagnostics.CodeAnalysis.DisallowNull] params System.Type[] parameters);
        /// <summary>
        /// Gets the <see cref="PropertySignature"/> with the specified name.
        /// </summary>
        /// <param name="name">The name of the property to get.</param>
        /// <returns>An <see cref="PropertySignature"/> object, where the type parameter is the type of the property</returns>
        /// <exception cref="System.ArgumentNullException"/>
        /// <exception cref="System.Collections.Generic.KeyNotFoundException"/>
        /// <exception cref="System.ObjectDisposedException"/>
        [System.Diagnostics.Contracts.Pure]
        [return: System.Diagnostics.CodeAnalysis.NotNull]
        public PropertySignature GetProperty([System.Diagnostics.CodeAnalysis.DisallowNull] System.String name);
        /// <summary>
        /// Gets the <see cref="IndexerSignature"/> with the index parameters.
        /// </summary>
        /// <param name="parameters">The index parameters of the indexer to get.</param>
        /// <returns>An <see cref="IndexerSignature"/> object, where the type parameters are of the types of the indexer</returns>
        /// <exception cref="System.ArgumentNullException"/>
        /// <exception cref="System.Collections.Generic.KeyNotFoundException"/>
        /// <exception cref="System.ObjectDisposedException"/>
        [System.Diagnostics.Contracts.Pure]
        [return: System.Diagnostics.CodeAnalysis.NotNull]
        public IndexerSignature GetIndexer([System.Diagnostics.CodeAnalysis.DisallowNull] params System.Type[] parameters);

        /// <summary>
        /// Gets all exposed <see cref="MethodSignature"/> objects.
        /// </summary>
        [System.Diagnostics.Contracts.Pure]
        public System.Collections.Generic.IReadOnlyList<MethodSignature> Methods { get; }
        /// <summary>
        /// Gets all exposed <see cref="PropertySignature"/> objects.
        /// </summary>
        [System.Diagnostics.Contracts.Pure]
        public System.Collections.Generic.IReadOnlyList<PropertySignature> Properties { get; }
        /// <summary>
        /// Gets all exposed <see cref="IndexerSignature"/> objects.
        /// </summary>
        [System.Diagnostics.Contracts.Pure]
        public System.Collections.Generic.IReadOnlyList<IndexerSignature> Indexers { get; }
        /// <summary>
        /// Gets whether the corresponding AddIn has been activated.
        /// </summary>
        /// <exception cref="System.ObjectDisposedException"/>
        [System.Diagnostics.Contracts.Pure]
        public System.Boolean IsActive { get; }
        /// <summary>
        /// Gets the <see cref="AddInDefinition"/> for this controller.
        /// </summary>
        /// <exception cref="System.ObjectDisposedException"/>
        [System.Diagnostics.Contracts.Pure]
        [System.Diagnostics.CodeAnalysis.NotNull]
        public AddInDefinition Definition { get; }
        /// <summary>
        /// Gets the process in which the AddIn runs.
        /// </summary>
        /// <exception cref="System.InvalidOperationException"/>
        /// <exception cref="System.ObjectDisposedException"/>
        [System.Diagnostics.Contracts.Pure]
        [System.Diagnostics.CodeAnalysis.NotNull]
        public AddInProcess Process { get; }
    }
}
