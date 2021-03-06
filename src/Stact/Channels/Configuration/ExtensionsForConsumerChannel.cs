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
namespace Stact
{
	using Configuration;
	using Configuration.Internal;


	public static class ExtensionsForConsumerChannel
	{
		/// <summary>
		/// Add a consumer to the channel using the message type defined for the channel
		/// </summary>
		/// <typeparam name="TChannel">The channel type, specifies the type of message sent by the channel</typeparam>
		/// <param name="connectionConfigurator">The connection configurator</param>
		/// <param name="consumer">The consumer to add to the channel</param>
		/// <returns>A consumer configurator to customize the consumer settings</returns>
		public static ConsumerConfigurator<TChannel> AddConsumer<TChannel>(
			this ConnectionConfigurator<TChannel> connectionConfigurator,
			Consumer<TChannel> consumer)
		{
			var channelConfigurator = new ChannelConfiguratorImpl<TChannel>();
			connectionConfigurator.AddConfigurator(channelConfigurator);

			return channelConfigurator
				.UsingConsumer(consumer);
		}

		/// <summary>
		/// Consumes the message on a ConsumerChannel, given the specified delegate
		/// </summary>
		/// <param name="configurator"></param>
		/// <param name="consumer"></param>
		/// <returns></returns>
		public static ConsumerConfigurator<TChannel> UsingConsumer<TChannel>(
			this ChannelConfigurator<TChannel> configurator, Consumer<TChannel> consumer)
		{
			var consumerConfigurator = new ConsumerConfiguratorImpl<TChannel>(consumer);

			configurator.AddConfigurator(consumerConfigurator);

			return consumerConfigurator;
		}
	}
}