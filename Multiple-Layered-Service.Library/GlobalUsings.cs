

// Services
global using Multiple_Layered_Service.Library.Services.AuthRepo;
global using Multiple_Layered_Service.Library.Dtos.UserDtos;
global using Multiple_Layered_Service.Library.Services.JwtServices;
global using Multiple_Layered_Service.Library.Paginations;
global using Multiple_Layered_Service.Library.Dtos.OrderProduct;
global using Multiple_Layered_Service.Library.Dtos.OrderDtos;
global using Multiple_Layered_Service.Library.Dtos.ProductDtos;
global using Multiple_Layered_Service.Library.Helpers;
global using Multiple_Layered_Service.Library.Dtos.RoleDtos;
global using Multiple_Layered_Service.Library.Dtos.AuthDtos;


// Data acces 
global using Multiple_Layered_DataAccess.Library.Data;
global using Multiple_Layered_DataAccess.Library.Repositories;
global using Multiple_Layered_DataAccess.Library.Models;
global using Multiple_Layered_DataAccess.Library.UnitOfWork;


// Api
global using Microsoft.Extensions.Logging;
global using Microsoft.IdentityModel.Tokens;
global using System.IdentityModel.Tokens.Jwt;
global using System.Security.Claims;
global using System.Security.Cryptography;
global using System.Text;
global using Microsoft.EntityFrameworkCore;
global using Microsoft.EntityFrameworkCore.Storage;
global using Microsoft.Extensions.Caching.Distributed;
global using System.Text.Json;
global using Microsoft.AspNetCore.Identity;