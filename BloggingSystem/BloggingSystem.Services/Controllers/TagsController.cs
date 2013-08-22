using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Web.Http.ValueProviders;
using BloggingSystem.Data;
using BloggingSystem.Models;
using BloggingSystem.Services.Attributes;
using BloggingSystem.Services.Models;

namespace BloggingSystem.Services.Controllers
{
    public class TagsController : BaseApiController
    {
        [HttpGet]
        public IQueryable<TagFullModel> GetAll(
            [ValueProvider(typeof(HeaderValueProviderFactory<string>))]
            string sessionKey)
        {
            var responseMsg = this.PerformOperationAndHandleExceptions(() =>
            {
                var context = new BloggingSystemContext();
                var user = context.Users.FirstOrDefault(usr => usr.SessionKey == sessionKey);
                if (user == null)
                {
                    throw new InvalidOperationException("Invalid username or password");
                }
                var postEntities = context.Tags;
                var models =
                            from tag in postEntities
                            select new TagFullModel
                            {
                                Id = tag.Id,
                                Name = tag.Name,
                                Posts = tag.Posts.Count
                            };
                return models.OrderBy(thr => thr.Name);
            });

            return responseMsg;
        }

        [HttpGet]
        public IQueryable<PostModel> Posts(int tagId, [ValueProvider(typeof(HeaderValueProviderFactory<string>))]
                                           string sessionKey)
        {
            var responseMsg = this.PerformOperationAndHandleExceptions(() =>
            {
                var context = new BloggingSystemContext();
                Tag foundPost = context.Tags.Where(x => x.Id == tagId).FirstOrDefault();
                
                if (foundPost == null)
                {
                    throw new ArgumentException("No post was found");
                }
                
                var posts = from post in foundPost.Posts
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

                return posts.OrderByDescending(thr => thr.PostDate);
            });

            return responseMsg.AsQueryable<PostModel>();
        }
    }
}