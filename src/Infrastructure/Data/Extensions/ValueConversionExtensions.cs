using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Newtonsoft.Json;

namespace N1coLoyalty.Infrastructure.Data.Extensions
{
    public static class ValueConversionExtensions
    {
        public static PropertyBuilder<T> HasJsonConversion<T>(this PropertyBuilder<T> propertyBuilder) where T : class?, new()
        {
            var converter = new ValueConverter<T, string>
            (
                v => JsonConvert.SerializeObject(v),
                v => JsonConvert.DeserializeObject<T>(v) ?? new T()
            );

            ValueComparer<T> comparer = new ValueComparer<T>
            (
                (l, r) => JsonConvert.SerializeObject(l) == JsonConvert.SerializeObject(r),
                v => JsonConvert.SerializeObject(v).GetHashCode(),
                v => JsonConvert.DeserializeObject<T>(JsonConvert.SerializeObject(v)) ?? new T()
            );

            propertyBuilder.HasConversion(converter);
            propertyBuilder.Metadata.SetValueConverter(converter);
            propertyBuilder.Metadata.SetValueComparer(comparer);
            propertyBuilder.HasColumnType("nvarchar(max)");

            return propertyBuilder;
        }
    }
}
