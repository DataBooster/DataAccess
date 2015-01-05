using System;
using System.Data;
using System.Collections.Generic;

namespace DbParallel.DataAccess
{
	public class StoredProcedureRequest : ICloneable
	{
		public string CommandText { get; set; }
		public CommandType CommandType { get; set; }
		public int CommandTimeout { get; set; }
		public IDictionary<string, IConvertible> InputParameters { get; set; }

		public StoredProcedureRequest()
		{
			CommandType = CommandType.StoredProcedure;
			CommandTimeout = 0;
		}

		object ICloneable.Clone()
		{
			StoredProcedureRequest cloneRequest = new StoredProcedureRequest();

			cloneRequest.CommandText = this.CommandText;
			cloneRequest.CommandType = this.CommandType;
			cloneRequest.CommandTimeout = this.CommandTimeout;
			cloneRequest.InputParameters = new Dictionary<string, IConvertible>(this.InputParameters, StringComparer.OrdinalIgnoreCase);

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
//	(Keep clean code rather than complicated code plus long comments.)
//
////////////////////////////////////////////////////////////////////////////////////////////////////
