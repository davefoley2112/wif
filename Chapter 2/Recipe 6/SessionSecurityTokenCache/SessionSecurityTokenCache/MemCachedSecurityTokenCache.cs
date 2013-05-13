using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens;
using MemcachedProviders.Cache;

namespace SessionSecurityTokenCache
{
    public class MemCachedSecurityTokenCache : SecurityTokenCache
    {
        private object _syncRoot;
        public MemCachedSecurityTokenCache()
        {
            this._syncRoot = new object();
        }
        public override void ClearEntries()
        {
            lock (this._syncRoot)
            {
                DistCache.RemoveAll();
            }
        }

        public override bool TryAddEntry(object key, System.IdentityModel.Tokens.SecurityToken value)
        {
            bool flag;
            lock (this._syncRoot)
            {
                SecurityToken token;
                flag = this.TryGetEntry(key, out token);
                if (!flag)
                {
                    DistCache.Add(key.ToString(), value);
                }
            }
            return !flag;
        }

        public override bool TryGetAllEntries(object key, out IList<System.IdentityModel.Tokens.SecurityToken> tokens)
        {
            //TODO: No implementation necessary for the sample
            tokens = new List<SecurityToken>();
            return true;
        }

        public override bool TryGetEntry(object key, out System.IdentityModel.Tokens.SecurityToken value)
        {
            bool flag = false;
            lock (this._syncRoot)
            {
                value = DistCache.Get(key.ToString()) as SecurityToken;
                if (value != null)
                    flag = true;
            }
            return flag;
        }

        public override bool TryRemoveAllEntries(object key)
        {
            if (key != null)
            {
                lock (this._syncRoot)
                {
                    DistCache.RemoveAll();
                    return true;
                }
            }
            return false;
        }

        public override bool TryRemoveEntry(object key)
        {
            if (key != null)
            {
                lock (this._syncRoot)
                {
                    SecurityToken entry;
                    if (TryGetEntry(key, out entry))
                    {
                        DistCache.Remove(key.ToString());
                        return true;
                    }
                }
            }
            return false;
        }

        public override bool TryReplaceEntry(object key, System.IdentityModel.Tokens.SecurityToken newValue)
        {
            lock (this._syncRoot)
            {
                return (this.TryRemoveEntry(key) && this.TryAddEntry(key, newValue));
            }
        }
    }
}