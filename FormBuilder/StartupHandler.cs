﻿using Composite.Core.Application;
using Composite.Data.DynamicTypes;

using CompositeC1Contrib.FormBuilder.Data.Types;

namespace CompositeC1Contrib.FormBuilder
{
    [ApplicationStartup]
    public sealed class StartupHandler
    {
        public static void OnBeforeInitialize() { }

        public static void OnInitialized()
        {
            DynamicTypeManager.EnsureCreateStore(typeof(IForm));
            DynamicTypeManager.EnsureCreateStore(typeof(IDefaultFormField));
        }
    }
}