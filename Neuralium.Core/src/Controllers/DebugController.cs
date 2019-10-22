using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using Neuralium.Core.Controllers.Dto.Parameters;

namespace Neuralium.Core.Controllers {
	[Route("api/[controller]")]

	//[ApiController]
	public class DebugController {
		// GET api/values
		[HttpGet]
		public IEnumerable<string> Get() {
			return new[] {"value1", "value2"};
		}

		[HttpPost("GetGenesisBlock")]
		public IActionResult GetGenesisBlock([FromBody] GetGenesisBlockParameters parameters) {
			//new ObjectResult(item);
			//projects.Router.stop = true;
			// NeuraliumBlockChainInterface neuraliumBlockChainController = ServiceSet.Instance.GetService<NeuraliumBlockChainInterface>();

			// Task<int> task = neuraliumBlockChainController.GetBlockCount();

			// task.Wait();

			//return new ObjectResult(new GenesisBlockDto{iii = task.Result, Uuid = neuraliumBlockChainController.Router.NeuraliumBlockChainController.blockChain.GetBlockId(),CreatedTime = DateTime.UtcNow});
			return null;
		}

		// GET api/values/5
		// [HttpGet("{id}")]
		// public string Get(int id)
		// {
		//     return "value";
		// }

		// // POST api/values
		// [HttpPost]
		// public void Post([FromBody]string value)
		// {
		// }

		// // PUT api/values/5
		// [HttpPut("{id}")]
		// public void Put(int id, [FromBody]string value)
		// {
		// }

		// // DELETE api/values/5
		// [HttpDelete("{id}")]
		// public void Delete(int id)
		// {
		// }
	}
}