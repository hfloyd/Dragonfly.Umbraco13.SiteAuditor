namespace Dragonfly.SiteAuditor.Models
{
	using System;
	using Newtonsoft.Json;

	public class RawMediaWithCrops
	{
		[JsonProperty("key")]
		public Guid Key { get; set; }

		[JsonProperty("mediaKey")]
		public Guid MediaKey { get; set; }
	}
}
