using System;
using System.Xml;
using System.Xml.Schema;

namespace DbParallel.DataAccess
{
	internal static class XmlWriterExtensions
	{
		private const string NsNetXs = "http://schemas.microsoft.com/2003/10/Serialization/";

		public static void WriteElementValue(this XmlWriter writer, string localName, object value, bool emitNullValue = true,
			BindableDynamicObject.XmlSettings.DataSchemaType emitDataSchemaType = BindableDynamicObject.XmlSettings.DataSchemaType.None)
		{
			bool isNull = IsNull(value);

			if (emitNullValue || !isNull)
			{
				writer.WriteStartElement(localName);

				if (isNull)
					writer.WriteAttributeString("nil", XmlSchema.InstanceNamespace, "true");
				else
				{
					switch (emitDataSchemaType)
					{
						case BindableDynamicObject.XmlSettings.DataSchemaType.XSD:
							writer.WriteQualifiedAttributeString("type", XmlSchema.InstanceNamespace, GetXsiType(value), XmlSchema.Namespace);
							break;
						case BindableDynamicObject.XmlSettings.DataSchemaType.NET:
							writer.WriteAttributeString("type", NsNetXs, value.GetType().FullName);
							break;
					}

					writer.WriteValue(value);
				}

				writer.WriteEndElement();
			}
		}

		private static void WriteQualifiedAttributeString(this XmlWriter writer, string localName, string ns, string value, string valueNs)
		{
			writer.WriteStartAttribute(localName, ns);
			writer.WriteQualifiedName(value, valueNs);
			writer.WriteEndAttribute();
		}

		public static void WriteAttributeValue(this XmlWriter writer, string localName, object value, bool emitNullValue = true)
		{
			bool isNull = IsNull(value);

			if (emitNullValue || !isNull)
			{
				writer.WriteStartAttribute(localName);

				if (!isNull)
					writer.WriteValue(value);

				writer.WriteEndAttribute();
			}
		}

		internal static void PrepareTypeNamespaceAttribute(this XmlWriter writer, string localName, string value,
			BindableDynamicObject.XmlSettings.DataSchemaType emitDataSchemaType)
		{
			switch (emitDataSchemaType)
			{
				case BindableDynamicObject.XmlSettings.DataSchemaType.XSD:
					if (string.IsNullOrEmpty(writer.LookupPrefix(XmlSchema.Namespace)))
						writer.WriteAttributeString(localName, XmlSchema.Namespace, value);
					break;
				case BindableDynamicObject.XmlSettings.DataSchemaType.NET:
					if (string.IsNullOrEmpty(writer.LookupPrefix(NsNetXs)))
						writer.WriteAttributeString(localName, NsNetXs, value);
					break;
			}
		}

		private static string GetXsiType(object value)
		{
			Type type = value.GetType();

			return GetBuiltInXsiType(type) ?? type.Name;
		}

		private static string GetBuiltInXsiType(Type type)
		{
			if (type.IsEnum)
				return null;

			switch (Type.GetTypeCode(type))
			{
				case TypeCode.Boolean: return "boolean";
				case TypeCode.Byte: return "unsignedByte";
				case TypeCode.Char: return "char";
				case TypeCode.DateTime: return "dateTime";
				case TypeCode.Decimal: return "decimal";
				case TypeCode.Double: return "double";
				case TypeCode.Int16: return "short";
				case TypeCode.Int32: return "int";
				case TypeCode.Int64: return "long";
				case TypeCode.SByte: return "byte";
				case TypeCode.Single: return "float";
				case TypeCode.String: return "string";
				case TypeCode.UInt16: return "unsignedShort";
				case TypeCode.UInt32: return "unsignedInt";
				case TypeCode.UInt64: return "unsignedLong";
			}

			if (type == typeof(TimeSpan))
				return "duration";
			else if (type == typeof(Guid))
				return "guid";
			else if (type == typeof(Uri))
				return "anyURI";
			else if (type == typeof(XmlQualifiedName))
				return "QName";
			else if (type == typeof(byte[]))
				return "base64Binary";

			return null;
		}

		private static bool IsNull(object value)
		{
			return (value == null || Convert.IsDBNull(value));
		}
	}
}

////////////////////////////////////////////////////////////////////////////////////////////////////
//
//	Copyright 2015 Abel Cheng
//	This source code is subject to terms and conditions of the Apache License, Version 2.0.
//	See http://www.apache.org/licenses/LICENSE-2.0.
//	All other rights reserved.
//	You must not remove this notice, or any other, from this software.
//
//	Original Author:	Abel Cheng <abelcys@gmail.com>
//	Created Date:		2015-09-12
//	Original Host:		http://dbParallel.codeplex.com
//	Primary Host:		http://DataBooster.codeplex.com
//	Change Log:
//	Author				Date			Comment
//
//
//
//
//	(Keep code clean rather than complicated code plus long comments.)
//
////////////////////////////////////////////////////////////////////////////////////////////////////
