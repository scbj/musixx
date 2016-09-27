using Google.Apis.Auth.OAuth2;
using Google.Apis.Auth.OAuth2.Responses;
using Google.Apis.Drive.v2;
using Google.Apis.Oauth2.v2;
using Google.Apis.Services;
using Musixx.Models;
using Musixx.Shared.Models;
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
                await credential.RefreshTokenAsync(CancellationToken.None);
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
        public async Task<IEnumerable<MusicHOLD>> GetMusics()
        {
            FilesResource.ListRequest listRequest = DriveService.Files.List();
            listRequest.MaxResults = 1000;
            listRequest.Fields = "items(downloadUrl,fileExtension,fileSize,id,title)";
            listRequest.Q = "mimeType='audio/mp3' and trashed=false";

            var fileList = await listRequest.ExecuteAsync();

            var baseUrl = "https://googledrive.com/host/";

            return fileList.Items.Select(f => new MusicHOLD(f.Title, baseUrl + f.Id + "?access_token=" + accessToken,
                f.FileSize.HasValue ? f.FileSize.Value : 0));
        }

        public async Task<List<IMusic>> GetMusic()
        {
            FilesResource.ListRequest listRequest = DriveService.Files.List();
            listRequest.MaxResults = 1000;
            listRequest.Fields = "items(downloadUrl,fileExtension,fileSize,id,md5Checksum,title)";
            listRequest.Q = "mimeType='audio/mp3' and trashed=false";

            var fileList = await listRequest.ExecuteAsync();

            const string baseUrl = "https://googledrive.com/host/";

            var songs = new List<IMusic>();
            foreach(var f in fileList.Items)
            {
                string path = baseUrl + f.Id + "?access_token=" + accessToken;
                long size = f.FileSize.HasValue ? f.FileSize.Value : 0;
                var file = new File(f.Id, f.Title, f.Md5Checksum, size, path);
                songs.Add(new Song(file));
            }

            return songs;
        }
    }
}
