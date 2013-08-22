using BloggingSystem.Data;
using BloggingSystem.Models;
using BloggingSystem.Services.Attributes;
using BloggingSystem.Services.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Web.Http.ValueProviders;

namespace BloggingSystem.Services.Controllers
{
    // pri staniciraneto trqbva da pokazvame vseki post ima tagove pri listvaneto
    public class PostsController : BaseApiController
    {
        [HttpPost]
        public HttpResponseMessage CreatePost([FromBody]
                                              PostModel model,
            [ValueProvider(typeof(HeaderValueProviderFactory<string>))]
            string sessionKey)
        {
            var responseMsg = this.PerformOperationAndHandleExceptions(() =>
            {
                var context = new BloggingSystemContext();

                var user = context.Users
                                  .Where(usr => usr.SessionKey == sessionKey)
                                  .FirstOrDefault();

                if (user == null)
                {
                    throw new InvalidOperationException("The user is not logged in");
                }
            
                var newPost = new Post()
                {
                    Title = model.Title,
                    Text = model.Text,
                    PostDate = DateTime.Now,
                    User = user,
                };

                var titleSplited = model.Title.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

                foreach (var tagName in titleSplited)
                {
                    Tag addedTag = new Tag()
                    {
                        Name = tagName.ToLower()
                    };
                    
                    context.Tags.Add(addedTag);
                    newPost.Tags.Add(addedTag);
                }

                if (model.Tags != null)
                {
                    foreach (var post in model.Tags)
                    {
                        Tag addedTag = new Tag()
                        {
                            Name = post.ToLower()
                        };
                        
                        context.Tags.Add(addedTag);
                        newPost.Tags.Add(addedTag);
                    }
                }

                // var filteredTags = FilterTags(possibleTags, context);

                context.Posts.Add(newPost);
                context.SaveChanges();

                var response = this.Request.CreateResponse(HttpStatusCode.Created,
                    new PostCreatedModel
                    {
                        Id = newPost.Id,
                        Title = model.Title
                    });

                return response;
            });

            return responseMsg;
        }

        [HttpGet]
        public IQueryable<PostModel> GetAll(
            [ValueProvider(typeof(HeaderValueProviderFactory<string>))]
            string sessionKey)
        {
            var responseMsg = this.PerformOperationAndHandleExceptions(() =>
            {
                var context = new BloggingSystemContext();

                var user = context.Users.FirstOrDefault(usr => usr.SessionKey == sessionKey);
                if (user == null)
                {
                    throw new InvalidOperationException("The user is not logged in");
                }

                var postEntities = context.Posts;

                var models =
                            from post in postEntities
                            select new PostModel
                            {
                                Id = post.Id,
                                PostedBy = post.User.DisplayName,
                                PostDate = post.PostDate,
                                Text = post.Text,
                                Comments = (from comment in post.Comments
                                            select new CommentModel()
                                            {
                                                CommentedBy = comment.User.DisplayName,
                                                PostDate = comment.CommentDate,
                                                Text = comment.Text
                                            }),
                                Tags = from tag in post.Tags
                                       select tag.Name,
                                Title = post.Title
                            };

                return models.OrderByDescending(thr => thr.PostDate);
            });

            return responseMsg;
        }

        [HttpGet]
        public IQueryable<PostModel> GetPage(int page, int count, [ValueProvider(typeof(HeaderValueProviderFactory<string>))]
                                             string sessionKey)
        {
            var models = this.GetAll(sessionKey)
                             .Skip(page * count)
                             .Take(count);

            return models.OrderByDescending(thr => thr.PostDate);
        }

        [HttpGet]
        public IQueryable<PostModel> GetByKeyword(string keyword, [ValueProvider(typeof(HeaderValueProviderFactory<string>))]
                                                  string sessionKey)
        {
            var models = this.GetAll(sessionKey);
            List<PostModel> foundPosts = new List<PostModel>();

            foreach (var post in models)
            {
                if (post.Title != null && post.Title.ToLower().Contains(keyword.ToLower()))
                {
                    foundPosts.Add(post);
                }
            }

            if (models.Count() == 0)
            {
                var errResponse = this.Request.CreateErrorResponse(HttpStatusCode.NotFound, "No matches were found");
                throw new HttpResponseException(errResponse);
            }

            return foundPosts.OrderByDescending(thr => thr.PostDate).AsQueryable();
        }

        [HttpGet]
        public IQueryable<PostModel> GetByTags(string tags, [ValueProvider(typeof(HeaderValueProviderFactory<string>))]
                                               string sessionKey)
        {
            var models = this.GetAll(sessionKey);

            var tagsList = tags.Split(',');          
            var foundPosts = models.Where(x => tagsList.All(tag => x.Tags.Contains(tag)));

            if (foundPosts.Count() == 0)
            {
                var errResponse = this.Request.CreateErrorResponse(HttpStatusCode.NotFound, "No matches were found");
                throw new HttpResponseException(errResponse);
            }

            return foundPosts.OrderByDescending(thr => thr.PostDate);
        }

        [HttpPut]
        public HttpResponseMessage Comment(int postId, [FromBody]
                                           CommentModel model, [ValueProvider(typeof(HeaderValueProviderFactory<string>))]
                                           string sessionKey)
        {
            var responseMsg = this.PerformOperationAndHandleExceptions(() =>
            {
                var context = new BloggingSystemContext();
                Post foundPost = context.Posts.Where(x => x.Id == postId).FirstOrDefault();
                User foundUser = context.Users.Where(x => x.Id == foundPost.User.Id).FirstOrDefault();
                if (foundPost == null)
                {
                    throw new ArgumentException("No post was found");
                }

                if (foundUser == null)
                {
                    throw new ArgumentException("No user was found");
                }

                Comment newComment = new Comment()
                {
                    Text = model.Text,
                    CommentDate = DateTime.Now
                };
                foundUser.Comments.Add(newComment);
                foundPost.Comments.Add(newComment);
                context.SaveChanges();

                string result = null;
                return Request.CreateResponse(HttpStatusCode.OK, result);
            });

            return responseMsg;
        }
    }
}