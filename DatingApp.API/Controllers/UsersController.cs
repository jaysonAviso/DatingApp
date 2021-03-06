using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using AutoMapper;
using DatingApp.API.Data;
using DatingApp.API.Dtos;
using DatingApp.API.Helpers;
using DatingApp.API.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace DatingApp.API.Controllers
{
    [Authorize]
    public class UsersController : BaseApiController
    {
        private readonly IDatingRepository _repo;
        private readonly IMapper _mapper;
        private readonly IPhotoService _photoService;
        public UsersController(IDatingRepository repo, IMapper mapper, IPhotoService photoService)
        {
            _photoService = photoService;
            _mapper = mapper;
            _repo = repo;
        }

        [HttpGet]
        public async Task<IActionResult> GetUsers([FromQuery]UserParams userParams)
        {
            var user = await _repo.GetByUsername(User.GetUsername());
            userParams.CurrentUser = user.UserName;

            if(string.IsNullOrEmpty(userParams.Gender))
                userParams.Gender = user.Gender == "male"? "female" : "male";

            var users = await _repo.GetUsers(userParams);

            Response.AddPagenationHeader(users.CurrentPage, users.PageSize, users.TotalCount, users.TotalPages);

            return Ok(users);
        }

        [HttpGet("{username}")]
        public async Task<IActionResult> GetUser(string username)
        {
            var user = await _repo.GetByUsername(username);

            if (user == null) return BadRequest("there's no username " + username);

            var userToReturn = _mapper.Map<UserDetailedDto>(user);

            return Ok(userToReturn);
        }

        [HttpPut("{username}", Name = "GetUser")]
        public async Task<IActionResult> UpdateUser(string username, UserForUpdateDto userForUpdateDto)
        {
            if (username != User.GetUsername()) return Unauthorized();

            var userFromRepo = await _repo.GetByUsername(username);

            _mapper.Map(userForUpdateDto, userFromRepo);

            if (await _repo.SaveAll()) return NoContent();

            return BadRequest($"Updating user failed on save");
        }

        [HttpPost("add-photo")]
        public async Task<ActionResult<PhotoDto>> AddPhoto(IFormFile file)
        {
            var user = await _repo.GetByUsername(User.GetUsername());

            var result = await _photoService.AddPhoto(file);

            if(result.Error != null) return BadRequest(result.Error.Message);

            var photo = new Photo
            {
                Url = result.SecureUrl.AbsoluteUri,
                PublicId = result.PublicId
            };

            if(user.Photos.Count == 0)
            {
                photo.IsMain = true;
            }

            user.Photos.Add(photo);

            if (await _repo.SaveAll())
                return Created("GetUser",_mapper.Map<PhotoDto>(photo));

            return BadRequest("Problem adding photo");
        }

        [HttpPost("setMain/{photoId}")]
        public async Task<IActionResult> SetMainPhoto(int photoId)
        {
            var user = await _repo.GetByUsername(User.GetUsername());

            if(!user.Photos.Any(p => p.Id == photoId))
                return Unauthorized();

            var photoFromRepo = await _repo.GetPhoto(photoId);

            if(photoFromRepo.IsMain)
                return BadRequest("This is allready main photo");

            var mainPhoto = await _repo.GetMainPhoto(user.Id);
            mainPhoto.IsMain = false;

            photoFromRepo.IsMain = true;

            if(await _repo.SaveAll())
                return NoContent();

            return BadRequest("Could not set photo to main");
        }

        [HttpDelete("delete-photo/{photoId}")]
        public async Task<ActionResult> DeletePhoto(int photoId)
        {
            var user = await _repo.GetByUsername(User.GetUsername());

            var photo = user.Photos.FirstOrDefault(p => p.Id == photoId);

            if(photo == null) return NotFound();

            if(photo.IsMain) return BadRequest("You cannot delete your main photo");

            if(photo.PublicId != null)
            {
                var result = await _photoService.DeletePhoto(photo.PublicId);
                if(result.Error != null) return BadRequest(result.Error.Message);
            }

            user.Photos.Remove(photo);

            if(await _repo.SaveAll()) return Ok();

            return BadRequest("Failed to delete the photo");
        }
    }
}