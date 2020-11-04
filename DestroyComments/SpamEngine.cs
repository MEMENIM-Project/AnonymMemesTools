using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Threading.Tasks;
using DestroyComments.Utils;
using Memenim.Core.Schema;

namespace DestroyComments
{
    public class SpamEngine
    {
        private const string SpamCommentsFileName = "SpamComments.txt";
        private const string BotAccountsFileName = "BotAccounts.txt";

        private object BotAccountsFileSyncRoot { get; }
        private volatile StreamWriter _botAccountsFile;
        private StreamWriter BotAccountsFile
        {
            get
            {
                if (_botAccountsFile != null)
                    return _botAccountsFile;

                lock (BotAccountsFileSyncRoot)
                {
                    if (_botAccountsFile == null)
                    {
                        _botAccountsFile =
                            new StreamWriter(BotAccountsFileName, true);
                    }
                }

                return _botAccountsFile;
            }
        }

        public ReadOnlyCollection<string> SpamComments { get; private set; }
        public ReadOnlyCollection<SpamNode> SpamNodes { get; private set; }

        public bool Initialized { get; private set; }

        public SpamEngine()
        {
            BotAccountsFileSyncRoot = new object();
            _botAccountsFile = null;

            SpamComments = new ReadOnlyCollection<string>(new List<string>());
            SpamNodes = new ReadOnlyCollection<SpamNode>(new List<SpamNode>());
        }

        public async Task Initialize(int nodesCount, int nodeBotsCount,
            PostType nodePostsType = PostType.Popular,
            int nodePostsCount = 10, int nodePostsOffset = 0)
        {
            if (nodesCount == 0)
            {
                Initialized = false;
                return;
            }

            try
            {
                if (!File.Exists(SpamCommentsFileName))
                {
                    LogUtils.LogError("The file does not exist",
                        $"fileName = '{nameof(SpamCommentsFileName)}'");

                    File.Create(SpamCommentsFileName).Close();

                    LogUtils.LogInformation("An empty file was created. You must fill it out",
                        $"fileName = '{nameof(SpamCommentsFileName)}'");
                    Initialized = false;
                    return;
                }

                string[] spamCommentsList = await File.ReadAllLinesAsync(SpamCommentsFileName)
                    .ConfigureAwait(false);

                if (spamCommentsList.Length == 0)
                {
                    LogUtils.LogError("The file is empty. You must fill it out",
                        $"fileName = '{nameof(SpamCommentsFileName)}'");
                    Initialized = false;
                    return;
                }

                SpamComments = new ReadOnlyCollection<string>(spamCommentsList);

                List<SpamNode> spamNodesList = new List<SpamNode>(nodesCount);

                for (int i = 0; i < nodesCount; ++i)
                {
                    SpamNode node = new SpamNode();

                    await node.Initialize(this, nodeBotsCount, nodePostsType,
                            nodePostsCount, nodePostsOffset)
                        .ConfigureAwait(false);

                    if (!node.Initialized)
                    {
                        Initialized = false;
                        return;
                    }

                    spamNodesList.Add(node);
                }

                SpamNodes = new ReadOnlyCollection<SpamNode>(spamNodesList);
            }
            catch (Exception)
            {
                Initialized = false;
                return;
            }

            Initialized = true;
        }

        public void Start()
        {
            for (int i = 0; i < SpamNodes.Count; ++i)
            {
                if (!SpamNodes[i].Initialized)
                    continue;

                SpamNodes[i].Start();
            }
        }

        public void Cancel()
        {
            for (int i = 0; i < SpamNodes.Count; ++i)
            {
                if (!SpamNodes[i].Initialized)
                    continue;

                SpamNodes[i].Cancel();
            }
        }

        public string GetRandomCommentText()
        {
            if (SpamComments.Count == 0)
                return null;

            return SpamComments[RandomUtils.RandomInt(0, SpamComments.Count)];
        }

        public async Task SaveBotAccount(Bot bot)
        {
            if (bot == null)
                return;

            await BotAccountsFile.WriteLineAsync($"type = {bot.GetType().Name} " +
                                                 $"| login = {bot.Login} | password = {bot.Password} " +
                                                 $"| token = {bot.Token} | id = {bot.Id}")
                .ConfigureAwait(false);

            await BotAccountsFile.FlushAsync()
                .ConfigureAwait(false);
        }
    }
}
