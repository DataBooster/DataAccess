using System;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Schema;

namespace DbParallel.DataAccess
{
	internal static class XmlWriterExtensions
	{
		private const string NsNet = "http://schemas.microsoft.com/2003/10/Serialization/";
		internal static readonly XNamespace XNsNet = NsNet;
		internal static readonly XNamespace XNsXsd = XmlSchema.Namespace;
		internal static readonly XNamespace XNsXsi = XmlSchema.InstanceNamespace;
		private static readonly XName XnNil = XNsXsi + "nil";

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
					writer.WriteValueWithType(value, emitDataSchemaType);

				writer.WriteEndElement();
			}
		}

		internal static void WriteValueWithType(this XmlWriter writer, object value, BindableDynamicObject.XmlSettings.DataSchemaType emitDataSchemaType)
		{
			switch (emitDataSchemaType)
			{
				case BindableDynamicObject.XmlSettings.DataSchemaType.Xsd:
					writer.WriteQualifiedAttributeString("type", XmlSchema.InstanceNamespace, GetXsiType(value), XmlSchema.Namespace);
					break;

				case BindableDynamicObject.XmlSettings.DataSchemaType.Net:
					writer.WriteAttributeString("Type", NsNet, value.GetType().FullName);
					break;
			}

			writer.TryWriteValue(value);
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
					writer.TryWriteValue(value);

				writer.WriteEndAttribute();
			}
		}

		private static void TryWriteValue(this XmlWriter writer, object value)
		{
			try
			{
				writer.WriteValue(value);
			}
			catch (InvalidCastException)
			{
				writer.WriteString(value.ToString());
			}
		}

		internal static void PrepareTypeNamespaceAttribute(this XmlWriter writer, string localName, string value, BindableDynamicObject.XmlSettings.DataSchemaType emitDataSchemaType)
		{
			switch (emitDataSchemaType)
			{
				case BindableDynamicObject.XmlSettings.DataSchemaType.Xsd:
					if (string.IsNullOrEmpty(writer.LookupPrefix(XmlSchema.Namespace)))
						writer.WriteAttributeString(localName, XmlSchema.Namespace, value);
					break;

				case BindableDynamicObject.XmlSettings.DataSchemaType.Net:
					if (string.IsNullOrEmpty(writer.LookupPrefix(NsNet)))
						writer.WriteAttributeString(localName, NsNet, value);
					break;
			}
		}

		internal static void PrepareTypeNamespaceRoot(this XmlWriter writer, BindableDynamicObject.XmlSettings.DataSchemaType emitDataSchemaType)
		{
			switch (emitDataSchemaType)
			{
				case BindableDynamicObject.XmlSettings.DataSchemaType.Xsd:
					if (string.IsNullOrEmpty(writer.LookupPrefix(XmlSchema.Namespace)))
						writer.WriteAttributeString("xmlns", "x", null, XmlSchema.Namespace);
					break;

				case BindableDynamicObject.XmlSettings.DataSchemaType.Net:
					if (string.IsNullOrEmpty(writer.LookupPrefix(NsNet)))
						writer.WriteAttributeString("xmlns", "z", null, NsNet);
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

		internal static string NilAwareXmlValue(this XElement xe)
		{
			return ((bool?)xe.Attribute(XnNil) ?? false) ? null : xe.Value;
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
