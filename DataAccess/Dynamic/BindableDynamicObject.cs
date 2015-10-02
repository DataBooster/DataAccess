using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.Serialization;
using System.Security.Permissions;
using System.Dynamic;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Schema;
using System.Xml.Serialization;

namespace DbParallel.DataAccess
{
	[Serializable, XmlSchemaProvider("GetSchema"), XmlRoot(Namespace = "", ElementName = BindableDynamicObject.XmlElementName)]
	public class BindableDynamicObject : DynamicObject, ICustomTypeDescriptor, IDictionary<string, object>, ISerializable, IXmlSerializable
	{
		internal const string XmlElementName = "Record";
		private static readonly XmlQualifiedName _typeName = new XmlQualifiedName(XmlElementName, "");
		private static readonly XmlSettings _defaultXmlSettings = new XmlSettings(true);
		private readonly XmlSettings _xmlSettings;
		private readonly IDictionary<string, object> _data;

		public BindableDynamicObject() : this(null) { }

		public BindableDynamicObject(IDictionary<string, object> content, XmlSettings xmlSettings = null)
		{
			_data = content ?? new ExpandoObject();
			_xmlSettings = xmlSettings ?? _defaultXmlSettings;
		}

		#region DynamicObject Members

		/// <returns>A sequence that contains dynamic member names.</returns>
		public override IEnumerable<string> GetDynamicMemberNames()
		{
			return _data.Keys;
		}

		/// <param name="binder">Provides information about the object that called the dynamic operation. The binder.Name property provides the name of the member on which the dynamic operation is performed. For example, for the Console.WriteLine(sampleObject.SampleProperty) statement, where sampleObject is an instance of the class derived from the <see cref="T:System.Dynamic.DynamicObject"/> class, binder.Name returns "SampleProperty". The binder.IgnoreCase property specifies whether the member name is case-sensitive.</param>
		/// <param name="result">The result of the get operation. For example, if the method is called for a property, you can assign the property value to <paramref name="result"/>.</param>
		/// <returns>true if the operation is successful; otherwise, false. If this method returns false, the run-time binder of the language determines the behavior. (In most cases, a run-time exception is thrown.)</returns>
		public override bool TryGetMember(GetMemberBinder binder, out object result)
		{
			return _data.TryGetValue(binder.Name, out result);
		}

		/// <param name="binder">Provides information about the object that called the dynamic operation. The binder.Name property provides the name of the member to which the value is being assigned. For example, for the statement sampleObject.SampleProperty = "Test", where sampleObject is an instance of the class derived from the <see cref="T:System.Dynamic.DynamicObject"/> class, binder.Name returns "SampleProperty". The binder.IgnoreCase property specifies whether the member name is case-sensitive.</param>
		/// <param name="value">The value to set to the member. For example, for sampleObject.SampleProperty = "Test", where sampleObject is an instance of the class derived from the <see cref="T:System.Dynamic.DynamicObject"/> class, the <paramref name="value"/> is "Test".</param>
		/// <returns>true if the operation is successful; otherwise, false. If this method returns false, the run-time binder of the language determines the behavior. (In most cases, a language-specific run-time exception is thrown.)</returns>
		public override bool TrySetMember(SetMemberBinder binder, object value)
		{
			_data[binder.Name] = value;
			return true;
		}

		#endregion

		#region ICustomTypeDescriptor Members

		AttributeCollection ICustomTypeDescriptor.GetAttributes()
		{
			return AttributeCollection.Empty;
		}

		string ICustomTypeDescriptor.GetClassName()
		{
			return null;
		}

		string ICustomTypeDescriptor.GetComponentName()
		{
			return null;
		}

		TypeConverter ICustomTypeDescriptor.GetConverter()
		{
			return null;
		}

		EventDescriptor ICustomTypeDescriptor.GetDefaultEvent()
		{
			return null;
		}

