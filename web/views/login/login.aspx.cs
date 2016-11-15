﻿using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using Helpers;
using Models;
using Newtonsoft.Json;
using RestSharp;
using Security;

namespace views.login
{
    public partial class Default : Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            Session.RemoveAll();

            if (Request.QueryString["session"] == "expired")
                SessionExpired.Visible = true;

            if (Settings.ForceSsL == "Yes")
            {
                if (!HttpContext.Current.Request.IsSecureConnection)
                {
                    var root = Request.Url.GetLeftPart(UriPartial.Authority);
                    root = root + Page.ResolveUrl("~/");
                    root = root.Replace("http://", "https://");
                    Response.Redirect(root);
                }
            }

            if (Request.IsAuthenticated)
            {
                Response.Redirect("~/views/dashboard/dash.aspx");
            }
        }

        public Models.Token GetToken(string userName, string password)
        {
            var client = new RestClient("http://localhost/clonedeploy/");
            var request = new RestRequest("Token", Method.POST);
          
            request.AddParameter("grant_type", "password");
            request.AddParameter("userName", userName);
            request.AddParameter("password", password);
           

            var response = client.Execute<Models.Token>(request);
            return response.Data;
            /*var pairs = new List<KeyValuePair<string, string>>
                        {
                            new KeyValuePair<string, string>( "grant_type", "password" ), 
                            new KeyValuePair<string, string>( "userName", userName ), 
                            new KeyValuePair<string, string> ( "password", password )
                        };
            var content = new FormUrlEncodedContent(pairs);
            using (var client = new HttpClient())
            {
                var response =
                    client.PostAsync("http://localhost/clonedeploy/Token", content).Result;
                return response.Content.ReadAsStringAsync().Result;
            }*/
        }
        protected void CrucibleLogin_Authenticate(object sender, AuthenticateEventArgs e)
        {
            var token = GetToken(CrucibleLogin.UserName, CrucibleLogin.Password);
          
            System.Web.HttpContext.Current.Response.Cookies.Add(new System.Web.HttpCookie("Token")
            {
                Value = token.access_token,
                HttpOnly = true
            });
            var auth = new Authenticate();

            var validationResult = auth.GlobalLogin(CrucibleLogin.UserName, CrucibleLogin.Password, "Web");
            if ((validationResult.Success))
            {
                var cloneDeployUser = BLL.User.GetUser(CrucibleLogin.UserName);
                cloneDeployUser.Salt = "";
                cloneDeployUser.Password = "";

                Session["CloneDeployUser"] = cloneDeployUser;
                e.Authenticated = true; 
            }
            else
            {
                e.Authenticated = false;
                lblError.Text = validationResult.Message;
                lblError.Visible = true;
            }
        }

       
    }
}