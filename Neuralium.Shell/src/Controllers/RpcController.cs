using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;

namespace Neuralium.Shell.Controllers {
	[Route("rest/[controller]")]

	//[ApiController]
	public class RpcController {

		public RpcController(IHubContext<RpcHub<IRpcClient>> hubContext) {

		}

		// GET api/values
		[HttpGet]
		public IEnumerable<string> Get() {
			return new[] {"value1", "value2"};
		}

		[HttpGet("stop")]
		public IActionResult Stop() {
			//new ObjectResult(item);
			//projects.Router.stop = true;
			return new OkResult();
		}
	}
}