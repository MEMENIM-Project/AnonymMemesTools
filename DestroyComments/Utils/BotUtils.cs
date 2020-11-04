using System;
using System.Threading.Tasks;
using Memenim.Core.Api;
using Memenim.Core.Schema;

namespace DestroyComments.Utils
{
    public static class BotUtils
    {
        public static async Task<Bot> CopyProfile(int id)
        {
            if (id < 0)
                return null;

            try
            {
                var result = await UserApi.GetProfileById(id)
                    .ConfigureAwait(false);

                if (result.error)
                {
                    LogUtils.LogError(result.message,
                        $"id = {id}");
                    return null;
                }

                if (result.data == null)
                    return null;

                string login = result.data.login;
                string nickname = result.data.name;
                Bot bot = new Bot();

                while (!bot.Initialized)
                {
                    login += "_";

                    await bot.Initialize(login, nickname)
                        .ConfigureAwait(false);
                }

                ProfileSchema botProfileSchema = result.data;
                botProfileSchema.login = bot.Login;
                botProfileSchema.id = bot.Id;

                var resultEdit = await UserApi.EditProfile(bot.Token, botProfileSchema)
                    .ConfigureAwait(false);

                if (resultEdit.error)
                {
                    if (resultEdit.message ==
                        "[cluster_block_exception] blocked by: [FORBIDDEN/12/index read-only / allow delete (api)];")
                    {
                        LogUtils.LogWarning(resultEdit.message,
                            $"id = {id}");
                    }
                    else
                    {
                        LogUtils.LogError(resultEdit.message,
                            $"id = {id}");
                        return null;
                    }
                }

                await bot.Initialize(bot.Login, bot.Password, null, false)
                    .ConfigureAwait(false);

                if (!bot.Initialized)
                    return null;

                return bot;
            }
            catch (Exception)
            {
                return null;
            }
        }
    }
}
