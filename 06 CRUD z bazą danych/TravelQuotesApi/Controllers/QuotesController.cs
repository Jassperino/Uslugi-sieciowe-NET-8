using Microsoft.AspNetCore.Mvc;
using TravelQuotesApi.Interfaces;
using TravelQuotesApi.Models;

[Route("api/[controller]")]
[ApiController]
public class QuotesController : ControllerBase
{
    private readonly IRepository<Quote> _quoteRepository;

    public QuotesController(IRepository<Quote> quoteRepository)
    {
        _quoteRepository = quoteRepository;
    }

    // GET: api/Quotes
    [HttpGet]
    public async Task<ActionResult<IEnumerable<Quote>>> GetQuotes()
    {
        var quotes = await _quoteRepository.GetAllAsync();
        return Ok(quotes);
    }

    // GET: api/Quotes/5
    [HttpGet("{id:int}")]
    public async Task<ActionResult<Quote>> GetQuote(int id)
    {
        var quote = await _quoteRepository.GetByIdAsync(id);

        if (quote is null)
        {
            return NotFound();
        }

        return Ok(quote);
    }

    // POST: api/Quotes
    [HttpPost]
    public async Task<ActionResult<Quote>> PostQuote(Quote quote)
    {
        await _quoteRepository.CreateAsync(quote);

        return CreatedAtAction(nameof(GetQuote), new { id = quote.Id }, quote);
    }

    // PUT: api/Quotes/5
    [HttpPut("{id:int}")]
    public async Task<IActionResult> PutQuote(int id, Quote quote)
    {
        if (id != quote.Id)
        {
            return BadRequest();
        }

        var existingQuote = await _quoteRepository.GetByIdAsync(id);

        if (existingQuote is null)
        {
            return NotFound();
        }

        existingQuote.Author = quote.Author;
        existingQuote.Message = quote.Message;
        await _quoteRepository.UpdateAsync(existingQuote);

        return NoContent();
    }

    // DELETE: api/Quotes/5
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> DeleteQuote(int id)
    {
        var quote = await _quoteRepository.GetByIdAsync(id);

        if (quote is null)
        {
            return NotFound();
        }

        await _quoteRepository.DeleteAsync(id);
        return NoContent();
    }
}
