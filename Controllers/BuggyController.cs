using Microsoft.AspNetCore.Mvc;
using API.Interfaces;
using API.Data;
using API.DTOs;
using System.Security.Cryptography;
using API.Entities;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;

namespace API.Controllers
{


    public class BuggyController : BaseApiController
    {
        private readonly DataContext _context;
        public BuggyController(DataContext _context)
        {
            this._context = _context;
        }

        [Authorize]
        [HttpGet("auth")]
        public ActionResult<string> GetSecret()
        {
            return "secret text";
        }

        [HttpGet("not-found")]
        public ActionResult<AppUser> GetNotFound()
        {
            var thing = _context.Users.Find(-1);


            if (thing == null) return NotFound();


            return Ok(thing);
        }
        [HttpGet("server-error")]
        public ActionResult<string> GetServerError()
        {
            var thing = _context.Users.Find(-1);


            var thingToReturn = thing.ToString();


            return thingToReturn;
        }

        [HttpGet("bad-request")]
        public ActionResult<string> GetBadRequest()
        {
            return BadRequest("This was not a good request");
        }


    }
}