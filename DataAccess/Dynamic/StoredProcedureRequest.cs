using System;
using System.Data;
using System.Linq;
using System.ComponentModel;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace DbParallel.DataAccess
{
	[DataContract(Namespace = "")]
	public class StoredProcedureRequest : ICloneable
	{
		[DataMember(Order = 1)]
		public string CommandText { get; set; }

		[DataMember(Order = 2)]
		public CommandType CommandType { get; set; }

		[DataMember(Order = 3)]
		public int CommandTimeout { get; set; }

		[DataMember(Order = 4)]
		public IDictionary<string, object> InputParameters { get; set; }

		public StoredProcedureRequest()
		{
			CommandType = CommandType.StoredProcedure;
			CommandTimeout = 0;
		}

		public StoredProcedureRequest(string sp, IDictionary<string, object> parameters = null)
			: this()
		{
			Init(sp, parameters);
		}

		[Obsolete("This constructor is deprecated and will be removed in the next major release. Use StoredProcedureRequest(string sp, IDictionary<string, object> parameters) instead.", false)]
		public StoredProcedureRequest(string sp, IDictionary<string, IConvertible> parameters)
			: this()
		{
			Init(sp, parameters.ToDictionary(p => p.Key, p => (p.Value ?? DBNull.Value) as object, StringComparer.OrdinalIgnoreCase));
		}

		private void Init(string sp, IDictionary<string, object> parameters)
		{
			CommandText = sp.Trim();
			InputParameters = parameters;
		}

		public StoredProcedureRequest(string sp, object anonymousTypeInstanceAsParameters)
			: this()
		{
			IDictionary<string, object> inputParameters = anonymousTypeInstanceAsParameters as IDictionary<string, object>;

			if (inputParameters != null)
			{
				Init(sp, inputParameters);
				return;
			}

			CommandText = sp.Trim();

			if (anonymousTypeInstanceAsParameters == null)
				return;

			PropertyDescriptorCollection properties = TypeDescriptor.GetProperties(anonymousTypeInstanceAsParameters);
			InputParameters = new Dictionary<string, object>(properties.Count, StringComparer.OrdinalIgnoreCase);

			foreach (PropertyDescriptor prop in properties)
				InputParameters.Add(prop.Name, prop.GetValue(anonymousTypeInstanceAsParameters));
		}

		object ICloneable.Clone()
		{
			StoredProcedureRequest cloneRequest = new StoredProcedureRequest();

			cloneRequest.CommandText = this.CommandText;
			cloneRequest.CommandType = this.CommandType;
			cloneRequest.CommandTimeout = this.CommandTimeout;

			if (this.InputParameters != null)
				cloneRequest.InputParameters = new Dictionary<string, object>(this.InputParameters, StringComparer.OrdinalIgnoreCase);

			return cloneRequest;
		}
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
//	Created Date:		2014-12-23
//	Original Host:		http://dbParallel.codeplex.com
//	Primary Host:		http://DataBooster.codeplex.com
//	Updated Host:		https://github.com/DataBooster/DataAccess
//	Change Log:
//	Author				Date			Comment
//
//
//
//
//	(Keep code clean rather than complicated code plus long comments.)
//
////////////////////////////////////////////////////////////////////////////////////////////////////
