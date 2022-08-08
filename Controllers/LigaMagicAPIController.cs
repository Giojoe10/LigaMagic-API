using Microsoft.AspNetCore.Mvc;
using LigaMagicAPI.Models;

namespace LigaMagicAPI.Controllers;

[ApiController]
[Route("[controller]")]
public class LigaMagicAPIController : ControllerBase{

    public LigaMagicAPIController(){
    }

    [HttpGet("{name}")]
    public ActionResult<Card> Get(string name){
        return (new Card(name));
    }

}