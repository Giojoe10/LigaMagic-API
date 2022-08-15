using Microsoft.AspNetCore.Mvc;
using LigaMagicAPI.Models;
using LigaMagicAPI.Services;

namespace LigaMagicAPI.Controllers;

[ApiController]
[Route("[controller]")]
public class LigaMagicAPIController : ControllerBase{

    public LigaMagicAPIController(){
    }

    [HttpGet("{name}")]
    public ActionResult<Card> Get(string name){
        var card = LigaMagicAPIService.Get(name);
        if(card is null){
            return NotFound();
        }
        return card;
    }

}