// Copyright 2010 Chris Patterson
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
namespace Stact
{
	using System;
	using System.Threading;


	/// <summary>
	/// A channel adapter is a mutable segment in a channel network. The output channel can
	/// be replaced, allowing a new channel network (built via a ChannelVisitor) to be installed
	/// in response to changes (attachments, detachments, etc.) to the network.
	/// 
	/// This particular version handles untyped channels
	/// </summary>
	public class ChannelAdapter :
		UntypedChannel
	{
		UntypedChannel _output;

		public ChannelAdapter()
			: this(new ShuntChannel())
		{
		}

		public ChannelAdapter(UntypedChannel output)
		{
			_output = output;
		}

		public UntypedChannel Output
		{
			get { return _output; }
		}

		public void Send<T>(T message)
		{
			Output.Send(message);
		}

		public void ChangeOutputChannel(Func<UntypedChannel, UntypedChannel> mutator)
		{
			for (;;)
			{
				UntypedChannel originalValue = _output;

				UntypedChannel changedValue = mutator(originalValue);

				UntypedChannel previousValue = Interlocked.CompareExchange(ref _output, changedValue, originalValue);

				// if the value returned is equal to the original value, we made the change
				if (previousValue == originalValue)
					return;
			}
		}
	}


	/// <summary>
	/// A channel adapter is a mutable segment in a channel network. The output channel can
	/// be replaced, allowing a new channel network (built via a ChannelVisitor) to be installed
	/// in response to changes (attachments, detachments, etc.) to the network.
	/// 
	/// This particular version handles typed channels
	/// </summary>
	public class ChannelAdapter<T> :
		Channel<T>
	{
		Channel<T> _output;

		public ChannelAdapter()
			: this(new ShuntChannel<T>())
		{
		}

		public ChannelAdapter(Channel<T> output)
		{
			_output = output;
		}

		public Channel<T> Output
		{
			get { return _output; }
		}

		public void Send(T message)
		{
			Output.Send(message);
		}

		public void ChangeOutputChannel(Func<Channel<T>, Channel<T>> mutator)
		{
			for (;;)
			{
				Channel<T> originalValue = _output;

				Channel<T> changedValue = mutator(originalValue);

				Channel<T> previousValue = Interlocked.CompareExchange(ref _output, changedValue, originalValue);

				// if the value returned is equal to the original value, we made the change
				if (previousValue == originalValue)
					return;
			}
		}
	}
}