		PropertyDescriptor ICustomTypeDescriptor.GetDefaultProperty()
		{
			return null;
		}

		object ICustomTypeDescriptor.GetEditor(Type editorBaseType)
		{
			return null;
		}

		EventDescriptorCollection ICustomTypeDescriptor.GetEvents(Attribute[] attributes)
		{
			return EventDescriptorCollection.Empty;
		}

		EventDescriptorCollection ICustomTypeDescriptor.GetEvents()
		{
			return EventDescriptorCollection.Empty;
		}

		PropertyDescriptorCollection ICustomTypeDescriptor.GetProperties(Attribute[] attributes)
		{
			return ((ICustomTypeDescriptor)this).GetProperties();
		}

		PropertyDescriptorCollection ICustomTypeDescriptor.GetProperties()
		{
			return new PropertyDescriptorCollection(_data.Select(d => new DynamicPropertyDescriptor(d.Key, d.Value.GetType())).ToArray(), readOnly: true);
		}

		object ICustomTypeDescriptor.GetPropertyOwner(PropertyDescriptor pd)
		{
			return this;
		}

		private class DynamicPropertyDescriptor : PropertyDescriptor
		{
			private static readonly Attribute[] _empty = new Attribute[0];
			private readonly Type _type;

			public DynamicPropertyDescriptor(string name, Type type)
				: base(name, _empty)
			{
				_type = type;
			}

			public override Type ComponentType
			{
				get { return typeof(BindableDynamicObject); }
			}

			public override bool IsReadOnly
			{
				get { return false; }
			}

			public override Type PropertyType
			{
				get { return _type; }
			}

			public override bool CanResetValue(object component)
			{
				return false;
			}

			public override object GetValue(object component)
			{
				IDictionary<string, object> record = component as IDictionary<string, object>;

				return (record == null) ? null : record[Name];
			}

			public override void ResetValue(object component)
			{
				IDictionary<string, object> record = component as IDictionary<string, object>;

				if (record != null)
				{
					Type t = component.GetType();

					if (t.IsValueType && Nullable.GetUnderlyingType(t) == null)
						record[Name] = Activator.CreateInstance(t);
					else
						record[Name] = DBNull.Value;
				}
			}

			public override void SetValue(object component, object value)
			{
				IDictionary<string, object> record = component as IDictionary<string, object>;

				if (record != null)
					record[Name] = value;
			}

			public override bool ShouldSerializeValue(object component)
			{
				return false;
			}
		}

		#endregion

		#region IDictionary<string, object> Members

		void IDictionary<string, object>.Add(string key, object value)
		{
			_data.Add(key, value);
		}

		bool IDictionary<string, object>.ContainsKey(string key)
		{
			return _data.ContainsKey(key);
		}

		ICollection<string> IDictionary<string, object>.Keys { get { return _data.Keys; } }

		bool IDictionary<string, object>.Remove(string key)
		{
			return _data.Remove(key);
		}

		bool IDictionary<string, object>.TryGetValue(string key, out object value)
		{
			return _data.TryGetValue(key, out value);
		}

		ICollection<object> IDictionary<string, object>.Values { get { return _data.Values; } }

		object IDictionary<string, object>.this[string key]
		{
			get { return _data[key]; }
			set { _data[key] = value; }
		}

		void ICollection<KeyValuePair<string, object>>.Add(KeyValuePair<string, object> item)
		{
			_data.Add(item);
		}

		void ICollection<KeyValuePair<string, object>>.Clear()
		{
			_data.Clear();
		}

		bool ICollection<KeyValuePair<string, object>>.Contains(KeyValuePair<string, object> item)
		{
			return _data.Contains(item);
		}

		void ICollection<KeyValuePair<string, object>>.CopyTo(KeyValuePair<string, object>[] array, int arrayIndex)
		{
			_data.CopyTo(array, arrayIndex);
		}

		int ICollection<KeyValuePair<string, object>>.Count { get { return _data.Count; } }

