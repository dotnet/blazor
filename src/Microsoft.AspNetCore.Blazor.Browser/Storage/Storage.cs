using Microsoft.AspNetCore.Blazor.Browser.Interop;
using System;
using System.Collections.Generic;
using System.Text;

namespace Microsoft.AspNetCore.Blazor.Browser.Storage
{
    public abstract class StorageBase
    {
        private string _fullTypeName;

        protected internal StorageBase()
        {
            _fullTypeName = GetType().FullName;
        }

        public void Clear()
        {
            RegisteredFunction.Invoke<object>($"{_fullTypeName}.Clear");
        }

        public string GetItem(string key)
        {
            return RegisteredFunction.Invoke<string>($"{_fullTypeName}.GetItem", key);
        }

        public string Key(int index)
        {
            return RegisteredFunction.Invoke<string>($"{_fullTypeName}.Key", index);
        }

        public int Length
        {
            get
            {
                return RegisteredFunction.Invoke<int>($"{_fullTypeName}.Length");
            }
        }

        public void RemoveItem(string key)
        {
            RegisteredFunction.Invoke<object>($"{_fullTypeName}.RemoveItem", key);
        }

        public void SetItem(string key, string data)
        {
            RegisteredFunction.Invoke<object>($"{_fullTypeName}.SetItem", key, data);
        }

        public string this[string key]
        {
            get
            {
                return RegisteredFunction.Invoke<string>($"{_fullTypeName}.GetItemString", key);
            }
            set
            {
                RegisteredFunction.Invoke<object>($"{_fullTypeName}.SetItemString", key, value);
            }
        }

        public string this[int index]
        {
            get
            {
                return RegisteredFunction.Invoke<string>($"{_fullTypeName}.GetItemNumber", index);
            }
            set
            {
                RegisteredFunction.Invoke<object>($"{_fullTypeName}.SetItemNumber", index, value);
            }
        }
    }

    public sealed class LocalStorage : StorageBase
    {

    }

    public sealed class SessionStorage : StorageBase
    {

    }
}
