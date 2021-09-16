using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;

namespace Narumikazuchi.Extensibility
{
    /// <summary>
    /// Represents the signature of an indexer.
    /// </summary>
    public readonly partial struct IndexerSignature
    {
        /// <inheritdoc/>
        public override String? ToString()
        {
            String result = $"{this.Type.FullName} this[{String.Join(',', this.Indecies.Select(t => t.FullName))}] {{ ";
            if (this.HasGetter)
            {
                result += "get; ";
            }
            if (this.HasSetter)
            {
                result += "set; ";
            }
            return result + "}";
        }

        /// <summary>
        /// Gets the compiler internal name of the property for the indexer.
        /// </summary>
        [NotNull]
        public String Name { get; }
        /// <summary>
        /// Gets the type of the indexer.
        /// </summary>
        [NotNull]
        public Type Type { get; }
        /// <summary>
        /// Gets the list of index types in the order of their definition.
        /// </summary>
        [NotNull]
        public IReadOnlyList<Type> Indecies { get; }
        /// <summary>
        /// Gets whether this indexer has a getter.
        /// </summary>
        public Boolean HasGetter { get; }
        /// <summary>
        /// Gets whether this indexer has a setter.
        /// </summary>
        public Boolean HasSetter { get; }
    }

    // Non-Public
    partial struct IndexerSignature
    {
        internal IndexerSignature(PropertyInfo info)
        {
            ParameterInfo[] parameters = info.GetIndexParameters();
            if (parameters.Length == 0)
            {
                throw new InvalidOperationException();
            }

            this.Name = info.Name;
            this.Type = info.PropertyType;
            this.Indecies = new List<Type>(parameters.Select(p => p.ParameterType));
            this.HasGetter = info.GetGetMethod(false) is not null;
            this.HasSetter = info.GetSetMethod(false) is not null;
        }
        internal IndexerSignature(String typename,
                                  String name,
                                  IEnumerable<String> parameters,
                                  Boolean? hasGetter,
                                  Boolean? hasSetter)
        {
            Type? type = Type.GetType(typename);
            if (type is null)
            {
                throw new TypeAccessException();
            }
            if (!hasGetter.HasValue ||
                !hasSetter.HasValue)
            {
                throw new NotSupportedException();
            }
            this.Name = name;
            this.Type = type;
            this.HasGetter = hasGetter.Value;
            this.HasSetter = hasSetter.Value;
            List<Type> paramTypes = new();
            foreach (String param in parameters)
            {
                type = Type.GetType(param);
                if (type is null)
                {
                    throw new TypeAccessException();
                }
                paramTypes.Add(type);
            }
            this.Indecies = paramTypes;
        }
    }

    // IEquatable<IndexerSignature>
    partial struct IndexerSignature : IEquatable<IndexerSignature>
    {
        /// <inheritdoc/>
        public Boolean Equals(IndexerSignature other) =>
            this.Name == other.Name &&
            this.Type == other.Type &&
            this.Indecies.SequenceEqual(other.Indecies) &&
            this.HasGetter == other.HasGetter &&
            this.HasSetter == other.HasSetter;

        /// <inheritdoc/>
        public override Boolean Equals(Object? obj) => 
            obj is IndexerSignature other && 
            this.Equals(other);

        /// <inheritdoc/>
        public override Int32 GetHashCode() => 
            this.Name.GetHashCode() ^
            this.Type.GetHashCode() ^ 
            this.Indecies.GetHashCode() ^
            this.HasGetter.GetHashCode() ^
            this.HasSetter.GetHashCode();

#pragma warning disable
        public static Boolean operator ==(IndexerSignature left, IndexerSignature right) =>
            left.Equals(right);
        public static Boolean operator !=(IndexerSignature left, IndexerSignature right) =>
            !left.Equals(right);
#pragma warning restore
    }
}
