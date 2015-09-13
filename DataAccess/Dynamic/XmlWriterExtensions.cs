using System;
using System.Xml;
using System.Xml.Schema;

namespace DbParallel.DataAccess
{
	internal static class XmlWriterExtensions
	{
		public static void WriteElement(this XmlWriter writer, string localName, object value, bool emitNullValue = true)
		{
			bool isNull = IsNull(value);

			if (emitNullValue || !isNull)
			{
				writer.WriteStartElement(localName);

				if (isNull)
					writer.WriteAttributeString("nil", XmlSchema.InstanceNamespace, "true");
				else
					writer.WriteValue(value);

				writer.WriteEndElement();
			}
		}

		public static void WriteAttribute(this XmlWriter writer, string localName, object value, bool emitNullValue = true)
		{
			bool isNull = IsNull(value);

			if (emitNullValue || !isNull)
			{
				writer.WriteStartAttribute(localName);

				if (isNull)
					writer.WriteString(string.Empty);
				else
					writer.WriteValue(value);

				writer.WriteEndAttribute();
			}
		}

		private static bool IsNull(object value)
		{
			return (value == null || Convert.IsDBNull(value));
		}
	}
}
