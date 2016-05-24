using Google.Apis.Auth.OAuth2;
using Google.Apis.Auth.OAuth2.Responses;
using Google.Apis.Drive.v3;
using Google.Apis.Oauth2.v2;
using Google.Apis.Services;
using Musixx.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Musixx.Clouds
{
    public class GoogleDrive
    {
        private string accessToken;
        UserCredential credential;

        static string[] Scopes = {
            DriveService.Scope.Drive,
            Oauth2Service.Scope.UserinfoProfile,
            Oauth2Service.Scope.UserinfoEmail
        };
        static string ApplicationName = "Musixx";

        public GoogleDrive()
        {

        }

        private Oauth2Service Oauth2Service { get; set; }
        private DriveService DriveService { get; set; }

        public async Task<bool> Login()
        {
            var clientSecretUri = new Uri("ms-appx:///Resources/client_secret.json", UriKind.Absolute);
            try
            {
                credential = await GoogleWebAuthorizationBroker.AuthorizeAsync(clientSecretUri, Scopes, "user", CancellationToken.None);
                bool refreshSuccess = await credential.RefreshTokenAsync(CancellationToken.None);
            }
            catch (TokenResponseException e)
            {
                await credential.GetAccessTokenForRequestAsync();
            }
            catch (Exception e)
            {
                return false;
            }

            var initializer = new BaseClientService.Initializer()
            {
                HttpClientInitializer = credential,
                ApplicationName = ApplicationName
            };
            accessToken = credential.Token.AccessToken;

            Oauth2Service = new Oauth2Service(initializer);
            DriveService = new DriveService(initializer);

            return true;
        }


        public Task<bool> Logout() => credential.RevokeTokenAsync(CancellationToken.None);

        public async Task<User> GetUser()
        {
            var userInfoPlus = await Oauth2Service.Userinfo.Get().ExecuteAsync();
            return new User(userInfoPlus.Name, userInfoPlus.Picture);
        }
        public async Task<List<Music>> GetMusics()
        {
            FilesResource.ListRequest listRequest = DriveService.Files.List();
            listRequest.PageSize = 20;
            listRequest.Fields = "nextPageToken, files(id, name, fileExtension)";
            listRequest.Q = "mimeType = 'audio/mp3'";

            var fileList = await listRequest.ExecuteAsync();

            return fileList.Files.Select(f => new Music(f.Name.Replace("." + f.FileExtension, ""),
                "https://www.googleapis.com/drive/v3/files/" + f.Id + "?alt=media&access_token=" + accessToken)).ToList();
        }
    }
}
