using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Reta.Controllers.Helpers;
using Reta.Models;
using Reta;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;

namespace UnitTestReta
{
    [TestClass]
    public class UserServiceTest
    {
        [TestMethod]
        public void TestCodeGeneration()
        {
            ApplicationUser user = UserService.getUserByEmail("123456789AZERTY@gmail.com");
            if (user != null)
                UserService.deleteUser(user);
            user = new ApplicationUser();
            user.UserName = "123456789AZERTY@gmail.com";
            user.Email = "123456789AZERTY@gmail.com";
            user.TwoFactorEnabled = true;
            if (UserService.createUser(user))
            {
                user = UserService.getUserByEmail("123456789AZERTY@gmail.com");
                var code = UserService.generateCodeForAutoLogin(user);
                var result = UserService.verifyCodeForAutoLogin(code);
                Assert.IsTrue(result);
                var userFound = UserService.getUserFromCode(code);
                Assert.IsNotNull(userFound);
                UserService.deleteUser(user);
            }
        }
    }
}
