using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Http;
using System.Net.Http.Formatting;
using System.Net.Http.Headers;
using System.Data.SqlClient;
using TableDependency.SqlClient;
using TableDependency;
using TableDependency.EventArgs;
using TableDependency.Enums;
using TestClient.Model;
using System.Data.Linq;
using System.Data;
using System.Globalization;

namespace TestClient.BusinessLogic
{
    class DatabaseListener
    {
        #region Declarations

        static RequestsEntities req = new RequestsEntities();
        static SPEntities sp = new SPEntities();
        static SqlCommand cmd;
        static HttpClient client = new HttpClient();
        static int affectedRows;


        #endregion

        #region Methods

        static void dependency_OnChanged(object sender, RecordChangedEventArgs<Request> e)
        {


            Console.WriteLine(Environment.NewLine);

            if (e.ChangeType == ChangeType.Insert)
            {
                var changedEntity = e.Entity;

                var hn = changedEntity.hospital_number;
                var r_id = changedEntity.request_id;
                var dob = changedEntity.date_of_birth.ToString("yyyy-MM-dd");


                Console.WriteLine(@"HN:" + hn);
                Console.WriteLine(@"Date_of_birth:" + dob);
                Console.WriteLine(@"Request_DateTime:" + changedEntity.request_datetime);

                using (SqlConnection sp_con = new SqlConnection(sp.Database.Connection.ConnectionString))
                {
                    try
                    {
                        if (isValidated(hn, dob))
                        {
                            UpdateRequestModel model = new UpdateRequestModel
                            {
                                request_id = r_id,
                                isValidated = true
                            };

                            Usermodel user = new Usermodel
                            {
                                Username = hn,
                                Password = dob
                            };

                            using (cmd = new SqlCommand("DumpSummaryToAWS", sp_con))
                            {
                                cmd.CommandType = CommandType.StoredProcedure;
                                cmd.Parameters.Add("@HN", SqlDbType.VarChar).Value = hn;
                                sp_con.Open();

                                ExecuteCreate(user).Wait();
                                affectedRows = cmd.ExecuteNonQuery();
                                ExecuteUpdate(model).Wait();
                                
                                
                                Console.WriteLine("Rows Affected: " + affectedRows);
                                Console.WriteLine("Dumping of Patient Summary Successful");
                            }
                        }
                        else
                        {
                            ExecuteDelete(r_id).Wait();
                            Console.WriteLine("User not validated; Request Deleted.");
                        }
                    }

                    catch (Exception ex)
                    {
                        Console.WriteLine(ex.Message);
                    }
                }

            }
        }

        static async Task ExecuteCreate(Usermodel user)
        {
            await CreateUser(user);
        }

        static async Task ExecuteUpdate(UpdateRequestModel model)
        {
            await UpdateRequest(model);
        }

        static async Task ExecuteDelete(Guid request_id)
        {
            await DeleteRequest(request_id);
        }

        static async Task CreateUser(Usermodel user)
        {
            try
            {
                HttpResponseMessage res = await client.PostAsJsonAsync("Account/Register", user);
                res.EnsureSuccessStatusCode();

                Console.WriteLine(res.Headers.Location);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
        }

        static async Task DeleteRequest(Guid request_id)
        {
            try
            {
                var uri = String.Format("DeleteRequest/{0}", request_id);
                HttpResponseMessage res = await client.DeleteAsync(uri);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
        }

        static async Task UpdateRequest(UpdateRequestModel model)
        {
            try
            {
                var uri = String.Format("UpdateRequest/{0}", model.request_id);
                HttpResponseMessage update_req = await client.PutAsJsonAsync(uri, model);
                Console.WriteLine(update_req.Headers.Location);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
        }

        static void dependency_OnError(object sender, ErrorEventArgs e)
        {
            Console.WriteLine(e.Error.Message);
        }

        static bool isValidated(string hn, string dob)
        {
            using (SqlConnection sp_con = new SqlConnection(sp.Database.Connection.ConnectionString))
            {
                using (cmd = new SqlCommand("ValidateUser", sp_con))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Add("@HN", SqlDbType.VarChar).Value = hn;
                    cmd.Parameters.Add("@DOB", SqlDbType.Date).Value = dob;

                    var user = cmd.Parameters.Add("@Valid", SqlDbType.Int);
                    user.Direction = ParameterDirection.ReturnValue;

                    sp_con.Open();
                    cmd.ExecuteNonQuery();
                    var result = user.Value;

                    return Convert.ToBoolean(result);
                }
            }
        }

        #endregion

        static void Main(string[] args)
        {
            client.BaseAddress = new Uri("http://52.77.5.78/API/");
            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            using (var dependency = new SqlTableDependency<Request>(req.Database.Connection.ConnectionString, "Requests"))
            {
                try
                {
                    dependency.OnChanged += dependency_OnChanged;
                    dependency.OnError += dependency_OnError;
                    dependency.Start();

                    Console.WriteLine("Listener has started ...");

                    Console.ReadLine();
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.Message);
                    Console.ReadLine();
                }
            }
        }
    }
}
