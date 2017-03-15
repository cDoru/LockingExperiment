using System;
using System.Collections.Generic;
using System.Web.Http;
using LockingWebApp.Locks.Contracts;

namespace LockingWebApp.Controllers
{
    public class ValuesController : ApiController
    {
        private readonly ILock _lock;

        public ValuesController( ILock @lock)
        {
            _lock = @lock;
        }

        // GET api/values
        public IEnumerable<string> Get()
        {
            using (var handle = _lock.Acquire("api/values", TimeSpan.FromSeconds(5)))
            {
                if (handle.AcquisitionFailed)
                {
                    return new[] {"too fast"};
                }

                return new[] { "value1", "value2" };
            }
        }

        // GET api/values/5
        public string Get(int id)
        {
            return "value";
        }

        // POST api/values
        public void Post([FromBody]string value)
        {
        }

        // PUT api/values/5
        public void Put(int id, [FromBody]string value)
        {
        }

        // DELETE api/values/5
        public void Delete(int id)
        {
        }
    }
}
