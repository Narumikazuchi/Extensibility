using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;

namespace Narumikazuchi.Extensibility
{
    /// <summary>
    /// Represents a signature for a method.
    /// </summary>
    public readonly partial struct MethodSignature
    {
        /// <inheritdoc/>
        public override String? ToString() =>
            $"{this.ReturnType.FullName} {this.Name}({String.Join(',', this.Parameters.Select(t => t.FullName))})";

        /// <summary>
        /// Gets the name of the method.
        /// </summary>
        [NotNull]
        public String Name { get; }
        /// <summary>
        /// Gets the type which the method returns.
        /// </summary>
        [NotNull]
        public Type ReturnType { get; }
        /// <summary>
        /// Gets the list of parameter type in order of their definition.
        /// </summary>
        [NotNull]
        public IReadOnlyList<Type> Parameters { get; }
    }

    // Non-Public
    partial struct MethodSignature
    {
        internal MethodSignature(MethodInfo info)
        {
            this.Name = info.Name;
            this.ReturnType = info.ReturnType;
            this.Parameters = new List<Type>(info.GetParameters()
                                                 .Select(p => p.ParameterType));
        }
        internal MethodSignature(String typname,
                                 String name,
                                 IEnumerable<String> parameters)
        {
            Type? type = Type.GetType(typname);
            if (type is null)
            {
                throw new TypeAccessException();
            }
            this.Name = name;
            this.ReturnType = type;
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
            this.Parameters = paramTypes;
        }
    }

    // IEquatable<MethodSignature>
    partial struct MethodSignature : IEquatable<MethodSignature>
    {
        /// <inheritdoc/>
        public Boolean Equals(MethodSignature other) =>
            this.Name == other.Name &&
            this.ReturnType == other.ReturnType &&
            this.Parameters.SequenceEqual(other.Parameters);

        /// <inheritdoc/>
        public override Boolean Equals(Object? obj) => 
            obj is MethodSignature other && 
            this.Equals(other);

        /// <inheritdoc/>
        public override Int32 GetHashCode() =>
            this.Name.GetHashCode() ^
            this.ReturnType.GetHashCode() ^
            this.Parameters.GetHashCode();

#pragma warning disable
        public static Boolean operator ==(MethodSignature left, MethodSignature right) =>
            left.Equals(right);
        public static Boolean operator !=(MethodSignature left, MethodSignature right) =>
            !left.Equals(right);
#pragma warning restore
    }
}
