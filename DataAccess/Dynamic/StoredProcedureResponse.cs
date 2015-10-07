using System;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Schema;
using System.Xml.Serialization;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace DbParallel.DataAccess
{
	[XmlRoot(Namespace = ""), XmlSchemaProvider("GetSchema")]
	public class StoredProcedureResponse : IXmlSerializable
	{
		#region Original Data Members
		public IList<IList<BindableDynamicObject>> ResultSets { get; set; }
		public BindableDynamicObject OutputParameters { get; set; }
		public object ReturnValue { get; set; }
		#endregion

		public StoredProcedureResponse() : this(null) { }

		public StoredProcedureResponse(BindableDynamicObject.XmlSettings xmlSettings)
		{
			ResultSets = new List<IList<BindableDynamicObject>>();
			_xmlSettings = xmlSettings ?? new BindableDynamicObject.XmlSettings();
		}

		#region Xml Serialization Decoration

		private BindableDynamicObject.XmlSettings _xmlSettings;
		private static readonly DataContractSerializer _dataContractSerializer = new DataContractSerializer(typeof(XStoredProcedureResponse));
		private static readonly XmlQualifiedName _typeName = new XmlQualifiedName(typeof(StoredProcedureResponse).Name, "");
		public static XmlQualifiedName GetSchema(XmlSchemaSet schemas)
		{
			return _typeName;
		}

		[DataContract(Namespace = "")]
		private class XStoredProcedureResponse
		{
			[CollectionDataContract(Name = "ResultSets", ItemName = "ResultSet", Namespace = "")]
			internal class XResultSets : List<IList<BindableDynamicObject>>
			{
				internal XResultSets() : base() { }
				internal XResultSets(IList<IList<BindableDynamicObject>> resultSets) : base(resultSets) { }
			}

			[XmlRoot(Namespace = "")]
			internal class XValue : IXmlSerializable
			{
				private object _Value;
				private readonly BindableDynamicObject.XmlSettings _XmlSettings;

				internal object Value
				{
					get { return _Value; }
					set { _Value = value; }
				}

				private XValue() : this(null, null) { }

				internal XValue(object simpleValue, BindableDynamicObject.XmlSettings xmlSettings)
				{
					_Value = simpleValue;
					_XmlSettings = xmlSettings ?? new BindableDynamicObject.XmlSettings(true);
				}

				internal void ReadXml(XElement xe)
				{
					_Value = xe.ReadValue(_XmlSettings);
				}

				XmlSchema IXmlSerializable.GetSchema()
				{
					return null;
				}

				void IXmlSerializable.ReadXml(XmlReader reader)
				{
					reader.ReadValue(_XmlSettings);
				}

				void IXmlSerializable.WriteXml(XmlWriter writer)
				{
					if (_Value != null)
						writer.WriteTypedValue(_Value, _XmlSettings.EmitDataSchemaType);
				}
			}

			private readonly StoredProcedureResponse _OriginalResponse;
			private XResultSets _ResultSets;
			private XValue _ReturnValue;

			[DataMember(Order = 1)]
			internal XResultSets ResultSets
			{
				get { return _ResultSets; }
				set { if (value != _ResultSets) _OriginalResponse.ResultSets = _ResultSets = value; }
			}

			[DataMember(Order = 2)]
			internal BindableDynamicObject OutputParameters
			{
				get { return _OriginalResponse.OutputParameters; }
				set { _OriginalResponse.OutputParameters = value; }
			}

			[DataMember(Order = 3)]
			internal XValue ReturnValue
			{
				get
				{
					return _ReturnValue;
				}
				set
				{
					_ReturnValue = value;
					_OriginalResponse.ReturnValue = _ReturnValue.Value;
				}
			}

			private XStoredProcedureResponse() : this(null, null) { }

			internal XStoredProcedureResponse(StoredProcedureResponse spResponse, BindableDynamicObject.XmlSettings xmlSettings)
			{
				if (xmlSettings == null)
					xmlSettings = new BindableDynamicObject.XmlSettings();

				_OriginalResponse = spResponse ?? new StoredProcedureResponse(xmlSettings);

				_ResultSets = new XResultSets(_OriginalResponse.ResultSets);

				if (_OriginalResponse.ReturnValue != null)
					_ReturnValue = new XValue(_OriginalResponse.ReturnValue, xmlSettings);
			}

			internal StoredProcedureResponse GetOriginalResponse()
			{
				return _OriginalResponse;
			}
		}

		#region IXmlSerializable Members

		XmlSchema IXmlSerializable.GetSchema()
		{
			return null;
		}

		void IXmlSerializable.ReadXml(XmlReader reader)
		{
			(_xmlSettings as IXmlSerializable).ReadXml(reader);
			int depth1 = reader.Depth + 1;

			if (reader.ReadToFirstChildElement())
			{
				while (reader.Depth >= depth1)
				{
					if (reader.NodeType == XmlNodeType.Element && reader.Depth == depth1)
					{
						switch (reader.Name)
						{
							case "ResultSets":
								if (ResultSets == null)
									ResultSets = new List<IList<BindableDynamicObject>>();
								else if (ResultSets.Count > 0)
									ResultSets.Clear();

								int depthResultSet = reader.Depth + 1;

								if (reader.ReadToFirstChildElement())
								{
									while (reader.Depth >= depthResultSet)
									{
										if (reader.NodeType == XmlNodeType.Element && reader.Depth == depthResultSet && reader.Name == "ResultSet")
										{
											List<BindableDynamicObject> resultSet = new List<BindableDynamicObject>();
											int depthRecord = reader.Depth + 1;

											if (reader.ReadToFirstChildElement())
											{
												while (reader.Depth >= depthRecord)
												{
													if (reader.NodeType == XmlNodeType.Element && reader.Depth == depthRecord && reader.Name == "Record")
														resultSet.Add(reader.ReadDynamicObject(_xmlSettings));
													else
														reader.Read();
												}
											}
											ResultSets.Add(resultSet);
										}
										else
											reader.Read();
									}
								}
								break;

							case "OutputParameters":
								OutputParameters = reader.ReadDynamicObject(_xmlSettings);
								break;

							case "ReturnValue":
								ReturnValue = reader.ReadValue(_xmlSettings);
								break;

							default:
								reader.Read();
								break;
						}
					}
					else
						reader.Read();
				}
			}
		}

		void IXmlSerializable.WriteXml(XmlWriter writer)
		{
			writer.PrepareTypeNamespaceRoot(_xmlSettings.EmitDataSchemaType);
			(_xmlSettings as IXmlSerializable).WriteXml(writer);

			XStoredProcedureResponse responseXml = new XStoredProcedureResponse(this, _xmlSettings);

			_dataContractSerializer.WriteObjectContent(writer, responseXml);
		}

		#endregion
		#endregion
	}
}

////////////////////////////////////////////////////////////////////////////////////////////////////
//
//	Copyright 2014 Abel Cheng
//	This source code is subject to terms and conditions of the Apache License, Version 2.0.
//	See http://www.apache.org/licenses/LICENSE-2.0.
//	All other rights reserved.
//	You must not remove this notice, or any other, from this software.
//
//	Original Author:	Abel Cheng <abelcys@gmail.com>
//	Created Date:		2014-12-19
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
