using System;
using System.Collections.Generic;
using System.Net;
using System.Transactions;
using System.Web.Http;
using BloggingSystem.Services.Controllers;
using BloggingSystem.Services.Models;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;

namespace BloggingSystem.Services.Tests
{
    [TestClass]
    public class UsersControllerIntegrationTest
    {
        static TransactionScope tran;
        private InMemoryHttpServer httpServer;

        [TestInitialize]
        public void TestInit()
        {
            var type = typeof(UsersController);
            tran = new TransactionScope();

            var routes = new List<Route>
            {
                new Route(
                    "PostsApi",
                    "api/posts/{postId}/comment",
                    new
                {
                    controller = "posts",
                    action = "comment"
                }),
                new Route(
                    "TagsApi",
                    "api/tags/{tagId}/posts",
                    new
                {
                    controller = "tags",
                    action = "posts"
                }),
                new Route(
                    "UsertApi",
                    "api/users/{action}",
                    new { controller = "users" }),
                new Route(
                    "DefaultApi",
                    "api/{controller}/{id}",
                    new { id = RouteParameter.Optional }),
            };
            this.httpServer = new InMemoryHttpServer("http://localhost/", routes);
        }

        [TestCleanup]
        public void TearDown()
        {
            tran.Dispose();
        }

        [TestMethod]
        public void Register_WhenValid_ShouldReturnSessionKey()
        {
            UserModel model = new UserModel()
            {
                Username = "DonchoMinkov1",
                DisplayName = "Doncho Minkov1",
                AuthCode = "tfff2dd4f1b310eb0dbf593bd83f94dd8d34077e"
            };

            var response = httpServer.Post("api/users/register", model);

            Assert.AreEqual(HttpStatusCode.Created, response.StatusCode);
            Assert.IsNotNull(response.Content);
            var contentString = response.Content.ReadAsStringAsync().Result;
            var loggedModel = JsonConvert.DeserializeObject<UserLoggedModel>(contentString);

            Assert.AreEqual(model.DisplayName, loggedModel.DisplayName);
            Assert.IsNotNull(loggedModel.SessionKey);
        }

        [TestMethod]
        public void Logout_WhenValid_ShouldReturnStatusCodeOk()
        {
            UserModel model = new UserModel()
            {
                Username = "DonchoMinkov1",
                DisplayName = "Doncho Minkov1",
                AuthCode = "tfff2dd4f1b310eb0dbf593bd83f94dd8d34077e"
            };

            var responseRegister = httpServer.Post("api/users/register", model);

            var contentString = responseRegister.Content.ReadAsStringAsync().Result;
            var userModel = JsonConvert.DeserializeObject<UserLoggedModel>(contentString);

            var headers = new Dictionary<string, string>();
            headers["X-sessionKey"] = userModel.SessionKey;
            var response = httpServer.Put("api/users/logout", userModel, headers);

            Assert.AreEqual(response.StatusCode, HttpStatusCode.OK);
        }

        [TestMethod]
        public void Logout_InvalidSessionKey_ShouldReturnBadReques()
        {
            UserModel model = new UserModel()
            {
                Username = "DonchoMinkov1",
                DisplayName = "Doncho Minkov1",
                AuthCode = "tfff2dd4f1b310eb0dbf593bd83f94dd8d34077e"
            };

            var responseRegister = httpServer.Post("api/users/register", model);

            var contentString = responseRegister.Content.ReadAsStringAsync().Result;
            var userModel = JsonConvert.DeserializeObject<UserLoggedModel>(contentString);

            var headers = new Dictionary<string, string>();
            headers["X-sessionKey"] = userModel.SessionKey+1;
            var response = httpServer.Put("api/users/logout", userModel, headers);

            Assert.AreEqual(response.StatusCode, HttpStatusCode.BadRequest);
        }

        [TestMethod]
        public void Logout_WithNullSessionKey_ShouldReturnBadReques()
        {
            UserModel model = new UserModel()
            {
                Username = "DonchoMinkov1",
                DisplayName = "Doncho Minkov1",
                AuthCode = "tfff2dd4f1b310eb0dbf593bd83f94dd8d34077e"
            };

            var responseRegister = httpServer.Post("api/users/register", model);

            var contentString = responseRegister.Content.ReadAsStringAsync().Result;
            var userModel = JsonConvert.DeserializeObject<UserLoggedModel>(contentString);

            var headers = new Dictionary<string, string>();
            headers["X-sessionKey"] = null;
            var response = httpServer.Put("api/users/logout", userModel, headers);

            Assert.AreEqual(response.StatusCode, HttpStatusCode.BadRequest);
        }
        
        private UserLoggedModel RegisterTestUser(InMemoryHttpServer httpServer, UserModel testUser)
        {
            var response = httpServer.Post("api/users/register", testUser);
            var contentString = response.Content.ReadAsStringAsync().Result;
            var userModel = JsonConvert.DeserializeObject<UserLoggedModel>(contentString);

            return userModel;
        }

