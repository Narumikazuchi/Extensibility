using System;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;

namespace Narumikazuchi.Extensibility
{
    /// <summary>
    /// Represents the signature of a property.
    /// </summary>
    public readonly partial struct PropertySignature
    {
        /// <inheritdoc/>
        public override String? ToString()
        {
            String result = $"{this.Type.FullName} {this.Name} {{ ";
            if (this.HasGetter)
            {
                result += "get; ";
            }
            if (this.HasSetter)
            {
                result += "set; ";
            }
            return result += "}";
        }

        /// <summary>
        /// Gets the member name of the property.
        /// </summary>
        [NotNull]
        public String Name { get; }
        /// <summary>
        /// Gets the type of the property.
        /// </summary>
        [NotNull]
        public Type Type { get; }
        /// <summary>
        /// Gets whether this property has a getter.
        /// </summary>
        public Boolean HasGetter { get; }
        /// <summary>
        /// Gets whether this property has a setter.
        /// </summary>
        public Boolean HasSetter { get; }
    }

    // Non-Public
    partial struct PropertySignature
    {
        internal PropertySignature(PropertyInfo info)
        {
            this.Name = info.Name;
            this.Type = info.PropertyType;
            this.HasGetter = info.GetGetMethod(false) is not null;
            this.HasSetter = info.GetSetMethod(false) is not null;
        }
        internal PropertySignature(String typename,
                                   String name,
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
        }
    }

    // IEquatable<PropertySignature>
    partial struct PropertySignature : IEquatable<PropertySignature>
    {
        /// <inheritdoc/>
        public Boolean Equals(PropertySignature other) =>
            this.Name == other.Name &&
            this.Type == other.Type &&
            this.HasGetter == other.HasGetter &&
            this.HasSetter == other.HasSetter;

        /// <inheritdoc/>
        public override Boolean Equals(Object? obj) => 
            obj is PropertySignature other && 
            this.Equals(other);

        /// <inheritdoc/>
        public override Int32 GetHashCode() =>
            this.Name.GetHashCode() ^
            this.Type.GetHashCode() ^ 
            this.HasGetter.GetHashCode() ^
            this.HasSetter.GetHashCode();

#pragma warning disable
        public static Boolean operator ==(PropertySignature left, PropertySignature right) =>
            left.Equals(right);
        public static Boolean operator !=(PropertySignature left, PropertySignature right) =>
            !left.Equals(right);
#pragma warning restore
    }
}
