﻿using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using ServiceNow.Api.Test.Extensions;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;

namespace ServiceNow.Api.Test
{
	public class TableNameTests : ServiceNowTest
	{
		public TableNameTests(ITestOutputHelper output) : base(output)
		{
		}

		private async Task GetItems(string tableName)
		{
			var stopwatch = Stopwatch.StartNew();
			// Go and get 10 items for the type we're testing
			var page = await Client.GetPageByQueryAsync(0, 10, tableName).ConfigureAwait(false);
			Logger.LogInformation($"Call completed in {stopwatch.Elapsed.TotalSeconds:N0}s");
			// Make sure that IF we have items that they have unique SysIds
			Assert.True(page?.Items.AreDistinctBy(i => i["sys_id"]) ?? true);
		}

		private async Task<List<JObject>> GetAllItems(string tableName, string query, List<string> fieldList = null)
		{
			var stopwatch = Stopwatch.StartNew();
			// Go and get 10 items for the type we're testing
			var items = await Client.GetAllByQueryAsync(tableName, query, fieldList).ConfigureAwait(false);
			Logger.LogInformation($"Call completed in {stopwatch.Elapsed.TotalSeconds:N0}s retrieved {items.Count:N0} entries.");
			// Make sure that IF we have items that they have unique SysIds
			Assert.True(items.AreDistinctBy(i => i["sys_id"]));
			return items;
		}

		[Fact]
		public async void WindowsServers()
			=> await GetItems("cmdb_ci_win_server").ConfigureAwait(false);

		[Fact]
		public async void Servers()
			=> await GetItems("cmdb_ci_server").ConfigureAwait(false);

		/// <summary>
		/// Only get some fields for speed
		/// </summary>
		[Fact]
		public async void ServersAll()
			=> await GetAllItems("cmdb_ci_server", "install_status!=7^firewall_status=intranet", new List<string> { "sys_id", "name" }).ConfigureAwait(false);

		[Fact]
		public async void ServersAllWithFieldLimit()
			=> await GetAllItems("cmdb_ci_server", "install_status!=7", new List<string> { "sys_updated_on", "name", "sys_id", "sys_class_name" }).ConfigureAwait(false);

		[Fact]
		public async void Requests() => await GetItems("sc_request").ConfigureAwait(false);

		[Fact]
		public async void Choices()
		{
			var serverChoices = await GetAllItems("sys_choice", "name=cmdb_ci_server^element=os^inactive=false", new List<string> { "sys_id", "label", "value" }).ConfigureAwait(false);
			Assert.NotNull(serverChoices);
			Assert.Empty(serverChoices);

			var computerChoices = await GetAllItems("sys_choice", "name=cmdb_ci_computer^element=os^inactive=false", new List<string> { "sys_id", "label", "value" }).ConfigureAwait(false);
			Assert.NotNull(computerChoices);
			Assert.NotEmpty(computerChoices);
		}
	}
}
