using System;
using System.Collections.Generic;

namespace DbParallel.DataAccess
{
	public class StoredProcedureResponse
	{
		public List<IList<IDictionary<string, IConvertible>>> ResultSets { get; set; }

		public IDictionary<string, IConvertible> OutputParameters { get; set; }

		public IConvertible ReturnValue { get; set; }

		public Exception Error { get; set; }

		public StoredProcedureResponse()
		{
			ResultSets = new List<IList<IDictionary<string, IConvertible>>>();
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
//	Created Date:		2014-12-19
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
