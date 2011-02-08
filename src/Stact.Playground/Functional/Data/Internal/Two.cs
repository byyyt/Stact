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
namespace Stact.Functional.Data.Internal
{
	using System;


	public class Two<T, M> :
		Digit<T, M>
	{
		readonly Element<T, M> _v1;
		readonly Element<T, M> _v2;

		public Two(Measured<T, M> m, Element<T, M> v1, Element<T, M> v2)
			: base(m, m.Append(v1.Size, v2.Size))
		{
			_v1 = v1;
			_v2 = v2;
		}

		public Element<T, M> V1
		{
			get { return _v1; }
		}

		public Element<T, M> V2
		{
			get { return _v2; }
		}

		public override U FoldRight<U>(Func<T, Func<U, U>> f, U z)
		{
			return f(_v1.Value)(f(_v2.Value)(z));
		}

		public override U FoldLeft<U>(Func<U, Func<T, U>> f, U z)
		{
			return f(f(z)(_v2.Value))(_v1.Value);
		}

		public override Element<T, M> Left
		{
			get { return _v1; }
		}

		public override Element<T, M> Right
		{
			get { return _v2; }
		}

		public override U Match<U>(Func<One<T, M>, U> one, Func<Two<T, M>, U> two, Func<Three<T, M>, U> three,
		                           Func<Four<T, M>, U> four)
		{
			return two(this);
		}
	}
}