        [TestMethod]
        public void Register_WhenInvalidAuthCodeBiggerLength_ShouldReturnBadRequest()
        {
            UserModel model = new UserModel()
            {
                Username = "DonchoMinkov1",
                DisplayName = "Doncho Minkov1",
                AuthCode = "tfff2dd4f1b310eb0dbf593bd83f94dd8d34077ee"
            };

            var response = httpServer.Post("api/users/register", model);

            Assert.AreEqual(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [TestMethod]
        public void Register_WhenInvalidAuthCodeSmallerLength_ShouldReturnBadRequest()
        {
            UserModel model = new UserModel()
            {
                Username = "DonchoMinkov1",
                DisplayName = "Doncho Minkov1",
                AuthCode = "tfff2dd4f1b310eb0dbf593bd83f94dd8d34077"
            };

            var response = httpServer.Post("api/users/register", model);

            Assert.AreEqual(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [TestMethod]
        public void Register_TwoUsersWithSameName_ShouldReturnConflictRequest()
        {
            UserModel model = new UserModel()
            {
                Username = "DonchoMinkov1",
                DisplayName = "Doncho Minkov1",
                AuthCode = "tfff2dd4f1b310eb0dbf593bd83f94dd8d34077e"
            };

            var response = httpServer.Post("api/users/register", model);
            Assert.AreEqual(HttpStatusCode.Created, response.StatusCode);

            var response1 = httpServer.Post("api/users/register", model);
            Assert.AreEqual(HttpStatusCode.Conflict, response1.StatusCode);
        }

        [TestMethod]
        public void Register_WithInvalidCharacterInUsername_ShouldReturnConflictRequest()
        {
            UserModel model = new UserModel()
            {
                Username = "DonchoMinkov1@",
                DisplayName = "Doncho Minkov1",
                AuthCode = "tfff2dd4f1b310eb0dbf593bd83f94dd8d34077e"
            };

            var response = httpServer.Post("api/users/register", model);

            Assert.AreEqual(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [TestMethod]
        public void Register_WithInvalidCharacterInDisplayName_ShouldReturnBadRequest()
        {
            UserModel model = new UserModel()
            {
                Username = "DonchoMinkov1",
                DisplayName = "Doncho Minkov1@",
                AuthCode = "tfff2dd4f1b310eb0dbf593bd83f94dd8d34077e"
            };

            var response = httpServer.Post("api/users/register", model);

            Assert.AreEqual(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [TestMethod]
        public void Register_WithSmallerUsernameThen6_ShouldReturnBadRequest()
        {
            UserModel model = new UserModel()
            {
                Username = "Do",
                DisplayName = "Doncho Minkov1@",
                AuthCode = "tfff2dd4f1b310eb0dbf593bd83f94dd8d34077e"
            };

            var response = httpServer.Post("api/users/register", model);

            Assert.AreEqual(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [TestMethod]
        public void Register_WithSmallerDisplayNameThen6_ShouldReturnBadRequest()
        {
            UserModel model = new UserModel()
            {
                Username = "Dojdsalfkhsd",
                DisplayName = "Don",
                AuthCode = "tfff2dd4f1b310eb0dbf593bd83f94dd8d34077e"
            };

            var response = httpServer.Post("api/users/register", model);

            Assert.AreEqual(HttpStatusCode.BadRequest, response.StatusCode);
        }
        
        [TestMethod]
        public void Register_WithBiggerDisplayNameThen30_ShouldReturnBadRequest()
        {
            UserModel model = new UserModel()
            {
                Username = "Dojdsalfkhsd",
                DisplayName = "Donddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddd",
                AuthCode = "tfff2dd4f1b310eb0dbf593bd83f94dd8d34077e"
            };

            var response = httpServer.Post("api/users/register", model);

            Assert.AreEqual(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [TestMethod]
        public void Register_WithBiggerUsernameThen30_ShouldReturnBadRequest()
        {
            UserModel model = new UserModel()
            {
                Username = "DondddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddDojdsalfkhsd",
                DisplayName = "kasdjflkajsdfkjd",
                AuthCode = "tfff2dd4f1b310eb0dbf593bd83f94dd8d34077e"
            };

            var response = httpServer.Post("api/users/register", model);

            Assert.AreEqual(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [TestMethod]
        public void Register_WithNullAuthCode_ShouldReturnBadRequest()
        {
            UserModel model = new UserModel()
            {
                Username = "Dofdgsgdfg",
                DisplayName = "Doncho Minkov1",
                AuthCode = null
            };

            var response = httpServer.Post("api/users/register", model);

            Assert.AreEqual(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [TestMethod]
        public void Register_WithNullUsername_ShouldReturnBadRequest()
        {
            UserModel model = new UserModel()
            {
                Username = null,
                DisplayName = "Doncho Minkov1",
                AuthCode = "tfff2dd4f1b310eb0dbf593bd83f94dd8d34077e"
            };

            var response = httpServer.Post("api/users/register", model);

            Assert.AreEqual(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [TestMethod]
        public void Register_WithNullDisplayName_ShouldReturnBadRequest()
        {
            UserModel model = new UserModel()
            {
                Username = "Doncho Minkov1",
                DisplayName = null,
                AuthCode = "tfff2dd4f1b310eb0dbf593bd83f94dd8d34077e"
            };

            var response = httpServer.Post("api/users/register", model);

            Assert.AreEqual(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [TestMethod]
        public void Logout_WhenInvalidSessionKey_ShouldReturnBadRequest()
        {
            UserModel model = new UserModel()
            {
                Username = "DonchoMinkov1",
                DisplayName = "Doncho Minkov1",
                AuthCode = "tfff2dd4f1b310eb0dbf593bd83f94dd8d34077e"
            };

            var responseRegister = httpServer.Post("api/users/register", model);

            var contentString = responseRegister.Content.ReadAsStringAsync().Result;
            var userModel = JsonConvert.DeserializeObject<UserLoggedModel>(contentString);

            var headers = new Dictionary<string, string>();
            headers["X-sessionKey"] = "adfasdf";
            var response = httpServer.Put("api/users/logout", userModel, headers);

            Assert.AreEqual(response.StatusCode, HttpStatusCode.BadRequest);
            Assert.IsNotNull(response.Content);
        }
    }
}