using AutoMapper;
using AutoMapper.Configuration.Conventions;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Mvc;
using PokemonReviewApp.Dto;
using PokemonReviewApp.Interfaces;
using PokemonReviewApp.Models;
using PokemonReviewApp.Repository;

namespace PokemonReviewApp.Controllers
{
    [Microsoft.AspNetCore.Mvc.Route("api/[controller]")]
    [ApiController]
    public class PokemonController : Controller
    {
        private readonly IPokemonRepository _pokemonRepository;
        private readonly IOwnerRepository _ownerRepository;
        private readonly IReviewRepository _reviewRepository;
        private readonly ICategoryRepository _categoryRepository;   
        private readonly IMapper _mapper;
        public PokemonController(IPokemonRepository pokemonRepository,IOwnerRepository ownerRepository, IReviewRepository reviewRepository,ICategoryRepository categoryRepository, IMapper mapper)
        {
            _pokemonRepository = pokemonRepository;
            _ownerRepository = ownerRepository;
            _reviewRepository = reviewRepository;
            _categoryRepository = categoryRepository;
            _mapper = mapper;
        }

        [HttpGet]
        [ProducesResponseType(200, Type = typeof(IEnumerable<Pokemon>))]
        public IActionResult GetPokemons()
        {
            var pokemons = _mapper.Map<List<PokemonDto>>(_pokemonRepository.GetPokemons());
            if(!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            return Ok(pokemons);
        }

        [HttpGet("{pokeId}")]
        [ProducesResponseType(200, Type = typeof(Pokemon))]
        [ProducesResponseType(400)]
        public IActionResult GetPokemon(int pokeId)
        {
            if(!_pokemonRepository.PokemonExists(pokeId))
            {
                return NotFound();
            }
            var pokemon = _mapper.Map<PokemonDto>(_pokemonRepository.GetPokemon(pokeId));
            if(!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            return Ok(pokemon);
        }

        [HttpGet("{pokemonName}/name")]
        [ProducesResponseType(200, Type = typeof(Pokemon))]
        [ProducesResponseType(400)]
        public IActionResult GetPokemon(string pokemonName)
        {
            var pokemon = _pokemonRepository.GetPokemon(pokemonName);
            if (!_pokemonRepository.PokemonExists(pokemon.Id))
            {
                return NotFound();
            }
            var pokemon1 = _mapper.Map<PokemonDto>(_pokemonRepository.GetPokemon(pokemonName));
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            return Ok(pokemon1);
        }

        [HttpGet("{pokeId}/rating")]
        [ProducesResponseType(200, Type = typeof(decimal))]
        [ProducesResponseType(400)]
        public IActionResult getPokemonRating(int pokeId)
        {
            if(!_pokemonRepository.PokemonExists(pokeId))
            {
                return NotFound();
            }
            var rating = _pokemonRepository.GetPokemonRating(pokeId);
            if (!ModelState.IsValid)
                return BadRequest();
            return Ok(rating);
        }

        [HttpPost]
        [ProducesResponseType(204)]
        [ProducesResponseType(400)]
        public IActionResult CreatePokemon([FromQuery] int ownerId, [FromQuery] int catId, [FromBody] PokemonDto pokemonCreate)
        {
            if (pokemonCreate == null)
                return BadRequest(ModelState);

            var pokemon = _pokemonRepository.GetPokemons()
                .Where(p => p.Name.Trim().ToUpper() == pokemonCreate.Name.Trim().ToUpper())
                .FirstOrDefault();

            if(pokemon != null)
            {
                ModelState.AddModelError("", "Pokemon already exists");
                return StatusCode(422, ModelState);
            }

            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var pokemonMap = _mapper.Map<Pokemon>(pokemonCreate);

            if(!_pokemonRepository.CreatePokemon(ownerId, catId, pokemonMap))
            {
                ModelState.AddModelError("", "Something went wrong while saving");
                return StatusCode(500, ModelState);
            }

            return Ok("Successfully Created");
        }

        [HttpPut("{pokeId}")]
        [ProducesResponseType(204)]
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]
        public IActionResult UpdatePokemon(int pokeId,[FromQuery] int ownerId, [FromQuery] int catId, [FromBody] PokemonDto pokemonUpdate)
        {
            if (pokemonUpdate == null)
                return BadRequest(ModelState);

            if (pokeId != pokemonUpdate.Id)
                return BadRequest(ModelState);

            if (!_pokemonRepository.PokemonExists(pokeId))
            {
                return NotFound();
            }

            if (!ModelState.IsValid)
                return BadRequest();

            var pokemonMap = _mapper.Map<Pokemon>(pokemonUpdate);

            if (!_pokemonRepository.UpdatePokemon(ownerId, catId, pokemonMap))
            {
                ModelState.AddModelError("", "Something went wrong while updating country.");
                return StatusCode(500, ModelState);
            }

            return NoContent();
        }

        [HttpDelete("{pokeId}")]
        [ProducesResponseType(204)]
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]
        public IActionResult DeletePokemon(int pokeId)
        {
            if (!_pokemonRepository.PokemonExists(pokeId))
            {
                return NotFound(ModelState);
            }

            var reviewsToDelete = _reviewRepository.GetReviewsOfAPokemon(pokeId);
            var pokemonToDelete = _pokemonRepository.GetPokemon(pokeId);

            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            if(!_reviewRepository.DeleteReviews(reviewsToDelete.ToList()))
            {
                ModelState.AddModelError("", "Something went wrong when deleting reviews");
                return StatusCode(422, ModelState);
            }

            if (!_pokemonRepository.DeletePokemon(pokemonToDelete))
            {
                ModelState.AddModelError("", "Something went wrong deleting pokemon");
                return StatusCode(500, ModelState);
            }

            return NoContent();
        }
    }
}
