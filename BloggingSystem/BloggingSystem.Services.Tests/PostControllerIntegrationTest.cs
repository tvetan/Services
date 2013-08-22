using System;
using System.Collections.Generic;
using System.Net;
using System.Transactions;
using System.Web.Http;
using BloggingSystem.Models;
using BloggingSystem.Services.Controllers;
using BloggingSystem.Services.Models;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;

namespace BloggingSystem.Services.Tests
{
    [TestClass]
    public class PostControllerIntegrationTest
    {
        static TransactionScope tran;
        private InMemoryHttpServer httpServer;

        [TestInitialize]
        public void TestInit()
        {
            var type = typeof(PostsController);
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
        public void Create_WhenValid_ShouldReturnStatusCodeCreated()
        {
            PostModel model = new PostModel()
            {
                Text = "some text",
                Title = "my post",
            };
            UserModel user = new UserModel()
            {
                Username = "DonchoMinkov1",
                DisplayName = "Doncho Minkov1",
                AuthCode = "tfff2dd4f1b310eb0dbf593bd83f94dd8d34077e"
            };

            var responseRegister = httpServer.Post("api/users/register", user);

            var contentString = responseRegister.Content.ReadAsStringAsync().Result;
            var userModel = JsonConvert.DeserializeObject<UserLoggedModel>(contentString);

            var headers = new Dictionary<string, string>();
            headers["X-sessionKey"] = userModel.SessionKey;
            var postResponse = httpServer.Post("api/posts", model, headers);

            Assert.IsNotNull(postResponse.Content);
            Assert.AreEqual(HttpStatusCode.Created, postResponse.StatusCode);
        }

        [TestMethod]
        public void Create_WhenValid_ShouldReturnPostWithValidId()
        {
            PostModel model = new PostModel()
            {
                Text = "some text",
                Title = "my post",
            };
            UserModel user = new UserModel()
            {
                Username = "DonchoMinkov1",
                DisplayName = "Doncho Minkov1",
                AuthCode = "tfff2dd4f1b310eb0dbf593bd83f94dd8d34077e"
            };

            var responseRegister = httpServer.Post("api/users/register", user);

            var contentString = responseRegister.Content.ReadAsStringAsync().Result;
            var userModel = JsonConvert.DeserializeObject<UserLoggedModel>(contentString);

            var headers = new Dictionary<string, string>();
            headers["X-sessionKey"] = userModel.SessionKey;

            var postResponse = httpServer.Post("api/posts", model, headers);
            var createdPostString = postResponse.Content.ReadAsStringAsync().Result;
            var postModel = JsonConvert.DeserializeObject<PostModel>(createdPostString);

            Assert.IsTrue(postModel.Id > 0);
        }

        [TestMethod]
        public void Create_WhenValid_ShouldReturnPostWithValidTitle()
        {
            PostModel model = new PostModel()
            {
                Text = "some text",
                Title = "my post",
            };
            UserModel user = new UserModel()
            {
                Username = "DonchoMinkov1",
                DisplayName = "Doncho Minkov1",
                AuthCode = "tfff2dd4f1b310eb0dbf593bd83f94dd8d34077e"
            };

            var responseRegister = httpServer.Post("api/users/register", user);
            
            var contentString = responseRegister.Content.ReadAsStringAsync().Result;
            var userModel = JsonConvert.DeserializeObject<UserLoggedModel>(contentString);

            var headers = new Dictionary<string, string>();
            headers["X-sessionKey"] = userModel.SessionKey;

            var postResponse = httpServer.Post("api/posts", model, headers);
            var createdPostString = postResponse.Content.ReadAsStringAsync().Result;
            var postModel = JsonConvert.DeserializeObject<PostModel>(createdPostString);

            Assert.AreEqual(model.Title, postModel.Title);
        }

        [TestMethod]
        public void Create_WhenValid_ShouldReturnPostWithValidTags()
        { 
            PostModel model = new PostModel()
            {
                Text = "some text",
                Title = "my post",
            };
            
            UserModel user = new UserModel()
            {
                Username = "DonchoMinkov1",
                DisplayName = "Doncho Minkov1",
                AuthCode = "tfff2dd4f1b310eb0dbf593bd83f94dd8d34077e"
            };

            var responseRegister = httpServer.Post("api/users/register", user);

            var contentString = responseRegister.Content.ReadAsStringAsync().Result;
            var userModel = JsonConvert.DeserializeObject<UserLoggedModel>(contentString);

            var headers = new Dictionary<string, string>();
            headers["X-sessionKey"] = userModel.SessionKey;

            var postResponse = httpServer.Post("api/posts", model, headers);
            var createdPostString = postResponse.Content.ReadAsStringAsync().Result;
            var postModel = JsonConvert.DeserializeObject<PostModel>(createdPostString);

            Assert.AreEqual(postModel.Tags, null);
        }

        [TestMethod]
        public void Create_WithInvalidSessionKey_ShouldReturnConflict()
        {
            PostModel model = new PostModel()
            {
                Text = "some text",
                Title = "my post",
            };

            UserModel user = new UserModel()
            {
                Username = "DonchoMinkov1",
                DisplayName = "Doncho Minkov1",
                AuthCode = "tfff2dd4f1b310eb0dbf593bd83f94dd8d34077e"
            };

            var responseRegister = httpServer.Post("api/users/register", user);

            var contentString = responseRegister.Content.ReadAsStringAsync().Result;
            var userModel = JsonConvert.DeserializeObject<UserLoggedModel>(contentString);

            var headers = new Dictionary<string, string>();
            headers["X-sessionKey"] = userModel.SessionKey+1;

            var postResponse = httpServer.Post("api/posts", model, headers);
           

            Assert.AreEqual(HttpStatusCode.Conflict, postResponse.StatusCode);
        }

        [TestMethod]
        public void Create_WithNullSessionKey_ShouldReturnConflict()
        {
            PostModel model = new PostModel()
            {
                Text = "some text",
                Title = "my post",
            };

            UserModel user = new UserModel()
            {
                Username = "DonchoMinkov1",
                DisplayName = "Doncho Minkov1",
                AuthCode = "tfff2dd4f1b310eb0dbf593bd83f94dd8d34077e"
            };

            var responseRegister = httpServer.Post("api/users/register", user);

            var contentString = responseRegister.Content.ReadAsStringAsync().Result;
            var userModel = JsonConvert.DeserializeObject<UserLoggedModel>(contentString);

            var headers = new Dictionary<string, string>();
            headers["X-sessionKey"] = null;

            var postResponse = httpServer.Post("api/posts", model, headers);


            Assert.AreEqual(HttpStatusCode.Conflict, postResponse.StatusCode);
        }

        [TestMethod]
        public void LeaveAComment_Whenvalid_ShouldreturnStatusCodeOK()
        {
            PostModel model = new PostModel()
            {
                Text = "some text",
                Title = "my post",
            };
            UserModel user = new UserModel()
            {
                Username = "DonchoMinkov1",
                DisplayName = "Doncho Minkov1",
                AuthCode = "tfff2dd4f1b310eb0dbf593bd83f94dd8d34077e"
            };

            var responseRegister = httpServer.Post("api/users/register", user);

            var contentString = responseRegister.Content.ReadAsStringAsync().Result;
            var userModel = JsonConvert.DeserializeObject<UserLoggedModel>(contentString);

            var headers = new Dictionary<string, string>();
            headers["X-sessionKey"] = userModel.SessionKey;
            var postResponse = httpServer.Post("api/posts", model, headers);

            var contentStringPost = postResponse.Content.ReadAsStringAsync().Result;
            var postModel = JsonConvert.DeserializeObject<PostModel>(contentStringPost);

            CommentModel commentModel = new CommentModel() { Text = "some comment" };
            
            var commentResponse = httpServer.Put("api/posts/" + postModel.Id + "/comment", commentModel, headers);

            Assert.IsNotNull(commentResponse.Content);
            Assert.AreEqual(HttpStatusCode.OK, commentResponse.StatusCode);
        }

        [TestMethod]
        public void LeaveAComment_Null_ShouldreturnBadRequest()
        {
            PostModel model = new PostModel()
            {
                Text = "some text",
                Title = "my post",
            };
            UserModel user = new UserModel()
            {
                Username = "DonchoMinkov1",
                DisplayName = "Doncho Minkov1",
                AuthCode = "tfff2dd4f1b310eb0dbf593bd83f94dd8d34077e"
            };

            var responseRegister = httpServer.Post("api/users/register", user);

            var contentString = responseRegister.Content.ReadAsStringAsync().Result;
            var userModel = JsonConvert.DeserializeObject<UserLoggedModel>(contentString);

            var headers = new Dictionary<string, string>();
            headers["X-sessionKey"] = userModel.SessionKey;
            var postResponse = httpServer.Post("api/posts", model, headers);

            var contentStringPost = postResponse.Content.ReadAsStringAsync().Result;
            var postModel = JsonConvert.DeserializeObject<PostModel>(contentStringPost);

            CommentModel commentModel = null;
            
            var commentResponse = httpServer.Put("api/posts/" + postModel.Id + "/comment", commentModel, headers);

            Assert.IsNotNull(commentResponse.Content);
            Assert.AreEqual(HttpStatusCode.BadRequest, commentResponse.StatusCode);
        }

        [TestMethod]
        public void LeaveAComment_InvalidPostId_ShouldreturnBadRequest()
        {
            PostModel model = new PostModel()
            {
                Text = "some text",
                Title = "my post",
            };
            UserModel user = new UserModel()
            {
                Username = "DonchoMinkov1",
                DisplayName = "Doncho Minkov1",
                AuthCode = "tfff2dd4f1b310eb0dbf593bd83f94dd8d34077e"
            };

            var responseRegister = httpServer.Post("api/users/register", user);

            var contentString = responseRegister.Content.ReadAsStringAsync().Result;
            var userModel = JsonConvert.DeserializeObject<UserLoggedModel>(contentString);

            var headers = new Dictionary<string, string>();
            headers["X-sessionKey"] = userModel.SessionKey;
            var postResponse = httpServer.Post("api/posts", model, headers);

            var contentStringPost = postResponse.Content.ReadAsStringAsync().Result;
            
            CommentModel commentModel = null;

            var commentResponse = httpServer.Put("api/posts/" + int.MaxValue + "/comment", commentModel, headers);

            Assert.IsNotNull(commentResponse.Content);
            Assert.AreEqual(HttpStatusCode.BadRequest, commentResponse.StatusCode);
        }

        [TestMethod]
        public void LeaveAComment_WithEmptyText_ShouldreturnBadRequest()
        {
            PostModel model = new PostModel()
            {
                Text = "some text",
                Title = "my post",
            };
            UserModel user = new UserModel()
            {
                Username = "DonchoMinkov1",
                DisplayName = "Doncho Minkov1",
                AuthCode = "tfff2dd4f1b310eb0dbf593bd83f94dd8d34077e"
            };

            var responseRegister = httpServer.Post("api/users/register", user);

            var contentString = responseRegister.Content.ReadAsStringAsync().Result;
            var userModel = JsonConvert.DeserializeObject<UserLoggedModel>(contentString);

            var headers = new Dictionary<string, string>();
            headers["X-sessionKey"] = userModel.SessionKey;
            var postResponse = httpServer.Post("api/posts", model, headers);

            var contentStringPost = postResponse.Content.ReadAsStringAsync().Result;
            var postModel = JsonConvert.DeserializeObject<PostModel>(contentStringPost);
            CommentModel commentModel = new CommentModel() { Text = "" };

            var commentResponse = httpServer.Put("api/posts/" + postModel.Id + "/comment", commentModel, headers);

            Assert.IsNotNull(commentResponse.Content);
            Assert.AreEqual(HttpStatusCode.BadRequest, commentResponse.StatusCode);
        }

        [TestMethod]
        public void GetByTags_WhenInValid_ShouldreturnStatusCodeNotFound()
        {
            UserModel user = new UserModel()
            {
                Username = "DonchoMinkov1",
                DisplayName = "Doncho Minkov1",
                AuthCode = "tfff2dd4f1b310eb0dbf593bd83f94dd8d34077e"
            };

            var responseRegister = httpServer.Post("api/users/register", user);

            var contentString = responseRegister.Content.ReadAsStringAsync().Result;
            var userModel = JsonConvert.DeserializeObject<UserLoggedModel>(contentString);

            var headers = new Dictionary<string, string>();
            headers["X-sessionKey"] = userModel.SessionKey;
            var commentResponse = httpServer.Get("api/posts?tags=testinvalid", headers);

            Assert.IsNotNull(commentResponse.Content);
            Assert.AreEqual(HttpStatusCode.NotFound, commentResponse.StatusCode);
        }

        [TestMethod]
        public void GetByTags_Whenvalid_ShouldreturnStatusCodeOk()
        {
            PostModel model = new PostModel()
            {
                Text = "some text",
                Title = "my testvalid",
            };

            UserModel user = new UserModel()
            {
                Username = "DonchoMinkov1",
                DisplayName = "Doncho Minkov1",
                AuthCode = "tfff2dd4f1b310eb0dbf593bd83f94dd8d34077e"
            };

            var responseRegister = httpServer.Post("api/users/register", user);

            var contentString = responseRegister.Content.ReadAsStringAsync().Result;
            var userModel = JsonConvert.DeserializeObject<UserLoggedModel>(contentString);

            var headers = new Dictionary<string, string>();
            headers["X-sessionKey"] = userModel.SessionKey;
            var postResponse = httpServer.Post("api/posts", model, headers);
                      
            var commentResponse = httpServer.Get("api/posts?tags=testvalid", headers);

            Assert.IsNotNull(commentResponse.Content);
            Assert.AreEqual(HttpStatusCode.OK, commentResponse.StatusCode);
        }

        [TestMethod]
        public void GetByTags_Whenvalid_ShouldreturnPostCountOne()
        {
            PostModel model = new PostModel()
            {
                Text = "some text",
                Title = "my testvalid",
            };

            UserModel user = new UserModel()
            {
                Username = "DonchoMinkov1",
                DisplayName = "Doncho Minkov1",
                AuthCode = "tfff2dd4f1b310eb0dbf593bd83f94dd8d34077e"
            };

            var responseRegister = httpServer.Post("api/users/register", user);

            var contentString = responseRegister.Content.ReadAsStringAsync().Result;
            var userModel = JsonConvert.DeserializeObject<UserLoggedModel>(contentString);

            var headers = new Dictionary<string, string>();
            headers["X-sessionKey"] = userModel.SessionKey;
            var postResponse = httpServer.Post("api/posts", model, headers);

            var foundPostsResponse = httpServer.Get("api/posts?tags=testvalid", headers);

            var postsAsString = foundPostsResponse.Content.ReadAsStringAsync().Result;
            var postsList = JsonConvert.DeserializeObject<List<PostModel>>(postsAsString);

            Assert.AreEqual(1, postsList.Count);         
        }
    }
}