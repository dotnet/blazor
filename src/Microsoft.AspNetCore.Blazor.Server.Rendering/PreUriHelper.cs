using System;
using Microsoft.AspNetCore.Blazor.Services;

namespace Microsoft.AspNetCore.Blazor.Server.Rendering
{
    public class PreUriHelper : IUriHelper
    {
        public string GetAbsoluteUri()
        {
            throw new NotImplementedException();
        }

        public event EventHandler<string> OnLocationChanged;
        public Uri ToAbsoluteUri(string href)
        {
            throw new NotImplementedException();
        }

        public string GetBaseUriPrefix()
        {
            return "http://localhost:56484/";
        }

        public string ToBaseRelativePath(string baseUriPrefix, string locationAbsolute)
        {
            throw new NotImplementedException();
        }

        protected virtual void OnOnLocationChanged(string e)
        {
            OnLocationChanged?.Invoke(this, e);
        }
    }
}