		bool ICollection<KeyValuePair<string, object>>.IsReadOnly { get { return _data.IsReadOnly; } }

		bool ICollection<KeyValuePair<string, object>>.Remove(KeyValuePair<string, object> item)
		{
			return _data.Remove(item);
		}

		IEnumerator<KeyValuePair<string, object>> IEnumerable<KeyValuePair<string, object>>.GetEnumerator()
		{
			return _data.GetEnumerator();
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return _data.GetEnumerator();
		}

		#endregion

		#region ISerializable Members

		[SecurityPermission(SecurityAction.Demand, Flags = SecurityPermissionFlag.SerializationFormatter)]
		public BindableDynamicObject(SerializationInfo info, StreamingContext context)
			: this()
		{
			foreach (var entry in info)
				_data.Add(entry.Name, (entry.Value == null) ? DBNull.Value : entry.Value);
		}

		[SecurityPermission(SecurityAction.Demand, SerializationFormatter = true)]
		public virtual void GetObjectData(SerializationInfo info, StreamingContext context)
		{
			foreach (var dynProperty in _data)
				info.AddValue(dynProperty.Key, Convert.IsDBNull(dynProperty.Value) ? null : dynProperty.Value);
		}

		#endregion

		public static XmlQualifiedName GetSchema(XmlSchemaSet schemas)
		{
			//	XmlSerializableServices.AddDefaultSchema(schemas, _typeName);
			return _typeName;
		}

		internal void ReadXml(XElement xe)
		{
			xe.ReadTo(_data, _xmlSettings);
		}

		#region IXmlSerializable Members

		XmlSchema IXmlSerializable.GetSchema()
		{
			return null;
		}

		void IXmlSerializable.ReadXml(XmlReader reader)
		{
			XElement x = new XElement(reader.LocalName);
			(x as IXmlSerializable).ReadXml(reader);

			ReadXml(x);
		}

		void IXmlSerializable.WriteXml(XmlWriter writer)
		{
			if (_xmlSettings.SerializePropertyAsAttribute)
			{
				foreach (var pair in _data)
					writer.WriteAttributeValue(pair.Key, pair.Value, _xmlSettings.EmitNullValue);
			}
			else // Serialize Property as XML Element
			{
				writer.PrepareTypeNamespaceAttribute("_", "", _xmlSettings.EmitDataSchemaType);

				foreach (var pair in _data)
					writer.WriteElementValue(pair.Key, pair.Value, _xmlSettings.EmitNullValue, _xmlSettings.EmitDataSchemaType);
			}
		}

		/// <summary>
		/// Specifies BindableDynamicObject XML serialization settings
		/// </summary>
		public class XmlSettings : IXmlSerializable
		{
			/// <summary>
			/// Indicates whether to emit data type attributes in the XML, or which type system to use.
			/// </summary>
			public enum DataSchemaType
			{
				/// <summary>
				/// Not to emit data type information
				/// </summary>
				None = 0,

				/// <summary>
				/// Emit XSD type information ("http://www.w3.org/2001/XMLSchema" namespace)
				/// </summary>
				Xsd = 1,

				/// <summary>
				/// Emit .NET type information ("http://schemas.microsoft.com/2003/10/Serialization/" namespace)
				/// </summary>
				Net = 2
			}

			private bool _SerializePropertyAsAttribute;
			/// <summary>
			/// <para>Gets or sets a boolean value indicating whether to serialize dynamic properties as XML attributes.</para>
			/// <para>true to serialize dynamic properties as XML attributes; otherwise, false to serialize dynamic properties as XML elements.</para>
			/// <para>The default is false.</para>
			/// </summary>
			public bool SerializePropertyAsAttribute
			{
				get
				{
					return _SerializePropertyAsAttribute;
				}
				set
				{
					_SerializePropertyAsAttribute = value;

					if (_SerializePropertyAsAttribute && _EmitDataSchemaType != DataSchemaType.None)
						_EmitDataSchemaType = DataSchemaType.None;
				}
			}

