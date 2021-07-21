using System;
using MessagePack;
using MessagePack.Formatters;

namespace DashFire.Dashboard.Framework.SerializerOptions
{
    public class DateTimeFormatter : IMessagePackFormatter<DateTime>
    {
        public DateTime Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options)
        {
            if (reader.TryReadNil())
            {
                return default(DateTime);
            }

            options.Security.DepthStep(ref reader);

            var ticks = reader.ReadInt64();

            return new DateTime(ticks);
        }

        public void Serialize(ref MessagePackWriter writer, DateTime value, MessagePackSerializerOptions options)
        {
            if (value == null)
            {
                writer.WriteNil();
                return;
            }

            writer.WriteInt64(value.Ticks);
        }
    }
}
