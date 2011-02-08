﻿// Copyright 2010 Chris Patterson
//  
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use 
// this file except in compliance with the License. You may obtain a copy of the 
// License at 
// 
//     http://www.apache.org/licenses/LICENSE-2.0 
// 
// Unless required by applicable law or agreed to in writing, software distributed 
// under the License is distributed on an "AS IS" BASIS, WITHOUT WARRANTIES OR 
// CONDITIONS OF ANY KIND, either express or implied. See the License for the 
// specific language governing permissions and limitations under the License.
namespace Stact.Internal
{
	using System.Collections;
	using System.Collections.Generic;


	public class RightProjectionImpl<TLeft, TRight> :
		RightProjection<TLeft, TRight>
	{
		readonly Either<TLeft, TRight> _either;

		public RightProjectionImpl(Either<TLeft, TRight> either)
		{
			_either = either;
		}

		public IEnumerator<TRight> GetEnumerator()
		{
			if (_either.IsRight)
				yield return ((RightImpl<TLeft, TRight>)_either).Value;
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return GetEnumerator();
		}

		public Either<TLeft, TRight> Either
		{
			get { return _either; }
		}
	}
}