
using Ricebird.Security.ViewModels;

namespace Ricebird.Security.Services
{
    internal class CredentialService(TimeSpan frequency) : ICredentialService
    {
        private readonly Dictionary<string, AuthCredential> tokenStores = [];
        private readonly object locker = new object();
        private DateTime lastExpirationScan = DateTime.Now;

        private void ScanExpiredTokenIfNeed()
        {
            DateTime now = DateTime.Now;
            if (now - lastExpirationScan >= frequency)
            {
                lastExpirationScan = now;
                Task.Run(() =>
                {
                    lock (locker)
                    {
                        List<string> tokenNeedRemove = [];
                        foreach (var entry in tokenStores)
                        {
                            var token = entry.Value;
                            if (token.CheckExpired(now))
                            {
                                tokenNeedRemove.Add(entry.Key);
                            }
                        } // foreach

                        foreach (var item in tokenNeedRemove)
                        {
                            tokenStores.Remove(item);
                        }
                    }
                });
            } // if
        } // void ScanExpiredTokenIfNeed()

        #region 操作函数
        public int TotalToken => tokenStores.Count;
        public int TotalUser => tokenStores.Select(e => e.Value.UserPrincipal.ID).Distinct().Count();        
        public object GetJson()
        {
            return (new
            {
                success = true,
                msg = "",
                TotalToken,
                TotalUser,
                tokenStores,
            });
        }

        public (bool success, string msg, string credential) GetCredential(IUserPrincipal user, SecurityOption opt)
        {
            if (CountTokensByUser(user.ID) >= opt.MaxTokenForOneUser)
            {
                return (false, $"根据配置，一个用户最多只能同时拥有{opt.MaxTokenForOneUser}个客户端。", "");
            }

            if (user.ID == Guid.Empty)
            {
                throw new ArgumentException($"不可以加入空用户");
            }

            AuthCredential authCredential = new AuthCredential(user, opt.IdleTimeout);
            AddOrUpdateToken(authCredential);

            return (true, "", authCredential.Credential);
        }

        public IUserPrincipal? GetUser(string token)
        {
            IUserPrincipal? user = null;
            lock (locker)
            {
                DateTime now = DateTime.Now;
                if (tokenStores.TryGetValue(token, out var userCredential) && !userCredential.CheckExpired(now))
                {
                    userCredential.LastAccess = now;
                    user = userCredential.UserPrincipal;
                }
            }

            ScanExpiredTokenIfNeed();
            return user;
        }

        public void RemoveCredential(string token)
        {
            lock (locker)
            {
                if (tokenStores.TryGetValue(token, out var authCredential))
                {
                    tokenStores.Remove(token);
                }
            }

            ScanExpiredTokenIfNeed();
        }

        public void RemoveUser(string code)
        {
            List<string> tokens = [];
            foreach (var item in tokenStores)
            {
                if (item.Value.UserPrincipal.Code == code)
                {
                    tokens.Add(item.Key);
                }
            }

            lock (locker)
            {
                foreach (var item in tokens)
                {
                    tokenStores.Remove(item);
                }
            }

            ScanExpiredTokenIfNeed();
        }

        private void AddOrUpdateToken(AuthCredential token)
        {
            lock (locker)
            {
                tokenStores.MergeKey(token.Credential, token);
            }

            ScanExpiredTokenIfNeed();
        }

        public int CountTokensByUser(Guid userId) => tokenStores.Count(e => e.Value.UserPrincipal.ID == userId);        
        #endregion

        public void UpdateUser(CommonUser user)
        {
            lock (locker)
            {
                foreach (var item in tokenStores)
                {
                    if (item.Value.UserPrincipal.ID == user.ID && item.Value.UserPrincipal is UserPrincipal up)
                    {
                        up.Code = user.Code;
                        up.AuditStatus = user.AuditStatus;
                        up.Avatar = user.Avatar;
                        up.Email = user.Email;
                        up.Mobile = user.Mobile;
                        up.RealName = user.RealName;
                        up.RootDepartId = user.RootDepartId;
                    }
                }
            }
            ScanExpiredTokenIfNeed();
        }
    }
}
