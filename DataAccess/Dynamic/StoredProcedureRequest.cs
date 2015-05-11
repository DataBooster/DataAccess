using System;
using System.Data;
using System.ComponentModel;
using System.Collections.Generic;

namespace DbParallel.DataAccess
{
	public class StoredProcedureRequest : ICloneable
	{
		public string CommandText { get; set; }
		public CommandType CommandType { get; set; }
		public int CommandTimeout { get; set; }
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

			IDictionary<string, object> parameters = anonymousTypeInstanceAsParameters as IDictionary<string, object>;

			if (parameters != null)
			{
				Init(sp, parameters);
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
//	Change Log:
//	Author				Date			Comment
//
//
//
//
//	(Keep code clean rather than complicated code plus long comments.)
//
////////////////////////////////////////////////////////////////////////////////////////////////////
