using System;
using System.Collections.Generic;
using System.Text;

// ReSharper disable once CheckNamespace
namespace PropertyChanged
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = false, Inherited = false)]
    public class DoNotNotifyAttribute : Attribute
    {
    }
}