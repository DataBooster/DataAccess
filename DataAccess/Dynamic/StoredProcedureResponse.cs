using System.Collections.Generic;
using System.Runtime.Serialization;

namespace DbParallel.DataAccess
{
	[DataContract(Namespace = "")]
	public class StoredProcedureResponse
	{
		[DataMember(Order = 1)]
		public IList<IList<BindableDynamicObject>> ResultSets { get; set; }

		[DataMember(Order = 2)]
		public BindableDynamicObject OutputParameters { get; set; }

		[DataMember(Order = 3)]
		public object ReturnValue { get; set; }

		public StoredProcedureResponse()
		{
			ResultSets = new List<IList<BindableDynamicObject>>();
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
//	(Keep code clean rather than complicated code plus long comments.)
//
////////////////////////////////////////////////////////////////////////////////////////////////////
