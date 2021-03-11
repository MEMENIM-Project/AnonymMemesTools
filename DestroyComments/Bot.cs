using System;
using System.Threading.Tasks;
using DestroyComments.Utils;
using Memenim.Core.Api;
using Memenim.Core.Schema;

namespace DestroyComments
{
    public class Bot
    {
        private ProfileSchema Data { get; set; }

        public string Token { get; private set; }
        public int Id { get; private set; }
        public string Login { get; private set; }
        public string Password { get; private set; }
        public string Nickname { get; set; }

        public bool Initialized { get; private set; }

        public Bot()
        {
            Token = null;
            Id = -1;
            Login = null;
            Password = null;
            Nickname = null;
        }

        public Task Initialize()
        {
            string login = RandomUtils.GenerateString(10, 20);

            return Initialize(login);
        }
        public Task Initialize(string login)
        {
            return Initialize(login, login);
        }
        public Task Initialize(string login, string nickname)
        {
            string password = RandomUtils.GenerateString(20, 30);

            return Initialize(login, password, nickname);
        }
        public async Task Initialize(string login, string password,
            string nickname, bool register = true)
        {
            if (string.IsNullOrEmpty(login)
                || string.IsNullOrEmpty(password))
            {
                Initialized = false;
                return;
            }

            if (register && string.IsNullOrEmpty(nickname))
            {
                nickname = login;
            }

            try
            {
                ApiResponse<AuthSchema> result;

                if (register)
                {
                    result = await UserApi.Register(login, password, nickname)
                        .ConfigureAwait(false);
                }
                else
                {
                    result = await UserApi.Login(login, password)
                        .ConfigureAwait(false);
                }

                if (result.IsError)
                {
                    if (result.Message ==
                        "[cluster_block_exception] blocked by: [FORBIDDEN/12/index read-only / allow delete (api)];")
                    {
                        LogUtils.LogWarning(result.Message,
                            $"id = {Id}, login = {Login}");

                        result = await UserApi.Login(login, password)
                            .ConfigureAwait(false);

                        if (result.IsError)
                        {
                            LogUtils.LogError(result.Message,
                                $"id = {Id}, login = {Login}");
                            Initialized = false;
                            return;
                        }
                    }
                    else
                    {
                        LogUtils.LogError(result.Message,
                            $"id = {Id}, login = {Login}");
                        Initialized = false;
                        return;
                    }
                }

                var resultData = await UserApi.GetProfileById(result.Data.Id)
                    .ConfigureAwait(false);

                if (resultData.IsError)
                {
                    if (resultData.Message ==
                        "[cluster_block_exception] blocked by: [FORBIDDEN/12/index read-only / allow delete (api)];")
                    {
                        LogUtils.LogWarning(resultData.Message,
                            $"id = {Id}, login = {Login}");
                    }
                    else
                    {
                        LogUtils.LogError(resultData.Message,
                            $"id = {Id}, login = {Login}");
                        Initialized = false;
                        return;
                    }
                }

                if (resultData.Data == null)
                {
                    Initialized = false;
                    return;
                }

                Data = resultData.Data;
                Token = result.Data.Token;
                Id = result.Data.Id;
                Login = login;
                Password = password;
                Nickname = resultData.Data.Nickname;
            }
            catch (Exception)
            {
                Initialized = false;
                return;
            }

            Initialized = true;
        }

        public async Task<bool> IsBanned()
        {
            return !await PostUtils.SendTestLike(this)
                .ConfigureAwait(false);
        }
    }
}
