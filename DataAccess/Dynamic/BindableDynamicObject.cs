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

		/// <summary>
		/// Gets all property names of this dynamic object.
		/// </summary>
		/// <returns>An IEnumerable&lt;string&gt; of this dynamic object's property names.</returns>
		public IEnumerable<string> PropertyNames()
		{
			return _data.Keys;
		}

		/// <summary>
		/// Gets an IEnumerable&lt;object&gt; of this dynamic object's property values.
		/// </summary>
		/// <returns>An IEnumerable&lt;object&gt; of this dynamic object's property values.</returns>
		public IEnumerable<object> PropertyValues()
		{
			return _data.Values;
		}

		/// <summary>
		/// Gets the value with the specified propertyName. 
		/// </summary>
		/// <param name="propertyName">The property name.</param>
		/// <param name="ignoreCase">True if the specified key (propertyName) should be matched ignoring case; false otherwise. The default is true.</param>
		/// <returns>The raw value with the specified propertyName, or null if the specified propertyName is not found.</returns>
		public object Property(string propertyName, bool ignoreCase = true)
		{
			object value;
			TryGetValue(propertyName, out value, ignoreCase);
			return value;
		}

		/// <summary>
		/// Gets the value of the specified propertyName, converted to the specified type.
		/// </summary>
		/// <typeparam name="T">The type to convert the value to.</typeparam>
		/// <param name="propertyName">The property name.</param>
		/// <param name="ignoreCase">True if the specified key (propertyName) should be matched ignoring case; false otherwise. The default is true.</param>
		/// <returns>The type-converted value of the specified propertyName, or default(T) if the specified propertyName is not found.</returns>
		public T Property<T>(string propertyName, bool ignoreCase = true)
		{
			T value;
			TryGetValue<T>(propertyName, out value, ignoreCase);
			return value;
		}

		/// <summary>
		/// Creates the specified type from this dynamic object, transfers all matched properties by names.
		/// </summary>
		/// <typeparam name="T">The target object type.</typeparam>
		/// <param name="ignoreCase">True if the specified key (propertyName) should be matched ignoring case; false otherwise. The default is true.</param>
		/// <returns>The new object created from this dynamic object.</returns>
		public T ToObject<T>(bool ignoreCase = true) where T : class, new()
		{
			T instance = new T();
			return ToObject<T>(instance, ignoreCase);
		}

		/// <summary>
		/// Transfers all matched properties (by names) to a pre-created object.
		/// </summary>
		/// <typeparam name="T">The target object type.</typeparam>
		/// <param name="createdInstance">The pre-created object to be filled.</param>
		/// <param name="ignoreCase">True if the specified key (propertyName) should be matched ignoring case; false otherwise. The default is true.</param>
		/// <returns>The createdInstance.</returns>
		public T ToObject<T>(T createdInstance, bool ignoreCase = true) where T : class
		{
			if (createdInstance == null)
				return createdInstance;

			object value;

			foreach (var member in createdInstance.GetType().AllPropertiesOrFields())
				if (TryGetValue(member.ColumnName, out value, ignoreCase))
					member.SetValue(createdInstance, value);

			return createdInstance;
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
			return TryGetValue(binder.Name, out result, binder.IgnoreCase);
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

		/// <summary>
		/// Gets the value associated with the specified key (propertyName).
		/// </summary>
		/// <param name="propertyName">The specified key (propertyName) of the value to get - be matched ignoring case.</param>
		/// <param name="value">When this method returns, contains the value associated with the specified key, if the key is found; otherwise, the default value for the type of the value parameter.This parameter is passed uninitialized.</param>
		/// <returns>True if the member (propertyName) exists in the dynamic object, otherwise false.</returns>
		public bool TryGetValue(string propertyName, out object value)
		{
			return TryGetValue(propertyName, out value, true);
		}

		/// <summary>
		/// Gets the value associated with the specified key (propertyName).
		/// </summary>
		/// <param name="propertyName">The specified key (propertyName) of the value to get.</param>
		/// <param name="value">When this method returns, contains the value associated with the specified key, if the key is found; otherwise, the default value for the type of the value parameter.This parameter is passed uninitialized.</param>
		/// <param name="ignoreCase">True if the specified key (propertyName) should be matched ignoring case; false otherwise.</param>
		/// <returns>True if the member (propertyName) exists in the dynamic object, otherwise false.</returns>
		public bool TryGetValue(string propertyName, out object value, bool ignoreCase)
		{
			if (!string.IsNullOrEmpty("propertyName"))
			{
				if (_data.TryGetValue(propertyName, out value))
					return true;
				else if (ignoreCase)
				{
					foreach (var prop in _data)
						if (prop.Key.Equals(propertyName, StringComparison.OrdinalIgnoreCase))
						{
							value = prop.Value;
							return true;
						}
				}
			}

			value = null;
			return false;
		}

		/// <summary>
		/// Gets the value associated with the specified key (propertyName), converted to the specified type.
		/// </summary>
		/// <typeparam name="T">The type to convert the value to.</typeparam>
		/// <param name="propertyName">The specified key (propertyName) of the value to get.</param>
		/// <param name="value">When this method returns, contains the type-converted value associated with the specified key, if the key is found; otherwise, the default value for the type of the value parameter.This parameter is passed uninitialized.</param>
		/// <param name="ignoreCase">True if the specified key (propertyName) should be matched ignoring case; false otherwise. The default is true.</param>
		/// <returns>True if the member (propertyName) exists in the dynamic object, otherwise false.</returns>
		public bool TryGetValue<T>(string propertyName, out T value, bool ignoreCase = true)
		{
			object oValue;

			if (TryGetValue(propertyName, out oValue, ignoreCase))
			{
				value = DbExtensions.TryConvert<T>(oValue);
				return true;
			}
			else
			{
				value = default(T);
				return false;
			}
		}

		ICollection<object> IDictionary<string, object>.Values { get { return _data.Values; } }

		public object this[string propertyName]
		{
			get { return Property(propertyName); }
			set { _data[propertyName] = value; }
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

		public IEnumerator<KeyValuePair<string, object>> GetEnumerator()
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
			reader.ReadTo(_data, _xmlSettings);
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
				writer.PrepareTypeNamespaceAttribute("_", "", _xmlSettings.TypeSchema);

				foreach (var pair in _data)
					writer.WriteElementValue(pair.Key, pair.Value, _xmlSettings.EmitNullValue, _xmlSettings.TypeSchema);
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
			public enum DataTypeSchema
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

					if (_SerializePropertyAsAttribute && _TypeSchema != DataTypeSchema.None)
						_TypeSchema = DataTypeSchema.None;
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

			private DataTypeSchema _TypeSchema = DataTypeSchema.Xsd;
			/// <summary>
			/// <para>Gets or sets a value indicating whether to emit data type attributes in the XML, or which type system to use. This setting only apply to SerializePropertyAsAttribute = false (serialize dynamic properties as XML elements).</para>
			/// <para>None: Do not emit data type information;</para>
			/// <para>XSD: Emit XSD type information ("http://www.w3.org/2001/XMLSchema" namespace);</para>
			/// <para>NET: Emit .NET type information ("http://schemas.microsoft.com/2003/10/Serialization/" namespace);</para>
			/// <para>The default is None.</para>
			/// </summary>
			public DataTypeSchema TypeSchema
			{
				get { return _TypeSchema; }
				set { if (!_SerializePropertyAsAttribute) _TypeSchema = value; }
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
				string typeSchema = (string)xe.Attribute(defaultNamespace + "TypeSchema");

				if (serializePropertyAsAttribute.HasValue)
					_SerializePropertyAsAttribute = serializePropertyAsAttribute.Value;

				if (emitNullValue.HasValue)
					_EmitNullValue = emitNullValue.Value;

				if (!string.IsNullOrWhiteSpace(typeSchema))
					_TypeSchema = (DataTypeSchema)Enum.Parse(typeof(DataTypeSchema), typeSchema, true);
			}

			void IXmlSerializable.ReadXml(XmlReader reader)
			{
				bool? serializePropertyAsAttribute = reader.GetAttributeAsBool("SerializePropertyAsAttribute");
				bool? emitNullValue = reader.GetAttributeAsBool("EmitNullValue");
				string typeSchema = reader.GetAttribute("TypeSchema");

				if (serializePropertyAsAttribute.HasValue)
					_SerializePropertyAsAttribute = serializePropertyAsAttribute.Value;

				if (emitNullValue.HasValue)
					_EmitNullValue = emitNullValue.Value;

				if (!string.IsNullOrWhiteSpace(typeSchema))
					_TypeSchema = (DataTypeSchema)Enum.Parse(typeof(DataTypeSchema), typeSchema, true);
			}

			void IXmlSerializable.WriteXml(XmlWriter writer)
			{
				PropertyDescriptorCollection properties = TypeDescriptor.GetProperties(this);

				foreach (PropertyDescriptor prop in properties)
					writer.WriteAttributeValue(prop.Name, prop.GetValue(this));
			}
		}

		#endregion

		/// <summary>
		/// Convert all null values to DBNull
		/// </summary>
		public void ConvertNullToDBNull()
		{
			foreach (var prop in _data)
				if (prop.Value == null)
					_data[prop.Key] = DBNull.Value;
		}

		/// <summary>
		/// Convert all DBNull values to null
		/// </summary>
		public void ConvertDBNullToNull()
		{
			foreach (var prop in _data)
				if (Convert.IsDBNull(prop.Value))
					_data[prop.Key] = null;
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
