using System;
using Microsoft.AspNetCore.Blazor.Services;

namespace Microsoft.AspNetCore.Blazor.Server.Rendering
{
    /// <summary>
    /// 
    /// </summary>
    public class PreUriHelper : IUriHelper
    {
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public string GetAbsoluteUri()
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// 
        /// </summary>
        public event EventHandler<string> OnLocationChanged;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="href"></param>
        /// <returns></returns>
        public Uri ToAbsoluteUri(string href)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public string GetBaseUriPrefix()
        {
            return "http://localhost:56484/";
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="baseUriPrefix"></param>
        /// <param name="locationAbsolute"></param>
        /// <returns></returns>
        public string ToBaseRelativePath(string baseUriPrefix, string locationAbsolute)
        {
            throw new NotImplementedException();
        }
    
        /// <summary>
        /// 
        /// </summary>
        /// <param name="e"></param>
        protected virtual void OnOnLocationChanged(string e)
        {
            OnLocationChanged?.Invoke(this, e);
        }
        
        /// <summary>
        /// 
        /// </summary>
        /// <param name="uri"></param>
        public void NavigateTo(string uri)
        {
            throw new NotImplementedException();
        }
    }
}