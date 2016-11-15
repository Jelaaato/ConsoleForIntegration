//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;
//using System.Net;
//using TestClient.Model;
//using System.Net.Http;
//using System.Net.Http.Formatting;
//using System.Net.Http.Headers;
//using System.Data.SqlClient;


//namespace TestClient
//{
//    class SendPostRequestToGateway
//    {
//        #region Declarations

//        static HttpClient client = new HttpClient();
//        static CommentsEntities commentEntities = new CommentsEntities();

//        #endregion

//        #region Methods

//        Retrieve all data from on premise database
//        static IEnumerable<Comments> GetCommentsFromDb()
//        {
//            var comments = commentEntities.Comments.ToList();
//            return comments;
//        }

//        Send a post request to API Gateway
//        static async Task<Uri> CreateAnotherComment(CommentsModel comment)
//        {
//            HttpResponseMessage res = await client.PostAsJsonAsync("comments/", comment);
//            res.EnsureSuccessStatusCode();

//            return res.Headers.Location;
//        }


//        Database Listener for every insert made


//        #endregion

//        static async Task EndpointConnection()
//        {
//            client.BaseAddress = new Uri("https://attmmupb6f.execute-api.ap-northeast-1.amazonaws.com/PROD/");
//            client.DefaultRequestHeaders.Accept.Clear();
//            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

//            Insert to DynamoDB table : Comments
//            try
//            {
//                var comments = GetCommentsFromDb();

//                foreach (var c in comments)
//                {
//                    CommentsModel comment = new CommentsModel
//                    {
//                        commentId = c.commentId,
//                        userName = c.userName,
//                        message = c.message
//                    };

//                    await CreateAnotherComment(comment);
//                    Console.WriteLine(String.Format("Successfully inserted: {0}", comment.commentId));
//                }
//            }

//            catch (Exception e)
//            {
//                Console.WriteLine(e.Message);
//            }

//            Console.ReadLine();

//        }

//        static void Main(string[] args)
//        {
//            EndpointConnection().Wait();
//        }
//    }
//}
