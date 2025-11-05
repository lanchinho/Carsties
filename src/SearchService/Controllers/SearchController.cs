using Microsoft.AspNetCore.Mvc;
using MongoDB.Entities;
using SearchService.Models;

namespace SearchService.Controllers;

[ApiController]
[Route("api/[controller]")]
public class SearchController : ControllerBase
{
    private readonly ILogger<SearchController> _logger;

    public SearchController(ILogger<SearchController> logger)
    {
        _logger = logger;
    }

    [HttpGet]
    public async Task<IActionResult> SearchItems(string searchTerm)
    {
        var query = DB.Find<Item>();
        query.Sort(x => x.Ascending(a => a.Make));

        if (!string.IsNullOrWhiteSpace(searchTerm))
            query.Match(Search.Full, searchTerm).SortByTextScore();
        
        var result = await query.ExecuteAsync();
        if (result == null)
            return NotFound("Items not found");

        return Ok(result);
    }
}
