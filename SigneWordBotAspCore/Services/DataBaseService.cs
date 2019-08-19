using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using SigneWordBotAspCore.Models;
using SigneWordBotAspCore.Utils;
using SigneWordBotAspCore.BotCommands;
using SigneWordBotAspCore.BotCommands.Options;
using SigneWordBotAspCore.Exceptions;
using Telegram.Bot.Types;
using TgUser = Telegram.Bot.Types.User;

namespace SigneWordBotAspCore.Services
{
    public class DataBaseService: IDataBaseService
    {
        private readonly IAppContext _appContext;
        
        public DataBaseService(IAppContext appContext)
        {
            _appContext = appContext;
            GetUsers();
        }

        public void GetUsers()
        {
           
            
        }
        
        public int CreateUser(User tgUser, string password)
        {
            using (var db = new SwDbContext(_appContext))
            {
                if (db.User.Any(u => u.TgId == tgUser.Id))
                    return -1;
                
                var res = db.User.Add(new UserModel
                {
                    TgId = tgUser.Id,
                    Password = password,
                    FirstName = tgUser.FirstName,
                    SecondName = tgUser.LastName,
                    TgUsername = tgUser.Username
                });
                
                db.SaveChanges();
                return res.Entity.Id;
            }
        }

        public int CreateBasket(User user, string name, string basketPass = null, string description = null)
        {
            using (var context = new SwDbContext(_appContext))
            {
                var currentUser = context.User.FirstOrDefault(u => u.TgId == user.Id);

                if (currentUser == null)
                    throw new UserNotFoundException();

                var isBasketExist = context.User.Include(u => u.UserBasket).ThenInclude(u => u.BasketModel)
                    .Any(u => u.UserBasket.Select(ub => ub.BasketModel).Any(b => b.Name == name));
                
                if (isBasketExist)
                    throw new BasketAlreadyExistException();

                var basket = context.PasswordsBasket.Add(new PasswordsBasketModel
                {
                    BasketPass = basketPass,
                    Description = description,
                    Name = name
                    
                });

                var userBasket = context.UserBasket.Add(new UserBasket
                {
                    BasketId = basket.Entity.Id,
                    UserId = currentUser.Id,
                    AccessTypeId = 1
                });
                context.SaveChanges();

                return userBasket.Entity.Id;
            }

        }

        public int CreateBasket(long userId, string name, string basketPass = null, string description = null)
        {
            using (var context = new SwDbContext(_appContext))
            {
                var currentUser = context.User.FirstOrDefault(u => u.Id == userId);

                if (currentUser == null)
                    throw new UserNotFoundException();

                var isBasketExist = context.User.Include(u => u.UserBasket).ThenInclude(u => u.BasketModel)
                    .Any(u => u.UserBasket.Select(ub => ub.BasketModel).Any(b => b.Name == name));
                
                if (isBasketExist)
                    throw new BasketAlreadyExistException();

                var basket = context.PasswordsBasket.Add(new PasswordsBasketModel
                {
                    BasketPass = basketPass,
                    Description = description,
                    Name = name
                    
                });

                var userBasket = context.UserBasket.Add(new UserBasket
                {
                    BasketId = basket.Entity.Id,
                    UserId = currentUser.Id,
                    AccessTypeId = 1
                });
                context.SaveChanges();

                return userBasket.Entity.Id;
            }
        }

        public int CreateCredentials(User tgUser, AddCredsOption credsOption)
        {
            var basketName = credsOption.Basket ?? "default";
            using (var context = new SwDbContext(_appContext))
            {
                var user = context.User.FirstOrDefault(u => u.TgId == tgUser.Id);
                
                if (user == null)
                    throw new UserNotFoundException();

                var basket = context.UserBasket
                    .Include(x => x.BasketModel)
                    .FirstOrDefault(ub => ub.BasketModel.Name.ToUpper().Equals(basketName.ToUpper()) 
                                          && ub.UserModel.TgId == user.TgId
                                          && ub.AccessTypeId == 1);
                
                if (basket == null)
                    throw new BasketOperationNotAllowed();

                var res = context.Credentials.Add(new CredentialsModel
                {
                    UnitPassword = credsOption.Password,
                    Name = credsOption.Title, 
                    Login = credsOption.Login,
                    BasketPassId = basket.BasketId
                    
                });

                context.SaveChanges();
                
                return res.Entity.Id;
            }
        }

        public bool UnShareBasket(User user, ShareOptions shareOptions)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<CredentialsModel> GetCredentials(User user, ShowCredentialOptions credentialOptions)
        {
            using (var context = new SwDbContext(_appContext))
            {
                var res = context.Credentials
                    .Include(c => c.BasketModelPass)
                    .ThenInclude(b => b.UserBasket)
                    .ThenInclude(ub => ub.UserModel).ToList();
                
                if (credentialOptions.ShowAll)
                {
                    return res;

                }
                
                if (!string.IsNullOrEmpty(credentialOptions.Title))
                {
                    return res.Where(c => c.Name.ToUpper().Equals(credentialOptions.Title.ToUpper())).ToList();
                }

                if (!string.IsNullOrEmpty(credentialOptions.Basket))
                {
                    return res.Where(c => c.BasketModelPass.Name.ToUpper().Equals(credentialOptions.Basket.ToUpper())).ToList();
                }

                return res.Where(c => c.BasketModelPass.Name.Equals("default")).ToList();
                
                
            }
        }

        public ShareResult ShareBasket(User user, ShareOptions shareOptions)
        {
            throw new NotImplementedException();
        }
    }
}