			private bool _EmitNullValue = true;
			/// <summary>
			/// <para>Gets or sets a boolean value indicating whether to serialize the null or DbNull value for a property being serialized.</para>
			/// <para>true if the null or DbNull value for a property should be generated in the serialization stream; otherwise, false.</para>
			/// <para>The default is true.</para>
			/// </summary>
			public bool EmitNullValue
			{
				get { return _EmitNullValue; }
				set { _EmitNullValue = value; }
			}

			private DataSchemaType _EmitDataSchemaType = DataSchemaType.None;
			/// <summary>
			/// <para>Gets or sets a value indicating whether to emit data type attributes in the XML, or which type system to use. This setting only apply to SerializePropertyAsAttribute = false (serialize dynamic properties as XML elements).</para>
			/// <para>None: Do not emit data type information;</para>
			/// <para>XSD: Emit XSD type information ("http://www.w3.org/2001/XMLSchema" namespace);</para>
			/// <para>NET: Emit .NET type information ("http://schemas.microsoft.com/2003/10/Serialization/" namespace);</para>
			/// <para>The default is None.</para>
			/// </summary>
			public DataSchemaType EmitDataSchemaType
			{
				get { return _EmitDataSchemaType; }
				set { if (!_SerializePropertyAsAttribute) _EmitDataSchemaType = value; }
			}

			private readonly bool _IsImplicit;

			public XmlSettings() { }

			internal XmlSettings(bool isImplicit)
			{
				_IsImplicit = isImplicit;
			}

			internal bool IsImplicit()
			{
				return _IsImplicit;
			}

			XmlSchema IXmlSerializable.GetSchema()
			{
				return null;
			}

			internal void ReadXml(XElement xe)
			{
				XNamespace defaultNamespace = xe.GetDefaultNamespace();
				bool? serializePropertyAsAttribute = (bool?)xe.Attribute(defaultNamespace + "SerializePropertyAsAttribute");
				bool? emitNullValue = (bool?)xe.Attribute(defaultNamespace + "EmitNullValue");
				string emitDataSchemaType = (string)xe.Attribute(defaultNamespace + "EmitDataSchemaType");

				if (serializePropertyAsAttribute.HasValue)
					_SerializePropertyAsAttribute = serializePropertyAsAttribute.Value;

				if (emitNullValue.HasValue)
					_EmitNullValue = emitNullValue.Value;

				if (!string.IsNullOrWhiteSpace(emitDataSchemaType))
					_EmitDataSchemaType = (DataSchemaType)Enum.Parse(typeof(DataSchemaType), emitDataSchemaType, true);
			}

			void IXmlSerializable.ReadXml(XmlReader reader)
			{
				bool? serializePropertyAsAttribute = reader.GetAttributeAsBool("SerializePropertyAsAttribute");
				bool? emitNullValue = reader.GetAttributeAsBool("EmitNullValue");
				string emitDataSchemaType = reader.GetAttribute("EmitDataSchemaType");

				if (serializePropertyAsAttribute.HasValue)
					_SerializePropertyAsAttribute = serializePropertyAsAttribute.Value;

				if (emitNullValue.HasValue)
					_EmitNullValue = emitNullValue.Value;

				if (!string.IsNullOrWhiteSpace(emitDataSchemaType))
					_EmitDataSchemaType = (DataSchemaType)Enum.Parse(typeof(DataSchemaType), emitDataSchemaType, true);
			}

			void IXmlSerializable.WriteXml(XmlWriter writer)
			{
				PropertyDescriptorCollection properties = TypeDescriptor.GetProperties(this);

				foreach (PropertyDescriptor prop in properties)
					writer.WriteAttributeValue(prop.Name, prop.GetValue(this));
			}
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
//	Created Date:		2015-01-07
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
