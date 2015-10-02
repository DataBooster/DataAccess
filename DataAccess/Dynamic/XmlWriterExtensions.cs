using System;
using System.Linq;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Schema;
using System.Data.Linq;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Globalization;

namespace DbParallel.DataAccess
{
	internal static class XmlWriterExtensions
	{
		private const string XsdTypeAttributeName = "type";
		private const string NetTypeAttributeName = "Type";
		private const string NsNet = "http://schemas.microsoft.com/2003/10/Serialization/";
		internal static readonly XNamespace XNsNet = NsNet;
		internal static readonly XNamespace XNsXsd = XmlSchema.Namespace;
		internal static readonly XNamespace XNsXsi = XmlSchema.InstanceNamespace;
		private static readonly XName XnNil = XNsXsi + "nil";
		private static readonly DataContractSerializer _ObjectDataContractSerializer = new DataContractSerializer(typeof(object));

		#region Xml Writer

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
					try
					{
						_ObjectDataContractSerializer.WriteObjectContent(writer, value);
					}
					catch
					{
						Type type = value.GetType();
						string xsiType = GetBuiltInXsiType(type);

						if (xsiType == null)
							writer.WriteAttributeString(XsdTypeAttributeName, XmlSchema.InstanceNamespace, type.FullName);
						else
							writer.WriteQualifiedAttributeString(XsdTypeAttributeName, XmlSchema.InstanceNamespace, xsiType, XmlSchema.Namespace);

						writer.TryWriteValue(value);
					}
					break;

				case BindableDynamicObject.XmlSettings.DataSchemaType.Net:
					writer.WriteAttributeString(NetTypeAttributeName, NsNet, value.GetType().FullName);
					writer.TryWriteValue(value);
					break;
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

		#endregion

		#region Xml Reader - by XElement

		private static Type GetXsdType(string xsdType)
		{
			switch (xsdType)
			{
				case null:
				case "":
				case "string": return typeof(string);
				case "boolean": return typeof(bool);
				case "unsignedByte": return typeof(byte);
				case "char": return typeof(char);
				case "dateTime": return typeof(DateTime);
				case "decimal": return typeof(decimal);
				case "double": return typeof(double);
				case "short": return typeof(short);
				case "int": return typeof(int);
				case "long": return typeof(long);
				case "byte": return typeof(sbyte);
				case "float": return typeof(float);
				case "unsignedShort": return typeof(ushort);
				case "unsignedInt": return typeof(uint);
				case "unsignedLong": return typeof(ulong);
				case "duration": return typeof(TimeSpan);
				case "guid": return typeof(Guid);
				case "anyURI": return typeof(Uri);
				case "QName": return typeof(XmlQualifiedName);
				case "base64Binary": return typeof(byte[]);
			}

			return Type.GetType(xsdType) ?? typeof(string);
		}

		private static bool IsNull(object value)
		{
			return (value == null || Convert.IsDBNull(value));
		}

		internal static object ReadValue(this XElement xe, BindableDynamicObject.XmlSettings xmlSettings)
		{
			if ((bool?)xe.Attribute(XnNil) ?? false)
				return null;

			string declaredType = null;

			switch (xmlSettings.EmitDataSchemaType)
			{
				case BindableDynamicObject.XmlSettings.DataSchemaType.Xsd:
					declaredType = xe.GetXsdTypeAttributeString();
					if (string.IsNullOrEmpty(declaredType))
					{
						if (xmlSettings.IsImplicit())
							declaredType = xe.GetNetTypeAttributeString();
					}
					else
					{
						try { return _ObjectDataContractSerializer.ReadObject(xe.CreateReader(), false); }
						catch { }
					}
					break;

				case BindableDynamicObject.XmlSettings.DataSchemaType.Net:
					declaredType = xe.GetNetTypeAttributeString();
					if (declaredType == null && xmlSettings.IsImplicit())
						declaredType = xe.GetXsdTypeAttributeString();
					break;

				default:
					if (xmlSettings.IsImplicit())
					{
						declaredType = xe.GetXsdTypeAttributeString();
						if (string.IsNullOrEmpty(declaredType))
							declaredType = xe.GetNetTypeAttributeString();
						else
						{
							try { return _ObjectDataContractSerializer.ReadObject(xe.CreateReader(), false); }
							catch { }
						}
					}
					break;
			}

			Type valueType = GetXsdType(declaredType);

			if (valueType == typeof(string))
				return xe.Value;

			try
			{
				return DBConvert.ChangeType(xe.Value, valueType);
			}
			catch
			{
				return xe.Value;
			}
		}

		private static string GetXsdTypeAttributeString(this XElement xe)
		{
			XAttribute declaredAttribute = xe.Attribute(XNsXsi + XsdTypeAttributeName);

			if (declaredAttribute == null)
				return null;
			if (string.IsNullOrEmpty(declaredAttribute.Value))
				return declaredAttribute.Value;

			string declaredType = declaredAttribute.Value;
			int colon = declaredType.IndexOf(':');

			return (colon < 0) ? declaredType : declaredType.Substring(colon + 1);
		}

		private static string GetNetTypeAttributeString(this XElement xe)
		{
			XAttribute declaredAttribute = xe.Attribute(XNsNet + NetTypeAttributeName);

			if (declaredAttribute == null)
				return null;
			else
				return declaredAttribute.Value;
		}

		private static void ReadAttributes(this XElement xe, IDictionary<string, object> dynamicObject)
		{
			XNamespace defaultNamespace = xe.GetDefaultNamespace();
			var localAttributes = xe.Attributes().Where(a => a.Name.Namespace == defaultNamespace);

			foreach (var attr in localAttributes)
				dynamicObject[attr.Name.LocalName] = attr.Value;
		}

		private static void ReadElements(this XElement xe, IDictionary<string, object> dynamicObject, BindableDynamicObject.XmlSettings xmlSettings)
		{
			XNamespace defaultNamespace = xe.GetDefaultNamespace();

			foreach (var e in xe.Elements().Where(e => e.Name.Namespace == defaultNamespace))
				dynamicObject[e.Name.LocalName] = e.ReadValue(xmlSettings) ?? DBNull.Value;
		}

		internal static void ReadTo(this XElement xe, IDictionary<string, object> dynamicObject, BindableDynamicObject.XmlSettings xmlSettings)
		{
			if (xmlSettings.IsImplicit())
			{
				xe.ReadAttributes(dynamicObject);
				xe.ReadElements(dynamicObject, xmlSettings);
			}
			else
				if (xmlSettings.SerializePropertyAsAttribute)
					xe.ReadAttributes(dynamicObject);
				else
					xe.ReadElements(dynamicObject, xmlSettings);
		}

		#endregion

		#region Xml Reader - raw


		internal static bool? GetAttributeAsBool(this XmlReader reader, string name)
		{
			string strValue = reader.GetAttribute(name);

			if (strValue == null)
				return null;
			else
				return XmlConvert.ToBoolean(strValue.ToLower(CultureInfo.InvariantCulture));
		}

		#endregion
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
